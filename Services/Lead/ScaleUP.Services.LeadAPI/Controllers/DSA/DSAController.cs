using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadDTO.Lead.DSA;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;

namespace ScaleUP.Services.LeadAPI.Controllers.DSA
{
    [Route("[controller]")]
    [ApiController]
    public class DSAController : ControllerBase
    {
        private readonly LeadManager _leadManager;
        public DSAController(LeadManager leadManager)
        {
            _leadManager = leadManager;

        }
        [Route("SaveDSAPayouts")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CommonResponseDc> SaveDSAPayouts(List<AddLeadPayOutsDTO> req)
        {
            var res = await _leadManager.SaveDSAPayouts(req);
            return res;
        }

        [Route("DSADeactivate")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CommonResponseDc> DSADeactivate(long leadId, bool isActive, bool isReject)
        {
            var res = await _leadManager.DSADeactivate(leadId, isActive, isReject);
            return res;
        }

        [HttpGet]
        [Route("GetDSAAgreement")]
        public async Task<CommonResponseDc> GetDSAAgreement(long LeadId)
        {
            return await _leadManager.GetDSAAgreement(LeadId);
        }
    }
}
