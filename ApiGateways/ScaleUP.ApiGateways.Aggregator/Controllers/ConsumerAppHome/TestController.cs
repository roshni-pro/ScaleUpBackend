using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.Managers.ConsumerAppHome;
using ScaleUP.ApiGateways.AggregatorDTO.ConsumerAppHome;
using ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Common;

namespace ScaleUP.ApiGateways.Aggregator.Controllers.ConsumerAppHome
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : BaseController
    {
        private TestManager _testManager;

        public TestController(TestManager testManager)
        {
            _testManager = testManager;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveTestDetail")]
        public async Task<ResultViewModel<bool>> SaveTestDetail(TestDTO testDTO)
        {
            var SaveData = await _testManager.SaveTestDetail(testDTO, UserId);
            return SaveData;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("GetTestDetail")]
        public async Task<ResultViewModel<List<Test>>> GetTestDetail()
        {
            var GetData = await _testManager.GetTestDetail();
            return GetData;
        }
    }
}
