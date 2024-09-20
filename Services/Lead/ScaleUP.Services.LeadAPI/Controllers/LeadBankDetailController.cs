using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadDTO.Lead;

namespace ScaleUP.Services.LeadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeadBankDetailController : BaseController
    {
        private readonly LeadBankDetailManager _LeadBankDetailManager;

        public LeadBankDetailController(LeadBankDetailManager leadBankDetailManager)
        {
            _LeadBankDetailManager = leadBankDetailManager;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveLeadBankDetail")]
        public async Task<ResultViewModel<bool>> SaveLeadBankDetail(LeadBankInfoDTO leadBankInfoDTO)
        {
            var SaveData = await _LeadBankDetailManager.SaveLeadBankDetail(leadBankInfoDTO,UserId);
            return SaveData;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadBankDetail")]
        public async Task<ResultViewModel<List<LeadBankDetailDTO>>> GetLeadBankDetail(long LeadId)
        {
            var GetData = await _LeadBankDetailManager.GetLeadBankDetail(LeadId);
            return GetData;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadDocumentDetail")]
        public async Task<ResultViewModel<List<LeadDocumentDetailDTO>>> GetLeadDocumentDetail(long LeadId)
        {
            var GetData = await _LeadBankDetailManager.GetLeadDocumentDetail(LeadId);
            return GetData;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("GetBankDetail")]
        public async Task<ResultViewModel<LeadBankInfoDTO>> GetBankDetail(long LeadId)
        {
            var GetData = await _LeadBankDetailManager.GetBankDetail(LeadId);
            return GetData;
        }
    }
}
