using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Constants;
using ScaleUP.Services.LeadAPI.Helper;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using ScaleUP.Services.LeadModels;
using Serilog;

namespace ScaleUP.Services.LeadAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ThirdPartyApiConfigController : ControllerBase
    {
        private IHostEnvironment _hostingEnvironment;
        private readonly LeadApplicationDbContext _context;
        private readonly IMassTransitService _massTransitService;
        private readonly IRequestClient<ICheckStatus> _checkStatusEvent;
        private readonly IRequestClient<ICreateLeadActivityMessage> _createLeadactivityEvent;
        private readonly ExperianManager _experianManager;
        Serilog.ILogger logger = Log.ForContext<LeadController>();

        public ThirdPartyApiConfigController(LeadApplicationDbContext context, IMassTransitService massTransitService, IRequestClient<ICheckStatus> checkStatusEvent, IRequestClient<ICreateLeadActivityMessage> createLeadactivityEvent, IHostEnvironment hostingEnvironment
            , ExperianManager experianManager)
        {
            _context = context;
            _massTransitService = massTransitService;
            _checkStatusEvent = checkStatusEvent;
            _createLeadactivityEvent = createLeadactivityEvent;
            _hostingEnvironment = hostingEnvironment;
            _experianManager = experianManager;
        }

      
        #region OTP Registration
        [HttpPost]
        [Route("SaveExperianOTPRegistration")]
        public async Task<bool> SaveExperianOTPRegistration(ExperianOTPRegistrationDTO experianOTPRegistrationDTO)
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<ExperianOTPRegistrationDTO>(experianOTPRegistrationDTO, ThirdPartyApiConfigConstant.ExperianOTPRegistration);
            return res;
        }

        [HttpGet]
        [Route("GetExperianOTPRegistration")]
        public async Task<ExperianOTPRegistrationDTO> GetExperianOTPRegistration()
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            ThirdPartyAPIConfigResult<ExperianOTPRegistrationDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<ExperianOTPRegistrationDTO>(ThirdPartyApiConfigConstant.ExperianOTPRegistration);
            if (res != null)
            {
                return res.Config;
            }
            return null;
        }
        #endregion
       
        #region OTP Generation
        [HttpPost]
        [Route("SaveExperianOTPGeneration")]
        public async Task<bool> SaveExperianOTPGeneration(ExperianOTPGenerateDTO experianOTPGenerateDTO)
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<ExperianOTPGenerateDTO>(experianOTPGenerateDTO, ThirdPartyApiConfigConstant.ExperianOTPGeneration);
            return res;
        }

        [HttpGet]
        [Route("GetExperianOTPGeneration")]
        public async Task<ExperianOTPGenerateDTO> GetExperianOTPGeneration()
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            ThirdPartyAPIConfigResult<ExperianOTPGenerateDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<ExperianOTPGenerateDTO>(ThirdPartyApiConfigConstant.ExperianOTPGeneration);
            if (res != null)
            {
                return res.Config;
            }
            return null;
        }
        #endregion
       
        #region OTP Validation
        [HttpPost]
        [Route("SaveExperianOTPValidation")]
        public async Task<bool> SaveExperianOTPValidation(ExperianOTPValidationDTO experianOTPValidationDTO)
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<ExperianOTPValidationDTO>(experianOTPValidationDTO, ThirdPartyApiConfigConstant.ExperianOTPValidation);
            return res;
        }

        [HttpGet]
        [Route("GetExperianOTPValidation")]
        public async Task<ExperianOTPValidationDTO> GetExperianOTPValidation()
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            ThirdPartyAPIConfigResult<ExperianOTPValidationDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<ExperianOTPValidationDTO>(ThirdPartyApiConfigConstant.ExperianOTPValidation);
            if(res != null)
            {
                return res.Config;
            }
            return null;
        }

        #endregion
        
        #region Masked Mobile Generation
        [HttpPost]
        [Route("SaveMaskedMobileGeneration")]
        public async Task<bool> SaveMaskedMobileGeneration(MaskedMobileGenerationDTO maskedMobileGenerationDTO)
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<MaskedMobileGenerationDTO>(maskedMobileGenerationDTO, ThirdPartyApiConfigConstant.MaskedMobileGeneration);
            return res;
        }

        [HttpGet]
        [Route("GetMaskedMobileGeneration")]
        public async Task<MaskedMobileGenerationDTO> GetMaskedMobileGeneration()
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            ThirdPartyAPIConfigResult<MaskedMobileGenerationDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<MaskedMobileGenerationDTO>(ThirdPartyApiConfigConstant.MaskedMobileGeneration);
            if (res != null)
            {
                return res.Config;
            }
            return null;
        }
        #endregion
        
        #region Masked Mobile OTP Generation
        [HttpPost]
        [Route("SaveMaskedMobileOTPGeneration")]
        public async Task<bool> SaveMaskedMobileOTPGeneration(MaskedMobileOTPGenerationDTO maskedMobileOTPGenerationDTO)
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<MaskedMobileOTPGenerationDTO>(maskedMobileOTPGenerationDTO, ThirdPartyApiConfigConstant.MaskedMobileOTPGeneration);
            return res;
        }

        [HttpGet]
        [Route("GetMaskedMobileOTPGeneration")]
        public async Task<MaskedMobileOTPGenerationDTO> MaskedMobileOTPGeneration()
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            ThirdPartyAPIConfigResult<MaskedMobileOTPGenerationDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<MaskedMobileOTPGenerationDTO>(ThirdPartyApiConfigConstant.MaskedMobileOTPGeneration);
            if (res != null)
            {
                return res.Config;
            }
            return null;
        }
        #endregion
      
        #region Masked Mobile OTP Validation
        [HttpPost]
        [Route("SaveMaskedMobileOTPValidation")]
        public async Task<bool> SaveMaskedMobileOTPValidation(MaskedMobileOTPValidationDTO maskedMobileOTPValidationDTO)
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<MaskedMobileOTPValidationDTO>(maskedMobileOTPValidationDTO, ThirdPartyApiConfigConstant.MaskedMobileOTPValidation);
            return res;
        }

        [HttpGet]
        [Route("GetMaskedMobileOTPValidation")]
        public async Task<MaskedMobileOTPValidationDTO> GetMaskedMobileOTPValidation()
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            ThirdPartyAPIConfigResult<MaskedMobileOTPValidationDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<MaskedMobileOTPValidationDTO>(ThirdPartyApiConfigConstant.MaskedMobileOTPValidation);
            if (res != null)
            {
                return res.Config;
            }
            return null;
        }
        #endregion


        #region eNach Bank List Data
        [HttpPost]
        [Route("SaveEnachBankList")]
        public async Task<bool> SaveEnachBankList(eNachBankListDTO maskedMobileOTPValidationDTO)
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<eNachBankListDTO>(maskedMobileOTPValidationDTO, ThirdPartyApiConfigConstant.eNachBankList);
            return res;
        }

        [HttpGet]
        [Route("GetEnachBankList")]
        public async Task<eNachBankListDTO> GetEnachBankList()
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            ThirdPartyAPIConfigResult<eNachBankListDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<eNachBankListDTO>(ThirdPartyApiConfigConstant.eNachBankList);
            if (res != null)
            {
                return res.Config;
            }
            return null;
        }


        //[HttpPost]
        //[Route("SaveEnachPost")]
        //public async Task<bool> SaveEnachPost(eNachBankListDTO maskedMobileOTPValidationDTO)
        //{
        //    ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
        //    var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<eNachBankListDTO>(maskedMobileOTPValidationDTO, ThirdPartyApiConfigConstant.eNachPost);
        //    return res;
        //}

        //[HttpGet]
        //[Route("GetEnachPost")]
        //public async Task<eNachBankListDTO> GetEnachPost()
        //{
        //    ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
        //    ThirdPartyAPIConfigResult<eNachBankListDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<eNachBankListDTO>(ThirdPartyApiConfigConstant.eNachPost);
        //    if (res != null)
        //    {
        //        return res.Config;
        //    }
        //    return null;
        //}


        //[HttpPost]
        //[Route("SaveEnachKey")]
        //public async Task<bool> SaveEnachKey(eNachKeyDTO maskedEnachKeyDTO)
        //{
        //    ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
        //    var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<eNachKeyDTO>(maskedEnachKeyDTO, ThirdPartyApiConfigConstant.eNachKey);
        //    return res;
        //}

        //[HttpGet]
        //[Route("GetEnachKey")]
        //public async Task<eNachKeyDTO> GetEnachKey()
        //{
        //    ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
        //    ThirdPartyAPIConfigResult<eNachKeyDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<eNachKeyDTO>(ThirdPartyApiConfigConstant.eNachKey);
        //    if (res != null)
        //    {
        //        return res.Config;
        //    }
        //    return null;
        //}

        
        [HttpPost]
        [Route("SaveEnachConfig")]
        public async Task<bool> SaveEnachConfig(eNachConfigDTO maskedEnachConfigDTO)
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            var res = await thirdPartyApiConfigManager.SaveThirdPartyApiConfig<eNachConfigDTO>(maskedEnachConfigDTO, ThirdPartyApiConfigConstant.eNachConfig);
            return res;
        }

        [HttpGet]
        [Route("GetEnachConfig")]
        public async Task<eNachConfigDTO> GetEnachConfig()
        {
            ThirdPartyApiConfigManager thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            ThirdPartyAPIConfigResult<eNachConfigDTO> res = await thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<eNachConfigDTO>(ThirdPartyApiConfigConstant.eNachConfig);
            if (res != null)
            {
                return res.Config;
            }
            return null;
        }
        #endregion



        #region Main Methods call
        [HttpPost]
        [Route("ExperianOTPRegisterAsync")]
        public async Task<ExperianOTPRegistrationResponseDTO> ExperianOTPRegisterAsync(ExperianOTPRegistrationRequestDC requestDC)
        {
            if (EnvironmentConstants.EnvironmentName.ToLower() == "development")
            {

                requestDC.LeadId = requestDC.LeadId;
                requestDC.activityId = 0;
                requestDC.companyId = 0;
                requestDC.subActivityId = 0;
                requestDC.aadhaar = "";
                requestDC.city = "Wagholi";
                requestDC.dateOfBirth = "11-Oct-1990";
                requestDC.email = "harry@shopkirana.com";
                requestDC.firstName = "Hari";
                requestDC.surName = "Shinde";
                requestDC.flatno = "D 571";
                requestDC.gender = 2;
                requestDC.middleName = "";
                requestDC.mobileNo = "8319524344";
                requestDC.pincode = "412207";
                requestDC.state = 27;
                requestDC.pan = "BQGPM7296M";
                requestDC.experianState = 23;
                requestDC.cityId = 1;
            }


            ExperianOTPRegistrationResponseDTO res = new ExperianOTPRegistrationResponseDTO();
            string basePath = _hostingEnvironment.ContentRootPath;
            ExperianHelper experianHelper = new ExperianHelper(_context);
            if(requestDC.state.HasValue)
            {
                var stateId = _experianManager.GetExperianStateId(requestDC.state.Value);
                if (stateId != null && stateId.Result != null && stateId.Result.Response != null)
                {
                    requestDC.experianState = stateId.Result.Response.ExperianStateId;
                    res = await experianHelper.ExperianOTPRegisterAsync(requestDC, basePath);
                    return res;
                }
                else
                {
                    res = new ExperianOTPRegistrationResponseDTO
                    {
                        stgTwoHitId = "",
                        stgOneHitId = "",
                        errorString = "ExperianState not found"
                    };
                } 
            }
            else
            {
                res = new ExperianOTPRegistrationResponseDTO
                {
                    stgTwoHitId = "",
                    stgOneHitId = "",
                    errorString = "State not passed"
                };
            }
            return res;
        }

        [HttpPost]
        [Route("ExperianOTPGenerationAsync")]
        public async Task<ExperianOTPGenerationResponseDC> ExperianOTPGenerationAsync(ExperianOTPGenerationRequestDC requestDC)
        {
            string basePath = _hostingEnvironment.ContentRootPath;
            ExperianHelper experianHelper = new ExperianHelper(_context);
            var res = await experianHelper.ExperianOTPGenerationAsync(requestDC, basePath);
            return res;
        }

        [HttpPost]
        [Route("ExperianOTPValidationAsync")]
        public async Task<ExperianOTPValidationResponseDC> ExperianOTPValidationAsync(ExperianOTPValidationRequestDC requestDC)
        {
            string basePath = _hostingEnvironment.ContentRootPath;
            ExperianHelper experianHelper = new ExperianHelper(_context);
            var res = await experianHelper.ExperianOTPValidationAsync(requestDC, basePath);

            if(res != null && !string.IsNullOrEmpty( res.CreditScore ))
            {
                double creditScore;
                bool isSuccess =  double.TryParse(res.CreditScore, out creditScore);
                if(isSuccess)
                {
                    var leadActivityMasterProgresses = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == requestDC.LeadId &&
                                                                                       x.ActivityMasterId == requestDC.ActivityId &&
                                                                                       x.SubActivityMasterId == requestDC.SubActivityId &&
                                                                                       x.IsActive && !x.IsDeleted);
                    if (leadActivityMasterProgresses != null)
                    {
                        leadActivityMasterProgresses.IsCompleted = true;                        
                        //_logger.LogInformation("Lead: {leadId} with ActivityId: {ActivityId} or SubActivityId: {SubActivityId} completed successfully", context.Message.LeadId, context.Message.ActivityId, context.Message.SubActivityId);
                    }
                    Leads leads = await _context.Leads.Where(x => x.Id == requestDC.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    if (leads != null)
                    {
                        leads.Status = LeadStatusEnum.CibilSuccess.ToString();
                        leads.CreditScore = Convert.ToInt64(res.CreditScore);
                        leads.CibilReport = res.FilePath;
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return res;
        }

        [HttpPost]
        [Route("MaskedMobileGenerationAsync")]
        public async Task<MaskedMobileGenerationResponseDC> MaskedMobileGenerationAsync(MaskedMobileGenerationRequestDC requestDC)
        {
            string basePath = _hostingEnvironment.ContentRootPath;
            ExperianHelper experianHelper = new ExperianHelper(_context);
            var res = await experianHelper.MaskedMobileGenerationAsync(requestDC, basePath);
            return res;
        }

        [HttpPost]
        [Route("MaskedMobileOTPGenerationAsync")]
        public async Task<MaskedMobileOTPGenerationResponseDC> MaskedMobileOTPGenerationAsync(MaskedMobileOTPGenerationRequestDC requestDC)
        {
            string basePath = _hostingEnvironment.ContentRootPath;
            ExperianHelper experianHelper = new ExperianHelper(_context);
            var res = await experianHelper.MaskedMobileOTPGenerationAsync(requestDC, basePath);
            return res;
        }

        [HttpPost]
        [Route("MaskedMobileOTPValidationAsync")]
        public async Task<ExperianOTPValidationResponseDC> MaskedMobileOTPValidationAsync(MaskedMobileOTPValidationRequestDC requestDC)
        {
            string basePath = _hostingEnvironment.ContentRootPath;
            ExperianHelper experianHelper = new ExperianHelper(_context);
            var res = await experianHelper.MaskedMobileOTPValidationAsync(requestDC, basePath);
            if (res != null && !string.IsNullOrEmpty(res.CreditScore))
            {
                double creditScore;
                bool isSuccess = double.TryParse(res.CreditScore, out creditScore);
                if (isSuccess)
                {
                    var leadActivityMasterProgresses = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == requestDC.LeadId &&
                                                                                       x.ActivityMasterId == requestDC.ActivityId &&
                                                                                       x.SubActivityMasterId == requestDC.SubActivityId &&
                                                                                       x.IsActive && !x.IsDeleted);

                    if (leadActivityMasterProgresses != null)
                    {
                        leadActivityMasterProgresses.IsCompleted = true;
                        //_logger.LogInformation("Lead: {leadId} with ActivityId: {ActivityId} or SubActivityId: {SubActivityId} completed successfully", context.Message.LeadId, context.Message.ActivityId, context.Message.SubActivityId);
                    }
                    Leads leads = await _context.Leads.Where(x => x.Id == requestDC.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    if (leads != null)
                    {
                        leads.Status = LeadStatusEnum.CibilSuccess.ToString();
                        leads.CreditScore = Convert.ToInt64(res.CreditScore);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return res;
        }
        #endregion
    }
}
