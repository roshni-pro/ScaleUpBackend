using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.WebHook;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using static MassTransit.ValidationResultExtensions;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using ScaleUP.Services.LoanAccountDTO.Loan;

namespace ScaleUP.Services.LoanAccountAPI.Consumer
{
    public class AccountDisbursementConsumer : IConsumer<AccountDisbursementEvent>
    {
        private readonly LoanAccountApplicationDbContext _context;
        public AccountDisbursementConsumer(LoanAccountApplicationDbContext context)
        {
            _context = context;
        }
        public async Task Consume(ConsumeContext<AccountDisbursementEvent> context)
        {
            var anchorAccount = await _context.LoanAccountCompanyLead.Where(x => x.LoanAccountId == context.Message.AccountId && x.UserUniqueCode != null && x.UserUniqueCode != "").ToListAsync();
            if (anchorAccount != null && anchorAccount.Any())
            {
                var loan = await _context.LoanAccounts.Where(x => x.Id == context.Message.AccountId).Select(x => new { LeadId = x.LeadId, CreditLimitAmount = x.LoanAccountCredits.CreditLimitAmount }).FirstOrDefaultAsync();
                var companyIds = anchorAccount.Select(x => x.CompanyId).ToList();

                var anchorCompanies= await _context.CompanyAPIs.Where(x => companyIds.Contains(x.CompanyId) && x.Code == "Disbursement").ToListAsync();

                if (anchorCompanies != null && anchorCompanies.Any())
                {
                    foreach (var account in anchorAccount)
                    {
                        var companyApi = anchorCompanies.FirstOrDefault(x => x.CompanyId == account.CompanyId);
                        if (companyApi != null)
                        {
                            AccountDisbursementNotify disbursementNotify = new AccountDisbursementNotify
                            {
                                AccountId = account.LoanAccountId,
                                CustomerUniqueCode = account.UserUniqueCode,
                                LeadId = loan.LeadId,
                                Status = true,
                                Message = "Loan disburse",
                                CreditLimit = loan.CreditLimitAmount
                            };
                            //var response = await CallAnchorAPI("https://uat.shopkirana.in//api/ScaleUpIntegration/UpdateCustomerAccount", "", "", "", disbursementNotify);
                            var response = await CallAnchorAPI(companyApi.APIUrl, companyApi.Authtype, companyApi.APIKey, companyApi.APISecret, disbursementNotify);
                            var reqmsg = JsonConvert.SerializeObject(disbursementNotify);
                            _context.ThirdPartyRequest.Add(new LoanAccountModels.Transaction.ThirdPartyRequest
                            {
                                CompanyId= companyApi.CompanyId,
                                CompanyAPI= companyApi.Id,
                                IsActive= true,
                                IsDeleted=false,
                                IsError= response.StatusCode==500,
                                Request= reqmsg,
                                Response= JsonConvert.SerializeObject(response)                              
                            });
                        }

                    }

                    _context.SaveChanges();
                }
            }
        }


        public async Task<DisbursementAnchorResponse> CallAnchorAPI(string ApiUrl, string authtype, string key, string secretKey, AccountDisbursementNotify disbursementNotify)
        {
            DisbursementAnchorResponse disbursementAnchorResponse = new DisbursementAnchorResponse();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                    {
                        string authenticationString = "";
                        if (authtype == "Basic")
                        {
                            authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(key + ":" + secretKey));
                        }
                        else if (authtype == "Authorization")
                        {
                            authenticationString = key + " " + secretKey;
                        }

                        if (!string.IsNullOrEmpty(authenticationString))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue(authtype, authenticationString);
                        }

                        var reqmsg = JsonConvert.SerializeObject(disbursementNotify);

                        request.Content = new StringContent(reqmsg);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        disbursementAnchorResponse.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            disbursementAnchorResponse.Response = jsonString;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            disbursementAnchorResponse.Response = jsonString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                disbursementAnchorResponse.StatusCode = 500;
                disbursementAnchorResponse.Response = ex.ToString();
            }
            return disbursementAnchorResponse;
        }
    }
}
