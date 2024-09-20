using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate;
using ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountDTO.Transaction;

namespace ScaleUP.Services.LoanAccountAPI.NBFCFactory.Implementation
{
    public class DefaultLoanNBFCService : BaseNBFCService, ILoanNBFCService
    {

        private readonly LoanAccountApplicationDbContext _context;

        public DefaultLoanNBFCService(LoanAccountApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<NBFCFactoryResponse> OrderInitiate(long invoiceId, long accountId, double transAmount)
        {
            return new NBFCFactoryResponse
            {
                IsSuccess = true,
                Message = "Success"
            };
        }
       
        public async Task<NBFCFactoryResponse> OrderCaptured(long invoiceId, long accountId, double transAmount, bool status, string? OrderNo, string? ayeFinNBFCToken = "")
        {
            return new NBFCFactoryResponse
            {
                IsSuccess = status,
                Message = status ? "Success" : "Fail"
            };
        }

        public async Task<bool> SaveNBFCLoanData(long accountId, string? webhookresposne, NBFCDetailDTO NBFCDetailDTO = null)
        {
            return true;
        }

        public async Task<double> GetAvailableCreditLimit(long accountId)
        {
            var loanAccount = await _context.LoanAccountCredits.FirstOrDefaultAsync(x => x.LoanAccountId == accountId);
            return loanAccount != null ? loanAccount.CreditLimitAmount : 0;
        }

        public Task<NBFCFactoryResponse> SettlePayment(long blackSoilRepaymentId, long blackSoilLoanAccountId)
        {
            throw new NotImplementedException();
        }

        public async Task<double> CalculatePerDayInterest(double interest)
        {
            return (interest / 360);
        }

        public async Task<List<InvoiceNBFCReqRes>> GetInvoiceNBFCReqRes(List<long> ApiDetailIds)
        {
            return new List<InvoiceNBFCReqRes>();
        }

        public Task<NBFCFactoryResponse> SettlePaymentLater(GRPCRequest<SettlePaymentJobRequest> request)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsInvoiceInitiated(long invoiceId)
        {
            return false;
        }

        public Task<NBFCFactoryResponse> InsertSkippedPayments(long loanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<List<LoanRepaymentScheduleDetailDc>>> LoanRepaymentScheduleDetails(long loanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long loanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> request)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> AyeSCFCOrderInitiate(GRPCRequest<AyeSCFCOrderInitiateDTO> req)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request)
        {
            throw new NotImplementedException();
        }
    }
}
