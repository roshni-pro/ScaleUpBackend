using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.ArthMate;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using MassTransit;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using Org.BouncyCastle.Ocsp;


namespace ScaleUP.Services.LeadAPI.GRPC.Server
{
    [Authorize]
    public class LeadGrpcService : ILeadGrpcService
    {
        private readonly LeadGrpcManager _leadGrpcManager;
        private readonly LeadManager _leadManager;
        private readonly ExperianManager _experianManager;
        private readonly eNachManager _eNachManager;
        private readonly ArthMateGrpcManager _ArthMateWebhookManager;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        public LeadGrpcService(LeadGrpcManager leadGrpcManager, ExperianManager experianManager, eNachManager eNachManagers, ArthMateGrpcManager arthMateWebhookManager, LeadNBFCSubActivityManager leadNBFCSubActivityManager, LeadManager leadManager)
        {
            _leadGrpcManager = leadGrpcManager;
            _experianManager = experianManager;
            _eNachManager = eNachManagers;
            _ArthMateWebhookManager = arthMateWebhookManager;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _leadManager = leadManager;
        }

        [AllowAnonymous]
        public Task<LeadActivitySequenceResponse> GetLeadCurrentActivity(LeadActivitySequenceRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadCurrentActivity(request));
            return Task.FromResult(response);
        }
        [Authorize]

