using Grpc.Core;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Consumers;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadDTO.Lead;
using System.Text.Json;

namespace ScaleUP.Services.LeadAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly LeadManager _leadManager;
        private readonly ILogger<WebhookController> _logger;
        public WebhookController(LeadManager leadManager, ILogger<WebhookController> logger)
        {
            _logger = logger;
            _leadManager = leadManager;
        }

        //[HttpPost]
        //[Route("Blacksoil/Callback")]
        //[AllowAnonymous]
        //public async Task<bool> BlacksoilCallback([FromBody] JsonElement json)
        //{

        //    _logger.LogInformation("Black Soil Callback Started on : {Date}", DateTime.Now);
        //    //// var content = request.Content;
        //    // //string jsonContent = await content.ReadAsStringAsync();
        //    string requestbody = json.ToString();

        //    _logger.LogInformation("Black Soil Callback request body : {requestbody}", requestbody);

        //    await _leadManager.WebhookResponse(requestbody);
        //    _logger.LogInformation("Black Soil Callback End on : {EndDate}", DateTime.Now);

        //    return true;
        //}

    }
}
