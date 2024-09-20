using MassTransit;
using MassTransit.Futures.Contracts;
using MassTransit.Internals;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Nito.AsyncEx;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LoanAccountAPI.Helpers;
using ScaleUP.Services.LoanAccountAPI.Helpers.NBFC;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountDTO;
using ScaleUP.Services.LoanAccountDTO.Loan;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using ScaleUP.Services.LoanAccountModels.Master;
using ScaleUP.Services.LoanAccountModels.Transaction;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using static MassTransit.ValidationResultExtensions;
using static ScaleUP.Services.LoanAccountAPI.Managers.LoanAccountManager;

namespace ScaleUP.Services.LoanAccountAPI.Managers
{
    public class TransactionSettlementManager
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        private readonly LoanAccountApplicationDbContext _context;
        private readonly OrderPlacementManager _orderPlacementManager;
        private readonly LoanNBFCFactory _loanNBFCFactory;
        private readonly DelayPenalityOnDuePerDayJobManager _delayPenalManager;
        private readonly PostDisbursementManager _postDisbursementManager;
        private readonly LoanAccountHelper _loanAccountHelper;

        private readonly LoanAccountHistoryManager _loanAccountHistoryManager;
        private readonly IMassTransitService _massTransitService;

        public TransactionSettlementManager(LoanAccountApplicationDbContext context, OrderPlacementManager orderPlacementManager, LoanNBFCFactory loanNBFCFactory, DelayPenalityOnDuePerDayJobManager delayPenalManager, PostDisbursementManager postDisbursementManager
           , LoanAccountHelper loanAccountHelper
           , LoanAccountHistoryManager loanAccountHistoryManager, IMassTransitService massTransitService
            )
        {
            _context = context;
            _orderPlacementManager = orderPlacementManager;
            _loanNBFCFactory = loanNBFCFactory;
            _delayPenalManager = delayPenalManager;
            _postDisbursementManager = postDisbursementManager;
            _loanAccountHelper = loanAccountHelper;

            _loanAccountHistoryManager = loanAccountHistoryManager;
            _massTransitService = massTransitService;

        }

        public async Task<TransactionSettlementDTO> GetManualTransactionSettleData(string TransactionId, double settleAmount, string username)
        {
            try
            {
                GRPCReply<TransactionSettlementDTO> reply = new GRPCReply<TransactionSettlementDTO>();
                var Transaction_Id = new SqlParameter("TransactionId", TransactionId);
                var Settle_Amt = new SqlParameter("SettleAmt", settleAmount);
                var user_name = new SqlParameter("username", "");
                var result = await _context.Database.SqlQueryRaw<TransactionSettlementDTO>("exec SpManualTransactionSettle @TransactionId, @SettleAmt, @username", Transaction_Id, Settle_Amt, user_name).FirstOrDefaultAsync();
                //reply.Response = result;
                //reply.Status = true;

                return result;
            }
            catch (Exception ex)
            {
                return null;
                //throw;
            }
        }


        public async Task<List<TransactionSettlementDTO>> GetManualTransactionSettleDataList(string TransactionId, double settleAmount, string username)
        {
            try
            {
                GRPCReply<List<TransactionSettlementDTO>> reply = new GRPCReply<List<TransactionSettlementDTO>>();
                var Transaction_Id = new SqlParameter("TransactionId", TransactionId);
                var Settle_Amt = new SqlParameter("SettleAmt", settleAmount);
                var user_name = new SqlParameter("username", "");
                var result = await _context.Database.SqlQueryRaw<TransactionSettlementDTO>("exec SpManualTransactionSettle @TransactionId, @SettleAmt, @username", Transaction_Id, Settle_Amt, user_name).ToListAsync();
                //reply.Response = result;
                //reply.Status = true;

                return result;
            }
            catch (Exception ex)
            {
                return null;
                //throw;
            }
        }


        public async Task<GRPCReply<long>> ManuallyTransactionSettle(GRPCRequest<TransactionSettlementRequestDC> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            AccountTransactionManager objAccounttransManager = new AccountTransactionManager(_context, _loanNBFCFactory);

            long LeadId = request.Request.LeadId;
            string TransactionId = request.Request.TransactionReqNo;
            double settleAmount = request.Request.Amount;
            string username = request.Request.UserName;

            var results = await GetManualTransactionSettleDataList(TransactionId, settleAmount, request.Request.UserName);
            if (results != null)
            {
                double TrnAmount = 0;
                double PaidAmount = 0;

                foreach (var item in results)
                {
                    TrnAmount = item.TrnAmount ?? 0;
                    PaidAmount = item.PaidAmount ?? 0;
                }

                if (TrnAmount != PaidAmount)
                {
                    var loanAccount = await _context.LoanAccounts.Where(x => x.LeadId == LeadId).Include(x => x.LoanAccountCredits).FirstOrDefaultAsync();
                    if (loanAccount != null)
                    {
                        long LoanAccountId = loanAccount.Id;

                        var TransactionStatusesId = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();
                        long TransactionStatusId = TransactionStatusesId.Id;

                        DateTime DueDate;
                        string sCustomerUniqueCode = "333"; ////////// ??????????????
                        string sReferenceId = request.Request.TransactionReqNo;        ////////// ??????????????
                        DateTime SettlementDate = request.Request.PaymentDate;

                        //DueDate = DateTime.Now.AddDays(Convert.ToInt64(loanAccount.LoanAccountCredits.CreditDays));
                        long CompanyProductId = loanAccount.NBFCCompanyId;
                        long AnchorCompanyId = loanAccount.AnchorCompanyId ?? 0;
                        double TransactionAmount = settleAmount;
                        double DiscountAmount = 0;

                        //double ConvenionFeeRate = loanAccount.LoanAccountCredits.ConvenienceFeeRate;
                        //double ProcessingFeeRate = loanAccount.LoanAccountCredits.ProcessingFeeRate;
                        //double GST = loanAccount.LoanAccountCredits.GstRate;
                        double ConvenionFeeRate = 0;
                        double ProcessingFeeRate = 0;
                        double GST = 0;

                        string GSTper = "GST" + Convert.ToString(GST);
                        var GST_ConvenionFee = 0;
                        double TotalAmount = TransactionAmount + GST_ConvenionFee;

                        long ParentAccountTransactionsID = 0;
                        var TransactionTypeId = _context.TransactionTypes.Where(x => x.Code == request.Request.TransactionTypeCode).FirstOrDefault();
                        var TransactionDetailHeadId = _context.TransactionDetailHeads.Where(x => x.Code == TransactionDetailHeadsConstants.Payment).FirstOrDefault();

                        var accountTransactionExist = await _context.AccountTransactions.Where(x => x.ReferenceId == sReferenceId).ToListAsync();
                        if ((accountTransactionExist) != null && accountTransactionExist.Any())
                        {
                            var AccountTransactionsFirstRecord = accountTransactionExist.Where(x => !x.ParentAccountTransactionsID.HasValue).FirstOrDefault();
                            DueDate = DateTime.Now.AddDays(Convert.ToInt64(AccountTransactionsFirstRecord.CreditDays));
                            //ConvenionFeeRate = AccountTransactionsFirstRecord.ConvenienceFeeRate;
                            ConvenionFeeRate = AccountTransactionsFirstRecord.InterestRate;
                            ProcessingFeeRate = AccountTransactionsFirstRecord.ProcessingFeeRate;
                            GST = AccountTransactionsFirstRecord.GstRate;


                            ParentAccountTransactionsID = accountTransactionExist.Where(x => x.ParentAccountTransactionsID.HasValue).Select(x => x.Id).Distinct().FirstOrDefault();

                            AccountTransaction accountTransaction = new AccountTransaction
                            {
                                AnchorCompanyId = AnchorCompanyId,
                                LoanAccountId = LoanAccountId,
                                ReferenceId = sReferenceId,
                                CustomerUniqueCode = sCustomerUniqueCode,
                                TransactionTypeId = TransactionTypeId.Id,
                                IsActive = true,
                                IsDeleted = false,
                                CompanyProductId = CompanyProductId,
                                SettlementDate = SettlementDate,
                                TransactionStatusId = (TrnAmount == (PaidAmount + TransactionAmount)) ? TransactionStatusId : accountTransactionExist.Select(x => x.TransactionStatusId).Distinct().FirstOrDefault(),
                                ParentAccountTransactionsID = ParentAccountTransactionsID,
                                DueDate = DueDate,
                                Created = DateTime.Now,
                                OrderAmount = TransactionAmount,
                                GSTAmount = GST_ConvenionFee,
                                PaidAmount = TotalAmount,
                            };
                            _context.AccountTransactions.Add(accountTransaction);
                            _context.SaveChanges();

                            long AccountTransactionId;
                            if (accountTransaction.Id > 0)
                            {
                                AccountTransactionId = accountTransaction.Id;

                                List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();

                                AccountTransactionDetail accountTransactionDetail = new AccountTransactionDetail
                                {
                                    AccountTransactionId = AccountTransactionId,
                                    Amount = TransactionAmount,
                                    TransactionDetailHeadId = TransactionDetailHeadId.Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Created = DateTime.Now
                                };
                                transactionDetails.Add(accountTransactionDetail);

                                //AccountTransactionDetail accountTransactionDetailGST = new AccountTransactionDetail
                                //{
                                //    AccountTransactionId = AccountTransactionId,
                                //    Amount = GST_ConvenionFee,
                                //    TransactionDetailHeadId = 2,
                                //    IsActive = true,
                                //    IsDeleted = false,
                                //    Created = DateTime.Now
                                //};
                                //transactionDetails.Add(accountTransactionDetailGST);
                                _context.AccountTransactionDetails.AddRange(transactionDetails);
                                //_context.SaveChanges();


                                var loanAccountcredit = _context.LoanAccountCredits.Where(x => x.LoanAccountId == LoanAccountId).FirstOrDefault();
                                loanAccountcredit.CreditLimitAmount = loanAccountcredit.CreditLimitAmount + TotalAmount;
                                _context.Entry(loanAccountcredit).State = EntityState.Modified;

                                //Change TransactionStatusescode
                                if (TrnAmount == (PaidAmount + TransactionAmount))
                                {
                                    foreach (var ACT in accountTransactionExist)
                                    {
                                        ACT.TransactionStatusId = TransactionStatusId;
                                        _context.Entry(ACT).State = EntityState.Modified;
                                    }
                                }

                                _context.SaveChanges();

                                gRPCReply.Status = true;
                                gRPCReply.Message = "Success";
                                gRPCReply.Response = AccountTransactionId;
                            }
                        }


                    }
                }


            }

            return gRPCReply;
        }



        public async Task<WaiveOffPenaltyBounceReplytDTO> WaiveOffPenaltyBounce(WaiveOffPenaltyBounceRequestDTO waiveOffPenaltyBounceRequestDTO)
        {

            WaiveOffPenaltyBounceReplytDTO waiveOffPenaltyBounceReplytDTO = new WaiveOffPenaltyBounceReplytDTO();
            var Transaction_Id = new SqlParameter("TransactionNo", waiveOffPenaltyBounceRequestDTO.TransactionId);
            var Penalty_Type = new SqlParameter("DiscountSourceType", waiveOffPenaltyBounceRequestDTO.PenaltyType);
            var Discount_Amount = new SqlParameter("DiscountAmount", waiveOffPenaltyBounceRequestDTO.DiscountAmount);
            var Discount_Gst = new SqlParameter("RemainingGSTAmount", waiveOffPenaltyBounceRequestDTO.DiscountGst);
            var result = _context.Database.SqlQueryRaw<string>("exec WaiveOffBouncePenaltyInsert  @TransactionNo,@DiscountSourceType,@DiscountAmount, @RemainingGSTAmount", Transaction_Id, Penalty_Type, Discount_Amount, Discount_Gst).AsEnumerable().FirstOrDefault();

            //var entity_name = new SqlParameter("entityname", entityname.Request);
            //var result = _context.Database.SqlQueryRaw<string>("exec spGetCurrentNumber @entityname", entity_name).AsEnumerable().FirstOrDefault();

            if (result != null)
            {
                if (result == "Success")
                {
                    waiveOffPenaltyBounceReplytDTO.Status = true;
                    waiveOffPenaltyBounceReplytDTO.Message = result;
                }
                else
                {
                    waiveOffPenaltyBounceReplytDTO.Status = false;
                    waiveOffPenaltyBounceReplytDTO.Message = result;
                }
            }

            return waiveOffPenaltyBounceReplytDTO;
        }

