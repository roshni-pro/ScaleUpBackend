using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory;

namespace ScaleUP.Services.LoanAccountAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly TransactionFactory _transactionFactory;
        
        
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, TransactionFactory transactionFactory)
        {
            _logger = logger;

            _transactionFactory = transactionFactory;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

        [AllowAnonymous]
        [HttpGet(Name = "GetWeatherForecast")]
        public dynamic GetDisbursement(string transactionType)
        {
            var type = _transactionFactory.GetAccountTransactionType(transactionType);

            return type.Run(null);
        }
    }
}
