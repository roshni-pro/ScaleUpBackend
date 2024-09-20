using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.LoanAccountAPI.Helpers.NBFC;
using ScaleUP.Services.LoanAccountAPI.Managers;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountDTO.Loan;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using ScaleUP.Services.LoanAccountModels.Master;
using ScaleUP.Services.LoanAccountModels.Transaction;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static MassTransit.ValidationResultExtensions;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using System.Collections.Generic;
using System.Drawing;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;

namespace ScaleUP.Services.LoanAccountAPI.NBFCFactory.Implementation
{

    public class BlackSoilLoanNBFCService : BaseNBFCService, ILoanNBFCService
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        private readonly LoanAccountApplicationDbContext _context;
        private readonly TransactionSettlementManager _TransactionSettlementManager;
        public BlackSoilLoanNBFCService(LoanAccountApplicationDbContext context, TransactionSettlementManager transactionSettlementManager) : base(context)
        {
            _context = context;
            _TransactionSettlementManager = transactionSettlementManager;
        }

        public async Task<NBFCFactoryResponse> OrderInitiate(long invoiceId, long accountId, double transAmount)
        {
            NBFCFactoryResponse nBFCFactoryResponse = new NBFCFactoryResponse();
            nBFCFactoryResponse.IsSuccess = false;
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilAccount = await _context.BlackSoilAccountDetails.FirstOrDefaultAsync(x => x.LoanAccountId == accountId && x.IsActive && !x.IsDeleted);
            if (blackSoilAccount != null)
            {
                string url = string.Empty;
                List<string> statusNoIN = new List<string> { LeadNBFCApiConstants.Completed, LeadNBFCApiConstants.CompletedWithError };
                await InsertNBFCComapnyAccountTransaction(invoiceId, LeadNBFCConstants.BlackSoil.ToString(), TransactionTypesConstants.OrderPlacement);
                var nbfcCComapnyAPI = await GetNBFCCompanyAPIMaster(invoiceId, TransactionTypesConstants.OrderPlacement, TransactionsStatusConstants.Initiate);

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
                            case CompanyApiConstants.BlackSoilWithdrawalRequest:
                                url = item.APIUrl.Replace("{{APPLICATION_ID}}", blackSoilAccount.ApplicationId.ToString());
                                var blackSoilCommonAPIRequestResponse = await blackSoilNBFCHelper.CreateWithdrawalRequest(item.NBFCCompanyAPIDetailId, url, item.TAPIKey, item.TAPISecretKey);
                                if (blackSoilCommonAPIRequestResponse != null)
                                {
                                    if (blackSoilCommonAPIRequestResponse.IsSuccess)
                                    {
                                        var withdrawalres = JsonConvert.DeserializeObject<WithdrawalResponseDTO>(blackSoilCommonAPIRequestResponse.Response);
                                        BlackSoilAccountTransaction blackSoilAccountTransaction = new BlackSoilAccountTransaction
                                        {
                                            LoanInvoiceId = invoiceId,
                                            WithdrawalId = withdrawalres.id,
                                            WithdrawalUrl = withdrawalres.update_url,
                                            Status = withdrawalres.status,
                                            TopUpNumber = withdrawalres.invoice_id,
                                            IsActive = true,
                                            IsDeleted = false
                                        };

                                        _context.BlackSoilAccountTransactions.Add(blackSoilAccountTransaction);
                                        IsSuccess = true;
                                    }
                                    else
                                    {
                                        nBFCFactoryResponse.Message = blackSoilCommonAPIRequestResponse.Response;
                                        IsSuccess = false;
                                    }

                                    _context.BlackSoilCommonAPIRequestResponses.Add(blackSoilCommonAPIRequestResponse);
                                    if (_context.SaveChanges() > 0)
                                    {
                                        nBFCFactoryResponse.IsSuccess = true;
                                    }
                                    else
                                    {
                                        IsSuccess = false;
                                        nBFCFactoryResponse.Message = "Data not save.";
                                    }
                                }
                                else
                                {
                                    IsSuccess = false;
                                    nBFCFactoryResponse.Message = "BlackSoil response not found.";
                                }
                                break;
                            case CompanyApiConstants.BlackSoilGetAvailableCreditLimit:
                                url = item.APIUrl.Replace("{{BUSINESS_ID}}", blackSoilAccount.BusinessId.ToString());
                                var RequestResponses = await blackSoilNBFCHelper.BlackSoilGetAvailableCreditLimit(item.NBFCCompanyAPIDetailId, url, item.TAPIKey, item.TAPISecretKey);
                                if (RequestResponses != null)
                                {
                                    if (RequestResponses.IsSuccess)
                                    {
                                        IsSuccess = true;
                                        var blackSoilCreditLine = JsonConvert.DeserializeObject<BlackSoilAvailableCreditLimit>(RequestResponses.Response);

                                        if (blackSoilCreditLine != null && !string.IsNullOrEmpty(blackSoilCreditLine.available_limit))
                                        {
                                            string limitstr = string.IsNullOrEmpty(blackSoilCreditLine.available_limit) != null ? blackSoilCreditLine.available_limit : "0";

                                            if (!string.IsNullOrEmpty(blackSoilCreditLine.available_limit))
                                            {
                                                double limit;
                                                var res = double.TryParse(limitstr, out limit);
                                                nBFCFactoryResponse.Limit = limit;
                                                if (res && limit > 0 && (limit - transAmount) >= 0)
                                                {
                                                    IsSuccess = true;
                                                }
                                                else
                                                {
                                                    IsSuccess = false;
                                                    nBFCFactoryResponse.Message = "Credit limit is less then transaction amount.";
                                                }
                                            }
                                            else
                                            {
                                                IsSuccess = false;
                                                nBFCFactoryResponse.Message = "Limit not available.";
                                            }
                                        }
                                        else
                                        {
                                            IsSuccess = false;
                                            nBFCFactoryResponse.Message = "Limit not available.";
                                        }
                                    }
                                    else
                                    {
                                        IsSuccess = false;
                                        nBFCFactoryResponse.Message = RequestResponses.Response;
                                    }

                                    _context.BlackSoilCommonAPIRequestResponses.AddRange(RequestResponses);
                                    if (_context.SaveChanges() > 0)
                                    {
                                        nBFCFactoryResponse.IsSuccess = true;
                                        nBFCFactoryResponse.Message = "Success";
                                    }
                                }
                                else
                                {
                                    IsSuccess = false;
                                    nBFCFactoryResponse.Message = "BlackSoil Limit response not found.";
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
                    nBFCFactoryResponse.IsSuccess = false;
            }

            return nBFCFactoryResponse;
        }

        public async Task<NBFCFactoryResponse> OrderCaptured(long invoiceId, long accountId, double transAmount, bool status, string? OrderNo, string? ayeFinNBFCToken = "")
        {
            string url = string.Empty;
            List<string> statusNoIN = new List<string> { LeadNBFCApiConstants.Completed, LeadNBFCApiConstants.CompletedWithError };

            NBFCFactoryResponse nBFCFactoryResponse = new NBFCFactoryResponse();
            nBFCFactoryResponse.IsSuccess = false;
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilAccount = await _context.BlackSoilAccountDetails.Where(x => x.LoanAccountId == accountId && x.IsActive && !x.IsDeleted).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            var blackSoilAccountTransaction = await _context.BlackSoilAccountTransactions.FirstOrDefaultAsync(x => x.LoanInvoiceId == invoiceId && x.IsActive && !x.IsDeleted);
            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId);
            if (blackSoilAccount != null && blackSoilAccountTransaction != null && invoice != null)
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
                                case CompanyApiConstants.BlackSoilGetAvailableCreditLimit:
                                    url = item.APIUrl.Replace("{{BUSINESS_ID}}", blackSoilAccount.BusinessId.ToString());
                                    var RequestResponses = await blackSoilNBFCHelper.BlackSoilGetAvailableCreditLimit(item.NBFCCompanyAPIDetailId, url, item.TAPIKey, item.TAPISecretKey);
                                    if (RequestResponses != null)
                                    {
                                        if (RequestResponses.IsSuccess)
                                        {
                                            IsSuccess = true;
                                            var blackSoilCreditLine = JsonConvert.DeserializeObject<BlackSoilAvailableCreditLimit>(RequestResponses.Response);

                                            if (blackSoilCreditLine != null && !string.IsNullOrEmpty(blackSoilCreditLine.available_limit))
                                            {
                                                string limitstr = string.IsNullOrEmpty(blackSoilCreditLine.available_limit) != null ? blackSoilCreditLine.available_limit : "0";

                                                if (!string.IsNullOrEmpty(blackSoilCreditLine.available_limit))
                                                {
                                                    double limit;
                                                    var res = double.TryParse(limitstr, out limit);
                                                    nBFCFactoryResponse.Limit = limit;
                                                    if (res && limit > 0 && (limit - transAmount) >= 0)
                                                    {
                                                        IsSuccess = true;
                                                    }
                                                    else
                                                    {
                                                        IsSuccess = false;
                                                        nBFCFactoryResponse.Message = "Credit limit is less then transaction amount.";
                                                    }
                                                }
                                                else
                                                {
                                                    IsSuccess = false;
                                                    nBFCFactoryResponse.Message = "Limit not available.";
                                                }
                                            }
                                            else
                                            {
                                                IsSuccess = false;
                                                nBFCFactoryResponse.Message = "Limit not available.";
                                            }
                                        }
                                        else
                                        {
                                            IsSuccess = false;
                                            nBFCFactoryResponse.Message = RequestResponses.Response;
                                        }

                                        _context.BlackSoilCommonAPIRequestResponses.AddRange(RequestResponses);
                                        if (_context.SaveChanges() > 0)
                                        {
                                            nBFCFactoryResponse.IsSuccess = true;
                                            nBFCFactoryResponse.Message = "Success";
                                        }
                                    }
                                    else
                                    {
                                        IsSuccess = false;
                                        nBFCFactoryResponse.Message = "BlackSoil Limit response not found.";
                                    }

                                    break;
                                case CompanyApiConstants.BlackSoilWithdrawalRequestUpdateDoc:
                                    BlackSoilWithdrawalRequestInput blackSoilWithdrawalRequestInput = new BlackSoilWithdrawalRequestInput
                                    {
                                        amount = Math.Round(transAmount, 2),
                                        disbursed_amount = Math.Round(transAmount, 2),
                                        file = invoice.InvoicePdfUrl ?? "",
                                        invoice_date = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd"),
                                        invoice_number = invoice.InvoiceNo ?? "",
                                        NBFCCompanyApiDetailId = item.NBFCCompanyAPIDetailId
                                    };

                                    url = item.APIUrl.Replace("{{APPLICATION_ID}}", blackSoilAccount.ApplicationId.ToString());
                                    url = url.Replace("{{ID}}", blackSoilAccountTransaction.WithdrawalId.ToString());

                                    var blackSoilCommonAPIRequestResponse = await blackSoilNBFCHelper.UploadWithdrawalRequestDocument(blackSoilWithdrawalRequestInput, url, item.TAPIKey, item.TAPISecretKey);
                                    if (blackSoilCommonAPIRequestResponse != null)
                                    {
                                        if (blackSoilCommonAPIRequestResponse.IsSuccess)
                                        {
                                            IsSuccess = true;
                                            var withdrawalInvoiceResponse = JsonConvert.DeserializeObject<BlackSoilUploadWithdrawalInvoiceResponse>(blackSoilCommonAPIRequestResponse.Response);

                                            if (withdrawalInvoiceResponse != null)
                                            {
                                                blackSoilAccountTransaction.InvoiceId = withdrawalInvoiceResponse.id.Value;
                                                blackSoilAccountTransaction.InvoiceUrl = withdrawalInvoiceResponse.update_url;

                                                _context.Entry(blackSoilAccountTransaction).State = EntityState.Modified;
                                            }
                                        }
                                        else
                                        {
                                            IsSuccess = false;
                                            nBFCFactoryResponse.Message = blackSoilCommonAPIRequestResponse.Response;
                                        }

                                        _context.BlackSoilCommonAPIRequestResponses.Add(blackSoilCommonAPIRequestResponse);
                                        if (_context.SaveChanges() > 0)
                                        {
                                            nBFCFactoryResponse.IsSuccess = true;
                                        }
                                        else
                                        {
                                            IsSuccess = false;
                                            nBFCFactoryResponse.Message = "Data not save.";
                                        }
                                    }
                                    else
                                    {
                                        IsSuccess = false;
                                        nBFCFactoryResponse.Message = "BlackSoil response not found.";
                                    }
                                    break;
                                case CompanyApiConstants.BlackSoilBulkInvoicesApprove:
                                    nBFCFactoryResponse.IsSuccess = true;
                                    if (!string.IsNullOrEmpty(blackSoilAccountTransaction.InvoiceId.ToString()))
                                    {
                                        var blackSoilCommonAPIRequestResponseInvoice = await blackSoilNBFCHelper.BulkInvoicesApprove(blackSoilAccountTransaction.InvoiceId.ToString(), item.NBFCCompanyAPIDetailId, item.APIUrl, item.TAPIKey, item.TAPISecretKey);
                                        if (blackSoilCommonAPIRequestResponseInvoice != null)
                                        {
                                            _context.BlackSoilCommonAPIRequestResponses.Add(blackSoilCommonAPIRequestResponseInvoice);
                                            if (_context.SaveChanges() > 0)
                                            {
                                                nBFCFactoryResponse.IsSuccess = true;
                                            }
                                        }
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

        public async Task<bool> SaveNBFCLoanData(long accountId, string? webhookresposne, NBFCDetailDTO nbfcSpecificData = null)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(webhookresposne) || nbfcSpecificData != null)
            {
                var account = await _context.BlackSoilAccountDetails.FirstOrDefaultAsync(x => x.LoanAccountId == accountId && x.IsActive && !x.IsDeleted);
                if (account == null)
                {

                    if (!string.IsNullOrEmpty(webhookresposne))
                    {
                        var blackSoilAccount = JsonConvert.DeserializeObject<BlackSoilLineActivated>(webhookresposne);
                        if (blackSoilAccount != null)
                        {
                            account = new BlackSoilAccountDetail
                            {
                                IsActive = true,
                                IsDeleted = false,
                                ApplicationId = Convert.ToInt64(blackSoilAccount.data.id),
                                BusinessId = blackSoilAccount.data.business.id.Value,
                                LoanAccountId = accountId,
                                BlackSoilLoanId = blackSoilAccount.data.loan_data.Value,
                                BusinessCode = blackSoilAccount.data.identifiers.business_id
                            };
                            _context.BlackSoilAccountDetails.Add(account);
                            result = await _context.SaveChangesAsync() > 0;
                        }
                    }
                    else
                    {
                        account = new BlackSoilAccountDetail
                        {
                            IsActive = true,
                            IsDeleted = false,
                            ApplicationId = Convert.ToInt64(nbfcSpecificData.ApplicationId),
                            BusinessId = Convert.ToInt64(nbfcSpecificData.BusinessId),
                            LoanAccountId = accountId,
                            BlackSoilLoanId = 0,
                            BusinessCode = nbfcSpecificData.BusinessCode
                        };
                        _context.BlackSoilAccountDetails.Add(account);
                        result = await _context.SaveChangesAsync() > 0;

                    }

                }
            }
            return result;
        }

