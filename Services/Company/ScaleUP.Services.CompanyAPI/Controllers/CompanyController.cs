using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.CompanyAPI.Manager;
using ScaleUP.Services.CompanyAPI.Persistence;
using ScaleUP.Services.CompanyDTO.Master;
using ScaleUP.Services.CompanyModels;
using ScaleUP.Services.CompanyModels.Master;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ScaleUP.Services.CompanyAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class CompanyController : BaseController
    {
        private readonly CompanyManager _CompanyManager;
        public CompanyController(CompanyManager companyManager)
        {
            _CompanyManager = companyManager;
        }


        [HttpPost]
        [Route("UpdateCompany")]
        public async Task<CompanyResponse> UpdateCompany(CompanyDTO.Master.CompanyDTO company)
        {
            return await _CompanyManager.UpdateCompany(company);
        }

        [HttpGet]
        [Route("CompanyExists")]
        public async Task<CompanyResponse> CompanyExists(string companyName, long? companyId)
        {
            return await _CompanyManager.CompanyExists(companyName, companyId);
        }

        [HttpGet]
        [Route("DeleteCompany")]
        public async Task<CompanyResponse> DeleteCompany(long companyId)
        {
            return await _CompanyManager.DeleteCompany(companyId);
        }

        [HttpGet]
        [Route("GetCompanyList")]
        public async Task<CompanyResponse> GetCompanyList()
        {
            if(UserType.ToLower()== UserTypeConstants.SuperAdmin)
            {
                return await _CompanyManager.GetCompanyList();
            }
            else
            {
                var ids = CompanyIds;
                return await _CompanyManager.GetCompanyListByids(ids);
            }
            
        }

        [HttpGet]
        [Route("GetCompanyListByCompanyType")]
        public async Task<CompanyResponse> GetCompanyListByCompanyType(string CompanyType)
        {
            var CompanyIdList = new List<long>();
            if (UserType.ToLower() != UserTypeConstants.SuperAdmin)
            {
                 CompanyIdList = CompanyIds;
            }
            return await _CompanyManager.GetCompanyListByCompanyType(CompanyType, CompanyIdList);
        }

        [HttpGet]
        [Route("GetCompanyById")]
        public async Task<GetCompanyRes> GetCompanyById(long companyId)
        {
            return await _CompanyManager.GetCompanyById(companyId);
        }

        [HttpGet]
        [Route("ActiveInActiveCompany")]
        public async Task<CompanyResponse> ActiveInActiveCompany(long CompanyId, bool IsActive)
        {
            return await _CompanyManager.ActiveInActiveCompany(CompanyId, IsActive);
        }

        [HttpGet]
        [Route("GetCompanyByTypeId")]
        public async Task<CompanyResponse> GetCompanyByType(int BusinessTypeId)
        {
            return await _CompanyManager.GetCompanyByType(BusinessTypeId);
        }

        [HttpGet]
        [Route("GenerateApiKey")]
        public CompanyResponse GenerateApiKey()
        {
            return _CompanyManager.GenerateApiKey();
        }

        [HttpGet]
        [Route("GenerateSecretKey")]
        public CompanyResponse GenerateSecretKey()
        {
            return _CompanyManager.GenerateSecretKey();
        }

        [HttpGet]
        [Route("GetBusinessTypeMasterList")]
        public async Task<CompanyResponse> GetBusinessTypeMasterList(string CompanyType)
        {
            return await _CompanyManager.GetBusinessTypeMasterList(CompanyType);
        }

        [HttpGet]
        [Route("GetCompanyPartnersList")]
        public async Task<CompanyResponse> GetCompanyPartnersList(long CompanyId)
        {
            return await _CompanyManager.GetCompanyPartnersList(CompanyId);
        }

        [HttpGet]
        [Route("UserActiveInactive")]
        public async Task<CompanyResponse> UserActiveInactive(string UserId, bool IsActive)
        {
            return await _CompanyManager.UserActiveInactive(UserId, IsActive);
        }

        [HttpGet]
        [Route("GetGSTDetail")]
        public async Task<GSTDetailDTO> GetGSTDetail(string GSTNO)
        {
            return await _CompanyManager.GetGSTDetail(GSTNO);
        }

        [HttpGet]
        [Route("CheckCompanyIsDefault")]
        public async Task<bool> CheckCompanyIsDefault(long CompanyId)
        {
            return await _CompanyManager.CheckCompanyIsDefault(CompanyId);
        }

        [HttpGet]
        [Route("GetLatestGSTRate")]
        public async Task<CompanyResponse> GetLatestGSTRate()
        {
            return await _CompanyManager.GetLatestGSTRate();
        }

        [HttpGet]
        [Route("CheckCompanyGSTExist")]
        public async Task<GSTDetailDTO> CheckCompanyGSTExist(string GSTNO)
        {
            return await _CompanyManager.CheckCompanyGSTExist(GSTNO);
        }

        [HttpGet]
        [Route("GetCompanyAuditLogs")]
        public async Task<CompanyResponse<List<CompanyLogDc>>> GetCompanyAuditLogs(long CompanyId)
        {
            return await _CompanyManager.GetCompanyAuditLogs(CompanyId);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SaveModifyTemplateMaster")]
        public async Task<CompanyTemplateResponseDc> SaveModifyTemplateMaster(CompanyTemplateDc templatedc)
        {
            return await _CompanyManager.SaveModifyTemplateMaster(templatedc);
        }

        [HttpGet]
        [Route("GetEducationMasterList")]
        public async Task<CompanyResponse<List<GetEducationMasterListDc>>> GetEducationMasterList()
        {
            return await _CompanyManager.GetEducationMasterList();
        }

        [HttpGet]
        [Route("GetLangauageMasterList")]
        public async Task<CompanyResponse<List<LanguageMasterDTO>>> GetLangauageMasterList()
        {
            return await _CompanyManager.GetLangauageMasterList();
        }
        [HttpGet]
        [Route("AddLangauage")]
        public async Task<CompanyResponse<bool>> AddLangauage(string LanguageName)
        {
            return await _CompanyManager.AddLangauage(LanguageName);
        }
    }
}
