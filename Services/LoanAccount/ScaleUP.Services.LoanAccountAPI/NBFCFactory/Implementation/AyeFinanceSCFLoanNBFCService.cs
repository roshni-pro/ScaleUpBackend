using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.LoanAccountAPI.Helpers.NBFC;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate;
using ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using ScaleUP.Services.LoanAccountModels.Master;
using DocumentFormat.OpenXml.InkML;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC.AyeFinance;
using MassTransit.Initializers;

using ScaleUP.Services.LoanAccountModels.Transaction.NBFC.AyeFinance;

namespace ScaleUP.Services.LoanAccountAPI.NBFCFactory.Implementation
{
    public class AyeFinanceSCFLoanNBFCService : BaseNBFCService, ILoanNBFCService
    {
        private readonly AyeFinanceSCFNBFCHelper _AyeFinanceSCFNBFCHelper;
        private readonly LoanAccountApplicationDbContext _context;
        public AyeFinanceSCFLoanNBFCService(AyeFinanceSCFNBFCHelper AyeFinanceSCFNBFCHelper, LoanAccountApplicationDbContext context) : base(context)
        {
            _context = context;
            _AyeFinanceSCFNBFCHelper = AyeFinanceSCFNBFCHelper;

        }
        #region AyeFinanceSCF
        public async Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> request)
        {
            GRPCReply<ApplyLoanResponseDC> gRPCReply = new GRPCReply<ApplyLoanResponseDC>();
            if (request != null && request.Request != null && !string.IsNullOrEmpty(request.Request.token))
            {
                var leadCode = await _context.LoanAccounts.Where(x => x.IsActive && !x.IsDeleted && x.Id == request.Request.loanId).Select(x => x.LeadCode).FirstOrDefaultAsync();
                ApplyLoanthirdpartyreq applyloan = new ApplyLoanthirdpartyreq
                {
                    leadId = leadCode != null ? leadCode : "",
                    refId = request.Request.orderId + DateTime.Now.ToString("HH:mm:ss"),
                    amount = request.Request.amount,
                    orderId = request.Request.orderId
                };

                var response = await _AyeFinanceSCFNBFCHelper.ApplyLoan(request.Request.loanId, request.Request.NBFCCompanyAPIDetailId, request.Request.APIUrl, request.Request.token, applyloan);
                _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                //_context.SaveChanges();

                if (response.IsSuccess)
                {
                    var res = JsonConvert.DeserializeObject<ApplyLoanResponseDC>(response.Response);
                    if (res != null && !string.IsNullOrEmpty(res.status))
                    {
                        GRPCRequest<AyeloanReq> checkrequest = new GRPCRequest<AyeloanReq>
                        {
                            Request = new AyeloanReq { loanId = request.Request.loanId, token = request.Request.token }
                        };
                        var existingUpdate = await _context.AyeFinanceUpdates.FirstOrDefaultAsync(x => x.orderId == request.Request.orderId && x.IsActive && !x.IsDeleted);
                        if (existingUpdate == null)
                        {
                            var ayeFinanceUpdate = new AyeFinanceUpdate
                            {
                                LoanAccountId = request.Request.loanId,
                                refId = applyloan.refId,
                                leadCode = leadCode,
                                switchpeReferenceId = res.switchpeReferenceId,
                                IsActive = true,
                                IsDeleted = false,
                                Created = DateTime.Now,
                                CreatedBy = "",
                                status = 0,
                                orderId = request.Request.orderId,
                                invoiceNo = request.Request.invoiceId.ToString(),
                                transactionId = res.data.transactionId,
                                totallimit = request.Request.TotalLimit??0 ,
                                availablelLimit = request.Request.AvailableLimit

                            };
                            _context.AyeFinanceUpdates.Add(ayeFinanceUpdate);
                        }

                        else
                        {
                            existingUpdate.refId = applyloan.refId;
                            existingUpdate.leadCode = leadCode;
                            existingUpdate.switchpeReferenceId = res.switchpeReferenceId;
                            existingUpdate.transactionId = res.data.transactionId;
                            existingUpdate.totallimit = request.Request.TotalLimit;
                            existingUpdate.availablelLimit = request.Request.AvailableLimit;
                            existingUpdate.LastModified = DateTime.Now;

                            _context.Entry(existingUpdate).State = EntityState.Modified;
                        }
                        _context.SaveChanges();
                        gRPCReply.Response = res;
                        gRPCReply.Status = true;
                        gRPCReply.Message = res.message;

                    }
                    else
                    {
                        gRPCReply.Message = res != null && res.message != null ? res.message : "Failed ";
                    }
                }
                else
                {
                    gRPCReply.Message = "Failed by third party "; gRPCReply.Status = response.IsSuccess;
                }

            }
            return gRPCReply;
        }

        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> request)
        {
            GRPCReply<DeliveryConfirmationResponseDC> gRPCReply = new GRPCReply<DeliveryConfirmationResponseDC>();
            if (request != null && request.Request != null && !string.IsNullOrEmpty(request.Request.token))
            {
                var nbfcApi = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.AyeFinDeliveryConfirmation && x.IsActive && !x.IsDeleted);
                if (nbfcApi != null)
                {
                    var NbfcApiDetails = await _context.NBFCComapnyApiDetails.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.NBFCCompanyAPIId == nbfcApi.Id);
                    var NbfcApiDetailsId = NbfcApiDetails != null ? NbfcApiDetails.Id : 0;
                    var leadCode = await _context.LoanAccounts.Where(x => x.IsActive && !x.IsDeleted && x.Id == request.Request.loanId).Select(x => x.LeadCode).FirstOrDefaultAsync();
                    DeliveryConfirmationthirdpartyreq applyloan = new DeliveryConfirmationthirdpartyreq
                    {
                        leadId = leadCode != null ? leadCode : "",
                        refId = Guid.NewGuid().ToString(),
                        amount = request.Request.amount,
                        orderId = request.Request.orderId,
                        trackingId = Guid.NewGuid().ToString(),
                        invoiceNo = request.Request.invoiceNo
                    };

                    var response = await _AyeFinanceSCFNBFCHelper.DeliveryConfirmation(request.Request.loanId, NbfcApiDetailsId, nbfcApi.APIUrl, request.Request.token, applyloan);
                    _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                    _context.SaveChanges();

                    if (response.IsSuccess)
                    {
                        var res = JsonConvert.DeserializeObject<DeliveryConfirmationResponseDC>(response.Response);
                        if (res != null && !string.IsNullOrEmpty(res.status))
                        {
                            gRPCReply.Response = res;
                            gRPCReply.Status = true;
                            gRPCReply.Message = res.message;
                            GRPCRequest<AyeloanReq> checkrequest = new GRPCRequest<AyeloanReq>
                            {
                                Request = new AyeloanReq { loanId = request.Request.loanId, token = request.Request.token }
                            };
                            var limit = CheckTotalAndAvailableLimit(checkrequest);
                            var existingUpdate = await _context.AyeFinanceUpdates.FirstOrDefaultAsync(x => x.LoanAccountId == request.Request.loanId && x.IsActive && !x.IsDeleted);
                            if (existingUpdate == null)
                            {
                                var ayeFinanceUpdate = new AyeFinanceUpdate
                                {
                                    LoanAccountId = request.Request.loanId,
                                    refId = applyloan.refId,
                                    leadCode = leadCode,
                                    switchpeReferenceId = res.switchpeReferenceId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Created = DateTime.Now,
                                    CreatedBy = "",
                                    status = 1,
                                    transactionId = res.data.transactionId,
                                    totallimit = limit.Result.Response.data.totalLimit,
                                    availablelLimit = limit.Result.Response.data.availableLimit

                                };

                                _context.AyeFinanceUpdates.Add(ayeFinanceUpdate);
                                _context.SaveChanges();
                            }
                            else
                            {
                                existingUpdate.refId = applyloan.refId;
                                existingUpdate.leadCode = leadCode;
                                existingUpdate.switchpeReferenceId = res.switchpeReferenceId;
                                existingUpdate.transactionId = res.data.transactionId;
                                existingUpdate.totallimit = limit.Result.Response.data.totalLimit;
                                existingUpdate.availablelLimit = limit.Result.Response.data.availableLimit;
                                existingUpdate.LastModified = DateTime.Now;

                                _context.Entry(existingUpdate).State = EntityState.Modified;
                            }
                            _context.SaveChanges();

                        }
                        else
                        {
                            gRPCReply.Message = res != null && res.message != null ? res.message : "Failed ";
                        }
                    }
                    else
                    {
                        gRPCReply.Message = "Failed by third party "; gRPCReply.Status = response.IsSuccess;
                    }
                }

            }
            return gRPCReply;
        }

        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request)
        {
            GRPCReply<DeliveryConfirmationResponseDC> gRPCReply = new GRPCReply<DeliveryConfirmationResponseDC>();
            if (request != null && request.Request != null && !string.IsNullOrEmpty(request.Request.token))
            {
                var nbfcApi = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.AyeFinRepayment && x.IsActive && !x.IsDeleted);
                if (nbfcApi != null)
                {
                    var paymentRequest = await _context.PaymentRequest.FirstOrDefaultAsync(y => y.IsActive && !y.IsDeleted && y.TransactionReqNo == request.Request.TransactionReqNo);
                    var NbfcApiDetails = await _context.NBFCComapnyApiDetails.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.NBFCCompanyAPIId == nbfcApi.Id);
                    var NbfcApiDetailsId = NbfcApiDetails != null ? NbfcApiDetails.Id : 0;
                    var leadCode = await _context.LoanAccounts.Where(x => x.IsActive && !x.IsDeleted && x.Id == request.Request.loanId).Select(x => x.LeadCode).FirstOrDefaultAsync();
                    RepaymentThirdpartyreq repaymentrequest = new RepaymentThirdpartyreq
                    {
                        leadId = leadCode != null ? leadCode : "",
                        refId = Guid.NewGuid().ToString(),
                        amount = request.Request.amount,
                        orderId = paymentRequest!=null ? paymentRequest.OrderNo :request.Request.orderId,
                        modeOfPayment = request.Request.modeOfPayment,
                        receiptId = request.Request.modeOfPayment,
                        adjustment = request.Request.adjustment
                    };

                    var response = await _AyeFinanceSCFNBFCHelper.Repayment(request.Request.loanId, NbfcApiDetailsId, nbfcApi.APIUrl, request.Request.token, repaymentrequest);
                    _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                    _context.SaveChanges();

                    if (response.IsSuccess)
                    {
                        var res = JsonConvert.DeserializeObject<DeliveryConfirmationResponseDC>(response.Response);
                        if (res != null && !string.IsNullOrEmpty(res.status))
                        {
                            gRPCReply.Response = res;
                            gRPCReply.Status = true;
                            gRPCReply.Message = res.message;

                        }
                        else
                        {
                            gRPCReply.Message = res != null && res.message != null ? res.message : "Failed ";
                        }
                    }
                    else
                    {
                        gRPCReply.Message = "Failed by third party "; gRPCReply.Status = response.IsSuccess;
                    }
                }

            }
            return gRPCReply;
        }

        public async Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> request)
        {


            GRPCReply<CancellationResponseDC> gRPCReply = new GRPCReply<CancellationResponseDC>();
            if (request != null && request.Request != null && !string.IsNullOrEmpty(request.Request.token))
            {
                var nbfcApi = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.AyeFinApplyLoan && x.IsActive && !x.IsDeleted);
                if (nbfcApi != null)
                {
                    var NbfcApiDetails = await _context.NBFCComapnyApiDetails.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.NBFCCompanyAPIId == nbfcApi.Id);
                    var NbfcApiDetailsId = NbfcApiDetails != null ? NbfcApiDetails.Id : 0;
                    var switchperefId = await _context.AyeFinanceUpdates.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.LoanAccountId == request.Request.loanId).Select(x => x.switchpeReferenceId);
                    var leadCode = await _context.LoanAccounts.Where(x => x.IsActive && !x.IsDeleted && x.Id == request.Request.loanId).Select(x => x.LeadCode).FirstOrDefaultAsync();
                    CancelTxnThirdPartyRed cacnelthirdparty = new CancelTxnThirdPartyRed
                    {
                        leadId = leadCode != null ? leadCode : "",
                        refId = request.Request.orderId + DateTime.Now.ToString("HH:mm:ss"),
                        amount = request.Request.amount,
                        orderId = request.Request.orderId,
                        remarks = request.Request.remarks,
                        cancellationCode = request.Request.cancellationCode,
                        switchpeReferenceId = switchperefId ?? "",

                    };

                    var response = await _AyeFinanceSCFNBFCHelper.CancelTransaction(request.Request.loanId, NbfcApiDetailsId, nbfcApi.APIUrl, request.Request.token, cacnelthirdparty);
                    _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                    _context.SaveChanges();

                    if (response.IsSuccess)
                    {

                        var res = JsonConvert.DeserializeObject<CancellationResponseDC>(response.Response);

                        if (res != null && !string.IsNullOrEmpty(res.status))
                        {
                            gRPCReply.Response = res;
                            gRPCReply.Status = true;
                            gRPCReply.Message = res.message;

                            GRPCRequest<AyeloanReq> checkrequest = new GRPCRequest<AyeloanReq>
                            {
                                Request = new AyeloanReq { loanId = request.Request.loanId, token = request.Request.token }
                            };
                            var limit = await CheckTotalAndAvailableLimit(checkrequest);
                            var existingUpdate = await _context.AyeFinanceUpdates.FirstOrDefaultAsync(x => x.LoanAccountId == request.Request.loanId && x.IsActive && !x.IsDeleted);
                            if (existingUpdate == null)
                            {
                                var ayeFinanceUpdate = new AyeFinanceUpdate
                                {
                                    LoanAccountId = request.Request.loanId,
                                    refId = cacnelthirdparty.refId,
                                    leadCode = leadCode,
                                    switchpeReferenceId = res.switchpeReferenceId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Created = DateTime.Now,
                                    CreatedBy = "",
                                    status = 1,
                                    transactionId = "",
                                    totallimit = limit.Response.data.totalLimit,
                                    availablelLimit = limit.Response.data.availableLimit

                                };

                                _context.AyeFinanceUpdates.Add(ayeFinanceUpdate);
                                _context.SaveChanges();
                            }
                            else
                            {
                                existingUpdate.refId = cacnelthirdparty.refId;
                                existingUpdate.leadCode = leadCode;
                                existingUpdate.switchpeReferenceId = res.switchpeReferenceId;
                                existingUpdate.transactionId = "";
                                existingUpdate.totallimit = limit.Response.data.totalLimit;
                                existingUpdate.availablelLimit = limit.Response.data.availableLimit;
                                existingUpdate.LastModified = DateTime.Now;

                                _context.Entry(existingUpdate).State = EntityState.Modified;
                            }
                            _context.SaveChanges();

                        }
                        else
                        {
                            gRPCReply.Message = res != null && res.message != null ? res.message : "Failed ";
                        }
                    }
                    else
                    {
                        gRPCReply.Message = "Failed by third party "; gRPCReply.Status = response.IsSuccess;
                    }
                }

            }
            return gRPCReply;
        }
        public async Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> request)
        {
            GRPCReply<CheckTotalAndAvailableLimitResponseDc> gRPCReply = new GRPCReply<CheckTotalAndAvailableLimitResponseDc>();
            if (request != null && request.Request != null && !string.IsNullOrEmpty(request.Request.token))
            {
                var nbfcApi = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.AyeFinCheckTotalAndAvailableLimit && x.IsActive && !x.IsDeleted);
                if (nbfcApi != null)
                {
                    var NbfcApiDetails = await _context.NBFCComapnyApiDetails.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.NBFCCompanyAPIId == nbfcApi.Id);
                    var NbfcApiDetailsId = NbfcApiDetails != null ? NbfcApiDetails.Id : 0;
                    var leadCode = await _context.LoanAccounts.Where(x => x.IsActive && !x.IsDeleted && x.Id == request.Request.loanId).Select(x => x.LeadCode).FirstOrDefaultAsync();
                    CheckCreditLineReqDc applyloan = new CheckCreditLineReqDc
                    {
                        leadId = leadCode != null ? leadCode : "",
                        refId = request.Request.loanId + DateTime.Now.ToString("HH:mm:ss"),
                    };

                    var response = await _AyeFinanceSCFNBFCHelper.CheckTotalAndAvailableLimit(request.Request.loanId, NbfcApiDetailsId, nbfcApi.APIUrl, request.Request.token, applyloan);
                    _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                    _context.SaveChanges();

                    if (response.IsSuccess)
                    {
                        var res = JsonConvert.DeserializeObject<CheckTotalAndAvailableLimitResponseDc>(response.Response);
                        if (res != null && !string.IsNullOrEmpty(res.status))
                        {
                            gRPCReply.Response = res;
                            gRPCReply.Status = true;
                            gRPCReply.Message = res.message;

                        }
                        else
                        {
                            gRPCReply.Message = res != null && res.message != null ? res.message : "Failed ";
                        }
                    }
                    else
                    {
                        gRPCReply.Message = "Failed by third party ";
                        gRPCReply.Status = response.IsSuccess;
                    }
                }
            }

            return gRPCReply;
        }
        #endregion

        public Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request)
        {
            throw new NotImplementedException();
        }

        //public Task<double> CalculatePerDayInterest(double interest)
        //{
        //    throw new NotImplementedException();
        //} 

        public async Task<double> CalculatePerDayInterest(double interest)
        {
            //DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365
            return (interest / 360);
        }

        public Task<double> GetAvailableCreditLimit(long accountId)
        {
            throw new NotImplementedException();
        }

        public Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<List<InvoiceNBFCReqRes>> GetInvoiceNBFCReqRes(List<long> ApiDetailIds)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> InsertSkippedPayments(long loanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInvoiceInitiated(long invoiceId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<List<LoanRepaymentScheduleDetailDc>>> LoanRepaymentScheduleDetails(long loanAccountId)
        {
            throw new NotImplementedException();
        }

        public async Task<NBFCFactoryResponse> OrderCaptured(long invoiceId, long accountId, double transAmount, bool status, string? OrderNo, string? ayeFinNBFCToken = "")
        {
            string url = string.Empty;
            List<string> statusNoIN = new List<string> { LeadNBFCApiConstants.Completed, LeadNBFCApiConstants.CompletedWithError };

            NBFCFactoryResponse nBFCFactoryResponse = new NBFCFactoryResponse();
            nBFCFactoryResponse.IsSuccess = false;
            AyeFinanceSCFNBFCHelper ayeFinanceSCFNBFCHelper = new AyeFinanceSCFNBFCHelper();
            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId);
            var loanAccountDetail = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == accountId && x.IsActive && !x.IsDeleted);
            if (loanAccountDetail != null && invoice != null)
            {
                if (status)
                {
                    var nbfcCComapnyAPI = await GetNBFCCompanyAPIMaster(invoiceId, TransactionTypesConstants.OrderPlacement, TransactionsStatusConstants.Captured);
                    if (nbfcCComapnyAPI != null && !statusNoIN.Contains(nbfcCComapnyAPI.Status)
                                                && nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList != null
                                                 && nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.Any()
                                                && nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.Any(y => !statusNoIN.Contains(y.Status)))
                    {
                        var IsSuccess = true;
                        foreach (var item in nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.Where(y => !statusNoIN.Contains(y.Status)).OrderBy(x => x.Sequence))
                        {
                            url = string.Empty;

                            switch (item.Code)
                            {
                                case CompanyApiConstants.AyeFinCheckTotalAndAvailableLimit:
                                    //var nbfcApi = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.AyeFinCheckTtoalAndAvailableLimit && x.IsActive && !x.IsDeleted);
                                    //if (nbfcApi != null)
                                    //{
                                    //    url = nbfcApi.APIUrl;
                                    //}
                                    //CheckCreditLineReqDc checkCreditLineReqDc = new CheckCreditLineReqDc();
                                    //checkCreditLineReqDc.refId = Guid.NewGuid().ToString();
                                    //checkCreditLineReqDc.leadId = loanAccountDetail.LeadCode;
                                    GRPCRequest<AyeloanReq> request = new GRPCRequest<AyeloanReq>
                                    {
                                        Request = new AyeloanReq { loanId = accountId, token = ayeFinNBFCToken }
                                    };
                                    var RequestResponses = await CheckTotalAndAvailableLimit(request);
                                    if (RequestResponses != null && RequestResponses.Status)
                                    {
                                        IsSuccess = RequestResponses.Status;
                                        nBFCFactoryResponse.Message = RequestResponses.Message;

                                    }
                                    else
                                    {
                                        IsSuccess = false;
                                        nBFCFactoryResponse.Message = "AyeFinance Limit response not found.";
                                    }

                                    break;


                                case CompanyApiConstants.AyeFinDeliveryConfirmation:

                                    var ayeFinUpdate = _context.AyeFinanceUpdates.FirstOrDefault(x => x.leadCode == loanAccountDetail.LeadCode && x.leadCode == loanAccountDetail.LeadCode);
                                    if (ayeFinUpdate != null && (ayeFinUpdate.availablelLimit >= transAmount))
                                    {


                                        GRPCRequest<DeliveryConfirmationreq> deliveryrequest = new GRPCRequest<DeliveryConfirmationreq>
                                        {
                                            Request = new DeliveryConfirmationreq
                                            {
                                                amount = Math.Round(transAmount, 2),
                                                orderId = OrderNo,
                                                invoiceNo = invoice.InvoiceNo ?? "",
                                                loanId = accountId,
                                                token = ayeFinNBFCToken
                                            }
                                        };

                                        var deliveryConfirmationres = await DeliveryConfirmation(deliveryrequest);
                                        if (deliveryConfirmationres != null && deliveryConfirmationres.Status)
                                        {
                                            IsSuccess = deliveryConfirmationres.Status;
                                            nBFCFactoryResponse.Message = deliveryConfirmationres.Message;

                                        }
                                        else
                                        {
                                            IsSuccess = false;
                                            nBFCFactoryResponse.Message = "AyeFinance Limit response not found.";
                                        }

                                    }
                                    else if (ayeFinUpdate != null && (ayeFinUpdate.availablelLimit < transAmount))
                                    {


                                        //RePaymennt Code---------------------------
                                        //var ayefinCommonAPIRequestResponse = await ayeFinanceSCFNBFCHelper.DeliveryConfirmation(accountId, item.NBFCCompanyAPIDetailId, url, ayeFinNBFCToken, deliveryConfirmationthirdpartyreq); ;
                                        //if (ayefinCommonAPIRequestResponse != null)
                                        //{

                                        //}

                                        GRPCRequest<DeliveryConfirmationreq> deliveryrequest = new GRPCRequest<DeliveryConfirmationreq>
                                        {
                                            Request = new DeliveryConfirmationreq
                                            {
                                                amount = Math.Round(transAmount, 2),
                                                orderId = OrderNo,
                                                invoiceNo = invoice.InvoiceNo ?? "",
                                                loanId = accountId,
                                                token = ayeFinNBFCToken
                                            }
                                        };

                                        var deliveryConfirmationres = await DeliveryConfirmation(deliveryrequest);
                                        if (deliveryConfirmationres != null && deliveryConfirmationres.Status)
                                        {
                                            IsSuccess = deliveryConfirmationres.Status;
                                            nBFCFactoryResponse.Message = deliveryConfirmationres.Message;

                                        }
                                        else
                                        {
                                            IsSuccess = false;
                                            nBFCFactoryResponse.Message = "AyeFinance Limit response not found.";
                                        }
                                    }
                                    else
                                    {
                                        nBFCFactoryResponse.Message = "Limit Exceed!!";
                                    }
                                    break;
                            }
                            item.Status = IsSuccess ? LeadNBFCApiConstants.Completed : LeadNBFCApiConstants.Error;
                            if (!IsSuccess)
                                break;
                        }
                        if (nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList != null && !nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.All(x => x.Status == LeadNBFCApiConstants.Completed))
                        {
                            nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.ForEach(x => x.Status = LeadNBFCApiConstants.Error);
                            nBFCFactoryResponse.IsSuccess = false;
                        }
                        else
                            nBFCFactoryResponse.IsSuccess = true;

                        await UpdateNBFCCompanyMaster(nbfcCComapnyAPI);
                    }
                    else
                    {
                        nBFCFactoryResponse.IsSuccess = false;
                        nBFCFactoryResponse.Message = "NBFC Company Api Detail not found.";
                    }
                }
                else
                {
                    nBFCFactoryResponse.IsSuccess = true;
                    nBFCFactoryResponse.Message = "Success";
                }
            }
            return nBFCFactoryResponse;
        }
        public async Task<NBFCFactoryResponse> AyeSCFCOrderInitiate(GRPCRequest<AyeSCFCOrderInitiateDTO> req)
        {
            NBFCFactoryResponse nBFCFactoryResponse = new NBFCFactoryResponse();
            nBFCFactoryResponse.IsSuccess = false;
            //var blackSoilAccount = await _context.BlackSoilAccountDetails.FirstOrDefaultAsync(x => x.LoanAccountId == accountId && x.IsActive && !x.IsDeleted);
            bool AYeFINWebHookResponse = true;
            if (AYeFINWebHookResponse)
            {
                string url = string.Empty;
                List<string> statusNoIN = new List<string> { LeadNBFCApiConstants.Completed, LeadNBFCApiConstants.CompletedWithError };
                await InsertNBFCComapnyAccountTransaction(req.Request.invoiceId, LeadNBFCConstants.AyeFinanceSCF.ToString(), TransactionTypesConstants.OrderPlacement);
                var nbfcCComapnyAPI = await GetNBFCCompanyAPIMaster(req.Request.invoiceId, TransactionTypesConstants.OrderPlacement, TransactionsStatusConstants.Initiate);

                if (nbfcCComapnyAPI != null && !statusNoIN.Contains(nbfcCComapnyAPI.Status)
                                            && nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList != null
                                             && nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.Any()
                                            && nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.Any(y => !statusNoIN.Contains(y.Status)))
                {
                    var IsSuccess = true;
                    foreach (var item in nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.Where(y => !statusNoIN.Contains(y.Status)).OrderBy(x => x.Sequence))
                    {
                        switch (item.Code)
                        {
                            case CompanyApiConstants.AyeFinApplyLoan:

                                var res = await ApplyLoan(new GRPCRequest<ApplyLoanreq>
                                {
                                    Request = new ApplyLoanreq
                                    {
                                        amount = req.Request.OrderAmount,
                                        loanId = req.Request.accountId,
                                        orderId = req.Request.OrderNo,
                                        token = req.Request.Token,
                                        NBFCCompanyAPIDetailId = item.NBFCCompanyAPIDetailId,
                                        APIUrl = item.APIUrl,
                                        invoiceId = req.Request.invoiceId,
                                        AvailableLimit = req.Request.AvailableLimit,
                                        TotalLimit = req.Request.TotalLimit??0,
                                    }

                                });

                                nBFCFactoryResponse.IsSuccess = res.Status;
                                IsSuccess = res.Status;
                                nBFCFactoryResponse.Message = res.Message;
                                break;
                        }

                        item.Status = IsSuccess ? LeadNBFCApiConstants.Completed : LeadNBFCApiConstants.Error;

                    }

                    if (nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList != null && !nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.All(x => x.Status == LeadNBFCApiConstants.Completed))
                    {
                        nbfcCComapnyAPI.NBFCCompanyAPIDetailDTOList.ForEach(x => x.Status = LeadNBFCApiConstants.Error);
                        nBFCFactoryResponse.IsSuccess = false;
                    }
                    else
                        nBFCFactoryResponse.IsSuccess = true;

                    await UpdateNBFCCompanyMaster(nbfcCComapnyAPI);
                }
                else
                    nBFCFactoryResponse.IsSuccess = false;
            }

            return nBFCFactoryResponse;
        }


        public Task<bool> SaveNBFCLoanData(long accountId, string? webhookresposne, NBFCDetailDTO nBFCDetailDTO = null)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> SettlePayment(long blackSoilRepaymentId, long blackSoilLoanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> SettlePaymentLater(GRPCRequest<SettlePaymentJobRequest> request)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> OrderInitiate(long invoiceId, long accountId, double transAmount)
        {
            throw new NotImplementedException();
        }


    }
}
