using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using System.Data;

namespace ScaleUP.ApiGateways.Aggregator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CompanyAggController : BaseController
    {
        private CompanyManager _companyManager;
        public CompanyAggController(CompanyManager companyManager)
        {
            _companyManager = companyManager;
        }

        [HttpPost]
        [Route("SaveCompanyAndLocationAsync")]
        public async Task<CreateCompanyResponse> SaveCompanyAndLocationAsync(SaveCompanyAndLocationDTO saveCompanyAndLocationDTO)
        {
            var response = await _companyManager.SaveCompanyAndLocationAsync(saveCompanyAndLocationDTO);
            return response;
        }

        [HttpGet]
        [Route("GetCompanyAddressAndDetails")]
        public async Task<CompanyAddressDetailsDTO> GetCompanyAddressAndDetails(long companyId)
        {
            return await _companyManager.GetCompanyAddressAndDetails(companyId);

        }

        [HttpPost]
        [Route("UpdateCompanyAsync")]
        public async Task<UpdateCompanyResponse> UpdateCompanyAsync(SaveCompanyAndLocationDTO companydc)
        {
            return await _companyManager.UpdateCompanyAsync(companydc);

        }

        [HttpPost]
        [Route("CreateCompanyLocationAsync")]
        public async Task<CompanyResponse> CreateCompanyLocationAsync(CreateAddressDTO createAddressDTO)
        {
            return await _companyManager.CreateCompanyLocationAsync(createAddressDTO);
        }

        [HttpPost]
        [Route("CreateUserAsync")]
        public async Task<UserResponse> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            return await _companyManager.CreateUserAsync(createUserDTO, UserType);
        }

        [HttpPost]
        [Route("UpdateUserByUserId")]
        public async Task<UserUpdateByIdResponseDTO> UpdateUserByUserId(UserUpdateByIdDTO req)
        {
            return await _companyManager.UpdateUserByUserId(req, UserType);
        }

        [HttpPost]
        [Route("CreateCompanyAsync")]
        public async Task<CreateCompanyResponse> CreateCompanyAsync(CreateCompanyDTO createCompanyDTO)
        {
            return await _companyManager.CreateCompanyAsync(createCompanyDTO);
        }

        [HttpPost]
        [Route("GetCompanyList")]
        public async Task<CompanyListResponse> GetCompanyListAsync(CompanyListDTO companyDc)
        {
            return await _companyManager.GetCompanyListAsync(companyDc);
        }

        [HttpPost]
        [Route("GetUserList")]
        public async Task<UserListResponse> GetUserList(CompanyDTO req)
        {
            if (UserType.ToLower() != UserTypeConstants.SuperAdmin)
                req.companyIds = CompanyIds;
            return await _companyManager.GetUserList(req);
        }

        [HttpGet]
        [Route("GetProdName")]
        public async Task<string> GetProductName(long productId)
        {
            return await _companyManager.GetProductName(productId);
        }

        [HttpPost]
        [Route("ResetUserPassword")]
        public async Task<bool> ResetUserPassword(string UserId)
        {
            return await _companyManager.ResetUserPassword(UserId);

        }

        [HttpGet]
        [Route("GetFinTechCompany")]
        [AllowAnonymous]
        public async Task GetFinTechCompany()
        {
            await _companyManager.GetFinTechCompany();
        }

        [HttpGet]
        [Route("GetCurrentNumber")]
        [AllowAnonymous]
        public async Task<string> GetCurrentNumber(string EntityName, string CompanyType)
        {
            return await _companyManager.GetCurrentNumber(EntityName, CompanyType);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ForgotUserPassword")]
        public async Task<ForgotPasswordResponse> ForgotUserPassword(string UserName)
        {
            return await _companyManager.ForgotUserPassword(UserName);

        }

        [HttpPost]
        [Route("GetAuditLogs")]
        public async Task<AuditLogAggResponseDc> GetAuditLogs(AuditLogAggRequestDc auditLogAggRequestDc)
        {
            return await _companyManager.GetAuditLogs(auditLogAggRequestDc);
        }



        [HttpGet]
        [Route("GetTemplateMasterAsync")]
        public async Task<TemplatesReplyDTO> GetTemplateMasterAsync()
        {
            return await _companyManager.GetTemplateMasterAsync();
        }

        [HttpGet]
        [Route("GetTemplateById")]
        public async Task<TemplateByIdResDTO> GetTemplateById(long Id, string type)
        {
            return await _companyManager.GetTemplateById(Id, type);
        }


        [HttpPost]
        [Route("GetGSTDetails")]
        public async Task<GSTDetailReply> GetGSTDetails(string GSTNo)
        {
            return await _companyManager.GetGSTDetails(GSTNo);
        }

        [HttpPost]
        [Route("CheckCompanyAdminExist")]
        public async Task<UserResponse> CheckCompanyAdminExist(CheckCompanyAdminDTO request)
        {
            return await _companyManager.CheckCompanyAdminExist(request);
        }
        [HttpGet]
        [Route("GetNbfcCompaniesByProduct")]
        public async Task<List<LeadNbfcResponse>> GetNbfcCompaniesByProduct(long productId)
        {
            List<LeadNbfcResponse> leadNbfcResponses = new List<LeadNbfcResponse>();
            var res = await _companyManager.GetCompaniesByProduct(productId);
            if (res.Status)
            {
                leadNbfcResponses = res.Response;
            }
            return leadNbfcResponses;
        }

        [HttpGet]
        [Route("GetAnchorCompaniesByProduct")]
        public async Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetAnchorCompaniesByProduct(string productType)
        {
            var res = await _companyManager.GetAnchorCompaniesByProduct(productType);
            return res;
        }

        [HttpGet]
        [Route("GetCompanyListForDropDown")]
        public async Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetCompanyListForDropDown()
        {
            var res = await _companyManager.GetCompanyListForDropDown(UserType, CompanyIds);
            return res;
        }
    }
}