        public async Task<TransactionSettlementByManualReplytDTO> TransactionSettleByManual(TransactionSettlementByManualDTO TransactionSettleByManualDc)
        {
            try
            {

                TransactionSettlementByManualReplytDTO TransactionSettlementByManualReplytDTO = new TransactionSettlementByManualReplytDTO();
                var Transaction_Id = new SqlParameter("TransactionNo", TransactionSettleByManualDc.TransactionNo);
                var ModeOfPayment_SourceType = new SqlParameter("ModeOfPaymentSourceType", TransactionSettleByManualDc.ModeOfPaymentSourceType);
                var Settle_Amount = new SqlParameter("SettleAmount", TransactionSettleByManualDc.SettleAmount);
                var user_name = new SqlParameter("username", TransactionSettleByManualDc.username);
                var Payment_RefNo = new SqlParameter("PaymentRefNo", TransactionSettleByManualDc.PaymentRefNo);
                var Payment_Date = new SqlParameter("PaymentDate", TransactionSettleByManualDc.PaymentDate);
                var result = _context.Database.SqlQueryRaw<string>("exec TransactionSettleByManual  @TransactionNo,@ModeOfPaymentSourceType,@SettleAmount, @username,@PaymentRefNo,@PaymentDate", Transaction_Id, ModeOfPayment_SourceType, Settle_Amount, user_name, Payment_RefNo, Payment_Date).AsEnumerable().FirstOrDefault();
                if (result != null)
                {
                    if (result == "Save Successfully")
                    {
                        TransactionSettlementByManualReplytDTO.Status = true;
                        TransactionSettlementByManualReplytDTO.Message = result;
                    }
                    else
                    {
                        TransactionSettlementByManualReplytDTO.Status = false;
                        TransactionSettlementByManualReplytDTO.Message = result;
                    }
                }

                return TransactionSettlementByManualReplytDTO;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CommonResponse> BounceChargeInsert(string TransactionNumber, string CreatedBy)
        {
            CommonResponse commonResponse = new CommonResponse();
            try
            {
                var Transaction_Id = new SqlParameter("TransactionNo", TransactionNumber);
                var Created_By = new SqlParameter("CreatedBy", CreatedBy);
                var result = _context.Database.SqlQueryRaw<string>("exec BounceChargesInsert  @TransactionNo,@CreatedBy", Transaction_Id, Created_By).AsEnumerable().FirstOrDefault();

                if (result != null)
                {
                    if (result == "Record Save Successfully!")
                    {
                        commonResponse.status = true;
                        commonResponse.Message = result;
                    }
                    else
                    {
                        commonResponse.status = false;
                        commonResponse.Message = result;
                    }
                }

                return commonResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<TransactionDetailHistoryDc>> TransactionDetailHistory(string TransactionId)
        {
            List<TransactionDetailHistoryDc> transactionDetailHistoryDc = new List<TransactionDetailHistoryDc>();
            try
            {
                var Transaction_Id = new SqlParameter("TransactionId", TransactionId);
                transactionDetailHistoryDc = await _context.Database.SqlQueryRaw<TransactionDetailHistoryDc>("exec GetTransactionHistoryDetail  @TransactionId", Transaction_Id).ToListAsync();


                if (transactionDetailHistoryDc != null)
                {
                    return transactionDetailHistoryDc;
                }

                return transactionDetailHistoryDc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<GRPCReply<long>> InsertDailyPenalty()
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();

            var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();
            var TransactionTypeID_PenaltyCharges = _context.TransactionTypes.Where(x => x.Code == TransactionTypesConstants.PenaltyCharges).FirstOrDefault();
            var TransactionStatusID_Ovedue = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Overdue).FirstOrDefault();

            long LoanAccounId = 0;
            //var LoanAccount_Id = new SqlParameter("LoanAccountId", LoanAccounId);
            var PenaltyChargeTransactionList = await GetOutstandingTransactionsList("", 0, null);

            double OrderAmount = 0, PaidAmount = 0, DelayPenalityAmount = 0, DelayPenalityGSTAmount = 0;
            bool PayableBy = false;

            foreach (var item in PenaltyChargeTransactionList)
            {
                var AccountTransactionsExist = _context.AccountTransactions.FirstOrDefault(x => x.ReferenceId == item.transactionReqNo && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement && x.IsActive && !x.IsDeleted);
                if (AccountTransactionsExist.TransactionStatusId == TransactionStatusID_Ovedue.Id)
                {
                    LoanAccounId = AccountTransactionsExist.LoanAccountId;
                    if (AccountTransactionsExist.PayableBy == "Customer")
                    { PayableBy = true; }

                    double GST = item.GstRate ?? 0;
                    string GSTper = "GST" + Convert.ToString(GST);

                    DelayPenalityAmount = item.DelayPenalityAmount;
                    DelayPenalityGSTAmount = 0; //item.DelayPenalityGstAmount; 
                    OrderAmount = DelayPenalityAmount;
                    if (PayableBy == true)
                    {
                        PaidAmount = OrderAmount + DelayPenalityGSTAmount;
                    }
                    else
                    {
                        PaidAmount = OrderAmount;
                    }

                    AccountTransaction accountTransaction = null;
                    if (item.PaneltyTxnId == 0)
                    {
                        accountTransaction = new AccountTransaction
                        {
                            AnchorCompanyId = AccountTransactionsExist.AnchorCompanyId,
                            LoanAccountId = LoanAccounId,
                            ReferenceId = AccountTransactionsExist.ReferenceId,
                            CustomerUniqueCode = AccountTransactionsExist.CustomerUniqueCode,
                            TransactionTypeId = TransactionTypeID_PenaltyCharges.Id,
                            IsActive = true,
                            IsDeleted = false,
                            CompanyProductId = AccountTransactionsExist.CompanyProductId,
                            TransactionStatusId = TransactionStatusID_Ovedue.Id,
                            OrderAmount = OrderAmount,
                            PaidAmount = PaidAmount,
                            ProcessingFeeType = "",
                            ProcessingFeeRate = 0,
                            GstRate = AccountTransactionsExist.GstRate,
                            InterestType = AccountTransactionsExist.InterestType,
                            InterestRate = AccountTransactionsExist.InterestRate,
                            CreditDays = AccountTransactionsExist.CreditDays,
                            BounceCharge = AccountTransactionsExist.BounceCharge,
                            DelayPenaltyRate = AccountTransactionsExist.DelayPenaltyRate,
                            PayableBy = AccountTransactionsExist.PayableBy,
                            ParentAccountTransactionsID = AccountTransactionsExist.Id
                            //OrderDate = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        accountTransaction = _context.AccountTransactions.FirstOrDefault(x => x.ReferenceId == item.transactionReqNo && x.TransactionType.Code == TransactionTypesConstants.PenaltyCharges && x.IsActive && !x.IsDeleted);
                        accountTransaction.OrderAmount = accountTransaction.OrderAmount + OrderAmount;
                        accountTransaction.PaidAmount = accountTransaction.PaidAmount + PaidAmount;
                    }

                    List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();
                    transactionDetails.Add(new AccountTransactionDetail()
                    {
                        IsActive = true,
                        IsDeleted = false,
                        Amount = DelayPenalityAmount,
                        TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.DelayPenalty).Id,
                        IsPayableBy = true,
                        TransactionDate = DateTime.Now,
                    });

                    if (DelayPenalityGSTAmount > 0)
                    {
                        transactionDetails.Add(new AccountTransactionDetail()
                        {
                            IsActive = true,
                            IsDeleted = false,
                            Amount = DelayPenalityGSTAmount,
                            TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == GSTper).Id,
                            IsPayableBy = PayableBy,
                            TransactionDate = DateTime.Now,
                        });
                    }

                    if (item.PaneltyTxnId == 0)
                    {
                        accountTransaction.AccountTransactionDetails = transactionDetails;
                        _context.AccountTransactions.Add(accountTransaction);
                    }
                    else
                    {
                        //if(accountTransaction.AccountTransactionDetails != null)
                        //{
                        //    accountTransaction.AccountTransactionDetails = new List<AccountTransactionDetail>();
                        //}
                        accountTransaction.AccountTransactionDetails = transactionDetails;
                        accountTransaction.AccountTransactionDetails.ToList().AddRange(transactionDetails);
                    }

                    //// Update Credit Limit
                    //var loanAccountcredit = _context.LoanAccountCredits.Where(x => x.LoanAccountId == LoanAccounId).FirstOrDefault();
                    //loanAccountcredit.CreditLimitAmount = loanAccountcredit.CreditLimitAmount - PaidAmount;
                    //_context.Entry(loanAccountcredit).State = EntityState.Modified;

                    if (_context.SaveChanges() > 0)
                    {
                        gRPCReply.Status = true;
                        gRPCReply.Message = "Success";
                        gRPCReply.Response = 0;// AccountTransactionId;
                    }
                    else
                    {
                        gRPCReply.Status = false;
                        gRPCReply.Message = "Fail";
                        gRPCReply.Response = 0;
                    }
                }
            }

            return gRPCReply;
        }


        public async Task<CommonResponse> DiscountOverdueInterestAmtDelayPenaltyAmt(string InvoiceNo, double DiscountAmount, string DiscountTransactionType)
        {
            CommonResponse commonResponse = new CommonResponse();
            commonResponse.status = false;
            commonResponse.Message = "";

            //TransactionTypesConstants.PenaltyCharges
            //TransactionTypesConstants.OverdueInterest
            var AccountTransactionsExist = _context.AccountTransactions.FirstOrDefault(x => x.InvoiceNo == InvoiceNo && x.TransactionType.Code == DiscountTransactionType && x.IsActive && !x.IsDeleted);
            if (AccountTransactionsExist != null)
            {
                long accountTransactionId = AccountTransactionsExist.Id;
                long LoanAccountId = AccountTransactionsExist.LoanAccountId;

                var OutstandingAmount = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == accountTransactionId && x.IsActive && !x.IsDeleted).Sum(y => y.Amount);
                if (OutstandingAmount >= DiscountAmount)
                {
                    var TransactionDetailHeadId = _context.TransactionDetailHeads.Where(x => x.Code == TransactionDetailHeadsConstants.Discount).FirstOrDefault();

                    List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();
                    AccountTransactionDetail accountTransactionDetail = new AccountTransactionDetail
                    {
                        AccountTransactionId = accountTransactionId,
                        Amount = DiscountAmount,
                        TransactionDetailHeadId = TransactionDetailHeadId.Id,
                        IsPayableBy = AccountTransactionsExist.PayableBy == "Customer",
                        IsActive = true,
                        IsDeleted = false,
                        Created = DateTime.Now
                    };
                    transactionDetails.Add(accountTransactionDetail);

                    _context.AccountTransactionDetails.AddRange(transactionDetails);


                    var loanAccountcredit = _context.LoanAccountCredits.Where(x => x.LoanAccountId == LoanAccountId).FirstOrDefault();
                    loanAccountcredit.CreditLimitAmount = loanAccountcredit.CreditLimitAmount + DiscountAmount;
                    _context.Entry(loanAccountcredit).State = EntityState.Modified;

                    ////Change TransactionStatusescode
                    //if (TrnAmount == (PaidAmount + TransactionAmount))
                    //{
                    //    foreach (var ACT in accountTransactionExist)
                    //    {
                    //        ACT.TransactionStatusId = TransactionStatusId;
                    //        _context.Entry(ACT).State = EntityState.Modified;
                    //    }
                    //}

                    _context.SaveChanges();

                    commonResponse.status = true;
                    commonResponse.Message = "Record Save Sucessfully!";
                }

            }
            return commonResponse;
        }
        #region blacksoil methods

        public async Task<ResultViewModel<string>> BlackSoilPaymentCredit(BlackSoilRepaymentDc repayment)
        {

            long AccountTransactionId = 0;
            long LoanAccounId = 0;

            ResultViewModel<string> result = new ResultViewModel<string>();
            LoanAccountRepayment loanAccountRepayment = null;

            //step 1: Make entry in LoanAccountRepayment
            var loanAccountRepaymentExist = await _context.LoanAccountRepayments.Where(x => x.ThirdPartyPaymentId == Convert.ToString(repayment.id) && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (loanAccountRepaymentExist != null)
            {
                //repayment.settlement_date = new DateTime(DateTime.Now.AddMonths(0).Year, DateTime.Now.AddMonths(0).Month, 23);

                result = new ResultViewModel<string>
                {
                    IsSuccess = false,
                    Message = "Third Party PaymentId exist",
                    Result = ""
                };
            }
            else
            {

                var blackSoilAccountDetailsExist = await _context.BlackSoilAccountDetails.Where(x => x.BlackSoilLoanId == repayment.loan_account && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                if (blackSoilAccountDetailsExist == null)
                {
                    result = new ResultViewModel<string>
                    {
                        IsSuccess = false,
                        Message = "BlackSoil LoanId not exist",
                        Result = ""
                    };
                }
                else
                {
                    LoanAccounId = blackSoilAccountDetailsExist.LoanAccountId;

                    loanAccountRepayment = new LoanAccountRepayment
                    {
                        ThirdPartyPaymentId = Convert.ToString(repayment.id),
                        LoanAccountId = LoanAccounId,
                        ThirdPartyLoanAccountId = Convert.ToString(repayment.loan_account),
                        PaymentMode = repayment.payment_mode,
                        BankRefNo = repayment.ref_no,
                        ThirdPartyTxnId = repayment.txn_id,
                        Status = BlackSoilRepaymentConstants.Pending,
                        Amount = Convert.ToDouble(repayment.amount),
                        PaymentDate = repayment.settlement_date,  //DateTime.Now,
                        TotalAmount = Convert.ToDouble(repayment.actual_amount),
                        InterestAmount = Convert.ToDouble(repayment.interest),
                        ProcessingFees = Convert.ToDouble(repayment.pf),
                        PenalInterest = Convert.ToDouble(repayment.penal_interest),
                        OverdueInterest = Convert.ToDouble(repayment.overdue_interest),
                        PrincipalAmount = Convert.ToDouble(repayment.principal),
                        ExtraPaymentAmount = Convert.ToDouble(repayment.extra_payment),
                        IsActive = true,
                        IsDeleted = false,
                        IsRunning = false
                    };
                    _context.LoanAccountRepayments.Add(loanAccountRepayment);
                    var rowChanged = _context.SaveChanges();
                    if (rowChanged > 0)
                    {
                        //var PendingTransactionsList = await GetOutstandingTransactionsList("", LoanAccounId, repayment.settlement_date);
                        //if (PendingTransactionsList != null && PendingTransactionsList.Any())
                        //{
                        //    var data = await SettlePaymentTransaction(PendingTransactionsList, repayment, allInvoiceData);

                        //}
                    }

                }
            }


            return result;
        }



        public async Task<List<GetOutstandingTransactionsDTO>> GetOutstandingTransactionsList(string TransactionNo, long LoanAccountId, DateTime? invoiceDate)
        {
            List<GetOutstandingTransactionsDTO> result = new List<GetOutstandingTransactionsDTO>();

            var Transaction_No = new SqlParameter("TransactionNo", TransactionNo);
            var LoanAccount_Id = new SqlParameter("LoanAccountId", LoanAccountId);
            var Invoice_Date = new SqlParameter("InvoiceDate", invoiceDate == null ? DBNull.Value : (object)invoiceDate);

            result = await _context.Database.SqlQueryRaw<GetOutstandingTransactionsDTO>("exec GetOutstandingTransactionsList @TransactionNo, @LoanAccountId, @InvoiceDate", Transaction_No, LoanAccount_Id, Invoice_Date).ToListAsync();

            return result;
        }

        public async Task<string> SettlePaymentTransaction(List<GetOutstandingTransactionsDTO> pendingTransactionList, BlackSoilRepaymentDc blackSoilRepayment, BlackSoilLoanAccountExpandDc accountDetails, LoanAccountRepayment payment, GRPCRequest<SettlePaymentJobRequest> productConfigs)
        {

            using var transaction1 = _context.Database.BeginTransaction();

            try
            {
                List<TransactionSettlementDetailDTO> transactionList = new List<TransactionSettlementDetailDTO>();
                double Repaymentprincipalamount = double.Parse(blackSoilRepayment.principal);
                double RepaymentPenalAmount = double.Parse(blackSoilRepayment.penal_interest);
                double RepaymentExtraAmount = double.Parse(blackSoilRepayment.extra_payment);
                double RepaymentOverDueAmount = double.Parse(blackSoilRepayment.overdue_interest);
                double RepaymentInterestAmount = double.Parse(blackSoilRepayment.interest);
                if (payment.Status == BlackSoilRepaymentConstants.PartialSettled)
                {
                    Repaymentprincipalamount = (payment.RemainingPrincipalAmount.Value) > .01 ? payment.RemainingPrincipalAmount.Value : 0;
                    RepaymentPenalAmount = (payment.RemainingPenalInterest.Value) > +.01 ? payment.RemainingPenalInterest.Value : 0;
                    RepaymentExtraAmount = (payment.RemainingExtraPaymentAmount.Value) > +.01 ? payment.RemainingExtraPaymentAmount.Value : 0;
                    RepaymentOverDueAmount = (payment.RemainingOverdueInterest.Value) > +.01 ? payment.RemainingOverdueInterest.Value : 0;
                    RepaymentInterestAmount = (payment.RemainingInterestAmount.Value) > +.01 ? payment.RemainingInterestAmount.Value : 0;

                }


                if (accountDetails != null && accountDetails.topups != null && accountDetails.topups.Any())
                {
                    accountDetails.topups = accountDetails.topups.OrderBy(x => x.disbursement_date).ToList();

                    foreach (var topup in accountDetails.topups)
                    {
                        var transactions = pendingTransactionList.Where(x => x.WithdrawlId == topup.invoice.id).ToList();


                        if (transactions != null)
                        {
                            var totalTxnPrinciplePaidAmount = transactions.Sum(x => x.PaymentAmount);
                            var totalTxnInterestPaidAmount = transactions.Sum(x => x.InterestPaymentAmount);
                            var totalTxnPenalPaidAmount = transactions.Sum(x => x.PenalPaymentAmount);
                            var totalTxnOverduePaidAmount = transactions.Sum(x => x.OverduePaymentAmount);



                            double principlePaid;
                            bool isDoubleConverted = double.TryParse(topup.extra.principal_paid, out principlePaid);

                            double OverDueAmount;
                            bool isOverDueAmount = double.TryParse(topup.extra.overdue_interest_paid, out OverDueAmount);

                            double PenalAmount;
                            bool isPenal = double.TryParse(topup.extra.penal_interest_paid, out PenalAmount);

                            double InterestAmount;
                            bool isInterestAmount = double.TryParse(topup.extra.interest_paid, out InterestAmount);

                            double ExtraAmount;
                            bool isExtraAmount = double.TryParse(topup.extra.extra_payment, out ExtraAmount);


                            foreach (var transaction in transactions)
                            {


                                TransactionSettlementDetailDTO txn = new TransactionSettlementDetailDTO();
                                txn.ModeOfPaymentSourceType = blackSoilRepayment.payment_mode;
                                txn.PaymentDate = blackSoilRepayment.settlement_date.Value;
                                txn.PaymentRefNo = blackSoilRepayment.ref_no;
                                txn.TransactionNo = transaction.transactionReqNo.ToString();
                                txn.username = "";
                                txn.ParentAccountTransactionId = transaction.Id; //blackSoilRepayment.id;
                                txn.OrderPrincipalAmount = transaction.PricipleAmount ?? 0;
                                txn.OrderPrincipleAlreadyPaidAmount = transaction.PaymentAmount ?? 0;

                                txn.InterestAmountOfOrder = transaction.InterestAmount ?? 0;
                                txn.InterestPaymentAlreadyPaidAmount = transaction.InterestPaymentAmount ?? 0;
                                txn.OverdueInterestAmountOfOrder = transaction.OverdueInterestAmount ?? 0;
                                txn.OverduePaymentAlreadyPaidAmount = transaction.OverduePaymentAmount ?? 0;
                                txn.DelayPenalityAmountOfOrder = transaction.PenaltyChargesAmount; //DelayPenalityAmount
                                txn.PenalPaymentAlreadyPaidAmount = transaction.PenalPaymentAmount ?? 0;


                                var txnRemainingAmount = transaction.PricipleAmount - transaction.PaymentAmount;
                                var MaxAmountToBeSettle = (principlePaid - totalTxnPrinciplePaidAmount) > txnRemainingAmount ? txnRemainingAmount : principlePaid - totalTxnPrinciplePaidAmount;
                                MaxAmountToBeSettle = Repaymentprincipalamount > MaxAmountToBeSettle ? MaxAmountToBeSettle : Repaymentprincipalamount;
                                //if (isDoubleConverted &&  transaction.PaymentAmount < principlePaid && Repaymentprincipalamount >= (principlePaid - transaction.PaymentAmount))
                                if (isDoubleConverted && totalTxnPrinciplePaidAmount < principlePaid && MaxAmountToBeSettle > 0)
                                {
                                    txn.PrincipalAmount = MaxAmountToBeSettle.Value;
                                    Repaymentprincipalamount = Repaymentprincipalamount - MaxAmountToBeSettle.Value;
                                    totalTxnPrinciplePaidAmount += MaxAmountToBeSettle.Value;
                                }



                                var txnRemainingInterestAmount = transaction.InterestAmount - transaction.InterestPaymentAmount;
                                var MaxInterestAmountToBeSettle = (InterestAmount - totalTxnInterestPaidAmount) > txnRemainingInterestAmount ? txnRemainingInterestAmount : InterestAmount - totalTxnInterestPaidAmount;
                                MaxInterestAmountToBeSettle = RepaymentInterestAmount > MaxInterestAmountToBeSettle ? MaxInterestAmountToBeSettle : RepaymentInterestAmount;
                                if (isInterestAmount && totalTxnInterestPaidAmount < InterestAmount && MaxInterestAmountToBeSettle > 0)
                                {
                                    txn.InterestAmount = MaxInterestAmountToBeSettle.Value;
                                    RepaymentInterestAmount = RepaymentInterestAmount - MaxInterestAmountToBeSettle.Value;
                                    totalTxnInterestPaidAmount += MaxInterestAmountToBeSettle.Value;
                                }


                                var txnRemainingPenalAmount = transaction.PenaltyChargesAmount - transaction.PenalPaymentAmount;
                                var MaxPenaltyChargesAmountToBeSettle = (PenalAmount - totalTxnPenalPaidAmount) > txnRemainingPenalAmount ? txnRemainingPenalAmount : PenalAmount - totalTxnPenalPaidAmount;
                                MaxPenaltyChargesAmountToBeSettle = RepaymentPenalAmount > MaxPenaltyChargesAmountToBeSettle ? MaxPenaltyChargesAmountToBeSettle : RepaymentPenalAmount;
                                if (isPenal && totalTxnPenalPaidAmount < PenalAmount && MaxPenaltyChargesAmountToBeSettle > 0)
                                {
                                    //if (isOverDueAmount && transaction.PenalPaymentAmount < OverDueAmount && RepaymentOverDueAmount >= (OverDueAmount - transaction.PenalPaymentAmount))
                                    //txn.PenalAmount = OverDueAmount - transaction.PenalPaymentAmount ?? 0;
                                    //RepaymentOverDueAmount = RepaymentOverDueAmount - (OverDueAmount - transaction.PenalPaymentAmount ?? 0);
                                    txn.PenalAmount = MaxPenaltyChargesAmountToBeSettle.Value;
                                    RepaymentPenalAmount = RepaymentPenalAmount - MaxPenaltyChargesAmountToBeSettle.Value;
                                    totalTxnPenalPaidAmount += MaxPenaltyChargesAmountToBeSettle.Value;
                                }

                                var txnRemainingOverdueAmount = transaction.OverdueInterestAmount - transaction.OverduePaymentAmount;
                                var MaxOverdueChargesAmountToBeSettle = (OverDueAmount - totalTxnOverduePaidAmount) > txnRemainingOverdueAmount ? txnRemainingOverdueAmount : OverDueAmount - totalTxnOverduePaidAmount;
                                MaxOverdueChargesAmountToBeSettle = RepaymentOverDueAmount > MaxOverdueChargesAmountToBeSettle ? MaxOverdueChargesAmountToBeSettle : RepaymentOverDueAmount;
                                if (isOverDueAmount && totalTxnOverduePaidAmount < OverDueAmount && MaxOverdueChargesAmountToBeSettle > 0)
                                {
                                    txn.OverdueAmount = MaxOverdueChargesAmountToBeSettle.Value;
                                    RepaymentOverDueAmount = RepaymentOverDueAmount - MaxOverdueChargesAmountToBeSettle.Value;
                                    totalTxnOverduePaidAmount += MaxOverdueChargesAmountToBeSettle.Value;

                                }


                                //if (isExtraAmount && transaction.ExtraPaymentAmount < ExtraAmount && RepaymentExtraAmount >= (ExtraAmount - transaction.ExtraPaymentAmount))
                                //{
                                //    txn.ExtraAmount += ExtraAmount - (transaction.ExtraPaymentAmount ?? 0);
                                //    RepaymentExtraAmount = RepaymentExtraAmount - (ExtraAmount - (transaction.ExtraPaymentAmount ?? 0));
                                //}

                                var txnExtraRemainingAmount = transaction.PricipleAmount - transaction.PaymentAmount - txn.PrincipalAmount;
                                var MaxExtraAmountToBeSettle = (principlePaid - totalTxnPrinciplePaidAmount) >= txnExtraRemainingAmount ? txnExtraRemainingAmount : principlePaid - totalTxnPrinciplePaidAmount;
                                MaxExtraAmountToBeSettle = RepaymentExtraAmount > MaxExtraAmountToBeSettle ? MaxExtraAmountToBeSettle : RepaymentExtraAmount;
                                //if (isDoubleConverted &&  transaction.PaymentAmount < principlePaid && Repaymentprincipalamount >= (principlePaid - transaction.PaymentAmount))
                                if (isDoubleConverted && totalTxnPrinciplePaidAmount < principlePaid && MaxExtraAmountToBeSettle > 0)
                                {
                                    txn.PrincipalAmount += MaxExtraAmountToBeSettle.Value;
                                    RepaymentExtraAmount = RepaymentExtraAmount - MaxExtraAmountToBeSettle.Value;
                                    totalTxnPrinciplePaidAmount += MaxExtraAmountToBeSettle.Value;
                                }

                                double TotalPaidAmount = txn.PrincipalAmount + txn.PenalAmount + txn.InterestAmount + txn.ExtraAmount + txn.OverdueAmount;
                                if (TotalPaidAmount > 0)
                                {
                                    transactionList.Add(txn);
                                }
                            }
                        }
                    }

                    if (transactionList != null && transactionList.Any())
                    {
                        await SettleTransaction(transactionList, productConfigs);
                    }
                }

                payment.RemainingExtraPaymentAmount = RepaymentExtraAmount;
                payment.RemainingInterestAmount = RepaymentInterestAmount;
                payment.RemainingOverdueInterest = RepaymentOverDueAmount;
                payment.RemainingPenalInterest = RepaymentPenalAmount;
                payment.RemainingPrincipalAmount = Repaymentprincipalamount;
                //payment.RemainingProcessingFees =0;
                if (payment.RemainingExtraPaymentAmount + payment.RemainingInterestAmount + payment.RemainingOverdueInterest + payment.RemainingPenalInterest + payment.RemainingPrincipalAmount > 0.2)
                {
                    payment.Status = BlackSoilRepaymentConstants.PartialSettled;
                }
                else
                {
                    payment.Status = BlackSoilRepaymentConstants.Settled;

                }
                _context.Entry(payment).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction1.CommitAsync();

            }
            catch (Exception ex)
            {
                await transaction1.RollbackAsync();
                throw ex;
            }
            return "";
        }

        public async Task<bool> SettleTransaction(List<TransactionSettlementDetailDTO> TransactionSettleByManuals, GRPCRequest<SettlePaymentJobRequest> productConfigs)
        {
            var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();

            List<AccountTransactionDetail> accountTransactionDetails = new List<AccountTransactionDetail>();

            foreach (var item in TransactionSettleByManuals)
            {
                await SettleTransactionDetail(item, productConfigs);
            }

            return true;
        }
        public async Task<bool> SettleTransactionDetail(TransactionSettlementDetailDTO item, GRPCRequest<SettlePaymentJobRequest> productConfigs)
        {
            var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();
            List<AccountTransactionDetail> accountTransactionDetails = new List<AccountTransactionDetail>();
            var query = from t in _context.AccountTransactions.Where(x => x.ParentAccountTransactionsID == item.ParentAccountTransactionId)
                        join tt in _context.TransactionTypes on t.TransactionTypeId equals tt.Id
                        where tt.Code == TransactionTypesConstants.OrderPayment && t.IsActive && !t.IsDeleted
                        select t;


            AccountTransaction accountTransaction = await query.FirstOrDefaultAsync();
            var TransactionTypes = await _context.TransactionTypes.Where(x => x.IsActive).ToListAsync();
            var TransactionTypeId_OrderPlacement = TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement);
            var TransactionTypesID_OrderPayment = await _context.TransactionTypes.Where(x => x.Code == TransactionTypesConstants.OrderPayment).FirstOrDefaultAsync();
            var TransactionStatusesId_Paid = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();

            var TransactionTypeIdList = TransactionTypes.Where(x => x.Code == TransactionTypesConstants.OrderPlacement
            || x.Code == TransactionTypesConstants.OverdueInterest
            || x.Code == TransactionTypesConstants.PenaltyCharges
            || x.Code == TransactionTypesConstants.OrderPayment).Select(x => x.Id).ToList();

            var AccountTransactionsExist = await _context.AccountTransactions.Where(x => (x.Id == item.ParentAccountTransactionId || x.ParentAccountTransactionsID == item.ParentAccountTransactionId)
            && TransactionTypeIdList.Contains(x.TransactionTypeId)
            && x.IsActive && !x.IsDeleted).AsAsyncEnumerable().ToListAsync();
            var AccountTranExist = AccountTransactionsExist.FirstOrDefault(x => x.Id == item.ParentAccountTransactionId && x.TransactionTypeId == TransactionTypeId_OrderPlacement.Id && x.IsActive && !x.IsDeleted);

            long AccountTransactionID = 0, loanAccountId = 0;
            double PaidAmount = 0;// OrderPaymentPaid = 0;
            double TotalPaidAmount = item.PrincipalAmount + item.PenalAmount + item.InterestAmount + item.ExtraAmount + item.OverdueAmount;

            if (AccountTranExist.TransactionStatusId == TransactionStatusesId_Paid.Id)
            { return true; }
            else
            {
                if (accountTransaction == null)
                {
                    loanAccountId = AccountTranExist.LoanAccountId;

                    accountTransaction = new AccountTransaction
                    {
                        AnchorCompanyId = AccountTranExist.AnchorCompanyId,
                        LoanAccountId = loanAccountId,
                        ReferenceId = AccountTranExist.ReferenceId,
                        CustomerUniqueCode = AccountTranExist.CustomerUniqueCode,
                        TransactionTypeId = TransactionTypesID_OrderPayment.Id,
                        IsActive = true,
                        IsDeleted = false,
                        CompanyProductId = AccountTranExist.CompanyProductId,
                        TransactionStatusId = (item.OrderPrincipalAmount == (item.PrincipalAmount)) ? TransactionStatusesId_Paid.Id : AccountTranExist.TransactionStatusId,
                        OrderAmount = 0,//TotalPaidAmount,
                        PaidAmount = 0,//TotalPaidAmount,
                        ProcessingFeeType = "",
                        ProcessingFeeRate = 0,
                        GstRate = AccountTranExist.GstRate,
                        InterestType = AccountTranExist.InterestType,
                        InterestRate = AccountTranExist.InterestRate,
                        CreditDays = AccountTranExist.CreditDays,
                        BounceCharge = AccountTranExist.BounceCharge,
                        DelayPenaltyRate = AccountTranExist.DelayPenaltyRate,
                        PayableBy = AccountTranExist.PayableBy,
                        ParentAccountTransactionsID = AccountTranExist.Id,
                        InvoiceId = AccountTranExist.InvoiceId,
                        InvoiceDate = AccountTranExist.InvoiceDate,
                        InvoiceNo = AccountTranExist.InvoiceNo,
                        DueDate = AccountTranExist.DueDate,
                        DisbursementDate = AccountTranExist.DisbursementDate,

                        //OrderDate = DateTime.UtcNow
                    };
                    _context.AccountTransactions.Add(accountTransaction);
                    _context.SaveChanges();
                    AccountTransactionID = accountTransaction.Id;
                }
                try
                {


                    if (accountTransaction != null)
                    {

                        #region ----------S------ For ReCalculation of Overdue / Penal-------------
                        //var curDate = DateTime.Today;
                        var curDate = item.PaymentDate;

                        var dd = AccountTranExist.DueDate.Value.Date.AddDays(1);
                        //while (dd <= DateTime.Today)
                        while (dd <= curDate)
                        {
                            //PrincipalAmount  
                            var OverDueInterestCharge = await _delayPenalManager.OverDueInterestCharge(dd, AccountTranExist.ReferenceId, AccountTranExist.LoanAccountId);
                            //PrincipalAmount  + InterestAmount
                            var OverDueInterestCharge1 = await _delayPenalManager.InsertDailyPenaltyCharges(dd, AccountTranExist.ReferenceId, AccountTranExist.LoanAccountId);
                            dd = dd.Date.AddDays(1);
                        }
                        #endregion


                        AccountTransactionID = accountTransaction.Id;
                        loanAccountId = accountTransaction.LoanAccountId;
                        //OrderPaymentPaid = accountTransaction.PaidAmount;

                        accountTransaction.SettlementDate = item.PaymentDate;
                        accountTransaction.PaidAmount += TotalPaidAmount; //item.PrincipalAmount;
                        accountTransaction.OrderAmount = accountTransaction.PaidAmount;

                        if (item.PrincipalAmount > 0)
                        {
                            AccountTransactionDetail penalaccountTransaction = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = -1.0 * item.PrincipalAmount,
                                PaymentReqNo = item.PaymentRefNo,
                                PaymentDate = item.PaymentDate,
                                TransactionDate = item.PaymentDate,
                                TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Payment).Id,
                                PaymentMode = item.ModeOfPaymentSourceType,
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = true,
                                Status = "success"
                            };
                            accountTransactionDetails.Add(penalaccountTransaction);
                        }

                        if (item.PenalAmount > 0)
                        {
                            AccountTransactionDetail penalaccountTransaction = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = -1.0 * item.PenalAmount,
                                PaymentReqNo = item.PaymentRefNo,
                                PaymentDate = item.PaymentDate,
                                TransactionDate = item.PaymentDate,
                                TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.PenalPaymentAmount).Id,
                                PaymentMode = item.ModeOfPaymentSourceType,
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = true,
                                Status = "success"
                            };
                            accountTransactionDetails.Add(penalaccountTransaction);
                        }

                        if (item.InterestAmount > 0)
                        {
                            AccountTransactionDetail InterestaccountTransactionDetail = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = -1.0 * item.InterestAmount,
                                PaymentReqNo = item.PaymentRefNo,
                                PaymentDate = item.PaymentDate,
                                TransactionDate = item.PaymentDate,
                                TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.InterestPaymentAmount).Id,
                                PaymentMode = item.ModeOfPaymentSourceType,
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = true,
                                Status = "success"
                            };
                            accountTransactionDetails.Add(InterestaccountTransactionDetail);
                        }

                        //#region Calculate Intreast
                        //var perDayInterestAmount = 0.00;
                        //long creditDays = 0;
                        //double TotalinterestAmount = 0;
                        //double transactionBalanceAmount = item.OrderPrincipalAmount - (item.OrderPrincipleAlreadyPaidAmount + item.PrincipalAmount);

                        var loanAccount = _context.LoanAccounts.FirstOrDefault(x => x.Id == AccountTranExist.LoanAccountId);
                        //DateTime firstDate = item.PaymentDate;
                        //DateTime secondDate = AccountTranExist.DueDate.Value;
                        //bool isSkipPaidInterest = true;

                        //var result = _loanNBFCFactory.GetService(loanAccount.NBFCIdentificationCode);
                        //double InterestRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(AccountTranExist.InterestRate));
                        //if (InterestRatePerDay > 0) //-----------30/05/2024--------------------------
                        //{
                        //    ////if (transactionBalanceAmount > 0)
                        //    if (transactionBalanceAmount > 0 && item.PrincipalAmount > 0) //-----------30/05/2024--------------------------
                        //    {
                        //        perDayInterestAmount = AccountTranExist.InterestType == "Percentage" ? (transactionBalanceAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                        //        //-----S------30/05/2024--------------------------
                        //        //// firstDate = item.PaymentDate; 
                        //        //if (AccountTranExist.DueDate.Value.Date > item.PaymentDate.Date) //Use For Advance Payment. true: 5>=3 , false: 5>=7
                        //        //{
                        //            if (item.PaymentDate.Date >= AccountTranExist.DisbursementDate.Value.Date)
                        //            { firstDate = item.PaymentDate; }
                        //            else
                        //            { firstDate = AccountTranExist.DisbursementDate.Value; } //Use For Advance Payment. true: 5>=3 , false: 5>=7
                        //        //}
                        //        //else
                        //        //{
                        //        //    firstDate = item.PaymentDate;
                        //        //}

                        //        creditDays = Convert.ToInt64((secondDate.Date - firstDate.Date).Days);
                        //    }
                        //    if (item.PrincipalAmount > 0)
                        //    {
                        //        if (item.PaymentDate.Date > AccountTranExist.DueDate.Value.Date) //Use For Advance Payment. true: 5>=3 , false: 5>=7
                        //        {
                        //            Console.WriteLine("Not Re-calculate Interest");
                        //        }
                        //        else
                        //        {
                        //            //var interestTransDetails = await _orderPlacementManager.CalculateIntrest(AccountTranExist.Id, perDayInterestAmount, Convert.ToInt64(creditDays), item.PaymentDate, AccountTranExist.PayableBy, true);
                        //            var interestTransDetails = await _orderPlacementManager.CalculateIntrest(AccountTranExist.Id, perDayInterestAmount, Convert.ToInt64(creditDays), firstDate, AccountTranExist.PayableBy, isSkipPaidInterest);

                        //            if (interestTransDetails != null && interestTransDetails.Any())
                        //            {
                        //                accountTransactionDetails.AddRange(interestTransDetails);
                        //                TotalinterestAmount = interestTransDetails.Where(x => x.IsActive).Sum(x => x.Amount);
                        //            }
                        //        }
                        //            //-----E------30/05/2024--------------------------
                        //    }
                        //}
                        //#endregion

                        if (item.ExtraAmount > 0)
                        {
                            AccountTransactionDetail ExtraAccountTransactionDetail = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = -1.0 * item.ExtraAmount,
                                PaymentReqNo = item.PaymentRefNo,
                                PaymentDate = item.PaymentDate,
                                TransactionDate = item.PaymentDate,
                                TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.ExtraPaymentAmount).Id,
                                PaymentMode = item.ModeOfPaymentSourceType,
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = true,
                                Status = "success"
                            };
                            accountTransactionDetails.Add(ExtraAccountTransactionDetail);
                        }
                        if (item.OverdueAmount > 0)
                        {
                            AccountTransactionDetail BounceAccountTransactionDetail = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = -1.0 * item.OverdueAmount,
                                PaymentReqNo = item.PaymentRefNo,
                                PaymentDate = item.PaymentDate,
                                TransactionDate = item.PaymentDate,
                                TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.OverduePaymentAmount).Id,
                                PaymentMode = item.ModeOfPaymentSourceType,
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = true,
                                Status = "success"
                            };
                            accountTransactionDetails.Add(BounceAccountTransactionDetail);
                        }
                        _context.AccountTransactionDetails.AddRangeAsync(accountTransactionDetails);
                        _context.Entry(accountTransaction).State = EntityState.Modified;


                        AccountTranExist.PaidAmount = (item.OrderPrincipleAlreadyPaidAmount + item.PrincipalAmount);
                        _context.Entry(AccountTranExist).State = EntityState.Modified;

                        var loanAccountcredit = _context.LoanAccountCredits.Where(x => x.LoanAccountId == loanAccountId).FirstOrDefault();
                        loanAccountcredit.CreditLimitAmount = loanAccountcredit.CreditLimitAmount + item.PrincipalAmount;
                        _context.Entry(loanAccountcredit).State = EntityState.Modified;


                        if (item.OrderPrincipalAmount == (item.OrderPrincipleAlreadyPaidAmount + item.PrincipalAmount)
                            && (Math.Abs((decimal)(item.InterestAmountOfOrder - (item.InterestPaymentAlreadyPaidAmount + item.InterestAmount))) < 2)
                            && (Math.Abs((decimal)((item.OverdueInterestAmountOfOrder) - (item.OverduePaymentAlreadyPaidAmount + item.OverdueAmount))) < 2)
                            && (Math.Abs((decimal)((item.DelayPenalityAmountOfOrder) - (item.PenalPaymentAlreadyPaidAmount + item.PenalAmount))) < 2)
                            )
                        {
                            foreach (var ACT in AccountTransactionsExist)
                            {
                                //ACT.TransactionStatusId = TransactionStatusesId_Paid.Id;
                                //_context.Entry(ACT).State = EntityState.Modified;
                            }
                        }


                        await _context.SaveChangesAsync();

                        //------------------S : Make log---------------------
                        #region Make History
                        if (productConfigs.Request != null && productConfigs.Request.IsRunningManually == true)
                        { }
                        else
                        {
                            long leadId = _context.LoanAccounts.FirstOrDefault(x => x.Id == loanAccountId && x.IsActive && !x.IsDeleted).LeadId;
                            string doctype_Payments = "Payments";

                            var resultHistory = await _loanAccountHistoryManager.GetLeadHistroy(leadId, doctype_Payments, loanAccountId);
                            LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                            {
                                LeadId = leadId,
                                UserID = resultHistory.UserId,
                                UserName = "",
                                EventName = doctype_Payments,//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                                Narretion = resultHistory.Narretion,
                                NarretionHTML = resultHistory.NarretionHTML,
                                CreatedTimeStamp = resultHistory.CreatedTimeStamp
                            };
                            await _massTransitService.Publish(histroyEvent);
                        }
                        #endregion
                        //------------------E : Make log---------------------



                        #region Calculate Intreast
                        if (item.PrincipalAmount > 0) //validate for principal amount >0 then its a Payment date of principal amount.
                        {
                            var CalculateIntrestAmt = await Re_CalculateIntrestAmount(loanAccount.NBFCIdentificationCode, item.ParentAccountTransactionId, item.PaymentDate.Date, AccountTranExist.PayableBy);
                        }
                        #endregion

                        #region ----------S------ For ReCalculation of Overdue / Penal-------------
                        //var tillCurDate = DateTime.Today;
                        var tillCurDate = DateTime.Today;

                        #region Save/Get Paid Payment Date
                        var LoanAccount_Id = new SqlParameter("LoanAccountId", loanAccountId);
                        var ActionType_value = new SqlParameter("ActionType", "Get");
                        //var Invoice_Date = new SqlParameter("InvoiceDate", invoiceDate == null ? DBNull.Value : (object)invoiceDate);
                        var tmpAccountTransactionPaymentInfoList = await _context.Database.SqlQueryRaw<GetAccountTransactionPaymentInfoDC>("exec sp_tmpAccountTransactionPaymentInfo @ActionType,@LoanAccountId", ActionType_value, LoanAccount_Id).ToListAsync();
                        if (tmpAccountTransactionPaymentInfoList != null && tmpAccountTransactionPaymentInfoList.Any())
                        {
                            if (tmpAccountTransactionPaymentInfoList.FirstOrDefault(x => x.ParentAccountTransactionsID == item.ParentAccountTransactionId) != null)
                            {
                                tillCurDate = tmpAccountTransactionPaymentInfoList.FirstOrDefault(x => x.ParentAccountTransactionsID == item.ParentAccountTransactionId).NewPaymentDate.Value;
                            }
                        }
                        #endregion

                        var PaymentDT = item.PaymentDate.Date.AddDays(1);
                        //while (PaymentDT <= DateTime.Today)
                        while (PaymentDT <= tillCurDate)
                        {
                            var OverDueInterestCharge = await _delayPenalManager.OverDueInterestCharge(PaymentDT, AccountTranExist.ReferenceId, AccountTranExist.LoanAccountId);
                            var OverDueInterestCharge1 = await _delayPenalManager.InsertDailyPenaltyCharges(PaymentDT, AccountTranExist.ReferenceId, AccountTranExist.LoanAccountId);

                            PaymentDT = PaymentDT.Date.AddDays(1);
                        }
                        #endregion




                        //-----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                        //long LoanAccountId = AccountTransactionsExist.LoanAccountId;
                        var PendingTransactionsList = await GetOutstandingTransactionsList("", loanAccountId, item.PaymentDate.Date);
                        if (PendingTransactionsList != null && PendingTransactionsList.Any())
                        {
                            var transactions = PendingTransactionsList.Where(x => x.InvoiceNo == AccountTranExist.InvoiceNo).ToList();

                            var OrderPrincipalAmount = Convert.ToDouble(transactions.Sum(x => x.PricipleAmount));
                            var totalTxnPrinciplePaidAmount = Convert.ToDouble(transactions.Sum(x => x.PaymentAmount));

                            var InterestAmountOfOrder = Convert.ToDouble(transactions.Sum(x => x.InterestAmount));
                            var totalTxnInterestPaidAmount = Convert.ToDouble(transactions.Sum(x => x.InterestPaymentAmount));

                            var PenaltyChargesAmount = Convert.ToDouble(transactions.Sum(x => x.PenaltyChargesAmount));
                            var totalPenalPaymentAmount = Convert.ToDouble(transactions.Sum(x => x.PenalPaymentAmount));

                            var OverdueInterestAmount = Convert.ToDouble(transactions.Sum(x => x.OverdueInterestAmount));
                            var totalOverduePaymentAmount = Convert.ToDouble(transactions.Sum(x => x.OverduePaymentAmount));

                            //if ((OrderPrincipalAmount == totalTxnPrinciplePaidAmount)
                            if (Math.Abs((OrderPrincipalAmount - (totalTxnPrinciplePaidAmount))) < 0.10
                                //&& (Math.Abs((decimal)(InterestAmountOfOrder - (totalTxnInterestPaidAmount))) < 2)
                                //&& (Math.Abs((decimal)(PenaltyChargesAmount - (totalPenalPaymentAmount))) < 2)
                                //&& (Math.Abs((decimal)(OverdueInterestAmount - (totalOverduePaymentAmount))) < 2)
                                )
                            {
                                foreach (var ACT in AccountTransactionsExist)
                                {
                                    ACT.TransactionStatusId = TransactionStatusesId_Paid.Id;
                                    _context.Entry(ACT).State = EntityState.Modified;
                                }
                                await _context.SaveChangesAsync();
                            }
                        }
                        //----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

                    }



                    #region Update Paid status for Invoice
                    //var trasnStatues = new List<string> {
                    //TransactionStatuseConstants.Initiate,
                    //TransactionStatuseConstants.Failed,
                    //TransactionStatuseConstants.Canceled
                    //    };
                    ////var transactionStatustxn = _context.TransactionStatuses.Where(x => trasnStatues.Contains(x.Code) && x.IsActive && !x.IsDeleted).ToList();

                    var transactionStatustxn = _context.TransactionStatuses.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    var transactionStatus_initiate = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Initiate).FirstOrDefault();
                    var transactionStatus_failed = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Failed).FirstOrDefault();
                    var transactionStatus_canceled = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Canceled).FirstOrDefault();

                    long invoiceId = AccountTranExist.InvoiceId ?? 0;

                    var AcTransExist = await _context.AccountTransactions.Where(x => x.InvoiceId == invoiceId
                                        && x.TransactionTypeId == TransactionTypeId_OrderPlacement.Id
                                        && (x.TransactionStatusId != transactionStatus_initiate.Id && x.TransactionStatusId != transactionStatus_failed.Id && x.TransactionStatusId != transactionStatus_canceled.Id)
                                        && x.IsActive && !x.IsDeleted).ToListAsync();
                    if (AcTransExist != null)
                    {
                        if (AcTransExist.Count() == AcTransExist.Count(x => x.TransactionStatusId == TransactionStatusesId_Paid.Id))
                        {
                            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId
                                                    && x.Status != AccountInvoiceStatus.Paid.ToString());
                            if (invoice != null)
                            {
                                invoice.Status = AccountInvoiceStatus.Paid.ToString();
                                _context.Entry(invoice).State = EntityState.Modified;
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    #endregion

                    try
                    {
                        if (item.InterestAmount > 0 || item.OverdueAmount > 0 || item.PenalAmount > 0)
                        {
                            var res = await SettleTransactionDetail_ScaleUpShareTransaction(new ScaleupSettleTransactionRequestDC
                            {
                                ParentAccountTransactionId = item.ParentAccountTransactionId,
                                InterestPaymentAmount = item.InterestAmount,
                                OverDuePaymentAmount = item.OverdueAmount,
                                PenalPaymentAmount = item.PenalAmount,
                                PaymentReqNo = item.PaymentRefNo,
                                TransactionTypeCode = TransactionTypesConstants.ScaleupShareAmount,
                                BouncePaymentAmount = 0,
                                LoanAccountId = loanAccountId,
                                ProductConfigs = productConfigs.Request,
                                PaymentDate = item.PaymentDate
                            });
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            return true;
        }
        public async Task<bool> SettleTransactionDetail_ScaleUpShareTransaction(ScaleupSettleTransactionRequestDC request)
        {
            var loanAccount = await _context.LoanAccounts.Where(x => x.Id == request.LoanAccountId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();

            if (loanAccount != null && request.ProductConfigs != null && request.ProductConfigs.AnchorCompanyConfigs != null && request.ProductConfigs.NBFCCompanyConfigs != null)
            {
                var anchorConfig = request.ProductConfigs.AnchorCompanyConfigs.FirstOrDefault(x => x.AnchorCompanyId == loanAccount.AnchorCompanyId && x.ProductId == loanAccount.ProductId);
                var nbfcConfig = request.ProductConfigs.NBFCCompanyConfigs.FirstOrDefault(x => x.NBFCCompanyId == loanAccount.NBFCCompanyId && x.ProductId == loanAccount.ProductId);
                if (anchorConfig != null && nbfcConfig != null)
                {
                    double OverDueSharePercent = nbfcConfig.IsInterestRateCoSharing ? anchorConfig.AnnualInterestRate - nbfcConfig.AnnualInterestRate : 0;
                    double PenalSharePercent = nbfcConfig.IsPenaltyChargeCoSharing ? anchorConfig.DelayPenaltyRate - nbfcConfig.PenaltyCharges : 0;
                    double InterestSharePercent = nbfcConfig.IsInterestRateCoSharing ? anchorConfig.AnnualInterestRate - nbfcConfig.AnnualInterestRate : 0;
                    double BounceShareAmount = nbfcConfig.IsBounceChargeCoSharing ? anchorConfig.BounceCharge - nbfcConfig.BounceCharge : 0;

                    double OverDuePaymentAmount = request.OverDuePaymentAmount;
                    double PenalPaymentAmount = request.PenalPaymentAmount;
                    double InterestPaymentAmount = request.InterestPaymentAmount;
                    double BouncePaymentAmount = request.BouncePaymentAmount;

                    //0.91 for BlackSoil
                    //double OverDueAmount = Math.Round((OverDuePaymentAmount * OverDueSharePercent / 100.0) * 0.91, 2);
                    //double PenalAmount = Math.Round((PenalPaymentAmount * PenalSharePercent / 100.0) * 0.91, 2);
                    //double InterestAmount = Math.Round((InterestPaymentAmount * InterestSharePercent / 100.0) * 0.91, 2);
                    //double BounceAmount = Math.Round((BouncePaymentAmount * BounceSharePercent / 100.0) * 0.91, 2);
                    double OverDueAmount = Math.Round((OverDuePaymentAmount / anchorConfig.AnnualInterestRate * OverDueSharePercent) * 0.91, 2);
                    double PenalAmount = Math.Round((PenalPaymentAmount / anchorConfig.DelayPenaltyRate * PenalSharePercent) * 0.91, 2);
                    double InterestAmount = Math.Round((InterestPaymentAmount / anchorConfig.AnnualInterestRate * InterestSharePercent) * 0.91, 2);
                    double BounceAmount = Math.Round(BounceShareAmount * 0.91, 2);
                    //request.Request.ProcessingFeeType == "Percentage" ? (OverDuePaymentAmount * OverDueSharePercent / 100.0) : OverDuePaymentAmount; 

                    var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();
                    var TransactionType = _context.TransactionTypes.Where(x => x.Code == request.TransactionTypeCode && x.IsActive && !x.IsDeleted).FirstOrDefault();
                    var TransactionStatusesId = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Pending && x.IsActive && !x.IsDeleted).FirstOrDefault();

                    var accountTransactionList = await _context.AccountTransactions.Where(x => (x.Id == request.ParentAccountTransactionId || x.ParentAccountTransactionsID == request.ParentAccountTransactionId) && x.IsActive && !x.IsDeleted).ToListAsync();

                    if (accountTransactionList != null && accountTransactionList.Any() && TransactionType != null && TransactionStatusesId != null)
                    {
                        long AccountTransactionID = 0;
                        AccountTransaction? ScaleUpAccTrans = accountTransactionList.FirstOrDefault(x => x.ParentAccountTransactionsID == request.ParentAccountTransactionId && x.TransactionTypeId == TransactionType.Id);
                        if (ScaleUpAccTrans != null)
                        {
                            AccountTransactionID = ScaleUpAccTrans.Id;

                        }
                        else
                        {
                            AccountTransaction? parentAccTrans = accountTransactionList.FirstOrDefault(x => x.Id == request.ParentAccountTransactionId);
                            if (parentAccTrans != null)
                            {
                                ScaleUpAccTrans = new AccountTransaction
                                {
                                    CustomerUniqueCode = parentAccTrans.CustomerUniqueCode,
                                    LoanAccountId = parentAccTrans.LoanAccountId,
                                    AnchorCompanyId = request.ProductConfigs.FintechCompanyId,

                                    CompanyProductId = parentAccTrans.CompanyProductId,
                                    ParentAccountTransactionsID = request.ParentAccountTransactionId,
                                    TransactionTypeId = TransactionType.Id,  //TransactionTypesConstants.ScaleupShare,
                                    TransactionStatusId = TransactionStatusesId.Id,
                                    TransactionAmount = 0,
                                    OrderAmount = 0,
                                    PaidAmount = 0,
                                    ProcessingFeeType = "",
                                    ProcessingFeeRate = 0,
                                    GstRate = request.ProductConfigs.GSTRate,
                                    InterestType = ValueTypeConstants.Percentage,
                                    InterestRate = InterestSharePercent,
                                    BounceCharge = BounceShareAmount,
                                    DelayPenaltyRate = PenalSharePercent,
                                    CreditDays = 0,
                                    PayableBy = "Company",
                                    ReferenceId = parentAccTrans.ReferenceId,
                                    DueDate = parentAccTrans.DueDate,
                                    InvoiceDate = parentAccTrans.InvoiceDate,
                                    InvoiceNo = parentAccTrans.InvoiceNo,
                                    InvoiceId = parentAccTrans.InvoiceId,
                                    InvoicePdfURL = parentAccTrans.InvoicePdfURL,
                                    OrderDate = parentAccTrans.OrderDate,
                                    DisbursementDate = parentAccTrans.DisbursementDate,

                                    IsActive = true,
                                    IsDeleted = false
                                };
                                _context.AccountTransactions.Add(ScaleUpAccTrans);
                                _context.SaveChanges();
                                AccountTransactionID = ScaleUpAccTrans.Id;
                            }
                        }

                        List<AccountTransactionDetail> accountTransactionDetails = new List<AccountTransactionDetail>();
                        DateConvertHelper dateConvertHelper = new DateConvertHelper();
                        DateTime transactionDate = dateConvertHelper.GetIndianStandardTime();
                        //Overdue Entry
                        if (OverDueAmount > 0)
                        {
                            var ScaleUpShareOverdueAmountHeadId = TransactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpShareOverdueAmount).Id;
                            //Remove Existing transaction Detail of Same PaymentReqNo and head
                            var existingTDetails = await _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == AccountTransactionID && x.PaymentReqNo == request.PaymentReqNo && x.TransactionDetailHeadId == ScaleUpShareOverdueAmountHeadId && x.IsActive && !x.IsDeleted).ToListAsync();
                            if (existingTDetails != null && existingTDetails.Any())
                            {
                                foreach (var item in existingTDetails)
                                {
                                    item.IsActive = false;
                                    item.IsDeleted = true;
                                    _context.Entry(item).State = EntityState.Modified;
                                }
                            }
                            AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = OverDueAmount,
                                PaymentDate = request.PaymentDate,
                                TransactionDate = request.PaymentDate,
                                TransactionDetailHeadId = ScaleUpShareOverdueAmountHeadId,
                                PaymentReqNo = request.PaymentReqNo,
                                Status = "success",
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = false
                            };
                            accountTransactionDetails.Add(accountTransactionDtl);
                            ScaleUpAccTrans.OrderAmount = ScaleUpAccTrans.OrderAmount + OverDueAmount;
                        }

                        //Penal Entry
                        if (PenalAmount > 0)
                        {
                            var ScaleUpSharePenalAmountHeadId = TransactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpSharePenalAmount).Id;
                            //Remove Existing transaction Detail of Same PaymentReqNo and head
                            var existingTDetails = await _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == AccountTransactionID && x.PaymentReqNo == request.PaymentReqNo && x.TransactionDetailHeadId == ScaleUpSharePenalAmountHeadId && x.IsActive && !x.IsDeleted).ToListAsync();
                            if (existingTDetails != null && existingTDetails.Any())
                            {
                                foreach (var item in existingTDetails)
                                {
                                    item.IsActive = false;
                                    item.IsDeleted = true;
                                    _context.Entry(item).State = EntityState.Modified;
                                }
                            }
                            AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = PenalAmount,
                                PaymentDate = request.PaymentDate,
                                TransactionDate = request.PaymentDate,
                                TransactionDetailHeadId = ScaleUpSharePenalAmountHeadId,
                                PaymentReqNo = request.PaymentReqNo,
                                Status = "success",
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = false
                            };
                            accountTransactionDetails.Add(accountTransactionDtl);
                            ScaleUpAccTrans.OrderAmount = ScaleUpAccTrans.OrderAmount + PenalAmount;
                        }

                        //Interest Entry
                        if (InterestAmount > 0)
                        {
                            var ScaleUpShareInterestAmountHeadId = TransactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpShareInterestAmount).Id;
                            //Remove Existing transaction Detail of Same PaymentReqNo and head
                            var existingTDetails = await _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == AccountTransactionID && x.PaymentReqNo == request.PaymentReqNo && x.TransactionDetailHeadId == ScaleUpShareInterestAmountHeadId && x.IsActive && !x.IsDeleted).ToListAsync();
                            if (existingTDetails != null && existingTDetails.Any())
                            {
                                foreach (var item in existingTDetails)
                                {
                                    item.IsActive = false;
                                    item.IsDeleted = true;
                                    _context.Entry(item).State = EntityState.Modified;
                                }
                            }
                            AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = InterestAmount,
                                PaymentDate = request.PaymentDate,
                                TransactionDate = request.PaymentDate,
                                TransactionDetailHeadId = ScaleUpShareInterestAmountHeadId,
                                PaymentReqNo = request.PaymentReqNo,
                                Status = "success",
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = false
                            };
                            accountTransactionDetails.Add(accountTransactionDtl);
                            ScaleUpAccTrans.OrderAmount = ScaleUpAccTrans.OrderAmount + InterestAmount;
                        }

                        //Bounce Entry
                        if (BouncePaymentAmount > 0)
                        {
                            var ScaleUpShareBounceAmountHeadId = TransactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpShareBounceAmount).Id;
                            //Remove Existing transaction Detail of Same PaymentReqNo and head
                            var existingTDetails = await _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == AccountTransactionID && x.PaymentReqNo == request.PaymentReqNo && x.TransactionDetailHeadId == ScaleUpShareBounceAmountHeadId && x.IsActive && !x.IsDeleted).ToListAsync();
                            if (existingTDetails != null && existingTDetails.Any())
                            {
                                foreach (var item in existingTDetails)
                                {
                                    item.IsActive = false;
                                    item.IsDeleted = true;
                                    _context.Entry(item).State = EntityState.Modified;
                                }
                            }
                            AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                            {
                                AccountTransactionId = AccountTransactionID,
                                Amount = BounceAmount,
                                PaymentDate = request.PaymentDate,
                                TransactionDate = request.PaymentDate,
                                TransactionDetailHeadId = ScaleUpShareBounceAmountHeadId,
                                PaymentReqNo = request.PaymentReqNo,
                                Status = "success",
                                IsActive = true,
                                IsDeleted = false,
                                IsPayableBy = false
                            };
                            accountTransactionDetails.Add(accountTransactionDtl);
                            ScaleUpAccTrans.OrderAmount = ScaleUpAccTrans.OrderAmount + BounceAmount;
                        }

                        await _context.AccountTransactionDetails.AddRangeAsync(accountTransactionDetails);
                        ScaleUpAccTrans.TransactionAmount = ScaleUpAccTrans.OrderAmount;
                        _context.Entry(ScaleUpAccTrans).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                    }
                }

                return true;
            }
            return true;
        }

        public async Task<bool> AddInvoiceSettlementData(AddInvoiceSettlementDataRequestDC request)
        {
            var TransactionHeads = await _context.TransactionDetailHeads.Where(x => x.IsActive).ToListAsync();
            var TransactionType = await _context.TransactionTypes.Where(x => x.Code == request.TransactionTypeCode && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            var TransactionStatuses = await _context.TransactionStatuses.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();

            var accountTransactionList = await _context.AccountTransactions.Where(x => (x.Id == request.ParentAccountTransactionId || x.ParentAccountTransactionsID == request.ParentAccountTransactionId) && x.IsActive && !x.IsDeleted).Include(x => x.TransactionType).ToListAsync();

            if (accountTransactionList != null && accountTransactionList.Any() && TransactionType != null && TransactionStatuses != null)
            {
                long AccountTransactionID = 0;
                AccountTransaction? ScaleUpPaymentTrans = accountTransactionList.FirstOrDefault(x => x.ParentAccountTransactionsID == request.ParentAccountTransactionId && x.TransactionTypeId == TransactionType.Id);
                AccountTransaction? scaleUpShareTrans = accountTransactionList.FirstOrDefault(x => x.ParentAccountTransactionsID == request.ParentAccountTransactionId && x.TransactionType.Code == TransactionTypesConstants.ScaleupShareAmount);
                if (scaleUpShareTrans != null)
                {
                    if (ScaleUpPaymentTrans != null)
                    {
                        AccountTransactionID = ScaleUpPaymentTrans.Id;
                    }
                    else
                    {
                        //AccountTransaction? parentAccTrans = accountTransactionList.FirstOrDefault(x => x.Id == request.ParentAccountTransactionId);

                        ScaleUpPaymentTrans = new AccountTransaction
                        {
                            CustomerUniqueCode = scaleUpShareTrans.CustomerUniqueCode,
                            LoanAccountId = scaleUpShareTrans.LoanAccountId,
                            AnchorCompanyId = scaleUpShareTrans.AnchorCompanyId,

                            CompanyProductId = scaleUpShareTrans.CompanyProductId,
                            ParentAccountTransactionsID = scaleUpShareTrans.ParentAccountTransactionsID,
                            TransactionTypeId = TransactionType.Id,  //TransactionTypesConstants.ScaleupSharePayment,
                            TransactionStatusId = TransactionStatuses.First(x => x.Code == TransactionStatuseConstants.Pending).Id,
                            TransactionAmount = 0,
                            OrderAmount = 0,
                            PaidAmount = 0,
                            ProcessingFeeType = scaleUpShareTrans.ProcessingFeeType,
                            ProcessingFeeRate = scaleUpShareTrans.ProcessingFeeRate,
                            GstRate = scaleUpShareTrans.GstRate,
                            InterestType = scaleUpShareTrans.InterestType,
                            InterestRate = scaleUpShareTrans.InterestRate,
                            BounceCharge = scaleUpShareTrans.BounceCharge,
                            DelayPenaltyRate = scaleUpShareTrans.DelayPenaltyRate,
                            CreditDays = scaleUpShareTrans.CreditDays,
                            PayableBy = "Company",
                            ReferenceId = scaleUpShareTrans.ReferenceId,
                            DueDate = scaleUpShareTrans.DueDate,
                            InvoiceDate = scaleUpShareTrans.InvoiceDate,
                            InvoiceNo = scaleUpShareTrans.InvoiceNo,
                            InvoiceId = scaleUpShareTrans.InvoiceId,
                            InvoicePdfURL = scaleUpShareTrans.InvoicePdfURL,
                            OrderDate = scaleUpShareTrans.OrderDate,
                            DisbursementDate = scaleUpShareTrans.DisbursementDate,

                            IsActive = true,
                            IsDeleted = false
                        };
                        _context.AccountTransactions.Add(ScaleUpPaymentTrans);
                        _context.SaveChanges();
                        AccountTransactionID = ScaleUpPaymentTrans.Id;
                    }

                    List<AccountTransactionDetail> accountTransactionDetails = new List<AccountTransactionDetail>();
                    //ScaleUpSharePaymentAmount
                    if (request.Amount > 0)
                    {
                        AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                        {
                            AccountTransactionId = AccountTransactionID,
                            Amount = (-1) * request.Amount,
                            TransactionDate = request.PaymentDate,
                            PaymentDate = request.PaymentDate,
                            TransactionDetailHeadId = TransactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpSharePaymentAmount).Id,
                            UTRNumber = request.UTRNumber,
                            Status = "success",
                            IsPayableBy = false,
                            IsActive = true,
                            IsDeleted = false
                        };
                        accountTransactionDetails.Add(accountTransactionDtl);
                        ScaleUpPaymentTrans.OrderAmount = ScaleUpPaymentTrans.OrderAmount + accountTransactionDtl.Amount;
                    }
                    if (request.IsFullPayment)
                    {
                        scaleUpShareTrans.TransactionStatusId = TransactionStatuses.First(x => x.Code == TransactionStatuseConstants.Paid).Id;
                        _context.Entry(scaleUpShareTrans).State = EntityState.Modified;
                    }
                    await _context.AccountTransactionDetails.AddRangeAsync(accountTransactionDetails);
                    ScaleUpPaymentTrans.TransactionAmount = ScaleUpPaymentTrans.OrderAmount;
                    _context.Entry(ScaleUpPaymentTrans).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            return true;
        }

        public async Task<List<GetOutstandingTransactionsDTO>> GetOutstandingTransactionsListForDisbursement(string TransactionNo, long LoanAccountId, DateTime? invoiceDate)
        {
            List<GetOutstandingTransactionsDTO> result = new List<GetOutstandingTransactionsDTO>();

            var Transaction_No = new SqlParameter("TransactionNo", TransactionNo);
            var LoanAccount_Id = new SqlParameter("LoanAccountId", LoanAccountId);
            var Invoice_Date = new SqlParameter("InvoiceDate", invoiceDate == null ? DBNull.Value : (object)invoiceDate);

            result = await _context.Database.SqlQueryRaw<GetOutstandingTransactionsDTO>("exec GetOutstandingTransactionsListForDisbursement @TransactionNo, @LoanAccountId, @InvoiceDate", Transaction_No, LoanAccount_Id, Invoice_Date).ToListAsync();

            return result;
        }

        #endregion

        public async Task<string> CalculateOverdueInterestAmount(string NBFCIdentificationCode, long ParentAccountTransactionId, DateTime PaymentDate, string PayableBy)
        {
            List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();

            DateTime DueDate;
            double transactionBalanceAmount = 0;
            String CalculateFor = "";
            string transactionDetailHead = "";
            bool IsRunPaid = false;
            string sResult = "";

            for (int j = 1; j <= 2; j++)
            {
                long LoanAccountId = 0;
                long TransactionId = 0;
                long creditDays = 0;
                double RateOfIntrest = 0.00;
                double perDayAmount = 0;
                bool isSkipPaidInterest = true;

                double OrderPrincipalAmount = 0;
                double InterestAmountOfOrder = 0;
                double totalTxnPrinciplePaidAmount = 0;
                double totalTxnInterestPaidAmount = 0;


                CalculateFor = (j == 1 ? "OverdueInterest" : (j == 2 ? "PenaltyCharges" : ""));
                if (j == 2) { IsRunPaid = true; }

                transactionDetailHead = (CalculateFor == TransactionTypesConstants.OverdueInterest ? TransactionDetailHeadsConstants.OverdueInterestAmount : (CalculateFor == TransactionTypesConstants.PenaltyCharges ? TransactionDetailHeadsConstants.DelayPenalty : ""));

                var TransactionStatusesId_Paid = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();
                var TransactionTypes = _context.TransactionTypes.Where(x => x.IsActive && x.Code == CalculateFor).FirstOrDefault();
                var TransactionTypesOrderPayment = _context.TransactionTypes.Where(x => x.IsActive && x.Code == TransactionTypesConstants.OrderPayment).FirstOrDefault();
                var AcuntTranExist = _context.AccountTransactions.Where(x => (x.Id == ParentAccountTransactionId || x.ParentAccountTransactionsID == ParentAccountTransactionId) && x.IsActive && !x.IsDeleted).ToList();

                var AccountTransactionsExist = AcuntTranExist.Where(x => (x.TransactionTypeId == TransactionTypes.Id)
                && (x.Id == ParentAccountTransactionId || x.ParentAccountTransactionsID == ParentAccountTransactionId) && x.IsActive && !x.IsDeleted).FirstOrDefault();
                if (AccountTransactionsExist != null)
                {
                    RateOfIntrest = (CalculateFor == TransactionTypesConstants.OverdueInterest ? AccountTransactionsExist.InterestRate :
                    (CalculateFor == TransactionTypesConstants.PenaltyCharges ? AccountTransactionsExist.DelayPenaltyRate : 0));

                    LoanAccountId = AccountTransactionsExist.LoanAccountId;
                    var PendingTransactionsList = await GetOutstandingTransactionsList("", LoanAccountId, PaymentDate.Date);
                    if (PendingTransactionsList != null && PendingTransactionsList.Any())
                    {
                        var transactions = PendingTransactionsList.Where(x => x.InvoiceNo == AccountTransactionsExist.InvoiceNo).ToList();

                        OrderPrincipalAmount = Convert.ToDouble(transactions.Sum(x => x.PricipleAmount));
                        InterestAmountOfOrder = Convert.ToDouble(transactions.Sum(x => x.InterestAmount));

                        totalTxnPrinciplePaidAmount = Convert.ToDouble(transactions.Sum(x => x.PaymentAmount));
                        totalTxnInterestPaidAmount = Convert.ToDouble(transactions.Sum(x => x.InterestPaymentAmount));


                        if (CalculateFor == TransactionTypesConstants.OverdueInterest)
                        {
                            transactionBalanceAmount = OrderPrincipalAmount - totalTxnPrinciplePaidAmount;
                        }
                        else if (CalculateFor == TransactionTypesConstants.PenaltyCharges)
                        {
                            transactionBalanceAmount = (OrderPrincipalAmount + InterestAmountOfOrder) - (totalTxnPrinciplePaidAmount + totalTxnInterestPaidAmount);
                        }

                        if (Math.Round(transactionBalanceAmount, 2) <= .01)
                        { transactionBalanceAmount = 0; }


                    }
                }


                if (RateOfIntrest > 0)
                {
                    TransactionId = AccountTransactionsExist.Id;
                    DueDate = AccountTransactionsExist.DueDate.Value.Date;

                    //var AcTraExist = AcuntTranExist.Where(x => (x.TransactionTypeId == TransactionTypesOrderPayment.Id)
                    //&& (x.Id == ParentAccountTransactionId || x.ParentAccountTransactionsID == ParentAccountTransactionId) && x.IsActive && !x.IsDeleted).FirstOrDefault();
                    //var accTranDetails = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == AcTraExist.Id 
                    //                            && x.TransactionDetailHead.Code == TransactionTypesConstants.OrderPayment && x.IsActive && !x.IsDeleted).ToList();

                    var accountTransactionDetails = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == TransactionId
                                                && x.TransactionDetailHead.Code == transactionDetailHead && x.IsActive && !x.IsDeleted).ToList();
                    if (accountTransactionDetails.Count > 0 && accountTransactionDetails.Any())
                    {
                        DateTime firstDate = DueDate.AddDays(Convert.ToInt64(1));
                        DateTime secondDate = DateTime.Now.Date;  //accountTransactionDetails.Max(x => x.TransactionDate.Value.Date);

                        if (transactionBalanceAmount == 0)
                        {
                            if (DueDate.Date > PaymentDate.Date) // true: 5>=3 , false: 5>=7
                            {
                                //Delete All Charges.
                                perDayAmount = 0;
                                creditDays = 0;
                                isSkipPaidInterest = false;
                            }
                        }
                        else
                        {
                            var result = _loanNBFCFactory.GetService(NBFCIdentificationCode);
                            double InterestRatePerDay = await result.CalculatePerDayInterest(RateOfIntrest);
                            perDayAmount = AccountTransactionsExist.InterestType == "Percentage" ? (transactionBalanceAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                            creditDays = Convert.ToInt64((secondDate.Date - firstDate.Date).Days);

                            if (DueDate.Date > PaymentDate.Date) // true: 5>=3 , false: 5>=7
                            {
                                //Delete All Charges.
                                perDayAmount = 0;
                                creditDays = 0;
                                isSkipPaidInterest = false;
                            }
                            else if (DueDate.Date < PaymentDate.Date)
                            {
                                // Recalculate Charges.
                                firstDate = PaymentDate.Date;
                                creditDays = Convert.ToInt64((secondDate.Date - firstDate.Date).Days);
                                isSkipPaidInterest = true;
                            }
                            else
                            {
                                //Not work.
                                TransactionId = 0;
                                perDayAmount = 0;
                                creditDays = 0;

                                goto NextMainLoop;
                            }

                        }

                        if (TransactionId > 0)
                        {
                            foreach (var item in accountTransactionDetails)
                            {
                                if (isSkipPaidInterest == false)
                                {
                                    item.IsActive = false;
                                    item.IsDeleted = true;
                                    _context.Entry(item).State = EntityState.Modified;
                                }
                                if (isSkipPaidInterest == true && item.TransactionDate.Value.Date > PaymentDate.Date)
                                {
                                    item.IsActive = false;
                                    item.IsDeleted = true;
                                    _context.Entry(item).State = EntityState.Modified;
                                }
                            }
                        }

                        if (perDayAmount > 0)
                        {
                            var TransactionHeads = _context.TransactionDetailHeads.FirstOrDefault(x => x.Code == transactionDetailHead);
                            for (int i = 1; i <= creditDays; i++)
                            {
                                transactionDetails.Add(new AccountTransactionDetail()
                                {
                                    AccountTransactionId = TransactionId,
                                    Amount = perDayAmount,
                                    TransactionDetailHeadId = TransactionHeads.Id,
                                    IsPayableBy = PayableBy == "Customer",
                                    IsActive = true,
                                    IsDeleted = false,
                                    TransactionDate = PaymentDate.AddDays(Convert.ToInt64(i)),
                                    Status = "initiate"
                                });
                            }
                        }

                        //if ((TransactionId > 0) || (perDayAmount > 0))
                        if (AccountTransactionsExist != null)
                        {
                            if (isSkipPaidInterest == false && transactionBalanceAmount == 0)
                            {
                                AccountTransactionsExist.IsActive = false;
                                AccountTransactionsExist.IsDeleted = true;
                                _context.Entry(AccountTransactionsExist).State = EntityState.Modified;
                            }
                            else
                            {
                                //Recalculate Ovedue + Penal
                                AccountTransactionsExist.OrderAmount = perDayAmount * creditDays;
                                AccountTransactionsExist.TransactionAmount = perDayAmount * creditDays;
                                _context.Entry(AccountTransactionsExist).State = EntityState.Modified;
                            }

                            //---S-----Add/Update in Detail ----------------
                            if (transactionDetails != null && transactionDetails.Any())
                            {
                                //accountTransactionDetails.AddRange(transactionDetails);
                                await _context.AccountTransactionDetails.AddRangeAsync(transactionDetails);
                                var TotalineOverdueInterestAmount = transactionDetails.Where(x => x.IsActive).Sum(x => x.Amount);
                            }

                            await _context.SaveChangesAsync();
                            sResult = "Done";
                            //---E-----Add/Update in Detail ----------------
                        }
                    }

                }

            NextMainLoop:
                Console.WriteLine("");
            }

            return sResult;
        }

        public async Task<string> CalculateOverdueInterestAmount_Testing(string InvoiceNo, DateTime PaymentDate)
        {
            string NBFCIdentificationCode = ""; long ParentAccountTransactionId = 0; ; string PayableBy = "";
            string sRes = "";

            var TransactionTypes = _context.TransactionTypes.Where(x => x.IsActive).ToList();
            var TransactionTypes_OrderPlacement = TransactionTypes.Where(x => x.IsActive && x.Code == TransactionTypesConstants.OrderPlacement).FirstOrDefault();
            var AccountTransactionsExist = _context.AccountTransactions.Where(x => (x.TransactionTypeId == TransactionTypes_OrderPlacement.Id)
            && (x.InvoiceNo == InvoiceNo) && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (AccountTransactionsExist != null)
            {
                var lonaAc = _context.LoanAccounts.Where(x => x.Id == AccountTransactionsExist.LoanAccountId && x.IsActive && !x.IsDeleted).FirstOrDefault();

                NBFCIdentificationCode = lonaAc.NBFCIdentificationCode;
                ParentAccountTransactionId = AccountTransactionsExist.Id;
                PayableBy = AccountTransactionsExist.PayableBy;

                var res = await CalculateOverdueInterestAmount(NBFCIdentificationCode, ParentAccountTransactionId, PaymentDate, PayableBy);
                sRes = res;

                if (sRes == "Done")
                {
                    long invoiceId = AccountTransactionsExist.InvoiceId ?? 0;

                    var TransactionTypeId_OrderPlacement = TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement);
                    var TransactionStatusesId_Paid = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();

                    var transactionStatustxn = _context.TransactionStatuses.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    var transactionStatus_initiate = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Initiate).FirstOrDefault();
                    var transactionStatus_failed = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Failed).FirstOrDefault();
                    var transactionStatus_canceled = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Canceled).FirstOrDefault();


                    var AcTransExist = await _context.AccountTransactions.Where(x => x.InvoiceId == invoiceId
                                                && x.TransactionTypeId == TransactionTypeId_OrderPlacement.Id
                                                && (x.TransactionStatusId != transactionStatus_initiate.Id && x.TransactionStatusId != transactionStatus_failed.Id && x.TransactionStatusId != transactionStatus_canceled.Id)
                                                && x.IsActive && !x.IsDeleted).ToListAsync();
                    if (AcTransExist != null)
                    {
                        if (AcTransExist.Count() == AcTransExist.Count(x => x.TransactionStatusId == TransactionStatusesId_Paid.Id))
                        {
                            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId
                                                    && x.Status != AccountInvoiceStatus.Paid.ToString());
                            if (invoice != null)
                            {
                                invoice.Status = AccountInvoiceStatus.Paid.ToString();
                                _context.Entry(invoice).State = EntityState.Modified;
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
            }

            return sRes;
        }

        public async Task<string> Re_CalculateIntrestAmount(string NBFCIdentificationCode, long ParentAccountTransactionId, DateTime PaymentDate, string PayableBy)
        {
            try
            {

                string sresult = "";


                //var TransactionStatusesId_Paid = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();
                var TransactionTypes = _context.TransactionTypes.Where(x => x.IsActive && x.Code == TransactionTypesConstants.OrderPlacement).FirstOrDefault();
                var AcuntTranExist = _context.AccountTransactions.Where(x => (x.Id == ParentAccountTransactionId || x.ParentAccountTransactionsID == ParentAccountTransactionId) && x.IsActive && !x.IsDeleted).ToList();

                var AccountTransactionsExist = AcuntTranExist.Where(x => (x.TransactionTypeId == TransactionTypes.Id)
                && (x.Id == ParentAccountTransactionId || x.ParentAccountTransactionsID == ParentAccountTransactionId) && x.IsActive && !x.IsDeleted).FirstOrDefault();

                long LoanAccountId = AccountTransactionsExist.LoanAccountId;
                long TransactionId = AccountTransactionsExist.Id;

                //---S------Validate Interest-----------
                var accountTransactionDetailsForINterest = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == TransactionId
                                && x.TransactionDetailHead.Code == TransactionDetailHeadsConstants.Interest && x.IsActive && !x.IsDeleted).ToList();

                if (accountTransactionDetailsForINterest.Count() == 0)
                {
                    List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();

                    var result = _loanNBFCFactory.GetService(NBFCIdentificationCode);
                    double InterestRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(AccountTransactionsExist.InterestRate));
                    var perDayInterestAmount1 = AccountTransactionsExist.InterestType == "Percentage" ? (AccountTransactionsExist.TransactionAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                    var interestTransDetails = await _orderPlacementManager.CalculateIntrest(AccountTransactionsExist.Id, perDayInterestAmount1, AccountTransactionsExist.CreditDays.Value, AccountTransactionsExist.DisbursementDate.Value, AccountTransactionsExist.PayableBy);
                    if (interestTransDetails != null && interestTransDetails.Any())
                    {
                        interestTransDetails.ForEach(x => x.Status = "success");
                        transactionDetails.AddRange(interestTransDetails);
                    }
                    await _context.AccountTransactionDetails.AddRangeAsync(transactionDetails);
                    await _context.SaveChangesAsync();
                }
                //---E------Validate Interest-----------

                double transactionBalanceAmount = 0;
                var PendingTransactionsList = await GetOutstandingTransactionsList("", LoanAccountId, PaymentDate.Date);
                if (PendingTransactionsList != null && PendingTransactionsList.Any())
                {
                    var transactions = PendingTransactionsList.Where(x => x.InvoiceNo == AccountTransactionsExist.InvoiceNo).ToList();

                    var OrderPrincipalAmount = Convert.ToDouble(transactions.Sum(x => x.PricipleAmount));
                    var totalTxnPrinciplePaidAmount = Convert.ToDouble(transactions.Sum(x => x.PaymentAmount));

                    transactionBalanceAmount = OrderPrincipalAmount - totalTxnPrinciplePaidAmount;
                }


                long creditDays = 0;
                double perDayInterestAmount = 0;
                DateTime firstDate = PaymentDate.Date;
                DateTime secondDate = AccountTransactionsExist.DueDate.Value;
                bool isSkipPaidInterest = true;

                if (transactionBalanceAmount == 0)
                {
                    if (AccountTransactionsExist.DisbursementDate.Value.Date > PaymentDate.Date) //Use For Advance Payment. true: 5>=3 , false: 5>=7
                    {
                        isSkipPaidInterest = false;
                    }
                }
                else
                {
                    var result = _loanNBFCFactory.GetService(NBFCIdentificationCode);
                    double InterestRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(AccountTransactionsExist.InterestRate));
                    // if (InterestRatePerDay > 0 && transactionBalanceAmount>0) 
                    perDayInterestAmount = AccountTransactionsExist.InterestType == "Percentage" ? (transactionBalanceAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                    //-----S------30/05/2024--------------------------
                    //// firstDate = item.PaymentDate; 
                    //if (AccountTransactionsExist.DueDate.Value.Date > PaymentDate.Date) //Use For Advance Payment. true: 5>=3 , false: 5>=7
                    //{
                    if (PaymentDate.Date >= AccountTransactionsExist.DisbursementDate.Value.Date)
                    { firstDate = PaymentDate; }
                    else
                    { firstDate = AccountTransactionsExist.DisbursementDate.Value; }
                    //}
                    //else
                    //{
                    //    firstDate = PaymentDate.Date;
                    //}

                    //-----E------30/05/2024--------------------------
                    creditDays = Convert.ToInt64((secondDate.Date - firstDate.Date).Days);
                }
                //-----S------30/05/2024--------------------------
                if (PaymentDate.Date > AccountTransactionsExist.DueDate.Value.Date) //Use For Advance Payment. true: 5>=3 , false: 5>=7
                {
                    Console.WriteLine("Not Re-calculate Interest");
                }
                else
                {

                    //var interestTransDetails = await _orderPlacementManager.CalculateIntrest(AccountTranExist.Id, perDayInterestAmount, Convert.ToInt64(creditDays), item.PaymentDate, AccountTranExist.PayableBy, true);
                    var interestTransDetails = await _orderPlacementManager.CalculateIntrest(AccountTransactionsExist.Id, perDayInterestAmount, Convert.ToInt64(creditDays), firstDate, AccountTransactionsExist.PayableBy, isSkipPaidInterest);
                    //-----E------30/05/2024--------------------------

                    if (interestTransDetails != null && interestTransDetails.Any())
                    {
                        _context.AccountTransactionDetails.AddRange(interestTransDetails);
                        await _context.SaveChangesAsync();
                        sresult = "Interest calculated Successfully";
                    }
                }

                return sresult;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        public async Task<string> RemovePaymentEntry_Utility(long loanAcoountId, long LoanAccountRepaymentID, DateTime? paymentDATE)
        {
            using var transaction1 = _context.Database.BeginTransaction();
            string res = "";
            string status = "Error";
            long invoiceId = 0;

            try
            {
                var TransactionTypeId_Paid = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();

                var TransactionStatus = await _context.TransactionStatuses.Where(x => x.IsActive).ToListAsync();
                var TransactionStatusIdList = TransactionStatus.Where(x => x.Code == TransactionStatuseConstants.Pending
                || x.Code == TransactionStatuseConstants.Due
                || x.Code == TransactionStatuseConstants.Overdue
                || x.Code == TransactionStatuseConstants.Paid).Select(x => x.Id).ToList();


                var loanAccountRepayment = await _context.LoanAccountRepayments.Where(x => x.LoanAccountId == loanAcoountId
            && (!paymentDATE.HasValue || x.PaymentDate >= paymentDATE)
            && (LoanAccountRepaymentID == 0 ? x.Id != 0 : x.Id == LoanAccountRepaymentID) && x.Status != status && x.IsActive && !x.IsDeleted).OrderByDescending(x => x.Id).ToListAsync();
                //&& (LoanAccountRepaymentID == 0 ? x.Id != 0 : x.Id == LoanAccountRepaymentID) && x.Status != status && !x.IsActive && x.IsDeleted).OrderByDescending(x => x.Id).ToListAsync();

                #region Use For Interest Calculation
                if (!loanAccountRepayment.Any() && loanAccountRepayment.Count() == 0)
                { return null; }

                var loanAccRepayment = loanAccountRepayment.Select(x => x.BankRefNo).Distinct().ToList();
                var accTransionDetailList = _context.AccountTransactionDetails.Where(x => loanAccRepayment.Contains(x.PaymentReqNo) && x.IsActive && !x.IsDeleted).ToList();
                //var accTransionDetailList = _context.AccountTransactionDetails.Where(x => loanAccRepayment.Contains(x.PaymentReqNo) && !x.IsActive && x.IsDeleted).ToList();
                #endregion

                foreach (var paymentTran in loanAccountRepayment)
                {
                    var totalScaleupPrinciplePaidAmt = paymentTran.PrincipalAmount - paymentTran.RemainingPrincipalAmount;
                    var totalScaleupIntrestPaidAmount = paymentTran.InterestAmount - paymentTran.RemainingInterestAmount;
                    var totalScaleupOverduePaidAmount = paymentTran.OverdueInterest - paymentTran.RemainingOverdueInterest;
                    var totalScaleupPenalPaidAmount = paymentTran.PenalInterest - paymentTran.RemainingPenalInterest;

                    var TotalScaleupPaidAmount = totalScaleupPrinciplePaidAmt + totalScaleupIntrestPaidAmount + totalScaleupOverduePaidAmount + totalScaleupPenalPaidAmount;

                    string bankRefno = string.IsNullOrEmpty(paymentTran.BankRefNo) ? "" : paymentTran.BankRefNo;
                    var accountTransactionDetails = _context.AccountTransactionDetails.Where(x => x.PaymentReqNo == bankRefno && x.IsActive && !x.IsDeleted).ToList();
                    if (accountTransactionDetails.Count > 0 && accountTransactionDetails.Any())
                    {
                        var TransactionStatusesId_Pending = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Pending).FirstOrDefault();

                        var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive && !x.IsDeleted).ToList();
                        var totalTxnPrinciplePaidAmt = Math.Abs(accountTransactionDetails.Where(x => x.TransactionDetailHeadId == TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Payment).Id).Sum(x => x.Amount));
                        var totalTxnIntrestPaidAmount = Math.Abs(accountTransactionDetails.Where(x => x.TransactionDetailHeadId == TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.InterestPaymentAmount).Id).Sum(x => x.Amount));
                        var totalTxnOverduePaidAmount = Math.Abs(accountTransactionDetails.Where(x => x.TransactionDetailHeadId == TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.OverduePaymentAmount).Id).Sum(x => x.Amount));
                        var totalTxnPenalPaidAmount = Math.Abs(accountTransactionDetails.Where(x => x.TransactionDetailHeadId == TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.PenalPaymentAmount).Id).Sum(x => x.Amount));

                        var TotalTransactionPaidAmount = totalTxnPrinciplePaidAmt + totalTxnIntrestPaidAmount + totalTxnOverduePaidAmount + totalTxnPenalPaidAmount;

                        if (Math.Abs((double)(TotalScaleupPaidAmount - TotalTransactionPaidAmount)) < 1)
                        {
                            //Remove:  PAID txn from Child Table.
                            foreach (var item in accountTransactionDetails)
                            {
                                item.IsActive = false;
                                item.IsDeleted = true;
                                _context.Entry(item).State = EntityState.Modified;
                            }


                            //Parent table
                            var accTanDetal = accountTransactionDetails.Select(x => x.AccountTransactionId).Distinct().ToList();
                            foreach (var item in accTanDetal)
                            {
                                var accountTransactionExist = _context.AccountTransactions.Where(x => x.Id == item && x.LoanAccountId == loanAcoountId && x.IsActive && !x.IsDeleted).FirstOrDefault();
                                if (accountTransactionExist != null)
                                {
                                    invoiceId = accountTransactionExist.InvoiceId ?? 0;

                                    ////Remove: PAID txn from Parent Table.
                                    //accountTransactionExist.IsActive = false;
                                    //accountTransactionExist.IsDeleted = true;

                                    accountTransactionExist.OrderAmount = 0;
                                    _context.Entry(accountTransactionExist).State = EntityState.Modified;


                                    //Change Status: in Parent Table.
                                    var accountTransaction = _context.AccountTransactions.Where(x => x.InvoiceNo == accountTransactionExist.InvoiceNo && x.LoanAccountId == loanAcoountId
                                    //&& x.TransactionStatusId == TransactionTypeId_Paid.Id 
                                                && TransactionStatusIdList.Contains(x.TransactionStatusId)
                                                && x.IsActive && !x.IsDeleted).ToList();
                                    foreach (var accTxn in accountTransaction)
                                    {
                                        accTxn.TransactionStatusId = TransactionStatusesId_Pending.Id;
                                        _context.Entry(accTxn).State = EntityState.Modified;
                                    }

                                    try
                                    {
                                        //Change Status: In Invoice Table.
                                        var invoce = await _context.Invoices.Where(x => x.Id == invoiceId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                                        if (invoce != null)
                                        {
                                            if (invoce.Status == AccountInvoiceStatus.Paid.ToString())
                                            {
                                                invoce.Status = AccountInvoiceStatus.Pending.ToString();
                                                _context.Entry(invoce).State = EntityState.Modified;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }

                                }
                            }

                            //Remove: txn from  LoanAccountRepayment table.
                            paymentTran.Status = "Pending";
                            paymentTran.RemainingPrincipalAmount = 0;
                            paymentTran.RemainingInterestAmount = 0;
                            paymentTran.RemainingOverdueInterest = 0;
                            paymentTran.RemainingPenalInterest = 0;
                            paymentTran.RemainingExtraPaymentAmount = 0;
                            paymentTran.IsActive = false;
                            paymentTran.IsDeleted = true;
                            _context.Entry(paymentTran).State = EntityState.Modified;

                            try
                            {
                                await _context.SaveChangesAsync();
                                res = "Done";
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }

                        } //if (Math.Abs((double)(TotalScaleupPaidAmount - TotalTransactionPaidAmount)) < .01)
                        else
                        {
                            res = "Error: Payment Mismatch [LoanAccountRepayments:" + TotalScaleupPaidAmount + "  AccountTransactionDetails: 0 ]";

                            //Remove:  PAID txn from Child Table.
                            foreach (var item in accountTransactionDetails)
                            {
                                item.IsActive = false;
                                item.IsDeleted = true;
                                _context.Entry(item).State = EntityState.Modified;
                            }
                            //Parent table
                            var accTanDetal = accountTransactionDetails.Select(x => x.AccountTransactionId).Distinct().ToList();
                            foreach (var item in accTanDetal)
                            {
                                var accountTransactionExist = _context.AccountTransactions.Where(x => x.Id == item && x.LoanAccountId == loanAcoountId
                                && x.TransactionStatusId == TransactionTypeId_Paid.Id
                                && x.IsActive && !x.IsDeleted).FirstOrDefault();
                                if (accountTransactionExist != null)
                                {
                                    invoiceId = accountTransactionExist.InvoiceId ?? 0;

                                    //Change Status: in Parent Table.
                                    var accountTransaction = _context.AccountTransactions.Where(x => x.InvoiceNo == accountTransactionExist.InvoiceNo && x.LoanAccountId == loanAcoountId
                                    //&& x.TransactionStatusId == TransactionTypeId_Paid.Id
                                    && TransactionStatusIdList.Contains(x.TransactionStatusId)
                                    && x.IsActive && !x.IsDeleted).ToList();
                                    foreach (var accTxn in accountTransaction)
                                    {
                                        accTxn.TransactionStatusId = TransactionStatusesId_Pending.Id;
                                        _context.Entry(accTxn).State = EntityState.Modified;
                                    }

                                    //Change Status: In Invoice Table.
                                    var invoce = await _context.Invoices.Where(x => x.Id == invoiceId && x.LoanAccountId == loanAcoountId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                                    if (invoce != null && (invoce.Status == AccountInvoiceStatus.Paid.ToString()))
                                    {
                                        invoce.Status = AccountInvoiceStatus.Pending.ToString();
                                        _context.Entry(invoce).State = EntityState.Modified;
                                    }
                                }
                            }

                            //Remove: txn from  LoanAccountRepayment table.
                            paymentTran.Status = "Pending";
                            paymentTran.RemainingPrincipalAmount = 0;
                            paymentTran.RemainingInterestAmount = 0;
                            paymentTran.RemainingOverdueInterest = 0;
                            paymentTran.RemainingPenalInterest = 0;
                            paymentTran.RemainingExtraPaymentAmount = 0;
                            paymentTran.IsActive = false;
                            paymentTran.IsDeleted = true;
                            _context.Entry(paymentTran).State = EntityState.Modified;

                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        //totalScaleupPrinciplePaidAmt = paymentTran.PrincipalAmount ;
                        //totalScaleupIntrestPaidAmount = paymentTran.InterestAmount ;
                        //totalScaleupOverduePaidAmount = paymentTran.OverdueInterest;
                        //totalScaleupPenalPaidAmount = paymentTran.PenalInterest - paymentTran.RemainingPenalInterest;
                        //TotalScaleupPaidAmount = totalScaleupPrinciplePaidAmt + totalScaleupIntrestPaidAmount + totalScaleupOverduePaidAmount + totalScaleupPenalPaidAmount;

                        res = $"Error: Payment Mismatch in AccountTransactionDetail Table for {paymentTran.BankRefNo}.";

                        if (TotalScaleupPaidAmount > 0)
                        {
                            //Remove: txn from  LoanAccountRepayment table.
                            paymentTran.Status = "Pending";
                            paymentTran.RemainingPrincipalAmount = 0;
                            paymentTran.RemainingInterestAmount = 0;
                            paymentTran.RemainingOverdueInterest = 0;
                            paymentTran.RemainingPenalInterest = 0;
                            paymentTran.RemainingExtraPaymentAmount = 0;
                            paymentTran.IsActive = false;
                            paymentTran.IsDeleted = true;
                            _context.Entry(paymentTran).State = EntityState.Modified;

                            await _context.SaveChangesAsync();
                            res = "Done  " + res;
                        }
                        else if (TotalScaleupPaidAmount == 0 && paymentTran.Status == "Pending")
                        {
                            paymentTran.IsActive = false;
                            paymentTran.IsDeleted = true;
                            _context.Entry(paymentTran).State = EntityState.Modified;

                            await _context.SaveChangesAsync();
                        }


                    }
                }

                #region Use For Interest Calculation
                if (loanAccountRepayment.Any() && loanAccountRepayment.Count() > 0)
                {
                    var TransactionType_OrderPlacement = await _context.TransactionTypes.FirstOrDefaultAsync(x => x.Code == TransactionTypesConstants.OrderPlacement);
                    //var TransactionStatusesID_Pending = await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.Code == TransactionStatuseConstants.Pending);

                    //var loanAccRepayment = loanAccountRepayment.Select(x => x.BankRefNo).Distinct().ToList();
                    //var accountTransactionDetails = _context.AccountTransactionDetails.Where(x => loanAccRepayment.Contains(x.PaymentReqNo) && x.IsActive && !x.IsDeleted).ToList();
                    //if (accountTransactionDetails.Count > 0 && accountTransactionDetails.Any())
                    //{ return null; }
                    if (accTransionDetailList.Count > 0 && accTransionDetailList.Any())
                    {
                        var accTanDetal = accTransionDetailList.Select(x => x.AccountTransactionId).Distinct().ToList();

                        var accountTransactionExistParent = _context.AccountTransactions.Where(x => accTanDetal.Contains(x.Id) && x.LoanAccountId == loanAcoountId
                        && x.IsActive && !x.IsDeleted).Select(x => x.ParentAccountTransactionsID).Distinct().ToList();

                        var accountTransaction = _context.AccountTransactions.Where(x => accountTransactionExistParent.Contains(x.Id) && x.LoanAccountId == loanAcoountId
                        && x.TransactionTypeId == TransactionType_OrderPlacement.Id && x.IsActive && !x.IsDeleted).ToList();
                        foreach (var accTxn in accountTransaction)
                        {
                            if (accTxn.TransactionTypeId == 2)
                            {
                                //----S------ Recalculate DueDate & Intrest According to Disbursement Date  --------------
                                DateTime disbursement_date = accTxn.DisbursementDate.Value;

                                double OutstandingAmount = 0;
                                //var PendingTransactionsList = await GetOutstandingTransactionsListForDisbursement("", accTxn.LoanAccountId, disbursement_date);
                                //if (PendingTransactionsList != null && PendingTransactionsList.Any())
                                //{
                                //    var transactions = PendingTransactionsList.Where(x => x.Id == accTxn.Id).FirstOrDefault();
                                //    if (transactions != null)
                                //    {
                                //        OutstandingAmount = transactions.Outstanding ?? 0;
                                //    }
                                //}
                                var accountTranDetails = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == accTxn.Id && x.IsActive && !x.IsDeleted).ToList();
                                //OutstandingAmount = accountTranDetails.Where(x => x.TransactionDetailHeadId == 2).Select(x => x.Amount).FirstOrDefault();
                                OutstandingAmount = accountTranDetails.Where(x => x.TransactionDetailHeadId == 2 || x.TransactionDetailHeadId == 7).Sum(x => x.Amount);

                                double totalInterestAmount = 0;
                                if (OutstandingAmount > 0)
                                {
                                    var loanAccount = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == accTxn.LoanAccountId);

                                    double transactionBalanceAmount = 0;
                                    var results = _loanNBFCFactory.GetService(loanAccount.NBFCIdentificationCode);
                                    double InterestRatePerDay = await results.CalculatePerDayInterest(Convert.ToDouble(accTxn.InterestRate));
                                    var perDayInterestAmount = accTxn.InterestType == "Percentage" ? (OutstandingAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                                    DateTime firstDate = disbursement_date;
                                    DateTime secondDate = accTxn.DueDate.Value;
                                    long creditDays = Convert.ToInt64((secondDate.Date - firstDate.Date).Days);

                                    totalInterestAmount = (Math.Round(perDayInterestAmount, 3) * Convert.ToInt64(creditDays));

                                    if (accountTranDetails.Count > 0 && accountTranDetails.Any())
                                    {
                                        double transactionInterestAmount = 0;
                                        transactionInterestAmount = Math.Round(accountTranDetails.Where(x => x.TransactionDetailHeadId == 3).Sum(x => x.Amount), 3);
                                        if (transactionInterestAmount != totalInterestAmount)
                                        {
                                            if (Math.Abs(Math.Round((transactionInterestAmount - totalInterestAmount), 2)) > 0.05)
                                            {
                                                var interestTransDetails = await _orderPlacementManager.CalculateIntrest(accTxn.Id, perDayInterestAmount, Convert.ToInt64(creditDays), disbursement_date, accTxn.PayableBy, false);
                                                if (interestTransDetails != null && interestTransDetails.Any())
                                                {
                                                    await _context.AccountTransactionDetails.AddRangeAsync(interestTransDetails);
                                                    await _context.SaveChangesAsync();
                                                }
                                            }
                                        }
                                    }

                                }
                                //----E------ Recalculate DueDate & Intrest According to Disbursement Date  --------------
                            }
                        }
                    }

                }
                #endregion


                await transaction1.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction1.RollbackAsync();
                throw ex;
            }

            return res;
        }

        public async Task<string> RemoveAndPaidALLPaymentEntry_Utility(long loanAcoountId, DateTime? paymentDATE)
        {
            string res = "";
            string status = "Error";

            #region Use For ReProcess Payment entry from Day one TO till today
            var refresh = await _context.LoanAccountRepaymentRefreshs.Where(x => x.LoanAccountId == loanAcoountId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (refresh == null)
            {
                LoanAccountRepaymentRefresh loanAccoRepaymentRefresh = null;
                loanAccoRepaymentRefresh = new LoanAccountRepaymentRefresh
                {
                    LoanAccountId = loanAcoountId,
                    IsRunning = true,
                    IsError = false,
                    ErrorMsg = "",
                    IsActive = true,
                    IsDeleted = false
                };
                _context.LoanAccountRepaymentRefreshs.Add(loanAccoRepaymentRefresh);
                await _context.SaveChangesAsync();

                refresh = await _context.LoanAccountRepaymentRefreshs.Where(x => x.LoanAccountId == loanAcoountId && x.IsRunning && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            }
            else
            {
                if (refresh.IsRunning == false)
                {
                    refresh.IsRunning = true;
                    _context.Entry(refresh).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    res = "Error: This LoanAccountId Already Running";
                    return res;
                }
            }
            #endregion

            try
            {
                //Step 01: Save/Get Paid Payment Date
                #region Save/Get Paid Payment Date
                var LoanAccount_Id = new SqlParameter("LoanAccountId", loanAcoountId);
                var ActionType_value = new SqlParameter("ActionType", "Insert");
                //var Invoice_Date = new SqlParameter("InvoiceDate", invoiceDate == null ? DBNull.Value : (object)invoiceDate);
                var tmpAccountTransactionPaymentInfoList = await _context.Database.SqlQueryRaw<GetAccountTransactionPaymentInfoDC>("exec sp_tmpAccountTransactionPaymentInfo @ActionType,@LoanAccountId", ActionType_value, LoanAccount_Id).ToListAsync();
                #endregion


                //Step 02: Remove All Payment transaction of  loanAccountId
                long LoanAccountRepaymentID = 0;
                var loanRepayment = await RemovePaymentEntry_Utility(loanAcoountId, LoanAccountRepaymentID, paymentDATE);


                //Step 03: One by One Run Payment Entry.
                var loanAccountRepayment = await _context.LoanAccountRepayments.Where(x => x.LoanAccountId == loanAcoountId && x.Status != status && x.IsDeleted && !x.IsActive).OrderBy(x => x.PaymentDate).ToListAsync();
                foreach (var paymentTran in loanAccountRepayment)
                {
                    paymentTran.IsActive = true;
                    paymentTran.IsDeleted = false;
                    _context.Entry(paymentTran).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    //var SettlePay = await SettlePaymentLaterByID(paymentTran.Id);
                    var service = _loanNBFCFactory.GetService(CompanyIdentificationCodeConstants.BlackSoil);
                    await service.SettlePaymentLater(new GRPCRequest<SettlePaymentJobRequest>
                    {
                        Request = new SettlePaymentJobRequest
                        {
                            loanAccountRepaymentsId = paymentTran.Id,
                            IsRunningManually = true
                        }
                    });

                    //res = SettlePay + $"[{paymentTran.Id}], " + res;
                }


                ////Step 04: Change Transaction Status Pending ==>> Due/Ovedue.
                //var abc = await UpdateTransactionStatusByOptionJob("",loanAcoountId);


                //Step 05: Update Credit Limit.
                double creditLimitAmount = 0;
                var replyLimit = await _loanAccountHelper.GetAvailableCreditLimitByLoanId(loanAcoountId);
                if (replyLimit != null)
                {
                    creditLimitAmount = replyLimit.Response.CreditLimit;
                    var creditLimit = await _context.LoanAccountCredits.Where(x => x.LoanAccountId == loanAcoountId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    if (creditLimit != null && creditLimitAmount != creditLimit.CreditLimitAmount)
                    {
                        creditLimit.CreditLimitAmount = creditLimitAmount;
                        _context.Entry(creditLimit).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }

                #region Use For ReProcess Payment entry from Day one TO till today
                DateConvertHelper _DateConvertHelper = new DateConvertHelper();
                var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
                if (refresh != null)
                {
                    refresh.IsRunning = false;
                    refresh.LastRunningDate = currentDateTime;
                    _context.Entry(refresh).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                #endregion

            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();

                #region Use For ReProcess Payment entry from Day one TO till today
                refresh.IsRunning = false;
                refresh.IsError = true;
                refresh.ErrorMsg = res;

                _context.Entry(refresh).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                #endregion

                throw ex;
            }

            return res;
        }

        public async Task<string> SettlePaymentLaterByID(long LoanAccountRepaymentID)
        {
            //Hint: This Function Copy From BlackSoilLoanNBFCService.cs
            string res = "";
            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddMinutes(-15), INDIAN_ZONE);  //DateTime.Now.AddMinutes(-15);

            var list = _context.LoanAccountRepayments.Where(x => x.Status != BlackSoilRepaymentConstants.Settled && x.Id == LoanAccountRepaymentID && x.IsActive && !x.IsDeleted && x.Created < currentTime).OrderBy(x => x.PaymentDate).ToList();
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {

                    BlackSoilRepaymentDc blackSoilRepayment = null;
                    BlackSoilLoanAccountExpandDc accountDetails = null;
                    var api = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilLoanRepayment && x.IsActive && !x.IsDeleted);
                    var repayment = item;
                    //if (repayment != null)
                    //{
                    //    return new NBFCFactoryResponse
                    //    {
                    //        IsSuccess = false,
                    //        Message = $"Repayment {item.ThirdPartyPaymentId} already exist, either processed or somthing went wrong"
                    //    };
                    //}

                    if (api != null)
                    {
                        string url = api.APIUrl;
                        url = url.Replace("{{accountid}}", item.ThirdPartyLoanAccountId.ToString());
                        url = url.Replace("{{repaymentid}}", item.ThirdPartyPaymentId.ToString());
                        BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();

                        BlackSoilCommonAPIRequestResponse response = await blackSoilNBFCHelper.GetLoanRepayment(url, api.TAPIKey, api.TAPISecretKey, 0);
                        _context.BlackSoilCommonAPIRequestResponses.Add(response);
                        await _context.SaveChangesAsync();
                        if (response != null && response.IsSuccess)
                        {
                            blackSoilRepayment = JsonConvert.DeserializeObject<BlackSoilRepaymentDc>(response.Response);


                            if (item.Status == BlackSoilRepaymentConstants.Pending)
                            {
                                item.PaymentDate = blackSoilRepayment.settlement_date.Value;
                                item.BankRefNo = blackSoilRepayment.ref_no;

                                item.TotalAmount = Convert.ToDouble(blackSoilRepayment.actual_amount);
                                //item.RemainingTotalAmount = Convert.ToDouble(blackSoilRepayment.actual_amount);

                                item.InterestAmount = Convert.ToDouble(blackSoilRepayment.interest);

                                item.RemainingInterestAmount = Convert.ToDouble(blackSoilRepayment.interest);

                                item.ProcessingFees = Convert.ToDouble(blackSoilRepayment.pf);
                                item.RemainingProcessingFees = Convert.ToDouble(blackSoilRepayment.pf);

                                item.PenalInterest = Convert.ToDouble(blackSoilRepayment.penal_interest);
                                item.RemainingPenalInterest = Convert.ToDouble(blackSoilRepayment.penal_interest);

                                item.OverdueInterest = Convert.ToDouble(blackSoilRepayment.overdue_interest);
                                item.RemainingOverdueInterest = Convert.ToDouble(blackSoilRepayment.overdue_interest);


                                item.PrincipalAmount = Convert.ToDouble(blackSoilRepayment.principal);
                                item.RemainingPrincipalAmount = Convert.ToDouble(blackSoilRepayment.principal);


                                item.ExtraPaymentAmount = Convert.ToDouble(blackSoilRepayment.extra_payment);
                                item.RemainingExtraPaymentAmount = Convert.ToDouble(blackSoilRepayment.extra_payment);

                                _context.Entry(item).State = EntityState.Modified;
                                _context.SaveChanges();
                            }
                            if (blackSoilRepayment != null)
                            {
                                var loanAccountDetailAPI = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilLoanAccountDetail && x.IsActive && !x.IsDeleted);
                                if (loanAccountDetailAPI != null)
                                {
                                    url = loanAccountDetailAPI.APIUrl;
                                    url = url.Replace("{{accountid}}", item.ThirdPartyLoanAccountId.ToString());

                                    response = await blackSoilNBFCHelper.GetLoanAccountDetail(url, loanAccountDetailAPI.TAPIKey, loanAccountDetailAPI.TAPISecretKey, 0);
                                    _context.BlackSoilCommonAPIRequestResponses.Add(response);
                                    await _context.SaveChangesAsync();
                                    if (response != null && response.IsSuccess)
                                    {
                                        accountDetails = JsonConvert.DeserializeObject<BlackSoilLoanAccountExpandDc>(response.Response);

                                        var PendingTransactionsList = await GetOutstandingTransactionsList("", item.LoanAccountId, blackSoilRepayment.settlement_date);
                                        if (PendingTransactionsList != null && PendingTransactionsList.Any())
                                        {
                                            var data = await SettlePaymentTransaction(PendingTransactionsList, blackSoilRepayment, accountDetails, item, new GRPCRequest<SettlePaymentJobRequest>());
                                            res = "Done Payment";
                                        }

                                        //item.Status = BlackSoilRepaymentConstants.Settled;
                                        //_context.Entry(item).State = EntityState.Modified;
                                        //_context.SaveChanges();
                                    }

                                }
                                else
                                {

                                }


                            }
                        }

                    }
                    else
                    {

                    }




                }
            }

            return res;

        }

        //public async Task<string> UpdatePaidStatusForInvoice( string InvoiceNo)
        //{
        //    string res = "";

        //    #region Update Paid status for Invoice
        //    //var trasnStatues = new List<string> {
        //    //TransactionStatuseConstants.Initiate,
        //    //TransactionStatuseConstants.Failed,
        //    //TransactionStatuseConstants.Canceled
        //    //    };
        //    ////var transactionStatustxn = _context.TransactionStatuses.Where(x => trasnStatues.Contains(x.Code) && x.IsActive && !x.IsDeleted).ToList();

        //    var TransactionTypeId_OrderPlacement = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement);
        //    var TransactionStatusesId_Paid = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();


        //    var transactionStatustxn = _context.TransactionStatuses.Where(x => x.IsActive && !x.IsDeleted).ToList();
        //    var transactionStatus_initiate = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Initiate).FirstOrDefault();
        //    var transactionStatus_failed = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Failed).FirstOrDefault();
        //    var transactionStatus_canceled = transactionStatustxn.Where(x => x.Code == TransactionStatuseConstants.Canceled).FirstOrDefault();

        //    long invoiceId =  0;

        //    //var AcTransExist = await _context.AccountTransactions.Where(x => x.InvoiceId == invoiceId
        //    //                    && x.TransactionTypeId == TransactionTypeId_OrderPlacement.Id
        //    //                    && (x.TransactionStatusId != transactionStatus_initiate.Id || x.TransactionStatusId != transactionStatus_failed.Id || x.TransactionStatusId != transactionStatus_canceled.Id)
        //    //                    && x.IsActive && !x.IsDeleted).ToListAsync();
        //    var AcTransExist = await _context.AccountTransactions.Where(x => x.InvoiceNo == InvoiceNo
        //                        && x.TransactionTypeId == TransactionTypeId_OrderPlacement.Id
        //                        && (x.TransactionStatusId != transactionStatus_initiate.Id && x.TransactionStatusId != transactionStatus_failed.Id && x.TransactionStatusId != transactionStatus_canceled.Id)
        //                        && x.IsActive && !x.IsDeleted).ToListAsync();
        //    if (AcTransExist != null)
        //    {
        //        if (AcTransExist.Count() == AcTransExist.Count(x => x.TransactionStatusId == TransactionStatusesId_Paid.Id))
        //        {
        //            invoiceId = AcTransExist.Max(x=>x.InvoiceId ??0);

        //            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId
        //                                    && x.Status != AccountInvoiceStatus.Paid.ToString());
        //            if (invoice != null)
        //            {
        //                invoice.Status = AccountInvoiceStatus.Paid.ToString();
        //                _context.Entry(invoice).State = EntityState.Modified;
        //                await _context.SaveChangesAsync();

        //                res = "DOne";
        //            }
        //        }
        //    }
        //    #endregion

        //    return res;
        //}

        public async Task<string> UpdatePaidStatusByParentAccountTransactionId(long loanAccountId, DateTime PaymentDate, long ParentAccountTransactionId, string? InvoiceNo)
        {
            string res = "";

            var TransactionStatusesId_Paid = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();
            var TransactionTypes = await _context.TransactionTypes.Where(x => x.IsActive).ToListAsync();

            var TransactionTypeId_OrderPlacement = TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement);

            var TransactionTypeIdList = TransactionTypes.Where(x => x.Code == TransactionTypesConstants.OrderPlacement
            || x.Code == TransactionTypesConstants.OverdueInterest
            || x.Code == TransactionTypesConstants.PenaltyCharges
            || x.Code == TransactionTypesConstants.OrderPayment).Select(x => x.Id).ToList();

            var AccountTransactionsExist = await _context.AccountTransactions.Where(x => (x.Id == ParentAccountTransactionId || x.ParentAccountTransactionsID == ParentAccountTransactionId)
            && TransactionTypeIdList.Contains(x.TransactionTypeId)
            && x.IsActive && !x.IsDeleted).AsAsyncEnumerable().ToListAsync();

            var AccountTranExist = AccountTransactionsExist.FirstOrDefault(x => x.Id == ParentAccountTransactionId && x.TransactionTypeId == TransactionTypeId_OrderPlacement.Id && x.IsActive && !x.IsDeleted);


            //-----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //long LoanAccountId = AccountTransactionsExist.LoanAccountId;
            var PendingTransactionsList = await GetOutstandingTransactionsList("", loanAccountId, PaymentDate.Date);
            if (PendingTransactionsList != null && PendingTransactionsList.Any())
            {
                var transactions = PendingTransactionsList.Where(x => x.InvoiceNo == AccountTranExist.InvoiceNo).ToList();

                var OrderPrincipalAmount = Convert.ToDouble(transactions.Sum(x => x.PricipleAmount));
                var totalTxnPrinciplePaidAmount = Convert.ToDouble(transactions.Sum(x => x.PaymentAmount));

                var InterestAmountOfOrder = Convert.ToDouble(transactions.Sum(x => x.InterestAmount));
                var totalTxnInterestPaidAmount = Convert.ToDouble(transactions.Sum(x => x.InterestPaymentAmount));

                var PenaltyChargesAmount = Convert.ToDouble(transactions.Sum(x => x.PenaltyChargesAmount));
                var totalPenalPaymentAmount = Convert.ToDouble(transactions.Sum(x => x.PenalPaymentAmount));

                var OverdueInterestAmount = Convert.ToDouble(transactions.Sum(x => x.OverdueInterestAmount));
                var totalOverduePaymentAmount = Convert.ToDouble(transactions.Sum(x => x.OverduePaymentAmount));

                //if ((OrderPrincipalAmount == totalTxnPrinciplePaidAmount)
                if ((OrderPrincipalAmount - (totalTxnPrinciplePaidAmount)) < 0.10
                    //&& (Math.Abs((decimal)(InterestAmountOfOrder - (totalTxnInterestPaidAmount))) < 2)
                    //&& (Math.Abs((decimal)(PenaltyChargesAmount - (totalPenalPaymentAmount))) < 2)
                    //&& (Math.Abs((decimal)(OverdueInterestAmount - (totalOverduePaymentAmount))) < 2)
                    )
                {
                    foreach (var ACT in AccountTransactionsExist)
                    {
                        ACT.TransactionStatusId = TransactionStatusesId_Paid.Id;
                        _context.Entry(ACT).State = EntityState.Modified;
                    }
                    await _context.SaveChangesAsync();
                }
            }
            //----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

            return res;
        }

        public async Task<string> UpdatePaidStatusForAllLoanAccountId(long loanAccountId)
        {
            string res = "";

            var TransactionTypes = await _context.TransactionTypes.Where(x => x.IsActive).ToListAsync();
            var TransactionTypeIdList = TransactionTypes.Where(x => x.Code == TransactionTypesConstants.OrderPlacement
            || x.Code == TransactionTypesConstants.OverdueInterest
            || x.Code == TransactionTypesConstants.PenaltyCharges
            || x.Code == TransactionTypesConstants.OrderPayment).Select(x => x.Id).ToList();


            //-----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            //long LoanAccountId = AccountTransactionsExist.LoanAccountId;

            var GetAllAccountTransaction = new List<blackSoilAccountTransactionsDC>();
            var query = from t in _context.AccountTransactions //.Where(x => x.LoanAccountId == loanAccountId)
                        join tt in _context.TransactionTypes on t.TransactionTypeId equals tt.Id
                        join ts in _context.TransactionStatuses on t.TransactionStatusId equals ts.Id
                        join bb in _context.BlackSoilAccountTransactions on t.InvoiceId equals bb.LoanInvoiceId
                        where t.DisbursementDate != null
                        && tt.Code == TransactionTypesConstants.OrderPayment
                        && ts.Code != TransactionStatuseConstants.Paid
                        && t.IsActive && !t.IsDeleted && tt.IsActive && !tt.IsDeleted && ts.IsActive && !ts.IsDeleted
                        && t.LoanAccountId == loanAccountId
                        select new blackSoilAccountTransactionsDC
                        {
                            WithdrawalId = bb.WithdrawalId ?? 0,
                            AccountTransactionId = t.Id,
                            InvoiceNo = t.InvoiceNo,
                            StatusCode = ts.Code
                        };

            GetAllAccountTransaction = await query.ToListAsync();
            if (GetAllAccountTransaction != null && GetAllAccountTransaction.Count > 0)
            {
                ///var PendingTransactionsList = await GetOutstandingTransactionsList("", loanAccountId, PaymentDate.Date);
                List<GetOutstandingTransactionsDTO> PendingTransactionsList = new List<GetOutstandingTransactionsDTO>();
                var LoanAccount_Id = new SqlParameter("LoanAccountId", loanAccountId);
                PendingTransactionsList = await _context.Database.SqlQueryRaw<GetOutstandingTransactionsDTO>("exec GetAllTransactionsList @LoanAccountId", LoanAccount_Id).ToListAsync();


                foreach (var doc in GetAllAccountTransaction)
                {
                    string InvoiceNo = doc.InvoiceNo;

                    if (PendingTransactionsList != null && PendingTransactionsList.Any())
                    {
                        var transactions = PendingTransactionsList.Where(x => x.InvoiceNo == InvoiceNo).ToList();

                        var OrderPrincipalAmount = Convert.ToDouble(transactions.Sum(x => x.PricipleAmount));
                        var totalTxnPrinciplePaidAmount = Convert.ToDouble(transactions.Sum(x => x.PaymentAmount));

                        var InterestAmountOfOrder = Convert.ToDouble(transactions.Sum(x => x.InterestAmount));
                        var totalTxnInterestPaidAmount = Convert.ToDouble(transactions.Sum(x => x.InterestPaymentAmount));

                        var PenaltyChargesAmount = Convert.ToDouble(transactions.Sum(x => x.PenaltyChargesAmount));
                        var totalPenalPaymentAmount = Convert.ToDouble(transactions.Sum(x => x.PenalPaymentAmount));

                        var OverdueInterestAmount = Convert.ToDouble(transactions.Sum(x => x.OverdueInterestAmount));
                        var totalOverduePaymentAmount = Convert.ToDouble(transactions.Sum(x => x.OverduePaymentAmount));

                        if ((OrderPrincipalAmount == totalTxnPrinciplePaidAmount)
                            && (Math.Abs((decimal)(InterestAmountOfOrder - (totalTxnInterestPaidAmount))) < 2)
                            && (Math.Abs((decimal)(PenaltyChargesAmount - (totalPenalPaymentAmount))) < 2)
                            && (Math.Abs((decimal)(OverdueInterestAmount - (totalOverduePaymentAmount))) < 2)
                            )
                        {
                            var AccountTransactionsExist = _context.AccountTransactions.Where(x => x.InvoiceNo == InvoiceNo
                                        && TransactionTypeIdList.Contains(x.TransactionTypeId) && x.IsActive && !x.IsDeleted).ToList();
                            var TransactionStatusesId_Paid = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Paid).FirstOrDefault();


                            foreach (var ACT in AccountTransactionsExist)
                            {
                                ACT.TransactionStatusId = TransactionStatusesId_Paid.Id;
                                _context.Entry(ACT).State = EntityState.Modified;
                            }

                            //////For Invoice
                            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.InvoiceNo == InvoiceNo && x.Status != AccountInvoiceStatus.Paid.ToString());
                            if (invoice != null)
                            {
                                invoice.Status = AccountInvoiceStatus.Paid.ToString();
                                _context.Entry(invoice).State = EntityState.Modified;
                            }

                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            //----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

            return res;
        }

        public async Task<bool> UpdateTransactionStatusByOptionJob(string? invoiceno = "", long? loanAccountId = 0)
        {
            CommonResponse res = new CommonResponse();

            List<AccountTransaction> Transactions = new List<AccountTransaction>();

            var transactionStatuse = _context.TransactionStatuses.Where(x => x.IsActive).ToList();
            var transactionTypeIds = await _context.Database.SqlQueryRaw<long>($"SELECT  * FROM dbo.GetTransactionTypeId()").ToListAsync();


            var PendingStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Pending)?.Id;
            long DueStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Due).Id;
            long OverDueStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Overdue).Id;
            long DelinquentStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Delinquent).Id;
            long PaidStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Paid).Id;

            int days = 181;
            //DateTime currentdate = indianTime;
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
            DateTime currentdate = currentDateTime;

            //if (invoiceno == null)
            //{
            //    Transactions = await _context.AccountTransactions.Where(x => ((x.DueDate.Value.Date <= currentdate.Date && (x.TransactionStatusId == PendingStatusId || x.TransactionStatusId == DueStatusId))
            //   || (x.DueDate.Value.Date.AddDays(days) == currentdate.Date && (x.TransactionStatusId == OverDueStatusId))) && x.DisbursementDate.HasValue && x.TransactionStatusId != PaidStatusId && transactionTypeIds.Contains(x.TransactionTypeId) && x.IsActive && !x.IsDeleted).ToListAsync();
            //}
            //else
            //{
            //    Transactions = await _context.AccountTransactions.Where(x => x.InvoiceNo == invoiceno && ((x.DueDate.Value.Date <= currentdate.Date && (x.TransactionStatusId == PendingStatusId || x.TransactionStatusId == DueStatusId))
            //                   || (x.DueDate.Value.Date.AddDays(days) == currentdate.Date && (x.TransactionStatusId == OverDueStatusId))) && x.DisbursementDate.HasValue && x.TransactionStatusId != PaidStatusId && transactionTypeIds.Contains(x.TransactionTypeId) && x.IsActive && !x.IsDeleted).ToListAsync();
            //}
            Transactions = await _context.AccountTransactions.Where(x => (invoiceno == "" ? x.InvoiceNo != "" : x.InvoiceNo == invoiceno)
            && (loanAccountId == 0 ? x.LoanAccountId != 0 : x.LoanAccountId == loanAccountId)
            && ((x.DueDate.Value.Date <= currentdate.Date && (x.TransactionStatusId == PendingStatusId || x.TransactionStatusId == DueStatusId))
               || (x.DueDate.Value.Date.AddDays(days) == currentdate.Date && (x.TransactionStatusId == OverDueStatusId))) && x.DisbursementDate.HasValue && x.TransactionStatusId != PaidStatusId && transactionTypeIds.Contains(x.TransactionTypeId) && x.IsActive && !x.IsDeleted).ToListAsync();


            if (Transactions.Any())
            {
                foreach (var transaction in Transactions)
                {
                    if (transaction.DueDate.Value.Date == currentdate.Date)
                    {
                        transaction.TransactionStatusId = DueStatusId; //change status from pendig to due
                        transaction.LastModified = currentdate;
                    }
                    if (transaction.TransactionStatusId == DueStatusId && transaction.DueDate.Value.AddDays(1).Date <= currentdate.Date)
                    {
                        transaction.TransactionStatusId = OverDueStatusId;  //change status from due to overdue
                        transaction.LastModified = currentdate;
                    }
                    if (transaction.TransactionStatusId == OverDueStatusId && transaction.DueDate.Value.AddDays(days).Date <= currentdate.Date)
                    {
                        transaction.TransactionStatusId = DelinquentStatusId; //change status from overdue to delinquent
                        transaction.LastModified = currentdate;
                    }
                    _context.Entry(transaction).State = EntityState.Modified;
                }

                var invoiceIds = Transactions.Select(x => x.InvoiceId).Distinct().ToList();
                var invoices = await _context.Invoices.Where(x => invoiceIds.Contains(x.Id)).ToListAsync();
                foreach (var invoice in invoices)
                {
                    var transStatusId = Transactions.FirstOrDefault(x => x.InvoiceId == invoice.Id).TransactionStatusId;
                    if (DueStatusId == transStatusId)
                        invoice.Status = AccountInvoiceStatus.Due.ToString();
                    else if (OverDueStatusId == transStatusId)
                        invoice.Status = AccountInvoiceStatus.Overdue.ToString();
                    else if (DelinquentStatusId == transStatusId)
                        invoice.Status = AccountInvoiceStatus.Delinquent.ToString();

                    _context.Entry(invoice).State = EntityState.Modified;
                }

                if (_context.SaveChanges() > 0)
                {
                    res.status = true;
                    res.Message = "Success";
                }
                else
                {
                    res.status = false;
                    res.Message = "Failed";
                }
            }
            return res.status;
        }

    }

}