        public async Task<double> GetAvailableCreditLimit(long accountId)
        {
            double limit = 0;
            NBFCFactoryResponse nBFCFactoryResponse = new NBFCFactoryResponse();
            nBFCFactoryResponse.IsSuccess = false;
            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
            var blackSoilAccount = await _context.BlackSoilAccountDetails.Where(x => x.LoanAccountId == accountId && x.IsActive && !x.IsDeleted).OrderByDescending(x => x.Id).FirstOrDefaultAsync();

            if (blackSoilAccount != null)
            {
                var nbfcCComapnyAPI = await GetAvailableCreditLimit(CompanyApiConstants.BlackSoilGetAvailableCreditLimit);
                if (nbfcCComapnyAPI != null)
                {
                    var url = nbfcCComapnyAPI.APIUrl.Replace("{{BUSINESS_ID}}", blackSoilAccount.BusinessId.ToString());
                    var blackSoilCommonAPIRequestResponse = await blackSoilNBFCHelper.BlackSoilGetAvailableCreditLimit(nbfcCComapnyAPI.NBFCCompanyAPIDetailId, url, nbfcCComapnyAPI.TAPIKey, nbfcCComapnyAPI.TAPISecretKey);
                    if (blackSoilCommonAPIRequestResponse != null && blackSoilCommonAPIRequestResponse.IsSuccess)
                    {
                        var blackSoilCreditLine = JsonConvert.DeserializeObject<BlackSoilAvailableCreditLimit>(blackSoilCommonAPIRequestResponse.Response);

                        if (blackSoilCreditLine != null && !string.IsNullOrEmpty(blackSoilCreditLine.available_limit))
                        {
                            string limitstr = string.IsNullOrEmpty(blackSoilCreditLine.available_limit) != null ? blackSoilCreditLine.available_limit : "0";

                            if (!string.IsNullOrEmpty(blackSoilCreditLine.available_limit))
                            {
                                var res = double.TryParse(limitstr, out limit);
                            }
                        }
                        _context.BlackSoilCommonAPIRequestResponses.Add(blackSoilCommonAPIRequestResponse);
                        _context.SaveChanges();
                    }
                }

            }

            return limit;
        }


