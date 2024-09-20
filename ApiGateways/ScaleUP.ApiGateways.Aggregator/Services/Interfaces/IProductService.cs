using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System.ServiceModel;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface IProductService
    {
        Task<string> GetProductName(long productId);
        Task<LeadProductReply> GetProductActivity(LeadProductRequest request);
        Task<LeadProductReply> GetNBFCCompanyProductActivity(LeadProductRequest request);
        Task<LeadProductReply> GetNBFCCompanyProductActivityByName(LeadProductRequest request);
        Task<GetCompanyProductConfigReply> GetCompanyProductConfig(GetCompanyProductConfigRequest companyproductDTO);
        Task<LeadStatusByActivityReply> GetLeadStatus();

        Task<GRPCReply<List<ActivitySubActivityNameReply>>> GetActivitySubActivityName(List<ActivitySubActivityNameRequest> activitySubActivityNameRequest);

        Task<GRPCReply<OfferCompanyReply>> GetOfferCompany(long ProductId, List<long> CompanyIds, long? AnchorCompanyId);

        Task<CompanyConfigReply> GetLeadCompanyConfig(CompanyConfigProdRequest request);
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request);

        Task<GRPCReply<List<LeadNBFCSubActivityRequestDc>>> GetCompanyApiConfig(GRPCRequest<CompanyProductRequest> request);
        Task<GRPCReply<List<int>>> GetCompanyCreditDays(GRPCRequest<CreditDaysRequest> request);
        Task<GetCompanyProductConfigReply> GetAnchoreCompanyProductConfig(GetAnchoreProductConfigRequest request);
        Task<GRPCReply<GetProductDataReply>> GetProductDataById(GRPCRequest<long> request);
        Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync();
        Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request);
        Task<GRPCReply<List<CompanyApiReply>>> GetCompanyApiData();
        Task<GRPCReply<List<AnchorCompanyReply>>> GetAnchorComapnyData(GRPCRequest<List<long>> request);
        Task<NBFCCompanyConfigForProcessingFee> GetLeadNBFCCompanyConfig(NBFCConfigProdRequest request);
        Task<GRPCReply<List<ProductListResponseDc>>> GetProductByProductType(GRPCRequest<string> request);
        Task<GRPCReply<List<ProductListResponseDc>>> GetDSAProductByProductType(GRPCRequest<string> request);
        Task<GRPCReply<long>> GetProductIdByCode(GRPCRequest<string> request);
        Task<GRPCReply<string>> GetProductCodeById(GRPCRequest<long> request);

        Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request);
        Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request);
        Task<GRPCReply<SalesAgentReply>> GetExistDSACompanyDetail(SaleAgentRequest request);
        Task<GRPCReply<bool>> AddUpdateSalesAgent(GRPCRequest<SalesAgentRequest> request);
        Task<GRPCReply<ProductDetailRespnose>> GetProductIdByCodeAndProductActivities(GRPCRequest<ProductdetailRequest> request);
        Task<GRPCReply<bool>> CreateDSAUser(GRPCRequest<CreateDSAUser> request);

        Task<GRPCReply<eSignReply>> GeteSignAgreementData(GetesignRequest request);
        Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetDSASalesAgentList(GRPCRequest<string> request);
        Task<GRPCReply<GetDSASalesAgentListResponseDc>> GetSalesAgentDetails(GRPCRequest<string> request);
        Task<GRPCReply<GetproductCommissionConfigDC>> GetCommissionConfigByProductId(GRPCRequest<List<long>> request);
        Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetSalesAgentDetailsByUserIds(GRPCRequest<List<string>> request);
        Task<GRPCReply<List<SalesAgentDetailDC>>> GetSalesAgentByUserId(GRPCRequest<List<string>> request);
        Task<GRPCReply<List<string>>> GetUsersIDS(LeadListPageRequest req);
        Task<GRPCReply<string>> GetSalesAgentNameByUserId(GRPCRequest<string> request);
        Task<GRPCReply<List<ProductSlabConfigResponse>>> GetProductSlabConfigs(GRPCRequest<ProductSlabConfigRequest> request);
        Task<GRPCReply<List<long>>> GetNbfcCompaniesByProduct(GetCompanyByProductRequestDc request);
        Task<GRPCReply<bool>> DSASalesAgentActivationReject(GRPCRequest<ActivatDeActivateDSALeadRequest> request);
        Task<GRPCReply<List<GetProductNBFCConfigResponseDc>>> GetProductNBFCConfigs(GRPCRequest<GetProductNBFCConfigRequestDc> request);
        Task<GRPCReply<bool>> GetSalesLeadActivationSatus(string UserName);
        Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetALLDSASalesAgentList(GRPCRequest<string> request);
        Task<GRPCReply<List<long>>> GetCompanyListByProduct(GRPCRequest<GetCompanyListByProductRequest> request);
        Task<GRPCReply<List<long>>> GetAnchorCompaniesByProduct(GetCompanyByProductRequestDc request);
        Task<GRPCReply<string>> GetDSACurrentCode(GRPCRequest<DSAEntityDc> req);
        Task<GRPCReply<List<BLNBFCConfigs>>> GetBLNBFCConfigByCompanyIds(GRPCRequest<List<long>> req);
        Task<GRPCReply<string>> AddUpdateTokenNBFCCompany(AddUpdateTokenNBFCCompany request);
        Task<GRPCReply<string>> GetNBFCProductToken(AddUpdateTokenNBFCCompany request);
    }
}
