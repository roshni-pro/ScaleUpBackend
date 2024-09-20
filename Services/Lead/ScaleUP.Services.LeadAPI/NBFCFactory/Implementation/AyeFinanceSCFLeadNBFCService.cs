using Google.Protobuf.WellKnownTypes;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Response;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Services.LeadModels.AyeFinance;
namespace ScaleUP.Services.LeadAPI.NBFCFactory.Implementation
{
    public class AyeFinanceSCFLeadNBFCService : ILeadNBFCService
    {
        private readonly LeadApplicationDbContext _context;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly AyeFinanceSCFNBFCHelper _AyeFinanceSCFNBFCHelper;

        private readonly ILogger<AyeFinanceSCFLeadNBFCService> _logger;
        public AyeFinanceSCFLeadNBFCService(LeadApplicationDbContext context, LeadNBFCSubActivityManager leadNBFCSubActivityManager
        , ILogger<AyeFinanceSCFLeadNBFCService> logger, AyeFinanceSCFNBFCHelper AyeFinanceSCFNBFCHelper)
        {
            _context = context;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _AyeFinanceSCFNBFCHelper = AyeFinanceSCFNBFCHelper;
            _logger = logger;
        }

        #region AyeFinance
        string refId = Guid.NewGuid().ToString();
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwYXJ0bmVySWQiOiJQMTAwMDUxNSIsImlhdCI6MTcyNTg3NDgzMiwiZXhwIjoxNzI1OTYxMjMyfQ.sHcAXvjsMV6RdqeCkuHLyU0BZoAMUNybpr9ilwjC5ys";

        public async Task<GRPCReply<string>> GenerateToken()
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            string status = "";


