using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.Extensions;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.Interfaces;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class ProductService : IProductService
    {

        private readonly IProductGrpcService _client;
        public ProductService(IProductGrpcService client)
        {
            _client = client;
        }


        public async Task<LeadProductReply> GetProductActivity(LeadProductRequest request)
        {
            var reply = await _client.GetProductActivity(request);
            return reply;
        }

        public async Task<LeadProductReply> GetNBFCCompanyProductActivity(LeadProductRequest request)
        {
            var reply = await _client.GetNBFCCompanyProductActivity(request);
            return reply;
        }

        public async Task<LeadProductReply> GetNBFCCompanyProductActivityByName(LeadProductRequest request)
        {
            var reply = await _client.GetNBFCCompanyProductActivityByName(request);
            return reply;
        }

        public async Task<string> GetProductName(long productId)
        {
            var reply = await _client.GetProductName(
                new GetProductRequest { ProductId = productId });

            return reply.ProductName;
        }

        public async Task<GetCompanyProductConfigReply> GetCompanyProductConfig(GetCompanyProductConfigRequest companyproductIdDTO)
        {
            var reply = await _client.GetCompanyProductConfig(companyproductIdDTO);

            return reply;
        }

        public async Task<LeadStatusByActivityReply> GetLeadStatus()
        {
            var reply = await _client.GetLeadStatus();
            return reply;
        }

        public async Task<GRPCReply<List<ActivitySubActivityNameReply>>> GetActivitySubActivityName(List<ActivitySubActivityNameRequest> request)
        {
            var reply = await _client.GetActivitySubActivityName(request);

            return reply;
        }

        public async Task<GRPCReply<OfferCompanyReply>> GetOfferCompany(long ProductId, List<long> CompanyIds, long? AnchorCompanyId)
        {
            GRPCReply<OfferCompanyReply> reply = new GRPCReply<OfferCompanyReply>();
            OfferCompanyRequest offerCompanyRequest = new OfferCompanyRequest { CompanyIds = CompanyIds, ProductId = ProductId, AnchorCompanyId = AnchorCompanyId };
            reply = await _client.GetOfferCompany(offerCompanyRequest);

            return reply;
        }

        public async Task<CompanyConfigReply> GetLeadCompanyConfig(CompanyConfigProdRequest request)
        {
            var reply = await _client.GetLeadCompanyConfig(request);
            return reply;
        }

        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            var reply = await _client.GetAuditLogs(request);
            return reply;
        }

        public async Task<GRPCReply<List<LeadNBFCSubActivityRequestDc>>> GetCompanyApiConfig(GRPCRequest<CompanyProductRequest> request)
        {
            var reply = await _client.GetCompanyApiConfig(request);

            return reply;
        }

        public async Task<GRPCReply<List<int>>> GetCompanyCreditDays(GRPCRequest<CreditDaysRequest> request)
        {
            var reply = await _client.GetCompanyCreditDays(request);
            return reply;
        }

        public async Task<GetCompanyProductConfigReply> GetAnchoreCompanyProductConfig(GetAnchoreProductConfigRequest request)
        {
            var reply = await _client.GetAnchoreCompanyProductConfig(request);
            return reply;
        }

        public async Task<GRPCReply<GetProductDataReply>> GetProductDataById(GRPCRequest<long> request)
        {
            var reply = await _client.GetProductDataById(request);
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

        public async Task<GRPCReply<List<CompanyApiReply>>> GetCompanyApiData()
        {
            var reply = await _client.GetCompanyApiData();
            return reply;
        }
        public async Task<GRPCReply<List<AnchorCompanyReply>>> GetAnchorComapnyData(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetAnchorComapnyData(request);
            return reply;
        }
        public async Task<NBFCCompanyConfigForProcessingFee> GetLeadNBFCCompanyConfig(NBFCConfigProdRequest request)
        {
            var reply = await _client.GetLeadNBFCCompanyConfig(request);
            return reply;
        }
        public async Task<GRPCReply<List<ProductListResponseDc>>> GetProductByProductType(GRPCRequest<string> request)
        {
            var reply = await _client.GetProductByProductType(request);
            return reply;
        }

        public async Task<GRPCReply<List<ProductListResponseDc>>> GetDSAProductByProductType(GRPCRequest<string> request)
        {
            var reply = await _client.GetDSAProductByProductType(request);
            return reply;
        }
        public async Task<GRPCReply<long>> GetProductIdByCode(GRPCRequest<string> request)
        {
            var reply = await _client.GetProductIdByCode(request);
            return reply;
        }

        public async Task<GRPCReply<string>> GetProductCodeById(GRPCRequest<long> request)
        {
            {
                var reply = await _client.GetProductCodeById(request);
                return reply;
            }
        }

        public async Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request)
        {
            var reply = await _client.AddUpdateAnchorProductConfig(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request)
        {
            var reply = await _client.AddUpdateNBFCProductConfig(request);
            return reply;
        }
        public async Task<GRPCReply<SalesAgentReply>> GetExistDSACompanyDetail(SaleAgentRequest request)
        {
            var reply = await _client.GetExistDSACompanyDetail(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> AddUpdateSalesAgent(GRPCRequest<SalesAgentRequest> request)
        {
            var reply = await _client.AddUpdateSalesAgent(request);
            return reply;
        }
        public async Task<GRPCReply<ProductDetailRespnose>> GetProductIdByCodeAndProductActivities(GRPCRequest<ProductdetailRequest> request)
        {
            var reply = await _client.GetProductIdByCodeAndProductActivities(request);
            return reply;
        }
        public async Task<GRPCReply<bool>> CreateDSAUser(GRPCRequest<CreateDSAUser> request)
        {
            var reply = await _client.CreateDSAUser(request);
            return reply;
        }

        public async Task<GRPCReply<eSignReply>> GeteSignAgreementData(GetesignRequest request)
        {
            var reply = await _client.GeteSignAgreementData(request);
            return reply;
        }

        
        public async Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetDSASalesAgentList(GRPCRequest<string> request)
        {
            var reply = await _client.GetDSASalesAgentList(request);
            return reply;
        }

        public async Task<GRPCReply<GetDSASalesAgentListResponseDc>> GetSalesAgentDetails(GRPCRequest<string> request)
        {
            var reply = await _client.GetSalesAgentDetails(request);
            return reply;
        }

        public async Task<GRPCReply<GetproductCommissionConfigDC>> GetCommissionConfigByProductId(GRPCRequest<List<long>> request)
        {
            var reply = await _client.GetCommissionConfigByProductId(request);
            return reply;
        }
        public async Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetSalesAgentDetailsByUserIds(GRPCRequest<List<string>> request)
        {
            var reply = await _client.GetSalesAgentDetailsByUserIds(request);
            return reply;
        }


        public async Task<GRPCReply<List<SalesAgentDetailDC>>> GetSalesAgentByUserId(GRPCRequest<List<string>> request)
        {
            var reply = await _client.GetSalesAgentByUserId(request);
            return reply;
        }
        public async Task<GRPCReply<List<string>>> GetUsersIDS(LeadListPageRequest req)
        {
            var reply = await _client.GetUsersIDS(req);
            return reply;
        }
        public async Task<GRPCReply<string>> GetSalesAgentNameByUserId(GRPCRequest<string> request)
        {
            var reply = await _client.GetSalesAgentNameByUserId(request);
            return reply;
        }
        public async Task<GRPCReply<List<long>>> GetNbfcCompaniesByProduct(GetCompanyByProductRequestDc request)
        {
            var reply = await _client.GetNbfcCompaniesByProduct(request);
            return reply;
        }
        public async Task<GRPCReply<List<ProductSlabConfigResponse>>> GetProductSlabConfigs(GRPCRequest<ProductSlabConfigRequest> request)
        {
            var reply = await _client.GetProductSlabConfigs(request);
            return reply;
        }
        public async Task<GRPCReply<bool>> DSASalesAgentActivationReject(GRPCRequest<ActivatDeActivateDSALeadRequest> request)
        {
            var reply = await _client.DSASalesAgentActivationReject(request);
            return reply;
        }

        public async Task<GRPCReply<List<GetProductNBFCConfigResponseDc>>> GetProductNBFCConfigs(GRPCRequest<GetProductNBFCConfigRequestDc> request)
        {
            var reply = await _client.GetProductNBFCConfigs(request);
            return reply;
        }
        public async Task<GRPCReply<bool>> GetSalesLeadActivationSatus(string request)
        {
            var reply = await _client.GetSalesLeadActivationSatus(request);
            return reply;
        }
        public async Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetALLDSASalesAgentList(GRPCRequest<string> request)
        {
            var reply = await _client.GetALLDSASalesAgentList(request);
            return reply;
        }

        public async Task<GRPCReply<List<long>>> GetCompanyListByProduct(GRPCRequest<GetCompanyListByProductRequest> request)
        {
            var reply = await _client.GetCompanyListByProduct(request);
            return reply;
        }
        public async Task<GRPCReply<List<long>>> GetAnchorCompaniesByProduct(GetCompanyByProductRequestDc request)
        {
            var reply = await _client.GetAnchorCompaniesByProduct(request);
            return reply;
        }

        public async Task<GRPCReply<string>> GetDSACurrentCode(GRPCRequest<DSAEntityDc> req)
        {
            var reply = await _client.GetDSACurrentCode(req);
            return reply;
        }

        public async Task<GRPCReply<List<BLNBFCConfigs>>> GetBLNBFCConfigByCompanyIds(GRPCRequest<List<long>> req)
        {
            var reply = await _client.GetBLNBFCConfigByCompanyIds(req);
            return reply;
        }

        public async Task<GRPCReply<string>> AddUpdateTokenNBFCCompany(AddUpdateTokenNBFCCompany request)
        {
            var reply = await _client.AddUpdateTokenNBFCCompany(request);
            return reply;
        }

        public async Task<GRPCReply<string>> GetNBFCProductToken(AddUpdateTokenNBFCCompany request)
        {
            var reply = await _client.GetNBFCProductToken(request);
            return reply;
        }
    }
}
