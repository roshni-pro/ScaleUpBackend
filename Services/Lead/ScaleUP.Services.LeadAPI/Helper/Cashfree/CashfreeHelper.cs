using MassTransit.Courier.Contracts;
using Newtonsoft.Json;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadDTO.Cashfree;
using ScaleUP.Services.LeadModels.Cashfree;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ScaleUP.Services.LeadAPI.Helper.Cashfree
{
    public class CashfreeHelper
    {
        public async Task<CashfreeCommonAPIRequestResponse> CreateSubscriptionwithPlanInfo(CreateSubscriptionwithPlanInfoInput cashfreeCreateEnachInput, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new CashfreeCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;

            if (cashfreeCreateEnachInput != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Add("X-Client-Id", Key);
                            request.Headers.Add("X-Client-Secret", SecretKey);
                            result.Request = ApiUrl;

                            var reqmsg = JsonConvert.SerializeObject(cashfreeCreateEnachInput);

                            result.Request = reqmsg;

                            request.Content = new StringContent(reqmsg);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK)
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
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            return result;
        }
        public async Task<CashfreeCommonAPIRequestResponse> GetSubscriptionDetails(string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new CashfreeCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), ApiUrl))
                    {
                        request.Headers.Add("X-Client-Id", Key);
                        request.Headers.Add("X-Client-Secret", SecretKey);
                        result.Request = ApiUrl;
                        var content = new StringContent(string.Empty);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        request.Content = content;

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
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
            result.LeadNBFCApiId = LeadNBFCApiId;
            result.LeadId = LeadId;
            return result;
        }

        public async Task<CashfreeCommonAPIRequestResponse> CancelSubscription(string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new CashfreeCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                    {
                        request.Headers.Add("X-Client-Id", Key);
                        request.Headers.Add("X-Client-Secret", SecretKey);
                        result.Request = ApiUrl;
                        var content = new StringContent(string.Empty);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        request.Content = content;

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
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
            result.LeadNBFCApiId = LeadNBFCApiId;
            result.LeadId = LeadId;
            return result;
        }

    }
}
