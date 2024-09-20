
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyLocationReply> CreateCompanyLocation(CompanyLocationDTO createCompanyRequest);
        Task<AddCompanyReply> AddCompany(AddCompanyDTO createCompanyDTO);
        Task<CompanyListReply> GetCompanyList(CompanyListRequest companyDc);
        Task<AddCompanyUserMappingReply> AddUpdateCompanyUserMapping(AddCompanyUserMappingRequest addCompanyUserMappingRequest);
        Task<UserIdsListResponse> GetUserList(UserIdsListResponseRequest companyIds);
        Task<GRPCReply<long>> GetFinTechCompany();
        Task<GRPCReply<List<LeadNbfcResponse>>> GetAllNBFCCompany();
        Task<GRPCReply<List<long>>> GetDefaultConfigNBFCCompany(GRPCRequest<List<long>> request);
        Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<GetEntityCodeRequest> EntityName);
        Task<GRPCReply<List<long>>> GetCompanyLocationById(GRPCRequest<long> CompanyId);
        Task<GRPCReply<double>> GetLatestGSTRate(string Gstcode);

        Task<GRPCReply<List<GstList>>> GetLatestGSTRateList(GRPCRequest<List<string>> Gstcodes);  

        Task<GRPCReply<CompanyDetail>> GetCompanyLogo(GRPCRequest<long> request);
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request);
        Task<GRPCReply<List<CompanyIdentificationCodeDc>>> GetCompanyIdentificationCode(GRPCRequest<List<long>> request);
        Task<GRPCReply<List<NBFCCompanyReply>>> GetNBFCCompanyById(GRPCRequest<List<long>> request);
        Task<GRPCReply<List<long>>> GetAllConfigNBFCCompany(GRPCRequest<List<long>> request);
        Task<GRPCReply<CompanyAddressAndDetailsResponse>> GetCompanyAddressAndDetails(CompanyAddressAndDetailsReq request);
        Task<GRPCReply<UpdateCompanyRequest>> UpdateCompanyAsync(UpdateCompanyRequest request);
        Task<GRPCReply<CompanyDataDc>> GetCompanyDataById(GRPCRequest<long> request);
        Task<GRPCReply<CompanyDataDc>> GetCompanyDataByCode(GRPCRequest<string> request);

        Task<GRPCReply<string>> GetCompanyCodeById(GRPCRequest<long> request);

        Task<GRPCReply<CompanyDetailDc>> GetCompany(GRPCRequest<long> request);
        Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync();
        Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request);
        Task<GRPCReply<GSTDetailReply>> GetGSTDetail(GRPCRequest<string> GSTNO);
        Task<GRPCReply<List<CompanySummaryReply>>> GetCompanySummary(GRPCRequest<CompanySummaryRequest> request);
        Task<GRPCReply<GetCustomerDetailReply>> GetCustomerDetails();
        Task<GRPCReply<string>> GetCompanyShortName(GRPCRequest<long> request);
        Task<GRPCReply<List<CompanyBankDetailsDc>>> GetCompanyBankDetailsById(GRPCRequest<List<long>> request);
        Task<GRPCReply<List<NBFCCompanyNameResponseDC>>> GetNBFCCompanyName();
        Task<GRPCReply<string>> GetFintechNBFCInvoiceUrl();
        Task<GRPCReply<string>> AddFinancialLiaisonDetails(AddfinancialLiaisonDetailsDTO request);
        Task<GRPCReply<FintechCompanyResponse>> GetFinTechCompanyDetail();
        Task<GRPCReply<List<GetCompanyDetailListResponse>>> GetCompanyDetailList(GRPCRequest<List<long>> request);

        Task<GRPCReply<bool>> GetGSTCompany(GRPCRequest<string> request);
        Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetAllCompanies();

    }
}
