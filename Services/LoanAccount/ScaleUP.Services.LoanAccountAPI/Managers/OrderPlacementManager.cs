using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountModels.Master;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using ScaleUP.Services.LoanAccountAPI.Helpers;
using ScaleUP.Services.LoanAccountDTO.Loan;
using Microsoft.Data.SqlClient;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LoanAccountDTO;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Services.LoanAccountModels.Transaction;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using ScaleUP.Services.LoanAccountAPI.Constants;
using ScaleUP.Global.Infrastructure.Constants.AccountLocation;
using System.Transactions;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using MassTransit.Initializers;
using ScaleUP.Global.Infrastructure.Helper;
using System.Linq;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Services.LoanAccountModels.Master.NBFC;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using DocumentFormat.OpenXml.Drawing.Charts;
using IdentityServer4.Models;
using Microsoft.Identity.Client;

namespace ScaleUP.Services.LoanAccountAPI.Managers
{
    public class OrderPlacementManager
    {

        private readonly LoanAccountApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly LoanNBFCFactory _loanNBFCFactory;
        private readonly LoanAccountHelper _loanAccountHelper;

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public OrderPlacementManager(LoanAccountApplicationDbContext context,
            IConfiguration configuration, LoanNBFCFactory loanNBFCFactory, LoanAccountHelper loanAccountHelper)
        {
            _context = context;
            _configuration = configuration;
            _loanNBFCFactory = loanNBFCFactory;
            _loanAccountHelper = loanAccountHelper;
        }
        public async Task<GRPCReply<string>> PostOrderPlacement(GRPCRequest<OrderPlacementRequestDC> request)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            gRPCReply.Response = null;
            gRPCReply.Status = false;
            var loanAccount = await _context.LoanAccounts.Where(x => x.Id == request.Request.LoanAccountId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).Include(x => x.LoanAccountCompanyLeads).FirstOrDefaultAsync();
            var OrderDetail = request.Request;
            if (loanAccount != null)
            {
                var creditLimitResopnse = await _loanAccountHelper.GetAvailableCreditLimitByLoanId(loanAccount.Id, request.Request.Token);

                if (creditLimitResopnse.Response.IsBlock)
                {
                    gRPCReply.Message = creditLimitResopnse.Message;
                    return gRPCReply;
                }

                double CreditLimit = 0;
                double? TotalCreditLimit = 0;
                if (creditLimitResopnse != null && creditLimitResopnse.Status)
                {
                    CreditLimit = creditLimitResopnse.Response.CreditLimit;
                    TotalCreditLimit = creditLimitResopnse.Response.TotalCreditLimit;
                }

                if (CreditLimit < OrderDetail.Amount)
                {
                    gRPCReply.Message = "Your Credit Limit is not enough for this transaction";
                    return gRPCReply;
                }

                var paymentrequest = await _context.PaymentRequest.FirstOrDefaultAsync(x => x.TransactionReqNo == OrderDetail.TransactionReqNo && x.PaymentStatus == "Pending"
                                                                             && x.IsActive && !x.IsDeleted);
                if (paymentrequest != null)
                {
                    var typeOrderPlaceId = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement).Id;
                    var TransactionStatuses = await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.Code == TransactionStatuseConstants.Initiate);
                    var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();


                    string customerUniqueCode = loanAccount.LoanAccountCompanyLeads != null && loanAccount.LoanAccountCompanyLeads.Any(x => x.CompanyId == paymentrequest.AnchorCompanyId) ? (loanAccount.LoanAccountCompanyLeads.FirstOrDefault(x => x.CompanyId == paymentrequest.AnchorCompanyId).UserUniqueCode) : "";

                    bool PayableBy = false;
                    string sPayableBY = request.Request.InterestPayableBy;
                    PayableBy = sPayableBY == "Customer";

                    var result = _loanNBFCFactory.GetService(loanAccount.NBFCIdentificationCode);
                    double InterestRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(request.Request.InterestRate));
                    var perDayInterestAmount = request.Request.InterestType == "Percentage" ? (paymentrequest.TransactionAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                    double GST = request.Request.GstRate ?? 0;
                    string GSTper = "GST" + Convert.ToString(GST);
                    double GST_ConvenionFee = 0;

                    double PaidAmount = 0;
                    if (PayableBy == true)
                    {
                        PaidAmount = paymentrequest.TransactionAmount + (perDayInterestAmount * request.Request.CreditDay) + GST_ConvenionFee;
                    }
                    else
                    {
                        PaidAmount = paymentrequest.TransactionAmount;
                    }

                    AccountTransaction accountTransaction = new AccountTransaction
                    {
                        AnchorCompanyId = request.Request.AnchorCompanyId,
                        LoanAccountId = request.Request.LoanAccountId,
                        ReferenceId = paymentrequest.TransactionReqNo,
                        CustomerUniqueCode = customerUniqueCode,
                        TransactionTypeId = typeOrderPlaceId,
                        IsActive = true,
                        IsDeleted = false,
                        CompanyProductId = loanAccount.ProductId,
                        TransactionStatusId = TransactionStatuses.Id,
                        OrderAmount = paymentrequest.OrderAmount,
                        TransactionAmount = paymentrequest.TransactionAmount,
                        PaidAmount = PaidAmount,
                        ProcessingFeeType = "",
                        ProcessingFeeRate = 0,
                        GstRate = request.Request.GstRate ?? 0,
                        InterestType = request.Request.InterestType,
                        InterestRate = request.Request.InterestRate ?? 0,
                        CreditDays = request.Request.CreditDay,
                        BounceCharge = request.Request.BounceCharge ?? 0,
                        DelayPenaltyRate = request.Request.DelayPenaltyRate ?? 0,
                        PayableBy = sPayableBY,
                        OrderDate = indianTime
                    };

                    List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();

                    //order
                    transactionDetails.Add(new AccountTransactionDetail()
                    {
                        IsActive = true,
                        IsDeleted = false,
                        Amount = paymentrequest.TransactionAmount,
                        TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Order).Id,
                        IsPayableBy = true, // PayableBy,
                        Status = "initiate",
                        TransactionDate = indianTime
                    });