            var nbfcApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.AyeFinGenerateToken && x.IsActive && !x.IsDeleted);
            if (nbfcApi != null)
            {
                var response = await _AyeFinanceSCFNBFCHelper.GenerateToken(0, nbfcApi.Id, nbfcApi.APIUrl, nbfcApi.TAPIKey, nbfcApi.TAPISecretKey, refId);

                _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();

                if (response.IsSuccess)
                {
                    var res = JsonConvert.DeserializeObject<GenerateTokenResponseDC>(response.Response);
                    if (res != null && !string.IsNullOrEmpty(res.token))
                    {
                        gRPCReply.Response = res.token;
                        gRPCReply.Status = true;
                        gRPCReply.Message = "Token Generated";
                        status = LeadNBFCApiConstants.Completed;

                    }
                }
                else
                {
                    gRPCReply.Message = response.Response ?? "Failed to Generate Token ";
                    status = LeadNBFCApiConstants.Error;

                }

            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> ayeleadReq)
        {

            GRPCReply<string> gRPCReply = new GRPCReply<string>();

            var nbfcApi = _context.LeadNBFCApis.FirstOrDefault(x => x.LeadNBFCSubActivitys.LeadId == ayeleadReq.Request.LeadId && x.Code == CompanyApiConstants.AyeFinCreateLead && x.IsActive && !x.IsDeleted);
            if (nbfcApi != null && !string.IsNullOrEmpty(ayeleadReq.Request.token))
            {
                var leadData = await (from l in _context.Leads
                                      join p in _context.PersonalDetails on l.Id equals p.LeadId
                                      join b in _context.BusinessDetails on l.Id equals b.LeadId

                                      where l.IsActive && !l.IsDeleted && p.IsActive && !p.IsDeleted && b.IsActive && !b.IsDeleted && l.Id == ayeleadReq.Request.LeadId
                                      select new AddLeadRequestDc
                                      {
                                          firstName = p.FirstName,
                                          lastName = p.LastName,
                                          fatherName = p.FatherName,
                                          address = p.PermanentAddressLineOne,
                                          addressLine1 = p.PermanentAddressLineTwo,
                                          landmark = p.PermanentAddressLineTwo,
                                          email = p.EmailId,
                                          pincode = Convert.ToString(p.PermanentZipCode),
                                          mobileNumber = p.MobileNo,
                                          gender = p.Gender,
                                          city = p.PermanentCityName,
                                          state = p.PermanentStateName,
                                          leadId = l.LeadCode,
                                          shopName = b.BusinessName,
                                          gstNo = b.BusGSTNO,
                                          businessType = b.BusEntityType,
                                          storeArea = "",
                                          storeLength = "",
                                          storeSize = "",
                                          storeWidth = "",
                                          refId = refId,
                                          salutation = "",
                                          signBoardLength = "",
                                          signBoardType = "",
                                          signBoardWidth = "",
                                          customerAttendingCapacity = "",
                                          userName = ""
                                      }).FirstOrDefaultAsync();

                if (leadData != null)
                {
                    _leadNBFCSubActivityManager.UpdateStatus(nbfcApi.Id, LeadNBFCApiConstants.Inprogress);
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(nbfcApi.LeadNBFCSubActivityId.Value, LeadNBFCSubActivityConstants.Inprogress);
                    var response = await _AyeFinanceSCFNBFCHelper.AddLead(ayeleadReq.Request.LeadId, nbfcApi.Id, nbfcApi.APIUrl, ayeleadReq.Request.token, leadData);

                    _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                    _context.SaveChanges();
                    string status = "";
                    if (response.IsSuccess)
                    {
                        var res = JsonConvert.DeserializeObject<GenerateTokenResponse>(response.Response);
                        if (res != null && !string.IsNullOrEmpty(res.status))
                        {

                            status = LeadNBFCApiConstants.Completed;
                            gRPCReply.Response = res.status;
                            gRPCReply.Status = true;
                            gRPCReply.Message = res.message;
                        }
                        else
                        {
                            status = LeadNBFCApiConstants.Error;
                            gRPCReply.Message = res != null && res.message != null ? res.message : "Failed to Generate Lead";
                        }
                    }
                    else
                    {
                        status = LeadNBFCApiConstants.Error;
                        gRPCReply.Message = response.Response ?? "Failed to Generate Lead from AyeFin";
                    }
                    _leadNBFCSubActivityManager.UpdateStatus(nbfcApi.Id, status, response.Id);
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(nbfcApi.LeadNBFCSubActivityId.Value, status);
                }

            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> request)
        {

            GRPCReply<string> gRPCReply = new GRPCReply<string>();

            var nbfcApi = _context.LeadNBFCApis.FirstOrDefault(x => x.LeadNBFCSubActivitys.LeadId == request.Request.LeadId && x.Code == CompanyApiConstants.AyeFinCreateLead && x.IsActive && !x.IsDeleted);
            if (nbfcApi != null)
            {


                var leadcode = await _context.Leads.Where(x => x.Id == request.Request.LeadId && x.IsActive && !x.IsDeleted).Select(x => x.LeadCode).FirstOrDefaultAsync();


                GetWebUrlReqDc getWebUrlReqDc = new GetWebUrlReqDc
                {
                    leadId = leadcode,
                    refId = refId,
                    callbackUrl = "https://partnerapp.com/callback"

                };
                var response = await _AyeFinanceSCFNBFCHelper.GetWebUrl(request.Request.LeadId, nbfcApi.Id, nbfcApi.APIUrl, request.Request.token, getWebUrlReqDc);
                _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();

                var res = JsonConvert.DeserializeObject<GetWebUrlResponseDC>(response.Response);


                if (res != null)
                {
                    var existingRecord = await _context.AyeFinanceUpdates.FirstOrDefaultAsync(x => x.leadId == request.Request.LeadId);

                    if (existingRecord != null)
                    {
                        // Update the existing record
                        existingRecord.leadCode = leadcode;
                        existingRecord.switchpeReferenceId = res.switchpeReferenceId;
                        existingRecord.status = "";
                        existingRecord.totalLimit = null;
                        existingRecord.availablelLimit = null;
                        //_context
                    }
                    else
                    {
                        // Insert a new record if none exists
                        _context.AyeFinanceUpdates.Add(new AyeFinanceUpdate
                        {
                            leadId = request.Request.LeadId,
                            refId = getWebUrlReqDc.refId,
                            leadCode = leadcode,
                            switchpeReferenceId = res.switchpeReferenceId,
                            totalLimit = null,
                            availablelLimit = null,
                            status = ""
                        });
                    }

                    _context.SaveChanges();
                }
                if (response.IsSuccess && res != null && !string.IsNullOrEmpty(res.status))
                {
                    gRPCReply.Response = res.url;
                    gRPCReply.Status = true;
                    gRPCReply.Message = res.message;
                }
                else
                {
                    gRPCReply.Message = res != null && res.message != null ? res.message : "Failed ";
                }

            }
            return gRPCReply;
        }

        public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> request)
        {
            GRPCReply<CheckCreditLineData> gRPCReply = new GRPCReply<CheckCreditLineData>();
            string status = "";

            var nbfcApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.LeadNBFCSubActivitys.LeadId == request.Request.LeadId && x.Code == CompanyApiConstants.AyeFinCheckCreditLine && x.IsActive && !x.IsDeleted);

