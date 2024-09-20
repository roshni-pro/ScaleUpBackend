using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;

using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.ArthMate;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System.ServiceModel;
using static MassTransit.ValidationResultExtensions;
namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface ILeadService
    {
        Task<GRPCReply<long>> LeadInitiate(InitiateLeadDetail initiateLeadDetail, string token);
        Task<LeadActivitySequenceResponse> GetLeadCurrentActivity(LeadActivitySequenceRequest request);
        Task<LeadMobileReply> GetLeadForMobile(LeadMobileRequest request, string token);
        Task<LeadListPageReply> GetLeadForListPage(LeadListPageRequest request);
        Task<GRPCReply<bool>> InitiateLeadOffer(long LeadId, List<LeadNbfcResponse> Companys, UserDetailsReply kycdetail, List<LeadNBFCSubActivityRequestDc> LeadNBFCSubActivityRequest);
        Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductId(long LeadId);
        Task<GRPCReply<LeadDetailReponse>> GetLeadByProductIdAndUserId(GRPCRequest<LeadDetailRequest> request);

        Task<GRPCReply<List<LeadActivityProgressListReply>>> GetLeadActivityProgressList(LeadActivityProgressListRequest request);

        Task<GRPCReply<List<LeadOfferReply>>> GetLeadNBFCId();
        Task<GRPCReply<bool>> UpdateleadOffer(List<NBFCSelfOfferReply> request);
        Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<string> EntityName);
        Task<GRPCReply<ExperianStateReply>> GetExperianStateId(GRPCRequest<long> LocationStateId);

        Task<GRPCReply<LeadDetailDC>> GetLeadUser(GRPCRequest<long> request);

        Task<GRPCReply<string>> CheckENachPreviousReq(GRPCRequest<long> request);

        Task<GRPCReply<bool>> eNachAddAsync(GRPCRequest<eNachBankDetailDc> request);

        //Task<GRPCReply<eNachSendReqDTO>> eNachSendeMandateRequestToHDFCAsync(GRPCRequest<long> request, GRPCRequest<string> requestUserName);
        Task<eNachSendReqDTO> eNachSendeMandateRequestToHDFCAsync(GRPCRequest<SendeMandateRequestDc> request);
        Task<GRPCReply<LeadCompanyConfigProdReply>> GetLeadAllCompanyAsync(GRPCRequest<LeadCompanyConfigProdRequest> request);
        Task<GRPCReply<LeadAcceptOffer>> GetLeadOfferCompany(GRPCRequest<long> request);
        Task<GRPCReply<bool>> eNachAddResponse(GRPCRequest<eNachResponseDocDC> request);
        Task<GRPCReply<FileUploadRequest>> CustomerNBFCAgreement(GRPCRequest<NBFCAgreement> request);


        Task<GRPCReply<bool>> AcceptOfferAndAddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities);
        Task<GRPCReply<bool>> AddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities);

        Task<GRPCReply<bool>> AddLeadAgreement(GRPCRequest<LeadAgreementDc> request);
        Task<GRPCReply<LeadCreditLimit>> GetCreditLimitByLeadId(GRPCRequest<long> request);

        Task<GRPCReply<List<BankStatementDetailDc>>> GetBankStatementDetailByLeadId(GRPCRequest<BankStatementRequestDc> request);
        Task<GRPCReply<CreditBureauListDc>> GetCreditBureauDetails(GRPCRequest<string> request);

        Task<GRPCReply<long>> GetLeadIdByMobile(GRPCRequest<LeadActivitySequenceRequest> request);

        Task<GRPCReply<LeadResponse>> GetLeadInfoById(GRPCRequest<long> request);
        Task<GRPCReply<BlackSoilUpdateResponse>> GetBlackSoilApplicationDetails(GRPCRequest<long> request);

        Task<GRPCReply<long>> UpdateCurrentActivity(GRPCRequest<long> request);
        Task<GRPCReply<AgreementDetailDc>> GetAgreementDetail(GRPCRequest<string> request);

        Task<GRPCReply<bool>> AddUpdateLeadDetail(GRPCRequest<UserDetailsReply> request);
        Task<GRPCReply<bool>> AddUpdatePersonalBussDetail(GRPCRequest<UpdateAddressRequest> request);

        Task<GRPCReply<bool>> IsKycCompleted(GRPCRequest<string> request);
        Task<GRPCReply<bool>> InsertLeadNBFCApi(GRPCRequest<List<LeadNBFCSubActivityRequestDc>> request);
        Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> selfConfigList);
        Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request);
        Task<GRPCReply<long>> ArthMateAScoreCallback(GRPCRequest<AScoreWebhookDc> request);
        Task<GRPCReply<List<OfferListReply>>> GetOfferList(long LeadId);

        Task<TemplateMasterResponseDc> GetLeadNotificationTemplate(TemplateMasterRequestDc request);
        Task<GRPCReply<string>> GetLeadPreAgreement(long LeadId);
        Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync();
        Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request);
        Task<LeadListPageReply> GetLeadForListPageExport(LeadListPageRequest request);
        Task<GRPCReply<VerifyLeadDocumentReply>> VerifyLeadDocument(GRPCRequest<VerifyLeadDocumentRequest> request);
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request);
        Task<GRPCReply<List<LeadBankDetailResponse>>> GetLeadBankDetailByLeadId(GRPCRequest<long> request);
        Task<GRPCReply<UpdateLeadOfferResponse>> UpdateLeadOffer(GRPCRequest<UpdateLeadOfferRequest> request);
        Task<List<LeadActivityHistoryDc>> LeadActivityHistory(GRPCRequest<long> request);
        Task<GRPCReply<string>> ResetLeadActivityMasterProgresse(GRPCRequest<long> LeadId);
        Task<GRPCReply<AgreementResponseDc>> ArthmateGenerateAgreement(GRPCRequest<ArthmateAgreementRequest> request);
        Task<List<LeadCompanyGenerateOfferNewDTO>> GetLeadActivityOfferStatusNew(GRPCRequest<GenerateOfferStatusPost> req);
        Task<GRPCReply<GetAnchoreProductConfigResponseDc>> GetProductCompanyIdByLeadId(GRPCRequest<long> LeadId);
        Task<GRPCReply<leadDashboardResponseDc>> LeadDashboardDetails(leadDashboardDetailDc req);
        Task<GRPCReply<List<long>>> GetLeadCityList();
        Task<GRPCReply<List<leadExportDc>>> LeadExport(leadDashboardDetailDc req);
        Task<GRPCReply<string>> SaveArthMateNBFCAgreement(GRPCRequest<ArthMateLeadAgreementDc> request);
        //new for Esign
        Task<GRPCReply<string>> SaveDsaEsignAgreement(GRPCRequest<EsignLeadAgreementDc> request);


        Task<GRPCReply<string>> SaveOfferEmiDetailsPdf(GRPCRequest<ArthMateLeadAgreementDc> request);
        Task<GRPCReply<ArthMateLoanDataResDc>> CompositeDisbursement(GRPCRequest<ArthmatDisbursementWebhookRequest> request);
        //Task<GRPCReply<bool>> AddLoanConfiguration(GRPCRequest<AddLoanConfigurationDc> request);
        Task<GRPCReply<StampRemainderEmailReply>> GetStampRemainderEmailData();
        Task<GRPCReply<ArthMateLoanDataResDc>> GetDisbursementAPI(GRPCRequest<long> leadid);
        Task<GRPCReply<string>> eSignDocumentsAsync(GRPCRequest<eSignDocumentStatusDc> request);
        Task<GRPCReply<string>> updateLeadAgreement(GRPCRequest<DSALeadAgreementDc> request);
        Task<GRPCReply<List<SCAccountResponseDc>>> GetSCAccountList(SCAccountRequestDC request);
        Task<GRPCReply<List<BLAccountResponseDC>>> GetBLAccountList(BLAccountRequestDc request);
        Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> request);
        Task<GRPCReply<bool>> AddUpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> request);
        Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> request);
        Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> request);
        Task<GRPCReply<List<LeadDocumentDetailReply>>> GetLeadDocumentsByLeadId(GRPCRequest<long> request);
        Task<GRPCReply<bool>> GetLeadActivityOfferStatus(GRPCRequest<long> request);
        Task<GRPCReply<bool>> eSignCallback(GRPCRequest<eSignWebhookResponseDc> request);

        Task<GRPCReply<string>> EsignGenerateAgreement(GRPCRequest<EsignAgreementRequest> request);
        Task<GRPCReply<List<DSADashboardLeadResponse>>> GetDSALeadDashboardData(GRPCRequest<DSADashboardLeadRequest> request);
        Task<GRPCReply<GetDSALeadPayoutDetailsResponse>> GetDSALeadPayoutDetails(GRPCRequest<long> request);
        Task<GRPCReply<List<CompanyBuyingHistoryTotalAmountResponseDc>>> GetCompanyBuyingHistory(GRPCRequest<CompanyBuyingHistoryRequestDc> request);
        Task<GRPCReply<string>> PrepareAgreement(GRPCRequest<long> request);
        Task<GRPCReply<bool>> UpdateLeadStatus(GRPCRequest<UpdateLeadStatusRequest> request);
        Task<GRPCReply<bool>> UpdateLeadPersonalDetail(GRPCRequest<UserDetailsReply> request);
        Task<GRPCReply<LeadAggrementDetailReponse>> GetDSAAggreementDetailByLeadId(GRPCRequest<long> request);
        Task<GRPCReply<LeadDataDC>> GetDSALeadDataById(GRPCRequest<LeadRequestDataDC> request);
        Task<GRPCReply<string>> AadhaarOtpVerify(GRPCRequest<SecondAadharXMLDc> request);
        Task<LeadListPageReply> GetDSALeadForListPage(LeadListPageRequest request);
        Task<GRPCReply<bool>> CheckLeadCreatePermission(GRPCRequest<CheckLeadCreatePermissionRequest> request);
        Task<GRPCReply<List<long>>> GetLeadByIDs(GRPCRequest<List<string>> request);
        Task<GRPCReply<string>> ActiviateDeActivateLeadInfoByLeadId(GRPCRequest<ActivatDeActivateDSALeadRequest> request);
        Task<GRPCReply<bool>> GenerateLeadOfferByFinance(long LeadId, double CreditLimit, List<LeadNbfcResponse> Companys, UserDetailsReply kycdetail, List<LeadNBFCSubActivityRequestDc> LeadNBFCSubActivityRequest);
        Task<GRPCReply<LeadCityIds>> GetAllLeadCities();

        Task<GRPCReply<string>> UploadMASfinanceAgreement(GRPCRequest<MASFinanceAgreementDc> req);
        Task<GRPCReply<bool>> UpdateBuyingHistory(GRPCRequest<UpdateBuyingHistoryRequest> request);
        Task<GRPCReply<double>> GetOfferAmountByNbfc(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request);
        Task<bool> AddLeadConsent(long LeadId);

        Task<GRPCReply<LeadDetailForDisbursement>> GetLeadDetailForDisbursement(NBFCDisbursementPostdc request);
        Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductIdByRole(GRPCRequest<GetGenerateOfferByFinanceRequestDc> req);
        Task<GRPCReply<bool>> AddLeadOfferConfig(GRPCRequest<AddLeadOfferConfigRequestDc> request);

        Task<GRPCReply<string>> GenerateKarzaAadhaarOtpForNBFC(GRPCRequest<AcceptOfferByLeadDc> request);

        Task<GRPCReply<long>> CurrentActivityCompleteForNBFC(GRPCRequest<long> request);
        Task<GRPCReply<List<LeadAnchorProductRequest>>> GetLeadOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request);
        Task<GRPCReply<Loandetaildc>> GetNBFCOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request);

        Task<GRPCReply<List<BLAccountResponseDC>>> GetNBFCBLAccountList(BLAccountRequestDc request);
        Task<GRPCReply<LeadCibilDataResponseDc>> GetLeadCibilData(GRPCRequest<List<long>> request);
        Task<GRPCReply<DashBoardCohortData>> GetDashBoardTATData(GRPCRequest<DashboardTATLeadtDetailRequestDc> req);
        Task<GRPCReply<UpdateKYCStatusResponseDc>> UpdateKYCStatus(UpdateKYCStatusDc request);
        Task<GRPCReply<string>> GetDSALeadCodeByCreatedBy(GRPCRequest<string> req);
        Task<GRPCReply<List<DSAMISLeadResponseDC>>> GetDSAMISLeadData(GRPCRequest<List<long>> request);

        Task<GRPCReply<string>> GenerateAyeToken();

        Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> request);
        Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> request);
        Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> request);
        Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> request);
        Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> request);
        Task<GRPCReply<long>> UpdateActivityForAyeFin(GRPCRequest<long> request);


    }
}
