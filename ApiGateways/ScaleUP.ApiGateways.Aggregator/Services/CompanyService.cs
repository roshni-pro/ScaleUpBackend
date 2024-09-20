using MassTransit;
using Microsoft.Net.Http.Headers;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.Extensions;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using System.ServiceModel;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class CompanyService : ICompanyService
    {
        private IConfiguration Configuration;
        private readonly ICompanyGrpcService _client;
        public CompanyService(IConfiguration _configuration
            , ICompanyGrpcService client
            )
        {
            Configuration = _configuration;
            _client = client;
        }

        public async Task<AddCompanyReply> AddCompany(AddCompanyDTO createCompanyDTO)
        {
            var reply = await _client.AddCompany(createCompanyDTO);
            return reply;
        }

        public async Task<CompanyLocationReply> CreateCompanyLocation(CompanyLocationDTO createCompanyRequest)
        {
            var reply = await _client.CreateCompanyLocation(new CompanyLocationDTO
            {
                CompanyId = createCompanyRequest.CompanyId,
                LocationId = createCompanyRequest.LocationId,
            });
            return reply;
        }

        public async Task<CompanyListReply> GetCompanyList(CompanyListRequest companyDc)
        {
            var reply = await _client.GetCompanyList(companyDc);

            return reply;
        }
        public async Task<UserIdsListResponse> GetUserList(UserIdsListResponseRequest compRequest)
        {
            var reply = await _client.GetUserList(compRequest);

            return reply;
        }
        public async Task<AddCompanyUserMappingReply> AddUpdateCompanyUserMapping(AddCompanyUserMappingRequest addCompanyUserMappingRequest)
        {
            var reply = await _client.AddUpdateCompanyUserMapping(addCompanyUserMappingRequest);

            return reply;
        }

        public async Task<GRPCReply<long>> GetFinTechCompany()
        {
            var reply = await _client.GetFinTechCompany();

            return reply;

        }

        public async Task<GRPCReply<List<LeadNbfcResponse>>> GetAllNBFCCompany()
        {
            var reply = await _client.GetAllNBFCCompany();
            return reply;
        }

        public async Task<GRPCReply<List<long>>> GetDefaultConfigNBFCCompany(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetDefaultConfigNBFCCompany(request);
            return reply;
        }
        public async Task<GRPCReply<List<long>>> GetAllConfigNBFCCompany(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetAllConfigNBFCCompany(request);
            return reply;
        }
        public async Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<GetEntityCodeRequest> request)
        {
            var reply = await _client.GetCurrentNumber(request);
            return reply;
        }

        public async Task<GRPCReply<List<long>>> GetCompanyLocationById(GRPCRequest<long> CompanyId)
        {
            var reply = await _client.GetCompanyLocationById(CompanyId);
            return reply;
        }

        public async Task<GRPCReply<double>> GetLatestGSTRate(string Gstcode)
        {
            var reply = await _client.GetLatestGSTRate(Gstcode);
            return reply;
        }

        public async Task<GRPCReply<List<GstList>>> GetLatestGSTRateList(GRPCRequest<List<string>> Gstcodes)
        {
            var reply = await _client.GetLatestGSTRateList(Gstcodes);
            return reply;
        }

        public async Task<GRPCReply<CompanyDetail>> GetCompanyLogo(GRPCRequest<long> request)
        {
            var reply = await _client.GetCompanyLogo(request);
            return reply;
        }

        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            var reply = await _client.GetAuditLogs(request);
            return reply;
        }

        public async Task<GRPCReply<List<CompanyIdentificationCodeDc>>> GetCompanyIdentificationCode(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetCompanyIdentificationCode(request);
            return reply;
        }


        public async Task<GRPCReply<List<NBFCCompanyReply>>> GetNBFCCompanyById(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetNBFCCompanyById(request);
            return reply;
        }

        public async Task<GRPCReply<CompanyAddressAndDetailsResponse>> GetCompanyAddressAndDetails(CompanyAddressAndDetailsReq request)
        {
            var reply = await _client.GetCompanyAddressAndDetails(request);

            return reply;
        }
        public async Task<GRPCReply<UpdateCompanyRequest>> UpdateCompanyAsync(UpdateCompanyRequest request)
        {
            var reply = await _client.UpdateCompanyAsync(request);

            return reply;
        }

        public async Task<GRPCReply<CompanyDataDc>> GetCompanyDataById(GRPCRequest<long> request)
        {
            var reply = await _client.GetCompanyDataById(request);
            return reply;
        }

        public async Task<GRPCReply<CompanyDataDc>> GetCompanyDataByCode(GRPCRequest<string> request)
        {
            var reply = await _client.GetCompanyDataByCode(request);
            return reply;
        }

        public async Task<GRPCReply<string>> GetCompanyCodeById(GRPCRequest<long> request)
        {
            var reply = await _client.GetCompanyCodeById(request);
            return reply;
        }
        public async Task<GRPCReply<CompanyDetailDc>> GetCompany(GRPCRequest<long> request)
        {
            var reply = await _client.GetCompany(request);
            return reply;
        }
        public async Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync()
        {
            var reply = await _client.GetTemplateMasterAsync();
            return reply;
        }

        public async Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request)
        {
            var reply = await _client.GetTemplateById(request);
            return reply;
        }

        public async Task<GRPCReply<GSTDetailReply>> GetGSTDetail(GRPCRequest<string> GSTNO)
        {
            var reply = await _client.GetGSTDetail(GSTNO);
            return reply;
        }
        public async Task<GRPCReply<List<CompanySummaryReply>>> GetCompanySummary(GRPCRequest<CompanySummaryRequest> request)
        {
            var reply = await _client.GetCompanySummary(request);
            return reply;
        }
        public async Task<GRPCReply<GetCustomerDetailReply>> GetCustomerDetails()
        {
            var reply = await _client.GetCustomerDetails();
            return reply;
        }

        public async Task<GRPCReply<string>> GetCompanyShortName(GRPCRequest<long> request) {
            var reply = await _client.GetCompanyShortName(request);
            return reply;
        }

        public async Task<GRPCReply<List<CompanyBankDetailsDc>>> GetCompanyBankDetailsById(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetCompanyBankDetailsById(request);
            return reply;
        }
        public async Task<GRPCReply<List<NBFCCompanyNameResponseDC>>> GetNBFCCompanyName()
        {
            var reply = await _client.GetNBFCCompanyName();
            return reply;
        }
        public async Task<GRPCReply<string>> GetFintechNBFCInvoiceUrl()
        {
            var reply = await _client.GetFintechNBFCInvoiceUrl();
            return reply;
        }
        public async Task<GRPCReply<string>> AddFinancialLiaisonDetails(AddfinancialLiaisonDetailsDTO request)
        {
            var reply = await _client.AddFinancialLiaisonDetails(request);
            return reply;
        }
        public async Task<GRPCReply<FintechCompanyResponse>> GetFinTechCompanyDetail()
        {
            var reply = await _client.GetFinTechCompanyDetail();
            return reply;

        }

        public async Task<GRPCReply<List<GetCompanyDetailListResponse>>> GetCompanyDetailList(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetCompanyDetailList(request);
            return reply;
        }
        public async Task<GRPCReply<bool>> GetGSTCompany(GRPCRequest<string> request)
        {
            var reply = await _client.GetGSTCompany(request);
            return reply;
        }

        public async Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetAllCompanies()
        {
            var reply = await _client.GetAllCompanies();
            return reply;
        }
    }
}
