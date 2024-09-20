using Newtonsoft.Json;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.BlackSoil;
using ScaleUP.Services.LeadModels.LeadNBFC;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Microsoft.OpenApi.Models;
using System;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.LeadModels.ArthMate;
using Google.Protobuf.WellKnownTypes;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ScaleUP.Services.LeadModels;
using Azure.Storage.Blobs;
using ScaleUP.Services.LeadAPI.Migrations;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using ScaleUP.Services.LeadAPI.Constants;

namespace ScaleUP.Services.LeadAPI.Helper.NBFC
{
    public class ArthMateNBFCHelper
    {

        public async Task<ArthMateCommonAPIRequestResponse> GenerateLead(List<LeadPostdc> lead, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;

            if (lead != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", Key);
                            var reqmsg = JsonConvert.SerializeObject(lead);
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

        public async Task<ArthMateCommonAPIRequestResponse> LoanDocumentApi(LoanDocumentPostDc document, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId, string basePath = "")
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            string reqString = "";

            //string responseFilename = string.Empty;
            //string responseFullpath = string.Empty;
            //string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            if (document.IsBankStatement)
            {
                LoanDocumentBankDc obj = new LoanDocumentBankDc()
                {
                    base64pdfencodedfile = document.base64pdfencodedfile,
                    borrower_id = document.borrower_id,
                    code = document.code,
                    doc_key = document.PdfPassword,
                    loan_app_id = document.loan_app_id,
                    partner_borrower_id = document.partner_borrower_id,
                    partner_loan_app_id = document.partner_loan_app_id,
                    fileType = "bank_stmnts"
                };
                reqString = JsonConvert.SerializeObject(obj);
            }
            else
            {
                var req = new LoanDocumentDc
                {
                    base64pdfencodedfile = document.base64pdfencodedfile,
                    borrower_id = document.borrower_id,
                    code = document.code,
                    loan_app_id = document.loan_app_id,
                    partner_borrower_id = document.partner_borrower_id,
                    partner_loan_app_id = document.loan_app_id
                };
                reqString = JsonConvert.SerializeObject(req);
            }

            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
            requestFullpath = Path.Combine(basePath, "wwwroot");
            requestFilecontent = reqString;
            requestFullpath = fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);

            if (document != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", Key);
                            //string reqmsg = JsonConvert.SerializeObject(req);
                            //result.Request = reqString;
                            result.Request = requestFullpath;

                            request.Content = new StringContent(reqString);
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

        public async Task<ArthMateCommonAPIRequestResponse> AscoreApi(AScoreAPIRequest scorereq, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (scorereq != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", SecretKey);
                            var reqmsg = JsonConvert.SerializeObject(scorereq);
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

        public async Task<ArthMateCommonAPIRequestResponse> PanVerificationAsync(PanVerificationRequestJsonV3 RequestJson, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (RequestJson != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", SecretKey);
                            var reqmsg = JsonConvert.SerializeObject(RequestJson);
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

        public async Task<ArthMateCommonAPIRequestResponse> CoLenderApi(CoLenderRequest req, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {


            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (req != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", SecretKey);
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

        public async Task<ArthMateCommonAPIRequestResponse> GenerateOtpToAcceptOffer(FirstAadharXMLPost req, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (req != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            req.consent = "Y";
                            req.consent_timestamp = result.Created.ToString("yyyy-MM-dd HH:mm:ss");
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", SecretKey);
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
        public async Task<ArthMateCommonAPIRequestResponse> StepTwoAadharToAcceptOffer(SecondAadharXMLPost req, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (req != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            req.consent = "Y";
                            req.consent_timestamp = result.Created.ToString("yyyy-MM-dd HH:mm:ss");
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", SecretKey);
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

        public async Task<ArthMateCommonAPIRequestResponse> LoanApi(List<LoanApiRequestDc> req, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;

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

        public async Task<ArthMateCommonAPIRequestResponse> repayment_schedule(Postrepayment_scheduleDc req, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId, bool isTest = false)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 

            ApiUrl = $"{ApiUrl}/{req.loan_id}";
            if (isTest)
            {
                //ApiUrl = $"https://apiorigin.arthmate.com/api/repayment_schedule/{req.loan_id}";
                ApiUrl = ApiUrl;
                //SecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo5MDkzMTAxLCJjb21wYW55X2NvZGUiOiJTSE8wMTQ2IiwicHJvZHVjdF9pZCI6OTA5MzQyNSwidXNlcl9pZCI6NTg4MjYyNywidXNlcl9uYW1lIjoiWWF0cmVlIFBhbmR5YSIsInR5cGUiOiJhcGkiLCJsb2FuX3NjaGVtYV9pZCI6IjkwOTMyNzUiLCJjcmVkaXRfcnVsZV9ncmlkX2lkIjpudWxsLCJhdXRvbWF0aWNfY2hlY2tfY3JlZGl0IjowLCJ0b2tlbl9pZCI6IjkwOTMxMDEtOTA5MzQyNS0xNzAyNTM5NjcxNzk0IiwiZW52aXJvbm1lbnQiOiJwcm9kIiwiaWF0IjoxNzAyNTM5NjcxfQ.o0S80PrHEi9du-8qXYjlp7OBospyg3aBsrXqMKmGSgw";
                SecretKey = SecretKey;
            }
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
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

        public async Task<ArthMateCommonAPIRequestResponse> GetLeadApi(string loan_app_id, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (loan_app_id != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", SecretKey);
                            //var reqmsg = JsonConvert.SerializeObject(req);
                            result.Request = loan_app_id;

                            //request.Content = new StringContent(reqmsg);
                            //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

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

        public async Task<ArthMateCommonAPIRequestResponse> borrowerinfostatusupdate(LoanStatusChangeAPIReq req, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (req != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), ApiUrl))
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

        public async Task<ArthMateCommonAPIRequestResponse> PostCplr(CeplrPostDc CeplrPost, string TAPISecretKey, string APIUrl, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = APIUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            result.Request = JsonConvert.SerializeObject(CeplrPost);

            string pdfFilePath = CeplrPost.filepath;
            FileStream fs = new FileStream(pdfFilePath, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();
            string filename = Path.GetFileName(pdfFilePath);
            try
            {
                // Generate post objects
                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                //postParameters.Add("filename", filename);
                //postParameters.Add("fileformat", "pdf");
                postParameters.Add("file", new FormUpload.FileParameter(data, filename, "application/pdf"));
                postParameters.Add("email", CeplrPost.email);
                postParameters.Add("ifsc_code", CeplrPost.ifsc_code);
                postParameters.Add("fip_id", (string.IsNullOrEmpty(CeplrPost.fip_id) ? "" : CeplrPost.fip_id));
                postParameters.Add("callback_url", CeplrPost.callback_url);
                postParameters.Add("mobile", CeplrPost.mobile);
                postParameters.Add("name", CeplrPost.name);
                postParameters.Add("file_password", (string.IsNullOrEmpty(CeplrPost.file_password) ? "" : CeplrPost.file_password));
                postParameters.Add("configuration_uuid", CeplrPost.configuration_uuid);
                if (CeplrPost.allow_multiple == true)
                {
                    postParameters.Add("allow_multiple", (CeplrPost.allow_multiple ? "true" : "false"));
                }
                if (CeplrPost.allow_multiple == true && CeplrPost.request_id > 0)
                {
                    postParameters.Add("token", CeplrPost.token);
                    postParameters.Add("request_id", CeplrPost.request_id);
                }
                if (CeplrPost.last_file == true)
                {
                    postParameters.Add("last_file", (CeplrPost.last_file ? "true" : "false"));
                }
                // Create request and receive response
                string postURL = APIUrl;
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36";

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postURL, userAgent, postParameters, TAPISecretKey);

                // Process response
                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                result.Response = responseReader.ReadToEnd();
                webResponse.Close();
                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Response = ex.ToString();
                result.IsSuccess = false;
                return result;
            }
        }
        public static class FormUpload
        {
            private static readonly Encoding encoding = Encoding.UTF8;
            public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters, string apikey)
            {
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
                string contentType = "multipart/form-data; boundary=" + formDataBoundary;

                byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

                return PostForm(postUrl, userAgent, contentType, formData, apikey);
            }
            private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData, string apikey)
            {
                //ServicePointManager.Expect100Continue = true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

                if (request == null)
                {
                    throw new NullReferenceException("request is not a http request");
                }

                request.Headers.Add("x-api-key", apikey);
                // Set up the request properties.
                request.Method = "POST";
                request.ContentType = contentType;
                request.UserAgent = userAgent;
                request.CookieContainer = new CookieContainer();
                request.ContentLength = formData.Length;

                // You could add authentication here as well if needed:
                // request.PreAuthenticate = true;
                // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

                // Send the form data to the request.
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Close();
                }
                //try
                //{
                return request.GetResponse() as HttpWebResponse;
                //}
                //catch (Exception es)
                //{

                //    throw;
                //}

            }
            private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
            {
                Stream formDataStream = new System.IO.MemoryStream();
                bool needsCLRF = false;

                foreach (var param in postParameters)
                {
                    // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                    // Skip it on the first parameter, add it to subsequent parameters.
                    if (needsCLRF)
                        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                    needsCLRF = true;

                    if (param.Value is FileParameter)
                    {
                        FileParameter fileToUpload = (FileParameter)param.Value;

                        // Add just the first part of this param, since we will write the file data directly to the Stream
                        string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                            boundary,
                            param.Key,
                            fileToUpload.FileName ?? param.Key,
                            fileToUpload.ContentType ?? "application/octet-stream");

                        formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                        // Write the file data directly to the Stream, rather than serializing it to a string.
                        formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                    }
                    else
                    {
                        string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                            boundary,
                            param.Key,
                            param.Value);
                        formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                    }
                }

                // Add the end of the request.  Start with a newline
                string footer = "\r\n--" + boundary + "--\r\n";
                formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

                // Dump the Stream into a byte[]
                formDataStream.Position = 0;
                byte[] formData = new byte[formDataStream.Length];
                formDataStream.Read(formData, 0, formData.Length);
                formDataStream.Close();

                return formData;
            }
            public class FileParameter
            {
                public byte[] File { get; set; }
                public string FileName { get; set; }
                public string ContentType { get; set; }
                public FileParameter(byte[] file) : this(file, null) { }
                public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
                public FileParameter(byte[] file, string filename, string contenttype)
                {
                    File = file;
                    FileName = filename;
                    ContentType = contenttype;
                }
            }
        }
        public async Task<ArthMateCommonAPIRequestResponse> CeplrBasicReport(CeplrBasicReportDc basicdata, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (basicdata != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            //request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", Key);
                            request.Headers.Add("x-api-key", Key);
                            var reqmsg = JsonConvert.SerializeObject(basicdata);
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

        public string ConvertToBase64String(string FileUrl)
        {
            string DocBase64String = "";
            //var fileurl = string.Concat(HttpRuntime.AppDomainAppPath, FileUrl.Replace("/", "\\"));
            //FileUrl = fileurl;

            //string extension = Path.GetExtension(FileUrl);
            //string ext = extension.Trim().ToLower();
            //if (ext == ".jpeg" || ext == ".png" || ext == ".jpg")
            //{
            //    string folderPath = HttpContext.Current.Server.MapPath("~/ArthmateDocument/OtherDoc");
            //    string filename = "ArthmateDoc_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
            //    if (!Directory.Exists(folderPath))
            //    {
            //        Directory.CreateDirectory(folderPath);
            //    }
            //    string path = Path.Combine(folderPath, filename);


            //    iTextSharp.text.Rectangle pageSize = null;

            //    using (var srcImage = new Bitmap(FileUrl))
            //    {
            //        pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
            //    }
            //    using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            //    {
            //        var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
            //        PdfWriter.GetInstance(document, stream);

            //        document.Open();
            //        using (var imageStream = new FileStream(FileUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //        {
            //            var image = iTextSharp.text.Image.GetInstance(imageStream);
            //            document.Add(image);
            //        }
            //        document.Close();
            //        byte[] bytes = File.ReadAllBytes(stream.Name);
            //        DocBase64String = Convert.ToBase64String(bytes);
            //    }
            //}
            //else
            //{
            //    byte[] bytes = File.ReadAllBytes(FileUrl);
            //    DocBase64String = Convert.ToBase64String(bytes);
            //}
            return DocBase64String;
        }

        public async Task<ArthMateCommonAPIRequestResponse> ArthmateDocValidationJsonXml(JsonXmlRequest RequestJson, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (RequestJson != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", Key);
                            var reqmsg = JsonConvert.SerializeObject(RequestJson);
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
        public async Task<ArthMateCommonAPIRequestResponse> CeplrBankList(string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            //if (RequestJson != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), ApiUrl))
                        {
                            request.Headers.Add("x-api-key", Key);
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

        public async Task<ArthMateCommonAPIRequestResponse> LoanNachPatchAPI(LoanNachAPIDc LoanNachAPI, string loanId, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            ApiUrl = $"{ApiUrl}/{loanId}";
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (LoanNachAPI != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", Key);
                            var reqmsg = JsonConvert.SerializeObject(LoanNachAPI);
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

        public async Task<ArthMateCommonAPIRequestResponse> GetLoanById(string loanappid, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse(); //need to change table name 
            ApiUrl = $"{ApiUrl}/{loanappid}";
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (!string.IsNullOrEmpty(loanappid))
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", Key);
                            //var reqmsg = JsonConvert.SerializeObject(LoanNachAPI);
                            result.Request = loanappid;

                            //request.Content = new StringContent(reqmsg);
                            //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

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
        public async Task<ArthMateCommonAPIRequestResponse> GetDisbursementAPI(string loanappid, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new ArthMateCommonAPIRequestResponse();
            ApiUrl = $"{ApiUrl}/{loanappid}";
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (!string.IsNullOrEmpty(loanappid))
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), ApiUrl))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", Key);
                            //var reqmsg = JsonConvert.SerializeObject(LoanNachAPI);
                            //result.Request = loanappid;

                            //request.Content = new StringContent(reqmsg);
                            //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

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
