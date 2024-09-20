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
    public class KarzaElectricityHelper
    {
        private readonly ApplicationDbContext _context;
        private ThirdPartyAPIConfigManager thirdPartyAPIConfigManager;
        public KarzaElectricityHelper(ApplicationDbContext context)
        {
            _context = context;
            thirdPartyAPIConfigManager = new ThirdPartyAPIConfigManager(_context);
        }

        #region KarzaElectricityBillAuthentication
        public async Task<KarzaElectricityBillAuthenticationDTO> KarzaElectricity(KarzaElectricityInputDTO karzaElectricityInputDTO, string basePath, string userId)
        {
            bool isError = true;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            string requestFileFullPath = string.Empty;
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string responseFileFullPath = string.Empty;

            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string karzaType = ThirdPartyAPIConfigCodeConstants.KarzaElectricityBillAuthentication;
            KarzaElectricityBillAuthenticationDTO result = new KarzaElectricityBillAuthenticationDTO();
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

                            karzaElectricityInputDTO.consent = "Y";
                            string jsonstringReq = JsonConvert.SerializeObject(karzaElectricityInputDTO);
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
                                requestFileFullPath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);
                                isError = false;
                            }
                            catch (Exception)
                            {
                            }

                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result = JsonConvert.DeserializeObject<KarzaElectricityBillAuthenticationDTO>(jsonString);
                                if (Convert.ToInt32(result.StatusCode) != 101)
                                    result.error = "";
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

            }catch(Exception ex)
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
            return result;
        }
        #endregion
        #region DateFormatReturn
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

    }
}
