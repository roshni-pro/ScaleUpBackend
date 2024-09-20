using Microsoft.Identity.Client;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Constants;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Migrations;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using System.Xml;

namespace ScaleUP.Services.LeadAPI.Helper
{
    public class ExperianHelper
    {
        private readonly LeadApplicationDbContext _context;
        private ThirdPartyApiConfigManager thirdPartyApiConfigManager;
        public ExperianHelper(LeadApplicationDbContext context)
        {
            _context = context;
            thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
        }

        #region Direct
        public async Task<ExperianOTPRegistrationResponseDTO> ExperianOTPRegisterAsync(ExperianOTPRegistrationRequestDC requestDC, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            ExperianOTPRegistrationResponseDTO responseDC = new ExperianOTPRegistrationResponseDTO();
            ThirdPartyAPIConfigResult<ExperianOTPRegistrationDTO> apiConfigdataFull = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<ExperianOTPRegistrationDTO>(ThirdPartyApiConfigConstant.ExperianOTPRegistration);
            ExperianOTPRegistrationDTO apiConfigdata = null;
            if (apiConfigdataFull != null)
            {
                apiConfigdata = apiConfigdataFull.Config;
            }
            if (apiConfigdata != null)
            {
                RequestResponseDc addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.ApiUrl))
                        {
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            var contentList = new List<string>();
                            contentList.Add($"clientName={Uri.EscapeDataString(apiConfigdata.ApiSecret)}");
                            contentList.Add($"allowInput={Uri.EscapeDataString("1")}");
                            contentList.Add($"allowEdit={Uri.EscapeDataString("1")}");
                            contentList.Add($"allowCaptcha={Uri.EscapeDataString("1")}");
                            contentList.Add($"allowConsent={Uri.EscapeDataString("1")}");
                            contentList.Add($"allowEmailVerify={Uri.EscapeDataString("1")}");
                            contentList.Add($"allowVoucher={Uri.EscapeDataString("1")}");
                            contentList.Add($"voucherCode={Uri.EscapeDataString(apiConfigdata.Other)}");
                            contentList.Add($"firstName={Uri.EscapeDataString(requestDC.firstName)}");
                            contentList.Add($"surName={Uri.EscapeDataString(requestDC.surName)}");
                            contentList.Add($"dateOfBirth={Uri.EscapeDataString(requestDC.dateOfBirth)}");
                            contentList.Add($"gender={Uri.EscapeDataString(requestDC.gender.ToString())}");
                            contentList.Add($"mobileNo={Uri.EscapeDataString(requestDC.mobileNo)}");
                            contentList.Add($"email={Uri.EscapeDataString(requestDC.email)}");
                            contentList.Add($"flatno={Uri.EscapeDataString(requestDC.flatno)}");
                            contentList.Add($"city={Uri.EscapeDataString(requestDC.city)}");
                            contentList.Add($"state={Uri.EscapeDataString(requestDC.experianState.ToString())}");
                            contentList.Add($"pincode={Uri.EscapeDataString(requestDC.pincode)}");
                            contentList.Add($"pan={Uri.EscapeDataString(requestDC.pan)}");
                            contentList.Add($"reason={Uri.EscapeDataString("Find out my credit score")}");
                            contentList.Add($"middleName={Uri.EscapeDataString(requestDC.middleName ?? "")}");
                            contentList.Add($"passport={Uri.EscapeDataString(requestDC.passport ?? "")}");
                            contentList.Add($"aadhaar={Uri.EscapeDataString(requestDC.aadhaar ?? "")}");
                            contentList.Add($"noValidationByPass={Uri.EscapeDataString("0")}");
                            contentList.Add($"emailConditionalByPass={Uri.EscapeDataString("1")}");

                            request.Content = new StringContent(string.Join("&", contentList));
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                            var reqmsg = JsonConvert.SerializeObject(requestDC);
                            RequestResponseDc addRequest = new RequestResponseDc()
                            {
                                ApiMasterId = apiConfigdata.ApiMasterId,
                                Header = request.Headers.ToString(),
                                RequestResponseMsg = reqmsg,
                                Type = "Request",
                                Url = apiConfigdata.ApiUrl
                            };
                            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                            requestFullpath = Path.Combine(basePath, "wwwroot");
                            requestFilecontent = JsonConvert.SerializeObject(addRequest);
                            requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            //await InsertReqestReponseAsync(addRequest);

                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC = JsonConvert.DeserializeObject<ExperianOTPRegistrationResponseDTO>(jsonString);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.errorString = "please try again";
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.errorString = "please try again,after sometime";
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                var encoded = HttpUtility.HtmlEncode(jsonString);
                                responseDC.errorString = jsonString;
                            }
                            addresponse = new RequestResponseDc()
                            {
                                ApiMasterId = apiConfigdata.ApiMasterId,
                                Header = response.Headers.ToString(),
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.ApiUrl
                            };
                            responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot");
                            responseFilecontent = JsonConvert.SerializeObject(addresponse);
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            isError = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new RequestResponseDc()
                    {
                        ApiMasterId = apiConfigdata.ApiMasterId,
                        Header = string.Empty,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.ApiUrl
                    };
                    responseDC.errorString = ex.Message.ToString();

                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);

