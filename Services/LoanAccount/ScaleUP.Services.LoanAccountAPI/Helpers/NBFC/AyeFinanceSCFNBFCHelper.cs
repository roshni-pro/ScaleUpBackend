using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC.AyeFinance;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ScaleUP.Services.LoanAccountAPI.Helpers.NBFC
{
    public class AyeFinanceSCFNBFCHelper
    {
        public async Task<AyeFinanceSCFCommonAPIRequestResponse> ApplyLoan(long LoanAccountId, long NBFCCompanyApiDetailId, string apiUrl, string token, ApplyLoanthirdpartyreq req)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LoanAccountId = LoanAccountId,
                NBFCCompanyApiDetailId = NBFCCompanyApiDetailId,
                URL = apiUrl,
                IsActive = true,
                IsDeleted = false
            };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                    {
                        result.Request = JsonConvert.SerializeObject(req);

                        request.Content = new StringContent(result.Request, Encoding.UTF8, "application/json");

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.SendAsync(request);
                        result.StatusCode = (int)response.StatusCode;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result.Response = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.IsSuccess = true;
                        }
                        else
                        {
                            result.Response = (await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = "An error occurred while generating the token: " + ex.Message;
            }

            return result;


        }

        public async Task<AyeFinanceSCFCommonAPIRequestResponse> CheckTotalAndAvailableLimit(long LoanAccountId, long NBFCCompanyApiDetailId, string apiUrl, string token, CheckCreditLineReqDc req)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LoanAccountId = LoanAccountId,
                NBFCCompanyApiDetailId = NBFCCompanyApiDetailId,
                URL = apiUrl,
                IsActive = true,
                IsDeleted = false
            };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                    {
                        result.Request = JsonConvert.SerializeObject(req);

                        request.Content = new StringContent(result.Request, Encoding.UTF8, "application/json");

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.SendAsync(request);
                        result.StatusCode = (int)response.StatusCode;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result.Response = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.IsSuccess = true;
                        }
                        else
                        {
                            result.Response = (await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = "An error occurred while generating the token: " + ex.Message;
            }

            return result;


        }

        public async Task<AyeFinanceSCFCommonAPIRequestResponse> DeliveryConfirmation(long LoanAccountId, long NBFCCompanyApiDetailId, string apiUrl, string token, DeliveryConfirmationthirdpartyreq req)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LoanAccountId = LoanAccountId,
                NBFCCompanyApiDetailId = NBFCCompanyApiDetailId,
                URL = apiUrl,
                IsActive = true,
                IsDeleted = false
            };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                    {
                        result.Request = JsonConvert.SerializeObject(req);

                        request.Content = new StringContent(result.Request, Encoding.UTF8, "application/json");

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.SendAsync(request);
                        result.StatusCode = (int)response.StatusCode;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result.Response = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.IsSuccess = true;
                        }
                        else
                        {
                            result.Response = (await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = "An error occurred while generating the token: " + ex.Message;
            }

            return result;


        }

        public async Task<AyeFinanceSCFCommonAPIRequestResponse> Repayment(long LoanAccountId, long NBFCCompanyApiDetailId, string apiUrl, string token, RepaymentThirdpartyreq req)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LoanAccountId = LoanAccountId,
                NBFCCompanyApiDetailId = NBFCCompanyApiDetailId,
                URL = apiUrl,
                IsActive = true,
                IsDeleted = false
            };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                    {
                        result.Request = JsonConvert.SerializeObject(req);

                        request.Content = new StringContent(result.Request, Encoding.UTF8, "application/json");

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.SendAsync(request);
                        result.StatusCode = (int)response.StatusCode;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result.Response = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.IsSuccess = true;
                        }
                        else
                        {
                            result.Response = (await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = "An error occurred while generating the token: " + ex.Message;
            }

            return result;


        }
        public async Task<AyeFinanceSCFCommonAPIRequestResponse> CancelTransaction(long LoanAccountId, long NBFCCompanyApiDetailId, string apiUrl, string token, CancelTxnThirdPartyRed req)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LoanAccountId = LoanAccountId,
                NBFCCompanyApiDetailId = NBFCCompanyApiDetailId,
                URL = apiUrl,
                IsActive = true,
                IsDeleted = false
            };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                    {
                        result.Request = JsonConvert.SerializeObject(req);

                        request.Content = new StringContent(result.Request, Encoding.UTF8, "application/json");

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.SendAsync(request);
                        result.StatusCode = (int)response.StatusCode;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result.Response = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.IsSuccess = true;
                        }
                        else
                        {
                            result.Response = (await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = "An error occurred while generating the token: " + ex.Message;
            }

            return result;


        }


    }
}
