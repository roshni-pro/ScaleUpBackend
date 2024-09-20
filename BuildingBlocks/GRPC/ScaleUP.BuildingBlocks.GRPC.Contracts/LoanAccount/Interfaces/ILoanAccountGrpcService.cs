using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.Interfaces
{
    [ServiceContract]
    public interface ILoanAccountGrpcService
    {
        [OperationContract]
        Task<GRPCReply<long>> SaveLoanAccount(GRPCRequest<SaveLoanAccountRequestDC> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> AddLoanAccountCredit(GRPCRequest<LoanAccountCreditsRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> SaveLoanAccountCompanyLead(GRPCRequest<SaveLoanAccountCompanyLeadRequestDC> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<DisbursementResponse>> GetDisbursement(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> PostTransaction(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request, CallContext context = default);
        //[OperationContract]
        //Task<LoanAccountList> GetLoanAccountList(GRPCRequest<LoanAccountListRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<LoanCreditLimit>> GetAvailableCreditLimitByLeadId(GRPCRequest<long> LeadMasterId, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<PaymentResponseDc>> GetByTransactionReqNo(GRPCRequest<GetPaymentReqByTxnNo> request, CallContext context = default);
        [OperationContract]
        public Task<GRPCReply<double>> GetTransactionInterestRate(GRPCRequest<GetPaymentReqByTxnNo> transNoRequest, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> PostOrderPlacement(GRPCRequest<OrderPlacementRequestDC> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<OrderInitiateResponse>> OrderInitiate(OrderPlacementWithOtp request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<LeadProductDc>> GetLoanAccountDetailByTxnId(GRPCRequest<string> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<LoanAccountReplyDC>> GetLoanAccountById(GRPCRequest<string> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync(CallContext context = default);

        [OperationContract]
        Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> SaveNBFCCompanyAPIs(GRPCRequest<List<CompanyApiReply>> request, CallContext context = default);
        [OperationContract]
        Task<TemplateMasterResponseDc> GetLoanAccountNotificationTemplate(TemplateMasterRequestDc request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<DisbursmentSMSDetailDC>> GetDisbursmentSMSDetail(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> SaveLoanBankDetails(GRPCRequest<List<LeadBankDetailResponse>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> SaveLoanAccountCompLead(GRPCRequest<SaveLoanAccountCompLeadReqDC> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetOverdueAccountsResponse>>> GetOverdueAccounts(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<DisbursmentSMSDetailDC>>> GetDueDisbursmentDetails(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> AnchorOrderCompleted(OrderCompletedRequest orderCompletedRequest, CallContext context = default);
        [OperationContract]
        Task<InvoiceResponseDC> UpdateInvoiceInformation(InvoiceRequestDC request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GetTransactionMobileNo(TranMobileRequest tranMobileRequest, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateTransactionStatusJob(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<AnchorMISListResponseDC>>> GetAnchorMISList(AnchorMISRequestDC obj, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<NbfcMisListResponseDc>>> GetNbfcMISList(NbfcMisListRequestDc obj, CallContext context = default);
        [OperationContract]
        Task<LoanAccountDashboardResponse> ScaleupLoanAccountDashboardDetails(DashboardLoanAccountDetailDc req, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<long>> PostArthMateDataLeadToLoan(ArthMateLoanDataResDc req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<LoanAccountListResponseDc>>> GetBusinessLoanAccountList(LoanAccountListRequestDc request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<CompanyInvoicesChargesResponseDc>>> GetCompanyInvoiceCharges(CompanyInvoiceChargesRequestDc request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<CompanyInvoicesListResponseDc>>> GetCompanyInvoiceList(CompanyInvoiceRequestDc request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<CompanyInvoiceDetailsResponseDc>>> GetCompanyInvoiceDetails(CompanyInvoiceDetailsRequestDC request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<UpdateCompanyInvoiceReplyDC>> UpdateCompanyInvoiceDetails(GRPCRequest<UpdateCompanyInvoiceRequestDC> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateCompnayInvoicePdfPath(GRPCRequest<UpdateCompanyInvoicePDFDC> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<LoanAccountDisbursementResponse>>> GetLoanAccountDisbursement(GRPCRequest<List<long>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<DSADashboardLoanResponse>> GetDSALoanDashboardData(GRPCRequest<DSALoanDashboardDataRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> AddDSALoanAccount(GRPCRequest<AddDSALoanAccountRequest> request);

        [OperationContract]
        Task<GRPCReply<List<GetSalesAgentLoanDisbursmentDc>>> GetSalesAgentLoanDisbursment(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<EmailAnchorMISDataJobResponse>>> GetEmailAnchorMISDataJob(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> SaveSalesAgentLoanDisbursmentData(GRPCRequest<SalesAgentDisbursmentDataDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<DSADashboardPayoutResponse>> GetDSALoanPayoutList(GRPCRequest<DSALoanDashboardDataRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetInvoiceRegisterDataResponse>>> GetInvoiceRegisterData(GRPCRequest<GetInvoiceRegisterDataRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> PostNBFCDisbursement(GRPCRequest<LeadDetailForDisbursement> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> AddInvoiceSettlementData(GRPCRequest<SettleCompanyInvoiceTransactionsRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetSettlePaymentJobData(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> SettlePaymentLaterJob(GRPCRequest<SettlePaymentJobRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetScaleUpShareCompanyData(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> InsertScaleUpShareTransactions(GRPCRequest<SettlePaymentJobRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<LoanAccountListResponseDc>>> GetNBFCBusinessLoanAccountList(LoanAccountListRequestDc request, CallContext context = default);
        [OperationContract]
        Task<DisbursementDashboardDataResponseDc> GetHopTrendDashboardData(HopDashboardRequestDc req, CallContext context = default);
        [OperationContract]
        public Task<HopDisbursementDashboardResponseDc> GetHopDisbursementDashboard(HopDashboardRequestDc req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<long>>> GetLoanAccountData(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<DSAMISLoanResponseDC>>> GetDSAMISLoanData(DSAMISRequestDC request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<LoanPaymentsResultDC>> GetLedger_RetailerStatement(GetLedger_RetailerStatementDC request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> InsertBLEmiTransactionsJob(GRPCReply<List<BLNBFCConfigs>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UploadBLInvoiceExcel(UploadBLInvoiceExcelReq request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<BLDetailsDc>> GetBLDetails(GRPCRequest<long> req,CallContext context = default); 
        [OperationContract]
        Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> req,CallContext context = default);
        [OperationContract]
        Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> req,CallContext context = default);
        [OperationContract]
        Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<LeadProductDc>> GetNBFCLoanAccountDetailByTxnId(GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> RefundTransaction(GRPCRequest<RefundRequestDTO> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> SaveLoanAccountDetails(GRPCRequest<SaveLoanAccountRequestDC> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<NBFCLoanAccountDetailResponseDTO>> GetNBFCLoanAccountDetail(GRPCRequest<GetNBFCLoanAccountDetailDTO> req,CallContext context = default);

    }
}
