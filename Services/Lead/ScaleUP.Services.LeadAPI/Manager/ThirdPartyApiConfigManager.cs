using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.LeadNBFC;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class ThirdPartyApiConfigManager
    {
        private readonly LeadApplicationDbContext _context;
        public ThirdPartyApiConfigManager(LeadApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<bool> SaveThirdPartyApiConfig<T>(T experianOTPRegistrationDTO,string Code)
        {
            bool res = false;

            var ThirdPartyData = _context.ThirdPartyApiConfigs.Where(x => x.Code == Code).FirstOrDefault();
            if (ThirdPartyData == null)
            {

                ThirdPartyApiConfig thirdPartyApiConfig = new ThirdPartyApiConfig
                {
                    Code = Code,
                    ConfigJson = JsonConvert.SerializeObject(experianOTPRegistrationDTO),
                    IsActive = true,
                    IsDeleted = false
                };
                _context.ThirdPartyApiConfigs.Add(thirdPartyApiConfig);
            }
            else
            {
                ThirdPartyData.ConfigJson = JsonConvert.SerializeObject(experianOTPRegistrationDTO);
                _context.ThirdPartyApiConfigs.Update(ThirdPartyData);
            }
            if (_context.SaveChanges() > 0)
            {
                res = true;
                return res;
            }
            return res;
        }

        public async Task<T> GetThirdPartyApiConfig<T>(string code) where T : new()
        {
            T experianOTPRegistrationDTO = new T();
            var ThirdPartyData = _context.ThirdPartyApiConfigs.Where(x => x.Code == code).FirstOrDefault();
            if (ThirdPartyData != null)
            {
                string json = ThirdPartyData.ConfigJson;
                experianOTPRegistrationDTO = JsonConvert.DeserializeObject<T>(json);
            }
            return experianOTPRegistrationDTO ;
        }


        public async Task<ThirdPartyAPIConfigResult<T>> GetThirdPartyApiConfigWithId<T>(string code) where T : new()
        {
            T experianOTPRegistrationDTO = new T();
            var ThirdPartyData = _context.ThirdPartyApiConfigs.Where(x => x.Code == code).FirstOrDefault();
            if (ThirdPartyData != null)
            {
                string json = ThirdPartyData.ConfigJson;
                experianOTPRegistrationDTO = JsonConvert.DeserializeObject<T>(json);
                return new ThirdPartyAPIConfigResult<T>
                {
                    Code = ThirdPartyData.Code,
                    Id = ThirdPartyData.Id,
                    Config = experianOTPRegistrationDTO
                };
            }
            return null;
        }

        public async Task<LeadNBFCApi?> GetByCode(string code)
        {
            LeadNBFCApi thirdPartyAPIConfigData = await _context.LeadNBFCApis.Where(x => x.Code == code && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
            return thirdPartyAPIConfigData;
        }
        public async Task<bool> InsertApiReqRes(ThirdPartyApiRequestDTO request)
        {
            var res = await _context.ApiRequestResponse.AddAsync(new CommonApiRequestResponse
            {
                UserId = request.UserId,
                APIConfigId = request.APIConfigId,
                CompanyId = request.CompanyId,
                ProcessedResponse = request.ProcessedResponse,
                Request = request.Request,
                Response = request.Response,
                Created = request.Created,
                IsActive = true,
                IsDeleted = false
            });
            return _context.SaveChanges() > 0;

        }

    }
}
