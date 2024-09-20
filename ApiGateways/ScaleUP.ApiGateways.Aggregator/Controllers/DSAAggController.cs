using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.DTOs.DSA;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Managers.NBFC;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using System.Text.Json;
using System.Drawing;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DSAAggController : BaseController
    {
        private DSAManager dsaManager;
        public DSAAggController(DSAManager _dSAManager)
        {
            dsaManager = _dSAManager;
        }



        #region Login/signup/profile
        [HttpGet]
        [Route("GenerateDSAOtp")]
        [AllowAnonymous]
        public async Task<GenerateOTPResponse> GenerateDSAOtp(string MobileNo)
        {
            return await dsaManager.GenerateDSAOtp(MobileNo);
        }

        [HttpPost]
        [Route("DSAValidateOTP")]
        [AllowAnonymous]
        public async Task<DSAMobileOTPValidateResponse> DSAValidateOTP(DSAMobileValidateOTPRequest dSAMobileValidateOTPRequest)
        {
            return await dsaManager.DSALeadMobileValidate(dSAMobileValidateOTPRequest);

        }

        [HttpGet]
        [Route("GetDSAUserProfile")]
        public async Task<DSAUserProfileResponse> GetDSAUserProfile(string UserId, string Mobile)
        {
            return await dsaManager.GetDSAUserProfile(UserId, Mobile);
        }

        [HttpPost]
        [Route("GetLeadByMobileNo")]
        public async Task<DSALeadResponse> GetLeadByMobileNo(string UserId, string Mobile)
        {
            return await dsaManager.GetLeadByMobileNo(UserId, Mobile, Token);

        }
        #endregion



        [HttpPost]
        [Route("ApproveDSALead")]
        public async Task<GRPCReply<bool>> ApproveDSALead(ApproveDSALeadRequest request)
        {
            return await dsaManager.ApproveDSALead(request);
        }

        [HttpPost]
        [Route("GetDSALeadForListPage")]
        public async Task<DSALeadListPageListDTO> GetDSALeadForListPage(DSALeadListPageRequest LeadListPageRequest)
        {
            return await dsaManager.GetDSALeadForListPage(LeadListPageRequest);
        }

        [HttpGet]
        [Route("GetDSAProfileType")]
        public async Task<DSAProfileTypeResponse> GetDSAProfileType(string UserId, string productCode)
        {

            return await dsaManager.GetDSAProfileType(UserId,0, productCode);
        }
        [HttpPost]
        [Route("CreateDSAUser")]
        public async Task<GRPCReply<bool>> CreateDSAUser(CreateDSAUserRequest request)
        {
            return await dsaManager.CreateDSAUser(request, UserId, ProductId, CompanyId);
        }

        [Route("DSAGenerateAgreement")]
        [HttpGet]
        public async Task<GRPCReply<string>> DSAGenerateAgreement(long leadId, long ProductId, long CompanyId, bool IsProceedToEsign) //type=connector or dsa
        {
            return await dsaManager.DSAGenerateAgreement(UserId, leadId, ProductId, CompanyId, IsProceedToEsign);

        }


        //Dashboard
        [Route("GetDSADashboardDetails")]
        [HttpPost]
        public async Task<GRPCReply<GetDSADashboardDetailResponse>> GetDSADashboardDetails(DSADashboardRequest request)
        {
            string role = "";
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.DSAAdmin.ToLower())))
                role = UserRoleConstants.DSAAdmin;
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.Connector.ToLower())))
                role = UserRoleConstants.Connector;
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SalesAgent.ToLower())))
                role = UserRoleConstants.SalesAgent;
            var reply = await dsaManager.GetDSADashboardDetails(request, UserId, role, ProductId);
            return reply;
        }


        //List Of Lead
        [Route("GetDSADashboardLeadList")]
        [HttpPost]
        public async Task<GRPCReply<List<DSADashboardLeadResponse>>> GetDSADashboardLeadList(DSADashboardLeadListRequest request)
        {
            string role = "";
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.DSAAdmin.ToLower())))
                role = UserRoleConstants.DSAAdmin;
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.Connector.ToLower())))
                role = UserRoleConstants.Connector;
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SalesAgent.ToLower())))
                role = UserRoleConstants.SalesAgent;
            var reply = await dsaManager.GetDSADashboardLeadList(request, UserId, role, ProductId);
            return reply;
        }

        // LIst Of Payout againt LeadId
        [Route("GetDSADashboardPayoutList")]
        [HttpPost]
        public async Task<GRPCReply<DSADashboardPayoutResponse>> GetDSADashboardPayoutList(DSADashboardLeadListRequest request)
        {
            string role = "";
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.DSAAdmin.ToLower())))
                role = UserRoleConstants.DSAAdmin;
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.Connector.ToLower())))
                role = UserRoleConstants.Connector;
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SalesAgent.ToLower())))
                role = UserRoleConstants.SalesAgent;
            var reply = await dsaManager.GetDSADashboardPayoutList(request, UserId, role, ProductId);
            return reply;
        }

        [HttpGet]
        [Route("GetConnectorPersonalDetail")]
        public async Task<ConnectorPersonalDetailDTO> GetConnectorPersonalDetail(string UserId, string productCode)
        {
            return await dsaManager.GetConnectorPersonalDetail(UserId, productCode);
        }

        [HttpGet]
        [Route("GetDSAPersonalDetail")]
        public async Task<DSAPersonalDetailDTO> GetDSAPersonalDetail(string UserId, string productCode)
        {
            return await dsaManager.GetDSAPersonalDetail(UserId, productCode);
        }

        [HttpGet]
        [Route("PrepareAgreement")]
        public async Task<GRPCReply<string>> PrepareAgreement(string UserId, long LeadId)
        {
            return await dsaManager.PrepareAgreement(UserId, LeadId);
        }

        [HttpGet]
        [Route("CheckeSignDocumentStatus")]
        public async Task<GRPCReply<string>> eSignDocumentsAsync(long LeadId)
        {
            eSignDocumentStatusDc obj = new eSignDocumentStatusDc();
            obj.LeadId = LeadId;
            obj.DocumentId = "";
            GRPCRequest<eSignDocumentStatusDc> request = new GRPCRequest<eSignDocumentStatusDc> { Request = obj };
            return await dsaManager.eSignDocumentsAsync(request);

        }
        [HttpGet]
        [Route("GetDSAAgreement")]
        public async Task<GRPCReply<LeadAggrementDetailReponse>> GetDSAAgreement(long leadId)
        {
            return await dsaManager.GetDSAAgreement(leadId);
        }

        [HttpGet]
        [Route("GetDSALeadDataById")]
        public async Task<GRPCReply<DSALeadListPageDTO>> GetDSALeadDataById(long LeadId,string Status)
        {
            return await dsaManager.GetDSALeadDataById(LeadId, Status);
        }


        [HttpGet]
        [Route("CheckLeadCreatePermission")]
        public async Task<GRPCReply<bool>> CheckLeadCreatePermission(string mobileNo)
        {
            string role = "";
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.DSAAdmin.ToLower())))
                role = UserRoleConstants.DSAAdmin;
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.Connector.ToLower())))
                role = UserRoleConstants.Connector;
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SalesAgent.ToLower())))
                role = UserRoleConstants.SalesAgent;
            return await dsaManager.CheckLeadCreatePermission(mobileNo, UserId, role, ProductId);
        }

        [HttpGet]
        [Route("GetDSAGSTExist")]
        public async Task<GRPCReply<bool>> GetDSAGSTExist(string UserId, string gst, string productCode)
        {
            return await dsaManager.GetDSAGSTExist(UserId, gst, productCode);
        }

        [Route("DSADeactivate")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> DSADeactivate(long leadId, bool isActive, bool isReject)
        {
            var res = await dsaManager.DSADeactivate(leadId, isActive, isReject);
            return res;
        }

        [Route("GetDSACityList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<GRPCReply<List<DSACityListDc>>> GetDSACityList(string ProfileType)
        {
            return await dsaManager.GetDSACityList(ProfileType);
        }

        [HttpGet]
        [Route("GetDSACurrentCode")]
        public async Task<GRPCReply<string>> GetDSACurrentCode(string EntityName,string DSAType)
        {
            return await dsaManager.GetDSACurrentCode(EntityName, DSAType);
        }
    }
}
