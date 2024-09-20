using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Constants;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.Lead;
using System.Net;
using Error = ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.Error;

namespace ScaleUP.Services.LeadAPI.Helper
{
    public class KarzaAadharHelper
    {
        private readonly LeadApplicationDbContext _context;
        private ThirdPartyApiConfigManager thirdPartyAPIConfigManager;
        //private IHostEnvironment _hostingEnvironment;
        public KarzaAadharHelper(LeadApplicationDbContext context /*IHostEnvironment hostingEnvironment*/)
        {
            _context = context;
            thirdPartyAPIConfigManager = new ThirdPartyApiConfigManager(_context);
            //_hostingEnvironment = hostingEnvironment;
        }
        #region For Aadhar Verification
        public async Task<eAadhaarDigilockerResponseDc> eAdharDigilockerOTPXml(string DocumentNumber, string userId, string basePath = "")
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
            //basePath = _hostingEnvironment.ContentRootPath;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);


            string karzaType = ThirdPartyApiConfigConstant.KarzaAdhaarOtp;
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
                        eAadhaarRequestDC.aadhaarNo = DocumentNumber;

                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {

                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);

                            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            string jsonstringReq = JsonConvert.SerializeObject(eAadhaarRequestDC);
                            request.Content = new StringContent(jsonstringReq);

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
        public async Task<eAdhaarDigilockerVerifyOTPResponseDcXml> eAadharDigilockerVerifyOTPXml(eAadhaarDigilockerRequesTVerifyOTPDCXml verifyOtp,string UserId, string basePath = "")
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
            //basePath = _hostingEnvironment.ContentRootPath;


            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string karzaType = ThirdPartyApiConfigConstant.KarzaAdhaarVerification;
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
                        eAadhaarRequestDC.aadhaarNo = verifyOtp.aadhaarNo;
                        eAadhaarRequestDC.consent = "Y";

                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            string jsonstringReq = JsonConvert.SerializeObject(eAadhaarRequestDC);

                            ApiRequestResponseDTO addRequest = new ApiRequestResponseDTO()
                            {
                                UserId = UserId,
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
                            else if (response.StatusCode == HttpStatusCode.Processing)
                            {
                                responseDC.error = new ErrorResponse
                                {
                                    error = new Error { message = responseDC.statusMessage }
                                };
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC.error = JsonConvert.DeserializeObject<ErrorResponse>(jsonString);
                            }
                            ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                            {
                                UserId = UserId,
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
                    UserId = UserId,
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
                UserId = UserId,
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
            //var configdata = await thirdPartyAPIConfigManager.GetByCode(providername);

            var configdata = _context.LeadNBFCApis.Where(x => x.Code == providername && x.IsActive && !x.IsDeleted).FirstOrDefault();


            if (configdata != null)
            {
                data.url = configdata.APIUrl;
                data.token = configdata.TAPIKey;
                data.CompanyCode = configdata.Code == null ? "" : configdata.Code.ToString();
                data.id = configdata.Id;
                data.ApiSecretKey = configdata.TAPISecretKey;
            }
            return data;
        }


    }
}
