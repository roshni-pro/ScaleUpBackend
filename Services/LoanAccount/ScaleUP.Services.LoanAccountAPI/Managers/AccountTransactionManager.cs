using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountModels.Transaction;
using ScaleUP.Services.LoanAccountModels.Master;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using Nito.AsyncEx;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using ScaleUP.Services.LoanAccountDTO.Loan;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using System.Collections.Immutable;
using ScaleUP.Services.LoanAccountDTO;
using static MassTransit.ValidationResultExtensions;
using MassTransit.Transports;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Global.Infrastructure.Constants;


namespace ScaleUP.Services.LoanAccountAPI.Managers
{
    public class AccountTransactionManager
    {
        private readonly LoanAccountApplicationDbContext _context;
        private readonly LoanNBFCFactory _loanNBFCFactory;
        public AccountTransactionManager(LoanAccountApplicationDbContext context, LoanNBFCFactory loanNBFCFactory)
        {
            _context = context;
            _loanNBFCFactory = loanNBFCFactory;
        }

        public async Task<GRPCReply<long>> SaveTransaction(GRPCRequest<LoanAccountTransactionRequest> request, long CompanyProductId, DateTime? SettlementDate, long TransactionStatusId, DateTime DueDate)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            var TransactionTypeId = _context.TransactionTypes.Where(x => x.Code == request.Request.TransactionTypeCode).FirstOrDefault();

            AccountTransaction accountTransaction = new AccountTransaction
            {
                AnchorCompanyId = request.Request.AnchorCompanyId,
                LoanAccountId = request.Request.LoanAccountId,
                ReferenceId = request.Request.ReferenceId,
                CustomerUniqueCode = request.Request.CustomerUniqueCode,
                TransactionTypeId = TransactionTypeId.Id,
                IsActive = true,
                IsDeleted = false,
                CompanyProductId = CompanyProductId,
                SettlementDate = SettlementDate,
                TransactionStatusId = TransactionStatusId,
                ParentAccountTransactionsID = request.Request.ParentAccountTransactionsID,
                DueDate = DueDate,
                Created = DateTime.Now,
                OrderAmount = request.Request.OrderAmount,
                //ConvenienceFeeAmount = request.Request.FeeAmount,
                InterestAmountCalculated = request.Request.FeeAmount,
                GSTAmount = request.Request.GSTAmount,
                PaidAmount = request.Request.PaidAmount,

                ProcessingFeeType = request.Request.ProcessingFeeType,
                ProcessingFeeRate = request.Request.ProcessingFeeRate,
                GstRate = request.Request.GstRate,
                //ConvenienceFeeType = request.Request.ConvenienceFeeType,
                InterestType = request.Request.InterestType,
                //ConvenienceFeeRate = request.Request.ConvenienceFeeRate,
                InterestRate = request.Request.InterestRate,
                CreditDays = request.Request.CreditDays,
                BounceCharge = request.Request.BounceCharge,
                DelayPenaltyRate = request.Request.DelayPenaltyRate,
                PayableBy = request.Request.PayableBy,
                //PaymentRefNo = request.Request.PaymentRefNo,
            };
            _context.AccountTransactions.Add(accountTransaction);
            _context.SaveChanges();
            if (accountTransaction.Id > 0)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
                gRPCReply.Response = accountTransaction.Id;
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Fail";
                gRPCReply.Response = 0;
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<long>> SaveTransactionDetail(GRPCRequest<LoanAccountTransactionDetailRequest> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            var TransactionTypeId = _context.TransactionDetailHeads.Where(x => x.Code == request.Request.TransactionDetailHeadCode).FirstOrDefault();

            AccountTransactionDetail accountTransactionDetail = new AccountTransactionDetail
            {
                AccountTransactionId = request.Request.AccountTransactionId,
                Amount = request.Request.Amount,
                TransactionDetailHeadId = TransactionTypeId.Id,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Now,
                IsPayableBy = request.Request.IsPayableBy,
                Status = "success",
            };
            _context.AccountTransactionDetails.Add(accountTransactionDetail);
            _context.SaveChanges();
            if (accountTransactionDetail.Id > 0)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
                gRPCReply.Response = accountTransactionDetail.Id;
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Fail";
                gRPCReply.Response = 0;
            }
            return gRPCReply;
        }

