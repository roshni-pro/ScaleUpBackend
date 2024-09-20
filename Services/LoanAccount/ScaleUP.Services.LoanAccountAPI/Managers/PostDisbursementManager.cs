using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.LoanAccountModels.Master;
using System.Security.Cryptography;
using ScaleUP.Services.LoanAccountAPI.Helpers;
using Microsoft.Data.SqlClient;
using ScaleUP.BuildingBlocks.EventBus.Messages.WebHook;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Services.LoanAccountModels.Transaction;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Global.Infrastructure.Constants.LoanAccount;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;

namespace ScaleUP.Services.LoanAccountAPI.Managers
{
    public class PostDisbursementManager
    {
        private readonly LoanAccountApplicationDbContext _context;
        private readonly IMassTransitService _massTransitService;
        private readonly LoanNBFCFactory _loanNBFCFactory;

        public PostDisbursementManager(LoanAccountApplicationDbContext context, IMassTransitService massTransitService, LoanNBFCFactory loanNBFCFactory)
        {
            _context = context;
            _massTransitService = massTransitService;
            _loanNBFCFactory = loanNBFCFactory;
        }

        public async Task<GRPCReply<long>> PostAccountDisbursement(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request)
        {
            //IDbContextTransaction dbContextTransaction = _context.Database.BeginTransaction();
            AccountTransactionManager objAccounttransManager = new AccountTransactionManager(_context, _loanNBFCFactory);
            long iLoanAccountId = 0;

            GRPCReply<long> gRPCReply = new GRPCReply<long>();

            bool bPayableBy = false;
            string PayableBY = request.Request.ProcessingFeePayableBy;

            var loanAccount = await _context.LoanAccounts.Where(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).Include(x => x.LoanAccountCompanyLeads).FirstOrDefaultAsync();

            if (loanAccount != null)
            {
                iLoanAccountId = loanAccount.Id;
                var loanAccountcredit = loanAccount.LoanAccountCredits;
                if (loanAccountcredit != null)
                {
                    DateTime DueDate;

                    string sCustomerUniqueCode = request.Request.CustomerUniqueCode;

                    string source = iLoanAccountId.ToString() + request.Request.LeadId.ToString();
                    string sReferenceId = request.Request.TransactionReqNo != "" ? request.Request.TransactionReqNo : GenerateReferenceNo();        ////////// ??????????????

                    long AnchorCompanyId = request.Request.AnchorCompanyId;
                    double CreditDays = Convert.ToInt64(request.Request.CreditDays ?? 0);
                    double DiscountAmount = request.Request.DiscountAmount ?? 0;
                    DueDate = DateTime.Now.AddDays(CreditDays);

                    long CompanyProductId = loanAccount.NBFCCompanyId;
                    double DisbursalAmount = loanAccountcredit.DisbursalAmount;
                    double ProcessingFeeRate = request.Request.ProcessingFeeRate ?? 0;
                    double GST = request.Request.GstRate ?? 0;

                    var ProcessingFee = request.Request.ProcessingFeeType == "Percentage" ? (DisbursalAmount * ProcessingFeeRate / 100.0) : ProcessingFeeRate;
                    var ProcessingFeeTax = Math.Round(((ProcessingFee - DiscountAmount) * GST / 100), 2);

                    var TransactionStatusesId = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Pending).FirstOrDefault();
                    long TransactionStatusId = TransactionStatusesId.Id;
                    double CreditLimit = 0;// (DisbursalAmount - (ProcessingFee + ProcessingFeeTax));

                    if (PayableBY == "Customer")
                    { bPayableBy = true; }

                    double PaidAmt = 0;

                    if (bPayableBy)
                    {
                        PaidAmt = (ProcessingFee + ProcessingFeeTax);
                    }

                    if (request.Request.DisbursementType == DisbursementTypeEnum.PFLessDisbursement.ToString() && bPayableBy)
                    {
                        CreditLimit = (DisbursalAmount - (ProcessingFee + ProcessingFeeTax));
                    }
                    else
                    {
                        CreditLimit = DisbursalAmount;
                    }

                    string GSTper = "GST" + Convert.ToString(GST);
                    //var res = objAccounttransManager.SaveTransactionDetailHead(GSTper);


                    LoanAccountTransactionRequest AccountTransaction = new LoanAccountTransactionRequest()
                    {
                        CustomerUniqueCode = sCustomerUniqueCode,
                        LoanAccountId = iLoanAccountId,
                        AnchorCompanyId = AnchorCompanyId,
                        ReferenceId = sReferenceId,
                        TransactionTypeCode = request.Request.TransactionTypeCode, // TransactionTypesConstants.Disbursement,

                        FeeAmount = ProcessingFee,
                        GSTAmount = ProcessingFeeTax,
                        TransactionAmount = DisbursalAmount,
                        OrderAmount = DisbursalAmount,
                        PaidAmount = PaidAmt,
                        ProcessingFeeType = request.Request.ProcessingFeeType ?? "",
                        ProcessingFeeRate = request.Request.ProcessingFeeRate ?? 0,
                        GstRate = request.Request.GstRate ?? 0,
                        InterestType = request.Request.ConvenienceFeeType,
                        InterestRate = request.Request.ConvenienceFeeRate ?? 0,
                        CreditDays = request.Request.CreditDays,
                        BounceCharge = request.Request.BounceCharge ?? 0,
                        DelayPenaltyRate = request.Request.DelayPenaltyRate ?? 0,
                        PayableBy = PayableBY,
                        PaymentRefNo = "",
                    };
                    GRPCRequest<LoanAccountTransactionRequest> requestAccountTransaction = new GRPCRequest<LoanAccountTransactionRequest> { Request = AccountTransaction };
                    var replyAccountTransaction = objAccounttransManager.SaveTransaction(requestAccountTransaction, CompanyProductId, null, TransactionStatusId, DueDate);


                    if (replyAccountTransaction.Result.Response > 0 && replyAccountTransaction.Result.Status == true)
                    {
                        List<LoanAccountTransactionDetailRequest> transactionDetails = new List<LoanAccountTransactionDetailRequest>();
                        long AccountTransactionId = replyAccountTransaction.Result.Response;

                        //initial entry
                        transactionDetails.Add(new LoanAccountTransactionDetailRequest()
                        {
                            AccountTransactionId = AccountTransactionId,
                            Amount = DisbursalAmount,
                            TransactionDetailHeadCode = TransactionDetailHeadsConstants.DisbursementAmount,
                            IsPayableBy = false, // bPayableBy,
                        });

                        ////ProcessingFee entry
                        //if (ProcessingFee > 0)
                        //{
                        //    transactionDetails.Add(new LoanAccountTransactionDetailRequest()
                        //    {
                        //        AccountTransactionId = AccountTransactionId,
                        //        Amount = ProcessingFee,
                        //        TransactionDetailHeadCode = TransactionDetailHeadsConstants.ProcessingFee,
                        //        IsPayableBy = bPayableBy,
                        //    });
                        //}

                        ////Discount entry
                        //if (DiscountAmount > 0)
                        //{
                        //    transactionDetails.Add(new LoanAccountTransactionDetailRequest()
                        //    {
                        //        AccountTransactionId = AccountTransactionId,
                        //        Amount = (-1) * DiscountAmount,
                        //        TransactionDetailHeadCode = TransactionDetailHeadsConstants.Discount,
                        //        IsPayableBy = bPayableBy,
                        //    });
                        //}

                        //// GST on ProcessingFee  mean Tax
                        //if (ProcessingFeeTax >= 0)
                        //{
                        //    transactionDetails.Add(new LoanAccountTransactionDetailRequest()
                        //    {
                        //        AccountTransactionId = AccountTransactionId,
                        //        Amount = (ProcessingFeeTax),
                        //        TransactionDetailHeadCode = GSTper,
                        //        IsPayableBy = bPayableBy,
                        //    });
                        //}


                        foreach (var item in transactionDetails)
                        {
                            GRPCRequest<LoanAccountTransactionDetailRequest> requestAccountTransactionDetail = new GRPCRequest<LoanAccountTransactionDetailRequest> { Request = item };
                            var replyAccountTransactionDetail = objAccounttransManager.SaveTransactionDetail(requestAccountTransactionDetail);

                        }

                        if (replyAccountTransaction.Id > 0)
                        {
                            loanAccountcredit.CreditLimitAmount = CreditLimit; //Loanamount;
                            _context.Entry(loanAccountcredit).State = EntityState.Modified;
                            _context.SaveChanges();

                            if (request.Request.BlackSoilPfCollection != null)
                            {
                                if (request.Request.BlackSoilPfCollection.total_processing_fee > 0)
                                {
                                    var res = PostDisbursementPF(replyAccountTransaction.Id, TransactionStatuseConstants.Pending, request.Request.BlackSoilPfCollection);
                                    var resPaid = PostDisbursementPF(res.Id, TransactionStatuseConstants.Paid, request.Request.BlackSoilPfCollection);
                                }
                            }

                            gRPCReply.Status = true;
                            gRPCReply.Message = "Success";
                            gRPCReply.Response = AccountTransactionId;
                        }
                        else
                        {

                            gRPCReply.Status = false;
                            gRPCReply.Message = "Fail";
                            gRPCReply.Response = 0;
                        }
                    }
                }
            }

