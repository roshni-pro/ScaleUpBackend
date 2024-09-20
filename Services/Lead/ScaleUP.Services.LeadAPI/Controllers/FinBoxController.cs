using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LeadAPI.Helper;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.FinBox;

namespace ScaleUP.Services.LeadAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class FinBoxController : BaseController
    {
        private readonly LeadApplicationDbContext _context;
        //private IHostEnvironment _hostingEnvironment;
        private FinBoxManager _FinBoxManager;

        public FinBoxController(LeadApplicationDbContext context, FinBoxManager finBoxManager)
        {
            _context = context;
            //_hostingEnvironment = hostingEnvironment;
            _FinBoxManager = finBoxManager;
        }

        [HttpGet]
        [Route("CreateSessionAsync")]
        [AllowAnonymous]
        public async Task<string> CreateSessionAsync(string linkid, long leadid)
        {

            var res = await _FinBoxManager.CreateSessionAsync(linkid, leadid);
            return res;
        }
        [HttpGet]
        [Route("UploadSessionAsync")]
        [AllowAnonymous]
        public async Task<string> UploadSessionAsync(string linkid, long leadid)
        {
            var res = await _FinBoxManager.UploadSessionAsync(linkid,leadid);
            return res;
        }

        [HttpGet]
        [Route("SessionUploadStatus")]
        [AllowAnonymous]
        public async Task<SessionUploadStatusResponse> SessionUploadStatus(string session_id, long LeadId)
        {
            //SessionUploadStatusResponse res = new SessionUploadStatusResponse();
           var res = await _FinBoxManager.SessionUploadStatus(session_id, LeadId);
            return res;
        }

        [HttpGet]
        [Route("InitiateProcessing")]
        [AllowAnonymous]
        public async Task<InitiateProcessingResponse> InitiateProcessing(string session_id, long LeadId)
        {
           // InitiateProcessingResponse responseDc = new InitiateProcessingResponse();
           var responseDc = await _FinBoxManager.InitiateProcessing(session_id, LeadId);
            return responseDc;
        }

        [HttpGet]
        [Route("ProcessingStatus")]
        [AllowAnonymous]
        public async Task<ProcessingStatusResponse> ProcessingStatus(string session_id, long LeadId)
        {
            //ProcessingStatusResponse responseDc = new ProcessingStatusResponse();
           var responseDc = await _FinBoxManager.ProcessingStatus(session_id, LeadId);
            return responseDc;
        }

        [HttpGet]
        [Route("SessionStatus")]
        [AllowAnonymous]
        public async Task<SessionStatusResponse> SessionStatus(string session_id, long LeadId)
        {
            //SessionStatusResponse responseDc = new SessionStatusResponse();
            var responseDc = await _FinBoxManager.SessionStatus(session_id, LeadId);
            return responseDc;
        }

        [HttpGet]
        [Route("Insights")]
        [AllowAnonymous]
        public async Task<InsightsResponse> Insights(string session_id, long LeadId)
        {
            var res = await _FinBoxManager.Insights(session_id, LeadId);
            return res;
        }
    }
}