        public async Task<long> SaveTransactionDetailHead(string HeadCode)
        {
            long ids = 0;

            var TransactionTypeId = _context.TransactionDetailHeads.Where(x => x.Code == HeadCode).FirstOrDefault();
            if (TransactionTypeId == null)
            {
                TransactionDetailHead accountTransactionDetail = new TransactionDetailHead
                {
                    Code = HeadCode
                };
                _context.TransactionDetailHeads.Add(accountTransactionDetail);
                _context.SaveChanges();
            }

            return ids;
        }

        //public async Task<GRPCReply<long>> PostAccountDisbursement(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request)
        //{
        //    //IDbContextTransaction dbContextTransaction = _context.Database.BeginTransaction();

        //    GRPCReply<long> gRPCReply = new GRPCReply<long>();

        //    var loanAccount = _context.LoanAccounts.Where(x => x.LeadId == request.Request.LeadId).FirstOrDefault();
        //    if (loanAccount != null)
        //    {
        //        long iLoanAccountId = loanAccount.Id;
        //        var loanAccountcredit = _context.LoanAccountCredits.Where(x => x.LoanAccountId == iLoanAccountId).FirstOrDefault();
        //        if (loanAccountcredit != null)
        //        {
        //            DateTime DueDate;
        //            string sCustomerUniqueCode = "333"; ////////// ??????????????
        //            string sReferenceId = "444";        ////////// ??????????????

        //            long AnchorCompanyId = request.Request.AnchorCompanyConfig.CompanyId;
        //            double CreditDays = request.Request.AnchorCompanyConfig.CreditDays ?? 0;
        //            double DiscountAmount = request.Request.DiscountAmount ?? 0;
        //            DueDate = DateTime.Now.AddDays(CreditDays);

        //            long CompanyProductId = loanAccount.NBFCCompanyId;
        //            double DisbursalAmount = loanAccountcredit.DisbursalAmount;
        //            double ProcessingFeeRate = loanAccountcredit.ProcessingFeeRate;
        //            double GST = loanAccountcredit.GstRate;

        //            var ProcessingFee = (DisbursalAmount * ProcessingFeeRate / 100.0);
        //            var ProcessingFeeTax = Math.Round(((ProcessingFee - DiscountAmount) * GST / 100), 2);

        //            var finalProcessingfeeAmt = ProcessingFee + ProcessingFeeTax;
        //            double Loanamount = loanAccountcredit.CreditLimitAmount - finalProcessingfeeAmt;
        //            Loanamount += DiscountAmount;

        //            var TransactionStatusesId = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Pending).FirstOrDefault();
        //            long TransactionStatusId = TransactionStatusesId.Id;

        //            LoanAccountTransactionRequest AccountTransaction = new LoanAccountTransactionRequest()
        //            {
        //                CustomerUniqueCode = sCustomerUniqueCode,
        //                LoanAccountId = iLoanAccountId,
        //                AnchorCompanyId = AnchorCompanyId,
        //                ReferenceId = sReferenceId,
        //                TransactionTypeCode = request.Request.TransactionTypeCode, // TransactionTypesConstants.Disbursement,
        //                TransactionAmount = DisbursalAmount,
        //            };
        //            GRPCRequest<LoanAccountTransactionRequest> requestAccountTransaction = new GRPCRequest<LoanAccountTransactionRequest> { Request = AccountTransaction };
        //            var replyAccountTransaction = SaveTransaction(requestAccountTransaction, CompanyProductId, null, TransactionStatusId, DueDate);


        //            if (replyAccountTransaction.Result.Response > 0 && replyAccountTransaction.Result.Status == true)
        //            {
        //                List<LoanAccountTransactionDetailRequest> transactionDetails = new List<LoanAccountTransactionDetailRequest>();
        //                long AccountTransactionId = replyAccountTransaction.Result.Response;

        //                //initial entry
        //                transactionDetails.Add(new LoanAccountTransactionDetailRequest()
        //                {
        //                    AccountTransactionId = AccountTransactionId,
        //                    Amount = (DisbursalAmount - (ProcessingFee + ProcessingFeeTax)),
        //                    TransactionDetailHeadCode = TransactionDetailHeadsConstants.Initial,
        //                });