                    //TextFileLogHelper.TraceLog("Error in ExperianOTPRegisterAsync ...." + ex.Message.ToString());
                }
                ThirdPartyRequestDTO thirdPartyRequestDTO = new ThirdPartyRequestDTO
                {
                    SubActivityId = requestDC.subActivityId,
                    ActivityId = requestDC.activityId,
                    CompanyId = requestDC.companyId,
                    LeadId = requestDC.LeadId,
                    Code = "",
                    ProcessedResponse = responseFullpath,
                    Request = requestFullpath,
                    Response = responseFullpath,
                    IsError = isError,
                    ThirdPartyApiConfigId= apiConfigdataFull.Id,
                };
                ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
                var res = thirdPartyRequestManager.SaveThirdPartyRequest(thirdPartyRequestDTO);

                //await InsertReqestReponseAsync(addresponse);
            }
            return responseDC;
        }
        public async Task<ExperianOTPGenerationResponseDC> ExperianOTPGenerationAsync(ExperianOTPGenerationRequestDC requestDC, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            ExperianOTPGenerationResponseDC responseDC = new ExperianOTPGenerationResponseDC();
            ThirdPartyAPIConfigResult<ExperianOTPGenerateDTO> apiConfigdataFull = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<ExperianOTPGenerateDTO>(ThirdPartyApiConfigConstant.ExperianOTPGeneration);
            ExperianOTPGenerateDTO apiConfigdata = null;
            if (apiConfigdataFull != null)
            {
                apiConfigdata = apiConfigdataFull.Config;
            }
            if (apiConfigdata != null)
            {
                RequestResponseDc addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.ApiUrl))
                        {
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            var contentList = new List<string>();
                            contentList.Add($"stgOneHitId={Uri.EscapeDataString(requestDC.stgOneHitId)}");
                            contentList.Add($"stgTwoHitId={Uri.EscapeDataString(requestDC.stgTwoHitId)}");
                            contentList.Add($"mobileNo={Uri.EscapeDataString(requestDC.mobileNo)}");
                            contentList.Add($"type={Uri.EscapeDataString(apiConfigdata.Other)}");

                            request.Content = new StringContent(string.Join("&", contentList));
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                            var reqmsg = JsonConvert.SerializeObject(requestDC);
                            RequestResponseDc addRequest = new RequestResponseDc()
                            {
                                Header = request.Headers.ToString(),
                                RequestResponseMsg = reqmsg,
                                Type = "Request",
                                Url = apiConfigdata.ApiUrl
                            };
                            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                            requestFullpath = Path.Combine(basePath, "wwwroot");
                            requestFilecontent = JsonConvert.SerializeObject(addRequest);
                            requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);

                            //await InsertReqestReponseAsync(addRequest);
                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC = JsonConvert.DeserializeObject<ExperianOTPGenerationResponseDC>(jsonString);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.errorString = "please try again";
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.errorString = "please try again,after sometime";
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC.errorString = jsonString;
                            }
                            addresponse = new RequestResponseDc()
                            {
                                Header = response.Headers.ToString(),
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.ApiUrl
                            };
                            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot");
                            requestFilecontent = JsonConvert.SerializeObject(addRequest);
                            responseFullpath = fileSaverHelper.SaveFile(requestFilename, responseFullpath, requestFilecontent);
                            isError = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new RequestResponseDc()
                    {
                        Header = string.Empty,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.ApiUrl
                    };
                    responseDC.errorString = ex.Message.ToString();

                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                }
                ThirdPartyRequestDTO thirdPartyRequestDTO = new ThirdPartyRequestDTO
                {
                    SubActivityId = requestDC.SubActivityId,
                    ActivityId = requestDC.ActivityId,
                    CompanyId = requestDC.CompanyId,
                    LeadId = requestDC.LeadId,
                    Code = "",
                    ProcessedResponse = responseFullpath,
                    Request = requestFullpath,
                    Response = responseFullpath,
                    IsError = isError,
                    ThirdPartyApiConfigId = apiConfigdataFull.Id

                };
                ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
                var res = thirdPartyRequestManager.SaveThirdPartyRequest(thirdPartyRequestDTO);
            }
            return responseDC;
        }
        public async Task<ExperianOTPValidationResponseDC> ExperianOTPValidationAsync(ExperianOTPValidationRequestDC requestDC, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            ExperianOTPValidationResponseDC responseDC = new ExperianOTPValidationResponseDC();
            ThirdPartyAPIConfigResult<ExperianOTPValidationDTO> apiConfigdataFull = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<ExperianOTPValidationDTO>(ThirdPartyApiConfigConstant.ExperianOTPValidation);
            ExperianOTPValidationDTO apiConfigdata = null;
            if (apiConfigdataFull != null)
            {
                apiConfigdata = apiConfigdataFull.Config;
            }
            if (apiConfigdata != null)
            {
                RequestResponseDc addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.ApiUrl))
                        {

                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            var contentList = new List<string>();
                            contentList.Add($"stgOneHitId={Uri.EscapeDataString(requestDC.stgOneHitId)}");
                            contentList.Add($"stgTwoHitId={Uri.EscapeDataString(requestDC.stgTwoHitId)}");
                            contentList.Add($"otp={Uri.EscapeDataString(requestDC.otp)}");
                            contentList.Add($"mobileNo={Uri.EscapeDataString(requestDC.mobileNo)}");
                            contentList.Add($"type={Uri.EscapeDataString(apiConfigdata.Other)}");

                            request.Content = new StringContent(string.Join("&", contentList));
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");


                            var reqmsg = JsonConvert.SerializeObject(requestDC);
                            RequestResponseDc addRequest = new RequestResponseDc()
                            {
                                //ApiMasterId = apiConfigdata.ApiMasterId,
                                Header = request.Headers.ToString(),
                                RequestResponseMsg = reqmsg,
                                Type = "Request",
                                Url = apiConfigdata.ApiUrl
                            };

                            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                            requestFullpath = Path.Combine(basePath, "wwwroot");
                            requestFilecontent = JsonConvert.SerializeObject(addRequest);
                            requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);


                            //await InsertReqestReponseAsync(addRequest);

                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<ExperianOTPValidationResponseDC>(jsonString);
                                if (!string.IsNullOrEmpty(responseDC.errorString) && responseDC.errorString == "OTP validation already tried,register consumer again for new OTP")
                                {
                                    responseDC.errorString = "OTP validation already tried,please click on resend for new OTP";
                                }
                                if (string.IsNullOrEmpty(responseDC.errorString))
                                {
                                    string xmlres = HttpUtility.HtmlDecode(responseDC.showHtmlReportForCreditReport);
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(xmlres);
                                    XmlNodeList xnList = doc.SelectNodes("/INProfileResponse/SCORE");
                                    foreach (XmlNode xn in xnList)
                                    {
                                        responseDC.CreditScore = xn["BureauScore"].InnerText;
                                    }

                                    try
                                    {
                                        requestFilename = "BureauReport_" + Guid.NewGuid().ToString() + ".json";
                                        requestFullpath = Path.Combine(basePath, "wwwroot/CibiReport");
                                        requestFilecontent = xmlres;
                                        requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                                        responseDC.FilePath = requestFullpath;
                                    }
                                    catch (Exception)
                                    {
                                    }

                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.errorString = "please try again";
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.errorString = "please try again,after sometime";
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC.errorString = jsonString;
                            }
                            addresponse = new RequestResponseDc()
                            {
                                //ApiMasterId = apiConfigdata.ApiMasterId,
                                Header = response.Headers.ToString(),
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.ApiUrl
                            };

                            responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot");
                            responseFilecontent = JsonConvert.SerializeObject(addresponse);
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            isError = false;
                            //await InsertReqestReponseAsync(addresponse);

                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new RequestResponseDc()
                    {
                        //ApiMasterId = apiConfigdata.ApiMasterId,
                        Header = string.Empty,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.ApiUrl
                    };
                    responseDC.errorString = ex.Message.ToString();

                    //TextFileLogHelper.TraceLog("Error in ExperianOTPValidationAsync ...." + ex.Message.ToString());

                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);

                    //await InsertReqestReponseAsync(addresponse);

                }

                ThirdPartyRequestDTO thirdPartyRequestDTO = new ThirdPartyRequestDTO
                {
                    SubActivityId = requestDC.SubActivityId,
                    ActivityId = requestDC.ActivityId,
                    CompanyId = requestDC.CompanyId,
                    LeadId = requestDC.LeadId,
                    Code = "",
                    ProcessedResponse = responseFullpath,
                    Request = requestFullpath,
                    Response = responseFullpath,
                    IsError = isError,
                    ThirdPartyApiConfigId = apiConfigdataFull.Id
                };
                ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
                var res = thirdPartyRequestManager.SaveThirdPartyRequest(thirdPartyRequestDTO);

            }
            return responseDC;
        }
        #endregion

        #region MASKED MOBILE GENERATION
        public async Task<MaskedMobileGenerationResponseDC> MaskedMobileGenerationAsync(MaskedMobileGenerationRequestDC requestDC, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            MaskedMobileGenerationResponseDC responseDC = new MaskedMobileGenerationResponseDC();
            ThirdPartyAPIConfigResult<MaskedMobileGenerationDTO> apiConfigdataFull = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<MaskedMobileGenerationDTO>(ThirdPartyApiConfigConstant.MaskedMobileGeneration);
            MaskedMobileGenerationDTO apiConfigdata = null;
            if (apiConfigdataFull != null)
            {
                apiConfigdata = apiConfigdataFull.Config;
            }
            if (apiConfigdata != null)
            {
                AddRequestResponseDc addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.ApiUrl))
                        {
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                            request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                            request.Headers.TryAddWithoutValidation("Cookie", "ecssessionprotector=6E106493E6634F67C6EE5F54C76BE68C; incap_ses_706_2264524=Gs5CNEtHSmz4gjAjxzfMCWPUuWEAAAAAl84koilqYxVUvmS5wv5buQ==; nlbi_2264524=dAe2EERUSGH9lfuFnHQ/lQAAAAAXMNDyrA0tF1lPKPyWzvIv; visid_incap_2264524=yZBzhSMoRZWkquunLMGrbLMut2EAAAAAQUIPAAAAAADaIKOJgw2BYquA3DYw6N/E");

                            var contentList = new List<string>();

                            contentList.Add($"stgOneHitId={Uri.EscapeDataString(requestDC.stgOneHitId)}");
                            contentList.Add($"stgTwoHitId={Uri.EscapeDataString(requestDC.stgTwoHitId)}");
                            contentList.Add($"clientName={Uri.EscapeDataString(apiConfigdata.Other)}");

                            request.Content = new StringContent(string.Join("&", contentList));
                            //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                            var reqmsg = JsonConvert.SerializeObject(requestDC);
                            AddRequestResponseDc addRequest = new AddRequestResponseDc()
                            {
                                Header = request.Headers.ToString(),
                                RequestResponseMsg = reqmsg,
                                Type = "Request",
                                Url = apiConfigdata.ApiUrl
                            };
                            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                            requestFullpath = Path.Combine(basePath, "wwwroot");
                            requestFilecontent = JsonConvert.SerializeObject(addRequest);
                            requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            //await InsertReqestReponseAsync(addRequest);

                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC = JsonConvert.DeserializeObject<MaskedMobileGenerationResponseDC>(jsonString);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.errorString = "please try again";
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.errorString = "please try again,after sometime";
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC.errorString = jsonString;
                            }
                            addresponse = new AddRequestResponseDc()
                            {
                                Header = response.Headers.ToString(),
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.ApiUrl
                            };
                            responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot");
                            responseFilecontent = JsonConvert.SerializeObject(addresponse);
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            isError = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new AddRequestResponseDc()
                    {
                        Header = string.Empty,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.ApiUrl
                    };
                    responseDC.errorString = ex.Message.ToString();
                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                }
                ThirdPartyRequestDTO thirdPartyRequestDTO = new ThirdPartyRequestDTO
                {
                    SubActivityId = requestDC.SubActivityId,
                    ActivityId = requestDC.ActivityId,
                    CompanyId = requestDC.CompanyId,
                    LeadId = requestDC.LeadId,
                    Code = "",
                    ProcessedResponse = responseFullpath,
                    Request = requestFullpath,
                    Response = responseFullpath,
                    IsError = isError,
                    ThirdPartyApiConfigId = apiConfigdataFull.Id
                };
                ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
                var res = thirdPartyRequestManager.SaveThirdPartyRequest(thirdPartyRequestDTO);
            }
            return responseDC;
        }
        public async Task<MaskedMobileOTPGenerationResponseDC> MaskedMobileOTPGenerationAsync(MaskedMobileOTPGenerationRequestDC requestDC, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            MaskedMobileOTPGenerationResponseDC responseDC = new MaskedMobileOTPGenerationResponseDC();
            ThirdPartyAPIConfigResult<MaskedMobileOTPGenerationDTO> apiConfigdataFull = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<MaskedMobileOTPGenerationDTO>(ThirdPartyApiConfigConstant.MaskedMobileOTPGeneration);
            MaskedMobileOTPGenerationDTO apiConfigdata = null;
            if (apiConfigdataFull != null)
            {
                apiConfigdata = apiConfigdataFull.Config;
            }
            if (apiConfigdata != null)
            {
                AddRequestResponseDc addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.ApiUrl))
                        {
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            var contentList = new List<string>();
                            contentList.Add($"stgOneHitId={Uri.EscapeDataString(requestDC.stgOneHitId)}");
                            contentList.Add($"stgTwoHitId={Uri.EscapeDataString(requestDC.stgTwoHitId)}");
                            contentList.Add($"mobileNo={Uri.EscapeDataString(requestDC.mobileNo)}");
                            contentList.Add($"type={Uri.EscapeDataString(apiConfigdata.Other)}");

                            request.Content = new StringContent(string.Join("&", contentList));
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                            var reqmsg = JsonConvert.SerializeObject(requestDC);
                            AddRequestResponseDc addRequest = new AddRequestResponseDc()
                            {
                                Header = request.Headers.ToString(),
                                RequestResponseMsg = reqmsg,
                                Type = "Request",
                                Url = apiConfigdata.ApiUrl
                            };
                            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                            requestFullpath = Path.Combine(basePath, "wwwroot");
                            requestFilecontent = JsonConvert.SerializeObject(addRequest);
                            requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            //await InsertReqestReponseAsync(addRequest);

                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responseDC = JsonConvert.DeserializeObject<MaskedMobileOTPGenerationResponseDC>(jsonString);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.errorString = "please try again";
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.errorString = "please try again,after sometime";
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC.errorString = jsonString;
                            }
                            addresponse = new AddRequestResponseDc()
                            {
                                Header = response.Headers.ToString(),
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.ApiUrl
                            };
                            responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot");
                            responseFilecontent = JsonConvert.SerializeObject(addresponse);
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            isError = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new AddRequestResponseDc()
                    {
                        Header = string.Empty,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.ApiUrl
                    };
                    responseDC.errorString = ex.Message.ToString();
                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);

                    //TextFileLogHelper.TraceLog("Error in ExperianOTPGenerationAsync ...." + ex.Message.ToString());
                }
                ThirdPartyRequestDTO thirdPartyRequestDTO = new ThirdPartyRequestDTO
                {
                    SubActivityId = requestDC.SubActivityId,
                    ActivityId = requestDC.ActivityId,
                    CompanyId = requestDC.CompanyId,
                    LeadId = requestDC.LeadId,
                    Code = "",
                    ProcessedResponse = responseFullpath,
                    Request = requestFullpath,
                    Response = responseFullpath,
                    IsError = isError,
                    ThirdPartyApiConfigId = apiConfigdataFull.Id
                };
                ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
                var res = thirdPartyRequestManager.SaveThirdPartyRequest(thirdPartyRequestDTO);
                //await InsertReqestReponseAsync(addresponse);
            }
            return responseDC;
        }
        public async Task<ExperianOTPValidationResponseDC> MaskedMobileOTPValidationAsync(MaskedMobileOTPValidationRequestDC requestDC, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            ExperianOTPValidationResponseDC responseDC = new ExperianOTPValidationResponseDC();
            ThirdPartyAPIConfigResult<MaskedMobileOTPValidationDTO> apiConfigdataFull = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<MaskedMobileOTPValidationDTO>(ThirdPartyApiConfigConstant.MaskedMobileOTPValidation);
            MaskedMobileOTPValidationDTO apiConfigdata = null;
            if (apiConfigdataFull != null)
            {
                apiConfigdata = apiConfigdataFull.Config;
            }
            if (apiConfigdata != null)
            {
                AddRequestResponseDc addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.ApiUrl))
                        {
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            var contentList = new List<string>();
                            contentList.Add($"stgOneHitId={Uri.EscapeDataString(requestDC.stgOneHitId)}");
                            contentList.Add($"stgTwoHitId={Uri.EscapeDataString(requestDC.stgTwoHitId)}");
                            contentList.Add($"otp={Uri.EscapeDataString(requestDC.otp)}");
                            contentList.Add($"mobileNo={Uri.EscapeDataString(requestDC.maskedMobile)}");
                            contentList.Add($"type={Uri.EscapeDataString(apiConfigdata.Other)}");

                            request.Content = new StringContent(string.Join("&", contentList));
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                            var reqmsg = JsonConvert.SerializeObject(requestDC);
                            AddRequestResponseDc addRequest = new AddRequestResponseDc()
                            {
                                Header = request.Headers.ToString(),
                                RequestResponseMsg = reqmsg,
                                Type = "Request",
                                Url = apiConfigdata.ApiUrl
                            };
                            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                            requestFullpath = Path.Combine(basePath, "wwwroot");
                            requestFilecontent = JsonConvert.SerializeObject(addRequest);
                            requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);

                            //await InsertReqestReponseAsync(addRequest);

                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<ExperianOTPValidationResponseDC>(jsonString);
                                if (!string.IsNullOrEmpty(responseDC.errorString) && responseDC.errorString == "OTP validation already tried,register consumer again for new OTP")
                                {
                                    responseDC.errorString = "OTP validation already tried,please click on resend for new OTP";
                                }
                                if (string.IsNullOrEmpty(responseDC.errorString))
                                {
                                    string xmlres = HttpUtility.HtmlDecode(responseDC.showHtmlReportForCreditReport);
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(xmlres);
                                    XmlNodeList xnList = doc.SelectNodes("/INProfileResponse/SCORE");
                                    foreach (XmlNode xn in xnList)
                                    {
                                        responseDC.CreditScore = xn["BureauScore"].InnerText;
                                    }
                                    try
                                    {
                                        requestFilename = "BureauReport_" + Guid.NewGuid().ToString() + ".json";
                                        requestFullpath = Path.Combine(basePath, "wwwroot/CibiReport");
                                        requestFilecontent = xmlres;
                                        requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                                        responseDC.FilePath = requestFullpath;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.errorString = "please try again";
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                responseDC.errorString = "please try again,after sometime";
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC.errorString = jsonString;
                            }
                            addresponse = new AddRequestResponseDc()
                            {
                                Header = response.Headers.ToString(),
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = apiConfigdata.ApiUrl
                            };
                            responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot");
                            responseFilecontent = JsonConvert.SerializeObject(addresponse);
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            isError = false;
                            //await InsertReqestReponseAsync(addresponse);
                        }
                    }
                }
                catch (Exception ex)
                {
                    addresponse = new AddRequestResponseDc()
                    {
                        Header = string.Empty,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = apiConfigdata.ApiUrl
                    };
                    responseDC.errorString = ex.Message.ToString();
                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);

                    //TextFileLogHelper.TraceLog("Error in ExperianOTPValidationAsync ...." + ex.Message.ToString());
                    //await InsertReqestReponseAsync(addresponse);
                }
                ThirdPartyRequestDTO thirdPartyRequestDTO = new ThirdPartyRequestDTO
                {
                    SubActivityId = requestDC.SubActivityId,
                    ActivityId = requestDC.ActivityId,
                    CompanyId = requestDC.CompanyId,
                    LeadId = requestDC.LeadId,
                    Code = "",
                    ProcessedResponse = responseFullpath,
                    Request = requestFullpath,
                    Response = responseFullpath,
                    IsError = isError,
                    ThirdPartyApiConfigId = apiConfigdataFull.Id
                };
                ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
                var res = thirdPartyRequestManager.SaveThirdPartyRequest(thirdPartyRequestDTO);
            }
            return responseDC;
        }
        #endregion
    }
}
