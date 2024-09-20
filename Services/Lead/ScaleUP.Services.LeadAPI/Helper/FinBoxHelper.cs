using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Crmf;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Constants;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.eSign;
using ScaleUP.Services.LeadDTO.FinBox;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using ScaleUP.Services.LeadModels;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using static ScaleUP.Services.LeadAPI.Helper.NBFC.ArthMateNBFCHelper;

namespace ScaleUP.Services.LeadAPI.Helper
{
    public class FinBoxHelper
    {
        private readonly LeadApplicationDbContext _context;
        private ThirdPartyApiConfigManager thirdPartyApiConfigManager;
        private IHostEnvironment _hostingEnvironment;
        public FinBoxHelper(LeadApplicationDbContext context, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            _hostingEnvironment = hostingEnvironment;
            _hostingEnvironment = hostingEnvironment;
        }



        public async Task<CreateSessionResponse> CreateSessionAsync(CreateSessionPost req, string ApiURL, long LeadId, string basePath)
        {
            CreateSessionResponse responsedc = new CreateSessionResponse();
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            int StatusCode = 0;
            bool IsSuccess = false;

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            DateTime CurrentDate = dateConvertHelper.GetIndianStandardTime();

            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            if (req != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiURL))
                        {
                            //request.Headers.Add("api_key", req.api_key);
                            var reqmsg = JsonConvert.SerializeObject(req);

                            request.Content = new StringContent(reqmsg);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                            requestFullpath = Path.Combine(basePath, "wwwroot");
                            requestFilecontent = reqmsg;
                            requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);

                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                IsSuccess = true;
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                responsedc = JsonConvert.DeserializeObject<CreateSessionResponse>(jsonString);

