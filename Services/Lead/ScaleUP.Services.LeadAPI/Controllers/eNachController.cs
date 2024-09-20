using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Services.LeadAPI.Helper;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using Serilog;
using System.Security.Claims;

namespace ScaleUP.Services.LeadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class eNachController : ControllerBase
    {
        private readonly LeadApplicationDbContext _context;
        private readonly eNachManager _enachManager;
        private IHostEnvironment _hostingEnvironment;
        Serilog.ILogger logger = Log.ForContext<LeadController>();

        public eNachController(LeadApplicationDbContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _enachManager = new eNachManager(context, hostingEnvironment);
            _hostingEnvironment = hostingEnvironment;   
        }


        [Route("BankList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<eNachBankListResponseDTO>> getBankListAsync()
        {
            var res = await _enachManager.getBankListAsync();
            return Ok(res);
        }


        //[Route("eNachAddResponse")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<bool> eNachAddResponse(eNachRespDocDC eMandateResponseDC)
        //{
        //    try
        //    {
        //        int UserId = 0;

        //        var res = await _enachManager.eNachAddeResponseDetails(eMandateResponseDC, UserId.ToString());

        //        return res;
        //    }
        //    catch (Exception ex)
        //    {
        //        //TextFileLogHelper.TraceLog("eMandate addResponse Exception ex  :" + ex.ToString());

        //        return false;
        //    }
        //}


    }
}
