using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.KYCAPI.Constants;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;
using System.Net;

namespace ScaleUP.Services.KYCAPI.Helpers
{
    public class KarzaAadharHelper
    {
        private readonly ApplicationDbContext _context;
        private ThirdPartyAPIConfigManager thirdPartyAPIConfigManager;
        public KarzaAadharHelper(ApplicationDbContext context)
        {
            _context = context;
            thirdPartyAPIConfigManager = new ThirdPartyAPIConfigManager(_context);
        }
        #region For Aadhar Verification
        public async Task<eAadhaarDigilockerResponseDc> eAdharDigilockerOTPXml(KYCActivityAadhar UpdateAdhaarInfo, string basePath, string userId)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            string requestFileFullPath = string.Empty;
            string responseFileFullPath = string.Empty;
            string processedResponseFileFullPath = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);


            string karzaType = ThirdPartyAPIConfigCodeConstants.KarzaAdhaarVerification;
            eAadhaarDigilockerResponseDc responseDC = new eAadhaarDigilockerResponseDc();
            var apiConfigdata = await GetUrlTokenForApi(karzaType);

            try
            {
                if (apiConfigdata != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        eAadhaarDigilockerRequestDc eAadhaarRequestDC = new eAadhaarDigilockerRequestDc();
                        eAadhaarRequestDC.consent = "Y";
                        eAadhaarRequestDC.aadhaarNo = UpdateAdhaarInfo.DocumentNumber;

                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {

                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);

                            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            string jsonstringReq = JsonConvert.SerializeObject(eAadhaarRequestDC);
                            request.Content = new StringContent(jsonstringReq);

                            ApiRequestResponseDTO addRequest = new ApiRequestResponseDTO()
                            {
                                UserId = "",
                                APIConfigId = apiConfigdata.id,
                                RequestResponse = jsonstringReq,
                                Type = "Request",
                                URL = apiConfigdata.url,
                                Header = request.Headers.ToString(),
                                Created = DateTime.Now,
                            };
                            try
                            {
                                requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                                requestFullpath = Path.Combine(basePath, "wwwroot");
                                requestFilecontent = JsonConvert.SerializeObject(addRequest);
                                requestFileFullPath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            }
                            catch (Exception)
                            {
                            }

                            string jsonString = string.Empty;
                            var response = await httpClient.SendAsync(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<eAadhaarDigilockerResponseDc>(jsonString);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = response.StatusCode == HttpStatusCode.BadGateway ? "Your Aadhaar could not be validated due to technical reason. Please re-try after sometime." : "Timeout Issue.Please try again." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "please try again,after sometime" }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "Your request is invalid." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "please try again,after sometime." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "The API requested is hidden for administrators only." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "The specified API could not be found." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "We had a problem with our server. Try again later." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Processing)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "It is in processing. Try again later." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "We had a problem with our server. Try again later." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                            {
                                UserId = "",
                                APIConfigId = apiConfigdata.id,
                                RequestResponse = jsonString,
                                Type = "Response",
                                URL = apiConfigdata.url,
                                Header = request.Headers.ToString(),
                                Created = DateTime.Now,
                            };
                            try
                            {
                                responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                                responseFullpath = Path.Combine(basePath, "wwwroot");
                                responseFilecontent = JsonConvert.SerializeObject(addresponse);
                                responseFileFullPath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                                isError = false;
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                {
                    UserId = userId,
                    APIConfigId = apiConfigdata.id,
                    RequestResponse = ex.ToString(),
                    Type = "Response",
                    URL = apiConfigdata.url,
                    Header = string.Empty,
                    Created = DateTime.Now,
                };
                try
                {
                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                    isError = false;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            ThirdPartyApiRequestDTO thirdPartyRequestDTO = new ThirdPartyApiRequestDTO
            {
                APIConfigId = apiConfigdata.id,
                UserId = userId,
                CompanyId = 0,
                ProcessedResponse = responseFileFullPath,
                Request = requestFileFullPath,
                Response = responseFileFullPath,
                IsError = isError
            };
            var res = await thirdPartyAPIConfigManager.InsertApiReqRes(thirdPartyRequestDTO);
            return responseDC;
        }
        #endregion

        #region For AadharOTP
        public async Task<eAdhaarDigilockerVerifyOTPResponseDcXml> eAadharDigilockerVerifyOTPXml(KYCActivityAadhar verifyOtp, string basePath, string userId)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            string requestFileFullPath = string.Empty;
            string responseFileFullPath = string.Empty;
            string processedResponseFileFullPath = string.Empty;


            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string karzaType = ThirdPartyAPIConfigCodeConstants.KarzaAdhaarOtp;
            eAdhaarDigilockerVerifyOTPResponseDcXml responseDC = new eAdhaarDigilockerVerifyOTPResponseDcXml();
            var apiConfigdata = await GetUrlTokenForApi(karzaType);
            try
            {
                if (apiConfigdata != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        eAadhaarDigilockerRequesTVerifyOTPDCXml eAadhaarRequestDC = new eAadhaarDigilockerRequesTVerifyOTPDCXml();

                        eAadhaarRequestDC.otp = verifyOtp.otp;
                        eAadhaarRequestDC.requestId = verifyOtp.requestId;
                        eAadhaarRequestDC.aadhaarNo = verifyOtp.DocumentNumber;
                        eAadhaarRequestDC.consent = "Y";

                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            string jsonstringReq = JsonConvert.SerializeObject(eAadhaarRequestDC);

                            ApiRequestResponseDTO addRequest = new ApiRequestResponseDTO()
                            {
                                UserId = userId,
                                APIConfigId = apiConfigdata.id,
                                RequestResponse = jsonstringReq,
                                Type = "Request",
                                URL = apiConfigdata.url,
                                Header = request.Headers.ToString(),
                                Created = DateTime.Now,
                            };
                            try
                            {
                                requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                                requestFullpath = Path.Combine(basePath, "wwwroot");
                                requestFilecontent = JsonConvert.SerializeObject(addRequest);
                                requestFileFullPath  = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                            request.Content = new StringContent(jsonstringReq);

                            string jsonString = string.Empty;
                            var response = await httpClient.SendAsync(request);

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<eAdhaarDigilockerVerifyOTPResponseDcXml>(jsonString);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "please try again" }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "please try again,after sometime" }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "Your request is invalid." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "please try again,after sometime." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "The API requested is hidden for administrators only." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "The specified API could not be found." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = "We had a problem with our server. Try again later." }
                                };
                                jsonString = response.StatusCode.ToString();
                            }
                            else if(response.StatusCode == HttpStatusCode.Processing)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = responseDC.statusMessage}
                                };
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC.error = JsonConvert.DeserializeObject<ErrorResponse>(jsonString);
                            }
                            ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                            {
                                UserId = userId,
                                APIConfigId = apiConfigdata.id,
                                RequestResponse = jsonString,
                                Type = "Response",
                                URL = apiConfigdata.url,
                                Header = request.Headers.ToString(),
                                Created = DateTime.Now,
                            };
                            try
                            {
                                responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                                responseFullpath = Path.Combine(basePath, "wwwroot");
                                responseFilecontent = JsonConvert.SerializeObject(addresponse);
                                responseFileFullPath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                                isError = false;
                            }
                            catch (Exception dx)
                            {

                                //throw dx;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                {
                    UserId = userId,
                    APIConfigId = apiConfigdata.id,
                    RequestResponse = ex.ToString(),
                    Type = "Response",
                    URL = apiConfigdata.url,
                    Header = string.Empty,
                    Created = DateTime.Now,
                };
                try
                {
                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFileFullPath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                    isError = false;
                }
                catch (Exception e)
                {

                    //throw e;
                }
            }
            ThirdPartyApiRequestDTO thirdPartyRequestDTO = new ThirdPartyApiRequestDTO
            {
                APIConfigId = apiConfigdata.id,
                UserId = userId,
                CompanyId = 0,
                ProcessedResponse = responseFileFullPath,
                Request = requestFileFullPath,
                Response = responseFileFullPath,
                IsError = isError
            };
            var res = await thirdPartyAPIConfigManager.InsertApiReqRes(thirdPartyRequestDTO);

            return responseDC;
        }

        #endregion
        public async Task<GetUrlTokenDTO> GetUrlTokenForApi(string providername)
        {
            GetUrlTokenDTO data = new GetUrlTokenDTO();
            var configdata = await thirdPartyAPIConfigManager.GetByCode(providername);
            if (configdata != null)
            {
                data.url = configdata.URL;
                data.token = configdata.Token;
                data.CompanyCode = configdata.Code == null ? "" : configdata.Code.ToString();
                data.id = configdata.Id;
                data.ApiSecretKey = configdata.Secret;
            }
            return data;
        }



    }
}
