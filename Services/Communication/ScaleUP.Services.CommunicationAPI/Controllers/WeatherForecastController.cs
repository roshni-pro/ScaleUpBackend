using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.Global.Infrastructure.Common;

namespace ScaleUp.Services.CommunicationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : BaseController
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        //Serilog.ILogger _logger = Log.ForContext<GrpcServerInterceptor>();

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("GetForecast")]
        [AllowAnonymous]
        public string Get()
        {
            _logger.Log(LogLevel.Warning, "WeatherForecastController --> GetForecast is called....");
            return "Forecast is good!!!";
        }
    }

   

}
