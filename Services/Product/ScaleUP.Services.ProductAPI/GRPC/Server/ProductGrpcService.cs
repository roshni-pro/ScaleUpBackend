using Azure;
using Grpc.Core;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.Interfaces;
using ScaleUP.Services.ProductAPI.Manager;
using ScaleUP.Services.ProductAPI.Persistence;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ScaleUP.Services.ProductAPI.GRPC.Server
{
    [Authorize]
    public class ProductGrpcService : IProductGrpcService
    {
        private readonly ProductApplicationDbContext _context;
        private readonly ProductGrpcManager _ProductGrpcManager;

        public ProductGrpcService(ProductApplicationDbContext context)
        {
            _context = context;

            _ProductGrpcManager = new ProductGrpcManager(context);
        }

        [AllowAnonymous]
        public Task<GetCompanyProductConfigReply> GetCompanyProductConfig(GetCompanyProductConfigRequest request, CallContext context = default)
        {

            var response = AsyncContext.Run(() => _ProductGrpcManager.GetCompanyProductConfig(request));
            return Task.FromResult(response);

        }
        [AllowAnonymous]
        public Task<LeadProductReply> GetProductActivity(LeadProductRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _ProductGrpcManager.GetProductActivity(request));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<LeadProductReply> GetNBFCCompanyProductActivity(LeadProductRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _ProductGrpcManager.GetNBFCCompanyProductActivity(request));
            return Task.FromResult(response);
        }
        //[AllowAnonymous]
        public Task<LeadProductReply> GetNBFCCompanyProductActivityByName(LeadProductRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _ProductGrpcManager.GetNBFCCompanyProductActivityByName(request));
            return Task.FromResult(response);
        }

        public Task<GetProductReply> GetProductName(GetProductRequest request, CallContext context = default)
        {
            var response = _ProductGrpcManager.GetProductName(request);
            return Task.FromResult(response);

        }

        [AllowAnonymous]
        public Task<LeadStatusByActivityReply> GetLeadStatus(CallContext context = default)
        {
            var response = _ProductGrpcManager.GetLeadStatus();
            return Task.FromResult(response);

        }
        [AllowAnonymous]
        public Task<GRPCReply<List<ActivitySubActivityNameReply>>> GetActivitySubActivityName(List<ActivitySubActivityNameRequest> activitySubActivityNameRequest, CallContext context = default)
        {
            var response = _ProductGrpcManager.GetActivitySubActivityName(activitySubActivityNameRequest);
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public async Task<GRPCReply<OfferCompanyReply>> GetOfferCompany(OfferCompanyRequest offerCompanyRequest, CallContext context = default)
        {
            return await _ProductGrpcManager.GetOfferCompany(offerCompanyRequest);
            //return Task.FromResult(response);
        }

        [AllowAnonymous]//Job
        public Task<CompanyConfigReply> GetLeadCompanyConfig(CompanyConfigProdRequest request, CallContext context = default)
        {
            var reply = _ProductGrpcManager.GetLeadCompanyConfig(request);
            return reply;
        }

        public Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetAuditLogs(request));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<List<LeadNBFCSubActivityRequestDc>>> GetCompanyApiConfig(GRPCRequest<CompanyProductRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetCompanyApiConfig(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<int>>> GetCompanyCreditDays(GRPCRequest<CreditDaysRequest> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetCompanyCreditDays(request));
            return Task.FromResult(response);
        }

        [AllowAnonymous]
        public Task<GetCompanyProductConfigReply> GetAnchoreCompanyProductConfig(GetAnchoreProductConfigRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetAnchoreCompanyProductConfig(request));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<GetProductDataReply>> GetProductDataById(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetProductDataById(request));
            return Task.FromResult(response);
        }
        public Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync(CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetTemplateMasterAsync());
            return Task.FromResult(response);
        }

        public Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetTemplateById(request));
            return Task.FromResult(response);
        }

        [AllowAnonymous]
        public Task<GRPCReply<List<CompanyApiReply>>> GetCompanyApiData(CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetCompanyApiData());
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<GRPCReply<List<AnchorCompanyReply>>> GetAnchorComapnyData(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var response = AsyncContext.Run(async () => await _ProductGrpcManager.GetAnchorComapnyData(request));
            return Task.FromResult(response);
        }
        [AllowAnonymous]
        public Task<NBFCCompanyConfigForProcessingFee> GetLeadNBFCCompanyConfig(NBFCConfigProdRequest request, CallContext context = default)
        {
            var reply = _ProductGrpcManager.GetLeadNBFCCompanyConfig(request);
            return Task.FromResult(reply);
        }

        [AllowAnonymous]
        public Task<GRPCReply<List<ProductListResponseDc>>> GetProductByProductType(GRPCRequest<string> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetProductByProductType(request));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<long>> GetProductIdByCode(GRPCRequest<string> request)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetProductIdByCode(request));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<string>> GetProductCodeById(GRPCRequest<long> request)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetProductCodeById(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.AddUpdateAnchorProductConfig(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.AddUpdateNBFCProductConfig(request));
            return Task.FromResult(reply);
        }

        [AllowAnonymous]
        public Task<GRPCReply<SalesAgentReply>> GetExistDSACompanyDetail(SaleAgentRequest request, CallContext context = default)
        {
            var response = AsyncContext.Run(() => _ProductGrpcManager.GetExistDSACompanyDetail(request));
            return Task.FromResult(response);
        }

        public Task<GRPCReply<bool>> AddUpdateSalesAgent(GRPCRequest<SalesAgentRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.AddUpdateSalesAgent(request));
            return Task.FromResult(reply);
        }
        [AllowAnonymous]
        public Task<GRPCReply<ProductDetailRespnose>> GetProductIdByCodeAndProductActivities(GRPCRequest<ProductdetailRequest> request)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetProductIdByCodeAndProductActivities(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<bool>> CreateDSAUser(GRPCRequest<CreateDSAUser> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.CreateDSAUser(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<eSignReply>> GeteSignAgreementData(GetesignRequest request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GeteSignAgreementData(request));
            return Task.FromResult(reply);
        }
        
        public Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetDSASalesAgentList(GRPCRequest<string> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetDSASalesAgentList(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<GetDSASalesAgentListResponseDc>> GetSalesAgentDetails(GRPCRequest<string> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetSalesAgentDetails(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<GetproductCommissionConfigDC>> GetCommissionConfigByProductId(GRPCRequest<List<long>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetCommissionConfigByProductId(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<ProductListResponseDc>>> GetDSAProductByProductType(GRPCRequest<string> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetDSAProductByProductType(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetSalesAgentDetailsByUserIds(GRPCRequest<List<string>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetSalesAgentDetailsByUserIds(request));
            return Task.FromResult(reply);
        }

        [AllowAnonymous]
        public Task<GRPCReply<List<SalesAgentDetailDC>>> GetSalesAgentByUserId(GRPCRequest<List<string>> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetSalesAgentByUserId(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<string>>> GetUsersIDS(LeadListPageRequest req, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetUsersIDS(req));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<string>> GetSalesAgentNameByUserId(GRPCRequest<string> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetSalesAgentNameByUserId(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<ProductSlabConfigResponse>>> GetProductSlabConfigs(GRPCRequest<ProductSlabConfigRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetProductSlabConfigs(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<long>>> GetNbfcCompaniesByProduct(GetCompanyByProductRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetNbfcCompaniesByProduct(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<bool>> DSASalesAgentActivationReject(GRPCRequest<ActivatDeActivateDSALeadRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.DSADeactivate(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<GetProductNBFCConfigResponseDc>>> GetProductNBFCConfigs(GRPCRequest<GetProductNBFCConfigRequestDc> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetProductNBFCConfigs(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<bool>> GetSalesLeadActivationSatus(string request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetSalesLeadActivationSatus(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetALLDSASalesAgentList(GRPCRequest<string> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetALLDSASalesAgentList(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<List<long>>> GetCompanyListByProduct(GRPCRequest<GetCompanyListByProductRequest> request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetCompanyListByProduct(request));
            return Task.FromResult(reply);
        }
        public Task<GRPCReply<List<long>>> GetAnchorCompaniesByProduct(GetCompanyByProductRequestDc request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetAnchorCompaniesByProduct(request));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<string>> GetDSACurrentCode(GRPCRequest<DSAEntityDc> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetDSACurrentCode(req));
            return Task.FromResult(reply);
        }

        [AllowAnonymous]//Job
        public Task<GRPCReply<List<BLNBFCConfigs>>> GetBLNBFCConfigByCompanyIds(GRPCRequest<List<long>> req, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetBLNBFCConfigByCompanyIds(req));
            return Task.FromResult(reply);
        }

        public Task<GRPCReply<string>> AddUpdateTokenNBFCCompany(AddUpdateTokenNBFCCompany request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.AddUpdateTokenNBFCCompany(request));
            return Task.FromResult(reply);
        }
       // [AllowAnonymous]//Job
        public Task<GRPCReply<string>> GetNBFCProductToken(AddUpdateTokenNBFCCompany request, CallContext context = default)
        {
            var reply = AsyncContext.Run(async () => await _ProductGrpcManager.GetNBFCProductToken(request));
            return Task.FromResult(reply);
        }
    }
}
