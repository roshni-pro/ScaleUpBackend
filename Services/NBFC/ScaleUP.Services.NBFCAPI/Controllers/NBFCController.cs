using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.NBFCAPI.Persistence;
using ScaleUP.Services.NBFCDTO.Master;
using ScaleUP.Services.NBFCModels.Master;
using System.Collections.Generic;

namespace ScaleUP.Services.NBFCAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class NBFCController : BaseController
    {
        private readonly NBFCApplicationDbContext _context;
        public NBFCController(NBFCApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("AddUpdateSelfConfiguration")]
        public async Task<NBFCResponse<List<SelfConfigDTO>>> AddUpdateSelfConfiguration(List<SelfConfigDTO> selfConfigList)
        {
            NBFCResponse<List<SelfConfigDTO>> response = new NBFCResponse<List<SelfConfigDTO>>();
            foreach (var config in selfConfigList)
            {
                if (config.Id > 0)
                {
                    var existing = await _context.OfferSelfConfigurations.FirstOrDefaultAsync(x => x.Id == config.Id);
                    if (existing != null)
                    {
                        existing.CompanyId = config.CompanyId;
                        existing.CustomerType = config.CustomerType;
                        existing.MaxCibilScore = config.MaxCibilScore;
                        existing.MaxCreditLimit = config.MaxCreditLimit;
                        existing.MaxVintageDays = config.MaxVintageDays;
                        existing.MinCibilScore = config.MinCibilScore;
                        existing.MinCreditLimit = config.MinCreditLimit;
                        existing.MinVintageDays = config.MinVintageDays;
                        existing.MultiPlier = config.MultiPlier;
                        existing.IsActive = config.IsActive;

                        _context.Entry(existing).State = EntityState.Modified;
                    }
                }
                else
                {
                    OfferSelfConfiguration offerSelfConfiguration = new OfferSelfConfiguration
                    {
                        CompanyId = config.CompanyId,
                        CustomerType = config.CustomerType,
                        MaxCibilScore = config.MaxCibilScore,
                        MaxCreditLimit = config.MaxCreditLimit,
                        MaxVintageDays = config.MaxVintageDays,
                        MinCibilScore = config.MinCibilScore,
                        MinCreditLimit = config.MinCreditLimit,
                        MinVintageDays = config.MinVintageDays,
                        MultiPlier = config.MultiPlier,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.OfferSelfConfigurations.Add(offerSelfConfiguration);
                }
            }
            int rowChanged = await _context.SaveChangesAsync();
            if (rowChanged > 0)
            {
                response.Status = true;
                response.Message = "Configuration Updated Successfully";
                response.ReturnObject = selfConfigList;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed To Update Configs";
            }
            return response;
        }

        [HttpGet]
        [Route("GetSelfConfigurationList")]
        public async Task<NBFCResponse<List<SelfConfigDTO>>> GetSelfConfigurationList(long CompanyId)
        {
            NBFCResponse<List<SelfConfigDTO>> response = new NBFCResponse<List<SelfConfigDTO>>();
            var configs = await _context.OfferSelfConfigurations.Where(x => x.CompanyId == CompanyId && !x.IsDeleted).Select(x => new SelfConfigDTO
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                CustomerType = x.CustomerType,
                MaxCibilScore = x.MaxCibilScore,
                MaxCreditLimit = x.MaxCreditLimit,
                MaxVintageDays = x.MaxVintageDays,
                MinCibilScore = x.MinCibilScore,
                MinCreditLimit = x.MinCreditLimit,
                MinVintageDays = x.MinVintageDays,
                MultiPlier = x.MultiPlier,
                IsActive = x.IsActive
            }).ToListAsync();
            if (configs != null && configs.Any())
            {
                response.Status = true;
                response.Message = "Data Found";
                response.ReturnObject = configs;
            }
            else
            {
                response.Status = false;
                response.Message = "No Data Found";
            }
            return response;
        }

        [HttpGet]
        [Route("GetSelfConfigurationById")]
        public async Task<NBFCResponse<SelfConfigDTO>> GetSelfConfigurationById(long SelfConfigId)
        {
            NBFCResponse<SelfConfigDTO> response = new NBFCResponse<SelfConfigDTO>();
            var configs = await _context.OfferSelfConfigurations.Where(x => x.Id == SelfConfigId && !x.IsDeleted).Select(x => new SelfConfigDTO
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                CustomerType = x.CustomerType,
                MaxCibilScore = x.MaxCibilScore,
                MaxCreditLimit = x.MaxCreditLimit,
                MaxVintageDays = x.MaxVintageDays,
                MinCibilScore = x.MinCibilScore,
                MinCreditLimit = x.MinCreditLimit,
                MinVintageDays = x.MinVintageDays,
                MultiPlier = x.MultiPlier,
                IsActive = x.IsActive
            }).FirstOrDefaultAsync();
            if (configs != null)
            {
                response.Status = true;
                response.Message = "Data Found";
                response.ReturnObject = configs;
            }
            else
            {
                response.Status = false;
                response.Message = "No Data Found";
            }
            return response;
        }

        [HttpGet]
        [Route("DeleteSelfConfigurationById")]
        public async Task<NBFCResponse<SelfConfigDTO>> DeleteSelfConfigurationById(long SelfConfigId)
        {
            NBFCResponse<SelfConfigDTO> response = new NBFCResponse<SelfConfigDTO>();
            var config = await _context.OfferSelfConfigurations.FirstOrDefaultAsync(x => x.Id == SelfConfigId);
            if (config != null)
            {
                config.IsActive = false;
                config.IsDeleted = true;
                _context.Entry(config).State = EntityState.Modified;
                int rowChanged = await _context.SaveChangesAsync();
                if (rowChanged > 0)
                {
                    response.Status = true;
                    response.Message = "Deleted Successfully";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed To delete Config";
                }
            }
            else
            {
                response.Status = false;
                response.Message = "Config Not Found";
            }
            return response;
        }
    }
}