        //                //ProcessingFee entry
        //                if (ProcessingFee > 0)
        //                {
        //                    transactionDetails.Add(new LoanAccountTransactionDetailRequest()
        //                    {
        //                        AccountTransactionId = AccountTransactionId,
        //                        Amount = ProcessingFee,
        //                        TransactionDetailHeadCode = TransactionDetailHeadsConstants.ProcessingFee,
        //                    });
        //                }

        //                //Discount entry
        //                if (DiscountAmount > 0)
        //                {
        //                    transactionDetails.Add(new LoanAccountTransactionDetailRequest()
        //                    {
        //                        AccountTransactionId = AccountTransactionId,
        //                        Amount = (-1) * DiscountAmount,
        //                        TransactionDetailHeadCode = TransactionDetailHeadsConstants.ProcessingFeeDiscount,
        //                    });
        //                }

        //                // GST on ProcessingFee  mean Tax
        //                if (ProcessingFeeTax >= 0)
        //                {
        //                    transactionDetails.Add(new LoanAccountTransactionDetailRequest()
        //                    {
        //                        AccountTransactionId = AccountTransactionId,
        //                        Amount = (ProcessingFeeTax),
        //                        TransactionDetailHeadCode = TransactionDetailHeadsConstants.ProcessingFeeGST,
        //                    });
        //                }


        //                int rowChanged = 0;
        //                foreach (var item in transactionDetails)
        //                {
        //                    GRPCRequest<LoanAccountTransactionDetailRequest> requestAccountTransactionDetail = new GRPCRequest<LoanAccountTransactionDetailRequest> { Request = item };
        //                    var replyAccountTransactionDetail = SaveTransactionDetail(requestAccountTransactionDetail);

        //                    rowChanged++;
        //                }

        //                if (replyAccountTransaction.Id > 0)
        //                {
        //                    loanAccountcredit.CreditLimitAmount = Loanamount;
        //                    _context.Entry(loanAccountcredit).State = EntityState.Modified;
        //                    _context.SaveChanges();

        //                    //_context.SaveChanges();
        //                    //dbContextTransaction.Commit();

        //                    gRPCReply.Status = true;
        //                    gRPCReply.Message = "Success";
        //                    gRPCReply.Response = AccountTransactionId;
        //                }
        //                else
        //                {
        //                    //dbContextTransaction.Rollback();

        //                    gRPCReply.Status = false;
        //                    gRPCReply.Message = "Fail";
        //                    gRPCReply.Response = 0;
        //                }
        //            }
        //        }
        //    }

        //    return gRPCReply;
        //}

        public async Task<CommonResponse> GetAccountTransaction(TransactionFilterDTO obj)
        {
            CommonResponse result = new CommonResponse();

            List<AccountTransactionDTO> list = new List<AccountTransactionDTO>();

            var searchKeyward = new SqlParameter("SearchKeyward", obj.SearchKeyward);
            var status = new SqlParameter("Status", obj.TabType == "Partial" ? obj.TabType : obj.Status);
            var skip = new SqlParameter("skip", obj.Skip);
            var take = new SqlParameter("take", obj.Take);
            var cityname = new SqlParameter("CityName", obj.CityName);
            var AnchorId = new SqlParameter("AnchorId", obj.AnchorCompanyId);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var loanaccountid = new SqlParameter("LoanAccountId", obj.LoanAccountId);

            list = _context.Database.SqlQueryRaw<AccountTransactionDTO>("exec GetTransactionList @Status,@SearchKeyward,@skip,@take,@CityName,@AnchorId,@FromDate,@ToDate,@LoanAccountId", status, searchKeyward, skip, take, cityname, AnchorId, fromdate, toDate, loanaccountid).AsEnumerable().ToList();



            if (list.Any())
            {
                list.ForEach(x =>
                {
                    x.PayableAmount = obj.Status == "Partially Paid" ? x.ReceivedPayment : x.PayableAmount;
                });
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<AccountTransactionDTO>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;

        }
        public async Task<CommonResponse> GetTransactionDetailById(long AccountTransactionId)
        {
            CommonResponse result = new CommonResponse();

            List<TransactionDetailDTO> list = new List<TransactionDetailDTO>();

            var TransactionId = new SqlParameter("AccountTransactionId", AccountTransactionId);

            list = _context.Database.SqlQueryRaw<TransactionDetailDTO>("exec GetTransactionDetailById @AccountTransactionId", TransactionId).AsEnumerable().ToList();
            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<TransactionDetailDTO>();
                result.status = false;
                result.Message = "Not Found";
            }

            return result;

        }

