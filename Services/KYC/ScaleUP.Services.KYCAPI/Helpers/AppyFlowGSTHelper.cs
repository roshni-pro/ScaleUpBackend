using Newtonsoft.Json;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;
using System.Net;

namespace ScaleUP.Services.KYCAPI.Helpers
{
    public class AppyFlowGSTHelper
    {
        private readonly ApplicationDbContext _context;
        private ThirdPartyAPIConfigManager thirdPartyAPIConfigManager;
        public AppyFlowGSTHelper(ApplicationDbContext context)
        {
            _context = context;
            thirdPartyAPIConfigManager = new ThirdPartyAPIConfigManager(_context);
        }
        public async Task<AppyFlowGSTResDTO> AppyFlowGSTVerification(string GSTNumber)
        {
            try
            {
                string apiType = ThirdPartyAPIConfigCodeConstants.AppyFlowGSTInfo;
                AppyFlowGSTResDTO response = new AppyFlowGSTResDTO();
                var apiConfigdata = await thirdPartyAPIConfigManager.GetByCode(apiType);

                if (apiConfigdata != null)
                {
                    apiConfigdata.URL = apiConfigdata.URL.Replace("[[GstNo]]", GSTNumber);

                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), apiConfigdata.URL + apiConfigdata.Secret))
                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                            var res = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            if (res.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await res.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                var result = JsonConvert.DeserializeObject<AppyFlowGSTResponseDTO>(jsonString);
                                if (result != null && !result.error)
                                {
                                    AppyFlowGSTDTO appyFlowGSTDTO = new AppyFlowGSTDTO
                                    {
                                        BusinessName = result.taxpayerInfo.tradeNam,
                                        LandingName = result.taxpayerInfo.lgnm,
                                        Address = result.taxpayerInfo.pradr.addr.bno,
                                        CityName = result.taxpayerInfo.pradr.addr.city,
                                        StateName = result.taxpayerInfo.pradr.addr.stcd,
                                        ZipCode = result.taxpayerInfo.pradr.addr.pncd
                                    };
                                    response.GSTInfo = appyFlowGSTDTO;
                                    response.Status = true;
                                    response.Message = "GST is Validated";
                                }
                            }
                            else if (res.StatusCode == HttpStatusCode.BadRequest)
                            {
                                response.Message = "Your request is invalid.";
                            }
                            else if (res.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                response.Message = "please try again,after sometime.";
                            }
                            else if (res.StatusCode == HttpStatusCode.Forbidden)
                            {
                                response.Message = "The API requested is hidden for administrators only.";
                            }
                            else if (res.StatusCode == HttpStatusCode.NotFound)
                            {
                                response.Message = "The specified API could not be found.";
                            }
                            else if (res.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                response.Message = "We had a problem with our server.Try again later.";
                            }
                            else if (res.StatusCode == HttpStatusCode.BadGateway || res.StatusCode == HttpStatusCode.GatewayTimeout || res.StatusCode == HttpStatusCode.RequestTimeout)
                            {
                                response.Message = "please try again";
                            }
                            else if (res.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                response.Message = "please try again,after sometime";
                            }
                            else
                            {
                                response.Message = await res.Content.ReadAsStringAsync();
                            }
                        }
                    }

                }
                else
                {
                    response.Status = false;
                    response.Message = "Invalid API Url";
                }
                return response;
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}

