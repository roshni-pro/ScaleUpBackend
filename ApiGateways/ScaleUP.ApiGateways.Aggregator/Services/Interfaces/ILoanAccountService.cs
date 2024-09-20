using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;

using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System.ServiceModel;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface ILoanAccountService
    {
        Task<GRPCReply<long>> SaveLoanAccount(GRPCRequest<SaveLoanAccountRequestDC> request);

        Task<GRPCReply<long>> AddLoanAccountCredit(GRPCRequest<LoanAccountCreditsRequest> request);
        Task<GRPCReply<long>> SaveLoanAccountCompanyLead(GRPCRequest<SaveLoanAccountCompanyLeadRequestDC> request);
        Task<GRPCReply<DisbursementResponse>> GetDisbursement(GRPCRequest<long> request);
        Task<GRPCReply<long>> PostTransaction(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request);
        Task<GRPCReply<PaymentResponseDc>> GetByTransactionReqNo(GRPCRequest<GetPaymentReqByTxnNo> request);
        Task<GRPCReply<double>> GetTransactionInterestRate(GRPCRequest<GetPaymentReqByTxnNo> transNoRequest);
        Task<GRPCReply<string>> PostOrderPlacement(GRPCRequest<OrderPlacementRequestDC> request);
        Task<GRPCReply<LoanCreditLimit>> GetAvailableCreditLimitByLeadId(GRPCRequest<long> LeadMasterId);
        Task<GRPCReply<OrderInitiateResponse>> OrderInitiate(OrderPlacementWithOtp request);
        Task<GRPCReply<LeadProductDc>> GetLoanAccountDetailByTxnId(GRPCRequest<string> request);
        Task<GRPCReply<LoanAccountReplyDC>> GetLoanAccountById(GRPCRequest<string> request);
        Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request);
        Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync();
        Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request);
        Task<GRPCReply<long>> SaveNBFCCompanyAPIs(GRPCRequest<List<CompanyApiReply>> request);
        Task<TemplateMasterResponseDc> GetLoanAccountNotificationTemplate(TemplateMasterRequestDc request);
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request);
        Task<GRPCReply<DisbursmentSMSDetailDC>> GetDisbursmentSMSDetail(GRPCRequest<long> request);
        Task<GRPCReply<bool>> SaveLoanBankDetails(GRPCRequest<List<LeadBankDetailResponse>> request);
        Task<GRPCReply<long>> SaveLoanAccountCompLead(GRPCRequest<SaveLoanAccountCompLeadReqDC> request);
        Task<GRPCReply<List<GetOverdueAccountsResponse>>> GetOverdueAccounts();

        Task<GRPCReply<List<DisbursmentSMSDetailDC>>> GetDueDisbursmentDetails();
        Task<GRPCReply<string>> AnchorOrderCompleted(OrderCompletedRequest orderCompletedRequest, string token);

        Task<InvoiceResponseDC> UpdateInvoiceInformation(InvoiceRequestDC request, string token);

        Task<GRPCReply<string>> GetTransactionMobileNo(TranMobileRequest tranMobileRequest);
        Task<GRPCReply<bool>> UpdateTransactionStatusJob();
        Task<GRPCReply<List<AnchorMISListResponseDC>>> GetAnchorMISList(AnchorMISRequestDC obj);
        Task<GRPCReply<List<NbfcMisListResponseDc>>> GetNbfcMISList(NbfcMisListRequestDc obj);
        Task<LoanAccountDashboardResponse> ScaleupLoanAccountDashboardDetails(DashboardLoanAccountDetailDc req);
        Task<GRPCReply<long>> PostArthMateDataLeadToLoan(ArthMateLoanDataResDc req);
        Task<GRPCReply<List<LoanAccountListResponseDc>>> GetBusinessLoanAccountList(LoanAccountListRequestDc request);

        Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request);
        Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request);
        Task<GRPCReply<List<CompanyInvoicesChargesResponseDc>>> GetCompanyInvoiceCharges(CompanyInvoiceChargesRequestDc request);
        Task<GRPCReply<List<CompanyInvoicesListResponseDc>>> GetCompanyInvoiceList(CompanyInvoiceRequestDc request);
        Task<GRPCReply<List<CompanyInvoiceDetailsResponseDc>>> GetCompanyInvoiceDetails(CompanyInvoiceDetailsRequestDC request);
        Task<GRPCReply<UpdateCompanyInvoiceReplyDC>> UpdateCompanyInvoiceDetails(GRPCRequest<UpdateCompanyInvoiceRequestDC> request);
        Task<GRPCReply<bool>> UpdateCompnayInvoicePdfPath(GRPCRequest<UpdateCompanyInvoicePDFDC> request);

        Task<GRPCReply<List<LoanAccountDisbursementResponse>>> GetLoanAccountDisbursement(GRPCRequest<List<long>> request);
        Task<GRPCReply<DSADashboardLoanResponse>> GetDSALoanDashboardData(GRPCRequest<DSALoanDashboardDataRequest> request);
        Task<GRPCReply<List<GetSalesAgentLoanDisbursmentDc>>> GetSalesAgentLoanDisbursment();

        Task<GRPCReply<long>> AddDSALoanAccount(GRPCRequest<AddDSALoanAccountRequest> request);
        Task<GRPCReply<List<EmailAnchorMISDataJobResponse>>> GetEmailAnchorMISDataJob();
        Task<GRPCReply<bool>> SaveSalesAgentLoanDisbursmentData(GRPCRequest<SalesAgentDisbursmentDataDc> request);
        Task<GRPCReply<DSADashboardPayoutResponse>> GetDSALoanPayoutList(GRPCRequest<DSALoanDashboardDataRequest> request);
        Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request);
        Task<GRPCReply<List<GetInvoiceRegisterDataResponse>>> GetInvoiceRegisterData(GRPCRequest<GetInvoiceRegisterDataRequest> request);

        Task<GRPCReply<string>> PostNBFCDisbursement(GRPCRequest<LeadDetailForDisbursement> request);
        Task<GRPCReply<bool>> AddInvoiceSettlementData(GRPCRequest<SettleCompanyInvoiceTransactionsRequest> request);
        Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetSettlePaymentJobData();
        Task<GRPCReply<bool>> SettlePaymentLaterJob(GRPCRequest<SettlePaymentJobRequest> request);
        Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetScaleUpShareCompanyData();
        Task<GRPCReply<bool>> InsertScaleUpShareTransactions(GRPCRequest<SettlePaymentJobRequest> request);
        Task<GRPCReply<List<LoanAccountListResponseDc>>> GetNBFCBusinessLoanAccountList(LoanAccountListRequestDc request);
        Task<DisbursementDashboardDataResponseDc> GetHopTrendDashboardData(HopDashboardRequestDc req);
        Task<HopDisbursementDashboardResponseDc> GetHopDisbursementDashboard(HopDashboardRequestDc req);
        Task<GRPCReply<List<long>>> GetLoanAccountData();
        Task<GRPCReply<List<DSAMISLoanResponseDC>>> GetDSAMISLoanData(DSAMISRequestDC request);
        Task<GRPCReply<LoanPaymentsResultDC>> GetLedger_RetailerStatement(GetLedger_RetailerStatementDC request);
        Task<GRPCReply<bool>> InsertBLEmiTransactionsJob(GRPCReply<List<BLNBFCConfigs>> request);
        Task<GRPCReply<bool>> UploadBLInvoiceExcel(UploadBLInvoiceExcelReq request);
        Task<GRPCReply<BLDetailsDc>> GetBLDetails(GRPCRequest<long> req);
        Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> request);
        Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> request);
        Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> request);
        Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> request);
        Task<GRPCReply<LeadProductDc>> GetNBFCLoanAccountDetailByTxnId(GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO> request);
        Task<GRPCReply<string>> RefundTransaction(GRPCRequest<RefundRequestDTO> req);

        Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request);
        Task<GRPCReply<long>> SaveLoanAccountDetails(GRPCRequest<SaveLoanAccountRequestDC> req);
        Task<GRPCReply<NBFCLoanAccountDetailResponseDTO>> GetNBFCLoanAccountDetail(GRPCRequest<GetNBFCLoanAccountDetailDTO> req);
    }

}
