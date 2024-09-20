using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Nito.AsyncEx;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Services.LeadAPI.Controllers;
using ScaleUP.Services.LeadAPI.Helper;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.Experian;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using ScaleUP.Services.LeadModels;
using Serilog;
using System.Data;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class eNachManager
    {
        private readonly LeadApplicationDbContext _context;
        private ThirdPartyApiConfigManager _thirdPartyApiConfigManager;
        private eNachHelper _eNachHelper;
        private IHostEnvironment _hostingEnvironment;

        public eNachManager(LeadApplicationDbContext context, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _thirdPartyApiConfigManager = new ThirdPartyApiConfigManager(_context);
            _eNachHelper = new eNachHelper(_context);
            _hostingEnvironment = hostingEnvironment;
        }



        public async Task<eNachBankListResponseDTO> getBankListAsync()
        {
            string basePath = _hostingEnvironment.ContentRootPath;
            ThirdPartyAPIConfigResult<eNachBankListConfigDc> res = await _thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<eNachBankListConfigDc>(ThirdPartyApiConfigConstant.eNachBankList);
            
            return await _eNachHelper.getBankListAsync(res.Config, basePath);
        }


        public async Task<GRPCReply<string>> eNachCheckENachPreviousReq(GRPCRequest<long> requestLeadId)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();


            var leadid = new SqlParameter("LeadMasterId", requestLeadId.Request);
            var result = _context.Database.SqlQueryRaw<string>("exec eNachCheckPreviousReq @LeadMasterId", leadid).AsEnumerable().FirstOrDefault();




            gRPCReply.Response = result;
            return gRPCReply;
        }


        public Task<GRPCReply<LeadDetailDC>> GetLeadUser(GRPCRequest<long> request)
        {
            GRPCReply<LeadDetailDC> reply = new GRPCReply<LeadDetailDC>();
            var user = (_context.Leads.Where(x => x.Id == request.Request).Select(x=>new LeadDetailDC
            {
                UserName = x.UserName,
                Product = x.ProductCode,
            }).FirstOrDefault());
            if (user != null)
            {
                reply.Response = user;
                reply.Status = true;
            }
            return Task.FromResult(reply);
        }

        public async Task<GRPCReply<bool>> eNachAddAsync(GRPCRequest<eNachBankDetailDc> request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            gRPCReply.Response = await _eNachHelper.AddAsync(request.Request);
            return (gRPCReply);
        }



        public async Task<eNachSendReqDTO> eNachSendeMandateRequestToHDFCAsync(GRPCRequest<SendeMandateRequestDc> request)
        {
            string basePath = _hostingEnvironment.ContentRootPath;
            eNachSendReqDTO gRPCReply = new eNachSendReqDTO();

            //ThirdPartyAPIConfigResult<eNachBankListConfigDc> eNachPostRes = await _thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<eNachBankListConfigDc>(ThirdPartyApiConfigConstant.eNachPost);
            //ThirdPartyAPIConfigResult<eNachKeyDTO> eNachKeyRes = await _thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<eNachKeyDTO>(ThirdPartyApiConfigConstant.eNachKey);
            ThirdPartyAPIConfigResult<eNachConfigDc> eNachConfigRes = await _thirdPartyApiConfigManager.GetThirdPartyApiConfigWithId<eNachConfigDc>(ThirdPartyApiConfigConstant.eNachConfig);


            var leads = await _context.Leads.Where(x => x.Id == request.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (leads != null)
            {
                var leadActivities = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leads.Id && x.IsActive && !x.IsDeleted && !x.IsCompleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
                if (leadActivities != null)
                {
                    request.Request.ActivityId = leadActivities.ActivityMasterId;
                    request.Request.SubActivityId = leadActivities.SubActivityMasterId;
                    request.Request.CompanyId = leads.OfferCompanyId;
                }

                request.Request.LeadNo = leads.LeadCode;
                request.Request.CreditLimit = Convert.ToInt64(leads.CreditLimit);

                eNachBankListConfigDc eNachPostRes = new eNachBankListConfigDc();
                eNachPostRes.ApiUrl = eNachConfigRes.Config.eNachPostApiUrl;
                eNachPostRes.ApiMasterId = eNachConfigRes.Config.eNachPostApiMasterId;

                //gRPCReply = await _eNachHelper.enachSendeEnachRequestToHDFCAsync(request.Request, eNachPostRes.Config, eNachKeyRes.Config.value, eNachConfigRes.Config, basePath);
                gRPCReply = await _eNachHelper.enachSendeEnachRequestToHDFCAsync(request.Request, eNachPostRes, eNachConfigRes.Config.eNachKey, eNachConfigRes.Config, basePath);
            }

            return gRPCReply;
        }

        [AllowAnonymous]
        public async Task<GRPCReply<bool>> eNachAddResponse(GRPCRequest<eNachResponseDocDC> response)
        {
            string basePath = _hostingEnvironment.ContentRootPath;

            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();

            gRPCReply.Response = await _eNachHelper.eNachAddResponseDetails(response, basePath);

            return gRPCReply;
        }


    }
}
