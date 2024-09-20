using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadModels;
using static MassTransit.ValidationResultExtensions;

namespace ScaleUP.Services.LeadAPI.Controllers.NBFC
{
    [Route("[controller]")]
    [ApiController]
    public class NBFCSchedularController : ControllerBase
    {
        private readonly NBFCSchedular _nBFCSchedular;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly LeadManager _leadManager;
        public NBFCSchedularController(NBFCSchedular nBFCSchedular, LeadNBFCSubActivityManager leadNBFCSubActivityManager, LeadManager leadManager)
        {
            _nBFCSchedular = nBFCSchedular;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _leadManager = leadManager;

        }
        [HttpGet]
        [Route("GetCreditLine")]
        [AllowAnonymous]
        public async Task<bool> GetCreditLine(long leadId)
        {
            await _nBFCSchedular.GenerateBlackSoilOfferForLead(leadId);
            return true;
        }


        [HttpGet]
        [Route("GenerateAgreement")]
        [AllowAnonymous]
        public async Task<bool> GenerateAgreement(long leadId)
        {
            await _nBFCSchedular.GenerateAgreement(leadId);
            return true;
        }



        [HttpGet]
        [Route("CheckEsignStatus")]
        [AllowAnonymous]
        public async Task<ResultViewModel<bool>> CheckEsignStatus(long leadId)
        {
            bool isCompleted = false;
            var result = await _leadNBFCSubActivityManager.IsGenerateAgreementCompleted(leadId);
            if (!result.Result)
            {
                await _nBFCSchedular.GenerateAgreement(leadId);
                result = await _leadNBFCSubActivityManager.IsGenerateAgreementCompleted(leadId);
                isCompleted = result.Result;
            }
            else
            {
                await _leadManager.AddLeadConsentLog(leadId, LeadTypeConsentConstants.Agrement, "");
                result = await _leadNBFCSubActivityManager.IsLeadCompleted(leadId);
            }
            return result;
        }


        [HttpGet]
        [Route("TestPan")]
        [AllowAnonymous]
        public async Task<bool> TestUpdateApi(string updatetype)
        {
            bool isCompleted = false;
            if (!string.IsNullOrEmpty(updatetype))
            {
                await _leadNBFCSubActivityManager.TestUpdateApi(updatetype);
            }
            return isCompleted;
        }


        [HttpGet]
        [Route("UpdateLeadInfo")]
        public async Task UpdateLeadInfo(long leadId, long NbfcId, string companyIdentificationCode)
        {
            await _nBFCSchedular.UpdateLeadInfo(leadId, NbfcId, companyIdentificationCode);

        }


        [HttpGet]
        [Route("BlackSoilInitiateApproved")] //ManualWebhook
        [AllowAnonymous]
        public async Task<ResultViewModel<bool>> BlackSoilInitiateApproved()
        {

            return await _nBFCSchedular.BlackSoilInitiateApproved();

        }

        [HttpGet]
        [Route("GetPFCollection")] 
        [AllowAnonymous]
        public async Task<ResultViewModel<string>> GetPFCollection(long leadId)
        {

            return await _nBFCSchedular.GetPFCollection(leadId);

        }
    }
}
