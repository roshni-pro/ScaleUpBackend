using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.ApiGateways.Aggregator.Managers.NBFC;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using Serilog;
using System.Text;
using System.Text.Json;

namespace ScaleUP.ApiGateways.Aggregator.Controllers.NBFCWebhook
{
    [Route("[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {

        private readonly ILogger<WebhookController> _logger;
        private BlackSoilManager _blacksoilmanager;
        private ArthMateManager _ArthMateManager;
        private DSAManager _dsaManager;

        public WebhookController(BlackSoilManager blacksoilmanager, ArthMateManager ArthMateManager, ILogger<WebhookController> logger, DSAManager dsaManager)
        {
            _blacksoilmanager = blacksoilmanager;
            _logger = logger;
            _ArthMateManager = ArthMateManager;
            _dsaManager = dsaManager;
        }

        [HttpPost]
        [Route("Blacksoil/Callback")]
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> BlacksoilCallback([FromBody] JsonElement json)
        {
            _logger.LogInformation("Black Soil Callback Started on : {Date}", DateTime.Now);
            string requestbody = json.ToString();
            _logger.LogInformation("Black Soil Callback requestbody : {requestbody}", requestbody);
            return await _blacksoilmanager.BlacksoilCallback(requestbody);

        }

        [HttpPost]
        [Route("AScore/callback")]
        [AllowAnonymous]
        public async Task<bool> AScoreWebhookResponse([FromBody] JsonElement json)
        {
            string requestbody = json.ToString();
            _logger.LogInformation("ArthMate Ascore Callback Started on : {Date}", DateTime.Now);
            _logger.LogInformation("ArthMate Ascore Callback request : {requestbody}", requestbody);
            return await _ArthMateManager.ArthMateAscoreCallback(requestbody);

        }

        [HttpPost]
        [Route("CompositeDisbursement/callback")]
        [AllowAnonymous]
        public async Task<bool> CompositeDisbursement([FromBody] JsonElement json)
        {
            string requestbody = json.ToString();
            _logger.LogInformation("ArthMate CompositeDisbursement Callback Started on : {Date}", DateTime.Now);
            _logger.LogInformation("ArthMate CompositeDisbursement Callback request : {requestbody}", requestbody);
            return await _ArthMateManager.CompositeDisbursement(requestbody);
        }

        [HttpPost]
        [Route("eSign/callback")]
        [AllowAnonymous]
        public async Task<bool> eSignWebhookResponse([FromBody] JsonElement json)
        {
            string responsebody = json.ToString();
            _logger.LogInformation("eSign Callback Started on : {Date}", DateTime.Now);
            _logger.LogInformation("eSign Callback request : {requestbody}", responsebody);
            return await _dsaManager.eSginCallback(responsebody);

        }


    }
}
