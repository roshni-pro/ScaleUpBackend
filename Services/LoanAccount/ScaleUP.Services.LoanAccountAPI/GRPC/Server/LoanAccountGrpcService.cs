using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.Interfaces;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.Services.LoanAccountModels.Master;
using ScaleUP.Services.LoanAccountAPI.Managers;
using ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory;
using ScaleUP.Services.LoanAccountDTO.Loan;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.Services.LoanAccountDTO;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using DocumentFormat.OpenXml.Drawing;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;

namespace ScaleUP.Services.LoanAccountAPI.GRPC.Server
{
    public class LoanAccountGrpcService : ILoanAccountGrpcService
    {
        private readonly LoanAccountApplicationDbContext _context;
        private readonly LoanAccountManager _loanAccountGrpcManager;
        private readonly TransactionFactory _transactionFactory;
        private readonly OrderPlacementManager _orderPlacementManager;

        public LoanAccountGrpcService(LoanAccountApplicationDbContext context, LoanAccountManager loanAccountGrpcManager, TransactionFactory transactionFactory, OrderPlacementManager orderPlacementManager)
        {
            _context = context;
            _loanAccountGrpcManager = loanAccountGrpcManager;
            _transactionFactory = transactionFactory;
            _orderPlacementManager = orderPlacementManager;
        }

        //public Task<long> GetLoanAccount(long request, CallContext context = default)
        //{
        //    throw new NotImplementedException();
        //}

