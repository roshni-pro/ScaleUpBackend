using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using Serilog;

namespace ScaleUP.Services.LeadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExperianController : ControllerBase
    {
        private readonly LeadApplicationDbContext _context;
        private readonly IMassTransitService _massTransitService;
        private readonly IRequestClient<ICheckStatus> _checkStatusEvent;
        private readonly IRequestClient<ICreateLeadActivityMessage> _createLeadactivityEvent;
        private readonly ExperianManager experianManager;
        Serilog.ILogger logger = Log.ForContext<LeadController>();

        public ExperianController(LeadApplicationDbContext context, IMassTransitService massTransitService, IRequestClient<ICheckStatus> checkStatusEvent, IRequestClient<ICreateLeadActivityMessage> createLeadactivityEvent, ExperianManager _experianManager)
        {
            _context = context;
            _massTransitService = massTransitService;
            _checkStatusEvent = checkStatusEvent;
            _createLeadactivityEvent = createLeadactivityEvent;
            experianManager = _experianManager; 
        }

        [HttpPost]
        [Route("SaveExperianState")]
        public async Task<bool> SaveExperianState(ExperianStateRequest experianStateRequest)
        {
            //ExperianManager experianManager = new ExperianManager(_context);
            var res = await experianManager.SaveExperianState(experianStateRequest);
            return res;

        }

        //[HttpGet]
        //[Route("GetExperianStateId")]
        //public async Task<ExperianStateReply> GetExperianStateId(long LocationStateId)
        //{
        //    ExperianManager experianManager = new ExperianManager(_context);
        //    var res = await experianManager.GetExperianStateId(LocationStateId);
        //    return res;

        //}
    }
}
