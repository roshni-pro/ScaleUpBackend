using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadModels.ArthMate;
using System.Net.Http.Headers;
using System.Net;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using Newtonsoft.Json;
using ScaleUP.Services.LeadDTO.eSign;
using ScaleUP.Services.LeadModels;

namespace ScaleUP.Services.LeadAPI.Helper
{
    public class eSignKarzaHelper
    {
        public async Task<eSignKarzaCommonAPIRequestResponse> eSignSessionAsync(eSignSessionRequest req, string ApiUrl, string Key, string SecretKey, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new eSignKarzaCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            if (req != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Add("x-karza-key", Key); //= new AuthenticationHeaderValue("x-karza-key", Key);
                            var reqmsg = JsonConvert.SerializeObject(req);
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
            return result;
        }
        public async Task<eSignKarzaCommonAPIRequestResponse> eSignDocumentsAsync(eSignDocumentRequest req, string ApiUrl, string Key, string SecretKey, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new eSignKarzaCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            if (req != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Add("x-karza-key", Key);
                            var reqmsg = JsonConvert.SerializeObject(req);
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
            return result;
        }
    }
}
