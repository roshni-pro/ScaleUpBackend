using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.LoanAccountAPI.Managers;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.EventBus.Messages.WebHook;
using ScaleUP.Services.LoanAccountDTO.Loan;
using ScaleUP.Services.LoanAccountDTO;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.Services.LoanAccountModels.Transaction;

namespace ScaleUP.Services.LoanAccountAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountTransactionController : ControllerBase
    {
        private readonly LoanAccountApplicationDbContext _context;
        private readonly PostDisbursementManager _postDisbursementManager;
        private readonly OrderPlacementManager _orderPlacementManager;
        private readonly IConfiguration _configuration;
        private readonly TransactionSettlementManager _transactionSettlementManager;


        public AccountTransactionController(LoanAccountApplicationDbContext context
            , PostDisbursementManager postDisbursementManager
            , OrderPlacementManager orderPlacementManager
            , TransactionSettlementManager transactionSettlementManager)
        {
            _context = context;
            _postDisbursementManager = postDisbursementManager;
            _orderPlacementManager = orderPlacementManager;
            _transactionSettlementManager = transactionSettlementManager;

        }


        [AllowAnonymous]
        [HttpPost("PostAccountDisbursement")]
        public async Task<GRPCReply<long>> PostAccountDisbursement(GRPCRequest<ACT_PostAccountDisbursementRequestDC> request)
        {
            return await _postDisbursementManager.PostAccountDisbursement(request);
        }
        [AllowAnonymous]
        [HttpPost("PostAccountDisbursementPF")]
        public async Task<long> PostAccountDisbursementPF(long DisbursementAccountTransactionId, string transactionStatus, BlackSoilPfCollectionDc request)
        {
            return await _postDisbursementManager.PostDisbursementPF(DisbursementAccountTransactionId, transactionStatus, request);
        }

        [AllowAnonymous]
        [HttpPost("PostOrderPlacement")]
        public async Task<GRPCReply<string>> PostOrderPlacement(GRPCRequest<OrderPlacementRequestDC> request)
        {
            return await _orderPlacementManager.PostOrderPlacement(request);
        }


        [AllowAnonymous]
        [HttpPost("ManuallyTransactionSettle")]
        public async Task<GRPCReply<long>> ManuallyTransactionSettle(GRPCRequest<TransactionSettlementRequestDC> request)
        {
            return await _transactionSettlementManager.ManuallyTransactionSettle(request);
        }

        //[HttpPost]
        //[Route("UpdateInvoiceInformation")]
        //public async Task<InvoiceResponseDC> UpdateInvoiceInformation(InvoiceRequestDTO request)
        //{
        //    return await _orderPlacementManager.UpdateInvoiceInformation(request);
        //}        


        [AllowAnonymous]
        [HttpPost("CalculateOverdueInterestAmount_Testing")]
        public async Task<string> CalculateOverdueInterestAmount(string InvoiceNo, DateTime PaymentDate)
        {
            return await _transactionSettlementManager.CalculateOverdueInterestAmount_Testing(InvoiceNo, PaymentDate);
        }


        [AllowAnonymous]
        [HttpPost("Re_CalculateIntrestAmount_Testing")]
        public async Task<string> Re_CalculateIntrestAmount_Testing(string NBFCIdentificationCode, long ParentAccountTransactionId, DateTime PaymentDate, string PayableBy)
        {
            return await _transactionSettlementManager.Re_CalculateIntrestAmount(NBFCIdentificationCode, ParentAccountTransactionId, PaymentDate, PayableBy);
        }

        [AllowAnonymous]
        [HttpPost("RemovePaymentEntry_Utility")]
        public async Task<string> RemovePaymentEntry_Utility(long loanAcoountId, long LoanAccountRepaymentID, DateTime? paymentDATE)
        {
            return await _transactionSettlementManager.RemovePaymentEntry_Utility(loanAcoountId, LoanAccountRepaymentID, paymentDATE);
        }


        [AllowAnonymous]
        [HttpPost("RemoveAndPaidALLPaymentEntry_Utility")]
        public async Task<string> RemoveAndPaidALLPaymentEntry_Utility(long loanAcoountId, DateTime? paymentDATE)
        {
            return await _transactionSettlementManager.RemoveAndPaidALLPaymentEntry_Utility(loanAcoountId,  paymentDATE);
        }


        [AllowAnonymous]
        [HttpPost("UpdatePaidStatusByParentAccountTransactionId_Utility")]
        public async Task<string> UpdatePaidStatusForALL_Utility(long loanAccountId, DateTime PaymentDate, long ParentAccountTransactionId, string? InvoiceNo)
        {
            return await _transactionSettlementManager.UpdatePaidStatusByParentAccountTransactionId(loanAccountId, PaymentDate, ParentAccountTransactionId, InvoiceNo);
        }

        [AllowAnonymous]
        [HttpPost("UpdatePaidStatusForAllLoanAccountId")]
        public async Task<string> UpdatePaidStatusForAllLoanAccountId(long loanAccountId)
        {
            return await _transactionSettlementManager.UpdatePaidStatusForAllLoanAccountId(loanAccountId);
        }

    }
}
