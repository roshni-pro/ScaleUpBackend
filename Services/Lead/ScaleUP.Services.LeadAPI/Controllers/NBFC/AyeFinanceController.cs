using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request;

namespace ScaleUP.Services.LeadAPI.Controllers.NBFC
{
    [Route("[controller]")]
    [ApiController]
    public class AyeFinanceController : ControllerBase
    {
        private readonly LeadManager _leadManager;
        private readonly NBFCSchedular _nBFCSchedular;
        public AyeFinanceController(LeadManager leadManager, NBFCSchedular nBFCSchedular)
        {
            _leadManager = leadManager;
            _nBFCSchedular = nBFCSchedular;
        }
        //[HttpPost]
        //[Route("GenerateAYEToken")]
        //public async Task<GRPCReply<string>> GenerateToken()
        //{
        //    var res = await _nBFCSchedular.GenerateToken();
        //    return res;
        //}

        //[HttpPost]
        //[Route("AddLead")]
        //public async Task<GRPCReply<string>> AddLead(AyeleadReq ayeleadReq)
        //{
        //    var res = await _nBFCSchedular.AddLead(ayeleadReq);
        //    return res;
        //}

        //[HttpPost]
        //[Route("CheckCreditLine")]
        //public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(AyeleadReq ayeleadReq, string token)
        //{
        //    var res = await _nBFCSchedular.CheckCreditLine(ayeleadReq, token);
        //    return res;
        //}

        //[HttpPost]
        //[Route("GetWebUrl")]
        //public async Task<GRPCReply<string>> GetWebUrl(AyeleadReq ayeleadReq)
        //{
        //    var res = await _nBFCSchedular.GetWebUrl(ayeleadReq);
        //    return res;
        //}

        //[HttpPost]
        //[Route("TransactionSendOtp")]
        //public async Task<GRPCReply<string>> TransactionSendOtp(AyeleadReq ayeleadReq)
        //{
        //    var res = await _nBFCSchedular.TransactionSendOtp(ayeleadReq);
        //    return res;
        //}

    }
}