        public async Task<NBFCFactoryResponse> SettlePayment(long blackSoilRepaymentId, long blackSoilLoanAccountId)
        {
            BlackSoilRepaymentDc blackSoilRepayment = null;
            BlackSoilLoanAccountExpandDc accountDetails = null;
            var api = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilLoanRepayment && x.IsActive && !x.IsDeleted);
            var repayment = await _context.LoanAccountRepayments.FirstOrDefaultAsync(x => x.ThirdPartyPaymentId == blackSoilRepaymentId.ToString() && x.IsActive && !x.IsDeleted);
            if (repayment != null)
            {
                return new NBFCFactoryResponse
                {
                    IsSuccess = false,
                    Message = $"Repayment {blackSoilRepaymentId} already exist, either processed or somthing went wrong"
                };
            }

            if (api != null)
            {
                string url = api.APIUrl;
                url = url.Replace("{{accountid}}", blackSoilLoanAccountId.ToString());
                url = url.Replace("{{repaymentid}}", blackSoilRepaymentId.ToString());
                BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();

                BlackSoilCommonAPIRequestResponse response = await blackSoilNBFCHelper.GetLoanRepayment(url, api.TAPIKey, api.TAPISecretKey, 0);
                _context.BlackSoilCommonAPIRequestResponses.Add(response);
                await _context.SaveChangesAsync();
                if (response != null && response.IsSuccess)
                {
                    blackSoilRepayment = JsonConvert.DeserializeObject<BlackSoilRepaymentDc>(response.Response);
                    if (blackSoilRepayment != null)
                    {
                        var loanAccountDetailAPI = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilLoanAccountDetail && x.IsActive && !x.IsDeleted);
                        if (loanAccountDetailAPI != null)
                        {

                            var data = await _TransactionSettlementManager.BlackSoilPaymentCredit(blackSoilRepayment);

                        }
                        else
                        {
                            return new NBFCFactoryResponse
                            {
                                IsSuccess = false,
                                Message = $"API Configuration not found for api for {CompanyApiConstants.BlackSoilLoanAccountDetail}"
                            };
                        }


                    }
                }

            }
            else
            {
                return new NBFCFactoryResponse
                {
                    IsSuccess = false,
                    Message = $"API Configuration not found for api for {CompanyApiConstants.BlackSoilLoanRepayment}"
                };
            }


            return null;
        }

        public async Task<NBFCFactoryResponse> SettlePaymentLater(GRPCRequest<SettlePaymentJobRequest> request)
        {

            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddMinutes(-15), INDIAN_ZONE);  //DateTime.Now.AddMinutes(-15);

            #region Use For ReProcess Payment entry from Day one TO till today
            long loanAccountRepaymentsId = 0;
            if (request != null && request.Request != null)
            { loanAccountRepaymentsId = request.Request.loanAccountRepaymentsId ?? 0; }

            var refreshList = await _context.LoanAccountRepaymentRefreshs.Where(y => y.IsActive && !y.IsDeleted && y.IsRunning && y.IsError == false).Select(x => x.LoanAccountId).ToListAsync();
            //&& !refreshList.Contains(l.LoanAccountId) 

            List<LoanAccountRepayment> list = new List<LoanAccountRepayment>();
            if (request.Request != null && request.Request.IsRunningManually == true)
            {
                //list = (from l in _context.LoanAccountRepayments.Where(x => x.Status != BlackSoilRepaymentConstants.Settled && x.IsActive && !x.IsDeleted
                //            && x.Created < currentTime && x.IsRunning == false)
                //        join r in _context.LoanAccountRepaymentRefreshs.Where(y => y.IsActive && y.IsDeleted == false
                //        && y.IsRunning == false && y.IsError == false
                //        ) on l.LoanAccountId equals r.LoanAccountId into lft
                //        from r in lft.DefaultIfEmpty()
                //        where (loanAccountRepaymentsId == 0 || l.Id == loanAccountRepaymentsId)
                //        && (r.IsRunning == false || r.IsRunning == null)
                //        && (r.IsRunning == false || r.IsError == null)
                //        select l).OrderBy(x => x.PaymentDate).ToList();
                list = _context.LoanAccountRepayments.Where(x => x.Status != BlackSoilRepaymentConstants.Settled && x.IsActive && !x.IsDeleted && x.Created < currentTime
                && (loanAccountRepaymentsId == 0 || x.Id == loanAccountRepaymentsId) && x.IsRunning == false).OrderBy(x => x.PaymentDate).ToList();
            }
            else
            {
                list = _context.LoanAccountRepayments.Where(x => x.Status != BlackSoilRepaymentConstants.Settled && x.IsActive && !x.IsDeleted && x.Created < currentTime
                && !refreshList.Contains(x.LoanAccountId) && x.IsRunning == false).OrderBy(x => x.PaymentDate).ToList();
            }

            #endregion    

            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {

                    BlackSoilRepaymentDc blackSoilRepayment = null;
                    BlackSoilLoanAccountExpandDc accountDetails = null;
                    var api = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilLoanRepayment && x.IsActive && !x.IsDeleted);
                    var repayment = item;

                    #region Use For ReProcess Payment entry from Day one TO till today
                    repayment.IsRunning = true;
                    _context.Entry(repayment).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    #endregion


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

                                        var PendingTransactionsList = await _TransactionSettlementManager.GetOutstandingTransactionsList("", item.LoanAccountId, blackSoilRepayment.settlement_date);
                                        if (PendingTransactionsList != null && PendingTransactionsList.Any())
                                        {
                                            var data = await _TransactionSettlementManager.SettlePaymentTransaction(PendingTransactionsList, blackSoilRepayment, accountDetails, item, request);

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

                    #region Use For ReProcess Payment entry from Day one TO till today
                    repayment.IsRunning = false;
                    _context.Entry(repayment).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    #endregion


                }
            }

            return new NBFCFactoryResponse
            {
                IsSuccess = true,
                Limit = 0,
                Message = ""
            };

        }

        public async Task<double> CalculatePerDayInterest(double interest)
        {
            //DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365
            return (interest / 360);
        }

        public async Task<List<InvoiceNBFCReqRes>> GetInvoiceNBFCReqRes(List<long> ApiDetailIds)
        {
            List<InvoiceNBFCReqRes> invoiceNBFCReqRes = await _context.BlackSoilCommonAPIRequestResponses.Where(x => x.NBFCCompanyApiDetailId.HasValue && ApiDetailIds.Contains(x.NBFCCompanyApiDetailId.Value))
           .Select(x => new InvoiceNBFCReqRes
           {
               URL = x.URL,
               Request = x.Request,
               Response = x.Response,
               CreatedDate = x.Created
           }).ToListAsync();
            return invoiceNBFCReqRes;
        }

        public async Task<bool> IsInvoiceInitiated(long invoiceId)
        {
            var res = await _context.BlackSoilAccountTransactions.AnyAsync(x => x.LoanInvoiceId == invoiceId && x.IsActive && !x.IsDeleted);
            return res;
        }


        public async Task<NBFCFactoryResponse> InsertSkippedPayments(long loanAccId)
        {
            //12/03/2024
            long blackSoilLoanId; long LoanAccountId = 0;
            long blackSoilRepaymentId = 0;
            bool recordInsert = false;

            var blackSoilAccountDetailList = await _context.BlackSoilAccountDetails.Where(x => x.BlackSoilLoanId != 0 && x.IsActive && !x.IsDeleted
            && (loanAccId == 0 || x.LoanAccountId == loanAccId)).ToListAsync();

            if (blackSoilAccountDetailList != null && blackSoilAccountDetailList.Count > 0)
            {
                foreach (var blackSoilAccountDetail in blackSoilAccountDetailList)
                {
                    blackSoilLoanId = blackSoilAccountDetail.BlackSoilLoanId;
                    LoanAccountId = blackSoilAccountDetail.LoanAccountId;

                    BlackSoilRepaymentList blackSoilRepaymentList = null;

                    var api = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilListRepayments && x.IsActive && !x.IsDeleted);
                    ////var repayment = await _context.LoanAccountRepayments.FirstOrDefaultAsync(x => x.ThirdPartyPaymentId == blackSoilRepaymentId.ToString() && x.IsActive && !x.IsDeleted);
                    //var repayment = await _context.LoanAccountRepayments.FirstOrDefaultAsync(x => x.ThirdPartyLoanAccountId == blackSoilLoanId.ToString() && x.IsActive && !x.IsDeleted);
                    //if (repayment != null)
                    //{
                    //    return new NBFCFactoryResponse
                    //    {
                    //        IsSuccess = false,
                    //        Message = $"Repayment {blackSoilRepaymentId} already exist, either processed or somthing went wrong"
                    //    };
                    //}

                    if (api != null)
                    {
                        string url = api.APIUrl;
                        url = url.Replace("{{LOAN_ACCOUNT_ID}}", blackSoilLoanId.ToString());
                        BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
                        List<BlackSoilRepaymentListResult> paymentList = new List<BlackSoilRepaymentListResult>();

                        while (!string.IsNullOrEmpty(url))
                        {

                            BlackSoilCommonAPIRequestResponse response = await blackSoilNBFCHelper.GetLoanRepaymentLIST(url, api.TAPIKey, api.TAPISecretKey);
                            _context.BlackSoilCommonAPIRequestResponses.Add(response);
                            await _context.SaveChangesAsync();
                            if (response != null && response.IsSuccess)
                            {
                                blackSoilRepaymentList = JsonConvert.DeserializeObject<BlackSoilRepaymentList>(response.Response);
                                if (blackSoilRepaymentList != null)
                                {
                                    if (blackSoilRepaymentList != null && blackSoilRepaymentList.results != null && blackSoilRepaymentList.results.Any())
                                    {
                                        paymentList.AddRange(blackSoilRepaymentList.results);
                                        url = blackSoilRepaymentList.next;
                                        //List<LoanAccountRepayment> LoanAccountRepaymentList = new List<LoanAccountRepayment>();

                                        //foreach (var blackSoilRepayment in blackSoilRepaymentList.results)
                                        //{
                                        //    //SettlePayment
                                        //    var data = await _TransactionSettlementManager.BlackSoilPaymentCredit(blackSoilRepayment);


                                        //    blackSoilRepaymentId = blackSoilRepayment.id;

                                        //    var repayment = await _context.LoanAccountRepayments.FirstOrDefaultAsync(x => x.ThirdPartyPaymentId == blackSoilRepaymentId.ToString() && x.IsActive && !x.IsDeleted);
                                        //    if (repayment == null)
                                        //    {
                                        //        LoanAccountRepayment loanAccountRepayment = new LoanAccountRepayment
                                        //        {
                                        //            ThirdPartyPaymentId = blackSoilRepaymentId.ToString(),
                                        //            LoanAccountId = LoanAccountId,
                                        //            ThirdPartyLoanAccountId = blackSoilLoanId.ToString(),

                                        //            PaymentMode = blackSoilRepayment.payment_mode,
                                        //            BankRefNo = blackSoilRepayment.ref_no,
                                        //            ThirdPartyTxnId = blackSoilRepayment.txn_id,
                                        //            Status = blackSoilRepayment.status, //???????????    ==>> Pending

                                        //            //???????????
                                        //            Amount = Convert.ToDouble(blackSoilRepayment.amount),
                                        //            PaymentDate = blackSoilRepayment.settlement_date,
                                        //            TotalAmount = Convert.ToDouble(blackSoilRepayment.actual_amount),
                                        //            InterestAmount = Convert.ToDouble(blackSoilRepayment.interest),
                                        //            ProcessingFees = Convert.ToDouble(blackSoilRepayment.pf),
                                        //            PenalInterest = Convert.ToDouble(blackSoilRepayment.penal_interest),
                                        //            OverdueInterest = Convert.ToDouble(blackSoilRepayment.overdue_interest),
                                        //            PrincipalAmount = Convert.ToDouble(blackSoilRepayment.principal),
                                        //            ExtraPaymentAmount = Convert.ToDouble(blackSoilRepayment.extra_payment),

                                        //            RemainingExtraPaymentAmount = 0,
                                        //            RemainingInterestAmount = 0,
                                        //            RemainingOverdueInterest = 0,
                                        //            RemainingPenalInterest = 0,
                                        //            RemainingPrincipalAmount = 0,
                                        //            RemainingProcessingFees = 0,

                                        //            IsActive = true,
                                        //            IsDeleted = false,
                                        //        };
                                        //        LoanAccountRepaymentList.Add(loanAccountRepayment);
                                        //    }
                                        //}

                                        //if (LoanAccountRepaymentList != null)
                                        //{
                                        //    await _context.LoanAccountRepayments.AddRangeAsync(LoanAccountRepaymentList);
                                        //    await _context.SaveChangesAsync();
                                        //    recordInsert = true;
                                        //}
                                    }
                                    else
                                    { url = null; }
                                }

                            }
                        }

                        if (paymentList != null && paymentList.Any())
                        {
                            var idList = _context.LoanAccountRepayments
                                                .Where(x => x.LoanAccountId == LoanAccountId && !x.IsDeleted && x.IsActive)
                                                .Select(x => x.ThirdPartyPaymentId)
                                                .ToList();
                            var blackSoilRecalculateAccounting = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilRecalculateAccounting && !x.IsDeleted && x.IsActive);
                            if (blackSoilRecalculateAccounting != null)
                            {
                                string url2 = blackSoilRecalculateAccounting.APIUrl.Replace("{{loan_account_id}}", blackSoilLoanId.ToString());
                                BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();
                                var ress = await helper.BlackSoilRecalculateAccounting(url2, blackSoilRecalculateAccounting.TAPIKey, blackSoilRecalculateAccounting.TAPISecretKey);
                                _context.BlackSoilCommonAPIRequestResponses.Add(ress);
                                await _context.SaveChangesAsync();

                                if (ress.IsSuccess)
                                {
                                    foreach (var item in paymentList)
                                    {
                                        if (idList == null || !idList.Any(x => int.Parse(x) == item.id))
                                        {
                                            idList.Add(item.id.ToString());
                                            await SettlePayment(item.id, item.loan_account);

                                            recordInsert = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var item in paymentList)
                                {
                                    if (idList == null || !idList.Any(x => int.Parse(x) == item.id))
                                    {
                                        idList.Add(item.id.ToString());
                                        await SettlePayment(item.id, item.loan_account);

                                        recordInsert = true;
                                    }
                                }
                            }

                        }
                    }

                }
            }


            return new NBFCFactoryResponse
            {
                IsSuccess = recordInsert,
                Limit = 0,
                Message = recordInsert == true ? "Save Records" : ""
            };
        }

        public Task<ResultViewModel<List<LoanRepaymentScheduleDetailDc>>> LoanRepaymentScheduleDetails(long loanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long loanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> request)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> AyeSCFCOrderInitiate(GRPCRequest<AyeSCFCOrderInitiateDTO> req)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request)
        {
            throw new NotImplementedException();
        }
    }
}