                    //GST
                    if (GST_ConvenionFee > 0)
                    {
                        transactionDetails.Add(new AccountTransactionDetail()
                        {
                            IsActive = true,
                            IsDeleted = false,
                            Amount = GST_ConvenionFee,
                            TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == GSTper).Id,
                            IsPayableBy = PayableBy,
                            Status = "initiate"
                        });
                    }

                    accountTransaction.AccountTransactionDetails = transactionDetails;

                    var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.OrderNo == paymentrequest.OrderNo && x.IsActive && !x.IsDeleted);

                    if (invoice == null)
                    {
                        invoice = new Invoice
                        {
                            IsActive = true,
                            IsDeleted = false,
                            LoanAccountId = loanAccount.Id,
                            OrderAmount = paymentrequest.OrderAmount,
                            OrderNo = paymentrequest.OrderNo,
                            TotalTransAmount = 0,
                            InvoiceAmount = 0,
                            Comment = "Order Initiate",
                            Status = AccountInvoiceStatus.Initiate.ToString(),
                            AccountTransactions = new List<AccountTransaction>()
                        };
                        invoice.AccountTransactions.Add(accountTransaction);
                        _context.Invoices.Add(invoice);
                    }
                    else
                    {
                        if (invoice.AccountTransactions == null)
                            invoice.AccountTransactions = new List<AccountTransaction>();
                        invoice.AccountTransactions.Add(accountTransaction);
                        _context.Entry(invoice).State = EntityState.Modified;
                    }

                    if (_context.SaveChanges() > 0)
                    {

                        if (!string.IsNullOrEmpty(loanAccount.NBFCIdentificationCode))
                        {
                            var nbfcFactory = _loanNBFCFactory.GetService(loanAccount.NBFCIdentificationCode);
                            if (loanAccount.NBFCIdentificationCode == LeadNBFCConstants.AyeFinanceSCF)
                            {
                                var nbfcresponse = await nbfcFactory.AyeSCFCOrderInitiate( new GRPCRequest<AyeSCFCOrderInitiateDTO> { Request = new AyeSCFCOrderInitiateDTO
                                {
                                    invoiceId = invoice.Id,
                                    accountId = request.Request.LoanAccountId,
                                    OrderAmount = paymentrequest.OrderAmount,
                                    OrderNo = paymentrequest.OrderNo,
                                    Token = request.Request.Token,
                                    AvailableLimit = CreditLimit,
                                    TotalLimit = TotalCreditLimit??0
                                } });
                                if (nbfcresponse.IsSuccess)
                                {
                                    gRPCReply.Status = nbfcresponse.IsSuccess;
                                    gRPCReply.Message = "Success";
                                    gRPCReply.Response = accountTransaction.ReferenceId;
                                }
                                else
                                {

                                    var FailTransactionStatusesId = (await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.Code == TransactionStatuseConstants.Failed))?.Id;
                                    if (FailTransactionStatusesId != null)
                                    {

                                        accountTransaction.TransactionStatusId = FailTransactionStatusesId.Value;
                                        accountTransaction.AccountTransactionDetails.ToList().ForEach(x => x.Status = "Failed");
                                        _context.Entry(accountTransaction).State = EntityState.Modified;


                                        paymentrequest.PaymentStatus = "Failed";
                                        _context.Entry(paymentrequest).State = EntityState.Modified;
                                    }

                                    gRPCReply.Message = nbfcresponse.Message;
                                }

                            }
                            else if (!await nbfcFactory.IsInvoiceInitiated(invoice.Id))
                            {
                                var nbfcresponse = await nbfcFactory.OrderInitiate(invoice.Id, request.Request.LoanAccountId, PaidAmount);

                                if (nbfcresponse.IsSuccess)
                                {
                                    gRPCReply.Status = true;
                                    gRPCReply.Message = "Success";
                                    gRPCReply.Response = accountTransaction.ReferenceId;
                                }
                                else
                                {

                                    var FailTransactionStatusesId = (await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.Code == TransactionStatuseConstants.Failed))?.Id;
                                    if (FailTransactionStatusesId != null)
                                    {

                                        accountTransaction.TransactionStatusId = FailTransactionStatusesId.Value;
                                        accountTransaction.AccountTransactionDetails.ToList().ForEach(x => x.Status = "Failed");
                                        _context.Entry(accountTransaction).State = EntityState.Modified;


                                        paymentrequest.PaymentStatus = "Failed";
                                        _context.Entry(paymentrequest).State = EntityState.Modified;
                                    }

                                    gRPCReply.Message = nbfcresponse.Message;
                                }
                            }
                            else
                            {
                                gRPCReply.Status = true;
                            }

                        }
                        else
                        {
                            gRPCReply.Message = "NBFC company not found";

                        }
                    }
                    else
                    {
                        gRPCReply.Message = "Some error occurred. please try after some time.";

                    }
                }
                else
                {
                    gRPCReply.Message = "Given transaction not found.";
                }
            }
            else
            {
                gRPCReply.Message = "Loan account is not available";

            }

            return gRPCReply;

        }
        public async Task<GRPCReply<OrderInitiateResponse>> OrderInitiate(OrderPlacementWithOtp request)
        {
            GRPCReply<OrderInitiateResponse> gRPCReply = new GRPCReply<OrderInitiateResponse>();
            string RedirectUrl = EnvironmentConstants.ScaleupWebViewUrl;
            List<PaymentRequest> paymentrequests = null;

            var loanAccount = await _context.LoanAccounts.Where(x => x.Id == request.LoanAccountId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).FirstOrDefaultAsync();

            if (loanAccount != null)
            {
                var AyeNBFCToken = request.Token;
                var creditLimitResopnse = await _loanAccountHelper.GetAvailableCreditLimitByLoanId(loanAccount.Id, AyeNBFCToken);

                if (creditLimitResopnse.Response.IsBlock)
                {
                    gRPCReply.Message = creditLimitResopnse.Message;
                    gRPCReply.Status = false;
                    return gRPCReply;
                }

                double CreditLimit = 0;
                if (creditLimitResopnse != null && creditLimitResopnse.Status)
                {
                    CreditLimit = creditLimitResopnse.Response.CreditLimit;
                }

                if (CreditLimit < request.TransactionAmount)
                {
                    gRPCReply.Response = null;
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Your Credit Limit is not enough for this transaction";
                    return gRPCReply;
                }

                paymentrequests = _context.PaymentRequest.Where(x => x.OrderNo == request.OrderNo && x.PaymentStatus != "Complete" && x.PaymentStatus != "Failed"
                                                                             && x.AnchorCompanyId == request.AnchorCompanyId && x.IsActive && !x.IsDeleted).ToList();


                if (paymentrequests != null)
                {
                    foreach (var paymentrequest in paymentrequests)
                    {
                        paymentrequest.PaymentStatus = "Failed";
                        _context.Entry(paymentrequest).State = EntityState.Modified;
                    }
                    //if (paymentrequest.TransactionAmount != request.TransactionAmount)
                    //{
                    //    gRPCReply.Response = null;
                    //    gRPCReply.Status = false;
                    //    gRPCReply.Message = "Your one transaction is in process.";
                    //    return gRPCReply;
                    //}

                    //OrderInitiateResponse res = new OrderInitiateResponse();
                    //res.RedirectUrl = RedirectUrl + "/#/order-processing/OrderDetailsConfirmOtp/" + paymentrequest.TransactionReqNo;
                    //res.OtptxnNo = paymentrequest.TransactionReqNo;
                    //res.MobileNo = loanAccount.MobileNo;
                    //gRPCReply.Response = res;
                    //gRPCReply.Status = true;
                    //gRPCReply.Message = "success";
                    //return gRPCReply;
                }
                //else
                //{
                PaymentRequest payment = new PaymentRequest
                {
                    AnchorCompanyId = request.AnchorCompanyId,
                    LoanAccountId = request.LoanAccountId,
                    OrderNo = request.OrderNo,
                    PaymentStatus = "Pending",
                    TransactionAmount = request.TransactionAmount,
                    TransactionReqNo = GenerateReferenceNo(),
                    IsActive = true,
                    IsDeleted = false,
                    Comment = "Order Place",
                    OrderAmount = request.OrderAmount
                };
                await _context.PaymentRequest.AddAsync(payment);


                if (_context.SaveChanges() > 0)
                {
                    OrderInitiateResponse res = new OrderInitiateResponse();
                    res.RedirectUrl = RedirectUrl + "/#/order-processing/OrderTransactionConfirmPin/" + payment.TransactionReqNo;
                    res.OtptxnNo = payment.TransactionReqNo;
                    res.MobileNo = loanAccount.MobileNo;
                    res.AnchorName = loanAccount.AnchorName;
                    res.CustomerName = loanAccount.CustomerName;
                    gRPCReply.Response = res;
                    gRPCReply.Status = true;
                    gRPCReply.Message = "success";

                }
                return gRPCReply;
                //}
            }
            else
            {
                gRPCReply.Response = null;
                gRPCReply.Status = false;
                gRPCReply.Message = "Loan account not available";
                return gRPCReply;
            }

        }
        public async Task<GRPCReply<PaymentResponseDc>> GetByTransactionReqNo(GRPCRequest<GetPaymentReqByTxnNo> request)
        {
            GRPCReply<PaymentResponseDc> result = new GRPCReply<PaymentResponseDc>();
            if (request.Request != null)
            {
                PaymentResponseDc res = new PaymentResponseDc();

                double GstRate = 0;
                double ConvenienceFeeRate = 0;
                if (request.Request.AnnualInterestPayableBy == "Customer")
                {
                    GstRate = request.Request.GstRate;
                }

                var data = await _context.PaymentRequest.Where(x => x.TransactionReqNo == request.Request.TransactionReqNo && x.PaymentStatus == "Pending" && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                if (data != null)
                {
                    var NBFCIdentificationCode = _context.LoanAccounts.FirstOrDefault(x => x.Id == data.LoanAccountId).NBFCIdentificationCode;
                    var resService = _loanNBFCFactory.GetService(NBFCIdentificationCode);
                    double InterestRatePerDay = await resService.CalculatePerDayInterest(request.Request.AnnualInterestRate);

                    double TransactionAmount = data.TransactionAmount;
                    double ConvFee = ConvenienceFeeRate > 0 ? Math.Round((TransactionAmount * ConvenienceFeeRate) / 100, 2) : 0;
                    double ConvFeeGst = GstRate > 0 ? Math.Round((ConvFee * GstRate) / 100, 2) : 0;
                    double finalAmount = TransactionAmount + ConvFee + ConvFeeGst;

                    res.TransactionAmount = data.TransactionAmount;
                    res.GSTConvenionFee = ConvFeeGst;
                    res.ConvenionFee = ConvFee;
                    res.TotalAmount = finalAmount;
                    res.TransactionReqNo = request.Request.TransactionReqNo;
                    res.OrderNo = data.OrderNo;
                    res.LoanAccountId = data.LoanAccountId;
                    res.InterestRate = InterestRatePerDay;

                    result.Message = "success";
                    result.Response = res;
                }
                else
                {
                    result.Status = false;
                    result.Message = "Transaction not found.";
                }
            }
            return result;

        }

        public async Task<GRPCReply<double>> GetTransactionInterestRate(GRPCRequest<GetPaymentReqByTxnNo> transNoRequest)
        {
            GRPCReply<double> gRPCReply = new GRPCReply<double>();
            var data = await _context.PaymentRequest.FirstOrDefaultAsync(x => x.TransactionReqNo == transNoRequest.Request.TransactionReqNo && x.PaymentStatus == "Pending" && x.IsActive && !x.IsDeleted);
            if (data != null)
            {
                var NBFCIdentificationCode = _context.LoanAccounts.FirstOrDefault(x => x.Id == data.LoanAccountId)?.NBFCIdentificationCode;
                if (!string.IsNullOrEmpty(NBFCIdentificationCode))
                {
                    var resService = _loanNBFCFactory.GetService(NBFCIdentificationCode);
                    double InterestRatePerDay = await resService.CalculatePerDayInterest(transNoRequest.Request.AnnualInterestRate);
                    gRPCReply.Response = InterestRatePerDay;
                    gRPCReply.Status = true;
                }
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> GetTransactionMobileNo(TranMobileRequest tranMobileRequest)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            var loanAccountId = (long?)null;
            if (tranMobileRequest.Type == "TransactionNo")
            {
                loanAccountId = _context.PaymentRequest.FirstOrDefault(x => x.TransactionReqNo == tranMobileRequest.Condition && x.IsActive)?.LoanAccountId;
            }
            else if (tranMobileRequest.Type == "OrderNo")
            {
                loanAccountId = _context.Invoices.FirstOrDefault(x => x.OrderNo == tranMobileRequest.Condition && x.IsActive)?.LoanAccountId;
            }

            if (loanAccountId.HasValue)
            {
                gRPCReply.Response = _context.LoanAccounts.FirstOrDefault(x => x.Id == loanAccountId.Value).MobileNo;
                gRPCReply.Status = true;
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<LeadProductDc>> GetLoanAccountDetailByTxnId(GRPCRequest<string> request)
        {
            var txn = new SqlParameter("TransactionReqNo", request.Request);
            var result = _context.Database.SqlQueryRaw<LeadProductDc>("exec GetLoanAccountDetailByTxnId @TransactionReqNo", txn).AsEnumerable().FirstOrDefault();

            if (result != null)
            {
                double CreditLimit = 0;
                var invoiceId = _context.Invoices.FirstOrDefault(x => x.OrderNo == result.OrderNo)?.Id;

                if (invoiceId.HasValue)
                    result.creditDay = _context.AccountTransactions.FirstOrDefault(x => x.InvoiceId == invoiceId && x.IsActive && !x.IsDeleted && x.TransactionStatus.Code == TransactionStatuseConstants.Intransit)?.CreditDays;

                //var IntransitPaidAmt = _context.AccountTransactions.Where(x => x.LoanAccountId == result.LoanAccountId && x.IsActive && !x.IsDeleted && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement
                //                         && (x.TransactionStatus.Code == TransactionStatuseConstants.Initiate || x.TransactionStatus.Code == TransactionStatuseConstants.Intransit)).Sum(x => x.PaidAmount);

                //var loanAccount = _context.LoanAccounts.FirstOrDefault(x => x.Id == result.LoanAccountId);
                //if (loanAccount.IsDefaultNBFC)
                //{
                //    CreditLimit = result.CreditLimitAmount;
                //}
                //else
                //{
                //    var loanfactory = _loanNBFCFactory.GetService(loanAccount.NBFCIdentificationCode);
                //    CreditLimit = await loanfactory.GetAvailableCreditLimit(loanAccount.Id);

                //}
                //if (CreditLimit > 0 && CreditLimit > IntransitPaidAmt)
                //{
                //    if (IntransitPaidAmt > 0)
                //        CreditLimit -= IntransitPaidAmt;
                //}
                //else
                //{
                //    CreditLimit = 0;
                //}

                var res = await _loanAccountHelper.GetAvailableCreditLimitByLoanId(result.LoanAccountId, "");

                result.CreditLimitAmount = res.Response.CreditLimit;
            }
            GRPCReply<LeadProductDc> reply = new GRPCReply<LeadProductDc>();
            reply.Response = result;
            return reply;
        }


        public async Task<GRPCReply<LeadProductDc>> GetNBFCLoanAccountDetailByTxnId(GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO> request)
        {
            var txn = new SqlParameter("TransactionReqNo", request.Request.TransactionReqNo);
            var result = _context.Database.SqlQueryRaw<LeadProductDc>("exec GetLoanAccountDetailByTxnId @TransactionReqNo", txn).AsEnumerable().FirstOrDefault();

            if (result != null)
            {
                double CreditLimit = 0;
                var invoiceId = _context.Invoices.FirstOrDefault(x => x.OrderNo == result.OrderNo)?.Id;

                if (invoiceId.HasValue)
                    result.creditDay = _context.AccountTransactions.FirstOrDefault(x => x.InvoiceId == invoiceId && x.IsActive && !x.IsDeleted && x.TransactionStatus.Code == TransactionStatuseConstants.Intransit)?.CreditDays;

                var res = await _loanAccountHelper.GetAvailableCreditLimitByLoanId(result.LoanAccountId, request.Request.NBFCToken);

                result.CreditLimitAmount = res.Response.CreditLimit;
            }
            GRPCReply<LeadProductDc> reply = new GRPCReply<LeadProductDc>();
            reply.Response = result;
            return reply;
        }
        public string GenerateReferenceNo()
        {
            var entityname = new SqlParameter("EntityName", "Transaction");
            var ReferenceNo = _context.Database.SqlQueryRaw<string>("exec GenerateReferenceNoForTrans @EntityName", entityname).AsEnumerable().FirstOrDefault();
            return ReferenceNo;
        }

        public async Task<CommonResponse> AnchorOrderCompleted(string transactionId, bool transStatus)
        {
            CommonResponse commonResponse = new CommonResponse();

            commonResponse.status = false;
            commonResponse.Message = "failed";
            var transStatusDb = _context.TransactionStatuses.Where(x => x.IsActive).ToList();
            var TransactionStatusId_Initiate = transStatusDb.Where(x => x.Code == TransactionStatuseConstants.Initiate).FirstOrDefault();


            var accountTreansaction = await _context.AccountTransactions.Where(x => x.ReferenceId == transactionId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (accountTreansaction == null)
            {
                commonResponse.status = false;
                commonResponse.Message = "failed";
                commonResponse.Result = "Transaction not exist";
            }
            else
            {
                if (accountTreansaction.TransactionStatusId != TransactionStatusId_Initiate.Id)
                {
                    commonResponse.status = false;
                    commonResponse.Message = "failed";
                    commonResponse.Result = "Transaction status should be Initiate";
                }
                else
                {
                    var paymentrequest = _context.PaymentRequest.Where(x => x.TransactionReqNo == transactionId && x.PaymentStatus == "Pending" && x.IsActive && !x.IsDeleted).FirstOrDefault();

                    var loanAccount = await _context.LoanAccounts.Where(x => x.Id == paymentrequest.LoanAccountId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).FirstOrDefaultAsync();

                    var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.OrderNo == paymentrequest.OrderNo && x.IsActive && !x.IsDeleted);

                    if (transStatus)
                    {
                        var TransactionStatusId = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Intransit).FirstOrDefault();

                        var transDetails = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == accountTreansaction.Id && x.Status == "Initiate" && x.IsActive && !x.IsDeleted).ToList();

                        if (transDetails != null && transDetails.Any())
                        {
                            foreach (var item in transDetails)
                            {
                                item.Status = "success";
                                _context.Entry(item).State = EntityState.Modified;
                            }
                        }

                        double PaidAmount = 0;

                        List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();

                        var result = _loanNBFCFactory.GetService(loanAccount.NBFCIdentificationCode);
                        double InterestRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(accountTreansaction.InterestRate));
                        var perDayInterestAmount = accountTreansaction.InterestType == "Percentage" ? (accountTreansaction.TransactionAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                        var interestTransDetails = await CalculateIntrest(accountTreansaction.Id, perDayInterestAmount, accountTreansaction.CreditDays.Value, accountTreansaction.OrderDate.Value, accountTreansaction.PayableBy);
                        if (interestTransDetails != null && interestTransDetails.Any())
                        {
                            interestTransDetails.ForEach(x => x.Status = "success");
                            transactionDetails.AddRange(interestTransDetails);
                        }
                        await _context.AccountTransactionDetails.AddRangeAsync(transactionDetails);

                        if (accountTreansaction.PayableBy == "Customer")
                            PaidAmount = paymentrequest.TransactionAmount + (perDayInterestAmount * accountTreansaction.CreditDays.Value);
                        else
                            PaidAmount = paymentrequest.TransactionAmount;

                        accountTreansaction.PaidAmount = PaidAmount;
                        accountTreansaction.TransactionStatusId = TransactionStatusId.Id;
                        _context.Entry(accountTreansaction).State = EntityState.Modified;


                        if (paymentrequest != null)
                        {
                            paymentrequest.PaymentStatus = "Complete";
                            _context.Entry(paymentrequest).State = EntityState.Modified;
                        }

                        if (invoice != null)
                        {
                            invoice.TotalTransAmount += paymentrequest.TransactionAmount;
                            invoice.Status = AccountInvoiceStatus.Pending.ToString();
                            _context.Entry(invoice).State = EntityState.Modified;
                        }

                        var loanAccountcredit = loanAccount.LoanAccountCredits;
                        loanAccountcredit.CreditLimitAmount = loanAccountcredit.CreditLimitAmount - accountTreansaction.OrderAmount;
                        _context.Entry(loanAccountcredit).State = EntityState.Modified;

                        int rowChanged = _context.SaveChanges();

                        if (rowChanged > 0)
                        {
                            commonResponse.status = true;
                            commonResponse.Message = "Success";
                            commonResponse.Result = "Transaction status change from Initiate to Intransit";
                        }
                        else
                        {
                            commonResponse.status = false;
                            commonResponse.Message = "Fail";
                            commonResponse.Result = "Some error occurred. Please try after some time.";
                        }
                    }
                    else
                    {

                        var TransactionStatusId = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Failed).FirstOrDefault();
                        accountTreansaction.TransactionStatusId = TransactionStatusId.Id;
                        _context.Entry(accountTreansaction).State = EntityState.Modified;
                        var transDetails = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == accountTreansaction.Id && x.Status == "initiate" && x.IsActive && !x.IsDeleted).ToList();

                        if (transDetails != null && transDetails.Any())
                        {
                            foreach (var item in transDetails)
                            {
                                item.Status = "failed";
                                _context.Entry(item).State = EntityState.Modified;
                            }
                        }

                        int rowChanged = _context.SaveChanges();

                        if (rowChanged > 0)
                        {
                            commonResponse.status = true;
                            commonResponse.Message = "Success";
                            commonResponse.Result = "Credit Limit Increase Due to Transaction is Failed";
                        }
                    }
                }
            }


            return commonResponse;
        }

        public async Task<InvoiceResponseDC> UpdateInvoiceInformation(InvoiceRequestDTO request)
        {
            InvoiceResponseDC commonResponse = new InvoiceResponseDC();

            commonResponse.status = false;
            commonResponse.Message = "failed";
            var Transactiontypes = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Intransit
                                                                        || x.Code == TransactionStatuseConstants.Pending
                                                                        || x.Code == TransactionStatuseConstants.NBFCPostingFailed).ToList();

            var Transaction_Intransit = Transactiontypes.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Intransit);
            var Transaction_NBFCFail = Transactiontypes.FirstOrDefault(x => x.Code == TransactionStatuseConstants.NBFCPostingFailed);

            var TransactionDetailHead_InterestId = _context.TransactionDetailHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Interest)?.Id;
            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.OrderNo == request.OrderNo && x.IsActive && !x.IsDeleted);
            if (invoice != null)
            {
                var LeadAccount = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == invoice.LoanAccountId && x.IsActive && !x.IsDeleted);

                if (!LeadAccount.IsAccountActive)
                {
                    commonResponse.status = false;
                    commonResponse.Message = "This account is temporary inactive. Please contact customer care.";
                    return commonResponse;
                }
                if (LeadAccount.IsBlock)
                {
                    commonResponse.status = false;
                    commonResponse.Message = "This account is block. Please contact customer care.";
                    return commonResponse;
                }

                if (invoice.Status == AccountInvoiceStatus.Created.ToString())
                {
                    commonResponse.status = true;
                    commonResponse.Message = "Success";
                    commonResponse.Result = "Invoice already posted.";
                    return commonResponse;
                }

                if (request.InvoiceAmount != invoice.TotalTransAmount)
                {
                    commonResponse.status = false;
                    commonResponse.Message = "failed";
                    commonResponse.Result = "Invoice amount does not match with transaction amount.";
                    return commonResponse;
                }

                var accountTreansactions = await _context.AccountTransactions.Where(x => x.InvoiceId == invoice.Id
                                                        && (x.TransactionStatusId == Transaction_Intransit.Id ||
                                                            x.TransactionStatusId == Transaction_NBFCFail.Id)
                                                        && x.IsActive && !x.IsDeleted).ToListAsync();



                if (accountTreansactions == null || !accountTreansactions.Any())
                {
                    commonResponse.status = false;
                    commonResponse.Message = "failed";
                    commonResponse.Result = "Transaction not exist";
                }
                else
                {
                    if (request.StatusMsg == "Success" && !string.IsNullOrEmpty(request.InvoiceNo.Trim()) && !string.IsNullOrEmpty(request.InvoicePdfURL.Trim()))
                    {
                        if (!StringHelper.URLExists(request.InvoicePdfURL.Trim()))
                        {
                            commonResponse.status = false;
                            commonResponse.Message = "failed";
                            commonResponse.Result = "Invoice Url not exists";
                            return commonResponse;
                        }

                        List<string> transhead = new List<string> {
                        TransactionDetailHeadsConstants.Order,
                        TransactionDetailHeadsConstants.Refund
                        };
                        //var TransAmount = accountTreansactions.SelectMany(x => x.AccountTransactionDetails.Where(x => x.IsActive && !x.IsDeleted
                        //                            && transhead.Contains(x.TransactionDetailHead.Code)).Select(x => x.Amount)).Sum();


                        var TransactionStatusId = Transactiontypes.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Pending);

                        invoice.InvoiceDate = request.InvoiceDate;
                        invoice.InvoiceNo = request.InvoiceNo;
                        invoice.InvoiceAmount = request.InvoiceAmount;
                        invoice.InvoicePdfUrl = request.InvoicePdfURL;
                        invoice.Status = AccountInvoiceStatus.Created.ToString();
                        _context.Entry(invoice).State = EntityState.Modified;
                        DateTime invoiceDate = request.InvoiceDate; //indianTime;
                        foreach (var accountTreansaction in accountTreansactions)
                        {

                            var InterTransDetails = accountTreansaction.AccountTransactionDetails.Where(x => x.IsActive && !x.IsDeleted
                                                                && x.TransactionDetailHeadId == TransactionDetailHead_InterestId).ToList();


                            int i = 1;
                            foreach (var InterTransDetail in InterTransDetails.OrderBy(x => x.TransactionDate))
                            {
                                InterTransDetail.TransactionDate = invoiceDate.AddDays(i);
                                _context.Entry(InterTransDetail).State = EntityState.Modified;
                                i++;
                            }

                            accountTreansaction.InvoiceDate = request.InvoiceDate;
                            accountTreansaction.DueDate = request.InvoiceDate.AddDays(Convert.ToInt64(accountTreansaction.CreditDays));
                            accountTreansaction.InvoiceNo = request.InvoiceNo;
                            accountTreansaction.InvoicePdfURL = request.InvoicePdfURL;
                            accountTreansaction.TransactionStatusId = TransactionStatusId.Id;
                            _context.Entry(accountTreansaction).State = EntityState.Modified;
                        }

                        var rowChanged = _context.SaveChanges();
                        if (rowChanged > 0)
                        {
                            var NBFCIdentificationCode = (await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == invoice.LoanAccountId))?.NBFCIdentificationCode;
                            if (!string.IsNullOrEmpty(NBFCIdentificationCode))
                            {
                                var nbfcFactory = _loanNBFCFactory.GetService(NBFCIdentificationCode);
                                var response = await nbfcFactory.OrderCaptured(invoice.Id, invoice.LoanAccountId, invoice.InvoiceAmount, true, request.OrderNo, request.ayeFinNBFCToken);
                                if (response.IsSuccess)
                                {
                                    var cutName = LeadAccount.CustomerName.Split(" ");
                                    commonResponse.AnchorName = (string.IsNullOrEmpty(LeadAccount.AnchorName) ? "" : LeadAccount.AnchorName);
                                    commonResponse.AnchorCompanyId = accountTreansactions.FirstOrDefault().AnchorCompanyId;
                                    commonResponse.CustomerName = cutName != null && cutName.Any() ? cutName[0] : "";
                                    commonResponse.MobileNo = LeadAccount.MobileNo;
                                    commonResponse.status = true;
                                    commonResponse.Message = "Success";
                                    commonResponse.Result = "Transaction status change from Initiate to Pending";
                                }
                                else
                                {
                                    var FailTransactionStatusesId = Transactiontypes.FirstOrDefault(x => x.Code == TransactionStatuseConstants.NBFCPostingFailed)?.Id;
                                    invoice.Status = AccountInvoiceStatus.NBFCPostingFailed.ToString();
                                    _context.Entry(invoice).State = EntityState.Modified;

                                    if (FailTransactionStatusesId != null)
                                    {
                                        foreach (var accountTreansaction in accountTreansactions)
                                        {
                                            accountTreansaction.TransactionStatusId = FailTransactionStatusesId.Value;
                                            _context.Entry(accountTreansaction).State = EntityState.Modified;
                                        }
                                        //var loanAccountCredit = await _context.LoanAccountCredits.FirstOrDefaultAsync(x => x.LoanAccountId == accountTreansaction.LoanAccountId && x.IsActive && !x.IsDeleted);
                                        //if (loanAccountCredit != null)
                                        //{
                                        //    loanAccountCredit.CreditLimitAmount = loanAccountCredit.CreditLimitAmount + accountTreansaction.PaidAmount;
                                        //    _context.Entry(loanAccountCredit).State = EntityState.Modified;
                                        //}
                                        _context.SaveChanges();
                                    }
                                    commonResponse.status = false;
                                    commonResponse.Message = "Your payment can not be processed due to technical reason.";
                                    commonResponse.Result = response.Message;
                                }
                            }
                            else
                            {

                                commonResponse.status = true;
                                commonResponse.Message = "Success";
                                commonResponse.Result = "Transaction status change from Initiate to Pending";
                            }
                        }


                    }
                    else
                    {
                        commonResponse.status = false;
                        commonResponse.Message = "failed";
                        commonResponse.Result = "Invoice no and Invoice Pdf required.";
                    }

                }
            }
            else
            {
                commonResponse.status = false;
                commonResponse.Message = "failed";
                commonResponse.Result = "Invoice not exist";
            }
            return commonResponse;
        }

        public async Task<CommonResponse> RefundTransactionold(string orderNo, double refundAmount)
        {
            CommonResponse res = new CommonResponse();
            double actualrefundAmount = refundAmount;
            var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();
            var transactionStatuse = _context.TransactionStatuses.Where(x => x.IsActive).ToList();
            var Transactiontypes = _context.TransactionTypes.ToList();

            var TransactionType_OrderPlacement = Transactiontypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement);
            var TransactionHead_RefundId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Refund).Id;
            var TransactionHead_OrderId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Order).Id;
            var TransactionHead_InterestId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Interest).Id;

            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.OrderNo == orderNo && x.IsActive && !x.IsDeleted);


            if (invoice != null)
            {
                var TransactionOrderPlacements = await _context.AccountTransactions.Where(x => x.InvoiceId == invoice.Id && x.TransactionTypeId == TransactionType_OrderPlacement.Id && x.IsActive && !x.IsDeleted).Include(x => x.AccountTransactionDetails).ToListAsync();   //.Include(x => x.AccountTransactionDetails).ToListAsync();
                var loanaccount = await _context.LoanAccounts.Where(x => x.Id == invoice.LoanAccountId).Include(x => x.LoanAccountCredits).FirstOrDefaultAsync();

                if (TransactionOrderPlacements != null)
                {



                    List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();
                    var result = _loanNBFCFactory.GetService(loanaccount.NBFCIdentificationCode);

                    if (result != null && loanaccount.NBFCIdentificationCode == LeadNBFCConstants.AyeFinanceSCF.ToString())
                    {
                        GRPCRequest<CancelTxnReq> cancelreq = new GRPCRequest<CancelTxnReq>
                        {
                            Request = new CancelTxnReq
                            {
                                loanId = invoice.LoanAccountId,
                                token = "",
                                amount = refundAmount,
                                orderId = orderNo,
                                cancellationCode = "CC1",
                                remarks = ""

                            }
                        };
                        var canceltxnRefund = result.CancelTransaction(cancelreq);
                        var loanAccountCredit = loanaccount.LoanAccountCredits;
                        if (loanAccountCredit != null)
                        {
                            loanAccountCredit.CreditLimitAmount = loanAccountCredit.CreditLimitAmount + canceltxnRefund.Result.Response.data.loanAmount;
                            _context.Entry(loanAccountCredit).State = EntityState.Modified;
                        }


                    }

                    else
                    {

                        double InterestRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(TransactionOrderPlacements.FirstOrDefault().InterestRate));


                        double GST = TransactionOrderPlacements.FirstOrDefault().GstRate;
                        string GSTper = "GST" + Convert.ToString(GST);

                        var prePaidAmount = TransactionOrderPlacements.Sum(x => x.TransactionAmount);
                        double totalTransAmt = 0;
                        foreach (var orderTrans in TransactionOrderPlacements.OrderBy(x => x.Id).ToList())
                        {
                            if (refundAmount == 0)
                                break;

                            double remainingTrasAmt = _context.AccountTransactionDetails.Where(x => x.IsActive && !x.IsDeleted && x.AccountTransactionId == orderTrans.Id
                                                        && (x.TransactionDetailHeadId == TransactionHead_RefundId || x.TransactionDetailHeadId == TransactionHead_OrderId))
                                                         .Select(x => x.Amount).Sum();
                            if (remainingTrasAmt > 0 && refundAmount > 0)
                            {
                                var newRefundAmt = remainingTrasAmt >= refundAmount ? refundAmount : remainingTrasAmt;
                                refundAmount -= newRefundAmt;
                                remainingTrasAmt -= newRefundAmt;
                                //Refund
                                transactionDetails.Add(new AccountTransactionDetail()
                                {
                                    AccountTransactionId = orderTrans.Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Amount = (-1) * newRefundAmt,
                                    TransactionDetailHeadId = TransactionHead_RefundId,
                                    IsPayableBy = orderTrans.PayableBy == "Customer",
                                    Status = "success"
                                });

                                if (remainingTrasAmt > 0)
                                {
                                    var perDayInterestAmount = orderTrans.InterestType == "Percentage" ? (remainingTrasAmt * InterestRatePerDay / 100.0) : InterestRatePerDay;

                                    var interestTransDetails = await CalculateIntrest(orderTrans.Id, perDayInterestAmount, orderTrans.CreditDays.Value, orderTrans.OrderDate.Value, orderTrans.PayableBy);
                                    double TotalinterestAmount = 0;
                                    if (interestTransDetails != null && interestTransDetails.Any())
                                    {
                                        interestTransDetails.ForEach(x => x.Status = "success");
                                        transactionDetails.AddRange(interestTransDetails);
                                        TotalinterestAmount = transactionDetails.Where(x => x.IsActive).Sum(x => x.Amount);
                                    }

                                    double PaidAmount = remainingTrasAmt + (orderTrans.PayableBy == "Customer" ? (TotalinterestAmount) : 0);
                                    orderTrans.PaidAmount = PaidAmount;
                                    orderTrans.TransactionAmount = remainingTrasAmt;
                                    if (PaidAmount == 0)
                                    {
                                        orderTrans.PaidAmount = 0;
                                        orderTrans.TransactionAmount = 0;
                                        orderTrans.TransactionStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Canceled).Id;


                                        var transdetailList = orderTrans.AccountTransactionDetails.Where(x => x.TransactionDetailHeadId == TransactionHead_InterestId && x.IsActive).ToList();
                                        foreach (var transdetail in transdetailList)
                                        {
                                            transdetail.IsActive = false;
                                            transdetail.IsDeleted = true;
                                            transdetail.LastModified = indianTime;
                                        };
                                    }
                                }
                                else
                                {
                                    orderTrans.PaidAmount = 0;
                                    orderTrans.TransactionAmount = 0;
                                    orderTrans.TransactionStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Canceled).Id;

                                    var transdetailList = orderTrans.AccountTransactionDetails.Where(x => x.TransactionDetailHeadId == TransactionHead_InterestId && x.IsActive).ToList();
                                    foreach (var transdetail in transdetailList)
                                    {
                                        transdetail.IsActive = false;
                                        transdetail.IsDeleted = true;
                                        transdetail.LastModified = indianTime;
                                    };
                                }

                                _context.AccountTransactionDetails.AddRange(transactionDetails);
                                _context.Entry(orderTrans).State = EntityState.Modified;
                            }

                        }



                        invoice.TotalTransAmount = TransactionOrderPlacements.Sum(x => x.TransactionAmount);
                        invoice.Comment = "Due to refund";
                        if (invoice.TotalTransAmount == 0)
                        {
                            invoice.Status = AccountInvoiceStatus.Canceled.ToString();
                        }
                        _context.Entry(invoice).State = EntityState.Modified;

                        var currentPaidAmount = TransactionOrderPlacements.Sum(x => x.TransactionAmount);
                        var loanAccountCredit = loanaccount.LoanAccountCredits;
                        loanAccountCredit.CreditLimitAmount = loanAccountCredit.CreditLimitAmount + (prePaidAmount - currentPaidAmount);

                        _context.Entry(loanAccountCredit).State = EntityState.Modified;
                    }

                    if (_context.SaveChanges() > 0)
                    {
                        ///if invoice post to nbfc then cancel invoice when prePaidAmount=0

                        res.status = true;
                        res.Message = "Refund Success";
                    }
                    else
                    {
                        res.status = false;
                        res.Message = "Refund Failed";
                    }

                }
                else
                {
                    res.status = false;
                    res.Message = "Invoice transaction not found.";
                }
            }
            else
            {
                res.status = false;
                res.Message = "Invoice not found.";
            }

            return res;
        }


        public async Task<List<AccountTransactionDetail>> CalculateIntrest(long TransactionId, double InterestAmount, long CreditDays, DateTime IntrestDate, string PayableBy, bool isSkipPaidInterest = false)
        {
            List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();

            var TransactionHeads = _context.TransactionDetailHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Interest);
            if (TransactionId > 0)
            {
                var transactiondetails = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == TransactionId && x.TransactionDetailHead.Code == TransactionDetailHeadsConstants.Interest && x.IsActive && !x.IsDeleted).ToList();
                foreach (var item in transactiondetails)
                {
                    if (isSkipPaidInterest == false)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        //item.LastModified = DateTime.UtcNow;
                        _context.Entry(item).State = EntityState.Modified;
                    }
                    if (isSkipPaidInterest == true && item.TransactionDate.Value.Date > IntrestDate.Date)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        //item.LastModified = DateTime.UtcNow;    
                        _context.Entry(item).State = EntityState.Modified;
                    }
                }
            }

            if (InterestAmount > 0)
            {
                for (int i = 1; i <= CreditDays; i++)
                {
                    transactionDetails.Add(new AccountTransactionDetail()
                    {
                        AccountTransactionId = TransactionId,
                        Amount = InterestAmount,
                        TransactionDetailHeadId = TransactionHeads.Id,
                        IsPayableBy = PayableBy == "Customer",
                        IsActive = true,
                        IsDeleted = false,
                        TransactionDate = IntrestDate.AddDays(Convert.ToInt64(i)),
                        Status = "initiate"
                    });
                }
            }

            return transactionDetails;

        }

        public async Task<InvoiceListDTO> GetInvoiceList(InvoiceRequest invoiceRequest)
        {
            InvoiceListDTO invoiceListDTO = new InvoiceListDTO();
            var predicate = PredicateBuilder.True<Invoice>();
            predicate = predicate.And(x => x.IsActive);
            if (!string.IsNullOrEmpty(invoiceRequest.Search.Trim()))
                predicate = predicate.And(x => x.InvoiceNo.Contains(invoiceRequest.Search)
                                            || x.OrderNo.Contains(invoiceRequest.Search)
                                            || x.LoanAccount.AccountCode.Contains(invoiceRequest.Search));
            invoiceListDTO.TotalRecord = _context.Invoices.Count(predicate);
            if (invoiceListDTO.TotalRecord > 0)
                invoiceListDTO.Invoices = _context.Invoices.Where(predicate).OrderByDescending(x => x.Created).Skip(invoiceRequest.Skip).Take(invoiceRequest.Take)
                                        .Select(x => new InvoiceDTO
                                        {
                                            InvoiceId = x.Id,
                                            LoanAccountId = x.LoanAccountId,
                                            AnchoreName = x.LoanAccount.AnchorName,
                                            CreatedDate = x.Created,
                                            CustomerName = x.LoanAccount.CustomerName,
                                            InvoiceAmount = x.InvoiceAmount,
                                            InvoiceDate = x.InvoiceDate,
                                            InvoiceNo = x.InvoiceNo,
                                            InvoicePdfUrl = x.InvoicePdfUrl,
                                            LoanNo = x.LoanAccount.AccountCode,
                                            OrderAmount = x.OrderAmount,
                                            OrderNo = x.OrderNo,
                                            Status = x.Status,
                                            TotalTransAmount = x.TotalTransAmount
                                        }).ToList();

            return invoiceListDTO;
        }

        public async Task<List<InvoiceNBFCReqRes>> GetInvoiceRequestResponse(long invoiceId, long loanAccountId)
        {
            List<InvoiceNBFCReqRes> invoiceNBFCReqRes = new List<InvoiceNBFCReqRes>();
            var NBFCIdentificationCode = _context.LoanAccounts.FirstOrDefault(x => x.Id == loanAccountId)?.NBFCIdentificationCode;
            List<string> statusIN = new List<string> { LeadNBFCApiConstants.Error, LeadNBFCApiConstants.CompletedWithError };

            var ApiDetailIds = _context.NBFCComapnyAPIMasters.Where(x => x.InvoiceId == invoiceId && x.TransactionTypeCode == TransactionTypesConstants.OrderPlacement
                                                        && statusIN.Contains(x.Status)
                                                         && x.TransactionStatuCode == TransactionsStatusConstants.Captured && x.IsActive && !x.IsDeleted)
              .SelectMany(x => x.NBFCComapnyApiDetails.Select(y => y.Id)).ToList();

            if (ApiDetailIds != null && ApiDetailIds.Any())
            {
                var loanfactory = _loanNBFCFactory.GetService(NBFCIdentificationCode);
                invoiceNBFCReqRes = await loanfactory.GetInvoiceNBFCReqRes(ApiDetailIds);
            }
            return invoiceNBFCReqRes;
        }

        public async Task<CommonResponse> PostNBFCInvoice(long invoiceId, long loanAccountId)
        {
            CommonResponse commonResponse = new CommonResponse();
            var NBFCIdentificationCode = _context.LoanAccounts.FirstOrDefault(x => x.Id == loanAccountId)?.NBFCIdentificationCode;
            if (!string.IsNullOrEmpty(NBFCIdentificationCode))
            {
                var token = "";
                var invoice = _context.Invoices.Where(x => x.Id == invoiceId).Include(x => x.AccountTransactions).FirstOrDefault();
                var nbfcFactory = _loanNBFCFactory.GetService(NBFCIdentificationCode);
                var response = await nbfcFactory.OrderCaptured(invoiceId, invoice.LoanAccountId, invoice.InvoiceAmount, true, invoice.OrderNo, token); //need to be give token
                if (response.IsSuccess)
                {

                    var pendingTransactionStatusesId = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Pending)?.Id;
                    invoice.Status = AccountInvoiceStatus.Created.ToString();
                    _context.Entry(invoice).State = EntityState.Modified;

                    if (pendingTransactionStatusesId != null)
                    {
                        foreach (var accountTransaction in invoice.AccountTransactions)
                        {
                            accountTransaction.TransactionStatusId = pendingTransactionStatusesId.Value;
                            _context.Entry(accountTransaction).State = EntityState.Modified;
                        }
                        _context.SaveChanges();
                    }

                    commonResponse.status = true;
                    commonResponse.Message = "Success";
                    commonResponse.Result = "Transaction status change from Initiate to Pending";
                }
                else
                {
                    var FailTransactionStatusesId = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionStatuseConstants.NBFCPostingFailed)?.Id;
                    invoice.Status = AccountInvoiceStatus.NBFCPostingFailed.ToString();
                    _context.Entry(invoice).State = EntityState.Modified;

                    if (FailTransactionStatusesId != null)
                    {
                        foreach (var accountTreansaction in invoice.AccountTransactions)
                        {
                            accountTreansaction.TransactionStatusId = FailTransactionStatusesId.Value;
                            _context.Entry(accountTreansaction).State = EntityState.Modified;
                        }
                        _context.SaveChanges();
                    }
                    commonResponse.status = false;
                    commonResponse.Message = response.Message;
                    commonResponse.Result = response.Message;
                }
            }
            else
            {

                commonResponse.status = false;
                commonResponse.Message = "Failed";
                commonResponse.Result = "NBFC Company Not Found.";
            }

            return commonResponse;
        }

        public async Task<GRPCReply<string>> RefundTransaction(GRPCRequest<RefundRequestDTO> req)
        {
            GRPCReply<string> res = new GRPCReply<string>() { Message = "Data not found"};
            double actualrefundAmount = req.Request.RefundAmount;
            var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();
            var transactionStatuse = _context.TransactionStatuses.Where(x => x.IsActive).ToList();
            var Transactiontypes = _context.TransactionTypes.ToList();

            var TransactionType_OrderPlacement = Transactiontypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement);
            var TransactionHead_RefundId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Refund).Id;
            var TransactionHead_OrderId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Order).Id;
            var TransactionHead_InterestId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Interest).Id;

            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.OrderNo == req.Request.OrderNo && x.IsActive && !x.IsDeleted);


            if (invoice != null)
            {
                var TransactionOrderPlacements = await _context.AccountTransactions.Where(x => x.InvoiceId == invoice.Id && x.TransactionTypeId == TransactionType_OrderPlacement.Id && x.IsActive && !x.IsDeleted).Include(x => x.AccountTransactionDetails).ToListAsync();   //.Include(x => x.AccountTransactionDetails).ToListAsync();
                var loanaccount = await _context.LoanAccounts.Where(x => x.Id == invoice.LoanAccountId).Include(x => x.LoanAccountCredits).FirstOrDefaultAsync();

                if (TransactionOrderPlacements != null)
                {



                    List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();
                    var result = _loanNBFCFactory.GetService(loanaccount.NBFCIdentificationCode);

                    if (result != null && loanaccount.NBFCIdentificationCode == LeadNBFCConstants.AyeFinanceSCF.ToString())
                    {
                        GRPCRequest<CancelTxnReq> cancelreq = new GRPCRequest<CancelTxnReq>
                        {
                            Request = new CancelTxnReq
                            {
                                loanId = invoice.LoanAccountId,
                                token = req.Request.NBFCToken,
                                amount = req.Request.RefundAmount,
                                orderId = req.Request.OrderNo,
                                cancellationCode = "CC1",
                                remarks = ""

                            }
                        };
                        var canceltxnRefund = result.CancelTransaction(cancelreq);
                        var loanAccountCredit = loanaccount.LoanAccountCredits;
                        if (loanAccountCredit != null)
                        {
                            loanAccountCredit.CreditLimitAmount = loanAccountCredit.CreditLimitAmount + canceltxnRefund.Result.Response.data.loanAmount;
                            _context.Entry(loanAccountCredit).State = EntityState.Modified;
                            //accounttransactiondetailsss
                        }


                    }

                    else
                    {

                        double InterestRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(TransactionOrderPlacements.FirstOrDefault().InterestRate));


                        double GST = TransactionOrderPlacements.FirstOrDefault().GstRate;
                        string GSTper = "GST" + Convert.ToString(GST);

                        var prePaidAmount = TransactionOrderPlacements.Sum(x => x.TransactionAmount);
                        double totalTransAmt = 0;
                        foreach (var orderTrans in TransactionOrderPlacements.OrderBy(x => x.Id).ToList())
                        {
                            if (req.Request.RefundAmount == 0)
                                break;

                            double remainingTrasAmt = _context.AccountTransactionDetails.Where(x => x.IsActive && !x.IsDeleted && x.AccountTransactionId == orderTrans.Id
                                                        && (x.TransactionDetailHeadId == TransactionHead_RefundId || x.TransactionDetailHeadId == TransactionHead_OrderId))
                                                         .Select(x => x.Amount).Sum();
                            if (remainingTrasAmt > 0 && req.Request.RefundAmount > 0)
                            {
                                var newRefundAmt = remainingTrasAmt >= req.Request.RefundAmount ? req.Request.RefundAmount : remainingTrasAmt;
                                req.Request.RefundAmount -= newRefundAmt;
                                remainingTrasAmt -= newRefundAmt;
                                //Refund
                                transactionDetails.Add(new AccountTransactionDetail()
                                {
                                    AccountTransactionId = orderTrans.Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Amount = (-1) * newRefundAmt,
                                    TransactionDetailHeadId = TransactionHead_RefundId,
                                    IsPayableBy = orderTrans.PayableBy == "Customer",
                                    Status = "success"
                                });

                                if (remainingTrasAmt > 0)
                                {
                                    var perDayInterestAmount = orderTrans.InterestType == "Percentage" ? (remainingTrasAmt * InterestRatePerDay / 100.0) : InterestRatePerDay;

                                    var interestTransDetails = await CalculateIntrest(orderTrans.Id, perDayInterestAmount, orderTrans.CreditDays.Value, orderTrans.OrderDate.Value, orderTrans.PayableBy);
                                    double TotalinterestAmount = 0;
                                    if (interestTransDetails != null && interestTransDetails.Any())
                                    {
                                        interestTransDetails.ForEach(x => x.Status = "success");
                                        transactionDetails.AddRange(interestTransDetails);
                                        TotalinterestAmount = transactionDetails.Where(x => x.IsActive).Sum(x => x.Amount);
                                    }

                                    double PaidAmount = remainingTrasAmt + (orderTrans.PayableBy == "Customer" ? (TotalinterestAmount) : 0);
                                    orderTrans.PaidAmount = PaidAmount;
                                    orderTrans.TransactionAmount = remainingTrasAmt;
                                    if (PaidAmount == 0)
                                    {
                                        orderTrans.PaidAmount = 0;
                                        orderTrans.TransactionAmount = 0;
                                        orderTrans.TransactionStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Canceled).Id;


                                        var transdetailList = orderTrans.AccountTransactionDetails.Where(x => x.TransactionDetailHeadId == TransactionHead_InterestId && x.IsActive).ToList();
                                        foreach (var transdetail in transdetailList)
                                        {
                                            transdetail.IsActive = false;
                                            transdetail.IsDeleted = true;
                                            transdetail.LastModified = indianTime;
                                        };
                                    }
                                }
                                else
                                {
                                    orderTrans.PaidAmount = 0;
                                    orderTrans.TransactionAmount = 0;
                                    orderTrans.TransactionStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Canceled).Id;

                                    var transdetailList = orderTrans.AccountTransactionDetails.Where(x => x.TransactionDetailHeadId == TransactionHead_InterestId && x.IsActive).ToList();
                                    foreach (var transdetail in transdetailList)
                                    {
                                        transdetail.IsActive = false;
                                        transdetail.IsDeleted = true;
                                        transdetail.LastModified = indianTime;
                                    };
                                }

                                _context.AccountTransactionDetails.AddRange(transactionDetails);
                                _context.Entry(orderTrans).State = EntityState.Modified;
                            }

                        }



                        invoice.TotalTransAmount = TransactionOrderPlacements.Sum(x => x.TransactionAmount);
                        invoice.Comment = "Due to refund";
                        if (invoice.TotalTransAmount == 0)
                        {
                            invoice.Status = AccountInvoiceStatus.Canceled.ToString();
                        }
                        _context.Entry(invoice).State = EntityState.Modified;

                        var currentPaidAmount = TransactionOrderPlacements.Sum(x => x.TransactionAmount);
                        var loanAccountCredit = loanaccount.LoanAccountCredits;
                        loanAccountCredit.CreditLimitAmount = loanAccountCredit.CreditLimitAmount + (prePaidAmount - currentPaidAmount);

                        _context.Entry(loanAccountCredit).State = EntityState.Modified;
                    }

                    if (_context.SaveChanges() > 0)
                    {
                        ///if invoice post to nbfc then cancel invoice when prePaidAmount=0

                        res.Status = true;
                        res.Message = "Refund Success";
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "Refund Failed";
                    }

                }
                else
                {
                    res.Status = false;
                    res.Message = "Invoice transaction not found.";
                }
            }
            else
            {
                res.Status = false;
                res.Message = "Invoice not found.";
            }

            return res;

        }

    }
}