            if (nbfcApi != null)
            {
                CheckCreditLineReqDc checkCreditLineReqDc = new CheckCreditLineReqDc
                {
                    leadId = "8000000556", // leadcode,
                    refId = refId
                };

                _leadNBFCSubActivityManager.UpdateStatus(nbfcApi.Id, LeadNBFCApiConstants.Inprogress);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(nbfcApi.LeadNBFCSubActivityId.Value, LeadNBFCSubActivityConstants.Inprogress);

                var response = await _AyeFinanceSCFNBFCHelper.CheckCreditLine(request.Request.LeadId, nbfcApi.Id, nbfcApi.APIUrl, request.Request.token, checkCreditLineReqDc);
                _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();


                if (response.IsSuccess)
                {
                    var res = JsonConvert.DeserializeObject<CheckCreditLineResponseDC>(response.Response);
                    if (res != null && res.data != null && !string.IsNullOrEmpty(res.data.status))
                    {
                        var pendingStatusList = new List<string> { "AP", "IP", "ED", "NS" };

                        string? webUrl = null;

                        if (pendingStatusList.Contains(res.data.status))
                        {
                            status = LeadNBFCApiConstants.Inprogress;
                            var webResponse = await GetWebUrl(new GRPCRequest<AyeleadReq>
                            {
                                Request = new AyeleadReq
                                {
                                    LeadId = request.Request.LeadId,
                                    token = request.Request.token
                                }
                            });

                            if (webResponse.Status && !string.IsNullOrEmpty(webResponse.Response))
                            {
                                webUrl = webResponse.Response; // Store the web URL
                            }
                        }
                        else
                        {
                            status = LeadNBFCApiConstants.Completed;
                            var tokenApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.LeadNBFCSubActivitys.LeadId == request.Request.LeadId && x.Code == CompanyApiConstants.AyeFinGenerateToken && x.IsActive && !x.IsDeleted);
                            _leadNBFCSubActivityManager.UpdateStatus(tokenApi.Id, LeadNBFCApiConstants.Completed);
                            _leadNBFCSubActivityManager.UpdateSubActivityStatus(nbfcApi.LeadNBFCSubActivityId.Value, LeadNBFCSubActivityConstants.Completed);

                        }
                        gRPCReply.Response = new CheckCreditLineData
                        {
                            loanId = res.data.loanId,
                            totalLimit = res.data.totalLimit,
                            availableLimit = res.data.availableLimit,
                            creditLineExist = res.data.creditLineExist,
                            status = res.data.status,
                            statusMessage = res.data.statusMessage,
                            isweburlapproved = webUrl != null,
                            webUrl = webUrl

                        };

                        gRPCReply.Status = true;
                        gRPCReply.Message = res.message;
                    }
                }
                else
                {
                    gRPCReply.Message = response.Response ?? "Failed in CheckCreditLine";
                    status = LeadNBFCApiConstants.Error;
                }
                _leadNBFCSubActivityManager.UpdateStatus(nbfcApi.Id, status, response.Id);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(nbfcApi.LeadNBFCSubActivityId.Value, status);

            }

            return gRPCReply;
        }

        public async Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> request)


        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            string status = "";
            var nbfcApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.AyeFinTransactionSendOtp && x.IsActive && !x.IsDeleted);
            if (nbfcApi != null)
            {

                var leadcode = await _context.Leads.Where(x => x.Id == request.Request.LeadId && x.IsActive && !x.IsDeleted).Select(x =>
                    x.LeadCode
                ).FirstOrDefaultAsync();


                TransactionSendOtpReqDc transactionSendOtpReqDc = new TransactionSendOtpReqDc
                {
                    leadId = "8000000556",
                    refId = refId
                };

                var response = await _AyeFinanceSCFNBFCHelper.TransactionSendOtp(request.Request.LeadId, nbfcApi.Id, nbfcApi.APIUrl, request.Request.token, transactionSendOtpReqDc);
                _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                _context.SaveChanges();

                if (response.IsSuccess)
                {
                    var res = JsonConvert.DeserializeObject<TransactionVerifyOtpResponseDC>(response.Response);
                    if (res != null && !string.IsNullOrEmpty(res.status))
                    {
                        gRPCReply.Response = res.status;
                        gRPCReply.Status = true;
                        gRPCReply.Message = res.message;
                        status = LeadNBFCApiConstants.Completed;

                    }
                }
                else
                {
                    gRPCReply.Message = response.Response ?? "Failed in CheckCreditLine";
                    status = LeadNBFCApiConstants.Error;
                }
                _leadNBFCSubActivityManager.UpdateStatus(nbfcApi.Id, status, response.Id);

            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> request)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            string status = "";

            var nbfcApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.AyeFinTransactionVerifyOtp && x.IsActive && !x.IsDeleted);
            if (nbfcApi != null)
            {

                var leadRecord = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request.leadId && x.IsActive && !x.IsDeleted);
                if (leadRecord != null)
                {
                    TransactionVerifythirdpartyreq transactionVerifyOtpReqDc = new TransactionVerifythirdpartyreq
                    {
                        leadId = leadRecord.LeadCode,
                        refId = refId, //need to find refid
                        otp = request.Request.otp
                    };

                    var response = await _AyeFinanceSCFNBFCHelper.TransactionVerifyOtp(request.Request.leadId, nbfcApi.Id, nbfcApi.APIUrl, request.Request.token, transactionVerifyOtpReqDc);
                    _context.AyeFinanceSCFCommonAPIRequestResponses.Add(response);
                    _context.SaveChanges();

                    var res = JsonConvert.DeserializeObject<TransactionVerifyOtpResponseDC>(response.Response);
                    if (response.IsSuccess && res != null && !string.IsNullOrEmpty(res.status))
                    {

                        gRPCReply.Response = res.status;
                        gRPCReply.Status = true;
                        gRPCReply.Message = res.message;
                        status = LeadNBFCApiConstants.Completed;

                    }
                    else
                    {

                        gRPCReply.Message = res != null && res.message != null ? res.message : "Failed ";
                        status = LeadNBFCApiConstants.Error;

                    }
                    _leadNBFCSubActivityManager.UpdateStatus(nbfcApi.Id, status, response.Id);
                }
                else gRPCReply.Message = "lead not found";

            }
            return gRPCReply;
        }

        #endregion
        public Task<ICreateLeadNBFCResponse> AadhaarUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> AcceptOfferWithXMLAadharOTP(GRPCRequest<SecondAadharXMLDc> AadharObj)
        {
            throw new NotImplementedException();
        }

        public Task<string> AgreementEsign(GRPCRequest<EsignAgreementRequest> req)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BlackSoilCommonApplicationDetail(long leadid)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BusinessAddressUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BusinessUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> ChangeLoanStatus(long LeadMasterId, string Status)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> eSignDocumentsAsync(long leadid, string DocumentId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> eSignSessionAsync(string agreementPdfUrl, long leadid)
        {
            throw new NotImplementedException();
        }

        public async Task<ICreateLeadNBFCResponse> GenerateOffer(long leadid, long nbfcCompanyId)
        {
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse() { IsSuccess = false };
            var subactivity = _leadNBFCSubActivityManager.GetSubactivityData(leadid, nbfcCompanyId, ActivityConstants.GenerateOffer);
            if (subactivity != null && subactivity.Any())
            {
                bool isSuccess = true;
                string token = "";
                foreach (var item in subactivity)
                {
                    if (isSuccess)
                    {
                        switch (item.Code)
                        {
                            case SubActivityConstants.GenerateToken:
                                var res = await GenerateToken();
                                if (res.Status)
                                {
                                    token = res.Response;
                                }
                                isSuccess = res.Status;
                                break;
                            case SubActivityConstants.CreateLead:
                                var leadres = await AddLead(new GRPCRequest<AyeleadReq> { Request = new AyeleadReq { token = token, LeadId = leadid } });
                                if (leadres.Status)
                                {
                                    response.Message = leadres.Response;
                                }
                                isSuccess = leadres.Status;
                                break;

                            case SubActivityConstants.CheckCreditLimit:
                                var clres = await CheckCreditLine(new GRPCRequest<AyeleadReq> { Request = new AyeleadReq { token = token, LeadId = leadid } });
                                isSuccess = clres.Status;
                                if (clres.Status)
                                {
                                    response.Message = clres.Response.status;
                                }
                                break;
                        }

                    }

                }
            }

            return response;
        }

        public Task<CommonResponseDc> GenerateOtpToAcceptOffer(long LeadMasterId)
        {
            throw new NotImplementedException();
        }


        public Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetDisbursementAPI(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetLeadMasterByLeadId(long leadId)
        {
            throw new NotImplementedException();
        }

        public Task<List<LeadLoanDataDc>> GetLoan(long LeadMasterId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetLoanByLoanId(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<List<OfferEmiDetailDC>>> GetOfferEmiDetails(long leadId, int tenure = 0)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> leadId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<string>> GetPFCollection(long leadid, string MobileNo)
        {
            throw new NotImplementedException();
        }

        public Task<LoanInsuranceConfiguration> GetRateOfInterest(int tenure)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> InsertCeplrBanklist()
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> InsertLoanDataByArthmateTest(long leadid, string loanid)
        {
            throw new NotImplementedException();
        }

        public Task<LBABusinessLoanDc> LBABusinessLoan(long leadid, string AgreementURL, bool IsSubmit, ProductCompanyConfigDc loanconfig)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanDataSave(long leadid, long id)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanNach(string UMRN, long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanRepaymentScheduleDetails(long LeadMasterId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> OfferRegenerate(int LeadId, int tenure, double sactionAmount)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PanUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PersonAddressUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PersonUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PrpareAgreement(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> SaveAgreementESignDocument(long leadmasterid, string eSignDocumentURL)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> UpdateBeneficiaryBankDetail(BeneficiaryBankDetailDc Obj)
        {
            throw new NotImplementedException();
        }
    }
}
