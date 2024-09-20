using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using System.ServiceModel;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces
{
    [ServiceContract]
    public interface ICompanyGrpcService
    {
        [OperationContract]
        Task<AddCompanyReply> AddCompany(AddCompanyDTO request,
            CallContext context = default);

        [OperationContract]
        Task<CompanyLocationReply> CreateCompanyLocation(CompanyLocationDTO request,
            CallContext context = default);

        [OperationContract]
        Task<CompanyUserReply> CreateCompanyUser(CompanyUserRequest request,
           CallContext context = default);

        [OperationContract]
        Task<CompanyListReply> GetCompanyList(CompanyListRequest request,
            CallContext context = default);
        [OperationContract]
        Task<AddCompanyUserMappingReply> AddUpdateCompanyUserMapping(AddCompanyUserMappingRequest request,
            CallContext context = default);
        [OperationContract]
        Task<UserIdsListResponse> GetUserList(UserIdsListResponseRequest request,
            CallContext context = default);

        [OperationContract]
        Task<GRPCReply<long>> GetFinTechCompany(CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<LeadNbfcResponse>>> GetAllNBFCCompany(CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<long>>> GetDefaultConfigNBFCCompany(GRPCRequest<List<long>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<GetEntityCodeRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<long>>> GetCompanyLocationById(GRPCRequest<long> CompanyId, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<double>> GetLatestGSTRate(string Gstcode,   CallContext context = default);
        Task<GRPCReply<List<GstList>>> GetLatestGSTRateList(GRPCRequest<List<string>> Gstcodes, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<CompanyDetail>> GetCompanyLogo(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<CompanyIdentificationCodeDc>>> GetCompanyIdentificationCode(GRPCRequest<List<long>> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<NBFCCompanyReply>>> GetNBFCCompanyById(GRPCRequest<List<long>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<long>>> GetAllConfigNBFCCompany(GRPCRequest<List<long>> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<CompanyAddressAndDetailsResponse>> GetCompanyAddressAndDetails(CompanyAddressAndDetailsReq request, CallContext context = default);


        [OperationContract]
        Task<GRPCReply<UpdateCompanyRequest>> UpdateCompanyAsync(UpdateCompanyRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<CompanyDataDc>> GetCompanyDataById(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<CompanyDataDc>> GetCompanyDataByCode(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GetCompanyCodeById(GRPCRequest<long> request);


        [OperationContract]
        Task<GRPCReply<CompanyDetailDc>> GetCompany(GRPCRequest<long> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync(CallContext context = default);

        [OperationContract]
        Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<GSTDetailReply>> GetGSTDetail(GRPCRequest<string> GSTNO, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<CompanySummaryReply>>> GetCompanySummary(GRPCRequest<CompanySummaryRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<GetCustomerDetailReply>> GetCustomerDetails(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GetCompanyShortName(GRPCRequest<long> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<CompanyBankDetailsDc>>> GetCompanyBankDetailsById(GRPCRequest<List<long>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<NBFCCompanyNameResponseDC>>> GetNBFCCompanyName(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GetFintechNBFCInvoiceUrl( CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> AddFinancialLiaisonDetails(AddfinancialLiaisonDetailsDTO request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<FintechCompanyResponse>> GetFinTechCompanyDetail(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetCompanyDetailListResponse>>> GetCompanyDetailList(GRPCRequest<List<long>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> GetGSTCompany(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetAllCompanies(CallContext context = default);

    }
}
