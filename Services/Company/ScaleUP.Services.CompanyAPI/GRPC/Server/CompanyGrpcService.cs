using Microsoft.AspNetCore.Authorization;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.Services.CompanyAPI.Manager;


namespace ScaleUP.Services.CompanyAPI.GRPC.Server
{
    public class CompanyGrpcService : ICompanyGrpcService
    {
        private readonly CompanyGrpcManager _companyGrpcManager;
        public CompanyGrpcService(CompanyGrpcManager companyGrpcManager)
        {
            _companyGrpcManager = companyGrpcManager;
        }

        public Task<AddCompanyReply> AddCompany(AddCompanyDTO request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.AddCompany(request));
            return Task.FromResult(response);

        }

        public Task<CompanyLocationReply> CreateCompanyLocation(CompanyLocationDTO request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.CreateCompanyLocation(request));
            return Task.FromResult(response);
        }

        public Task<CompanyUserReply> CreateCompanyUser(CompanyUserRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.CreateCompanyUser(request));
            return Task.FromResult(response);
        }

        public Task<CompanyListReply> GetCompanyList(CompanyListRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanyList(request));
            return Task.FromResult(response);
        }
        public Task<UserIdsListResponse> GetUserList(UserIdsListResponseRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetUserList(request));
            return Task.FromResult(response);
        }
        public Task<AddCompanyUserMappingReply> AddUpdateCompanyUserMapping(AddCompanyUserMappingRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.AddUpdateCompanyUserMapping(request));
            return Task.FromResult(response);
        }

        [AllowAnonymous]//Job
        public Task<GRPCReply<long>> GetFinTechCompany(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetFinTechCompany());
            return Task.FromResult(response);
        }

        [AllowAnonymous]//Job
        public Task<GRPCReply<List<LeadNbfcResponse>>> GetAllNBFCCompany(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetAllNBFCCompany());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<long>>> GetDefaultConfigNBFCCompany(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetDefaultConfigNBFCCompany(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<long>>> GetAllConfigNBFCCompany(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetAllConfigNBFCCompany(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<GetEntityCodeRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _companyGrpcManager.GetCurrentNumber(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<long>>> GetCompanyLocationById(GRPCRequest<long> CompanyId, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _companyGrpcManager.GetCompanyLocationById(CompanyId));
            return Task.FromResult(response);
        }

        [AllowAnonymous]//Job
        public Task<GRPCReply<double>> GetLatestGSTRate(string Gstcode ,  CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _companyGrpcManager.GetLatestGSTRate(Gstcode));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<GstList>>> GetLatestGSTRateList(GRPCRequest<List<string>> Gstcodes, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _companyGrpcManager.GetLatestGSTRateList(Gstcodes));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<CompanyDetail>> GetCompanyLogo(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _companyGrpcManager.GetCompanyLogo(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _companyGrpcManager.GetAuditLogs(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<CompanyIdentificationCodeDc>>> GetCompanyIdentificationCode(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _companyGrpcManager.GetCompanyIdentificationCode(request));
            return Task.FromResult(response);
        }


        public Task<GRPCReply<List<NBFCCompanyReply>>> GetNBFCCompanyById(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetNBFCCompanyById(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<CompanyAddressAndDetailsResponse>> GetCompanyAddressAndDetails(CompanyAddressAndDetailsReq request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanyAddressAndDetails(request));
            return Task.FromResult(response);
        }

        [AllowAnonymous]
        public Task<GRPCReply<CompanyDataDc>> GetCompanyDataById(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanyDataById(request));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<CompanyDataDc>> GetCompanyDataByCode(GRPCRequest<string> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanyDataByCode(request));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<string>> GetCompanyCodeById(GRPCRequest<long> request)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanyCodeById(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<UpdateCompanyRequest>> UpdateCompanyAsync(UpdateCompanyRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.UpdateCompanyAsync(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<CompanyDetailDc>> GetCompany(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompany(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync( CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetTemplateMasterAsync());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request,CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetTemplateById(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<GSTDetailReply>> GetGSTDetail(GRPCRequest<string> GSTNO, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetGSTDetail(GSTNO));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<CompanySummaryReply>>> GetCompanySummary(GRPCRequest<CompanySummaryRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanySummary(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<GetCustomerDetailReply>> GetCustomerDetails(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCustomerDetails());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<string>> GetCompanyShortName(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanyShortName(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<CompanyBankDetailsDc>>> GetCompanyBankDetailsById(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanyBankDetailsById(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<NBFCCompanyNameResponseDC>>> GetNBFCCompanyName(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetNBFCCompanyName());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<string>> GetFintechNBFCInvoiceUrl(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetFintechNBFCInvoiceUrl());
            return Task.FromResult(response);
        }
        public Task<GRPCReply<string>> AddFinancialLiaisonDetails(AddfinancialLiaisonDetailsDTO request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.AddFinancialLiaisonDetails(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<FintechCompanyResponse>> GetFinTechCompanyDetail(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetFinTechCompanyDetail());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<GetCompanyDetailListResponse>>> GetCompanyDetailList(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetCompanyDetailList(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<bool>> GetGSTCompany(GRPCRequest<string> request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetGSTCompany(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetAllCompanies(CallContext context = default)
        {
            var response = AsyncContext.Run(() => _companyGrpcManager.GetAllCompanies());
            return Task.FromResult(response);
        }
    }
}
