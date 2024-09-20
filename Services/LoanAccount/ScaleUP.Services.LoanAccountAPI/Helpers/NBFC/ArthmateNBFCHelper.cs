using Newtonsoft.Json;
using ScaleUP.Global.Infrastructure.Helper;
using System.Net.Http.Headers;
using System.Net;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC.Arthmate;
using ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate;

namespace ScaleUP.Services.LoanAccountAPI.Helpers.NBFC
{
    public class ArthmateNBFCHelper
    {
        public async Task<ArthMateCommonAPIRequestResponse> repayment_schedule(Postrepayment_scheduleDc req, string ApiUrl, string Key, string SecretKey, long NBFCCompanyApiDetailId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 

            ApiUrl = $"{ApiUrl}/{req.loan_id}";
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.NBFCCompanyApiDetailId = NBFCCompanyApiDetailId;
            //result.LeadId = LeadId;
            //result.LeadNBFCApiId = LeadNBFCApiId;
            if (req != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", Key);
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
