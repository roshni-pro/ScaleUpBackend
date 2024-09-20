using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Migrations;
using ScaleUP.Services.LeadDTO.NBFC.BlackSoil;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.LeadNBFC;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using static IdentityServer4.Models.IdentityResources;
using static MassTransit.ValidationResultExtensions;

namespace ScaleUP.Services.LeadAPI.Helper.NBFC
{
    public class BlackSoilNBFCHelper
    {
        public async Task<BlackSoilCommonAPIRequestResponse> CreateLead(BlackSoilCreateLeadInput BlackSoilCreateLeadInput, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;

            if (BlackSoilCreateLeadInput != null)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                            var reqmsg = JsonConvert.SerializeObject(BlackSoilCreateLeadInput);
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
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            return result;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> CreateBusinessPurchaseInvoice(BlackSoilCreateBusinessPurchaseInvoiceInput item, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            var reqmsg = JsonConvert.SerializeObject(item);
            result.Request = reqmsg;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                    {

                        string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent("invoices"), "type");
                        content.Add(new StringContent(item.MonthFirstBuyingDate.Value.ToString("yyyy-MM-dd")), "invoice_date");
                        content.Add(new StringContent(item.TotalMonthInvoice.ToString()), "no_of_invoices");
                        content.Add(new StringContent(item.MonthTotalAmount.Value.ToString()), "total_amount");

                        request.Content = content;
                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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


            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            return result;
        }
        public async Task<BlackSoilCommonAPIRequestResponse> CreateBusinessDocument(BlackSoilCreateBusinessDocumentInput item, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            var reqmsg = JsonConvert.SerializeObject(item);
            result.Request = reqmsg;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                    {

                        string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                        var stream = FileSaverHelper.GetStreamDataFromUrl(item.files);
                        var content = new MultipartFormDataContent();
                        content.Add(new StreamContent(stream), "files", item.files);
                        content.Add(new StringContent(item.doc_type), "doc_type");
                        content.Add(new StringContent(item.doc_number), "doc_number");
                        content.Add(new StringContent(item.doc_name), "doc_name");
                        request.Content = content;
                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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


            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            return result;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> SendToLos(string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("PUT"), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;

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
            result.LeadNBFCApiId = LeadNBFCApiId;
            result.LeadId = LeadId;
            return result;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> CreateBank(BlackSoilCreateBankInput BlackSoilCreateBanckInput, string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl.Replace("{{BUSINESS_ID}}", BlackSoilCreateBanckInput.business_id);
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;

            if (BlackSoilCreateBanckInput != null)
            {

                var reqmsg = JsonConvert.SerializeObject(BlackSoilCreateBanckInput);
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                            var content = new MultipartFormDataContent();
                            content.Add(new StringContent(BlackSoilCreateBanckInput.ifsc), "ifsc");
                            content.Add(new StringContent(BlackSoilCreateBanckInput.account_number), "account_number");
                            content.Add(new StringContent(BlackSoilCreateBanckInput.account_holder_name), "account_holder_name");
                            content.Add(new StringContent(BlackSoilCreateBanckInput.account_type), "account_type");
                            //content.Add(new StringContent(BlackSoilCreateBanckInput.password), "");
                            content.Add(new StringContent(BlackSoilCreateBanckInput.is_for_disbursement.ToString()), "is_for_disbursement");
                            content.Add(new StringContent(BlackSoilCreateBanckInput.is_for_nach.ToString()), "is_for_nach");
                            content.Add(new StringContent(BlackSoilCreateBanckInput.bank_name), "bank_name");


                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            return result;
        }
        public async Task<BlackSoilCommonAPIRequestResponse> GetBankDetailByIFSCCode(string ApiUrl, string Key, string SecretKey, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.Request = ApiUrl;

            if (!string.IsNullOrEmpty(ApiUrl))
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), ApiUrl))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

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

        public async Task<BlackSoilCommonAPIRequestResponse> GetCreditLine(string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;

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
            result.LeadNBFCApiId = LeadNBFCApiId;
            result.LeadId = LeadId;
            return result;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> CallGenerateAgreementURL(string ApiUrl, string Key, string SecretKey, string HttpMethodCode, string BodyContent, long LeadNBFCApiId, long LeadId)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(HttpMethodCode), ApiUrl))
                    {
                        string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                        result.Request = ApiUrl;

                        if (!string.IsNullOrEmpty(BodyContent))
                        {
                            result.Request = BodyContent;

                            request.Content = new StringContent(BodyContent);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        }

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        result.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
            result.LeadNBFCApiId = LeadNBFCApiId;
            result.LeadId = LeadId;
            return result;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> GenerateAgreement(string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            var ReturnResponse = await CallGenerateAgreementURL(ApiUrl, Key, SecretKey, "GET", "", LeadNBFCApiId, LeadId);
            return ReturnResponse;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> Attachstamp(string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            var ReturnResponse = await CallGenerateAgreementURL(ApiUrl, Key, SecretKey, "GET", "", LeadNBFCApiId, LeadId);
            return ReturnResponse;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> CreateEsign(string ApiUrl, string Key, string SecretKey, BlackSoilEsignsDTO blackSoilCreateEsignDTO, long LeadNBFCApiId, long LeadId)
        {
            var reqmsg = JsonConvert.SerializeObject(blackSoilCreateEsignDTO);
            var ReturnResponse = await CallGenerateAgreementURL(ApiUrl, Key, SecretKey, "POST", reqmsg, LeadNBFCApiId, LeadId);
            return ReturnResponse;
        }

        public async Task<BlackSoilCommonAPIRequestResponse> SignerStatus(string ApiUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            var ReturnResponse = await CallGenerateAgreementURL(ApiUrl, Key, SecretKey, "GET", "", LeadNBFCApiId, LeadId);
            return ReturnResponse;
        }



        #region LeadUpdate
        public async Task<BlackSoilCommonAPIRequestResponse> PanUpdate(BlackSoilPanUpdateInput BlackSoilPanUpdateInput, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = BlackSoilPanUpdateInput.update_url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (BlackSoilPanUpdateInput != null)
            {
                var reqmsg = JsonConvert.SerializeObject(BlackSoilPanUpdateInput);
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), BlackSoilPanUpdateInput.update_url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var stream = FileSaverHelper.GetStreamDataFromUrl(BlackSoilPanUpdateInput.file);
                            var content = new MultipartFormDataContent();
                            content.Add(new StreamContent(stream), "file", BlackSoilPanUpdateInput.file);
                            content.Add(new StringContent(BlackSoilPanUpdateInput.doc_number), "doc_number");
                            content.Add(new StringContent(BlackSoilPanUpdateInput.business), "business");
                            content.Add(new StringContent(BlackSoilPanUpdateInput.doc_name), "doc_name");
                            content.Add(new StringContent(BlackSoilPanUpdateInput.doc_type), "doc_type");
                            content.Add(new StringContent(BlackSoilPanUpdateInput.type), "type");

                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
        public async Task<BlackSoilCommonAPIRequestResponse> AadhaarUpdate(BlackSoilAadhaarUpdateInput BlackSoilAadhaarUpdateInput, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = BlackSoilAadhaarUpdateInput.update_url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (BlackSoilAadhaarUpdateInput != null)
            {
                var reqmsg = JsonConvert.SerializeObject(BlackSoilAadhaarUpdateInput);
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), BlackSoilAadhaarUpdateInput.update_url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var stream = FileSaverHelper.GetStreamDataFromUrl(BlackSoilAadhaarUpdateInput.file);
                            var content = new MultipartFormDataContent();
                            content.Add(new StreamContent(stream), "file", BlackSoilAadhaarUpdateInput.file);
                            content.Add(new StringContent(BlackSoilAadhaarUpdateInput.doc_number), "doc_number");
                            content.Add(new StringContent(BlackSoilAadhaarUpdateInput.business), "business");
                            content.Add(new StringContent(BlackSoilAadhaarUpdateInput.doc_name), "doc_name");
                            content.Add(new StringContent(BlackSoilAadhaarUpdateInput.doc_type), "doc_type");
                            content.Add(new StringContent(BlackSoilAadhaarUpdateInput.type), "type");
                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
        public async Task<BlackSoilCommonAPIRequestResponse> BusinessUpdate(BlackSoilBusinessUpdateInput BlackSoilBusinessUpdateInput, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = BlackSoilBusinessUpdateInput.update_url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (BlackSoilBusinessUpdateInput != null)
            {
                var reqmsg = JsonConvert.SerializeObject(BlackSoilBusinessUpdateInput);
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), BlackSoilBusinessUpdateInput.update_url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                            var content = new MultipartFormDataContent();
                            content.Add(new StringContent(BlackSoilBusinessUpdateInput.name), "name");
                            content.Add(new StringContent(BlackSoilBusinessUpdateInput.business_type.ToLower()), "business_type");

                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
        public async Task<BlackSoilCommonAPIRequestResponse> PersonUpdate(BlackSoilPersonUpdateInput BlackSoilPersonUpdateInput, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = BlackSoilPersonUpdateInput.update_url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (BlackSoilPersonUpdateInput != null)
            {
                var reqmsg = JsonConvert.SerializeObject(BlackSoilPersonUpdateInput);
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), BlackSoilPersonUpdateInput.update_url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var content = new MultipartFormDataContent();
                            content.Add(new StringContent(BlackSoilPersonUpdateInput.dob), "dob");
                            content.Add(new StringContent(BlackSoilPersonUpdateInput.full_name), "full_name");
                            content.Add(new StringContent(BlackSoilPersonUpdateInput.first_name), "first_name");
                            content.Add(new StringContent(BlackSoilPersonUpdateInput.last_name), "last_name");
                            content.Add(new StringContent(BlackSoilPersonUpdateInput.middle_name), "middle_name");
                            content.Add(new StringContent(BlackSoilPersonUpdateInput.gender), "gender");
                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
        public async Task<BlackSoilCommonAPIRequestResponse> BusinessAddressUpdate(BlackSoilBusinessAddressUpdateInput BlackSoilBusinessAddressUpdateInput, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = BlackSoilBusinessAddressUpdateInput.update_url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (BlackSoilBusinessAddressUpdateInput != null)
            {
                var reqmsg = JsonConvert.SerializeObject(BlackSoilBusinessAddressUpdateInput);
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), BlackSoilBusinessAddressUpdateInput.update_url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);


                            var content = new MultipartFormDataContent();
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.state), "state");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.city), "city");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.country), "country");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.address_line), "address_line");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.full_address), "full_address");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.pincode), "pincode");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.landmark), "landmark");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.locality), "locality");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.address_type), "address_type");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.address_name), "address_name");
                            content.Add(new StringContent(BlackSoilBusinessAddressUpdateInput.business), "business");



                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
        public async Task<BlackSoilCommonAPIRequestResponse> PersonAddressUpdate(BlackSoilPersonAddressUpdateInput BlackSoilPersonAddressUpdateInput, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = BlackSoilPersonAddressUpdateInput.update_url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (BlackSoilPersonAddressUpdateInput != null)
            {
                var reqmsg = JsonConvert.SerializeObject(BlackSoilPersonAddressUpdateInput);
                result.Request = reqmsg;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), BlackSoilPersonAddressUpdateInput.update_url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var content = new MultipartFormDataContent();
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.state), "state");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.city), "city");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.country), "country");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.address_line), "address_line");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.full_address), "full_address");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.pincode), "pincode");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.landmark), "landmark");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.locality), "locality");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.address_type), "address_type");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.address_name), "address_name");
                            content.Add(new StringContent(BlackSoilPersonAddressUpdateInput.business), "business");


                            request.Content = content;
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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

        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilEmailPost(string ApiUrl, string EmailId, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;

            if (ApiUrl != null && EmailId != null)
            {
                string paramstr = "{\"detail\":\"{#EmailId#}\",\"name\":\"email\"}";
                paramstr = paramstr.Replace("{#EmailId#}", EmailId);
                result.Request = paramstr;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                            request.Content = new StringContent(paramstr);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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
        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilSelfiePost(string ApiUrl, string SelfieUrl, string Key, string SecretKey, long LeadNBFCApiId, long LeadId)
        {
            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = ApiUrl;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            result.LeadId = LeadId;
            result.LeadNBFCApiId = LeadNBFCApiId;
            if (ApiUrl != null && SelfieUrl != null)
            {
                result.Request = SelfieUrl;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var stream = FileSaverHelper.GetStreamDataFromUrl(SelfieUrl);
                            var content = new MultipartFormDataContent();
                            content.Add(new StreamContent(stream), "files", SelfieUrl);
                            content.Add(new StringContent("selfie"), "doc_name");
                            content.Add(new StringContent("photo"), "doc_type");
                            request.Content = content;
                            var response = await httpClient.SendAsync(request);
                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
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

        #endregion

        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilCommonApplicationDetail(string url, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (url != null)
            {
                result.Request = url;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result.Response = jsonString;
                                var item = JsonConvert.DeserializeObject<ApplicationDetailDTO>(jsonString);
                                if (item != null && item.status == "line_activated")
                                {
                                    result.IsSuccess = true;
                                }
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

        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilCommonApplicationDetailPWA(string url, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();
            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (url != null)
            {
                result.Request = url;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result.Response = jsonString;
                                var item = JsonConvert.DeserializeObject<ApplicationDetailDTO>(jsonString);
                                if (item != null && item.processing_fee_status.Trim().ToLower() == "paid")
                                {
                                    result.IsSuccess = true;
                                }
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
        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilCommonBusinessDetail(string url, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();

            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (url != null)
            {
                result.Request = url;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result.Response = jsonString;
                                result.IsSuccess = true;

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

        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilGetEmbedSession(string url, string Key, string SecretKey, string mobile)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();

            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (url != null)
            {
                result.Request = url;
                string paramstr = "{\"mobile\":\"{#MobileNo#}\"}";
                paramstr = paramstr.Replace("{#MobileNo#}", mobile);
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);
                            request.Content = new StringContent(paramstr);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result.Response = jsonString;
                                result.IsSuccess = true;

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

        public async Task<BlackSoilCommonAPIRequestResponse> BlackSoilPFCollection(BlackSoilPFCollectionPostDc blackSoilPFCollectionPostDc, string url, string Key, string SecretKey)
        {

            DateConvertHelper dateConvertHelper = new DateConvertHelper();

            var result = new BlackSoilCommonAPIRequestResponse();
            result.URL = url;
            result.Created = dateConvertHelper.GetIndianStandardTime();
            result.IsActive = true;
            result.IsDeleted = false;
            if (blackSoilPFCollectionPostDc != null)
            {
                var reqmsg = JsonConvert.SerializeObject(blackSoilPFCollectionPostDc);
                result.Request = reqmsg;
                result.Request = url;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            string authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Key + ":" + SecretKey));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationString);

                            var content = new MultipartFormDataContent();
                            content.Add(new StringContent(blackSoilPFCollectionPostDc.session_id), "session_id");
                            content.Add(new StringContent(blackSoilPFCollectionPostDc.business_id), "business_id");
                            content.Add(new StringContent(blackSoilPFCollectionPostDc.person_id), "person_id");
                            request.Content = content;
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            var response = await httpClient.SendAsync(request);

                            string jsonString = string.Empty;
                            result.StatusCode = Convert.ToInt32(response.StatusCode);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                result.Response = jsonString;
                                result.IsSuccess = true;

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
