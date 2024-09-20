using Newtonsoft.Json;
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
    public class KarzaHelper
    {
        private readonly ApplicationDbContext _context;
        private ThirdPartyAPIConfigManager thirdPartyAPIConfigManager;
        public KarzaHelper(ApplicationDbContext context)
        {
            _context = context;
            thirdPartyAPIConfigManager = new ThirdPartyAPIConfigManager(_context);
        }

        #region Karza Pan verification
        #region KarzaPanAuthentication
        public async Task<ValidAuthenticationPanResDTO> KarzaPanVerification(string PanNumber, string basePath, string userId)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;

            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);


            string karzaType = ThirdPartyAPIConfigCodeConstants.KarzaPANValidation;
            ValidAuthenticationPanResDTO result = new ValidAuthenticationPanResDTO();
            var apiConfigdata = await GetUrlTokenForApi(karzaType);
            try
            {

                if (apiConfigdata != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            ValidAuthenticationPanPost reqdc = new ValidAuthenticationPanPost();
                            reqdc.pan = PanNumber;
                            reqdc.consent = "Y";
                            string jsonstringReq = JsonConvert.SerializeObject(reqdc);
                            request.Content = new StringContent(jsonstringReq);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

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
                                requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            }
                            catch (Exception)
                            {

                                //throw;
                            }

                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result = JsonConvert.DeserializeObject<ValidAuthenticationPanResDTO>(jsonString);
                                if (result.StatusCode != 101)
                                    result.error = await CheckAuthenticationError(result.StatusCode);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                result.error = "Your request is invalid.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                result.error = "please try again,after sometime.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                result.error = "The API requested is hidden for administrators only.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                result.error = "The specified API could not be found.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                result.error = "We had a problem with our server.Try again later.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                result.error = "please try again";
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                result.error = "please try again,after sometime";
                                jsonString = result.error;
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                result.error = jsonString;
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
                                responseFullpath =fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                                isError = false;
                            }
                            catch (Exception)
                            {
                                //throw;
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

                result.error = ex.Message.ToString();

                responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                responseFullpath = Path.Combine(basePath, "wwwroot");
                responseFilecontent = JsonConvert.SerializeObject(addresponse);
                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
            }
            ThirdPartyApiRequestDTO thirdPartyRequestDTO = new ThirdPartyApiRequestDTO
            {
                APIConfigId = apiConfigdata.id,
                UserId = userId,
                CompanyId = 0,
                ProcessedResponse = responseFullpath,
                Request = requestFullpath,
                Response = responseFullpath,
                IsError = isError
            };

            var res = await thirdPartyAPIConfigManager.InsertApiReqRes(thirdPartyRequestDTO);

            return result;
        }

        //ValidAuthenticationPan
        public async Task<ValidAuthenticationPanResDTO> ValidAuthenticationPan(string PanNumber, long LeadMasterId, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);


            string karzaType = ThirdPartyAPIConfigCodeConstants.KarzaPANValidation;
            ValidAuthenticationPanResDTO result = new ValidAuthenticationPanResDTO();
            var apiConfigdata = await GetUrlTokenForApi(karzaType);
            try
            {
                if (apiConfigdata != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            ValidAuthenticationPanPost reqdc = new ValidAuthenticationPanPost();
                            reqdc.pan = PanNumber;
                            reqdc.consent = "Y";
                            string jsonstringReq = JsonConvert.SerializeObject(reqdc);
                            request.Content = new StringContent(jsonstringReq);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

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
                                fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result = JsonConvert.DeserializeObject<ValidAuthenticationPanResDTO>(jsonString);
                                if (result.StatusCode != 101)
                                    result.error = await CheckAuthenticationError(result.StatusCode);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                result.error = "Your request is invalid.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                result.error = "please try again,after sometime.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                result.error = "The API requested is hidden for administrators only.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                result.error = "The specified API could not be found.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                result.error = "We had a problem with our server.Try again later.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                result.error = "please try again";
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                result.error = "please try again,after sometime";
                                jsonString = result.error;
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                result.error = jsonString;
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
                                fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
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
                    UserId = "",
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
                UserId = "",
                CompanyId = 0,
                ProcessedResponse = "wwwroot\\" + responseFilename,
                Request = "wwwroot\\" + requestFilename,
                Response = "wwwroot\\" + responseFilename,
                IsError = isError
            };
            var res = await thirdPartyAPIConfigManager.InsertApiReqRes(thirdPartyRequestDTO);
            return result;
        }
        private async Task<string> CheckAuthenticationError(int statuscode)
        {
            string errormsg = null;
            if (statuscode == 102)
                errormsg = "Invalid ID number or combination of inputs";
            else if (statuscode == 103)
                errormsg = "No records found for the given ID or combination of inputs";
            else if (statuscode == 104)
                errormsg = "Max retries exceeded";
            else if (statuscode == 105)
                errormsg = "Missing Consent";
            else if (statuscode == 106)
                errormsg = "Multiple Records Exist";
            else if (statuscode == 107)
                errormsg = "Not Supported";
            else if (statuscode == 108)
                errormsg = "Internal Resource Unavailable";
            else if (statuscode == 108)
                errormsg = "Too many records Found";
            return errormsg;
        }
        #endregion
        #region KarzaPANOCRAuthentication
        public async Task<KarzaPanOcrResDTO> KarzaOCRVerificationAsync(string imgurl, string basePath, string userid)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;

            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);


            string karzaType = ThirdPartyAPIConfigCodeConstants.KarzaPANOCRInfo;
            KarzaPanOcrResDTO responseDC = new KarzaPanOcrResDTO();
            var apiConfigdata = await GetUrlTokenForApi(karzaType);

            try
            {
                if (apiConfigdata != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            OcrPostDc OcrPost = new OcrPostDc();
                            OcrPost.url = imgurl;
                            OcrPost.docType = "file";
                            OcrPost.conf = true;
                            string jsonstringReq = JsonConvert.SerializeObject(OcrPost);
                            request.Content = new StringContent(jsonstringReq);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                            ApiRequestResponseDTO addRequest = new ApiRequestResponseDTO()
                            {
                                UserId = userid,
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
                                requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            }
                            catch (Exception)
                            {
                                    
                                //throw;
                            }
                            string jsonString = string.Empty;
                            var response = await httpClient.SendAsync(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                responseDC = JsonConvert.DeserializeObject<KarzaPanOcrResDTO>(jsonString);
                                if (responseDC.statusCode != 101)
                                    responseDC.error = await CheckOcrError(responseDC.statusCode);
                                if (responseDC.statusCode == 101)
                                {
                                    if (responseDC.result.FirstOrDefault(c => c.type == "Pan") != null)
                                    {
                                        var other = new KarzaPANDTO
                                        {
                                            date_of_birth = DateFormatReturn(responseDC.result.FirstOrDefault(c => c.type == "Pan").details.date.value),
                                            fathers_name = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.father.value,
                                            name_on_card = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.name.value,
                                            id_number = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.panNo.value,
                                            date_of_issue = DateFormatReturn(responseDC.result.FirstOrDefault(c => c.type == "Pan").details.dateOfIssue.value),
                                            age = Convert.ToInt32(DateTime.Now.Year - DateFormatReturn(responseDC.result.FirstOrDefault(c => c.type == "Pan").details.date.value).Year),
                                            id_scanned = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.id_scanned,
                                            minor = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.minor,
                                            pan_type = responseDC.result.FirstOrDefault(c => c.type == "Pan").details.pan_type
                                        };
                                        responseDC.OtherInfo = other;
                                    }
                                    else
                                    {
                                        responseDC.error = "Document Not Valid";
                                        jsonString = response.StatusCode.ToString();
                                    }
                                }
                            }

                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                responseDC.error = "please try again";
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {

                                responseDC.error = "please try again,after sometime";
                                jsonString = responseDC.error;
                            }

                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                responseDC.error = "your request is invalid.";
                                jsonString = responseDC.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                responseDC.error = "please try again,after sometime.";
                                jsonString = responseDC.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound)
                            {
                                responseDC.error = "something went wrong.";
                                jsonString = responseDC.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                responseDC.error = "we had a problem with our server. try again later.";
                                jsonString = responseDC.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.PaymentRequired)
                            {
                                responseDC.error = "Payment Required.";
                                jsonString = responseDC.error;
                            }
                            ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                            {
                                UserId = userid,
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
            }
            catch (Exception ex)
            {
                ApiRequestResponseDTO addresponse = new ApiRequestResponseDTO()
                {
                    UserId = userid,
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
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                    isError = false;
                }
                catch (Exception)
                {

                    //throw;
                }
            }

            ThirdPartyApiRequestDTO thirdPartyRequestDTO = new ThirdPartyApiRequestDTO
            {
                APIConfigId = apiConfigdata.id,
                UserId = userid,
                CompanyId = 0,
                ProcessedResponse = responseFullpath,
                Request = requestFullpath,
                Response = responseFullpath,
                IsError = isError
            };
            var res = await thirdPartyAPIConfigManager.InsertApiReqRes(thirdPartyRequestDTO);
            return responseDC;
        }
        private async Task<string> CheckOcrError(int statuscode)
        {
            string errormsg = null;
            if (statuscode == 102)
                errormsg = "No KYC Document identified";
            else if (statuscode == 103)
                errormsg = "Image Format Not Supported OR Size Exceeds 6MB";
            else if (statuscode == 104 || statuscode == 105 || statuscode == 106 || statuscode == 107 || statuscode == 108)
                errormsg = "N/A";
            return errormsg;
        }

        public DateTime DateFormatReturn(string sdate)
        {
            DateTime date;
            string[] formats = { "dd/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "d/MM/yyyy",
                                "dd/MM/yy", "dd/M/yy", "d/M/yy", "d/MM/yy", "MM/dd/yyyy",

            "dd-MM-yyyy", "dd-M-yyyy", "d-M-yyyy", "d-MM-yyyy",
                                "dd-MM-yy", "dd-M-yy", "d-M-yy", "d-MM-yy", "MM-dd-yyyy"
                                , "yyyy-MM-dd", "yyyy/MM/dd"
            };

            DateTime.TryParseExact(sdate, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);
            return date;
        }

        #endregion
        #region CommonKarzaPanConfig
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

        #endregion

        public async Task<ValidAuthenticationPanResDTO> ExistValidAuthenticationPan(string PanNumber,string basePath,string userId)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;

            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);


            ValidAuthenticationPanResDTO result = new ValidAuthenticationPanResDTO();
            string karzaType = ThirdPartyAPIConfigCodeConstants.KarzaPANValidation;
            var apiConfigdata = await GetUrlTokenForApi(karzaType);

            if (apiConfigdata != null)
            {
                //ApiRequestResponseDTO addresponse = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiConfigdata.url))
                        {
                            request.Headers.TryAddWithoutValidation("x-karza-key", apiConfigdata.ApiSecretKey);
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            ValidAuthenticationPanPost reqdc = new ValidAuthenticationPanPost();
                            reqdc.pan = PanNumber;
                            reqdc.consent = "Y";
                            string jsonstringReq = JsonConvert.SerializeObject(reqdc);
                            request.Content = new StringContent(jsonstringReq);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

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
                                requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                            }
                            catch (Exception)
                            {
                            }
                            //InsertRequestResponse(addRequest);
                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result = JsonConvert.DeserializeObject<ValidAuthenticationPanResDTO>(jsonString);
                                if (result.StatusCode != 101)
                                    result.error = await CheckAuthenticationError(result.StatusCode);
                            }
                            else if (response.StatusCode == HttpStatusCode.BadRequest)
                            {
                                result.error = "Your request is invalid.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                result.error = "please try again,after sometime.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                result.error = "The API requested is hidden for administrators only.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                result.error = "The specified API could not be found.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                result.error = "We had a problem with our server.Try again later.";
                                jsonString = result.error;
                            }
                            else if (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                result.error = "please try again";
                                jsonString = response.StatusCode.ToString();
                            }
                            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                result.error = "please try again,after sometime";
                                jsonString = result.error;
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                result.error = jsonString;
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
                                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                                isError = false;
                            }
                            catch (Exception)
                            {
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

                    result.error = ex.Message.ToString();

                    responseFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot");
                    responseFilecontent = JsonConvert.SerializeObject(addresponse);
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                }
                ThirdPartyApiRequestDTO thirdPartyRequestDTO = new ThirdPartyApiRequestDTO
                {
                    APIConfigId = apiConfigdata.id,
                    UserId = userId,
                    CompanyId = 0,
                    ProcessedResponse = responseFullpath,
                    Request = requestFullpath,
                    Response = responseFullpath,
                    IsError = isError
                };

                var res = await thirdPartyAPIConfigManager.InsertApiReqRes(thirdPartyRequestDTO);
              
            }
            return result;
        }
        #endregion

    }
}