        public Task<GRPCReply<long>> SaveLoanAccount(GRPCRequest<SaveLoanAccountRequestDC> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.SaveLoanAccount(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<long>> AddLoanAccountCredit(GRPCRequest<LoanAccountCreditsRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.AddLoanAccountCredit(request));
            return Task.FromResult(reply);
        }


        public Task<GRPCReply<long>> SaveLoanAccountCompanyLead(GRPCRequest<SaveLoanAccountCompanyLeadRequestDC> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.SaveLoanAccountCompanyLead(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<DisbursementResponse>> GetDisbursement(GRPCRequest<long> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetDisbursement(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<LoanAccountDisbursementResponse>>> GetLoanAccountDisbursement(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetLoanAccountDisbursement(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<long>> PostTransaction(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request, CallContext context = default)
        {
            IAccountTransactionType transaction = _transactionFactory.GetAccountTransactionType(request.Request.TransactionTypeCode);

            var response = transaction.PostTransaction(request);
            return response;

        }

        //public Task<LoanAccountList> GetLoanAccountList(GRPCRequest<LoanAccountListRequest> request, CallContext context = default)
        //{
        //    var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetLoanAccountList(request));
        //    return Task.FromResult(reply);
        //}
        public Task<GRPCReply<LoanCreditLimit>> GetAvailableCreditLimitByLeadId(GRPCRequest<long> LeadMasterId, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetAvailableCreditLimitByLeadId(LeadMasterId.Request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<PaymentResponseDc>> GetByTransactionReqNo(GRPCRequest<GetPaymentReqByTxnNo> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _orderPlacementManager.GetByTransactionReqNo(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<double>> GetTransactionInterestRate(GRPCRequest<GetPaymentReqByTxnNo> transNoRequest, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _orderPlacementManager.GetTransactionInterestRate(transNoRequest));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> PostOrderPlacement(GRPCRequest<OrderPlacementRequestDC> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _orderPlacementManager.PostOrderPlacement(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<OrderInitiateResponse>> OrderInitiate(OrderPlacementWithOtp request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _orderPlacementManager.OrderInitiate(request));
            return Task.FromResult(res);
        }

        public Task<GRPCReply<LeadProductDc>> GetLoanAccountDetailByTxnId(GRPCRequest<string> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _orderPlacementManager.GetLoanAccountDetailByTxnId(request));
            return Task.FromResult(res);
        }

        public Task<GRPCReply<LoanAccountReplyDC>> GetLoanAccountById(GRPCRequest<string> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _loanAccountGrpcManager.GetLoanAccountById(request));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.BlacksoilCallback(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetTemplateMasterAsync());
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request, CallContext context)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetTemplateById(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<long>> SaveNBFCCompanyAPIs(GRPCRequest<List<CompanyApiReply>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.SaveNBFCCompanyAPIs(request));
            return Task.FromResult(reply);
        }

        public Task<TemplateMasterResponseDc> GetLoanAccountNotificationTemplate(TemplateMasterRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetLoanAccountNotificationTemplate(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetAuditLogs(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<DisbursmentSMSDetailDC>> GetDisbursmentSMSDetail(GRPCRequest<long> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetDisbursmentSMSDetail(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> SaveLoanBankDetails(GRPCRequest<List<LeadBankDetailResponse>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.SaveLoanBankDetails(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<long>> SaveLoanAccountCompLead(GRPCRequest<SaveLoanAccountCompLeadReqDC> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.SaveLoanAccountCompLead(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<GetOverdueAccountsResponse>>> GetOverdueAccounts(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetOverdueAccounts());
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<DisbursmentSMSDetailDC>>> GetDueDisbursmentDetails(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetDueDisbursmentDetails());
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<string>> AnchorOrderCompleted(OrderCompletedRequest orderCompletedRequest, CallContext context = default)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            CommonResponse result = AsyncContext.Run(() => _orderPlacementManager.AnchorOrderCompleted(orderCompletedRequest.transactionNo, orderCompletedRequest.transStatus));
            reply.Status = result.status;
            reply.Message = result.Message;
            reply.Response = Convert.ToString(result.Result);
            return Task.FromResult(reply);

        }

        public Task<InvoiceResponseDC> UpdateInvoiceInformation(InvoiceRequestDC request, CallContext context = default)
        {
            InvoiceResponseDC reply = new InvoiceResponseDC();
            reply = AsyncContext.Run(() => _orderPlacementManager.UpdateInvoiceInformation(new InvoiceRequestDTO
            {
                InvoiceAmount = request.InvoiceAmount,
                InvoiceDate = request.InvoiceDate,
                InvoiceNo = request.InvoiceNo,
                InvoicePdfURL = request.InvoicePdfURL,
                OrderNo = request.OrderNo,
                StatusMsg = request.StatusMsg,
                ayeFinNBFCToken = request.ayeFinNBFCToken,
            }));

            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> GetTransactionMobileNo(TranMobileRequest tranMobileRequest, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _orderPlacementManager.GetTransactionMobileNo(tranMobileRequest));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> UpdateTransactionStatusJob(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.UpdateTransactionStatusJob());
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<AnchorMISListResponseDC>>> GetAnchorMISList(AnchorMISRequestDC obj, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetAnchorMISList(obj));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<NbfcMisListResponseDc>>> GetNbfcMISList(NbfcMisListRequestDc obj, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetNbfcMISList(obj));
            return Task.FromResult(reply);
        }
        public Task<LoanAccountDashboardResponse> ScaleupLoanAccountDashboardDetails(DashboardLoanAccountDetailDc req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.ScaleupLoanAccountDashboardDetails(req));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<long>> PostArthMateDataLeadToLoan(ArthMateLoanDataResDc req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.PostArthMateDataLeadToLoan(req));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<LoanAccountListResponseDc>>> GetBusinessLoanAccountList(LoanAccountListRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetBusinessLoanAccountList(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.AddUpdateAnchorProductConfig(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.AddUpdateNBFCProductConfig(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<CompanyInvoicesChargesResponseDc>>> GetCompanyInvoiceCharges(CompanyInvoiceChargesRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetCompanyInvoiceCharges(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<CompanyInvoicesListResponseDc>>> GetCompanyInvoiceList(CompanyInvoiceRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetCompanyInvoiceList(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<CompanyInvoiceDetailsResponseDc>>> GetCompanyInvoiceDetails(CompanyInvoiceDetailsRequestDC request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetCompanyInvoiceDetails(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<UpdateCompanyInvoiceReplyDC>> UpdateCompanyInvoiceDetails(GRPCRequest<UpdateCompanyInvoiceRequestDC> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.UpdateCompanyInvoiceDetails(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> UpdateCompnayInvoicePdfPath(GRPCRequest<UpdateCompanyInvoicePDFDC> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.UpdateCompnayInvoicePdfPath(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<DSADashboardLoanResponse>> GetDSALoanDashboardData(GRPCRequest<DSALoanDashboardDataRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetDSALoanDashboardData(request));
            return Task.FromResult(reply);
        }



        public Task<GRPCReply<List<GetSalesAgentLoanDisbursmentDc>>> GetSalesAgentLoanDisbursment(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetSalesAgentLoanDisbursment());
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<long>> AddDSALoanAccount(GRPCRequest<AddDSALoanAccountRequest> request)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.AddDSALoanAccount(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<EmailAnchorMISDataJobResponse>>> GetEmailAnchorMISDataJob(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetEmailAnchorMISDataJob());
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<bool>> SaveSalesAgentLoanDisbursmentData(GRPCRequest<SalesAgentDisbursmentDataDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.SaveSalesAgentLoanDisbursmentData(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<DSADashboardPayoutResponse>> GetDSALoanPayoutList(GRPCRequest<DSALoanDashboardDataRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetDSALoanPayoutList(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.BLoanEMIPdf(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<GetInvoiceRegisterDataResponse>>> GetInvoiceRegisterData(GRPCRequest<GetInvoiceRegisterDataRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetInvoiceRegisterData(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<string>> PostNBFCDisbursement(GRPCRequest<LeadDetailForDisbursement> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.PostNBFCDisbursement(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> AddInvoiceSettlementData(GRPCRequest<SettleCompanyInvoiceTransactionsRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.AddInvoiceSettlementData(request));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetSettlePaymentJobData(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetSettlePaymentJobData());
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<bool>> SettlePaymentLaterJob(GRPCRequest<SettlePaymentJobRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.SettlePaymentLaterJob(request));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetScaleUpShareCompanyData(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetScaleUpShareCompanyData());
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<bool>> InsertScaleUpShareTransactions(GRPCRequest<SettlePaymentJobRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.InsertScaleUpShareTransactions(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<LoanAccountListResponseDc>>> GetNBFCBusinessLoanAccountList(LoanAccountListRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetNBFCBusinessLoanAccountList(request));
            return Task.FromResult(reply);
        }
        public Task<DisbursementDashboardDataResponseDc> GetHopTrendDashboardData(HopDashboardRequestDc req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetHopTrendData(req));
            return Task.FromResult(reply);
        }
        public Task<HopDisbursementDashboardResponseDc> GetHopDisbursementDashboard(HopDashboardRequestDc req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetHopDisbursementDashboard(req));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<long>>> GetLoanAccountData(CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetLoanAccountData());
            return Task.FromResult(reply);
        }
        
        public Task<GRPCReply<List<DSAMISLoanResponseDC>>> GetDSAMISLoanData(DSAMISRequestDC request, CallContext context = default)
        {
            try
            {

            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetDSAMISLoanData(request));
            return Task.FromResult(reply);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Task<GRPCReply<LoanPaymentsResultDC>> GetLedger_RetailerStatement(GetLedger_RetailerStatementDC request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetLedger_RetailerStatement(request));
            return Task.FromResult(reply);
        }

        [AllowAnonymous]//Job
        public Task<GRPCReply<bool>> InsertBLEmiTransactionsJob(GRPCReply<List<BLNBFCConfigs>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.InsertBLEmiTransactionsJob(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> UploadBLInvoiceExcel(UploadBLInvoiceExcelReq request, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.UploadBLInvoiceExcel(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<BLDetailsDc>> GetBLDetails(GRPCRequest<long> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.GetBLDetails(req));
            return Task.FromResult(reply);
        }   
        public Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.ApplyLoan(req));
            return Task.FromResult(reply);
        }  
       
        public  Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.CheckTotalAndAvailableLimit(req));
            return Task.FromResult(reply);
        }
        public  Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.DeliveryConfirmation(req));
            return Task.FromResult(reply);
        }
        public  Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(() => _loanAccountGrpcManager.CancelTransaction(req));
            return Task.FromResult(reply);
        }
        public  Task<GRPCReply<LeadProductDc>> GetNBFCLoanAccountDetailByTxnId(GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _orderPlacementManager.GetNBFCLoanAccountDetailByTxnId(request));
            return Task.FromResult(res);
        }

        public Task<GRPCReply<string>> RefundTransaction(GRPCRequest<RefundRequestDTO> req, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _orderPlacementManager.RefundTransaction(req));
            return Task.FromResult(res);
        }

        public Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _loanAccountGrpcManager.Repayment(request));
            return Task.FromResult(res);
        }

        public Task<GRPCReply<long>> SaveLoanAccountDetails(GRPCRequest<SaveLoanAccountRequestDC> req, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _loanAccountGrpcManager.SaveLoanAccountDetails(req));
            return Task.FromResult(res);
        }
        public Task<GRPCReply<NBFCLoanAccountDetailResponseDTO>> GetNBFCLoanAccountDetail(GRPCRequest<GetNBFCLoanAccountDetailDTO> req, CallContext context = default)
        {
            var res = AsyncContext.Run(() => _loanAccountGrpcManager.GetNBFCLoanAccountDetail(req));
            return Task.FromResult(res);
        }
    }
}