                                responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                                responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                                responseFilecontent = jsonString;
                                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync());
                                IsSuccess = false;
                                responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                                responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                                responseFilecontent = jsonString;
                                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                                var errorRes = JsonConvert.DeserializeObject<Error>(jsonString);
                                responsedc.code = errorRes != null ? errorRes.code : "";
                                responsedc.message = errorRes != null ? errorRes.message : "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                    responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                    responseFilecontent = ex.ToString();
                    responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                }
                FinBoxRequestResponseDTO finboxRequestDTO = new FinBoxRequestResponseDTO
                {
                    LeadId = LeadId,
                    Request = requestFullpath,
                    Response = responseFullpath,
                    URL = ApiURL,
                    StatusCode = StatusCode,
                    IsSuccess = IsSuccess,
                };
                ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
                var res = thirdPartyRequestManager.SaveFinBoxRequestResponse(finboxRequestDTO);
            }
            return responsedc;
        }

        public async Task<UploadSessionResponse> UploadSessionAsync(UploadSessionPost 
            req, string ApiURL, long LeadId, string apikey, string serverhash, string basePath)
        {
            UploadSessionResponse responsedc = new UploadSessionResponse();
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            int StatusCode = 0;
            bool IsSuccess = false;

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            DateTime CurrentDate = dateConvertHelper.GetIndianStandardTime();

            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            try
            {
                HttpClient client = new HttpClient();
                using (var form = new MultipartFormDataContent())
                {
                    // Add file
                    string filename = Path.GetFileName(req.file);
                    byte[] data = null;
                    data = FileSaverHelper.GetBytecodeFromUrl(req.file);
                    string FilePath = Path.Combine(basePath, "wwwroot", "OtherDoc");

                    FilePath = Path.Combine(FilePath, filename);
                    File.WriteAllBytes(FilePath, data);

                    // pdfReportDc.file = FilePath;
                    var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(FilePath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
                    form.Add(fileContent, "file", FilePath);

                    // Add other parameters
                    form.Add(new StringContent(req.bank_name), "bank_name");
                    form.Add(new StringContent(req.session_id), "session_id");
                    form.Add(new StringContent(req.upload_type), "upload_type");

                    // Add headers
                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders.Add("server-hash", serverhash);

                    var reqmsg = JsonConvert.SerializeObject(req);
                    requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                    requestFullpath = Path.Combine(basePath, "wwwroot");
                    requestFilecontent = reqmsg;
                    requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);

                    // Send request
                    var response = await client.PostAsync(ApiURL, form);

                    string jsonString = string.Empty;
                    StatusCode = Convert.ToInt32(response.StatusCode);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        IsSuccess = true;
                        jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                        responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                        responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                        responseFilecontent = jsonString;
                        responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                        responsedc = JsonConvert.DeserializeObject<UploadSessionResponse>(jsonString);
                    }
                    else
                    {
                        jsonString = (await response.Content.ReadAsStringAsync());
                        IsSuccess = false;
                        responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                        responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                        responseFilecontent = jsonString;
                        responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                        var errorRes = JsonConvert.DeserializeObject<UploadSessionError>(jsonString);

                        responsedc.code = errorRes != null ? errorRes.error.code : "";
                        responsedc.message = errorRes != null ? errorRes.error.message : "";
                        responsedc.session_id = errorRes != null ? errorRes.session_id : "";
                        responsedc.statement_id = errorRes != null ? errorRes.statement_id : "";
                    }
                }
            }
            catch (Exception ex)
            {
                responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                responseFilecontent = ex.ToString();
                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
            }
            FinBoxRequestResponseDTO finboxRequestDTO = new FinBoxRequestResponseDTO
            {
                LeadId = LeadId,
                Request = requestFullpath,
                Response = responseFullpath,
                URL = ApiURL,
                StatusCode = StatusCode,
                IsSuccess = IsSuccess,
            };
            ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
            var res = thirdPartyRequestManager.SaveFinBoxRequestResponse(finboxRequestDTO);
            return responsedc;
        }




        public async Task<SessionUploadStatusResponse> SessionUploadStatus(finboxConfig req, string basePath, string session_id, long LeadId)
        {

            SessionUploadStatusResponse responseDc = new SessionUploadStatusResponse();
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;

            int StatusCode = 0;
            bool IsSuccess = false;
            string jsonString = string.Empty;


            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string apiUrl = req.ApiURL.Replace("<session_id>", session_id);
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), apiUrl))
                    {
                        request.Headers.Add("x-api-key", req.APIKey);
                        request.Headers.Add("server-hash", req.ServerHash);
                        //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);

                        StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            responseDc = JsonConvert.DeserializeObject<SessionUploadStatusResponse>(jsonString);


                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            IsSuccess = false;

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            var errorRes = JsonConvert.DeserializeObject<Error>(jsonString);
                            responseDc.code = errorRes != null ? errorRes.code : "";
                            responseDc.message = errorRes != null ? errorRes.message : "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                responseFilecontent = ex.ToString();
                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
            }
  
            FinBoxRequestResponseDTO finboxRequestDTO = new FinBoxRequestResponseDTO
            {
                LeadId = LeadId,
                Request = requestFullpath,
                Response = responseFullpath,
                URL = req.ApiURL,
                StatusCode = StatusCode,
                IsSuccess = IsSuccess,
            };
            ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
            var res = thirdPartyRequestManager.SaveFinBoxRequestResponse(finboxRequestDTO);
            return responseDc;

        }

        public async Task<InitiateProcessingResponse> InitiateProcessing(finboxConfig req, string basePath, string session_id, long LeadId)
        {
            InitiateProcessingResponse responseDc = new InitiateProcessingResponse();
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;

            int StatusCode = 0;
            bool IsSuccess = false;
            string jsonString = string.Empty;


            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string apiUrl = req.ApiURL.Replace("<session_id>", session_id);
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiUrl))
                    {
                        request.Headers.Add("x-api-key", req.APIKey);
                        request.Headers.Add("server-hash", req.ServerHash);
                        //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);

                        StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            responseDc = JsonConvert.DeserializeObject<InitiateProcessingResponse>(jsonString);

                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            IsSuccess = false;

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);

                            var errorRes = JsonConvert.DeserializeObject<ErrorRes>(jsonString);
                            responseDc.code = errorRes != null ? errorRes.error.code : "";
                            responseDc.message = errorRes != null ? errorRes.error.message : "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                responseFilecontent = ex.ToString();
                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
            }
            FinBoxRequestResponseDTO finboxRequestDTO = new FinBoxRequestResponseDTO
            {
                LeadId = LeadId,
                Request = requestFullpath,
                Response = responseFullpath,
                URL = req.ApiURL,
                StatusCode = StatusCode,
                IsSuccess = IsSuccess,
            };
            ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
            var res = thirdPartyRequestManager.SaveFinBoxRequestResponse(finboxRequestDTO);
            return responseDc;

        }

        public async Task<ProcessingStatusResponse> ProcessingStatus(finboxConfig req, string basePath, string session_id, long LeadId)
        {

            ProcessingStatusResponse responseDc = new ProcessingStatusResponse();
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;

            int StatusCode = 0;
            bool IsSuccess = false;
            string jsonString = string.Empty;


            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string apiUrl = req.ApiURL.Replace("<session_id>", session_id);
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), apiUrl))
                    {
                        request.Headers.Add("x-api-key", req.APIKey);
                        request.Headers.Add("server-hash", req.ServerHash);
                        //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);

                        StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            responseDc = JsonConvert.DeserializeObject<ProcessingStatusResponse>(jsonString);
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            IsSuccess = false;

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);


                            var errorRes = JsonConvert.DeserializeObject<ErrorRes>(jsonString);
                            responseDc.code = errorRes != null ? errorRes.error.code : "";
                            responseDc.message = errorRes != null ? errorRes.error.message : "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                responseFilecontent = ex.ToString();
                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
            }
            FinBoxRequestResponseDTO finboxRequestDTO = new FinBoxRequestResponseDTO
            {
                LeadId = LeadId,
                Request = requestFullpath,
                Response = responseFullpath,
                URL = req.ApiURL,
                StatusCode = StatusCode,
                IsSuccess = IsSuccess,
            };
            ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
            var res = thirdPartyRequestManager.SaveFinBoxRequestResponse(finboxRequestDTO);
            return responseDc;

        }

        public async Task<SessionStatusResponse> SessionStatus(finboxConfig req, string basePath, string session_id, long LeadId)
        {

            SessionStatusResponse responseDc = new SessionStatusResponse();
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;

            int StatusCode = 0;
            bool IsSuccess = false;
            string jsonString = string.Empty;


            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string apiUrl = req.ApiURL.Replace("<session_id>", session_id);
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), apiUrl))
                    {
                        request.Headers.Add("x-api-key", req.APIKey);
                        request.Headers.Add("server-hash", req.ServerHash);
                        //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);

                        StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            responseDc = JsonConvert.DeserializeObject<SessionStatusResponse>(jsonString);
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            IsSuccess = false;

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);


                            var errorRes = JsonConvert.DeserializeObject<ErrorRes>(jsonString);
                            responseDc.code = errorRes != null ? errorRes.error.code : "";
                            responseDc.message = errorRes != null ? errorRes.error.message : "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                responseFilecontent = ex.ToString();
                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
            }

            FinBoxRequestResponseDTO finboxRequestDTO = new FinBoxRequestResponseDTO
            {
                LeadId = LeadId,
                Request = requestFullpath,
                Response = responseFullpath,
                URL = req.ApiURL,
                StatusCode = StatusCode,
                IsSuccess = IsSuccess,
            };
            ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
            var res = thirdPartyRequestManager.SaveFinBoxRequestResponse(finboxRequestDTO);
            return responseDc;

        }

        public async Task<InsightsResponse> Insights(finboxConfig req, string basePath, string session_id, long LeadId)
        {
            InsightsResponse responseDc = new InsightsResponse();
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;

            int StatusCode = 0;
            bool IsSuccess = false;
            string jsonString = string.Empty;


            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            string apiUrl = req.ApiURL.Replace("<session_id>", session_id);
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), apiUrl))
                    {
                        request.Headers.Add("x-api-key", req.APIKey);
                        request.Headers.Add("server-hash", req.ServerHash);
                        //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);

                        StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            IsSuccess = true;
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
                            responseDc = JsonConvert.DeserializeObject<InsightsResponse>(jsonString);

                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            IsSuccess = false;

                            responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                            responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                            responseFilecontent = jsonString;
                            responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);


                            var errorRes = JsonConvert.DeserializeObject<ErrorRes>(jsonString);
                            responseDc.code = errorRes != null ? errorRes.error.code : "";
                            responseDc.message = errorRes != null ? errorRes.error.message : "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseFilename = "CreateSession_" + Guid.NewGuid().ToString() + ".json";
                responseFullpath = Path.Combine(basePath, "wwwroot/FinBox");
                responseFilecontent = ex.ToString();
                responseFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
            }
            FinBoxRequestResponseDTO finboxRequestDTO = new FinBoxRequestResponseDTO
            {
                LeadId = LeadId,
                Request = requestFullpath,
                Response = responseFullpath,
                URL = req.ApiURL,
                StatusCode = StatusCode,
                IsSuccess = IsSuccess,
            };
            ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
            var res = thirdPartyRequestManager.SaveFinBoxRequestResponse(finboxRequestDTO);
            return responseDc;

        }
    }
}
