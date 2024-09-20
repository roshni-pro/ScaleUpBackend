using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.ApiGateways.Aggregator.Managers.ConsumerAppHome;
using ScaleUP.ApiGateways.AggregatorDTO.ConsumerAppHome;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Common;
using System.ServiceModel.Channels;

namespace ScaleUP.ApiGateways.Aggregator.Controllers.ConsumerAppHome
{
    [Route("[controller]")]
    [ApiController]
    public class ConsumerAppHomeController : BaseController
    {
        private ConsumerAppHomeManager _consumerAppHomeManager ;
        public ConsumerAppHomeController(ConsumerAppHomeManager consumerAppHomeManager)
        {
            _consumerAppHomeManager = consumerAppHomeManager;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAppHomeFunctionList")]
        public async Task<ResultViewModel<List<AppHomeFunctionListDc>>> GetAppHomeFunctionList()
        {
            var GetData = await _consumerAppHomeManager.GetAppHomeFunctionList();
            return GetData;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveAppHome")]
        public async Task<ResultViewModel<bool>> SaveAppHome(List<AppHomeInput> input)
        {
            var SaveData = await _consumerAppHomeManager.SaveAppHome(input, UserId);
            return SaveData;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getAllAppHomeList")]
        public async Task<ResultViewModel<List<AppHomeReturnList>>> getAllAppHomeList()
        {
            ResultViewModel<List<AppHomeReturnList>> resultViewModel = new ResultViewModel<List<AppHomeReturnList>> { IsSuccess = false, Message="Failed to fetch Data" };

            resultViewModel = await _consumerAppHomeManager.getAllHomeList();
            return resultViewModel;
        }



        // nikhil new work
        
        // save/edit app home
        [AllowAnonymous]
        [HttpPost]
        [Route("SaveAppHomeV2")]
        public async Task<ResultViewModel<bool>> SaveAppHomeV2(AppHomeDC input)
        {
            var SaveData = await _consumerAppHomeManager.SaveAppHomeV2(input, UserId);
            return SaveData;
        }

        //get app home list
        [AllowAnonymous]
        [HttpGet]
        [Route("GetAppHomeList")]
        public async Task<ResultViewModel<List<AppHomelist>>> GetAppHomeList()
        {
            var res = await _consumerAppHomeManager.GetAppHomeList();
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAppHomeListById")]
        public async Task<ResultViewModel<AppHomeDC>> GetAppHomeListById(long Id)
        {
            ResultViewModel<AppHomeDC> resultViewModel = new ResultViewModel<AppHomeDC> { IsSuccess = false, Message = "Failed to fetch Data" };

            resultViewModel = await _consumerAppHomeManager.GetAppHomeListById(Id);
            return resultViewModel;
        }

    }
}