            if (gRPCReply.Status)
            {
                AccountDisbursementEvent accountDisbursementEvent = new AccountDisbursementEvent
                {
                    AccountId = iLoanAccountId
                };
                await _massTransitService.Publish(accountDisbursementEvent);
            }
            return gRPCReply;
        }


        public string GetUnquieId(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = LoanAccountHelper.GetMd5Hash(md5Hash, source);
                return LoanAccountHelper.HashString(hash);
            }
        }
        public string GenerateReferenceNo()
        {
            var entityname = new SqlParameter("EntityName", "Transaction");
            var ReferenceNo = _context.Database.SqlQueryRaw<string>("exec GenerateReferenceNoForTrans @EntityName", entityname).AsEnumerable().FirstOrDefault();
            return ReferenceNo;
        }

        public async Task<long> PostDisbursementPF(long DisbursementAccountTransactionId, string transactionStatus, BlackSoilPfCollectionDc request)
        {
            List<AccountTransactionDetail> accountTransactionDetails = new List<AccountTransactionDetail>();
            var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();
            var TransactionTypeId_Disbursement = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.Disbursement);
            var TransactionTypeId_ProcessingFee = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.ProcessingFee);
            var TransactionTypeId_ProcessingFeePayment = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.ProcessingFeePayment);
            var TransactionStatusesId_Paid = _context.TransactionStatuses.Where(x => x.Code == transactionStatus).FirstOrDefault();

            long AccountTransactionID = 0;

            //var query = from t in _context.AccountTransactions.Where(x => x.Id == DisbursementAccountTransactionId && x.IsActive && !x.IsDeleted)
            //            join tt in _context.TransactionTypes on t.TransactionTypeId equals tt.Id
            //            where tt.Code == TransactionTypesConstants.Disbursement && t.IsActive && !t.IsDeleted
            //            select t;
            //AccountTransaction AccountTransactionsExist = await query.FirstOrDefaultAsync();
            AccountTransaction AccountTransactionsExist = _context.AccountTransactions.Where(x => x.Id == DisbursementAccountTransactionId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (AccountTransactionsExist != null)
            {
                string sReferenceId = GenerateReferenceNo();
                if (transactionStatus == TransactionStatuseConstants.Paid)
                {
                    AccountTransactionsExist.TransactionStatusId = TransactionStatusesId_Paid.Id;
                    _context.Entry(AccountTransactionsExist).State = EntityState.Modified;
                }

                AccountTransactionsExist = new AccountTransaction
                {
                    AnchorCompanyId = AccountTransactionsExist.AnchorCompanyId,
                    LoanAccountId = AccountTransactionsExist.LoanAccountId,
                    ReferenceId = transactionStatus == TransactionStatuseConstants.Paid ? AccountTransactionsExist.ReferenceId : sReferenceId,
                    CustomerUniqueCode = AccountTransactionsExist.CustomerUniqueCode,
                    TransactionTypeId = transactionStatus == TransactionStatuseConstants.Paid ? TransactionTypeId_ProcessingFeePayment.Id : TransactionTypeId_ProcessingFee.Id,
                    TransactionStatusId = TransactionStatusesId_Paid.Id,
                    IsActive = true,
                    IsDeleted = false,
                    CompanyProductId = AccountTransactionsExist.CompanyProductId,
                    TransactionAmount = request.total_processing_fee ?? 0,
                    OrderAmount = request.total_processing_fee ?? 0, // request.processing_fee ?? 0,
                    PaidAmount = 0,//TotalPaidAmount,
                    ProcessingFeeType = "",
                    ProcessingFeeRate = 0,
                    GstRate = AccountTransactionsExist.GstRate,
                    InterestType = AccountTransactionsExist.InterestType,
                    InterestRate = AccountTransactionsExist.InterestRate,
                    CreditDays = AccountTransactionsExist.CreditDays,
                    BounceCharge = AccountTransactionsExist.BounceCharge,
                    DelayPenaltyRate = AccountTransactionsExist.DelayPenaltyRate,
                    PayableBy = AccountTransactionsExist.PayableBy,
                    //ParentAccountTransactionsID = transactionStatus == TransactionStatuseConstants.Paid ? AccountTransactionsExist.Id : 0,
                    ParentAccountTransactionsID = AccountTransactionsExist.Id,
                    InvoiceId = AccountTransactionsExist.InvoiceId,
                    InvoiceDate = AccountTransactionsExist.InvoiceDate,
                    InvoiceNo = AccountTransactionsExist.InvoiceNo,
                    DueDate = AccountTransactionsExist.DueDate,
                    DisbursementDate = AccountTransactionsExist.DisbursementDate,
                };
                _context.AccountTransactions.Add(AccountTransactionsExist);
                _context.SaveChanges();
                AccountTransactionID = AccountTransactionsExist.Id;

                if (AccountTransactionID > 0 && request.processing_fee > 0)
                {
                    AccountTransactionDetail PFtransaction = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = transactionStatus == TransactionStatuseConstants.Paid ? (-1) * request.processing_fee ?? 0 : (1) * request.processing_fee ?? 0,
                        PaymentReqNo = "",
                        PaymentDate = null,
                        TransactionDate = null,
                        TransactionDetailHeadId = transactionStatus == TransactionStatuseConstants.Paid ? TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.ProcessingFeePaymentAmount).Id : TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.ProcessingFee).Id,
                        PaymentMode = "",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = AccountTransactionsExist.PayableBy == "Customer",
                        Status = "success"
                    };
                    accountTransactionDetails.Add(PFtransaction);
                }
                if (request.processing_fee_tax > 0)
                {
                    //double GST = AccountTransactionsExist.GstRate;
                    //string GSTper = "GST" + Convert.ToString(GST);

                    AccountTransactionDetail PFtaxTransaction = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = transactionStatus == TransactionStatuseConstants.Paid ? (-1) * request.processing_fee_tax ?? 0 : request.processing_fee_tax ?? 0,
                        PaymentReqNo = "",
                        PaymentDate = null,
                        TransactionDate = null,
                        TransactionDetailHeadId = transactionStatus == TransactionStatuseConstants.Paid ? TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.GSTPaymentAmount).Id : TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Gst).Id,
                        PaymentMode = "",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = AccountTransactionsExist.PayableBy == "Customer",
                        Status = "success"
                    };
                    accountTransactionDetails.Add(PFtaxTransaction);
                }

                _context.AccountTransactionDetails.AddRangeAsync(accountTransactionDetails);
                await _context.SaveChangesAsync();
            }

            return AccountTransactionID;
        }
    }
}
