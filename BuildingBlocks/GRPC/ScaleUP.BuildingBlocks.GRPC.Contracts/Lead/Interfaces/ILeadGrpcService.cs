using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.ArthMate;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System.ServiceModel;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces
{
    [ServiceContract]
    public interface ILeadGrpcService
    {
        [OperationContract]
        Task<LeadActivitySequenceResponse> GetLeadCurrentActivity(LeadActivitySequenceRequest request,
            CallContext context = default);

        [OperationContract]
        Task<LeadMobileReply> GetLeadForMobile(LeadMobileRequest request,
           CallContext context = default);

        [OperationContract]
        Task<LeadListPageReply> GetLeadForListPage(LeadListPageRequest request,
           CallContext context = default);

        [OperationContract]
        Task<GRPCReply<long>> LeadInitiate(InitiateLeadDetail initiateLeadDetail,
           CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> InitiateLeadOffer(InitiateLeadOfferRequest initiateLeadOfferRequest,
           CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductId(GRPCRequest<long> LeadId,
          CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadDetailReponse>> GetLeadByProductIdAndUserId(GRPCRequest<LeadDetailRequest> request,
          CallContext context = default);




        [OperationContract]
        Task<GRPCReply<List<LeadActivityProgressListReply>>> GetLeadActivityProgressList(LeadActivityProgressListRequest req,
             CallContext context = default);


        [OperationContract]
        Task<GRPCReply<List<LeadOfferReply>>> GetLeadNBFCId(
        CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> UpdateleadOffer(List<NBFCSelfOfferReply> request,
            CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<string> request,
            CallContext context = default);

        [OperationContract]
        Task<GRPCReply<ExperianStateReply>> GetExperianStateId(GRPCRequest<long> LocationStateId,
            CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> CheckENachPreviousReq(GRPCRequest<long> request,
            CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> eNachAddAsync(GRPCRequest<eNachBankDetailDc> request,
          CallContext context = default);

        [OperationContract]
        Task<eNachSendReqDTO> eNachSendeMandateRequestToHDFCAsync(GRPCRequest<SendeMandateRequestDc> request,
        CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadCompanyConfigProdReply>> GetLeadAllCompanyAsync(GRPCRequest<LeadCompanyConfigProdRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<FileUploadRequest>> CustomerNBFCAgreement(GRPCRequest<NBFCAgreement> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadDetailDC>> GetLeadUser(GRPCRequest<long> request,
           CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> eNachAddResponse(GRPCRequest<eNachResponseDocDC> request, CallContext context = default);


        [OperationContract]
        Task<GRPCReply<LeadAcceptOffer>> GetLeadOfferCompany(GRPCRequest<long> leadId, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> AcceptOfferAndAddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> AddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> AddLeadAgreement(GRPCRequest<LeadAgreementDc> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadCreditLimit>> GetCreditLimitByLeadId(GRPCRequest<long> LeadId, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<BankStatementDetailDc>>> GetBankStatementDetailByLeadId(GRPCRequest<BankStatementRequestDc> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<CreditBureauListDc>> GetCreditBureauDetails(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> GetLeadIdByMobile(GRPCRequest<LeadActivitySequenceRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadResponse>> GetLeadInfoById(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> UpdateCurrentActivity(GRPCRequest<long> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<AgreementDetailDc>> GetAgreementDetail(GRPCRequest<string> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> AddUpdateLeadDetail(GRPCRequest<UserDetailsReply> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> AddUpdatePersonalBussDetail(GRPCRequest<UpdateAddressRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> IsKycCompleted(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> InsertLeadNBFCApi(GRPCRequest<List<LeadNBFCSubActivityRequestDc>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> selfConfigList, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<long>> ArthMateAScoreCallback(GRPCRequest<AScoreWebhookDc> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<OfferListReply>>> GetOfferList(GRPCRequest<long> request, CallContext context = default);

        [OperationContract]
        Task<TemplateMasterResponseDc> GetLeadNotificationTemplate(TemplateMasterRequestDc request,
             CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GetLeadPreAgreement(GRPCRequest<long> LeadId,
          CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync(
            CallContext context = default);

        [OperationContract]
        Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request,
            CallContext context = default);

        [OperationContract]
        Task<LeadListPageReply> GetLeadForListPageExport(LeadListPageRequest request, CallContext context = default);


        [OperationContract]
        Task<GRPCReply<VerifyLeadDocumentReply>> VerifyLeadDocument(GRPCRequest<VerifyLeadDocumentRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<LeadBankDetailResponse>>> GetLeadBankDetailByLeadId(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<UpdateLeadOfferResponse>> UpdateLeadOffers(GRPCRequest<UpdateLeadOfferRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<BlackSoilUpdateResponse>> GetBlackSoilApplicationDetails(GRPCRequest<long> request, CallContext context = default);

        [OperationContract]
        Task<List<LeadActivityHistoryDc>> LeadActivityHistory(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> ResetLeadActivityMasterProgresse(GRPCRequest<long> LeadId, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<AgreementResponseDc>> ArthmateGenerateAgreement(GRPCRequest<ArthmateAgreementRequest> request);


        // new for Esign
        [OperationContract]
        Task<GRPCReply<string>> EsignGenerateAgreement(GRPCRequest<EsignAgreementRequest> request);


        [OperationContract]
        Task<GRPCReply<leadDashboardResponseDc>> LeadDashboardDetails(leadDashboardDetailDc req, CallContext context = default);

        [OperationContract]
        Task<List<LeadCompanyGenerateOfferNewDTO>> GetLeadActivityOfferStatusNew(GRPCRequest<GenerateOfferStatusPost> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<GetAnchoreProductConfigResponseDc>> GetProductCompanyIdByLeadId(GRPCRequest<long> LeadId, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<long>>> GetLeadCityList(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<leadExportDc>>> LeadExport(leadDashboardDetailDc req, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> SaveArthMateNBFCAgreement(GRPCRequest<ArthMateLeadAgreementDc> request, CallContext context = default);
        //new for Esign
        [OperationContract]
        Task<GRPCReply<string>> SaveDsaEsignAgreement(GRPCRequest<EsignLeadAgreementDc> request, CallContext context = default);
        Task<GRPCReply<string>> SaveOfferEmiDetailsPdf(GRPCRequest<ArthMateLeadAgreementDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<ArthMateLoanDataResDc>> CompositeDisbursement(GRPCRequest<ArthmatDisbursementWebhookRequest> request, CallContext context = default);
        //[OperationContract]
        //Task<GRPCReply<bool>> AddLoanConfiguration(GRPCRequest<AddLoanConfigurationDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<StampRemainderEmailReply>> GetStampRemainderEmailData();
        [OperationContract]
        Task<GRPCReply<ArthMateLoanDataResDc>> GetDisbursementAPI(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> eSignDocumentsAsync(GRPCRequest<eSignDocumentStatusDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> updateLeadAgreement(GRPCRequest<DSALeadAgreementDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<SCAccountResponseDc>>> GetSCAccountList(SCAccountRequestDC request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<BLAccountResponseDC>>> GetBLAccountList(BLAccountRequestDc request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> AddUpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<LeadDocumentDetailReply>>> GetLeadDocumentsByLeadId(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> GetLeadActivityOfferStatus(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> eSignCallback(GRPCRequest<eSignWebhookResponseDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<DSADashboardLeadResponse>>> GetDSALeadDashboardData(GRPCRequest<DSADashboardLeadRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<GetDSALeadPayoutDetailsResponse>> GetDSALeadPayoutDetails(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> PrepareAgreement(GRPCRequest<long> request, CallContext callContext = default);
        [OperationContract]
        Task<GRPCReply<List<CompanyBuyingHistoryTotalAmountResponseDc>>> GetCompanyBuyingHistory(GRPCRequest<CompanyBuyingHistoryRequestDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateLeadStatus(GRPCRequest<UpdateLeadStatusRequest> request, CallContext callContext = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateLeadPersonalDetail(GRPCRequest<UserDetailsReply> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadAggrementDetailReponse>> GetDSAAggreementDetailByLeadId(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<LeadDataDC>> GetDSALeadDataById(GRPCRequest<LeadRequestDataDC> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> AadhaarOtpVerify(GRPCRequest<SecondAadharXMLDc> request, CallContext context = default);

        [OperationContract]
        Task<LeadListPageReply> GetDSALeadForListPage(LeadListPageRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> CheckLeadCreatePermission(GRPCRequest<CheckLeadCreatePermissionRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<long>>> GetLeadByIDs(GRPCRequest<List<string>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<LeadResponse>> GetLeadInfoByUserId(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> ActiviateDeActivateLeadInfoByLeadId(GRPCRequest<ActivatDeActivateDSALeadRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> GenerateLeadOfferByFinance(InitiateLeadOfferRequest initiateLeadOfferRequest,
          CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> UploadMASfinanceAgreement(GRPCRequest<MASFinanceAgreementDc> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<LeadCityIds>> GetAllLeadCities(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateBuyingHistory(GRPCRequest<UpdateBuyingHistoryRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<double>> GetOfferAmountByNbfc(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> AddLeadConsent(GRPCRequest<long> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadDetailForDisbursement>> GetLeadDetailForDisbursement(NBFCDisbursementPostdc request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductIdByRole(GRPCRequest<GetGenerateOfferByFinanceRequestDc> req,
            CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> AddLeadOfferConfig(GRPCRequest<AddLeadOfferConfigRequestDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GenerateKarzaAadhaarOtpForNBFC(GRPCRequest<AcceptOfferByLeadDc> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<long>> CurrentActivityCompleteForNBFC(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<LeadAnchorProductRequest>>> GetLeadOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<Loandetaildc>> GetNBFCOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<BLAccountResponseDC>>> GetNBFCBLAccountList(BLAccountRequestDc request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LeadCibilDataResponseDc>> GetLeadCibilData(GRPCRequest<List<long>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<DashBoardCohortData>> GetDashBoardTATData(GRPCRequest<DashboardTATLeadtDetailRequestDc> req, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<UpdateKYCStatusResponseDc>> UpdateKYCStatus(UpdateKYCStatusDc request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GetDSALeadCodeByCreatedBy(GRPCRequest<string> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<DSAMISLeadResponseDC>>> GetDSAMISLeadData(GRPCRequest<List<long>> request, CallContext context = default);

      
        [OperationContract]
        Task<GRPCReply<string>> GenerateAyeToken( CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> request, CallContext context = default);
         [OperationContract]
        Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> UpdateActivityForAyeFin(GRPCRequest<long> request, CallContext context = default);

    }

}
