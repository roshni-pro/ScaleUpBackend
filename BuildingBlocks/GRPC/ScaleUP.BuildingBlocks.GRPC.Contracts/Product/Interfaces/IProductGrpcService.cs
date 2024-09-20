using Grpc.Core;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System.ServiceModel;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.Interfaces
{
    [ServiceContract]
    public interface IProductGrpcService
    {
        [OperationContract]
        Task<GetProductReply> GetProductName(GetProductRequest request,
            CallContext context = default);

        [OperationContract]
        Task<LeadProductReply> GetProductActivity(LeadProductRequest request,
            CallContext context = default);

        [OperationContract]
        Task<GetCompanyProductConfigReply> GetCompanyProductConfig(GetCompanyProductConfigRequest request,
            CallContext context = default);

        [OperationContract]
        Task<LeadStatusByActivityReply> GetLeadStatus(CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<ActivitySubActivityNameReply>>> GetActivitySubActivityName(List<ActivitySubActivityNameRequest> activitySubActivityNameRequest, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<OfferCompanyReply>> GetOfferCompany(OfferCompanyRequest offerCompanyRequests, CallContext context = default);
        [OperationContract]
        Task<CompanyConfigReply> GetLeadCompanyConfig(CompanyConfigProdRequest request, CallContext context = default);
        [OperationContract]
        Task<LeadProductReply> GetNBFCCompanyProductActivity(LeadProductRequest request, CallContext context = default);

        [OperationContract]
        Task<LeadProductReply> GetNBFCCompanyProductActivityByName(LeadProductRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<LeadNBFCSubActivityRequestDc>>> GetCompanyApiConfig(GRPCRequest<CompanyProductRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<int>>> GetCompanyCreditDays(GRPCRequest<CreditDaysRequest> request, CallContext context = default);

        [OperationContract]
        Task<GetCompanyProductConfigReply> GetAnchoreCompanyProductConfig(GetAnchoreProductConfigRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<GetProductDataReply>> GetProductDataById(GRPCRequest<long> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync(CallContext context = default);

        [OperationContract]
        Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<CompanyApiReply>>> GetCompanyApiData(CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<AnchorCompanyReply>>> GetAnchorComapnyData(GRPCRequest<List<long>> request, CallContext context = default);
        [OperationContract]
        Task<NBFCCompanyConfigForProcessingFee> GetLeadNBFCCompanyConfig(NBFCConfigProdRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<ProductListResponseDc>>> GetProductByProductType(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<long>> GetProductIdByCode(GRPCRequest<string> request);
        [OperationContract]
        Task<GRPCReply<string>> GetProductCodeById(GRPCRequest<long> request);
        [OperationContract]
        Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<SalesAgentReply>> GetExistDSACompanyDetail(SaleAgentRequest request,
           CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> AddUpdateSalesAgent(GRPCRequest<SalesAgentRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<ProductDetailRespnose>> GetProductIdByCodeAndProductActivities(GRPCRequest<ProductdetailRequest> request);

        [OperationContract]
        Task<GRPCReply<bool>> CreateDSAUser(GRPCRequest<CreateDSAUser> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<eSignReply>> GeteSignAgreementData(GetesignRequest request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetDSASalesAgentList(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<GetDSASalesAgentListResponseDc>> GetSalesAgentDetails(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<GetproductCommissionConfigDC>> GetCommissionConfigByProductId(GRPCRequest<List<long>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<ProductListResponseDc>>> GetDSAProductByProductType(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetSalesAgentDetailsByUserIds(GRPCRequest<List<string>> request, CallContext context = default);


        [OperationContract]
        Task<GRPCReply<List<SalesAgentDetailDC>>> GetSalesAgentByUserId(GRPCRequest<List<string>> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<string>>> GetUsersIDS(LeadListPageRequest req, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> GetSalesAgentNameByUserId(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<ProductSlabConfigResponse>>> GetProductSlabConfigs(GRPCRequest<ProductSlabConfigRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<long>>> GetNbfcCompaniesByProduct(GetCompanyByProductRequestDc request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> DSASalesAgentActivationReject(GRPCRequest<ActivatDeActivateDSALeadRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetProductNBFCConfigResponseDc>>> GetProductNBFCConfigs(GRPCRequest<GetProductNBFCConfigRequestDc> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> GetSalesLeadActivationSatus(string request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetALLDSASalesAgentList(GRPCRequest<string> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<long>>> GetCompanyListByProduct(GRPCRequest<GetCompanyListByProductRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<long>>> GetAnchorCompaniesByProduct(GetCompanyByProductRequestDc request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<string>> GetDSACurrentCode(GRPCRequest<DSAEntityDc> req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<List<BLNBFCConfigs>>> GetBLNBFCConfigByCompanyIds(GRPCRequest<List<long>> req, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> AddUpdateTokenNBFCCompany(AddUpdateTokenNBFCCompany req, CallContext context = default);
         [OperationContract]
        Task<GRPCReply<string>> GetNBFCProductToken(AddUpdateTokenNBFCCompany req, CallContext context = default);

    }
}
