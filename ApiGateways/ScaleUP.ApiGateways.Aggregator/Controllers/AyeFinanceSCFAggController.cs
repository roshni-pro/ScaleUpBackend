using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.ApiGateways.Aggregator.Managers.NBFC;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class AyeFinanceSCFAggController : ControllerBase
    {

        private IProductService _productService;
        private ILeadService _leadService;
        private IIdentityService _identityService;
        private AyeFinanceSCFManger ayeFinanceSCFManger;


        public AyeFinanceSCFAggController(
            IProductService productService,
            ILeadService leadService,
            IIdentityService identityService,
         AyeFinanceSCFManger _ayeFinanceSCFManger

            )
        {
            ayeFinanceSCFManger = _ayeFinanceSCFManger;

            _productService = productService;
            _leadService = leadService;
            _identityService = identityService;
        }
        #region AyeFinance
        [HttpPost]
        [Route("GenerateAyeToken")]

        public async Task<GRPCReply<string>> GenerateToken()
        {
            return await ayeFinanceSCFManger.GenerateAyeToken();
        }

        [HttpPost]
        [Route("AddLead")]

        public async Task<GRPCReply<string>> AddLead(AyeleadReq request)
        {
            return await ayeFinanceSCFManger.AddLead(request);
        }

        [HttpPost]
        [Route("GetWebUrl")]

        public async Task<GRPCReply<string>> GetWebUrl(AyeleadReq request)
        {
            return await ayeFinanceSCFManger.GetWebUrl(request);
        }

        [HttpPost]
        [Route("CheckCreditLine")]

        public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(AyeleadReq request)
        {
            return await ayeFinanceSCFManger.CheckCreditLine(request);
        }
        [HttpPost]
        [Route("TransactionSendOtp")]

        public async Task<GRPCReply<string>> TransactionSendOtp(AyeleadReq request)
        {
            return await ayeFinanceSCFManger.TransactionSendOtp(request);
        }
        [HttpPost]
        [Route("TransactionVerifyOtp")]

        public async Task<GRPCReply<string>> TransactionVerifyOtp(TransactionVerifyOtpReqDc request)
        {
            return await ayeFinanceSCFManger.TransactionVerifyOtp(request);
        }
        [HttpPost]
        [Route("ApplyLoan")]

        public async Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(ApplyLoanreq request)
        {
            return await ayeFinanceSCFManger.ApplyLoan(request);
        }

        [HttpPost]
        [Route("CheckTotalAndAvailableLimit")]

        public async Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(AyeloanReq request)
        {
            return await ayeFinanceSCFManger.CheckTotalAndAvailableLimit(request);
        }

        [HttpPost]
        [Route("DeliveryConfirmation")]

        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(DeliveryConfirmationreq request)
        {
            return await ayeFinanceSCFManger.DeliveryConfirmation(request);
        }

        [HttpGet]
        [Route("RefundTransaction")]
        public async Task<GRPCReply<string>> RefundTransaction(string orderNo, double refundAmount)
        {
            return await ayeFinanceSCFManger.RefundTransaction(orderNo, refundAmount);
        }

        [HttpPost]
        [Route("Repayment")]
        [AllowAnonymous]
        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(Repaymentreq request)
        {
            return await ayeFinanceSCFManger.Repayment(request);
        }
        [HttpPost]
        [Route("GetNBFCLoanAccountDetail")]
        public async Task<GRPCReply<NBFCLoanAccountDetailResponseDTO>> GetNBFCLoanAccountDetail(GetNBFCLoanAccountDetailDTO req)
        {
            return await ayeFinanceSCFManger.GetNBFCLoanAccountDetail(req);
        }

        #endregion
    }
}