        public async Task<CommonResponse> TransactionDetail(long AccountTransactionsID)
        {
            CommonResponse result = new CommonResponse();

            List<TransactionDetailDc> list = new List<TransactionDetailDc>();

            var RefId = new SqlParameter("AccountTransactionsID", AccountTransactionsID);

            list = _context.Database.SqlQueryRaw<TransactionDetailDc>("exec TransactionDetail @AccountTransactionsID", RefId).AsEnumerable().ToList();
            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<TransactionDetailDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;
        }

        public async Task<CommonResponse> GetPenaltyBounceCharges(string ReferenceId, string Penaltytype)
        {
            CommonResponse result = new CommonResponse();

            List<PenaltyBounceChargesDc> list = new List<PenaltyBounceChargesDc>();

            var txnid = new SqlParameter("TransactionId", ReferenceId);
            var type = new SqlParameter("Type", Penaltytype);

            list = _context.Database.SqlQueryRaw<PenaltyBounceChargesDc>("exec GetPenaltyChargesByTxnId @TransactionId,@Type", txnid, type).AsEnumerable().ToList();
            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<PenaltyBounceChargesDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;
        }
        public async Task<CommonResponse> GetCityNameList()
        {
            CommonResponse result = new CommonResponse();
            List<CityNameDc> list = new List<CityNameDc>();
            list = await _context.LoanAccounts.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new CityNameDc { CityName = x.CityName }).ToListAsync();
            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<TransactionDetailDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;
        }

        public async Task<CommonResponse> GetAnchorNameList()
        {
            CommonResponse result = new CommonResponse();
            List<AnchorNameDc> list1 = new List<AnchorNameDc>();
            var list = await _context.LoanAccounts.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new AnchorNameDc { AnchorId = x.AnchorCompanyId, AnchorName = x.AnchorName }).ToListAsync();
            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<TransactionDetailDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;
        }

        public async Task<CommonResponse> CustomerBlock(long LoanAccountId, string Comment, bool IsHideLimit, string username)
        {
            CommonResponse result = new CommonResponse();

            var loanAccountData = _context.LoanAccounts.Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == LoanAccountId).AsEnumerable().FirstOrDefault();
            if (loanAccountData != null)
            {

                loanAccountData.IsBlockComment = Comment;
                loanAccountData.IsBlock = true;
                loanAccountData.IsBlockHideLimit = IsHideLimit;

                _context.Entry(loanAccountData).State = EntityState.Modified;
                int rowChanged = _context.SaveChanges();

                if (rowChanged > 0)
                {
                    //TransactionSettlementByManualReplytDTO TransactionSettlementByManualReplytDTO = new TransactionSettlementByManualReplytDTO();
                    //var leadmaster_id = new SqlParameter("leadmasterid", LoanAccountId);
                    //var created_by = new SqlParameter("createdby", username);
                    //var created_date = new SqlParameter("createddate", DateTime.Now);
                    //var status = new SqlParameter("status", "Block");
                    //var comment = new SqlParameter("comment", Comment);
                    //var modified_date = new SqlParameter("modifieddate", null);
                    //var isactive = new SqlParameter("isactive", 1);
                    //var isdelete = new SqlParameter("isdelete", 0); 
                    //var modified_by = new SqlParameter("modifiedby", null);

                    //var res = _context.Database.SqlQueryRaw<string>("exec sp_updateCustomerBlockHistory  leadmasterid,status,createddate, modifieddate,comment,createdby,modifiedby,isactive,isdelete", leadmaster_id, status, created_date, modified_date, comment, created_by, modified_by, isactive, isdelete).AsEnumerable().FirstOrDefault();

                    result.status = true;
                    result.Message = "Success";
                }
                else
                {
                    result.status = false;
                    result.Message = "Failed";
                }

            }
            return result;
        }

        public async Task<CommonResponse> CustomerActiveInActive(long LoanAccountId, bool IsAccountActive)
        {
            CommonResponse result = new CommonResponse();

            var loanAccountData = _context.LoanAccounts.Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == LoanAccountId).AsEnumerable().FirstOrDefault();
            if (loanAccountData != null)
            {
                loanAccountData.IsAccountActive = IsAccountActive;
                _context.Entry(loanAccountData).State = EntityState.Modified;
                int rowChanged = _context.SaveChanges();

                if (rowChanged > 0)
                {
                    result.status = true;
                    result.Message = "Success";
                }
                else
                {
                    result.status = false;
                    result.Message = "Failed";
                }

            }
            return result;
        }


        public async Task<CommonResponse> GetAccountTransactionExport(TransactionFilterDTO obj)
        {
            CommonResponse result = new CommonResponse();

            List<AccountTransactionExport> list = new List<AccountTransactionExport>();

            var searchKeyward = new SqlParameter("SearchKeyward", obj.SearchKeyward);
            var status = new SqlParameter("Status", obj.TabType == "Partial" ? obj.TabType : obj.Status);
            var skip = new SqlParameter("skip", obj.Skip);
            var take = new SqlParameter("take", obj.Take);
            var cityname = new SqlParameter("CityName", obj.CityName);
            var AnchorId = new SqlParameter("AnchorId", obj.AnchorCompanyId);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var loanaccountid = new SqlParameter("LoanAccountId", obj.LoanAccountId);


            list = _context.Database.SqlQueryRaw<AccountTransactionExport>("exec GetTransactionListExport @Status,@SearchKeyward,@skip,@take,@CityName,@AnchorId,@FromDate,@ToDate,@LoanAccountId", status, searchKeyward, skip, take, cityname, AnchorId, fromdate, toDate, loanaccountid).AsEnumerable().ToList();

            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<AccountTransactionDTO>();
                result.status = false;
                result.Message = "Not Found";
            }

            return result;

        }
        public async Task<CommonResponse> GetLoanAccountListExport(LoanAccountListRequest obj)
        {
            CommonResponse result = new CommonResponse();

            var productType = new SqlParameter("ProductType", obj.ProductType);
            var status = new SqlParameter("Status", obj.Status);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var cityname = new SqlParameter("CityName", obj.CityName);
            var keyword = new SqlParameter("Keyward", obj.Keyword);
            var min = new SqlParameter("Min", obj.Min);
            var max = new SqlParameter("Max", obj.Max);
            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);
            var anchorId = new SqlParameter("AnchorId", obj.AnchorId);


            var list = _context.Database.SqlQueryRaw<LoanAccountListDc>(" exec GetLoanAccountListExport @ProductType,  @Status,@Fromdate,@ToDate,@CityName,@Keyward,@Min,@Max,@Skip,@Take,@AnchorId", productType, status, fromdate, toDate, cityname, keyword, min, max, skip, take, anchorId).AsEnumerable().ToList();
            if (list.Any())
            {
                foreach (var item in list)
                {
                    var IntransitPaidAmt = await _context.AccountTransactions.Where(x => x.LoanAccountId == item.LoanAccountId && x.IsActive && !x.IsDeleted && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement
                                               && (x.TransactionStatus.Code == TransactionStatuseConstants.Initiate || x.TransactionStatus.Code == TransactionStatuseConstants.Intransit)).SumAsync(x => x.TransactionAmount);

                    var loanfactory = _loanNBFCFactory.GetService(CompanyIdentificationCodeConstants.BlackSoil);
                    item.AvailableCreditLimit = await loanfactory.GetAvailableCreditLimit(item.LoanAccountId);

                    if (item.AvailableCreditLimit > 0 && IntransitPaidAmt > 0 && item.AvailableCreditLimit > IntransitPaidAmt)
                    {
                        item.AvailableCreditLimit -= IntransitPaidAmt;
                    }
                }
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<LoanAccountListDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;
        }
        public async Task<CustomerOrderSummaryDTO> GetCustomerOrderSummary(long AnchorCompanyId, long LeadId, string TransactionType = "All")
        {
            CustomerOrderSummaryDTO res = new CustomerOrderSummaryDTO();

            var LoanAccount = _context.LoanAccounts.Where(x => x.LeadId == LeadId).FirstOrDefault();
            var data = await GetLoanAccountSummary(AnchorCompanyId, LeadId);
            if (data != null)
            {
                CustomerTransactionInput customerTransactionInput = new CustomerTransactionInput
                {
                    LeadId = LeadId,
                    Skip = 0,
                    TransactionType = TransactionType,
                    AnchorCompanyID = AnchorCompanyId,
                    Take = 5
                };
                //var datalist = await GetCustomerTransactionList(customerTransactionInput);
                var PaymentOutstanding = await GetPaymentOutstanding(LoanAccount.Id, AnchorCompanyId);
                res.AvailableLimit = data.AvailableLimit.HasValue ? data.AvailableLimit.Value : 0;
                res.TotalOutStanding = data.UtilizedAmount.HasValue ? data.UtilizedAmount.Value : 0;
                // res.CustomerInvoice = datalist;
                res.CustomerName = LoanAccount.CustomerName;
                res.CustomerImage = LoanAccount.CustomerImage;
                res.TotalPendingInvoiceCount = PaymentOutstanding.TotalPendingInvoiceCount;
                res.TotalPayableAmount = PaymentOutstanding.TotalPayableAmount;
            }
            return res;
        }

        public async Task<List<CustomerInvoiceDTO>> GetCustomerTransactionList(CustomerTransactionInput customerTransactionInput)
        {
            List<CustomerInvoiceDTO> res = new List<CustomerInvoiceDTO>();

            var LeadId = new SqlParameter("LeadId", customerTransactionInput.LeadId);
            var AnchorCompanyID = new SqlParameter("AnchorCompanyID", customerTransactionInput.AnchorCompanyID);
            var skip = new SqlParameter("Skip", customerTransactionInput.Skip);
            var take = new SqlParameter("Take", customerTransactionInput.Take);
            var TransactionType = new SqlParameter("TransactionType", customerTransactionInput.TransactionType);

            var result = _context.Database.SqlQueryRaw<CustomerInvoiceDTO>("exec GetCustomerTransactionList  @LeadId,@AnchorCompanyID,@Skip,@Take,@TransactionType",
                          LeadId, AnchorCompanyID, skip, take, TransactionType).AsEnumerable().ToList();
            if (result != null)
            {
                res = result;
            }
            return res;
        }

        public async Task<PaymentOutstandingDc> GetPaymentOutstanding(long LoanAccountId, long AnchorCompanyID)
        {
            PaymentOutstandingDc res = new PaymentOutstandingDc();
            var loanAccountId = new SqlParameter("LoanAccountId", LoanAccountId);
            var anchorCompanyID = new SqlParameter("AnchorCompanyID", AnchorCompanyID);
            var result = _context.Database.SqlQueryRaw<PaymentOutstandingDc>("exec GetPaymentOutstanding  @LoanAccountId,@AnchorCompanyID",
                   loanAccountId, anchorCompanyID).AsEnumerable().FirstOrDefault();
            if (result != null)
            {
                res = result;
            }

            return res;
        }
        public async Task<TransactionBreakupDc> GetTransactionBreakup(long InvoiceId)
        {
            TransactionBreakupDc res = new TransactionBreakupDc();

            var Id = new SqlParameter("InvoiceId", InvoiceId);
            var result = _context.Database.SqlQueryRaw<TransactionBreakupPartDc>("exec GetCustomerTransactionBreakup  @InvoiceId", Id).AsEnumerable().ToList();
            if (result != null)
            {
                res.TransactionList = result;
                var TotalPayableAmt = result.Sum(x => x.Amount);
                res.TotalPayableAmount = TotalPayableAmt;
            }
            return res;
        }
        public async Task<List<CustomerInvoiceDTO>> GetCustomerTransactionListTwo(CustomerTransactionTwoInput customerTransactionTwoInput)
        {
            List<CustomerInvoiceDTO> res = new List<CustomerInvoiceDTO>();

            var LeadId = new SqlParameter("LeadId", customerTransactionTwoInput.LeadId);
            var skip = new SqlParameter("Skip", customerTransactionTwoInput.Skip);
            var take = new SqlParameter("Take", customerTransactionTwoInput.Take);

            var result = _context.Database.SqlQueryRaw<CustomerInvoiceDTO>("exec GetCustomerTransactionList_Two  @LeadId,@Skip,@Take",
                          LeadId, skip, take).AsEnumerable().ToList();
            if (result != null)
            {
                res = result;
            }
            return res;
        }

        public async Task<LoanAccountSummaryDc> GetLoanAccountSummary(long AnchorCompanyID, long LeadId)
        {
            LoanAccountSummaryDc res = new LoanAccountSummaryDc();

            var anchorCompanyID = new SqlParameter("AnchorCompanyID", AnchorCompanyID);
            var leadId = new SqlParameter("LeadId", LeadId);
            var result = _context.Database.SqlQueryRaw<LoanAccountSummaryDc>("exec GetLoanAccountSummary @AnchorCompanyID,@LeadId",
                   anchorCompanyID, leadId).AsEnumerable().FirstOrDefault();
            if (result != null)
            {
                res = result;
            }

            return res;
        }

        public async Task<CommonResponse> GetInvoiceDetail(TransactionFilterDTO obj)
        {
            CommonResponse result = new CommonResponse();

            List<InvoiceDetailListDc> list = new List<InvoiceDetailListDc>();

            var searchKeyward = new SqlParameter("SearchKeyward", obj.SearchKeyward);
            var status = new SqlParameter("Status", obj.TabType == "Partial" ? obj.TabType : obj.Status);
            var skip = new SqlParameter("skip", obj.Skip);
            var take = new SqlParameter("take", obj.Take);
            var cityname = new SqlParameter("CityName", obj.CityName);
            var AnchorId = new SqlParameter("AnchorId", obj.AnchorCompanyId);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var loanaccountid = new SqlParameter("LoanAccountId", obj.LoanAccountId);
            var isexport = new SqlParameter("IsExport", false);



            list = _context.Database.SqlQueryRaw<InvoiceDetailListDc>("exec GetInvoiceDetailList @Status,@SearchKeyward,@skip,@take,@CityName,@AnchorId,@FromDate,@ToDate,@LoanAccountId,@IsExport", status, searchKeyward, skip, take, cityname, AnchorId, fromdate, toDate, loanaccountid, isexport).AsEnumerable().ToList();

            if (list.Any())
            {

                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<InvoiceDetailListDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;

        }

        public async Task<CommonResponse> GetInvoiceDetailExport(TransactionFilterDTO obj)
        {
            CommonResponse result = new CommonResponse();

            List<InvoiceDetailListDc> list = new List<InvoiceDetailListDc>();

            var searchKeyward = new SqlParameter("SearchKeyward", obj.SearchKeyward);
            var status = new SqlParameter("Status", obj.TabType == "Partial" ? obj.TabType : obj.Status);
            var skip = new SqlParameter("skip", obj.Skip);
            var take = new SqlParameter("take", obj.Take);
            var cityname = new SqlParameter("CityName", obj.CityName);
            var AnchorId = new SqlParameter("AnchorId", obj.AnchorCompanyId);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var loanaccountid = new SqlParameter("LoanAccountId", obj.LoanAccountId);
            var isexport = new SqlParameter("IsExport", true);

            list = _context.Database.SqlQueryRaw<InvoiceDetailListDc>("exec GetInvoiceDetailList @Status,@SearchKeyward,@skip,@take,@CityName,@AnchorId,@FromDate,@ToDate,@LoanAccountId,@IsExport", status, searchKeyward, skip, take, cityname, AnchorId, fromdate, toDate, loanaccountid, isexport).AsEnumerable().ToList();

            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<InvoiceDetailListDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;

        }
        public async Task<CommonResponse> TransactionDetailByInvoiceId(long InvoiceId, string HeadType)
        {
            CommonResponse result = new CommonResponse();

            List<TransactionDetailByTransactionIdDc> list = new List<TransactionDetailByTransactionIdDc>();

            var invoiceid = new SqlParameter("InvoiceId", InvoiceId);
            var headtype = new SqlParameter("Head", HeadType);

            list = _context.Database.SqlQueryRaw<TransactionDetailByTransactionIdDc>("exec TransactionDetailByInvoiceId @InvoiceId,@Head ", invoiceid, headtype).AsEnumerable().ToList();

            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<TransactionDetailByTransactionIdDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;

        }
        public async Task<CommonResponse> InvoiceDetail(long InvoiceId)
        {
            CommonResponse result = new CommonResponse();

            List<InvoiceDetailDc> list = new List<InvoiceDetailDc>();

            var invoiceid = new SqlParameter("InvoiceId", InvoiceId);

            list = _context.Database.SqlQueryRaw<InvoiceDetailDc>("exec InvoiceDetail @InvoiceId", invoiceid).AsEnumerable().ToList();

            if (list.Any())
            {
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<InvoiceDetailDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;

        }
    }
}
