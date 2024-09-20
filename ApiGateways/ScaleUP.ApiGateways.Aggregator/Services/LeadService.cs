using Grpc.Net.Client;
using Microsoft.AspNetCore.Http.HttpResults;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.ArthMate;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class LeadService : ILeadService
    {
        private IConfiguration Configuration;
        private readonly ILeadGrpcService _client;

        public LeadService(IConfiguration _configuration
                           , ILeadGrpcService client)
        {
            Configuration = _configuration;
            _client = client;
        }
        public async Task<LeadActivitySequenceResponse> GetLeadCurrentActivity(LeadActivitySequenceRequest request)
        {
            var reply = await _client.GetLeadCurrentActivity(request);
            return reply;
        }

        public async Task<LeadListPageReply> GetLeadForListPage(LeadListPageRequest request)
        {
            var reply = await _client.GetLeadForListPage(request);
            return reply;
        }

        public async Task<GRPCReply<long>> LeadInitiate(InitiateLeadDetail initiateLeadDetail, string token)
        {
            var _clientnew = ConfigureServices.GetCustomeChennle<ILeadGrpcService>(token, EnvironmentConstants.LeadUrl);
            var reply = await _clientnew.LeadInitiate(initiateLeadDetail);
            return reply;
        }

        public async Task<LeadMobileReply> GetLeadForMobile(LeadMobileRequest request, string token)
        {
            var _clientnew = ConfigureServices.GetCustomeChennle<ILeadGrpcService>(token, EnvironmentConstants.LeadUrl);
            var reply = await _clientnew.GetLeadForMobile(request);
            return reply;
        }

        public async Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductId(long LeadId)
        {
            GRPCRequest<long> LeadRequest = new GRPCRequest<long> { Request = LeadId };
            var reply = await _client.GetLeadProductId(LeadRequest);
            return reply;
        }
        public async Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductIdByRole(GRPCRequest<GetGenerateOfferByFinanceRequestDc> req)
        {
            var reply = await _client.GetLeadProductIdByRole(req);
            return reply;
        }
        public async Task<GRPCReply<LeadDetailReponse>> GetLeadByProductIdAndUserId(GRPCRequest<LeadDetailRequest> req)
        {

            var reply = await _client.GetLeadByProductIdAndUserId(req);
            return reply;
        }

        public async Task<GRPCReply<bool>> InitiateLeadOffer(long LeadId, List<LeadNbfcResponse> Companys, UserDetailsReply kycdetail, List<LeadNBFCSubActivityRequestDc> LeadNBFCSubActivityRequest)
        {
            InitiateLeadOfferRequest initiateLeadOfferRequest = new InitiateLeadOfferRequest
            {
                Companys = Companys,
                LeadId = LeadId,
                kycdetail = kycdetail,
                LeadNBFCSubActivityRequest = LeadNBFCSubActivityRequest
            };
            var reply = await _client.InitiateLeadOffer(initiateLeadOfferRequest);
            return reply;
        }


        public async Task<GRPCReply<List<LeadActivityProgressListReply>>> GetLeadActivityProgressList(LeadActivityProgressListRequest req)
        {
            var reply = await _client.GetLeadActivityProgressList(new LeadActivityProgressListRequest
            {
                LeadId = req.LeadId
            });
            return reply;
        }

        public async Task<GRPCReply<List<LeadOfferReply>>> GetLeadNBFCId()
        {
            var reply = await _client.GetLeadNBFCId();
            return reply;
        }
        public async Task<GRPCReply<bool>> UpdateleadOffer(List<NBFCSelfOfferReply> request)
        {
            var reply = await _client.UpdateleadOffer(request);
            return reply;
        }
        public async Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<string> EntityName)
        {
            var reply = await _client.GetCurrentNumber(EntityName);
            return reply;
        }

        public async Task<GRPCReply<ExperianStateReply>> GetExperianStateId(GRPCRequest<long> LocationStateId)
        {
            var reply = await _client.GetExperianStateId(LocationStateId);
            return reply;
        }


        public async Task<GRPCReply<string>> CheckENachPreviousReq(GRPCRequest<long> request)
        {
            var reply = await _client.CheckENachPreviousReq(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> eNachAddAsync(GRPCRequest<eNachBankDetailDc> request)
        {
            var reply = await _client.eNachAddAsync(request);
            return reply;
        }

        public async Task<eNachSendReqDTO> eNachSendeMandateRequestToHDFCAsync(GRPCRequest<SendeMandateRequestDc> request)
        {
            var reply = await _client.eNachSendeMandateRequestToHDFCAsync(request);
            return reply;
        }

        public async Task<GRPCReply<LeadDetailDC>> GetLeadUser(GRPCRequest<long> request)
        {
            var reply = await _client.GetLeadUser(request);
            return reply;
        }

        [AllowAnonymous]
        public async Task<GRPCReply<bool>> eNachAddResponse(GRPCRequest<eNachResponseDocDC> response)
        {

            var result = await _client.eNachAddResponse(response);
            return result;


        }

        [AllowAnonymous]
        public async Task<GRPCReply<LeadCompanyConfigProdReply>> GetLeadAllCompanyAsync(GRPCRequest<LeadCompanyConfigProdRequest> request)
        {
            var reply = await _client.GetLeadAllCompanyAsync(request);
            return reply;
        }

        public async Task<GRPCReply<LeadAcceptOffer>> GetLeadOfferCompany(GRPCRequest<long> request)
        {
            var reply = await _client.GetLeadOfferCompany(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> AcceptOfferAndAddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities)
        {
            var reply = await _client.AcceptOfferAndAddNBFCActivity(gRPCLeadProductActivities);
            return reply;
        }

        public async Task<GRPCReply<FileUploadRequest>> CustomerNBFCAgreement(GRPCRequest<NBFCAgreement> request)
        {
            try
            {

                var reply = await _client.CustomerNBFCAgreement(request);
                return reply;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<GRPCReply<bool>> AddLeadAgreement(GRPCRequest<LeadAgreementDc> request)
        {
            GRPCReply<bool> res = new GRPCReply<bool>();
            var result = await _client.AddLeadAgreement(request);
            return res;
        }

        public async Task<GRPCReply<LeadCreditLimit>> GetCreditLimitByLeadId(GRPCRequest<long> request)
        {
            var result = await _client.GetCreditLimitByLeadId(request);
            return result;
        }

        public async Task<GRPCReply<List<BankStatementDetailDc>>> GetBankStatementDetailByLeadId(GRPCRequest<BankStatementRequestDc> request)
        {
            var result = await _client.GetBankStatementDetailByLeadId(request);
            return result;
        }
        public async Task<GRPCReply<CreditBureauListDc>> GetCreditBureauDetails(GRPCRequest<string> request)
        {
            var result = await _client.GetCreditBureauDetails(request);
            return result;
        }

        public async Task<GRPCReply<long>> GetLeadIdByMobile(GRPCRequest<LeadActivitySequenceRequest> request)
        {
            var result = await _client.GetLeadIdByMobile(request);
            return result;
        }

        public async Task<GRPCReply<LeadResponse>> GetLeadInfoById(GRPCRequest<long> request)
        {
            var result = await _client.GetLeadInfoById(request);
            return result;
        }
        public async Task<GRPCReply<BlackSoilUpdateResponse>> GetBlackSoilApplicationDetails(GRPCRequest<long> request)
        {
            var result = await _client.GetBlackSoilApplicationDetails(request);
            return result;
        }

        [AllowAnonymous]
        public async Task<GRPCReply<long>> UpdateCurrentActivity(GRPCRequest<long> request)
        {
            var result = await _client.UpdateCurrentActivity(request);
            return result;
        }
        public async Task<GRPCReply<AgreementDetailDc>> GetAgreementDetail(GRPCRequest<string> request)
        {
            var result = await _client.GetAgreementDetail(request);
            return result;
        }


        public async Task<GRPCReply<bool>> AddUpdateLeadDetail(GRPCRequest<UserDetailsReply> request)
        {
            return await _client.AddUpdateLeadDetail(request);
        }

        public async Task<GRPCReply<bool>> AddUpdatePersonalBussDetail(GRPCRequest<UpdateAddressRequest> request)
        {
            return await _client.AddUpdatePersonalBussDetail(request);
        }

        public async Task<GRPCReply<bool>> IsKycCompleted(GRPCRequest<string> request)
        {
            var reply = await _client.IsKycCompleted(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> InsertLeadNBFCApi(GRPCRequest<List<LeadNBFCSubActivityRequestDc>> request)
        {
            var reply = await _client.InsertLeadNBFCApi(request);
            return reply;
        }

        public async Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> selfConfigList)
        {
            var reply = await _client.AddUpdateSelfConfiguration(selfConfigList);
            return reply;
        }

        public async Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request)
        {
            var reply = await _client.BlacksoilCallback(request);
            return reply;
        }
        public async Task<GRPCReply<long>> ArthMateAScoreCallback(GRPCRequest<AScoreWebhookDc> request)
        {
            var reply = await _client.ArthMateAScoreCallback(request);
            return reply;
        }
        public async Task<GRPCReply<List<OfferListReply>>> GetOfferList(long LeadId)
        {
            GRPCRequest<long> LeadRequest = new GRPCRequest<long> { Request = LeadId };
            var reply = await _client.GetOfferList(LeadRequest);
            return reply;
        }

        public async Task<TemplateMasterResponseDc> GetLeadNotificationTemplate(TemplateMasterRequestDc request)
        {
            var reply = await _client.GetLeadNotificationTemplate(request);
            return reply;
        }


        public async Task<GRPCReply<string>> GetLeadPreAgreement(long LeadId)
        {
            GRPCRequest<long> LeadRequest = new GRPCRequest<long> { Request = LeadId };
            var reply = await _client.GetLeadPreAgreement(LeadRequest);
            return reply;
        }

        public async Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync()
        {
            var reply = await _client.GetTemplateMasterAsync();
            return reply;
        }

        public async Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request)
        {
            var reply = await _client.GetTemplateById(request);
            return reply;
        }
        public async Task<LeadListPageReply> GetLeadForListPageExport(LeadListPageRequest request)
        {
            var reply = await _client.GetLeadForListPageExport(request);
            return reply;
        }
        public async Task<GRPCReply<VerifyLeadDocumentReply>> VerifyLeadDocument(GRPCRequest<VerifyLeadDocumentRequest> request)
        {
            var result = await _client.VerifyLeadDocument(request);
            return result;
        }

        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            var result = await _client.GetAuditLogs(request);
            return result;
        }
        [AllowAnonymous]
        public async Task<GRPCReply<List<LeadBankDetailResponse>>> GetLeadBankDetailByLeadId(GRPCRequest<long> request)
        {
            var result = await _client.GetLeadBankDetailByLeadId(request);
            return result;
        }
        public async Task<GRPCReply<UpdateLeadOfferResponse>> UpdateLeadOffer(GRPCRequest<UpdateLeadOfferRequest> request)
        {
            var reply = await _client.UpdateLeadOffers(request);
            return reply;
        }
        public async Task<List<LeadActivityHistoryDc>> LeadActivityHistory(GRPCRequest<long> request)
        {
            var reply = await _client.LeadActivityHistory(request);
            return reply;
        }
        public async Task<GRPCReply<string>> ResetLeadActivityMasterProgresse(GRPCRequest<long> LeadId)
        {
            var reply = await _client.ResetLeadActivityMasterProgresse(LeadId);
            return reply;
        }

        public async Task<GRPCReply<AgreementResponseDc>> ArthmateGenerateAgreement(GRPCRequest<ArthmateAgreementRequest> request)
        {
            var reply = await _client.ArthmateGenerateAgreement(request);
            return reply;
        }
        public async Task<List<LeadCompanyGenerateOfferNewDTO>> GetLeadActivityOfferStatusNew(GRPCRequest<GenerateOfferStatusPost> req)
        {
            var reply = await _client.GetLeadActivityOfferStatusNew(req);
            return reply;
        }
        public async Task<GRPCReply<GetAnchoreProductConfigResponseDc>> GetProductCompanyIdByLeadId(GRPCRequest<long> request)
        {
            var reply = await _client.GetProductCompanyIdByLeadId(request);
            return reply;
        }
        public async Task<GRPCReply<leadDashboardResponseDc>> LeadDashboardDetails(leadDashboardDetailDc req)
        {
            var reply = await _client.LeadDashboardDetails(req);
            return reply;
        }

        public async Task<GRPCReply<List<long>>> GetLeadCityList()
        {
            var reply = await _client.GetLeadCityList();
            return reply;
        }

        public async Task<GRPCReply<List<leadExportDc>>> LeadExport(leadDashboardDetailDc req)
        {
            var reply = await _client.LeadExport(req);
            return reply;
        }

        public async Task<GRPCReply<string>> SaveArthMateNBFCAgreement(GRPCRequest<ArthMateLeadAgreementDc> request)
        {
            var result = await _client.SaveArthMateNBFCAgreement(request);
            return result;
        }

        //new for Esign  SaveDsaEsignAgreement
        public async Task<GRPCReply<string>> SaveDsaEsignAgreement(GRPCRequest<EsignLeadAgreementDc> request)
        {
            var result = await _client.SaveDsaEsignAgreement(request);
            return result;
        }


        public async Task<GRPCReply<string>> SaveOfferEmiDetailsPdf(GRPCRequest<ArthMateLeadAgreementDc> request)
        {
            var result = await _client.SaveOfferEmiDetailsPdf(request);
            return result;
        }

        public async Task<GRPCReply<ArthMateLoanDataResDc>> CompositeDisbursement(GRPCRequest<ArthmatDisbursementWebhookRequest> request)
        {
            return await _client.CompositeDisbursement(request);
        }

        public async Task<GRPCReply<bool>> AddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities)
        {
            var result = await _client.AddNBFCActivity(gRPCLeadProductActivities);
            return result;
        }

        //public async Task<GRPCReply<bool>> AddLoanConfiguration(GRPCRequest<AddLoanConfigurationDc> request)
        //{
        //    var result = await _client.AddLoanConfiguration(request);
        //    return result;
        //}

        public async Task<GRPCReply<StampRemainderEmailReply>> GetStampRemainderEmailData()
        {
            var result = await _client.GetStampRemainderEmailData();
            return result;
        }
        public async Task<GRPCReply<ArthMateLoanDataResDc>> GetDisbursementAPI(GRPCRequest<long> request)
        {
            GRPCReply<ArthMateLoanDataResDc> result = new GRPCReply<ArthMateLoanDataResDc>();
            result = await _client.GetDisbursementAPI(request);
            return result;
        }
        public async Task<GRPCReply<string>> eSignDocumentsAsync(GRPCRequest<eSignDocumentStatusDc> request)
        {
            // GRPCReply<ArthMateLoanDataResDc> result = new GRPCReply<ArthMateLoanDataResDc>();
            var result = await _client.eSignDocumentsAsync(request);
            return result;
        }
        public async Task<GRPCReply<string>> updateLeadAgreement(GRPCRequest<DSALeadAgreementDc> request)
        {
            var result = await _client.updateLeadAgreement(request);
            return result;
        }
        public async Task<GRPCReply<List<SCAccountResponseDc>>> GetSCAccountList(SCAccountRequestDC request)
        {
            var reply = await _client.GetSCAccountList(request);
            return reply;
        }
        public async Task<GRPCReply<List<BLAccountResponseDC>>> GetBLAccountList(BLAccountRequestDc request)
        {
            var reply = await _client.GetBLAccountList(request);
            return reply;
        }
        public async Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> request)
        {
            var reply = await _client.GetOfferEmiDetailsDownloadPdf(request);
            return reply;
        }
        public async Task<GRPCReply<bool>> AddUpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> request)
        {
            return await _client.AddUpdateBuisnessDetail(request);
        }
        public async Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> request)
        {
            return await _client.UploadLeadDocuments(request);
        }
        public async Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> request)
        {
            return await _client.UploadMultiLeadDocuments(request);
        }
        public async Task<GRPCReply<List<LeadDocumentDetailReply>>> GetLeadDocumentsByLeadId(GRPCRequest<long> request)
        {
            var result = await _client.GetLeadDocumentsByLeadId(request);
            return result;
        }
        public async Task<GRPCReply<bool>> GetLeadActivityOfferStatus(GRPCRequest<long> request)
        {
            var result = await _client.GetLeadActivityOfferStatus(request);
            return result;
        }
        public async Task<GRPCReply<bool>> eSignCallback(GRPCRequest<eSignWebhookResponseDc> request)
        {
            var result = await _client.eSignCallback(request);
            return result;
        }
        public async Task<GRPCReply<string>> EsignGenerateAgreement(GRPCRequest<EsignAgreementRequest> request)
        {
            var reply = await _client.EsignGenerateAgreement(request);
            return reply;
        }

        public async Task<GRPCReply<List<DSADashboardLeadResponse>>> GetDSALeadDashboardData(GRPCRequest<DSADashboardLeadRequest> request)
        {
            var reply = await _client.GetDSALeadDashboardData(request);
            return reply;
        }

        public async Task<GRPCReply<GetDSALeadPayoutDetailsResponse>> GetDSALeadPayoutDetails(GRPCRequest<long> request)
        {
            var reply = await _client.GetDSALeadPayoutDetails(request);
            return reply;
        }

        public async Task<GRPCReply<string>> PrepareAgreement(GRPCRequest<long> request)
        {
            var reply = await _client.PrepareAgreement(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> UpdateLeadStatus(GRPCRequest<UpdateLeadStatusRequest> request)
        {
            var reply = await _client.UpdateLeadStatus(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> UpdateLeadPersonalDetail(GRPCRequest<UserDetailsReply> request)
        {
            return await _client.UpdateLeadPersonalDetail(request);
        }

        public async Task<GRPCReply<LeadAggrementDetailReponse>> GetDSAAggreementDetailByLeadId(GRPCRequest<long> req)
        {

            var reply = await _client.GetDSAAggreementDetailByLeadId(req);
            return reply;
        }

        public async Task<GRPCReply<LeadDataDC>> GetDSALeadDataById(GRPCRequest<LeadRequestDataDC> request)
        {
            var result = await _client.GetDSALeadDataById(request);
            return result;
        }
        public async Task<GRPCReply<List<CompanyBuyingHistoryTotalAmountResponseDc>>> GetCompanyBuyingHistory(GRPCRequest<CompanyBuyingHistoryRequestDc> request)
        {
            var reply = await _client.GetCompanyBuyingHistory(request);
            return reply;
        }


        public async Task<GRPCReply<string>> AadhaarOtpVerify(GRPCRequest<SecondAadharXMLDc> request)
        {
            var reply = await _client.AadhaarOtpVerify(request);
            return reply;
        }
        public async Task<LeadListPageReply> GetDSALeadForListPage(LeadListPageRequest request)
        {
            var reply = await _client.GetDSALeadForListPage(request);
            return reply;
        }


        public async Task<GRPCReply<bool>> CheckLeadCreatePermission(GRPCRequest<CheckLeadCreatePermissionRequest> request)
        {
            var reply = await _client.CheckLeadCreatePermission(request);
            return reply;
        }
        public async Task<GRPCReply<List<long>>> GetLeadByIDs(GRPCRequest<List<string>> request)
        {
            var reply = await _client.GetLeadByIDs(request);
            return reply;
        }
        public async Task<GRPCReply<LeadResponse>> GetLeadInfoByUserId(GRPCRequest<string> request)
        {
            var result = await _client.GetLeadInfoByUserId(request);
            return result;
        }

        public async Task<GRPCReply<string>> ActiviateDeActivateLeadInfoByLeadId(GRPCRequest<ActivatDeActivateDSALeadRequest> request)
        {
            var result = await _client.ActiviateDeActivateLeadInfoByLeadId(request);
            return result;
        }
        public async Task<GRPCReply<bool>> GenerateLeadOfferByFinance(long LeadId, double CreditLimit, List<LeadNbfcResponse> Companys, UserDetailsReply kycdetail, List<LeadNBFCSubActivityRequestDc> LeadNBFCSubActivityRequest)
        {
            InitiateLeadOfferRequest initiateLeadOfferRequest = new InitiateLeadOfferRequest
            {
                Companys = Companys,
                LeadId = LeadId,
                kycdetail = kycdetail,
                LeadNBFCSubActivityRequest = LeadNBFCSubActivityRequest,
                CreditLimit = CreditLimit
            };
            var reply = await _client.GenerateLeadOfferByFinance(initiateLeadOfferRequest);
            return reply;
        }
        public async Task<GRPCReply<string>> UploadMASfinanceAgreement(GRPCRequest<MASFinanceAgreementDc> req)
        {
            var reply = await _client.UploadMASfinanceAgreement(req);
            return reply;
        }

        public async Task<GRPCReply<LeadCityIds>> GetAllLeadCities()
        {
            GRPCReply<LeadCityIds> res = new GRPCReply<LeadCityIds>();
            try
            {
                var result = await _client.GetAllLeadCities();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
        public async Task<GRPCReply<double>> GetOfferAmountByNbfc(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request)
        {
            var reply = await _client.GetOfferAmountByNbfc(request);
            return reply;
        }
        public async Task<GRPCReply<bool>> UpdateBuyingHistory(GRPCRequest<UpdateBuyingHistoryRequest> request)
        {
            var reply = await _client.UpdateBuyingHistory(request);
            return reply;
        }
        public async Task<bool> AddLeadConsent(long request)
        {
            GRPCRequest<long> request1 = new GRPCRequest<long>();
            request1.Request = request;
            var result = await _client.AddLeadConsent(request1);
            return result.Response;
        }

        public async Task<GRPCReply<LeadDetailForDisbursement>> GetLeadDetailForDisbursement(NBFCDisbursementPostdc request)
        {
            var result = await _client.GetLeadDetailForDisbursement(request);
            return result;
        }

        public async Task<GRPCReply<bool>> AddLeadOfferConfig(GRPCRequest<AddLeadOfferConfigRequestDc> request)
        {
            var result = await _client.AddLeadOfferConfig(request);
            return result;
        }

        public async Task<GRPCReply<string>> GenerateKarzaAadhaarOtpForNBFC(GRPCRequest<AcceptOfferByLeadDc> request)
        {
            var result = await _client.GenerateKarzaAadhaarOtpForNBFC(request);
            return result;
        }

        public async Task<GRPCReply<long>> CurrentActivityCompleteForNBFC(GRPCRequest<long> request)
        {
            var result = await _client.CurrentActivityCompleteForNBFC(request);
            return result;
        }
        public async Task<GRPCReply<List<LeadAnchorProductRequest>>> GetLeadOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request)
        {
            var result = await _client.GetLeadOfferByLeadId(request);
            return result;
        }
        public async Task<GRPCReply<Loandetaildc>> GetNBFCOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request)
        {
            var result = await _client.GetNBFCOfferByLeadId(request);
            return result;
        }
        public async Task<GRPCReply<List<BLAccountResponseDC>>> GetNBFCBLAccountList(BLAccountRequestDc request)
        {
            var reply = await _client.GetNBFCBLAccountList(request);
            return reply;
        }
        public async Task<GRPCReply<LeadCibilDataResponseDc>> GetLeadCibilData(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetLeadCibilData(request);
            return reply;
        }
        public async Task<GRPCReply<DashBoardCohortData>> GetDashBoardTATData(GRPCRequest<DashboardTATLeadtDetailRequestDc> req)
        {
            var reply = await _client.GetDashBoardTATData(req);
            return reply;
        }
        public async Task<GRPCReply<UpdateKYCStatusResponseDc>> UpdateKYCStatus(UpdateKYCStatusDc request)
        {
            var reply = await _client.UpdateKYCStatus(request);
            return reply;
        }
        public async Task<GRPCReply<string>> GetDSALeadCodeByCreatedBy(GRPCRequest<string> req)
        {
            var reply = await _client.GetDSALeadCodeByCreatedBy(req);
            return reply;
        }

        public async Task<GRPCReply<List<DSAMISLeadResponseDC>>> GetDSAMISLeadData(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetDSAMISLeadData(request);
            return reply;
        }


        public async Task<GRPCReply<string>> GenerateAyeToken()
        {
            var reply = await _client.GenerateAyeToken();
            return reply;
        }

        public async Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> request)
        {
            var reply = await _client.AddLead(request);
            return reply;
        }

        public async Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> request)
        {
            var reply = await _client.GetWebUrl(request);
            return reply;
        }

        public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> request)
        {
            var reply = await _client.CheckCreditLine(request);
            return reply;
        }

        public async Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> request)
        {
            var reply = await _client.TransactionSendOtp(request);
            return reply;
        }
        public async Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> request)
        {
            var reply = await _client.TransactionVerifyOtp(request);
            return reply;
        }

        public async Task<GRPCReply<long>> UpdateActivityForAyeFin(GRPCRequest<long> request)
        {
            var result = await _client.UpdateActivityForAyeFin(request);
            return result;
        }

    }
}