        public Task<LeadMobileReply> GetLeadForMobile(LeadMobileRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadForMobile(request));
            return Task.FromResult(response);
        }
        public Task<LeadListPageReply> GetLeadForListPage(LeadListPageRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadForListPage(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<long>> LeadInitiate(InitiateLeadDetail initiateLeadDetail, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.LeadInitiate(initiateLeadDetail));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<bool>> InitiateLeadOffer(InitiateLeadOfferRequest initiateLeadOfferRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.InitiateLeadOffer(initiateLeadOfferRequest));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductId(GRPCRequest<long> LeadId, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadProductId(LeadId));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductIdByRole(GRPCRequest<GetGenerateOfferByFinanceRequestDc> req, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadProductIdByRole(req));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<LeadDetailReponse>> GetLeadByProductIdAndUserId(GRPCRequest<LeadDetailRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadByProductIdAndUserId(request));
            return Task.FromResult(response);
        }



        public Task<GRPCReply<List<LeadActivityProgressListReply>>> GetLeadActivityProgressList(LeadActivityProgressListRequest req, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadActivityProgressList(req));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<LeadOfferReply>>> GetLeadNBFCId(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadNBFCId());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<bool>> UpdateleadOffer(List<NBFCSelfOfferReply> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.UpdateleadOffer(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<string> EntityName, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _leadGrpcManager.GetCurrentNumber(EntityName));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<ExperianStateReply>> GetExperianStateId(GRPCRequest<long> LocationStateId, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _experianManager.GetExperianStateId(LocationStateId.Request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<string>> CheckENachPreviousReq(GRPCRequest<long> requestLeadId, CallContext context = default)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();

            gRPCReply = AsyncContext.Run(() => _eNachManager.eNachCheckENachPreviousReq(requestLeadId));
            return Task.FromResult(gRPCReply);
        }

        public Task<GRPCReply<bool>> eNachAddAsync(GRPCRequest<eNachBankDetailDc> request, CallContext context = default)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            gRPCReply = AsyncContext.Run(() => _eNachManager.eNachAddAsync(request));
            return Task.FromResult(gRPCReply);
        }

        public Task<eNachSendReqDTO> eNachSendeMandateRequestToHDFCAsync(GRPCRequest<SendeMandateRequestDc> request, CallContext context = default)
        {
            eNachSendReqDTO gRPCReply = new eNachSendReqDTO();
            gRPCReply = AsyncContext.Run(() => _eNachManager.eNachSendeMandateRequestToHDFCAsync(request));

            return Task.FromResult(gRPCReply);
        }

        public Task<GRPCReply<LeadDetailDC>> GetLeadUser(GRPCRequest<long> request, CallContext context = default)
        {
            GRPCReply<LeadDetailDC> gRPCReply = new GRPCReply<LeadDetailDC>();
            gRPCReply = AsyncContext.Run(() => _eNachManager.GetLeadUser(request));
            return Task.FromResult(gRPCReply);
        }

        [AllowAnonymous]
        public Task<GRPCReply<bool>> eNachAddResponse(GRPCRequest<eNachResponseDocDC> response, CallContext context = default)
        {

            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            gRPCReply = AsyncContext.Run(() => _eNachManager.eNachAddResponse(response));

            return Task.FromResult(gRPCReply);



        }
        [AllowAnonymous]
        public Task<GRPCReply<LeadCompanyConfigProdReply>> GetLeadAllCompanyAsync(GRPCRequest<LeadCompanyConfigProdRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetLeadAllCompanyAsync(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<LeadAcceptOffer>> GetLeadOfferCompany(GRPCRequest<long> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetLeadOfferCompany(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> AcceptOfferAndAddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.AcceptOfferAndAddNBFCActivity(gRPCLeadProductActivities));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<FileUploadRequest>> CustomerNBFCAgreement(GRPCRequest<NBFCAgreement> request, CallContext context = default)
        {
            try
            {
                var reply = AsyncContext.Run(() => _leadGrpcManager.CustomerNBFCAgreement(request));
                return Task.FromResult(reply);

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public Task<GRPCReply<bool>> AddLeadAgreement(GRPCRequest<LeadAgreementDc> request, CallContext context = default)
        {
            try
            {
                var reply = AsyncContext.Run(() => _leadGrpcManager.AddLeadAgreement(request));
                return Task.FromResult(reply);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public Task<GRPCReply<LeadCreditLimit>> GetCreditLimitByLeadId(GRPCRequest<long> LeadId, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetCreditLimitByLeadId(LeadId));
            return Task.FromResult(res);
        }

        public Task<GRPCReply<List<BankStatementDetailDc>>> GetBankStatementDetailByLeadId(GRPCRequest<BankStatementRequestDc> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetBankStatementDetailByLeadId(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<CreditBureauListDc>> GetCreditBureauDetails(GRPCRequest<string> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetCreditBureauDetails(request));
            return Task.FromResult(res);
        }
        [AllowAnonymous]
        public Task<GRPCReply<long>> GetLeadIdByMobile(GRPCRequest<LeadActivitySequenceRequest> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetLeadIdByMobile(request));
            return Task.FromResult(res);
        }
        [AllowAnonymous]
        public Task<GRPCReply<LeadResponse>> GetLeadInfoById(GRPCRequest<long> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetLeadInfoById(request));
            return Task.FromResult(res);
        }
        [AllowAnonymous] //webhook
        public Task<GRPCReply<BlackSoilUpdateResponse>> GetBlackSoilApplicationDetails(GRPCRequest<long> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetBlackSoilApplicationDetails(request));
            return Task.FromResult(res);
        }


        [AllowAnonymous]
        public Task<GRPCReply<long>> UpdateCurrentActivity(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.UpdateCurrentActivity(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<AgreementDetailDc>> GetAgreementDetail(GRPCRequest<string> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetAgreementDetail(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<bool>> AddUpdateLeadDetail(GRPCRequest<UserDetailsReply> request, CallContext context = default)
        {

            var res = AsyncContext.Run(() => _leadGrpcManager.AddUpdateLeadDetail(request));
            return Task.FromResult(res);
        }

        public Task<GRPCReply<bool>> AddUpdatePersonalBussDetail(GRPCRequest<UpdateAddressRequest> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.AddUpdatePersonalBussDetail(request));
            return Task.FromResult(res);
        }

        public Task<GRPCReply<bool>> IsKycCompleted(GRPCRequest<string> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.IsKycCompleted(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> InsertLeadNBFCApi(GRPCRequest<List<LeadNBFCSubActivityRequestDc>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.InsertLeadNBFCApi(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> selfConfigList, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.AddUpdateSelfConfiguration(selfConfigList));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.BlacksoilCallback(request));
            return Task.FromResult(reply);
        }


        public Task<GRPCReply<List<OfferListReply>>> GetOfferList(GRPCRequest<long> LeadId, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetOfferList(LeadId));
            return Task.FromResult(response);
        }

        [AllowAnonymous]
        public Task<TemplateMasterResponseDc> GetLeadNotificationTemplate(TemplateMasterRequestDc request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadNotificationTemplate(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<string>> GetLeadPreAgreement(GRPCRequest<long> LeadId, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadPreAgreement(LeadId));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetTemplateMasterAsync());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetTemplateById(request));
            return Task.FromResult(response);
        }
        public Task<LeadListPageReply> GetLeadForListPageExport(LeadListPageRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadForListPageExport(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<VerifyLeadDocumentReply>> VerifyLeadDocument(GRPCRequest<VerifyLeadDocumentRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.VerifyLeadDocument(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetAuditLogs(request));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<List<LeadBankDetailResponse>>> GetLeadBankDetailByLeadId(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadBankDetailByLeadId(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<UpdateLeadOfferResponse>> UpdateLeadOffers(GRPCRequest<UpdateLeadOfferRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.UpdateLeadOffers(request));
            return Task.FromResult(response);
        }
        public Task<List<LeadActivityHistoryDc>> LeadActivityHistory(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.LeadActivityHistory(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<string>> ResetLeadActivityMasterProgresse(GRPCRequest<long> LeadId, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.ResetLeadActivityMasterProgresse(LeadId));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<long>> ArthMateAScoreCallback(GRPCRequest<AScoreWebhookDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.ArthMateAScoreCallback(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<AgreementResponseDc>> ArthmateGenerateAgreement(GRPCRequest<ArthmateAgreementRequest> request)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.ArthmateGenerateAgreement(request));
            return Task.FromResult(reply);
        }

        //new for Esign //EsignGenerateAgreement
        public Task<GRPCReply<string>> EsignGenerateAgreement(GRPCRequest<EsignAgreementRequest> request)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.EsignGenerateAgreement(request));
            return Task.FromResult(reply);
        }



        public Task<List<LeadCompanyGenerateOfferNewDTO>> GetLeadActivityOfferStatusNew(GRPCRequest<GenerateOfferStatusPost> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadNBFCSubActivityManager.GetLeadActivityOfferStatusNew(req));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<GetAnchoreProductConfigResponseDc>> GetProductCompanyIdByLeadId(GRPCRequest<long> LeadId, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetProductCompanyIdByLeadId(LeadId));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<leadDashboardResponseDc>> LeadDashboardDetails(leadDashboardDetailDc req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.LeadDashboardDetails(req));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<long>>> GetLeadCityList(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetLeadCityList());
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<leadExportDc>>> LeadExport(leadDashboardDetailDc req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.LeadExport(req));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<string>> SaveArthMateNBFCAgreement(GRPCRequest<ArthMateLeadAgreementDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.SaveArthMateNBFCAgreement(request));
            return Task.FromResult(reply);
        }
        //new for Esign SaveDsaEsignAgreement
        public Task<GRPCReply<string>> SaveDsaEsignAgreement(GRPCRequest<EsignLeadAgreementDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.SaveDsaEsignAgreement(request));
            return Task.FromResult(reply);
        }


        public Task<GRPCReply<string>> SaveOfferEmiDetailsPdf(GRPCRequest<ArthMateLeadAgreementDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.SaveOfferEmiDetailsPdf(request));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<ArthMateLoanDataResDc>> CompositeDisbursement(GRPCRequest<ArthmatDisbursementWebhookRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.CompositeDisbursement(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> AddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.AddNBFCActivity(gRPCLeadProductActivities));
            return Task.FromResult(reply);
        }

        //public Task<GRPCReply<bool>> AddLoanConfiguration(GRPCRequest<AddLoanConfigurationDc> request, CallContext context = default)
        //{
        //    var reply = AsyncContext.Run(() => _leadGrpcManager.AddLoanConfiguration(request));
        //    return Task.FromResult(reply);
        //}
        [AllowAnonymous]//Job
        public Task<GRPCReply<StampRemainderEmailReply>> GetStampRemainderEmailData()
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetStampRemainderEmailData());
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<ArthMateLoanDataResDc>> GetDisbursementAPI(GRPCRequest<long> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.GetDisbursementAPI(request));
            return Task.FromResult(reply);
        }
        [AllowAnonymous] //webhook
        public Task<GRPCReply<string>> eSignDocumentsAsync(GRPCRequest<eSignDocumentStatusDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.eSignDocumentsAsync(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> updateLeadAgreement(GRPCRequest<DSALeadAgreementDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.updateLeadAgreement(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<SCAccountResponseDc>>> GetSCAccountList(SCAccountRequestDC request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetSCAccountList(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<BLAccountResponseDC>>> GetBLAccountList(BLAccountRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetBLAccountList(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetOfferEmiDetailsDownloadPdf(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> AddUpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.AddUpdateBuisnessDetail(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.UploadLeadDocuments(request));
            return Task.FromResult(res);
        }
        [AllowAnonymous]
        public Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.UploadMultiLeadDocuments(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<List<LeadDocumentDetailReply>>> GetLeadDocumentsByLeadId(GRPCRequest<long> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetLeadDocumentsByLeadId(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<bool>> GetLeadActivityOfferStatus(GRPCRequest<long> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetLeadActivityOfferStatus(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<bool>> eSignCallback(GRPCRequest<eSignWebhookResponseDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.eSignCallBack(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<DSADashboardLeadResponse>>> GetDSALeadDashboardData(GRPCRequest<DSADashboardLeadRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetDSALeadDashboardData(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<GetDSALeadPayoutDetailsResponse>> GetDSALeadPayoutDetails(GRPCRequest<long> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetDSALeadPayoutDetails(request));
            return Task.FromResult(reply);
        }


        public Task<GRPCReply<string>> PrepareAgreement(GRPCRequest<long> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.PrepareAgreement(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> UpdateLeadStatus(GRPCRequest<UpdateLeadStatusRequest> request, CallContext callContext = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.UpdateLeadStatus(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> UpdateLeadPersonalDetail(GRPCRequest<UserDetailsReply> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.UpdateLeadPersonalDetail(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<LeadAggrementDetailReponse>> GetDSAAggreementDetailByLeadId(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetDSAAggreementDetailByLeadId(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<LeadDataDC>> GetDSALeadDataById(GRPCRequest<LeadRequestDataDC> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetDSALeadDataById(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<CompanyBuyingHistoryTotalAmountResponseDc>>> GetCompanyBuyingHistory(GRPCRequest<CompanyBuyingHistoryRequestDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetCompanyBuyingHistory(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> AadhaarOtpVerify(GRPCRequest<SecondAadharXMLDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _ArthMateWebhookManager.AadhaarOtpVerify(request));
            return Task.FromResult(reply);
        }

        public Task<LeadListPageReply> GetDSALeadForListPage(LeadListPageRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetDSALeadForListPage(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<bool>> CheckLeadCreatePermission(GRPCRequest<CheckLeadCreatePermissionRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.CheckLeadCreatePermission(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<long>>> GetLeadByIDs(GRPCRequest<List<string>> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadByIDs(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<LeadResponse>> GetLeadInfoByUserId(GRPCRequest<string> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _leadGrpcManager.GetLeadInfoByUserId(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<string>> ActiviateDeActivateLeadInfoByLeadId(GRPCRequest<ActivatDeActivateDSALeadRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.DSADeactivate(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<bool>> GenerateLeadOfferByFinance(InitiateLeadOfferRequest initiateLeadOfferRequest, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GenerateLeadOfferByFinance(initiateLeadOfferRequest));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<string>> UploadMASfinanceAgreement(GRPCRequest<MASFinanceAgreementDc> req, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.UploadMASfinanceAgreement(req));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<LeadCityIds>> GetAllLeadCities(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetAllLeadCities());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<bool>> UpdateBuyingHistory(GRPCRequest<UpdateBuyingHistoryRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.UpdateBuyingHistory(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<double>> GetOfferAmountByNbfc(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetOfferAmountByNbfc(request));
            return Task.FromResult(response);
        }

        public async Task<GRPCReply<bool>> AddLeadConsent(GRPCRequest<long> request, CallContext context = default)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var result = AsyncContext.Run(() => _leadGrpcManager.AddLeadConsentLog(request.Request));
            reply.Response = await Task.FromResult(result);
            return reply;
        }
        public async Task<GRPCReply<LeadDetailForDisbursement>> GetLeadDetailForDisbursement(NBFCDisbursementPostdc request, CallContext context = default)
        {
            var result = AsyncContext.Run(() => _ArthMateWebhookManager.GetLeadDetailForDisbursement(request));
            return await Task.FromResult(result);
        }

        public Task<GRPCReply<bool>> AddLeadOfferConfig(GRPCRequest<AddLeadOfferConfigRequestDc> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.AddLeadOfferConfig(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<string>> GenerateKarzaAadhaarOtpForNBFC(GRPCRequest<AcceptOfferByLeadDc> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadManager.GenerateKarzaAadhaarOtpForNBFC(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<long>> CurrentActivityCompleteForNBFC(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.CurrentActivityCompleteForNBFC(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<LeadAnchorProductRequest>>> GetLeadOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetLeadOfferByLeadId(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<Loandetaildc>> GetNBFCOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.GetNBFCOfferByLeadId(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<BLAccountResponseDC>>> GetNBFCBLAccountList(BLAccountRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetNBFCBLAccountList(request));
            return Task.FromResult(reply);
        }

        [AllowAnonymous]
        public Task<GRPCReply<LeadCibilDataResponseDc>> GetLeadCibilData(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetLeadCibilData(request));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<DashBoardCohortData>> GetDashBoardTATData(GRPCRequest<DashboardTATLeadtDetailRequestDc> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetDashBoardTATData(req));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<string>> GetDSALeadCodeByCreatedBy(GRPCRequest<string> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetDSALeadCodeByCreatedBy(req));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<UpdateKYCStatusResponseDc>> UpdateKYCStatus(UpdateKYCStatusDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.UpdateKYCStatus(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<DSAMISLeadResponseDC>>> GetDSAMISLeadData(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetDSAMISLeadData(request));
            return Task.FromResult(reply);
        }
       
        public Task<GRPCReply<string>> GenerateAyeToken(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GenerateAyeToken());
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.AddLead(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.GetWebUrl(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> request , CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.CheckCreditLine(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.TransactionSendOtp(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _leadGrpcManager.TransactionVerifyOtp(request));
            return Task.FromResult(reply);
        }


        public Task<GRPCReply<long>> UpdateActivityForAyeFin(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _leadGrpcManager.UpdateActivityForAyeFin(request));
            return Task.FromResult(response);
        }

    }
}

