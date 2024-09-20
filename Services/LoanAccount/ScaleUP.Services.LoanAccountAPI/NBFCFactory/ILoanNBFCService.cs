using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate;
using ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountDTO.Transaction;

namespace ScaleUP.Services.LoanAccountAPI.NBFCFactory
{
    public interface ILoanNBFCService
    {

        Task<NBFCFactoryResponse> OrderInitiate(long invoiceId, long accountId, double transAmount);

        Task<NBFCFactoryResponse> OrderCaptured(long invoiceId, long accountId, double transAmount, bool status, string? OrderNo, string? ayeFinNBFCToken="");

        Task<bool> SaveNBFCLoanData(long accountId, string? webhookresposne, NBFCDetailDTO nBFCDetailDTO = null);

        Task<double> GetAvailableCreditLimit(long accountId);

        Task<NBFCFactoryResponse> SettlePayment(long blackSoilRepaymentId, long blackSoilLoanAccountId);

        Task<double> CalculatePerDayInterest(double interest);

        Task<List<InvoiceNBFCReqRes>> GetInvoiceNBFCReqRes(List<long> ApiDetailIds);

        Task<NBFCFactoryResponse> SettlePaymentLater(GRPCRequest<SettlePaymentJobRequest> request);
        Task<bool> IsInvoiceInitiated(long invoiceId);
        Task<NBFCFactoryResponse> InsertSkippedPayments(long loanAccountId);
        Task<ResultViewModel<List<LoanRepaymentScheduleDetailDc>>> LoanRepaymentScheduleDetails(long loanAccountId);
        Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid);
        Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request);

        Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> request);
        Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> request);
        Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> request);
        Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> request);
        Task<NBFCFactoryResponse> AyeSCFCOrderInitiate(GRPCRequest<AyeSCFCOrderInitiateDTO> req);
        Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request);
    }
}
