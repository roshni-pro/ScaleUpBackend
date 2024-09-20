using Newtonsoft.Json;
using RestSharp;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.KYCAPI.Constants;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;
using System.Net;
using System.Net.Http.Headers;

namespace ScaleUP.Services.KYCAPI.Helpers
{
    public class KarzaPanProfileHelper
    {
        private readonly ApplicationDbContext _context;
        private ThirdPartyAPIConfigManager thirdPartyAPIConfigManager;

        public KarzaPanProfileHelper(ApplicationDbContext context)
        {
            _context = context;
            thirdPartyAPIConfigManager = new ThirdPartyAPIConfigManager(_context);
        }

        public async Task<KarzaPanProfileResponseDTO> KarzaPanProfile(string PanNo, string basePath, string userid)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;

            KarzaPanProfileResponseDTO responseDC = null;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string paramstr = "{\"pan\":\"{#PanNo#}\",\"aadhaarLastFour\":\"\",\"dob\":\"\",\"name\":\"\",\"address\":\"\",\"getContactDetails\":\"N\",\"PANStatus\":\"N\",\"isSalaried\":\"N\",\"isDirector\":\"N\",\"isSoleProp\":\"N\",\"fathersName\":\"N\",\"consent\":\"Y\",\"clientData\":{\"caseId\":\"\"}}";
            var data = await thirdPartyAPIConfigManager.GetByCode(ThirdPartyAPIConfigCodeConstants.KarzaPANProfile);

            try
            {
                if (data != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), data.URL))
                        {
                            paramstr = paramstr.Replace("{#PanNo#}", PanNo);
                            request.Headers.TryAddWithoutValidation("x-karza-key", data.Secret);
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                            request.Content = new StringContent(paramstr);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                            ApiRequestResponseDTO addRequest = new ApiRequestResponseDTO()
                            {
                                UserId = userid,
                                APIConfigId = data.Id,
                                RequestResponse = paramstr,
                                Type = "Request",
                                URL = data.URL,
                                Header = request.Headers.ToString(),
                                Created = DateTime.Now,
                            };
                            try
                            {
                                requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                                requestFullpath = Path.Combine(basePath, "wwwroot");
                                requestFilecontent = JsonConvert.SerializeObject(addRequest);
                                requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            }
                            catch (Exception)
                            {

                            }

                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                var res = JsonConvert.DeserializeObject<KarzaPanProfileResponseDTO>(jsonString);
                                if (res != null && res.statusCode == 101)
                                {
                                    res.IsSuccess = true;
                                    res.message = "Success";
                                    responseDC = res;
                                }
                                else
                                {
                                    responseDC = new KarzaPanProfileResponseDTO
                                    {
                                        requestId = "",
                                        result = null,
                                        statusCode = 0,
                                        message = "Pan not found ",
                                        IsSuccess = false
                                    };
                                }
                            }
                            else
                            {
                                responseDC = new KarzaPanProfileResponseDTO
                                {
                                    requestId = "",
                                    result = null,
                                    statusCode = 0,
                                    message = "Your PAN could not be validated due to technical reason. Please re-try after sometime.",
                                    IsSuccess = false
                                };
                            }
                            ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                            {
                                UserId = userid,
                                APIConfigId = data.Id,
                                RequestResponse = jsonString,
                                Type = "Response",
                                URL = data.URL,
                                Header = request.Headers.ToString(),
                                Created = DateTime.Now,
                            };
                            try
                            {
                                responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                                responseFullpath = Path.Combine(basePath, "wwwroot");
                                responseFilecontent = JsonConvert.SerializeObject(addresponse);
                                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                                isError = false;
                            }
                            catch (Exception)
                            {
                                //throw;
                            }
                        }
                    }
                }
                else
                {
                    responseDC = new KarzaPanProfileResponseDTO
                    {
                        requestId = "",
                        result = null,
                        statusCode = 0,
                        message = "Api config not found.",
                        IsSuccess = false
                    };
                }
            }
            catch (Exception ex)
            {
                ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                {
                    UserId = userid,
                    APIConfigId = data.Id,
                    RequestResponse = ex.ToString(),
                    Type = "Response",
                    URL = data.URL,
                    Header = string.Empty,
                    Created = DateTime.Now,
                };
                try
                {
                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                    isError = false;
                }
                catch (Exception)
                {

                }
            }

            ThirdPartyApiRequestDTO thirdPartyRequestDTO = new ThirdPartyApiRequestDTO
            {
                APIConfigId = data.Id,
                UserId = userid,
                CompanyId = 0,
                ProcessedResponse = responseFullpath,
                Request = requestFullpath,
                Response = responseFullpath,
                IsError = isError
            };
            var result = await thirdPartyAPIConfigManager.InsertApiReqRes(thirdPartyRequestDTO);
            return responseDC;

        }
    }
}
