using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.Services.LeadAPI.Migrations;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request;
using ScaleUP.Services.LeadModels.AyeFinance;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ScaleUP.Services.LeadAPI.Helper.NBFC
{
    public class AyeFinanceSCFNBFCHelper
    {
        public async Task<AyeFinanceSCFCommonAPIRequestResponse> GenerateToken(long LeadId, long LeadNBFCApiId, string apiUrl, string ApiKey, string PartnerId, string ReferenceId)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LeadId = LeadId,
                LeadNBFCApiId = LeadNBFCApiId,
                URL = apiUrl,
                IsActive = true,
                IsDeleted = false
            };
            GenerateToken generateToken = new GenerateToken
            {
                apiKey = ApiKey,
                partnerId = PartnerId,
                refId = ReferenceId
            };
            try
                {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                    {
                        result.Request = JsonConvert.SerializeObject(generateToken);
                        request.Content = new StringContent(result.Request, Encoding.UTF8, "application/json");

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
                // Log the exception (ex) if necessary
                result.Response = ex.Message;
            }

            return result;
        }

        public async Task<AyeFinanceSCFCommonAPIRequestResponse> AddLead(long LeadId, long LeadNBFCApiId, string apiUrl, string token, AddLeadRequestDc addLeadRequestDc)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LeadId = LeadId,
                LeadNBFCApiId = LeadNBFCApiId,
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
                        result.Request = JsonConvert.SerializeObject(addLeadRequestDc);

                        request.Content = new StringContent(result.Request, Encoding.UTF8, "application/json");

                        // Add the Bearer token to the Authorization header
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
                // Log the exception (ex) if necessary
                result.Response = ex.Message;
            }

            return result;
        }


        public async Task<AyeFinanceSCFCommonAPIRequestResponse> CheckCreditLine(long LeadId, long LeadNBFCApiId, string apiUrl, string token, CheckCreditLineReqDc checkCreditLineReqDc)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LeadId = LeadId,
                LeadNBFCApiId = LeadNBFCApiId,
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
                        result.Request = JsonConvert.SerializeObject(checkCreditLineReqDc);

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
                result.Response = ex.Message;
            }

            return result;
        }

        public async Task<AyeFinanceSCFCommonAPIRequestResponse> GetWebUrl(long LeadId, long LeadNBFCApiId, string apiUrl, string token, GetWebUrlReqDc getWebUrlReqDc)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LeadId = LeadId,
                LeadNBFCApiId = LeadNBFCApiId,
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
                        result.Request = JsonConvert.SerializeObject(getWebUrlReqDc);

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
                result.Response = ex.Message;
            }

            return result;
        }


        public async Task<AyeFinanceSCFCommonAPIRequestResponse> TransactionSendOtp(long LeadId, long LeadNBFCApiId, string apiUrl, string token, TransactionSendOtpReqDc transactionSendOtpReqDc)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LeadId = LeadId,
                LeadNBFCApiId = LeadNBFCApiId,
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
                        result.Request = JsonConvert.SerializeObject(transactionSendOtpReqDc);

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

        public async Task<AyeFinanceSCFCommonAPIRequestResponse> TransactionVerifyOtp(long LeadId, long LeadNBFCApiId, string apiUrl, string token, TransactionVerifythirdpartyreq req)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LeadId = LeadId,
                LeadNBFCApiId = LeadNBFCApiId,
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
        public async Task<AyeFinanceSCFCommonAPIRequestResponse> ApplyLoan(long LeadId, long LeadNBFCApiId, string apiUrl, string token, ApplyLoanthirdpartyreq req)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LeadId = LeadId,
                LeadNBFCApiId = LeadNBFCApiId,
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

        public async Task<AyeFinanceSCFCommonAPIRequestResponse> CheckTotalAndAvailableLimit(long LeadId, long LeadNBFCApiId, string apiUrl, string token, CheckCreditLineReqDc req)
        {
            var result = new AyeFinanceSCFCommonAPIRequestResponse
            {
                LeadId = LeadId,
                LeadNBFCApiId = LeadNBFCApiId,
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