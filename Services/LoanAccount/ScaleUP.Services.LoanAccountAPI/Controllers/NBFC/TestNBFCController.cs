using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.LoanAccountAPI.Managers;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using System.Text.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using Microsoft.Identity.Client;

namespace ScaleUP.Services.LoanAccountAPI.Controllers.NBFC
{
    [Route("[controller]")]
    [ApiController]
    public class TestNBFCController : ControllerBase
    {
        private readonly LoanNBFCFactory _loanNBFCFactory;
        private readonly LoanAccountManager _loanAccountManager;


        public TestNBFCController(LoanNBFCFactory loanNBFCFactory, LoanAccountManager loanAccountManager)
        {
            _loanNBFCFactory = loanNBFCFactory;
            _loanAccountManager = loanAccountManager;

        }

        [AllowAnonymous]
        [HttpGet("OrderInitiate")]
        public async Task OrderInitiate(long invoiceId, long accountId, double transAmount)
        {
            var result = _loanNBFCFactory.GetService("BlackSoil");

            var re = await result.OrderInitiate(invoiceId, accountId, transAmount);
        }

        [AllowAnonymous]
        [HttpGet("OrderCaptured")]
        public async Task<NBFCFactoryResponse> OrderCaptured(long invoiceId, long accountId, double transAmount, bool status ,string? OrderNo, string? ayeFinNBFCToken = "")
        {
            var result = _loanNBFCFactory.GetService("BlackSoil");

            var re = await result.OrderCaptured(invoiceId, accountId, transAmount, status, OrderNo, ayeFinNBFCToken);
            return re;
        }
        [AllowAnonymous]
        [HttpGet("GetAvailableCreditLimit")]
        public async Task GetAvailableCreditLimit(long accountId)
        {
            var result = _loanNBFCFactory.GetService("BlackSoil");

            var re = await result.GetAvailableCreditLimit(accountId);
        }


        [HttpPost]
        [Route("Callback")]
        [AllowAnonymous]
        public async Task<GRPCReply<long>> BlacksoilCallback([FromBody] JsonElement json)
        {

            GRPCRequest<BlackSoilWebhookRequest> request = new GRPCRequest<BlackSoilWebhookRequest>
            {
                Request = new BlackSoilWebhookRequest { EventName = "invoice_disbursal_processed", Data = json.ToString() },

            };
            return await _loanAccountManager.BlacksoilCallback(request);

        }


        [AllowAnonymous]
        [Route("SettlePayment")]
        [HttpGet]
        public async Task<NBFCFactoryResponse> SettlePayment(long blackSoilRepaymentId, long blackSoilLoanAccountId)
        {
            var result = _loanNBFCFactory.GetService("BlackSoil");

            var re = await result.SettlePayment(blackSoilRepaymentId, blackSoilLoanAccountId);
            return re;
        }

    }
}
