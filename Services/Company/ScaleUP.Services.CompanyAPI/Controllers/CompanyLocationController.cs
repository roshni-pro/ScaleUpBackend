using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.CompanyAPI.Manager;
using ScaleUP.Services.CompanyAPI.Persistence;
using ScaleUP.Services.CompanyDTO.Master;
using ScaleUP.Services.CompanyModels;
using ScaleUP.Services.CompanyModels.Master;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ScaleUP.Services.CompanyAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CompanyLocationController : BaseController
    {
        private readonly CompanyManager _CompanyManager;
        public CompanyLocationController(CompanyManager companyManager)
        {
            _CompanyManager = companyManager;
        }

        [HttpPost]
        [Route("AddCompanyLocation")]
        public async Task<CompanyResponse> InsertCompanyLocation(CompanyLocationDTO companylocation)
        {
            return await _CompanyManager.InsertCompanyLocation(companylocation);
        }

        [HttpGet]
        [Route("GetCompanyLocationById")]
        public async Task<CompanyResponse> GetCompanyLocationById(long companyId)
        {
            return await _CompanyManager.GetCompanyLocationById(companyId);            
        }

        [HttpGet]
        [Route("ActiveInActiveCompanyLocation")]
        public async Task<CompanyResponse> ActiveInActiveCompanyLocation(long companyId, long locationId, bool IsActive)
        {
            return await _CompanyManager.ActiveInActiveCompanyLocation(companyId, locationId, IsActive);            
        }

        [HttpGet]
        [Route("DeleteCompanyLocation")]
        public async Task<CompanyResponse> DeleteCompanyLocation(long companyId, long locationId)
        {
            return await _CompanyManager.DeleteCompanyLocation(companyId, locationId);           
        }
    }
}
