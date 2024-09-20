using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ScaleUP.Services.LoanAccountAPI.Helpers.NBFC
{
    public class BlackSoilNBFCHelper
    {
        public async Task<BlackSoilCommonAPIRequestResponse> CreateWithdrawalRequest(long NBFCCompanyApiDetailId, string ApiUrl, string Key, string SecretKey)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.NBFCCompanyApiDetailId = NBFCCompanyApiDetailId;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            result.IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.Response = jsonString;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            result.Response = jsonString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = ex.Message.ToString();
            }
            return result;
        }
        public async Task<BlackSoilCommonAPIRequestResponse> UploadWithdrawalRequestDocument(BlackSoilWithdrawalRequestInput WithdrawalRequest, string ApiUrl, string Key, string SecretKey)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.NBFCCompanyApiDetailId = WithdrawalRequest.NBFCCompanyApiDetailId;
            if (WithdrawalRequest != null)
            {
                var reqmsg = JsonConvert.SerializeObject(WithdrawalRequest);
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var stream = FileSaverHelper.GetStreamDataFromUrl(WithdrawalRequest.file);
                            var content = new MultipartFormDataContent();
                            content.Add(new StreamContent(stream), "file", WithdrawalRequest.file);
                            content.Add(new StringContent(WithdrawalRequest.amount.ToString()), "amount");
                            content.Add(new StringContent(WithdrawalRequest.disbursed_amount.ToString()), "disbursed_amount");
                            content.Add(new StringContent(WithdrawalRequest.invoice_number), "invoice_number");
                            content.Add(new StringContent(WithdrawalRequest.invoice_date), "invoice_date");

                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                            {
                                result.IsSuccess = true;
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result.Response = jsonString;
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                result.Response = jsonString;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Response = ex.Message.ToString();
                }
            }
            return result;
        }
        public async Task<BlackSoilCommonAPIRequestResponse> BulkInvoicesApprove(string InvoiceIds, long NBFCCompanyApiDetailId, string ApiUrl, string Key, string SecretKey)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.NBFCCompanyApiDetailId = NBFCCompanyApiDetailId;
            if (InvoiceIds != null)
            {
                var reqmsg = JsonConvert.SerializeObject("{\"invoice_files_ids\":[" + InvoiceIds + "]}");
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            //var content = new MultipartFormDataContent();
                            //content.Add(new StringContent(InvoiceIds), "invoice_files_ids");
                            var content = new StringContent("{\"invoice_files_ids\":["+ InvoiceIds + "]}", null, "application/json");
                            request.Content = content;
                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                            {
                                result.IsSuccess = true;
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result.Response = jsonString;
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                result.Response = jsonString;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Response = ex.Message.ToString();
                }
            }
            return result;
        }


        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilGetAvailableCreditLimit(long NBFCCompanyApiDetailId, string ApiUrl, string Key, string SecretKey)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.NBFCCompanyApiDetailId = NBFCCompanyApiDetailId;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            result.IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.Response = jsonString;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            jsonString = null;
                            result.Response = jsonString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = ex.Message.ToString();
            }
            return result;
        }


        public async Task<List<BlackSoilCommonAPIRequestResponse>> GetWithUpdateCreditLimit(long NBFCCompanyApiDetailId, string businessId, double transAmt, string ApiUrl, string Key, string SecretKey)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            List<BlackSoilCommonAPIRequestResponse> lstAPIRequestResponse = new List<BlackSoilCommonAPIRequestResponse>();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.NBFCCompanyApiDetailId = NBFCCompanyApiDetailId;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            result.IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.Response = jsonString;
                            var blackSoilCreditLine = JsonConvert.DeserializeObject<BlackSoilCreditLine>(jsonString);
                            if (blackSoilCreditLine != null && blackSoilCreditLine.results != null && !string.IsNullOrEmpty(blackSoilCreditLine.results.FirstOrDefault().update_url))
                            {
                                string limitstr = blackSoilCreditLine.results.FirstOrDefault() != null ? blackSoilCreditLine.results.FirstOrDefault().usable_limit_amount : "0";

                                double limit;
                                var res = double.TryParse(limitstr, out limit);
                                if (res && limit > 0 && (limit - transAmt) >= 0)
                                {
                                    var newusable_limit_amount = limit - transAmt;
                                    var updateReqRes = await UpdateCreditLimit(NBFCCompanyApiDetailId, businessId, newusable_limit_amount, blackSoilCreditLine.results.FirstOrDefault().update_url, Key, SecretKey);
                                    lstAPIRequestResponse.Add(updateReqRes);
                                }
                            }
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            result.Response = jsonString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = ex.Message.ToString();
            }

            lstAPIRequestResponse.Add(result);
            return lstAPIRequestResponse;
        }


        public async Task<BlackSoilCommonAPIRequestResponse> UpdateCreditLimit(long NBFCCompanyApiDetailId, string businessId, double NewLimitAmt, string ApiUrl, string Key, string SecretKey)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.NBFCCompanyApiDetailId = NBFCCompanyApiDetailId;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("PUT"), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(NewLimitAmt.ToString()), "usable_limit_amount");
                        content.Add(new StringContent(businessId), "business");
                        request.Content = content;

                        //var contentList = new List<string>();
                        //contentList.Add($"usable_limit_amount={Uri.EscapeDataString(NewLimitAmt.ToString())}");
                        //contentList.Add($"business={Uri.EscapeDataString(businessId)}");
                        //request.Content = new StringContent(string.Join("&", contentList));
                        //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            result.IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.Response = jsonString;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            result.Response = jsonString;
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                result.Response = ex.Message.ToString();
            }

            return result;
        }


        private async Task<BlackSoilCommonAPIRequestResponse> CallAPI(string ApiUrl, string Key, string SecretKey, string HttpMethodCode, string BodyContent, long nbfcCompanyApiDetailId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(HttpMethodCode), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;

                        if (!string.IsNullOrEmpty(BodyContent))
                        {
                            request.Content = new StringContent(BodyContent);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        }

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            result.IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.Response = jsonString;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            result.Response = jsonString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = ex.Message.ToString();
            }
            result.NBFCCompanyApiDetailId = nbfcCompanyApiDetailId;

            return result;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilRecalculateAccounting(string ApiUrl, string Key, string SecretKey, bool isTesting = false)
        {
            string HttpMethodCode = "GET";
            string BodyContent = "";
            var result = await CallAPI(ApiUrl, Key, SecretKey, HttpMethodCode, BodyContent, 0);
            if (isTesting)
            {
                result = null;
            }
            return result;
        }


        
        public async Task<BlackSoilCommonAPIRequestResponse> GetLoanRepayment(string ApiUrl, string Key, string SecretKey, long nbfcCompanyApiDetailId, bool isTesting = false)
        {
            string HttpMethodCode = "GET";
            string BodyContent = "";
            var result = await CallAPI(ApiUrl, Key, SecretKey, HttpMethodCode, BodyContent, nbfcCompanyApiDetailId);
            if(isTesting )
            {
                result = null;
            }
            return result;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> GetLoanAccountDetail(string ApiUrl, string Key, string SecretKey, long nbfcCompanyApiDetailId)
        {

            string HttpMethodCode = "GET";
            string BodyContent = "";
            var result = await CallAPI(ApiUrl, Key, SecretKey, HttpMethodCode, BodyContent, nbfcCompanyApiDetailId);
            return result;
        }
        public async Task<BlackSoilCommonAPIRequestResponse> DeleteInvoice(string ApiUrl, string Key, string SecretKey)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            //result.NBFCCompanyApiDetailId = NBFCCompanyApiDetailId;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;
                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.NoContent)
                        {
                            result.IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.Response = jsonString;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            result.Response = jsonString;
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                result.Response = ex.Message.ToString();
            }

            return result;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> GetLoanRepaymentLIST(string ApiUrl, string Key, string SecretKey, bool isTesting = false)
        {
            string HttpMethodCode = "GET";
            string BodyContent = "";
            var result = await CallApiRepaymentLIST(ApiUrl, Key, SecretKey, HttpMethodCode, BodyContent);
            if (isTesting)
            {
                result = null;
            }
            return result;
        }

        private async Task<BlackSoilCommonAPIRequestResponse> CallApiRepaymentLIST(string ApiUrl, string Key, string SecretKey, string HttpMethodCode, string BodyContent)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(HttpMethodCode), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;

                        if (!string.IsNullOrEmpty(BodyContent))
                        {
                            request.Content = new StringContent(BodyContent);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        }

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        //result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            result.IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            result.Response = jsonString;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            result.Response = jsonString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Response = ex.Message.ToString();                
            }
            //result.NBFCCompanyApiDetailId = nbfcCompanyApiDetailId;

            return result;
        }

    }

}
