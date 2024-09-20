using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.Interfaces;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using Grpc.Core;
using Grpc.Net.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ProtoBuf.Grpc.Client;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using MassTransit;


namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class LoanAccountService : ILoanAccountService
    {
        private IConfiguration Configuration;
        private readonly ILoanAccountGrpcService _client;

        public LoanAccountService(IConfiguration _configuration
            , ILoanAccountGrpcService client
            )
        {
            Configuration = _configuration;
            _client = client;
        }


        public async Task<GRPCReply<long>> SaveLoanAccount(GRPCRequest<SaveLoanAccountRequestDC> request)
        {
            GRPCReply<long> res = new GRPCReply<long>();
            var result = await _client.SaveLoanAccount(request);
            return result;
        }
        public async Task<GRPCReply<long>> AddLoanAccountCredit(GRPCRequest<LoanAccountCreditsRequest> request)
        {
            GRPCReply<long> res = new GRPCReply<long>();
            var result = await _client.AddLoanAccountCredit(request);
            return result;
        }
        public async Task<GRPCReply<long>> SaveLoanAccountCompanyLead(GRPCRequest<SaveLoanAccountCompanyLeadRequestDC> request)
        {
            GRPCReply<long> res = new GRPCReply<long>();
            var result = await _client.SaveLoanAccountCompanyLead(request);
            return result;
        }

        public async Task<GRPCReply<DisbursementResponse>> GetDisbursement(GRPCRequest<long> request)
        {
            var result = await _client.GetDisbursement(request);
            return result;
        }
        public async Task<GRPCReply<List<LoanAccountDisbursementResponse>>> GetLoanAccountDisbursement(GRPCRequest<List<long>> request)
        {
            var result = await _client.GetLoanAccountDisbursement(request);
            return result;
        }

        public async Task<GRPCReply<long>> PostTransaction(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request)
        {
            GRPCReply<long> res = new GRPCReply<long>();
            var result = await _client.PostTransaction(request);
            return result;
        }

        public async Task<GRPCReply<PaymentResponseDc>> GetByTransactionReqNo(GRPCRequest<GetPaymentReqByTxnNo> request)
        {
            GRPCReply<PaymentResponseDc> res = new GRPCReply<PaymentResponseDc>();
            var result = await _client.GetByTransactionReqNo(request);
            res.Response = result.Response;
            return result;
        }

        public async Task<GRPCReply<double>> GetTransactionInterestRate(GRPCRequest<GetPaymentReqByTxnNo> transNoRequest)
        {
            var result = await _client.GetTransactionInterestRate(transNoRequest);
            return result;
        }

        public async Task<GRPCReply<string>> PostOrderPlacement(GRPCRequest<OrderPlacementRequestDC> request)
        {
            return await _client.PostOrderPlacement(request);
        }
        public async Task<GRPCReply<LoanCreditLimit>> GetAvailableCreditLimitByLeadId(GRPCRequest<long> LeadMasterId)
        {
            var result = await _client.GetAvailableCreditLimitByLeadId(LeadMasterId);
            return result;
        }

        public async Task<GRPCReply<OrderInitiateResponse>> OrderInitiate(OrderPlacementWithOtp request)
        {
            var result = await _client.OrderInitiate(request);
            return result;
        }
        public async Task<GRPCReply<LeadProductDc>> GetLoanAccountDetailByTxnId(GRPCRequest<string> transNo)
        {
            var result = await _client.GetLoanAccountDetailByTxnId(transNo);
            return result;
        }

        public async Task<GRPCReply<LoanAccountReplyDC>> GetLoanAccountById(GRPCRequest<string> request)
        {
            var result = await _client.GetLoanAccountById(request);
            return result;
        }

        public async Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request)
        {
            var reply = await _client.BlacksoilCallback(request);
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

        public async Task<GRPCReply<long>> SaveNBFCCompanyAPIs(GRPCRequest<List<CompanyApiReply>> request)
        {
            var reply = await _client.SaveNBFCCompanyAPIs(request);
            return reply;
        }

        public async Task<TemplateMasterResponseDc> GetLoanAccountNotificationTemplate(TemplateMasterRequestDc request)
        {
            var reply = await _client.GetLoanAccountNotificationTemplate(request);
            return reply;
        }

        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            var reply = await _client.GetAuditLogs(request);
            return reply;
        }
        public async Task<GRPCReply<DisbursmentSMSDetailDC>> GetDisbursmentSMSDetail(GRPCRequest<long> request)
        {
            var reply = await _client.GetDisbursmentSMSDetail(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> SaveLoanBankDetails(GRPCRequest<List<LeadBankDetailResponse>> request)
        {
            var reply = await _client.SaveLoanBankDetails(request);
            return reply;
        }

        public async Task<GRPCReply<long>> SaveLoanAccountCompLead(GRPCRequest<SaveLoanAccountCompLeadReqDC> request)
        {
            var reply = await _client.SaveLoanAccountCompLead(request);
            return reply;
        }

        public async Task<GRPCReply<List<GetOverdueAccountsResponse>>> GetOverdueAccounts()
        {
            var reply = await _client.GetOverdueAccounts();
            return reply;
        }

        public async Task<GRPCReply<List<DisbursmentSMSDetailDC>>> GetDueDisbursmentDetails()
        {
            var reply = await _client.GetDueDisbursmentDetails();
            return reply;
        }

        public async Task<GRPCReply<string>> AnchorOrderCompleted(OrderCompletedRequest orderCompletedRequest, string token)
        {
            var _clientnew = ConfigureServices.GetCustomeChennle<ILoanAccountGrpcService>(token, EnvironmentConstants.LoanAccountUrl);
            var reply = await _clientnew.AnchorOrderCompleted(orderCompletedRequest);
            return reply;
        }

        public async Task<InvoiceResponseDC> UpdateInvoiceInformation(InvoiceRequestDC request, string token)
        {
            var _clientnew = ConfigureServices.GetCustomeChennle<ILoanAccountGrpcService>(token, EnvironmentConstants.LoanAccountUrl);
            var reply = await _clientnew.UpdateInvoiceInformation(request);
            return reply;
        }

        public async Task<GRPCReply<string>> GetTransactionMobileNo(TranMobileRequest tranMobileRequest)
        {
            var reply = await _client.GetTransactionMobileNo(tranMobileRequest);
            return reply;

        }

        public async Task<GRPCReply<bool>> UpdateTransactionStatusJob()
        {
            var reply = await _client.UpdateTransactionStatusJob();
            return reply;
        }

        public async Task<GRPCReply<List<AnchorMISListResponseDC>>> GetAnchorMISList(AnchorMISRequestDC obj)
        {
            var reply = await _client.GetAnchorMISList(obj);
            return reply;
        }
        public async Task<GRPCReply<List<NbfcMisListResponseDc>>> GetNbfcMISList(NbfcMisListRequestDc obj)
        {
            var reply = await _client.GetNbfcMISList(obj);
            return reply;
        }
        public async Task<LoanAccountDashboardResponse> ScaleupLoanAccountDashboardDetails(DashboardLoanAccountDetailDc req)
        {
            var reply = await _client.ScaleupLoanAccountDashboardDetails(req);
            return reply;
        }
        public async Task<GRPCReply<long>> PostArthMateDataLeadToLoan(ArthMateLoanDataResDc req)
        {
            var reply = await _client.PostArthMateDataLeadToLoan(req);
            return reply;
        }
        public async Task<GRPCReply<List<LoanAccountListResponseDc>>> GetBusinessLoanAccountList(LoanAccountListRequestDc request)
        {
            var reply = await _client.GetBusinessLoanAccountList(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request)
        {
            var reply = await _client.AddUpdateAnchorProductConfig(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request)
        {
            var reply = await _client.AddUpdateNBFCProductConfig(request);
            return reply;
        }
        public async Task<GRPCReply<List<CompanyInvoicesChargesResponseDc>>> GetCompanyInvoiceCharges(CompanyInvoiceChargesRequestDc request)
        {
            var reply = await _client.GetCompanyInvoiceCharges(request);
            return reply;
        }
        public async Task<GRPCReply<List<CompanyInvoicesListResponseDc>>> GetCompanyInvoiceList(CompanyInvoiceRequestDc request)
        {
            var reply = await _client.GetCompanyInvoiceList(request);
            return reply;
        }
        public async Task<GRPCReply<List<CompanyInvoiceDetailsResponseDc>>> GetCompanyInvoiceDetails(CompanyInvoiceDetailsRequestDC request)
        {
            var reply = await _client.GetCompanyInvoiceDetails(request);
            return reply;
        }
        public async Task<GRPCReply<UpdateCompanyInvoiceReplyDC>> UpdateCompanyInvoiceDetails(GRPCRequest<UpdateCompanyInvoiceRequestDC> request)
        {
            var reply = await _client.UpdateCompanyInvoiceDetails(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> UpdateCompnayInvoicePdfPath(GRPCRequest<UpdateCompanyInvoicePDFDC> request)
        {
            var reply = await _client.UpdateCompnayInvoicePdfPath(request);
            return reply;
        }

        public async Task<GRPCReply<DSADashboardLoanResponse>> GetDSALoanDashboardData(GRPCRequest<DSALoanDashboardDataRequest> request)
        {
            var reply = await _client.GetDSALoanDashboardData(request);
            return reply;
        }

        public async Task<GRPCReply<List<GetSalesAgentLoanDisbursmentDc>>> GetSalesAgentLoanDisbursment()
        {
            var reply = await _client.GetSalesAgentLoanDisbursment();
            return reply;
        }



        public async Task<GRPCReply<long>> AddDSALoanAccount(GRPCRequest<AddDSALoanAccountRequest> request)
        {
            var reply = await _client.AddDSALoanAccount(request);
            return reply;
        }

        public async Task<GRPCReply<List<EmailAnchorMISDataJobResponse>>> GetEmailAnchorMISDataJob()
        {
            var reply = await _client.GetEmailAnchorMISDataJob();
            return reply;
        }

        public async Task<GRPCReply<bool>> SaveSalesAgentLoanDisbursmentData(GRPCRequest<SalesAgentDisbursmentDataDc> request)
        {
            var reply = await _client.SaveSalesAgentLoanDisbursmentData(request);
            return reply;
        }

        public async Task<GRPCReply<DSADashboardPayoutResponse>> GetDSALoanPayoutList(GRPCRequest<DSALoanDashboardDataRequest> request)
        {
            var reply = await _client.GetDSALoanPayoutList(request);
            return reply;
        }
        public async Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request)
        {
            var reply = await _client.BLoanEMIPdf(request);
            return reply;
        }

        public async Task<GRPCReply<List<GetInvoiceRegisterDataResponse>>> GetInvoiceRegisterData(GRPCRequest<GetInvoiceRegisterDataRequest> request)
        {
            var reply = await _client.GetInvoiceRegisterData(request);
            return reply;
        }

        public async Task<GRPCReply<string>> PostNBFCDisbursement(GRPCRequest<LeadDetailForDisbursement> request)
        {
            var reply = await _client.PostNBFCDisbursement(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> AddInvoiceSettlementData(GRPCRequest<SettleCompanyInvoiceTransactionsRequest> request)
        {
            var reply = await _client.AddInvoiceSettlementData(request);
            return reply;
        }

        public async Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetSettlePaymentJobData()
        {
            var reply = await _client.GetSettlePaymentJobData();
            return reply;
        }

        public async Task<GRPCReply<bool>> SettlePaymentLaterJob(GRPCRequest<SettlePaymentJobRequest> request)
        {
            var reply = await _client.SettlePaymentLaterJob(request);
            return reply;
        }

        public async Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetScaleUpShareCompanyData()
        {
            var reply = await _client.GetScaleUpShareCompanyData();
            return reply;
        }

        public async Task<GRPCReply<bool>> InsertScaleUpShareTransactions(GRPCRequest<SettlePaymentJobRequest> request)
        {
            var reply = await _client.InsertScaleUpShareTransactions(request);
            return reply;
        }

        public async Task<GRPCReply<List<LoanAccountListResponseDc>>> GetNBFCBusinessLoanAccountList(LoanAccountListRequestDc request)
        {
            var reply = await _client.GetNBFCBusinessLoanAccountList(request);
            return reply;
        }
        public async Task<DisbursementDashboardDataResponseDc> GetHopTrendDashboardData(HopDashboardRequestDc req)
        {
            var reply = await _client.GetHopTrendDashboardData(req);
            return reply;
        }
        public async Task<HopDisbursementDashboardResponseDc> GetHopDisbursementDashboard(HopDashboardRequestDc req)
        {
            var reply = await _client.GetHopDisbursementDashboard(req);
            return reply;
        }
        public async Task<GRPCReply<List<long>>> GetLoanAccountData()
        {
            var reply = await _client.GetLoanAccountData();
            return reply;
        }

        public async Task<GRPCReply<List<DSAMISLoanResponseDC>>> GetDSAMISLoanData(DSAMISRequestDC request)
        {
            var reply = await _client.GetDSAMISLoanData(request);
            return reply;
        }

        public async Task<GRPCReply<LoanPaymentsResultDC>> GetLedger_RetailerStatement(GetLedger_RetailerStatementDC request)
        {
            var reply = await _client.GetLedger_RetailerStatement(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> InsertBLEmiTransactionsJob(GRPCReply<List<BLNBFCConfigs>> request)
        {
            var reply = await _client.InsertBLEmiTransactionsJob(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> UploadBLInvoiceExcel(UploadBLInvoiceExcelReq request)
        {
            var reply = await _client.UploadBLInvoiceExcel(request);
            return reply;
        }

        public async Task<GRPCReply<BLDetailsDc>> GetBLDetails(GRPCRequest<long> req)
        {
            var reply = await _client.GetBLDetails(req);
            return reply;
        }
        public async Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> req)
        {
            var reply = await _client.ApplyLoan(req);
            return reply;
        }
        public async Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> req)
        {
            var reply = await _client.CheckTotalAndAvailableLimit(req);
            return reply;
        }
        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> req)
        {
            var reply = await _client.DeliveryConfirmation(req);
            return reply;
        }
        public async Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> req)
        {
            var reply = await _client.CancelTransaction(req);
            return reply;
        }

        public async Task<GRPCReply<LeadProductDc>> GetNBFCLoanAccountDetailByTxnId(GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO> request)
        {
                var reply = await _client.GetNBFCLoanAccountDetailByTxnId(request);
                return reply;
        }

        public async Task<GRPCReply<string>> RefundTransaction(GRPCRequest<RefundRequestDTO> req)
        {
            var reply = await _client.RefundTransaction(req);
            return reply;
        }

        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request)
        {
            var reply = await _client.Repayment(request);
            return reply;
        }
        public async Task<GRPCReply<long>> SaveLoanAccountDetails(GRPCRequest<SaveLoanAccountRequestDC> req)
        {
            var reply = await _client.SaveLoanAccountDetails(req);
            return reply;
        }
        public async Task<GRPCReply<NBFCLoanAccountDetailResponseDTO>> GetNBFCLoanAccountDetail(GRPCRequest<GetNBFCLoanAccountDetailDTO> req)
        {
            var reply = await _client.GetNBFCLoanAccountDetail(req);
            return reply;
        }
    }
}

