using Google.Protobuf.WellKnownTypes;
using MassTransit.SagaStateMachine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Constants;
using ScaleUP.Services.LeadAPI.Controllers;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using ScaleUP.Services.LeadModels;
using System.Data;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace ScaleUP.Services.LeadAPI.Helper
{
    public class eNachHelper
    {

        private readonly LeadApplicationDbContext _context;


        public eNachHelper(LeadApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<eNachBankListResponseDTO> getBankListAsync(eNachBankListConfigDc eNachBankListApiConfigdata, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);

            eNachBankListResponseDTO responseDC = new eNachBankListResponseDTO();

            List<LiveBankList> bankList = await GetBankListAsync();

            if (bankList != null && bankList.Count > 0)
            {
                responseDC.liveBankList = bankList;
                return responseDC;
            }


            if (eNachBankListApiConfigdata.ApiMasterId != null)
            {
                AddRequestResponseDc addresponse = null;

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), eNachBankListApiConfigdata.ApiUrl))
                        {
                            request.Headers.TryAddWithoutValidation("cache-control", "no-cache");

                            AddRequestResponseDc addRequest = new AddRequestResponseDc()
                            {
                                ApiMasterId = eNachBankListApiConfigdata.ApiMasterId,
                                Header = request.Headers.ToString(),
                                RequestResponseMsg = " ",
                                Type = "Request",
                                Url = eNachBankListApiConfigdata.ApiUrl,
                                PersonId = " "
                            };
                            //await InsertReqestReponseAsync(addRequest);
                            try
                            {
                                responseFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
                                responseFullpath = Path.Combine(basePath, "wwwroot");
                                responseFilecontent = JsonConvert.SerializeObject(addresponse);
                                requestFullpath = fileSaverHelper.SaveFile(responseFilename, responseFullpath, responseFilecontent);
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
                                responseDC = JsonConvert.DeserializeObject<eNachBankListResponseDTO>(jsonString);
                                if (responseDC != null) //&& responseDC.error == null && responseDC.liveBankList != null
                                {
                                    List<BankList> bankLists = responseDC.liveBankList.Select(X => new BankList
                                    {
                                        aadhaarActiveFrom = X.aadhaarActiveFrom,
                                        aadhaarFlag = X.aadhaarFlag,
                                        activeFrm = X.activeFrm,
                                        bankId = X.bankId,
                                        bankName = X.bankName,
                                        CreatedBy = "",
                                        Created = DateTime.Now,
                                        dcActiveFrom = X.dcActiveFrom,
                                        debitcardFlag = X.debitcardFlag,
                                        IsActive = true,
                                        IsDeleted = false,
                                        netbankFlag = X.netbankFlag

                                    }).ToList();
                                    _context.BankLists.AddRange(bankLists);
                                    _context.SaveChanges();

                                }
                            }
                            else
                            {
                                jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                                // responseDC.error = "No Data";
                            }
                            addresponse = new AddRequestResponseDc()
                            {
                                ApiMasterId = eNachBankListApiConfigdata.ApiMasterId,
                                Header = response.Headers.ToString(),
                                RequestResponseMsg = jsonString,
                                Type = "Response",
                                Url = eNachBankListApiConfigdata.ApiUrl,
                                PersonId = " "
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

                    addresponse = new AddRequestResponseDc()
                    {
                        ApiMasterId = eNachBankListApiConfigdata.ApiMasterId,
                        Header = string.Empty,
                        RequestResponseMsg = ex.Message.ToString(),
                        Type = "Response",
                        Url = eNachBankListApiConfigdata.ApiUrl,
                        PersonId = " "
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
                //await InsertReqestReponseAsync(addresponse);

            }
            return responseDC;
        }


        public async Task<bool> AddAsync(eNachBankDetailDc obj)
        {
            return await InsertEnachBankDetail(obj);
        }

        public async Task<bool> InsertEnachBankDetail(eNachBankDetailDc obj)
        {
            var LeadMasterId = new SqlParameter("@LeadMasterId", obj.LeadMasterId);
            var BankName = new SqlParameter("@BankName", obj.BankName);
            var AccountNo = new SqlParameter("@AccountNo", obj.AccountNo);
            var IfscCode = new SqlParameter("@IfscCode", obj.IfscCode);
            var AccountType = new SqlParameter("@AccountType", obj.AccountType);
            var Channel = new SqlParameter("@Channel", obj.Channel);
            var CreatedBy = new SqlParameter("@CreatedBy", obj.CreatedBy);
            var ModifiedBy = new SqlParameter("@ModifiedBy", obj.CreatedBy);
            var MsgId = new SqlParameter("@MsgId", obj.MsgId);

            var result = _context.Database.SqlQueryRaw<int>("exec eNachInsertBankDetail @LeadMasterId, @AccountNo, @BankName, @IfscCode, @AccountType, @Channel, @CreatedBy,@ModifiedBy, @MsgId", LeadMasterId, BankName, AccountNo, IfscCode, AccountType, Channel, CreatedBy, ModifiedBy, MsgId).AsEnumerable().FirstOrDefault();
            return result > 0;
        }

        //public async Task<eNachSendReqDTO> SendeMandateRequestToHDFCAsync(long LeadId, string username, eNachBankListConfigDc apiConfigdata, string eNachKey, eNachConfigDc eNachConfigRes)
        //{
        //    eNachSendReqDTO eMandateSend = new eNachSendReqDTO();
        //    ////var apiConfigdata = await apiConfig.GetApiConfig("eMandatePost");


        //    string key = eNachKey;

        //    ////AES256 aes = new AES256();
        //    //AccountManager manager = new AccountManager();
        //    //CustomerAccountInfoDC customer = await manager.GetCustomerAccountinfo(LeadId);
        //    //if (customer != null)
        //    //{
        //    //    var MaxAmount = Math.Round((customer.CreditLimit * customer.CreditLimitMultiplier), 0);
        //    //    //if (MaxAmount == 0) { MaxAmount = 10000; }
        //    string _Customer_ExpiryDate = "";
        //    string _Customer_DebitAmount = "";
        //    //string _Customer_MaxAmount = Convert.ToString(MaxAmount) + ".00";// "100000.00";
        //    eMandateSend.url = apiConfigdata.ApiUrl;
        //    string MsgId = GetUnquieId(LeadId.ToString()); //customer.LeadNo

        //    var reqdata = new request()
        //    {
        //        //        CheckSum = eNachHdfcAesHelper.ComputeSha256Hash(customer.AccountNo + "|" + customer.CreatedDate + "|" + _Customer_ExpiryDate + "|" + _Customer_DebitAmount + "|" + _Customer_MaxAmount),

        //        //        Customer_Name = eNachHdfcAesHelper.Encrypt(customer.Name),
        //        //        Customer_Mobile = eNachHdfcAesHelper.Encrypt(customer.MobileNo),
        //        //        Customer_EmailId = eNachHdfcAesHelper.Encrypt(customer.EmailId),
        //        //        Customer_AccountNo = eNachHdfcAesHelper.Encrypt(customer.AccountNo),
        //        //        Customer_StartDate = customer.CreatedDate,
        //        //        Customer_ExpiryDate = _Customer_ExpiryDate,
        //        //        Customer_MaxAmount = _Customer_MaxAmount,
        //        //        Customer_DebitFrequency = customer.CustomerDebitFrequency, //"ADHO", //(Customer_SequenceType ) RCUR and for OOFF this parameter should be sent blank.
        //        //        Customer_InstructedMemberId = customer.IfscCode,
        //        //        Short_Code = eNachHdfcAesHelper.Encrypt(customer.ShortCode),
        //        //        Customer_SequenceType = customer.CustomerSequenceType, //"RCUR", // Merchant will pass "RCUR" or "OOFF".Anything other than this, request will be rejected.When the value is “OOFF”, //the “Customer_DebitFrequency” should have empty value.
        //        //        Merchant_Category_Code = customer.MerchantCategoryCode, //"U099",
        //        //        Channel = customer.Channel, //"Debit" for Debit Card , "Net" for Net - banking.
        //        //        UtilCode = eNachHdfcAesHelper.Encrypt(customer.UtilCode),
        //        //        Filler5 = customer.AccountType, // Account Type: Value should be “S” for Savings, “C” for Current or “O” “Other”. It cannot be blank
        //    };
        //    eMandateSend.status = true;
        //    eMandateSend.request = reqdata;
        //    eMandateSend.request.MsgId = MsgId;

        //    //    eMandateManager mandateManager = new eMandateManager();
        //    //    bool i = await mandateManager.AddeMandateRequestDetails(reqdata, username, LeadId);
        //    //    if (i)
        //    //    {
        //    //        LeadMasterManager leadMasterManager = new LeadMasterManager();
        //    //        bool rs = await leadMasterManager.UpdateMsgIdinLeadMaster(LeadId, reqdata.MsgId);
        //    //    }
        //    //}

        //    return eMandateSend;
        //}


        public async Task<eNachSendReqDTO> enachSendeEnachRequestToHDFCAsync(SendeMandateRequestDc SendeMandateRequest, eNachBankListConfigDc apiConfigdata, string eNachKey, eNachConfigDc eNachConfigRes, string basePath)
        {
            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);
            ;

            eNachSendReqDTO eMandateSend = new eNachSendReqDTO();
            eMandateSend.request = new request();
            string key = eNachKey;

            string _Customer_ExpiryDate = "";
            string _Customer_DebitAmount = "";
            var MaxAmount = Math.Round(Convert.ToDecimal(SendeMandateRequest.CreditLimit * Convert.ToInt64(eNachConfigRes.CreditLimitMultiplier)), 0);
            if (MaxAmount > 999999)
            { MaxAmount = 999999; }
            string _Customer_MaxAmount = Convert.ToString(MaxAmount) + ".00";// "100000.00";
            eMandateSend.url = apiConfigdata.ApiUrl;
            string MsgId = GetUnquieId(SendeMandateRequest.LeadNo); //customer.LeadNo

            string StartDat = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            eMandateSend.request.Customer_StartDate = StartDat;

            DateTime currentdate = Convert.ToDateTime(eMandateSend.request.Customer_StartDate);
            _Customer_ExpiryDate = currentdate.AddYears(25).ToString("yyyy-MM-dd");
            eMandateSend.request.Customer_ExpiryDate = _Customer_ExpiryDate;
            eMandateSend.request.Customer_MaxAmount = _Customer_MaxAmount;

            string checksumbefore = SendeMandateRequest.Customer_AccountNo + "|" + eMandateSend.request.Customer_StartDate + "|" + _Customer_ExpiryDate + "|" + _Customer_DebitAmount + "|" + _Customer_MaxAmount;
            //eMandateSend.request.CheckSum = eNachHdfcAesHelper.ComputeSha256Hash(SendeMandateRequest.Customer_AccountNo + "|" + eMandateSend.request.Customer_StartDate + "|" + _Customer_ExpiryDate + "|" + _Customer_DebitAmount + "|" + _Customer_MaxAmount);
            //eMandateSend.request.MsgId = MsgId;
            //eMandateSend.request.Customer_Name = eNachHdfcAesHelper.Encrypt(SendeMandateRequest.Customer_Name, eNachKey);
            //eMandateSend.request.Customer_Mobile = eNachHdfcAesHelper.Encrypt(SendeMandateRequest.Customer_Mobile, eNachKey);
            //eMandateSend.request.Customer_EmailId = eNachHdfcAesHelper.Encrypt(SendeMandateRequest.Customer_EmailId, eNachKey);
            //eMandateSend.request.Customer_AccountNo = eNachHdfcAesHelper.Encrypt(SendeMandateRequest.Customer_AccountNo, eNachKey);
            //eMandateSend.request.Short_Code = eNachHdfcAesHelper.Encrypt(eNachConfigRes.ShortCode, eNachKey);
            //eMandateSend.request.Customer_DebitFrequency = eNachConfigRes.CustomerDebitFrequency;
            //eMandateSend.request.Customer_SequenceType = eNachConfigRes.CustomerSequenceType; //"RCUR", // Merchant will pass "RCUR" or "OOFF".Anything other than this, request will be rejected.When the value is “OOFF”, //the “Customer_DebitFrequency” should have empty value.
            //eMandateSend.request.Merchant_Category_Code = eNachConfigRes.MerchantCategoryCode; //"U099",
            //eMandateSend.request.UtilCode = eNachHdfcAesHelper.Encrypt(eNachConfigRes.UtilCode, eNachKey);
            //eMandateSend.request.Customer_InstructedMemberId = SendeMandateRequest.Customer_InstructedMemberId;
            //eMandateSend.request.Channel = SendeMandateRequest.Channel;
            //eMandateSend.request.Filler5 = SendeMandateRequest.Filler5;

            //var reqdata = new request()
            //{
            //    CheckSum = eMandateSend.request.CheckSum,
            //    MsgId = eMandateSend.request.MsgId,
            //    Customer_Name = eMandateSend.request.Customer_Name,
            //    Customer_Mobile = eMandateSend.request.Customer_Mobile,
            //    Customer_EmailId = eMandateSend.request.Customer_EmailId,
            //    Customer_AccountNo = eMandateSend.request.Customer_AccountNo,
            //    Customer_StartDate = eMandateSend.request.Customer_StartDate,
            //    Customer_ExpiryDate = _Customer_ExpiryDate,
            //    Customer_MaxAmount = eMandateSend.request.Customer_MaxAmount,
            //    Customer_DebitFrequency = eMandateSend.request.Customer_DebitFrequency,
            //    Customer_InstructedMemberId = eMandateSend.request.Customer_InstructedMemberId,
            //    Short_Code = eMandateSend.request.Short_Code,
            //    Customer_SequenceType = eMandateSend.request.Customer_SequenceType, //"RCUR", // Merchant will pass "RCUR" or "OOFF".Anything other than this, request will be rejected.When the value is “OOFF”, //the “Customer_DebitFrequency” should have empty value.
            //    Merchant_Category_Code = eMandateSend.request.Merchant_Category_Code, //"U099",
            //    Channel = eMandateSend.request.Channel, //"Debit" for Debit Card , "Net" for Net - banking.
            //    UtilCode = eMandateSend.request.UtilCode,
            //    Filler5 = eMandateSend.request.Filler5, // Account Type: Value should be “S” for Savings, “C” for Current or “O” “Other”. It cannot be blank
            //};

            var reqdata = new request()
            {
                CheckSum = eNachHdfcAesHelper.ComputeSha256Hash(SendeMandateRequest.Customer_AccountNo + "|" + eMandateSend.request.Customer_StartDate + "|" + _Customer_ExpiryDate + "|" + _Customer_DebitAmount + "|" + _Customer_MaxAmount),
                MsgId = MsgId,
                Customer_Name = eNachHdfcAesHelper.Encrypt(SendeMandateRequest.Customer_Name, eNachKey),
                Customer_Mobile = eNachHdfcAesHelper.Encrypt(SendeMandateRequest.Customer_Mobile, eNachKey),
                Customer_EmailId = eNachHdfcAesHelper.Encrypt(SendeMandateRequest.Customer_EmailId, eNachKey),
                Customer_AccountNo = eNachHdfcAesHelper.Encrypt(SendeMandateRequest.Customer_AccountNo, eNachKey),
                Customer_StartDate = eMandateSend.request.Customer_StartDate,
                Customer_ExpiryDate = _Customer_ExpiryDate,
                Customer_MaxAmount = eMandateSend.request.Customer_MaxAmount,
                Customer_DebitFrequency = eNachConfigRes.CustomerDebitFrequency,
                Customer_InstructedMemberId = SendeMandateRequest.Customer_InstructedMemberId,
                Short_Code = eNachHdfcAesHelper.Encrypt(eNachConfigRes.ShortCode, eNachKey),
                Customer_SequenceType = eNachConfigRes.CustomerSequenceType, //"RCUR", // Merchant will pass "RCUR" or "OOFF".Anything other than this, request will be rejected.When the value is “OOFF”, //the “Customer_DebitFrequency” should have empty value.
                Merchant_Category_Code = eNachConfigRes.MerchantCategoryCode, //"U099",
                Channel = SendeMandateRequest.Channel, //"Debit" for Debit Card , "Net" for Net - banking.
                UtilCode = eNachHdfcAesHelper.Encrypt(eNachConfigRes.UtilCode, eNachKey),
                Filler5 = SendeMandateRequest.Filler5, // Account Type: Value should be “S” for Savings, “C” for Current or “O” “Other”. It cannot be blank
            };

            eMandateSend.status = true;
            eMandateSend.request = reqdata;


            var reqmsg = JsonConvert.SerializeObject(reqdata);
            eNachThirdPartyRequestDTO addRequest = new eNachThirdPartyRequestDTO()
            {
                //ApiMasterId = apiConfigdata.ApiMasterId,
                Header = "",
                RequestResponseMsg = reqmsg,
                Type = "Request",
                Url = apiConfigdata.ApiUrl,
                LeadMasterId = Convert.ToString(SendeMandateRequest.LeadId),
                CheckSumBeforeData = checksumbefore
            };

            requestFilename = "Request_" + Guid.NewGuid().ToString() + ".json";
            requestFullpath = Path.Combine(basePath, "wwwroot");
            requestFilecontent = JsonConvert.SerializeObject(addRequest);
            requestFullpath= fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);


            ThirdPartyRequestDTO thirdPartyRequestDTO = new ThirdPartyRequestDTO
            {
                SubActivityId = SendeMandateRequest.SubActivityId,
                ActivityId = Convert.ToInt64(SendeMandateRequest.ActivityId),
                CompanyId = Convert.ToInt64(SendeMandateRequest.CompanyId),
                LeadId = SendeMandateRequest.LeadId,
                Code = "",
                ProcessedResponse = "wwwroot\\" + responseFilename,
                Request = requestFullpath,
                Response = "",
                IsError = isError
            };
            ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
            var res = thirdPartyRequestManager.SaveThirdPartyRequest(thirdPartyRequestDTO);

            //if (res > 0)
            {

                //if (MsgId != "")
                //{
                //    var LeadIds = new SqlParameter("LeadMasterId", SendeMandateRequest.LeadId);
                //    var MsgIds = new SqlParameter("MsgId", MsgId);

                //    var result = await _context.Database.SqlQueryRaw<int>("exec dbo.eNachUpdateMsgIdinLeadMaster @LeadMasterId, @MsgId", LeadIds, MsgIds).ToListAsync();
                //}
            }

            return eMandateSend;
        }

        private string GetUnquieId(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = eNachHashHelper.GetMd5Hash(md5Hash, source);
                return eNachHashHelper.HashString(hash);
            }
        }

        private async Task InsertReqestReponseAsync(AddRequestResponseDc addRequestResponseDc)
        {
            //try
            //{
            //    RequestResponseManager manager = new RequestResponseManager();
            //    bool i = await manager.InsertRequestResponse(addRequestResponseDc);
            //}
            //catch (Exception ex)
            //{

            //}
        }

        private async Task InsertBankListAsync(eNachBankListResponseDTO BankListResponse)
        {
            try
            {
                //BankListManager manager = new BankListManager();
                //bool i = await manager.InsertBankListResponse(BankListResponse);

            }
            catch (Exception ex)
            {

            }
        }
        public async Task<List<LiveBankList>> GetBankListAsync()
        {
            List<LiveBankList> res = new List<LiveBankList>();
            var sres = _context.BankLists.Where(x => x.IsActive && !x.IsDeleted).Select(x => new LiveBankList { bankId = x.bankId, activeFrm = x.activeFrm, debitcardFlag = x.debitcardFlag, bankName = x.bankName, dcActiveFrom = x.dcActiveFrom, netbankFlag = x.netbankFlag }).OrderBy(x=>x.bankName).ToList();

            return sres;
        }


        [AllowAnonymous]
        public async Task<bool> eNachAddResponseDetails(GRPCRequest<eNachResponseDocDC> _response, string basePath)
        {

            bool result = false;

            string responseFilename = string.Empty;
            string responseFullpath = string.Empty;
            string responseFilecontent = string.Empty;
            string requestFilename = string.Empty;
            string requestFullpath = string.Empty;
            string requestFilecontent = string.Empty;
            bool isError = true;
            FileSaverHelper fileSaverHelper = new FileSaverHelper(EnvironmentConstants.EnvironmentName, EnvironmentConstants.Azureconnectionstring, EnvironmentConstants.AzurecontainerName);
            ;

            if (_response != null)
            {
                //var eMandateBank = await GeteMandateBankDetailsbyMsgId(_response.Request.MsgId);
                var eMandateBank = await _context.eNachBankDetails.Where(x => x.MsgId == _response.Request.MsgId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                if (eMandateBank != null)
                {
                    //-------------S-----------------------
                    long ActivityMasterId = 0;
                    long SubActivityMasterId = 0;
                    long CompanyId = 0;

                    var leads = await _context.Leads.Where(x => x.Id == eMandateBank.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    if (leads != null)
                    {
                        var leadActivities = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leads.Id && x.IsActive && !x.IsDeleted && !x.IsCompleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
                        if (leadActivities != null)
                        {
                            ActivityMasterId = leadActivities.ActivityMasterId;
                            SubActivityMasterId = Convert.ToInt64(leadActivities.SubActivityMasterId);
                            CompanyId = Convert.ToInt64(leads.OfferCompanyId);
                        }

                        //-------------E-----------------------

                        var reqmsg = JsonConvert.SerializeObject(_response);
                        eNachThirdPartyRequestDTO addRequest = new eNachThirdPartyRequestDTO()
                        {
                            //ApiMasterId = apiConfigdata.ApiMasterId,
                            Header = "",
                            RequestResponseMsg = reqmsg,
                            Type = "Response",
                            Url = "",
                            LeadMasterId = Convert.ToString(eMandateBank.LeadId),
                            UMRN_Filler10 = _response.Request.Filler10
                        };

                        requestFilename = "Response_" + Guid.NewGuid().ToString() + ".json";
                        requestFullpath = Path.Combine(basePath, "wwwroot");
                        requestFilecontent = JsonConvert.SerializeObject(addRequest);
                        requestFullpath= fileSaverHelper.SaveFile(requestFilename, requestFullpath, requestFilecontent);


                        ThirdPartyRequestDTO thirdPartyRequestDTO = new ThirdPartyRequestDTO
                        {
                            SubActivityId = SubActivityMasterId,
                            ActivityId = ActivityMasterId,
                            CompanyId = CompanyId,
                            LeadId = eMandateBank.LeadId,
                            Code = "",
                            ProcessedResponse = requestFullpath,
                            Request = "",
                            Response = requestFullpath,
                            IsError = isError
                        };
                        ThirdPartyRequestManager thirdPartyRequestManager = new ThirdPartyRequestManager(_context);
                        bool res = await thirdPartyRequestManager.SaveThirdPartyRequest(thirdPartyRequestDTO);

                        if (res == true)
                        {
                            if (eMandateBank != null)
                            {
                                if (_response.Request.Status == "Success")
                                {
                                    if (eMandateBank.UMRN != _response.Request.Filler10)
                                    {
                                        eMandateBank.UMRN = _response.Request.Filler10;

                                        leadActivities.IsCompleted = true;
                                        leadActivities.IsApproved = 1;
                                        _context.Entry(leadActivities).State = EntityState.Modified;
                                    }
                                }
                                eMandateBank.responseJSON = reqmsg;
                                _context.Entry(eMandateBank).State = EntityState.Modified;
                                _context.SaveChanges();
                            }

                            result = true;
                        }
                    }
                }
            }

            return result;
        }





    }
}