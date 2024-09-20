using MassTransit;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Services.ProductAPI.Helpers;
using ScaleUP.Services.ProductAPI.Persistence;
using ScaleUP.Services.ProductDTO.Master;
using ScaleUP.Services.ProductModels.DSA;
using ScaleUP.Services.ProductModels.Master;
using System.Linq;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using System.Data;
using ScaleUP.Services.ProductAPI.Migrations;
using System.Collections.Generic;
using NuGet.Packaging;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.Services.ProductDTO.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using MassTransit.Internals;


namespace ScaleUP.Services.ProductAPI.Manager
{
    public class ProductGrpcManager
    {
        private readonly ProductApplicationDbContext _context;

        public ProductGrpcManager(ProductApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<GetCompanyProductConfigReply> GetCompanyProductConfig(GetCompanyProductConfigRequest request)
        {
            GetCompanyProductConfigReply getCompanyProductConfigReply = new GetCompanyProductConfigReply();
            getCompanyProductConfigReply.Status = false;

            if (request.CompanyIds != null && request.CompanyIds.Any())
            {
                var list = new List<GetCompanyProductConfig>();
                //var products = await _context.Products.Where(x => x.IsActive && !x.IsDeleted).Select(x => x).ToListAsync();
                if (request.CompanyType == "NBFC")
                {
                    list = await _context.ProductNBFCCompany.Where(x => x.Product.IsActive && !x.Product.IsDeleted && x.IsActive && !x.IsDeleted && request.CompanyIds.Contains(x.CompanyId)).Include(x => x.Product)
                     .Select(x => new GetCompanyProductConfig
                     {
                         ProductCompanyId = x.Id,
                         CompanyId = x.CompanyId,
                         ProcessingFeeType = x.ProcessingFeeType,
                         ProcessingFee = x.ProcessingFee,
                         AnnualInterestRate = x.AnnualInterestRate,
                         DelayPenaltyFee = x.PenaltyCharges,
                         BounceCharges = x.BounceCharges,
                         PlateFormFee = x.PlatformFee,
                         ProductId = x.ProductId,
                         AgreementDocId = x.AgreementDocId,
                         AgreementEndDate = x.AgreementEndDate,
                         AgreementStartDate = x.AgreementStartDate,
                         AgreementUrl = x.AgreementURL,
                         ProductName = x.Product.Name
                     }
                     ).AsNoTracking().ToListAsync();
                }
                else
                {
                    var anchorCompanyconfigs = await _context.ProductAnchorCompany.Where(x => x.Product.IsActive && !x.Product.IsDeleted && x.IsActive && !x.IsDeleted && request.CompanyIds.Contains(x.CompanyId)).Include(x => x.Product).AsNoTracking().ToListAsync();
                    list = anchorCompanyconfigs.Select(x => new GetCompanyProductConfig
                    {
                        ProductCompanyId = x.Id,
                        CompanyId = x.CompanyId,
                        ProductId = x.ProductId,
                        DelayPenaltyFee = x.DelayPenaltyRate,
                        BounceCharges = x.BounceCharge,
                        ProcessingFeeType = x.ProcessingFeeType,
                        ProcessingFee = x.ProcessingFeeRate,
                        AnnualInterestRate = x.AnnualInterestRate ?? 0,
                        AnnualInterestPayableBy = x.AnnualInterestPayableBy,
                        AgreementDocId = x.AgreementDocId,
                        AgreementEndDate = x.AgreementEndDate,
                        AgreementStartDate = x.AgreementStartDate,
                        AgreementUrl = x.AgreementURL,
                        ProductName = x.Product.Name
                    }
                     ).ToList();

                    var prodAnchoreIds = list.Select(x => x.ProductCompanyId).ToList();
                    var creditMasterIds = await _context.CompanyCreditDays.Where(x => prodAnchoreIds.Contains(x.ProductAnchorCompanyId)).Select(x => new { x.ProductAnchorCompanyId, x.CreditDaysMasterId }).ToListAsync();
                    var CompanyCreditDays = creditMasterIds.GroupBy(x => x.ProductAnchorCompanyId).Select(x => new { ProductAnchorCompanyId = x.Key, creditMasterIds = x.Select(y => y.CreditDaysMasterId).ToList(), creditDays = new List<int>() }).ToList();
                    if (CompanyCreditDays != null && CompanyCreditDays.Any())
                    {
                        foreach (var item in CompanyCreditDays)
                        {
                            var anchorProduct = anchorCompanyconfigs.FirstOrDefault(x => x.Id == item.ProductAnchorCompanyId);
                            List<int> creditDays = new List<int>();
                            var days = await _context.CreditDayMasters.Where(x => item.creditMasterIds.Contains(x.Id)).ToListAsync();
                            creditDays = days.Where(x => !x.Name.ToLower().Contains("custom")).Select(x => x.Days).ToList();
                            if (days.Any(x => x.Name.ToLower().Contains("custom")) && anchorProduct.CustomCreditDays != null)
                            {
                                if (creditDays == null) { creditDays = new List<int>(); }
                                creditDays.Add((int)anchorProduct.CustomCreditDays);
                            }
                            var anchorcompany = list.FirstOrDefault(s => s.ProductCompanyId == item.ProductAnchorCompanyId);
                            anchorcompany.CreditDays = creditDays;
                        }

                    }
                }

                if (list != null && list.Any())
                {
                    //list.ForEach(x => x.ProductName = products.Any(y => y.Id == x.ProductId) ? products.First(y => y.Id == x.ProductId).Name : "");
                    getCompanyProductConfigReply.Status = true;
                    getCompanyProductConfigReply.GetCompanyProductConfigList = list;
                }
                else
                {
                    getCompanyProductConfigReply.Status = false;
                }
            }

            return getCompanyProductConfigReply;

        }
        public async Task<LeadProductReply> GetProductActivity(LeadProductRequest request)
        {
            LeadProductReply leadProductReply = new LeadProductReply();
            var prod = await _context.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId && x.IsActive && !x.IsDeleted);
            if (prod != null)
            {
                leadProductReply.ProductType = prod.Type;
                if (await _context.ProductAnchorCompany.AnyAsync(x => x.ProductId == request.ProductId && x.CompanyId == request.AnchorCompanyId && x.IsActive && !x.IsDeleted))
                {
                    List<ProductCompanyActivityMasters> productActivityMasters = new List<ProductCompanyActivityMasters>();
                    if (request.CompanyType == CompanyTypeEnum.All.ToString())
                        productActivityMasters = _context.ProductCompanyActivityMasters.Where(x => x.ProductId == request.ProductId
                                 && x.CompanyId == request.CompanyId && x.IsActive && !x.IsDeleted && x.ActivityMasters.FrontOrBack == "Front").Include(x => x.ActivityMasters).Include(x => x.SubActivityMasters).AsNoTracking().ToList();
                    else
                        productActivityMasters = _context.ProductCompanyActivityMasters.Where(x => x.ProductId == request.ProductId
                            && x.CompanyId == request.CompanyId && x.IsActive && !x.IsDeleted && x.ActivityMasters.FrontOrBack == "Front" && x.ActivityMasters.CompanyType == request.CompanyType).Include(x => x.ActivityMasters).Include(x => x.SubActivityMasters).AsNoTracking().ToList();

                    leadProductReply.Status = true;
                    leadProductReply.Message = "Get product activity successfully.";
                    if (productActivityMasters.Any())
                    {
                        int i = 1;
                        List<GRPCLeadProductActivity> grpcLeadProdActivities = new List<GRPCLeadProductActivity>();
                        var ProdActivities = productActivityMasters.Select(x => new
                        {
                            ActivityMasterId = x.ActivityMasterId,
                            ActivityName = x.ActivityMasters.ActivityName,
                            ProductId = x.ProductId,
                            A_Sequence = x.Sequence,
                            //S_Sequence = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.Sequence : 1,
                            KycMasterCode = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.KycMasterCode : null,
                            SubActivityMasterId = x.SubActivityMasterId,
                            SubActivityName = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.Name : ""
                        });
                        foreach (var x in ProdActivities.OrderBy(x => x.A_Sequence))
                        {
                            grpcLeadProdActivities.Add(new GRPCLeadProductActivity
                            {
                                ActivityMasterId = x.ActivityMasterId,
                                ActivityName = x.ActivityName,
                                ProductId = x.ProductId,
                                Sequence = i,
                                KycMasterCode = x.KycMasterCode,
                                SubActivityMasterId = x.SubActivityMasterId,
                                SubActivityName = x.SubActivityName
                            });
                            i++;
                        }
                        leadProductReply.LeadProductActivity = grpcLeadProdActivities;
                    }
                }
                else
                {
                    leadProductReply.Status = false;
                    leadProductReply.Message = "Requested product not associate with company.";
                }
            }
            else
            {
                leadProductReply.Status = false;
                leadProductReply.Message = "Requested product not available.";
            }

            return leadProductReply;
        }

        public async Task<LeadProductReply> GetNBFCCompanyProductActivity(LeadProductRequest request)
        {
            LeadProductReply leadProductReply = new LeadProductReply();

            var productActivityMasters = _context.ProductCompanyActivityMasters.Where(x => x.ProductId == request.ProductId
                        && x.CompanyId == request.CompanyId && x.IsActive && !x.IsDeleted && x.ActivityMasters.FrontOrBack == "Front"
                        && x.ActivityMasters.CompanyType == request.CompanyType && x.ActivityMasters.ActivityName != ActivityConstants.PFCollection).Include(x => x.ActivityMasters).Include(x => x.SubActivityMasters).AsNoTracking().ToList();
            leadProductReply.Status = true;
            leadProductReply.Message = "Get product activity successfully.";
            if (productActivityMasters.Any())
            {
                int i = 1;
                List<GRPCLeadProductActivity> grpcLeadProdActivities = new List<GRPCLeadProductActivity>();
                var ProdActivities = productActivityMasters.Select(x => new
                {
                    ActivityMasterId = x.ActivityMasterId,
                    ActivityName = x.ActivityMasters.ActivityName,
                    ProductId = x.ProductId,
                    A_Sequence = x.Sequence,
                    //S_Sequence = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.Sequence : 1,
                    KycMasterCode = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.KycMasterCode : null,
                    SubActivityMasterId = x.SubActivityMasterId,
                    SubActivityName = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.Name : ""
                });
                foreach (var x in ProdActivities.OrderBy(x => x.A_Sequence))
                {
                    grpcLeadProdActivities.Add(new GRPCLeadProductActivity
                    {
                        ActivityMasterId = x.ActivityMasterId,
                        ActivityName = x.ActivityName,
                        ProductId = x.ProductId,
                        Sequence = i,
                        KycMasterCode = x.KycMasterCode,
                        SubActivityMasterId = x.SubActivityMasterId,
                        SubActivityName = x.SubActivityName
                    });
                    i++;
                }
                leadProductReply.LeadProductActivity = grpcLeadProdActivities;
            }

            return leadProductReply;
        }
        public async Task<LeadProductReply> GetNBFCCompanyProductActivityByName(LeadProductRequest request)
        {
            LeadProductReply leadProductReply = new LeadProductReply();
            var anchorProduct = await _context.ProductAnchorCompany.FirstOrDefaultAsync(x => x.CompanyId == request.AnchorCompanyId && x.ProductId == request.ProductId
                                && x.IsActive && !x.IsDeleted);
            if (anchorProduct != null && anchorProduct.ProcessingFeePayableBy != "Anchor")
            {
                var productActivityMasters = _context.ProductCompanyActivityMasters.Where(x => x.ProductId == request.ProductId
                            && x.CompanyId == request.CompanyId && x.IsActive && !x.IsDeleted && x.ActivityMasters.FrontOrBack == "Front"
                            && x.ActivityMasters.CompanyType == request.CompanyType && x.ActivityMasters.ActivityName == request.ActivityName).Include(x => x.ActivityMasters).Include(x => x.SubActivityMasters).AsNoTracking().FirstOrDefault();
                if (productActivityMasters != null)
                {
                    leadProductReply.Status = true;
                    leadProductReply.Message = "Get product activity successfully.";
                    List<GRPCLeadProductActivity> grpcLeadProdActivities = new List<GRPCLeadProductActivity>();
                    grpcLeadProdActivities.Add(new GRPCLeadProductActivity
                    {
                        ActivityMasterId = productActivityMasters.ActivityMasterId,
                        ActivityName = productActivityMasters.ActivityMasters.ActivityName,
                        ProductId = productActivityMasters.ProductId,
                        Sequence = productActivityMasters.Sequence,
                        KycMasterCode = productActivityMasters.SubActivityMasters.KycMasterCode,
                        SubActivityMasterId = productActivityMasters.SubActivityMasterId,
                        SubActivityName = productActivityMasters.SubActivityMasters.Name
                    });
                    leadProductReply.LeadProductActivity = grpcLeadProdActivities;
                }
            }
            return leadProductReply;
        }
        public GetProductReply GetProductName(GetProductRequest request)
        {
            //var httpContext = context.ServerCallContext.GetHttpContext();

            var prod = _context.Products.FirstOrDefault(x => x.Id == request.ProductId);
            prod.Description = "CreditLine";
            _context.Entry(prod).State = EntityState.Modified;
            _context.SaveChanges();
            return
                new GetProductReply
                {
                    ProductName = "Product 1"
                };
        }

        public LeadStatusByActivityReply GetLeadStatus()
        {
            LeadStatusByActivityReply leadStatusByActivityReply = new LeadStatusByActivityReply();

            var query = (from am in _context.ActivityMasters
                         join sm in _context.SubActivityMasters on am.Id equals sm.ActivityMasterId into subActivities
                         from subActivity in subActivities.DefaultIfEmpty()
                         select new LeadStatusActivityList
                         {
                             ActivityId = am.Id,
                             ActivityName = am.ActivityName,
                             SubActivityId = (int?)subActivity.Id,
                             SubActivityName = subActivity.Name,
                             ActivityMasterId = subActivity.ActivityMasterId
                         }).AsNoTracking().ToList();


            List<LeadStatusActivityList> leadStatusActivityList = new List<LeadStatusActivityList>();
            leadStatusActivityList.AddRange(query);

            leadStatusByActivityReply.Status = true;
            leadStatusByActivityReply.Message = " ";
            //leadStatusActivityList.Add(new LeadStatusActivityList
            //{
            //    ActivityId =1,
            //    ActivityMasterId =1,
            //    ActivityName="aaa",
            //    SubActivityId =1,
            //    SubActivityName = "sdds"
            //});
            leadStatusByActivityReply.LeadStatusActivityList = leadStatusActivityList;
            return leadStatusByActivityReply;
        }

        public GRPCReply<List<ActivitySubActivityNameReply>> GetActivitySubActivityName(List<ActivitySubActivityNameRequest> activitySubActivityNameRequest)
        {
            GRPCReply<List<ActivitySubActivityNameReply>> reply = new GRPCReply<List<ActivitySubActivityNameReply>>();
            List<long> activityIds = activitySubActivityNameRequest.Select(x => x.ActivityId).ToList();
            List<long?> subActivityIds = activitySubActivityNameRequest.Select(x => x.SubActivityId).ToList();

            reply.Response = (from am in _context.ActivityMasters
                              join sm in _context.SubActivityMasters.Where(x => subActivityIds.Contains(x.Id)) on am.Id equals sm.ActivityMasterId into subActivities
                              from subActivity in subActivities.DefaultIfEmpty()
                              select new ActivitySubActivityNameReply
                              {
                                  ActivityId = am.Id,
                                  ActivityName = string.IsNullOrEmpty(am.ActivityName) ? "" : am.ActivityName,
                                  SubActivityId = (int?)subActivity.Id,
                                  SubActivityName = string.IsNullOrEmpty(subActivity.Name) ? "" : subActivity.Name,
                              }).Where(x => activityIds.Contains(x.ActivityId)).AsNoTracking().ToList();
            reply.Status = true;
            return reply;
        }

        public async Task<GRPCReply<OfferCompanyReply>> GetOfferCompany(OfferCompanyRequest offerCompanyRequest)
        {
            GRPCReply<OfferCompanyReply> reply = new GRPCReply<OfferCompanyReply>();

            var result = new OfferCompanyReply();
            //int min = 0;
            //if (offerCompanyRequest.AnchorCompanyId.HasValue)
            //{
            //var anchorProdConfig = _context.ProductAnchorCompany.FirstOrDefault(x => x.IsActive && x.Product.Type == "BusinessLoan" && !x.IsDeleted && x.CompanyId == offerCompanyRequest.AnchorCompanyId.Value && x.ProductId == offerCompanyRequest.ProductId);
            //if (anchorProdConfig != null)
            //{
            //    //interest = anchorProdConfig.mi
            //}
            // }
            result.CompanyIds = _context.ProductNBFCCompany.Where(x => x.IsActive && !x.IsDeleted && x.ProductId == offerCompanyRequest.ProductId && offerCompanyRequest.CompanyIds.Contains(x.CompanyId)).Select(x => x.CompanyId).ToList();
            GRPCRequest<CompanyProductRequest> request = new GRPCRequest<CompanyProductRequest> { Request = new CompanyProductRequest { CompanyIds = offerCompanyRequest.CompanyIds, ProductId = offerCompanyRequest.ProductId, AnchorCompanyId = offerCompanyRequest.AnchorCompanyId ?? 0 } };
            var companyApiConfig = await GetCompanyApiConfig(request);
            result.LeadNBFCSubActivity = companyApiConfig.Response;

            reply.Response = result;
            reply.Status = true;

            return reply;

        }

        public async Task<CompanyConfigReply> GetLeadCompanyConfig(CompanyConfigProdRequest request)
        {
            CompanyConfigReply companyConfigReply = new CompanyConfigReply();
            var nbfcConfig = _context.ProductNBFCCompany.FirstOrDefault(x => x.IsActive && !x.IsDeleted && x.CompanyId == request.NBFCCompanyId && x.ProductId == request.ProductId);
            var anchorConfig = _context.ProductAnchorCompany.FirstOrDefault(x => x.IsActive && !x.IsDeleted && x.CompanyId == request.AnchorCompanyId && x.ProductId == request.ProductId);
            var ProductSlabConfigRes = await GetProductSlabConfigs(new GRPCRequest<ProductSlabConfigRequest> { Request = new ProductSlabConfigRequest { CompanyId = request.AnchorCompanyId, ProductId = request.ProductId } });
            var NBFCSlabConfigRes = await GetProductSlabConfigs(new GRPCRequest<ProductSlabConfigRequest> { Request = new ProductSlabConfigRequest { CompanyId = request.NBFCCompanyId, ProductId = request.ProductId } });


            if (nbfcConfig != null)
            {
                companyConfigReply.NBFCCompanyConfig = new NBFCCompanyConfig
                {
                    ProductNBFCCompanyId = nbfcConfig.Id,
                    BounceCharges = nbfcConfig.BounceCharges,
                    CompanyId = request.NBFCCompanyId,
                    InterestRate = nbfcConfig.AnnualInterestRate,
                    PenaltyCharges = nbfcConfig.PenaltyCharges,
                    PlatformFee = nbfcConfig.PlatformFee,
                    ProcessingFeeType = nbfcConfig.ProcessingFeeType,
                    ProcessingFee = nbfcConfig.ProcessingFee,
                    CustomerAgreementDocId = Convert.ToInt64(nbfcConfig.CustomerAgreementDocId),
                    CustomerAgreementURL = nbfcConfig.CustomerAgreementURL ?? "",
                    CustomerAgreementType = nbfcConfig.CustomerAgreementType,
                    DisbursementType = nbfcConfig.DisbursementType ?? DisbursementTypeEnum.FullDisbursement.ToString(),
                    Tenure = nbfcConfig.Tenure,
                    ProductId = nbfcConfig.ProductId,
                    IsInterestRateCoSharing = nbfcConfig.IsInterestRateCoSharing ?? false,
                    IsBounceChargeCoSharing = nbfcConfig.IsBounceChargeCoSharing ?? false,
                    IsPenaltyChargeCoSharing = nbfcConfig.IsPenaltyChargeCoSharing ?? false
                };
            }
            else
            {
                companyConfigReply.NBFCCompanyConfig = new NBFCCompanyConfig
                {
                    ProductNBFCCompanyId = 0,
                    BounceCharges = 0,
                    CompanyId = request.NBFCCompanyId,
                    InterestRate = 0,
                    PenaltyCharges = 0,
                    PlatformFee = 0,
                    ProcessingFee = 0,
                    Tenure = 0,
                    ProductId = 0
                };
            }

            if (anchorConfig != null)
            {
                companyConfigReply.AnchorCompanyConfig = new AnchorCompanyConfig
                {
                    ProductAnchorCompanyId = anchorConfig.Id,
                    AnnualInterestRate = anchorConfig.AnnualInterestRate,
                    BounceCharge = anchorConfig.BounceCharge,
                    CommissionPayout = anchorConfig.CommissionPayout,
                    CompanyId = request.AnchorCompanyId,
                    ConsiderationFee = anchorConfig.ConsiderationFee,
                    //CreditDays = anchorConfig.CreditDays,
                    DelayPenaltyRate = anchorConfig.DelayPenaltyRate,
                    DisbursementSharingCommission = anchorConfig.DisbursementSharingCommission,
                    DisbursementTAT = anchorConfig.DisbursementTAT,
                    EMIBounceCharge = anchorConfig.EMIBounceCharge,
                    EMIPenaltyRate = anchorConfig.EMIPenaltyRate,
                    EMIProcessingFeeRate = anchorConfig.EMIProcessingFeeRate,
                    EMIRate = anchorConfig.EMIRate,
                    MaxLoanAmount = anchorConfig.MaxLoanAmount,
                    MaxTenureInMonth = anchorConfig.MaxTenureInMonth,
                    MinLoanAmount = anchorConfig.MinLoanAmount,
                    MinTenureInMonth = anchorConfig.MinTenureInMonth,
                    ProcessingFeePayableBy = anchorConfig.ProcessingFeePayableBy,
                    ProcessingFeeRate = anchorConfig.ProcessingFeeRate,
                    ProcessingFeeType = anchorConfig.ProcessingFeeType,
                    AnnualInterestPayableBy = anchorConfig.AnnualInterestPayableBy,
                    //TransactionFeeRate = anchorConfig.TransactionFeeRate,
                    //AnnualInterestFeeType = anchorConfig.AnnualInterestFeeType,
                    AgreementEndDate = anchorConfig.AgreementEndDate,
                    AgreementStartDate = anchorConfig.AgreementStartDate,
                    AgreementURL = anchorConfig.AgreementURL,
                    AgreementDocId = anchorConfig.AgreementDocId,
                    MaxInterestRate = anchorConfig.MaxInterestRate,
                    IseSignEnable = anchorConfig.IseSignEnable,
                    PlatFormFee = anchorConfig.PlatFormFee,
                    ProductId = anchorConfig.ProductId
                };
            }
            else
            {
                companyConfigReply.AnchorCompanyConfig = new AnchorCompanyConfig
                {
                    ProductAnchorCompanyId = 0,
                    AnnualInterestRate = 0,
                    BounceCharge = 0,
                    CommissionPayout = 0,
                    CompanyId = request.AnchorCompanyId,
                    ConsiderationFee = 0,
                    CreditDays = 0,
                    DelayPenaltyRate = 0,
                    DisbursementSharingCommission = 0,
                    DisbursementTAT = 0,
                    EMIBounceCharge = 0,
                    EMIPenaltyRate = 0,
                    EMIProcessingFeeRate = 0,
                    EMIRate = 0,
                    MaxLoanAmount = 0,
                    MaxTenureInMonth = 0,
                    MinLoanAmount = 0,
                    MinTenureInMonth = 0,
                    ProcessingFeePayableBy = "",
                    ProcessingFeeRate = 0,
                    ProcessingFeeType = "",
                    AnnualInterestPayableBy = "",
                    //TransactionFeeRate = 0,
                    //TransactionFeeType = ""
                    MaxInterestRate = 0,
                    IseSignEnable = false,
                    PlatFormFee = 0,
                    ProductId = 0
                };
            }
            if (ProductSlabConfigRes != null && ProductSlabConfigRes.Response.Any())
            {
                companyConfigReply.AnchorSlabConfigs = ProductSlabConfigRes.Response;
            }
            else
            {
                companyConfigReply.AnchorSlabConfigs = new List<ProductSlabConfigResponse>();
            }
            if (NBFCSlabConfigRes != null && NBFCSlabConfigRes.Response.Any())
            {
                companyConfigReply.AnchorSlabConfigs = NBFCSlabConfigRes.Response;
            }
            else
            {
                companyConfigReply.NBFCSlabConfigs = new List<ProductSlabConfigResponse>();
            }
            return companyConfigReply;
        }

        //new for eSign

        public async Task<GRPCReply<eSignReply>> GeteSignAgreementData(GetesignRequest request)
        {
            GRPCReply<eSignReply> eSignReply = new GRPCReply<eSignReply> { Message = "failed" };
            //var nbfcConfig = _context.ProductNBFCCompany.FirstOrDefault(x => x.IsActive && !x.IsDeleted && x.CompanyId == request.NBFCCompanyId && x.ProductId == request.ProductId);

            var eSignAgreementData = await _context.ProductCommissionConfigs.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && !string.IsNullOrEmpty(x.DSAAgreement) && !string.IsNullOrEmpty(x.ConnectorAgreement));

            if (eSignAgreementData != null)
            {
                if (request.AgreementType == DSAProfileTypeConstants.DSA)
                {
                    eSignReply.Response = new eSignReply { AgreementURL = eSignAgreementData.DSAAgreement };
                    eSignReply.Message = "Success";
                    eSignReply.Status = true;

                }
                if (request.AgreementType == DSAProfileTypeConstants.Connector)
                {
                    eSignReply.Response = new eSignReply { AgreementURL = eSignAgreementData.ConnectorAgreement };
                    eSignReply.Message = "Success";
                    eSignReply.Status = true;
                }
            }
            else
            {
                eSignReply.Message = "Failed";
                eSignReply.Status = false;
            }
            return eSignReply;
        }


        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            GRPCReply<List<AuditLogReply>> gRPCReply = new GRPCReply<List<AuditLogReply>>();
            AuditLogHelper auditLogHelper = new AuditLogHelper(_context);
            var auditLogs = await auditLogHelper.GetAuditLogs(request.Request.EntityId, request.Request.EntityName, request.Request.Skip, request.Request.Take);
            if (auditLogs != null && auditLogs.Any())
            {
                gRPCReply.Status = true;
                gRPCReply.Response = auditLogs.Select(x => new AuditLogReply
                {
                    ModifiedDate = x.Timestamp,
                    Changes = x.Changes,
                    ModifiedBy = x.UserId,
                    TotalRecords = x.TotalRecords,
                    ActionType = x.Action
                }).ToList();
                gRPCReply.Message = "Data Found";
            }
            else
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Not Found";
            }
            return gRPCReply;
        }


        public async Task<GRPCReply<List<LeadNBFCSubActivityRequestDc>>> GetCompanyApiConfig(GRPCRequest<CompanyProductRequest> request)
        {
            GRPCReply<List<LeadNBFCSubActivityRequestDc>> reply = new GRPCReply<List<LeadNBFCSubActivityRequestDc>>();

            var _ProductAnchorCompany = await _context.ProductAnchorCompany.Where(x => x.CompanyId == request.Request.AnchorCompanyId && x.ProductId == request.Request.ProductId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();


            var query = from m in _context.ProductCompanyActivityMasters
                        join r in request.Request.CompanyIds on new { cid = m.CompanyId, pid = m.ProductId } equals new { cid = r, pid = request.Request.ProductId }
                        join c in request.Request.CompanyIds
                           on m.CompanyId equals c
                        join a in _context.ActivityMasters
                           on m.ActivityMasterId equals a.Id
                        join s in _context.SubActivityMasters
                            on m.SubActivityMasterId equals s.Id into ps_jointable
                        from p in ps_jointable.DefaultIfEmpty()
                        where a.IsActive && !a.IsDeleted
                            && m.IsActive && !m.IsDeleted
                            && (p == null || p.IsActive) && (p == null || !p.IsDeleted)
                            && (a.ActivityName == ActivityConstants.GenerateOffer || a.ActivityName == ActivityConstants.Agreement || a.ActivityName == ActivityConstants.PFCollection)
                        select new LeadNBFCSubActivityRequestDc
                        {
                            Code = p == null ? "" : p.Name,
                            ActivityName = a.ActivityName,
                            ActivityMasterId = a.Id,
                            IdentificationCode = "",
                            LeadId = 0,
                            NBFCCompanyId = c,
                            Status = LeadNBFCSubActivityConstants.NotStarted,
                            SubActivityMasterId = p == null ? 0 : p.Id,
                            SubActivitySequence = p == null ? 0 : p.Sequence,
                            NBFCCompanyApiList = null,
                            ProductCompanyActivityMasterId = m.Id,
                            ProductId = m.ProductId
                        };

            reply.Response = query.ToList();
            if (reply.Response != null && reply.Response.Any())
            {

                var q2 = from r in reply.Response
                         join api in _context.NBFCSubActivityApis
                            on r.ProductCompanyActivityMasterId equals api.ProductCompanyActivityMasterId
                         join cpi in _context.CompanyApis
                            on api.NBFCCompanyApiId equals cpi.Id
                         join p in _context.ProductNBFCCompany
                            on new { pid = r.ProductId, cid = r.NBFCCompanyId } equals new { pid = p.ProductId, cid = p.CompanyId }
                         where api.IsActive && !api.IsDeleted
                         && cpi.IsActive && !cpi.IsDeleted
                         && p.IsActive && !p.IsDeleted

                         select new LeadNBFCApiDc
                         {
                             APIUrl = cpi.APIUrl,
                             Code = cpi.Code,
                             CompanyApiId = cpi.Id,
                             Sequence = api.Sequence,
                             Status = LeadNBFCApiConstants.NotStarted,
                             TReferralCode = (_ProductAnchorCompany != null && !string.IsNullOrEmpty(_ProductAnchorCompany.BlackSoilReferralCode)) ? _ProductAnchorCompany.BlackSoilReferralCode : p.TReferralCode,
                             TAPISecretKey = p.TAPISecretKey,
                             TAPIKey = p.TAPIKey,
                             ProductCompanyActivityMasterId = r.ProductCompanyActivityMasterId

                         };
                var q2Result = q2.ToList();

                if (q2Result != null && q2Result.Any())
                {
                    reply.Status = true;
                    foreach (var item in reply.Response)
                    {
                        item.NBFCCompanyApiList = q2Result.Where(x => x.ProductCompanyActivityMasterId == item.ProductCompanyActivityMasterId).OrderBy(x => x.Sequence).ToList();
                    }
                }

            }
            #region Old

            //    var query = from p in _context.ProductNBFCCompany
            //            join m in _context.ProductCompanyActivityMasters
            //               on p.CompanyId equals m.CompanyId
            //            join c in request.Request.CompanyIds
            //               on m.CompanyId equals c
            //            join a in _context.ActivityMasters
            //               on m.ActivityMasterId equals a.Id
            //            join s in _context.SubActivityMasters
            //                on m.SubActivityMasterId equals s.Id into ps_jointable
            //            from j in ps_jointable.DefaultIfEmpty()
            //            where a.IsActive && !a.IsDeleted && m.IsActive && !m.IsDeleted && p.IsActive && !p.IsDeleted && p.ProductId == request.Request.ProductId
            //            select new
            //            {
            //                ProductCompanyActivityMasterId = m.Id,
            //                ActivityMasterId = a.Id,
            //                ActivityName = a.ActivityName,
            //                SubActivityMasterId = j == null ? 0 : j.Id,
            //                SubActivityName = j == null ? "" : j.Name,
            //                SubActivitySequence = j == null ? 0 : j.Sequence,
            //                CompanyId = c,
            //                TAPIKey = p.TAPIKey,
            //                TAPISecretKey = p.TAPISecretKey,
            //                TReferralCode = p.TReferralCode
            //            };
            //var activityData = query.ToList();
            //if (activityData!=null && activityData.Any())
            //{
            //    reply.Response = await _context.CompanyApis.Where(x => request.Request.CompanyIds.Contains(x.CompanyId) && x.IsActive && !x.IsDeleted).Include(x => x.nBFCSubActivityApis).Select(x => new LeadNBFCSubActivityRequestDc
            //    {
            //        NBFCCompanyId = x.CompanyId,
            //        ActivityMasterId = x.nBFCSubActivityApis != null && x.nBFCSubActivityApis.Any() ? x.nBFCSubActivityApis.FirstOrDefault().ActivityMasterId : 0,
            //        SubActivityMasterId = x.nBFCSubActivityApis != null && x.nBFCSubActivityApis.Any() ? x.nBFCSubActivityApis.FirstOrDefault().SubActivityMasterId : 0,
            //        Status = LeadNBFCSubActivityConstants.NotStarted,
            //        Code = "",
            //        ActivityName = "",
            //        IdentificationCode = "",
            //        SubActivitySequence = 0,
            //        NBFCCompanyApiList = x.nBFCSubActivityApis != null && x.nBFCSubActivityApis.Any() ? x.nBFCSubActivityApis.Select(z => new LeadNBFCApiDc
            //        {
            //            APIUrl = x.APIUrl,
            //            Code = x.Code,
            //            CompanyApiId = z.NBFCCompanyApiId,
            //            Sequence = z.Sequence,
            //            Status = LeadNBFCApiConstants.NotStarted,
            //            TAPIKey = "",
            //            TAPISecretKey = ""
            //        }).ToList() : new List<LeadNBFCApiDc>()
            //    }).ToListAsync();

            //    if (reply.Response != null && reply.Response.Any())
            //    {
            //        foreach (var item in reply.Response)
            //        {
            //            item.Code = activityData.Any(x => x.SubActivityMasterId == item.SubActivityMasterId) ? activityData.FirstOrDefault(x => x.SubActivityMasterId == item.SubActivityMasterId).SubActivityName : "";
            //            item.SubActivitySequence = activityData.Any(x => x.SubActivityMasterId == item.SubActivityMasterId) ? activityData.FirstOrDefault(x => x.SubActivityMasterId == item.SubActivityMasterId).SubActivitySequence : 0;
            //            item.ActivityName = activityData.Any(x => x.ActivityMasterId == item.ActivityMasterId) ? activityData.FirstOrDefault(x => x.ActivityMasterId == item.ActivityMasterId).ActivityName : "";
            //            if (item.NBFCCompanyApiList != null && item.NBFCCompanyApiList.Any())
            //            {
            //                foreach (var child in item.NBFCCompanyApiList)
            //                {
            //                    child.TAPIKey = activityData.Any(y => y.CompanyId == item.NBFCCompanyId) ? activityData.FirstOrDefault(y => y.CompanyId == item.NBFCCompanyId).TAPIKey : "";
            //                    child.TAPISecretKey = activityData.Any(y => y.CompanyId == item.NBFCCompanyId) ? activityData.FirstOrDefault(y => y.CompanyId == item.NBFCCompanyId).TAPISecretKey : "";
            //                    child.TReferralCode = activityData.Any(x => x.CompanyId == item.NBFCCompanyId) ? activityData.FirstOrDefault(x => x.CompanyId == item.NBFCCompanyId).TReferralCode : "";
            //                }
            //            }
            //        }

            //        reply.Response = reply.Response.GroupBy(x => new { x.ActivityMasterId, x.Code, x.NBFCCompanyId, x.SubActivityMasterId }).Select(x => new LeadNBFCSubActivityRequestDc
            //        {
            //            ActivityMasterId = x.Key.ActivityMasterId,
            //            Code = x.Key.Code,
            //            NBFCCompanyId = x.Key.NBFCCompanyId,
            //            SubActivityMasterId = x.Key.SubActivityMasterId,
            //            ActivityName = x.FirstOrDefault().ActivityName,
            //            SubActivitySequence = x.FirstOrDefault().SubActivitySequence,
            //            Status = x.FirstOrDefault().Status,
            //            IdentificationCode = x.FirstOrDefault().IdentificationCode,
            //            LeadId = x.FirstOrDefault().LeadId,
            //            NBFCCompanyApiList = x.SelectMany(y => y.NBFCCompanyApiList).GroupBy(x => new { x.Code, x.APIUrl, x.Sequence }).Select(y => new LeadNBFCApiDc
            //            {
            //                APIUrl = y.Key.APIUrl,
            //                Code = y.Key.Code,
            //                CompanyApiId = y.FirstOrDefault().CompanyApiId,
            //                Sequence = y.Key.Sequence,
            //                Status = y.FirstOrDefault().Status,
            //                TAPIKey = y.FirstOrDefault().TAPIKey,
            //                TAPISecretKey = y.FirstOrDefault().TAPISecretKey,
            //                TReferralCode = y.FirstOrDefault().TReferralCode
            //            }).Distinct().ToList()
            //        }).ToList();

            //        reply.Status = true;
            //        reply.Message = "Data Found";
            //    }
            //    else
            //    {
            //        reply.Status = false;
            //        reply.Message = "Data Not Found";
            //    }
            //}
            //else
            //{
            //    reply.Status = false;
            //    reply.Message = "Data Not Found";
            //}
            #endregion
            return reply;
        }
        public async Task<GRPCReply<List<int>>> GetCompanyCreditDays(GRPCRequest<CreditDaysRequest> request)
        {
            GRPCReply<List<int>> reply = new GRPCReply<List<int>>();
            var anchorProduct = await _context.ProductAnchorCompany.FirstOrDefaultAsync(x => x.CompanyId == request.Request.CompanyId && x.ProductId == request.Request.ProductId && x.IsActive && !x.IsDeleted);
            if (anchorProduct != null)
            {
                var creditMasterIds = await _context.CompanyCreditDays.Where(x => x.ProductAnchorCompanyId == anchorProduct.Id).Select(x => x.CreditDaysMasterId).ToListAsync();
                if (creditMasterIds != null && creditMasterIds.Any())
                {
                    List<int> creditDays = new List<int>();
                    var days = await _context.CreditDayMasters.Where(x => creditMasterIds.Contains(x.Id)).ToListAsync();
                    creditDays = days.Where(x => !x.Name.ToLower().Contains("custom")).Select(x => x.Days).ToList();
                    if (days.Any(x => x.Name.ToLower().Contains("custom")) && anchorProduct.CustomCreditDays != null)
                    {
                        if (creditDays == null) { creditDays = new List<int>(); }
                        creditDays.Add((int)anchorProduct.CustomCreditDays);
                    }
                    reply.Status = true;
                    reply.Message = "Data Found";
                    reply.Response = creditDays;
                }
                else
                {
                    reply.Status = false;
                    reply.Message = "Credit Days Not Found!!!";
                }
            }
            else
            {
                reply.Status = false;
                reply.Message = "Anchor Product Not Found!!!";
            }
            return reply;
        }


        public async Task<GetCompanyProductConfigReply> GetAnchoreCompanyProductConfig(GetAnchoreProductConfigRequest request)
        {
            GetCompanyProductConfigReply getCompanyProductConfigReply = new GetCompanyProductConfigReply();
            getCompanyProductConfigReply.Status = false;

            if (request.CompanyId != null && request.CompanyId > 0)
            {
                GetCompanyProductConfig anchorcompany = null;

                var anchorCompanyconfigs = await _context.ProductAnchorCompany.Where(x => x.IsActive && !x.IsDeleted && x.ProductId == request.ProductId
                                                    && request.CompanyId == x.CompanyId).AsNoTracking().FirstOrDefaultAsync();
                if (anchorCompanyconfigs != null)
                {
                    anchorcompany = new GetCompanyProductConfig
                    {
                        ProductCompanyId = anchorCompanyconfigs.Id,
                        CompanyId = anchorCompanyconfigs.CompanyId,
                        ProductId = anchorCompanyconfigs.ProductId,
                        //ConvenienceFee = anchorCompanyconfigs.TransactionFeeRate ?? 0,
                        DelayPenaltyFee = anchorCompanyconfigs.DelayPenaltyRate,
                        BounceCharges = anchorCompanyconfigs.BounceCharge,
                        ProcessingFeeType = anchorCompanyconfigs.ProcessingFeeType,
                        ProcessingFee = anchorCompanyconfigs.ProcessingFeeRate,
                        AnnualInterestRate = anchorCompanyconfigs.AnnualInterestRate ?? 0,
                        AnnualInterestPayableBy = anchorCompanyconfigs.AnnualInterestPayableBy,
                        //TransactionFeeRate = anchorCompanyconfigs.TransactionFeeRate,
                        //TransactionFeeType = anchorCompanyconfigs.TransactionFeeType
                    };


                    var prodAnchoreId = anchorcompany.ProductCompanyId;
                    var creditMasterIds = await _context.CompanyCreditDays.Where(x => prodAnchoreId == x.ProductAnchorCompanyId && x.IsActive && !x.IsDeleted).Select(x => new { x.ProductAnchorCompanyId, x.CreditDaysMasterId }).ToListAsync();
                    var CompanyCreditDays = creditMasterIds.GroupBy(x => x.ProductAnchorCompanyId).Select(x => new { ProductAnchorCompanyId = x.Key, creditMasterIds = x.Select(y => y.CreditDaysMasterId).ToList(), creditDays = new List<int>() }).ToList();
                    if (CompanyCreditDays != null && CompanyCreditDays.Any())
                    {
                        foreach (var item in CompanyCreditDays)
                        {
                            var anchorProduct = anchorCompanyconfigs;
                            List<int> creditDays = new List<int>();
                            var days = await _context.CreditDayMasters.Where(x => item.creditMasterIds.Contains(x.Id)).ToListAsync();
                            creditDays = days.Where(x => !x.Name.ToLower().Contains("custom")).Select(x => x.Days).ToList();
                            if (days.Any(x => x.Name.ToLower().Contains("custom")) && anchorProduct.CustomCreditDays != null)
                            {
                                if (creditDays == null) { creditDays = new List<int>(); }
                                creditDays.Add((int)anchorProduct.CustomCreditDays);
                            }
                            anchorcompany.CreditDays = creditDays;
                        }

                    }

                    getCompanyProductConfigReply.Status = true;
                    getCompanyProductConfigReply.GetCompanyProductConfigList = new List<GetCompanyProductConfig> { anchorcompany };
                }
                else
                {
                    getCompanyProductConfigReply.Status = false;
                }
            }

            return getCompanyProductConfigReply;

        }

        public async Task<GRPCReply<GetProductDataReply>> GetProductDataById(GRPCRequest<long> request)
        {
            GRPCReply<GetProductDataReply> reply = new GRPCReply<GetProductDataReply>();
            var product = await _context.Products.FirstOrDefaultAsync(x => request.Request == x.Id && x.IsActive && !x.IsDeleted);
            if (product != null)
            {
                reply.Status = true;
                reply.Response = new GetProductDataReply { ProductName = product.Name, ProductType = product.Type };
            }
            return reply;
        }

        public async Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync()
        {
            GRPCReply<List<GetTemplateMasterListResponseDc>> reply = new GRPCReply<List<GetTemplateMasterListResponseDc>>();
            var leadTemplateList = (await _context.ProductTemplateMasters.Where(x => !x.IsDeleted).Select(x => new GetTemplateMasterListResponseDc
            {
                DLTID = x.DLTID,
                Template = x.Template,
                TemplateCode = x.TemplateCode,
                TemplateType = x.TemplateType,
                TemplateId = x.Id,
                IsActive = x.IsActive,
                CreatedDate = x.Created
            }).ToListAsync());
            if (leadTemplateList != null && leadTemplateList.Any())
            {
                reply.Response = leadTemplateList;
                reply.Status = true;
                reply.Message = "data found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "data not found";
            }
            return reply;
        }
        public async Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request)
        {
            GRPCReply<GetTemplateMasterListResponseDc> reply = new GRPCReply<GetTemplateMasterListResponseDc>();
            var leadTemplate = (await _context.ProductTemplateMasters.Where(x => x.Id == request.Request && !x.IsDeleted).Select(x => new GetTemplateMasterListResponseDc
            {
                DLTID = x.DLTID,
                Template = x.Template,
                TemplateCode = x.TemplateCode,
                TemplateType = x.TemplateType,
                TemplateId = x.Id,
                IsActive = x.IsActive
            }).FirstOrDefaultAsync());
            if (leadTemplate != null)
            {
                reply.Response = leadTemplate;
                reply.Status = true;
                reply.Message = "data found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "data not found";
            }
            return reply;
        }

        public async Task<GRPCReply<List<CompanyApiReply>>> GetCompanyApiData()
        {
            GRPCReply<List<CompanyApiReply>> res = new GRPCReply<List<CompanyApiReply>>();
            res.Status = false;

            var result = _context.Database.SqlQueryRaw<CompanyApiReply>("exec GetCompanyApiData").AsEnumerable().ToList();
            if (result != null)
            {
                res.Status = true;
                res.Response = result;
            }
            return res;
        }

        public async Task<GRPCReply<List<AnchorCompanyReply>>> GetAnchorComapnyData(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<AnchorCompanyReply>> gRPCReply = new GRPCReply<List<AnchorCompanyReply>>();
            var query = from p in _context.ProductAnchorCompany
                        join r in request.Request on p.CompanyId equals r
                        where p.IsActive == true && p.IsDeleted == false
                        select new AnchorCompanyReply
                        {
                            AnchorCompanyId = p.Id,
                            CompanyId = p.CompanyId
                        };

            var CompanyList = query.ToList();

            gRPCReply.Response = CompanyList;
            gRPCReply.Status = true;
            gRPCReply.Message = "SuccesFully!!";
            return gRPCReply;
        }

        public NBFCCompanyConfigForProcessingFee GetLeadNBFCCompanyConfig(NBFCConfigProdRequest request)
        {
            //CompanyConfigReply companyConfigReply = new CompanyConfigReply();
            NBFCCompanyConfigForProcessingFee nBFCCompanyConfigForProcessingFee = new NBFCCompanyConfigForProcessingFee();
            var nbfcConfig = _context.ProductNBFCCompany.FirstOrDefault(x => x.IsActive && !x.IsDeleted && x.CompanyId == request.NBFCCompanyId && x.ProductId == request.ProductId);
            if (nbfcConfig != null)
            {
                nBFCCompanyConfigForProcessingFee = new NBFCCompanyConfigForProcessingFee
                {
                    ProcessingFeeType = nbfcConfig.ProcessingFeeType,
                    ProcessingFee = nbfcConfig.ProcessingFee,
                };
            }
            else
            {
                nBFCCompanyConfigForProcessingFee = new NBFCCompanyConfigForProcessingFee
                {
                    ProcessingFee = 0
                };
            }

            return nBFCCompanyConfigForProcessingFee;
        }

        public async Task<GRPCReply<List<ProductListResponseDc>>> GetProductByProductType(GRPCRequest<string> request)
        {
            GRPCReply<List<ProductListResponseDc>> gRPCReply = new GRPCReply<List<ProductListResponseDc>>();
            var productList = await _context.Products.Where(x => x.IsActive && !x.IsDeleted && x.Type == request.Request).Select(y => new ProductListResponseDc
            {
                ProductId = y.Id,
                ProductName = y.Name
            }).ToListAsync();

            if (productList.Count > 0)
            {

                gRPCReply.Response = productList;
                gRPCReply.Status = true;
                gRPCReply.Message = "";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<List<ProductListResponseDc>>> GetDSAProductByProductType(GRPCRequest<string> request)
        {
            GRPCReply<List<ProductListResponseDc>> gRPCReply = new GRPCReply<List<ProductListResponseDc>>();
            var productList = await _context.Products.Where(x => x.IsActive && !x.IsDeleted && x.Type == ProductTypeConstants.DSA).Select(y => new ProductListResponseDc
            {
                ProductId = y.Id,
                ProductName = y.Name
            }).ToListAsync();

            if (productList.Count > 0)
            {

                gRPCReply.Response = productList;
                gRPCReply.Status = true;
                gRPCReply.Message = "";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<long>> GetProductIdByCode(GRPCRequest<string> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            var productId = (await _context.Products.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.ProductCode == request.Request))?.Id;
            if (productId.HasValue)
            {
                gRPCReply.Status = true;
                gRPCReply.Response = productId.Value;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> GetProductCodeById(GRPCRequest<long> request)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            var productCode = (await _context.Products.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.Id == request.Request))?.ProductCode;
            if (!string.IsNullOrEmpty(productCode))
            {
                gRPCReply.Status = true;
                gRPCReply.Response = productCode;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request)
        {
            GRPCReply<bool> response = new GRPCReply<bool>();
            var existing = await _context.ProductAnchorCompany.Where(x => x.Id == request.Id && x.CompanyId == request.CompanyId && x.ProductId == request.ProductId).FirstOrDefaultAsync();
            if (existing == null && _context.ProductAnchorCompany.Any(x => x.CompanyId == request.CompanyId && x.ProductId == request.ProductId && !x.IsDeleted))
            {
                response.Status = false;
                response.Response = false;
                response.Message = "This Type of Product is Already Exists!!!";
                return response;
            }
            bool isAdded = true;
            if (existing != null)
            {
                var existingCompanyEmis = await _context.CompanyEMIOptions.Where(x => x.ProductAnchorCompanyId == existing.Id).ToListAsync();
                if (existingCompanyEmis != null && existingCompanyEmis.Any())
                {
                    foreach (var item in existingCompanyEmis)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        _context.Entry(item).State = EntityState.Modified;
                    }
                }
                var existingCompanyCreditDays = await _context.CompanyCreditDays.Where(x => x.ProductAnchorCompanyId == existing.Id).ToListAsync();
                if (existingCompanyCreditDays != null && existingCompanyCreditDays.Any())
                {
                    foreach (var item in existingCompanyCreditDays)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        _context.Entry(item).State = EntityState.Modified;
                    }
                }

                if (existing.AgreementURL == request.AgreementURL && existing.AgreementStartDate == request.AgreementStartDate && existing.AgreementEndDate == request.AgreementEndDate)
                {
                    existing.CompanyId = request.CompanyId;
                    existing.ProductId = request.ProductId;
                    existing.ProcessingFeePayableBy = request.ProcessingFeePayableBy;
                    existing.ProcessingFeeType = request.ProcessingFeeType;
                    existing.ProcessingFeeRate = request.ProcessingFeeRate;
                    existing.AnnualInterestPayableBy = request.AnnualInterestPayableBy;
                    //existing.TransactionFeeType = "Percentage";
                    //existing.TransactionFeeRate = request.TransactionFeeRate;
                    existing.DelayPenaltyRate = request.DelayPenaltyRate;
                    existing.BounceCharge = request.BounceCharge;
                    existing.DisbursementTAT = request.DisbursementTAT;
                    existing.AnnualInterestRate = request.AnnualInterestRate;
                    existing.MinTenureInMonth = request.MinTenureInMonth;
                    existing.MaxTenureInMonth = request.MaxTenureInMonth;
                    existing.EMIBounceCharge = request.EMIBounceCharge;
                    existing.EMIPenaltyRate = request.EMIPenaltyRate;
                    existing.EMIProcessingFeeRate = request.EMIProcessingFeeRate;
                    existing.EMIRate = request.EMIRate;
                    existing.MinLoanAmount = request.MinLoanAmount;
                    existing.MaxLoanAmount = request.MaxLoanAmount;
                    existing.CommissionPayout = request.CommissionPayout;
                    existing.ConsiderationFee = request.ConsiderationFee;
                    existing.DisbursementSharingCommission = request.DisbursementSharingCommission;

                    //existing.AgreementStartDate = request.AgreementStartDate;
                    //existing.AgreementEndDate = request.AgreementEndDate;
                    //existing.AgreementURL = request.AgreementURL;
                    existing.AgreementDocId = request.AgreementDocId;
                    existing.OfferMaxRate = request.OfferMaxRate;
                    existing.CustomCreditDays = request.CustomCreditDays;
                    existing.BlackSoilReferralCode = request.BlackSoilReferralCode; //new

                    _context.Entry(existing).State = EntityState.Modified;

                    List<CompanyEMIOptions> emiList = new List<CompanyEMIOptions>();
                    if (request.CompanyEMIOptions != null && request.CompanyEMIOptions.Any())
                    {
                        foreach (var item in request.CompanyEMIOptions)
                        {
                            CompanyEMIOptions companyEMIOptions = new CompanyEMIOptions
                            {
                                EMIOptionMasterId = item.EMIOptionMasterId,
                                ProductAnchorCompanyId = existing.Id,
                                IsActive = true,
                                IsDeleted = false,
                            };
                            emiList.Add(companyEMIOptions);
                        }
                        _context.AddRange(emiList);
                    }

                    List<CompanyCreditDays> creditDaysList = new List<CompanyCreditDays>();
                    if (request.CompanyCreditDays != null && request.CompanyCreditDays.Any())
                    {
                        foreach (var item in request.CompanyCreditDays)
                        {
                            CompanyCreditDays companyCreditDays = new CompanyCreditDays
                            {
                                CreditDaysMasterId = item.CreditDaysMasterId,
                                ProductAnchorCompanyId = existing.Id,
                                IsActive = true,
                                IsDeleted = false
                            };
                            creditDaysList.Add(companyCreditDays);
                        }
                        _context.AddRange(creditDaysList);
                    }
                    isAdded = false;
                }
                else
                {
                    existing.IsActive = false;
                    existing.IsDeleted = true;
                    _context.Entry(existing).State = EntityState.Modified;
                }
            }
            if (isAdded)
            {
                List<CompanyEMIOptions> emiList = new List<CompanyEMIOptions>();
                if (request.CompanyEMIOptions != null && request.CompanyEMIOptions.Any())
                {
                    foreach (var item in request.CompanyEMIOptions)
                    {
                        CompanyEMIOptions companyEMIOptions = new CompanyEMIOptions
                        {
                            EMIOptionMasterId = item.EMIOptionMasterId,
                            ProductAnchorCompanyId = 0,
                            IsActive = true,
                            IsDeleted = false,
                        };
                        emiList.Add(companyEMIOptions);
                    }
                }
                List<CompanyCreditDays> creditDaysList = new List<CompanyCreditDays>();
                if (request.CompanyCreditDays != null && request.CompanyCreditDays.Any())
                {
                    foreach (var item in request.CompanyCreditDays)
                    {
                        CompanyCreditDays companyCreditDays = new CompanyCreditDays
                        {
                            CreditDaysMasterId = item.CreditDaysMasterId,
                            ProductAnchorCompanyId = 0,
                            IsActive = true,
                            IsDeleted = false
                        };
                        creditDaysList.Add(companyCreditDays);
                    }
                }

                ProductAnchorCompany productAnchorCompany = new ProductAnchorCompany
                {
                    CompanyId = request.CompanyId,
                    ProductId = request.ProductId,
                    ProcessingFeePayableBy = request.ProcessingFeePayableBy,
                    ProcessingFeeType = request.ProcessingFeeType,
                    ProcessingFeeRate = request.ProcessingFeeRate,
                    AnnualInterestPayableBy = request.AnnualInterestPayableBy,
                    //TransactionFeeType = "Percentage",
                    //TransactionFeeRate = request.TransactionFeeRate,
                    DelayPenaltyRate = request.DelayPenaltyRate,
                    BounceCharge = request.BounceCharge,
                    DisbursementTAT = request.DisbursementTAT,
                    AnnualInterestRate = request.AnnualInterestRate,
                    MinTenureInMonth = request.MinTenureInMonth,
                    MaxTenureInMonth = request.MaxTenureInMonth,
                    EMIBounceCharge = request.EMIBounceCharge,
                    EMIPenaltyRate = request.EMIPenaltyRate,
                    EMIProcessingFeeRate = request.EMIProcessingFeeRate,
                    EMIRate = request.EMIRate,
                    MinLoanAmount = request.MinLoanAmount,
                    MaxLoanAmount = request.MaxLoanAmount,
                    CommissionPayout = request.CommissionPayout,
                    ConsiderationFee = request.ConsiderationFee,
                    DisbursementSharingCommission = request.DisbursementSharingCommission,

                    AgreementEndDate = request.AgreementEndDate,
                    AgreementStartDate = request.AgreementStartDate,
                    AgreementURL = request.AgreementURL,
                    AgreementDocId = request.AgreementDocId,
                    OfferMaxRate = request.OfferMaxRate,
                    CustomCreditDays = request.CustomCreditDays,
                    IsActive = true,
                    IsDeleted = false,
                    CompanyEMIOptions = emiList,
                    CompanyCreditDays = creditDaysList,
                    BlackSoilReferralCode = request.BlackSoilReferralCode
                };
                _context.Add(productAnchorCompany);
            }
            int rowChanged = await _context.SaveChangesAsync();

            if (rowChanged > 0)
            {
                response.Status = true;
                response.Message = isAdded ? "Product Added Successfully" : "Product Updated Successfully";
                response.Response = true;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed to Add/Update Product";
            }
            return response;
        }

        public async Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request)
        {
            GRPCReply<bool> response = new GRPCReply<bool>();
            var existing = await _context.ProductNBFCCompany.Where(x => x.Id == request.Id && x.CompanyId == request.CompanyId && x.ProductId == request.ProductId).FirstOrDefaultAsync();
            if (existing == null && _context.ProductNBFCCompany.Any(x => x.CompanyId == request.CompanyId && x.ProductId == request.ProductId && !x.IsDeleted))
            {
                response.Status = false;
                response.Response = false;
                response.Message = "This Type of Product is Already Exists!!!";
                return response;
            }
            bool isAdded = false;
            if (existing != null)
            {
                if (existing.AgreementURL == request.AgreementURL && existing.AgreementStartDate == request.AgreementStartDate && existing.AgreementEndDate == request.AgreementEndDate)
                {
                    existing.CompanyId = request.CompanyId;
                    existing.ProductId = request.ProductId;
                    existing.BounceCharges = request.BounceCharges;
                    existing.AnnualInterestRate = request.InterestRate;
                    existing.PenaltyCharges = request.PenaltyCharges;
                    existing.PlatformFee = request.PlatformFee;
                    existing.ProcessingFeeType = request.ProcessingFeeType;
                    existing.ProcessingFee = request.ProcessingFee;
                    existing.CustomerAgreementType = request.CustomerAgreementType;
                    existing.CustomerAgreementURL = request.CustomerAgreementURL;
                    existing.CustomerAgreementDocId = request.CustomerAgreementDocId;
                    existing.AgreementDocId = request.AgreementDocId;
                    existing.SanctionLetterDocId = request.SanctionLetterDocId;
                    existing.SanctionLetterURL = request.SanctionLetterURL;
                    existing.IsInterestRateCoSharing = request.IsInterestRateCoSharing;
                    existing.IsPenaltyChargeCoSharing = request.IsPenaltyChargeCoSharing;
                    existing.IsBounceChargeCoSharing = request.IsBounceChargeCoSharing;
                    existing.IsPlatformFeeCoSharing = request.IsPlatformFeeCoSharing;
                    existing.DisbursementType = request.DisbursementType;
                    _context.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    existing.IsActive = false;
                    existing.IsDeleted = true;
                    _context.Entry(existing).State = EntityState.Modified;

                    ProductNBFCCompany productNBFCCompany = new ProductNBFCCompany
                    {
                        CompanyId = request.CompanyId,
                        ProductId = request.ProductId,
                        BounceCharges = request.BounceCharges,
                        AnnualInterestRate = request.InterestRate,
                        PenaltyCharges = request.PenaltyCharges,
                        PlatformFee = request.PlatformFee,
                        ProcessingFeeType = request.ProcessingFeeType,
                        ProcessingFee = request.ProcessingFee,
                        AgreementEndDate = request.AgreementEndDate,
                        AgreementStartDate = request.AgreementStartDate,
                        AgreementURL = request.AgreementURL,
                        CustomerAgreementType = request.CustomerAgreementType,
                        CustomerAgreementURL = request.CustomerAgreementURL,
                        CustomerAgreementDocId = request.CustomerAgreementDocId,
                        AgreementDocId = request.AgreementDocId,
                        SanctionLetterDocId = request.SanctionLetterDocId,
                        SanctionLetterURL = request.SanctionLetterURL,
                        IsInterestRateCoSharing = request.IsInterestRateCoSharing,
                        IsPenaltyChargeCoSharing = request.IsPenaltyChargeCoSharing,
                        IsBounceChargeCoSharing = request.IsBounceChargeCoSharing,
                        IsPlatformFeeCoSharing = request.IsPlatformFeeCoSharing,
                        DisbursementType = request.DisbursementType,
                        TAPIKey = existing.TAPIKey,
                        TAPISecretKey = existing.TAPISecretKey,
                        TReferralCode = existing.TReferralCode,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.Add(productNBFCCompany);
                    isAdded = true;
                }
            }
            else
            {
                ProductNBFCCompany productNBFCCompany = new ProductNBFCCompany
                {
                    CompanyId = request.CompanyId,
                    ProductId = request.ProductId,
                    BounceCharges = request.BounceCharges,
                    AnnualInterestRate = request.InterestRate,
                    PenaltyCharges = request.PenaltyCharges,
                    PlatformFee = request.PlatformFee,
                    ProcessingFeeType = request.ProcessingFeeType,
                    ProcessingFee = request.ProcessingFee,
                    AgreementEndDate = request.AgreementEndDate,
                    AgreementStartDate = request.AgreementStartDate,
                    AgreementURL = request.AgreementURL,
                    CustomerAgreementType = request.CustomerAgreementType,
                    CustomerAgreementURL = request.CustomerAgreementURL,
                    CustomerAgreementDocId = request.CustomerAgreementDocId,
                    AgreementDocId = request.AgreementDocId,
                    SanctionLetterDocId = request.SanctionLetterDocId,
                    SanctionLetterURL = request.SanctionLetterURL,
                    IsInterestRateCoSharing = request.IsInterestRateCoSharing,
                    IsPenaltyChargeCoSharing = request.IsPenaltyChargeCoSharing,
                    IsBounceChargeCoSharing = request.IsBounceChargeCoSharing,
                    IsPlatformFeeCoSharing = request.IsPlatformFeeCoSharing,
                    DisbursementType = request.DisbursementType,
                    IsActive = true,
                    IsDeleted = false
                };
                _context.Add(productNBFCCompany);
                isAdded = true;
            }
            int rowChanged = await _context.SaveChangesAsync();

            if (rowChanged > 0)
            {
                response.Status = true;
                response.Message = isAdded ? "Product Added Successfully" : "Product Updated Successfully";
                response.Response = true;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed to Add/Update Product";
            }
            return response;
        }

        public async Task<GRPCReply<List<string>>> GetUsersIDS(LeadListPageRequest req)
        {
            GRPCReply<List<string>> res = new GRPCReply<List<string>> { Message = "Data Not Found" };
            List<string> agentList = new List<string>();

            if (req.UserIds != null && req.UserIds.Any())
            {
                var SalesAgent = await _context.SalesAgents.FirstOrDefaultAsync(x => req.UserIds != null && req.UserIds.Contains(x.UserId) && !x.IsDeleted && x.IsActive);
                if (SalesAgent != null && SalesAgent.Type == DSAProfileTypeConstants.DSA)
                {
                    var dsaSalesAgentUserIds = (from sa in _context.SalesAgents
                                                join sap in _context.SalesAgentProducts on sa.Id equals sap.SalesAgentId
                                                where !sa.IsDeleted && sap.IsActive && !sap.IsDeleted
                                                && req.ProductId.Contains(sap.ProductId)
                                                && sa.AnchorCompanyId == SalesAgent.AnchorCompanyId && sa.Type == DSAProfileTypeConstants.DSAUser
                                                select sa.UserId).ToList();

                    agentList = dsaSalesAgentUserIds != null && dsaSalesAgentUserIds.Any() ? dsaSalesAgentUserIds.ToList() : new List<string>();
                    agentList.Add(SalesAgent.UserId);
                }
                else if (SalesAgent != null && (SalesAgent.Type == DSAProfileTypeConstants.Connector || SalesAgent.Type == DSAProfileTypeConstants.DSAUser))
                {
                    agentList.Add(SalesAgent.UserId);
                }
            }
            else
            {
                if (req.IsDSA)
                {
                    var dsaSalesAgentUserIds = (from sa in _context.SalesAgents
                                                join sap in _context.SalesAgentProducts on sa.Id equals sap.SalesAgentId
                                                where !sa.IsDeleted && sap.IsActive && !sap.IsDeleted
                                                && req.ProductId.Contains(sap.ProductId)
                                                && req.CompanyId.Contains((int)(sa.AnchorCompanyId))
                                                select sa.UserId).ToList();
                    agentList = dsaSalesAgentUserIds != null && dsaSalesAgentUserIds.Any() ? dsaSalesAgentUserIds : new List<string>();
                }
                else
                {
                    var dsaSalesAgentUserIds = (from sa in _context.SalesAgents
                                                join sap in _context.SalesAgentProducts on sa.Id equals sap.SalesAgentId
                                                where !sa.IsDeleted && sap.IsActive && !sap.IsDeleted
                                                && req.ProductId.Contains(sap.ProductId)
                                                select sa.UserId).ToList();
                    agentList = dsaSalesAgentUserIds != null && dsaSalesAgentUserIds.Any() ? dsaSalesAgentUserIds : new List<string>();
                }
            }
            if (agentList.Any())
            {
                res.Response = agentList;
                res.Status = true;
            }
            return res;
        }

        public async Task<GRPCReply<List<long>>> GetNbfcCompaniesByProduct(GetCompanyByProductRequestDc request)
        {
            GRPCReply<List<long>> res = new GRPCReply<List<long>> { Message = "Data Not Found" };
            var companyIds = await _context.ProductNBFCCompany.Where(x => x.IsActive && !x.IsDeleted && x.ProductId == request.ProductId && request.CompanyIds.Contains(x.CompanyId)).Select(x => x.CompanyId).ToListAsync();
            if (companyIds != null && companyIds.Count > 0)
            {
                res.Response = companyIds;
                res.Status = true;
                res.Message = "Data found";
            }
            return res;
        }

        public async Task<GRPCReply<List<long>>> GetAnchorCompaniesByProduct(GetCompanyByProductRequestDc request)
        {
            GRPCReply<List<long>> res = new GRPCReply<List<long>> { Message = "Data Not Found" };
            var companyIds = await _context.ProductAnchorCompany.Where(x => x.IsActive && !x.IsDeleted && x.ProductId == request.ProductId && request.CompanyIds.Contains(x.CompanyId)).OrderBy(x => x.CompanyId).Select(x => x.CompanyId).ToListAsync();
            if (companyIds != null && companyIds.Count > 0)
            {
                res.Response = companyIds;
                res.Status = true;
                res.Message = "Data found";
            }
            return res;
        }


        public async Task<GRPCReply<List<GetProductNBFCConfigResponseDc>>> GetProductNBFCConfigs(GRPCRequest<GetProductNBFCConfigRequestDc> request)
        {
            GRPCReply<List<GetProductNBFCConfigResponseDc>> reply = new GRPCReply<List<GetProductNBFCConfigResponseDc>> { Message = "Data Not Found!!!" };
            var matchingSlabs = await _context.ProductSlabConfigurations
                .Where(x => x.MinLoanAmount <= request.Request.OfferAmount && x.MaxLoanAmount >= request.Request.OfferAmount
                && x.ProductId == request.Request.ProductId && x.IsActive && !x.IsDeleted && request.Request.NBFCCompanyIds.Contains(x.CompanyId))
                .ToListAsync();
            var productNBFCCompany = await _context.ProductNBFCCompany.Where(x => request.Request.NBFCCompanyIds.Contains(x.CompanyId) && x.ProductId == request.Request.ProductId && x.IsActive && !x.IsDeleted).ToListAsync();
            if (matchingSlabs.Any())
            {
                reply.Response = new List<GetProductNBFCConfigResponseDc>();
                foreach (var item in request.Request.NBFCCompanyIds)
                {
                    var productConfig = productNBFCCompany.FirstOrDefault(x => x.CompanyId == item);
                    if (productConfig != null)
                    {
                        GetProductNBFCConfigResponseDc getProductNBFCConfigResponseDc = new GetProductNBFCConfigResponseDc
                        {
                            CompanyId = productConfig.CompanyId,
                            ProductId = productConfig.ProductId,
                            ArrangementType = productConfig.ArrangementType,
                            PFSharePercentage = productConfig.PFSharePercentage,
                            Tenure = productConfig.Tenure,
                            BounceCharge = productConfig.BounceCharges,
                            MaxBounceCharge = productConfig.MaxBounceCharges,
                            PlatFormFee = productConfig.PlatformFee,
                            //InterestRate = productConfig.AnnualInterestRate,
                            PenalPercent = productConfig.PenaltyCharges,
                            MaxPenalPercent = Convert.ToDouble(productConfig.MaxPenaltyCharges),
                            GST = 0,
                            ODdays = 0,
                            IseSignEnable = productConfig.IseSignEnable ?? false,
                            ODCharges = 0,
                            CustomerAgreementURL = productConfig.CustomerAgreementURL ?? "",
                            //MaxInterestRate=productConfig.AnnualInterestRate,
                            //PF=productConfig.ProcessingFee,
                        };
                        var slabs = matchingSlabs.Where(x => x.CompanyId == item);
                        if (slabs != null && slabs.Any())
                        {
                            var pfSlab = matchingSlabs.FirstOrDefault(s => s.SlabType == SlabTypeConstants.PF);
                            getProductNBFCConfigResponseDc.NBFCProcessingFee = pfSlab != null ? pfSlab.MinValue : 0;
                            getProductNBFCConfigResponseDc.NBFCProcessingFeeType = pfSlab != null ? pfSlab.ValueType : "";
                            var roiSlab = matchingSlabs.FirstOrDefault(s => s.SlabType == SlabTypeConstants.ROI);
                            getProductNBFCConfigResponseDc.NBFCInterest = roiSlab != null ? roiSlab.MinValue : 0;
                        }
                        reply.Response.Add(getProductNBFCConfigResponseDc);
                    }
                }

                //reply.Response = await _context.ProductNBFCCompany.Where(x => request.Request.NBFCCompanyIds.Contains(x.CompanyId) && x.ProductId == request.Request.ProductId && x.IsActive && !x.IsDeleted).Select(x => new GetProductNBFCConfigResponseDc
                //{
                //    CompanyId = x.CompanyId,
                //    ProductId = x.ProductId,
                //    ArrangementType = x.ArrangementType,
                //    PFSharePercentage = x.PFSharePercentage,
                //    Tenure = x.Tenure,
                //    BounceCharge = x.BounceCharges,
                //    MaxBounceCharge = x.MaxBounceCharges,
                //    PlatFormFee = x.PlatformFee,
                //    //InterestRate = x.AnnualInterestRate,
                //    PenalPercent = x.PenaltyCharges,
                //    MaxPenalPercent = Convert.ToDouble(x.MaxPenaltyCharges),
                //    GST = 0,
                //    ODdays = 0,
                //    IseSignEnable = x.IseSignEnable ?? false,
                //    ODCharges = 0,
                //    CustomerAgreementURL = x.CustomerAgreementURL ?? "",
                //    //MaxInterestRate=x.AnnualInterestRate,
                //    //PF=x.ProcessingFee,
                //}).ToListAsync();
            }

            if (reply.Response != null && reply.Response.Any())
            {
                foreach (var item in reply.Response)
                {
                    var config = await GetProductSlabConfigs(new GRPCRequest<ProductSlabConfigRequest> { Request = new ProductSlabConfigRequest { CompanyId = item.CompanyId, ProductId = item.ProductId } });
                    item.ProductSlabConfigs = config.Response;
                }
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }
        #region DSA


        public async Task<GRPCReply<bool>> GetSalesAgentProduct()
        {
            GRPCReply<bool> response = new GRPCReply<bool>();

            return response;
        }

        public async Task<GRPCReply<bool>> GetSalesAgentList()
        {
            GRPCReply<bool> response = new GRPCReply<bool>();

            return response;
        }

        public async Task<GRPCReply<SalesAgentReply>> GetExistDSACompanyDetail(SaleAgentRequest request)
        {
            GRPCReply<SalesAgentReply> response = new GRPCReply<SalesAgentReply>();
            SalesAgentReply salesAgentReply = new SalesAgentReply();

            var isExistMobileNo = _context.SalesAgents.FirstOrDefault(x => x.UserId == request.UserId && !x.IsDeleted);
            if (isExistMobileNo != null)
            {
                if (isExistMobileNo.IsActive)
                {
                    var isExistProduct = await _context.SalesAgentProducts.Where(x => x.SalesAgentId == isExistMobileNo.Id && x.IsActive && !x.IsDeleted).Include(x => x.Products).FirstOrDefaultAsync();
                    if (isExistProduct != null)
                    {

                        var commissionData = await _context.SalesAgentCommisions.Where(x => x.SalesAgentProductId == isExistProduct.Id && x.IsActive && !x.IsDeleted).Include(x => x.SalesAgentProducts).Select(x => new SalesAgentCommissionList
                        {
                            MaxAmount = x.MaxAmount,
                            MinAmount = x.MinAmount,
                            PayoutPercentage = x.CommisionPercentage,
                            ProductId = x.SalesAgentProducts.ProductId
                        }).ToListAsync();

                        salesAgentReply.UserId = request.UserId;
                        salesAgentReply.ProductId = isExistProduct.ProductId;
                        salesAgentReply.AnchorCompanyId = isExistMobileNo.AnchorCompanyId;
                        salesAgentReply.ProductCode = isExistProduct.Products.ProductCode;
                        salesAgentReply.Role = isExistMobileNo.Role ?? "";
                        salesAgentReply.Type = isExistMobileNo.Type;
                        salesAgentReply.Name = isExistMobileNo.FullName;
                        salesAgentReply.Address = isExistMobileNo.AadharAddress ?? "";
                        salesAgentReply.AadharNumber = isExistMobileNo.AadharNo ?? "";
                        salesAgentReply.PanNumber = isExistMobileNo.PanNo ?? "";
                        salesAgentReply.Mobile = isExistMobileNo.MobileNo;
                        salesAgentReply.selfie = isExistMobileNo.SelfieUrl ?? "";
                        salesAgentReply.WorkingLocation = isExistMobileNo.WorkingLocation ?? "";
                        salesAgentReply.StartedOn = isExistMobileNo.AgreementStartDate;
                        salesAgentReply.ExpiredOn = isExistMobileNo.AgreementEndDate;
                        salesAgentReply.DocSignedUrl = isExistMobileNo.AgreementUrl;
                        salesAgentReply.SalesAgentCommissions = commissionData != null && commissionData.Any() ? commissionData : new List<SalesAgentCommissionList>();
                        salesAgentReply.CreatedBy = isExistProduct.CreatedBy ?? "";
                        salesAgentReply.DSACode = isExistMobileNo.DSACode != null ? isExistMobileNo.DSACode : "";
                        response.Response = salesAgentReply;
                        response.Status = true;
                    }
                }
                else
                {
                    var isExistProduct = await _context.SalesAgentProducts.Where(x => x.SalesAgentId == isExistMobileNo.Id).Include(x => x.Products).FirstOrDefaultAsync();
                    if (isExistProduct != null)
                    {
                        var commissionData = await _context.SalesAgentCommisions.Where(x => x.SalesAgentProductId == isExistProduct.Id && x.IsActive && !x.IsDeleted).Include(x => x.SalesAgentProducts).Select(x => new SalesAgentCommissionList
                        {
                            MaxAmount = x.MaxAmount,
                            MinAmount = x.MinAmount,
                            PayoutPercentage = x.CommisionPercentage,
                            ProductId = x.SalesAgentProducts.ProductId
                        }).ToListAsync();

                        salesAgentReply.UserId = request.UserId;
                        salesAgentReply.ProductId = isExistProduct.ProductId;
                        salesAgentReply.AnchorCompanyId = isExistMobileNo.AnchorCompanyId;
                        salesAgentReply.ProductCode = isExistProduct.Products.ProductCode;
                        salesAgentReply.Role = isExistMobileNo.Role ?? "";
                        salesAgentReply.Type = isExistMobileNo.Type;
                        salesAgentReply.Name = isExistMobileNo.FullName;
                        salesAgentReply.Address = isExistMobileNo.AadharAddress ?? "";
                        salesAgentReply.AadharNumber = isExistMobileNo.AadharNo ?? "";
                        salesAgentReply.PanNumber = isExistMobileNo.PanNo ?? "";
                        salesAgentReply.Mobile = isExistMobileNo.MobileNo;
                        salesAgentReply.selfie = isExistMobileNo.SelfieUrl ?? "";
                        salesAgentReply.WorkingLocation = isExistMobileNo.WorkingLocation ?? "";
                        salesAgentReply.StartedOn = isExistMobileNo.AgreementStartDate;
                        salesAgentReply.ExpiredOn = isExistMobileNo.AgreementEndDate;
                        salesAgentReply.DocSignedUrl = isExistMobileNo.AgreementUrl;
                        salesAgentReply.SalesAgentCommissions = salesAgentReply.SalesAgentCommissions = commissionData != null && commissionData.Any() ? commissionData : new List<SalesAgentCommissionList>();

                        response.Response = salesAgentReply;
                        response.Status = true;
                    }
                }
            }
            else if (isExistMobileNo != null && !isExistMobileNo.IsActive)
            {
                response.Status = true;
                response.Message = "InActive User!!";
                response.Response = null;
            }
            else
            {
                response.Status = false;
            }

            return response;
        }

        public async Task<GRPCReply<bool>> AddUpdateSalesAgent(GRPCRequest<SalesAgentRequest> request)
        {
            GRPCReply<bool> response = new GRPCReply<bool> { Message = "Failed" };
            if (request != null && request.Request != null)
            {
                var existing = await _context.SalesAgents.FirstOrDefaultAsync(x => x.Id == request.Request.SalesAgentId && x.IsActive && !x.IsDeleted);
                if (existing == null)
                {
                    GRPCReply<string> dsaCode = await GetDSACurrentCode(new GRPCRequest<DSAEntityDc> { Request = new DSAEntityDc { EntityName = ProductTypeConstants.DSA, DSAType = request.Request.Type } });
                    var salesAgent = new SalesAgent
                    {
                        AadharBackUrl = request.Request.AadharBackUrl,
                        AadharFrontUrl = request.Request.AadharFrontUrl,
                        AadharNo = request.Request.AadharNo,
                        AnchorCompanyId = request.Request.AnchorCompanyId,
                        CityName = request.Request.CityName,
                        FullName = request.Request.FullName,
                        MobileNo = request.Request.MobileNo,
                        PanNo = request.Request.PanNo,
                        PanUrl = request.Request.PanUrl,
                        StateName = request.Request.StateName,
                        Status = request.Request.Status,
                        Type = request.Request.Type,
                        UserId = request.Request.UserId,
                        AgreementEndDate = request.Request.AgreementEndDate,
                        AgreementStartDate = request.Request.AgreementStartDate,
                        AgreementUrl = request.Request.AgreementUrl,
                        GstnNo = request.Request.GstnNo,
                        GstnUrl = request.Request.GstnUrl,
                        Role = request.Request.Role,
                        AadharAddress = request.Request.AadharAddress,
                        SelfieUrl = request.Request.SelfieUrl,
                        WorkingLocation = request.Request.WorkingLocation,
                        EmailId = request.Request.EmailId,
                        IsActive = true,
                        IsDeleted = false,
                        DSACode = dsaCode.Status ? dsaCode.Response : ""
                    };
                    List<SalesAgentProduct> salesAgentProductList = new List<SalesAgentProduct>();
                    foreach (var productId in request.Request.ProductIds)
                    {
                        var salesAgentProduct = new SalesAgentProduct
                        {
                            ProductId = productId,
                            SalesAgentId = 0,
                            IsActive = true,
                            IsDeleted = false,
                            SalesAgents = salesAgent
                        };
                        salesAgentProductList.Add(salesAgentProduct);
                    }
                    var payoutMaster = await _context.PayOutMasters.FirstAsync(x => x.Type == PayOutMasterTypeConstants.Disbursment && x.IsActive && !x.IsDeleted);
                    foreach (var salesAgentProduct in salesAgentProductList)
                    {
                        foreach (var item in request.Request.SalesAgentCommissions.Where(x => x.ProductId == salesAgentProduct.ProductId))
                        {
                            var salesAgentCommision = new SalesAgentCommision
                            {
                                CommisionPercentage = item.PayoutPercentage,
                                PayOutMasterId = payoutMaster.Id,
                                SalesAgentProductId = 0,
                                IsActive = true,
                                IsDeleted = false,
                                SalesAgentProducts = salesAgentProduct,
                                MaxAmount = item.MaxAmount,
                                MinAmount = item.MinAmount
                            };
                            _context.Add(salesAgentCommision);
                        }
                    }
                }
                else
                {
                    existing.AadharBackUrl = request.Request.AadharBackUrl;
                    existing.AadharFrontUrl = request.Request.AadharFrontUrl;
                    existing.AadharNo = request.Request.AadharNo;
                    existing.AnchorCompanyId = request.Request.AnchorCompanyId;
                    existing.CityName = request.Request.CityName;
                    existing.FullName = request.Request.FullName;
                    existing.MobileNo = request.Request.MobileNo;
                    existing.PanNo = request.Request.PanNo;
                    existing.PanUrl = request.Request.PanUrl;
                    existing.StateName = request.Request.StateName;
                    existing.Status = request.Request.Status;
                    existing.Type = request.Request.Type;
                    existing.UserId = request.Request.UserId;
                    existing.AgreementEndDate = request.Request.AgreementEndDate;
                    existing.AgreementStartDate = request.Request.AgreementStartDate;
                    existing.AgreementUrl = request.Request.AgreementUrl;
                    existing.GstnNo = request.Request.GstnNo;
                    existing.GstnUrl = request.Request.GstnUrl;
                    existing.Role = request.Request.Role;
                    existing.EmailId = request.Request.EmailId;
                    existing.SelfieUrl = request.Request.SelfieUrl;
                    existing.AadharAddress = request.Request.AadharAddress;
                    existing.WorkingLocation = request.Request.WorkingLocation;
                    _context.Entry(existing).State = EntityState.Modified;
                }
                if (_context.SaveChanges() > 0)
                {
                    response.Response = true;
                    response.Message = "Data Saved";
                }
            }
            return response;
        }

        public async Task<GRPCReply<ProductDetailRespnose>> GetProductIdByCodeAndProductActivities(GRPCRequest<ProductdetailRequest> request)
        {
            GRPCReply<ProductDetailRespnose> gRPCReply = new GRPCReply<ProductDetailRespnose>();
            var productDetail = (await _context.Products.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.ProductCode == request.Request.ProductCode));
            if (productDetail != null)
            {
                ProductDetailRespnose productDetailRespnose = new ProductDetailRespnose();
                productDetailRespnose.ProductType = productDetail.Type;
                if (await _context.ProductAnchorCompany.AnyAsync(x => x.ProductId == productDetail.Id && x.CompanyId == request.Request.CompanyId && x.IsActive && !x.IsDeleted))
                {
                    List<ProductCompanyActivityMasters> productActivityMasters = new List<ProductCompanyActivityMasters>();
                    if (request.Request.CompanyType == CompanyTypeEnum.All.ToString())
                        productActivityMasters = _context.ProductCompanyActivityMasters.Where(x => x.ProductId == productDetail.Id
                                 && x.CompanyId == request.Request.CompanyId && x.IsActive && !x.IsDeleted && x.ActivityMasters.FrontOrBack == "Front").Include(x => x.ActivityMasters).Include(x => x.SubActivityMasters).AsNoTracking().ToList();
                    else
                        productActivityMasters = _context.ProductCompanyActivityMasters.Where(x => x.ProductId == productDetail.Id
                            && x.CompanyId == request.Request.CompanyId && x.IsActive && !x.IsDeleted && x.ActivityMasters.FrontOrBack == "Front" && x.ActivityMasters.CompanyType == request.Request.CompanyType).Include(x => x.ActivityMasters).Include(x => x.SubActivityMasters).AsNoTracking().ToList();

                    productDetailRespnose.Status = true;
                    productDetailRespnose.Message = "Get product activity successfully.";
                    if (productActivityMasters.Any())
                    {
                        int i = 1;
                        List<GRPCLeadProductActivity> grpcLeadProdActivities = new List<GRPCLeadProductActivity>();
                        var ProdActivities = productActivityMasters.Select(x => new
                        {
                            ActivityMasterId = x.ActivityMasterId,
                            ActivityName = x.ActivityMasters.ActivityName,
                            ProductId = x.ProductId,
                            A_Sequence = x.Sequence,
                            //S_Sequence = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.Sequence : 1,
                            KycMasterCode = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.KycMasterCode : null,
                            SubActivityMasterId = x.SubActivityMasterId,
                            SubActivityName = x.SubActivityMasterId.HasValue ? x.SubActivityMasters.Name : ""
                        });
                        foreach (var x in ProdActivities.OrderBy(x => x.A_Sequence))
                        {
                            grpcLeadProdActivities.Add(new GRPCLeadProductActivity
                            {
                                ActivityMasterId = x.ActivityMasterId,
                                ActivityName = x.ActivityName,
                                ProductId = x.ProductId,
                                Sequence = i,
                                KycMasterCode = x.KycMasterCode,
                                SubActivityMasterId = x.SubActivityMasterId,
                                SubActivityName = x.SubActivityName
                            });
                            i++;
                        }
                        productDetailRespnose.ProductId = productDetail.Id;
                        productDetailRespnose.ProductCode = productDetail.ProductCode;
                        productDetailRespnose.LeadProductActivities = grpcLeadProdActivities;
                    }
                }
                else
                {
                    productDetailRespnose.Status = false;
                    productDetailRespnose.Message = "Requested product not associate with company.";
                }

                gRPCReply.Status = true;
                gRPCReply.Response = productDetailRespnose;
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Requested product not available.";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> CreateDSAUser(GRPCRequest<CreateDSAUser> request)
        {
            GRPCReply<bool> response = new GRPCReply<bool> { Message = "Failed" };
            if (request != null && request.Request != null)
            {
                var ExixtsdsaUser = await _context.SalesAgents.AnyAsync(x => x.UserId == request.Request.UserId && !x.IsDeleted);
                if (ExixtsdsaUser)
                {
                    response.Message = "User already exists.";
                    return response;
                }
                var dsa = await _context.SalesAgents.FirstOrDefaultAsync(x => x.UserId == request.Request.CreateBy && x.Type == DSAProfileTypeConstants.DSA && x.AnchorCompanyId == request.Request.CompanyId && x.IsActive && !x.IsDeleted);
                if (dsa != null)
                {
                    var saleagentproduct = await _context.SalesAgentProducts.FirstOrDefaultAsync(x => x.SalesAgentId == dsa.Id && x.ProductId == request.Request.ProductId && x.IsActive && !x.IsDeleted);

                    GRPCReply<string> dsaCode = await GetDSACurrentCode(new GRPCRequest<DSAEntityDc> { Request = new DSAEntityDc { EntityName = ProductTypeConstants.DSA, DSAType = DSAProfileTypeConstants.DSAUser } });
                    if (dsa != null && saleagentproduct != null)
                    {
                        var salesAgent = new SalesAgent
                        {
                            AadharBackUrl = "",
                            AadharFrontUrl = "",
                            AadharNo = "",
                            AnchorCompanyId = dsa.AnchorCompanyId,
                            CityName = "",
                            FullName = request.Request.FullName,
                            MobileNo = request.Request.MobileNumber,
                            PanNo = "",
                            PanUrl = "",
                            StateName = "",
                            Status = dsa.Status,
                            Type = DSAProfileTypeConstants.DSAUser,
                            UserId = request.Request.UserId,
                            AgreementEndDate = null,
                            AgreementStartDate = null,
                            AgreementUrl = "",
                            GstnNo = "",
                            GstnUrl = "",
                            SelfieUrl = "",
                            WorkingLocation = "",
                            AadharAddress = "",
                            IsActive = true,
                            IsDeleted = false,
                            Role = UserRoleConstants.SalesAgent,
                            EmailId = request.Request.EmailId,
                            DSACode = dsaCode.Status ? dsaCode.Response : ""
                        };
                        var salesAgentProduct = new SalesAgentProduct
                        {
                            ProductId = saleagentproduct.ProductId,
                            SalesAgentId = 0,
                            IsActive = true,
                            IsDeleted = false,
                            SalesAgents = salesAgent
                        };
                        var payoutMaster = await _context.PayOutMasters.FirstAsync(x => x.Type == PayOutMasterTypeConstants.Disbursment && x.IsActive && !x.IsDeleted);
                        var salesAgentCommision = new SalesAgentCommision
                        {
                            CommisionPercentage = request.Request.PayoutPercenatge ?? 0,
                            PayOutMasterId = payoutMaster.Id,
                            SalesAgentProductId = 0,
                            IsActive = true,
                            IsDeleted = false,
                            SalesAgentProducts = salesAgentProduct,
                            MaxAmount = 10000000,
                            MinAmount = 0
                        };
                        _context.Add(salesAgentCommision);
                        if (_context.SaveChanges() > 0)
                        {
                            response.Status = true;
                            response.Response = true;
                            response.Message = "Create User successfully";
                        }
                    }
                }
            }
            return response;
        }

        public async Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetDSASalesAgentList(GRPCRequest<string> request)
        {
            GRPCReply<List<GetDSASalesAgentListResponseDc>> res = new GRPCReply<List<GetDSASalesAgentListResponseDc>> { Message = "Data Not Found" };
            var dsaAdmin = await _context.SalesAgents.FirstOrDefaultAsync(x => x.UserId == request.Request && x.Type == DSAProfileTypeConstants.DSA && x.IsActive && !x.IsDeleted);
            if (dsaAdmin != null)
            {
                res.Response = await _context.SalesAgents.Where(x => x.AnchorCompanyId == dsaAdmin.AnchorCompanyId && x.Type == DSAProfileTypeConstants.DSAUser && x.IsActive && !x.IsDeleted).Select(x => new GetDSASalesAgentListResponseDc
                {
                    UserId = x.UserId,
                    AnchorCompanyId = x.AnchorCompanyId,
                    FullName = x.FullName,
                    MobileNo = x.MobileNo,
                    //PayoutPercenatge = x.PayoutPercenatge,
                    Role = x.Role,
                    Type = x.Type,
                    IsActive = x.IsActive,
                    IsDeleted = x.IsDeleted
                }).ToListAsync();
                if (res.Response != null && res.Response.Any())
                {
                    res.Status = true;
                    res.Message = "Data Found";
                }
            }
            return res;
        }
        public async Task<GRPCReply<GetDSASalesAgentListResponseDc>> GetSalesAgentDetails(GRPCRequest<string> request)
        {
            GRPCReply<GetDSASalesAgentListResponseDc> res = new GRPCReply<GetDSASalesAgentListResponseDc> { Message = "Data Not Found" };
            var salesAgent = (from sa in _context.SalesAgents
                              join sap in _context.SalesAgentProducts on sa.Id equals sap.SalesAgentId
                              join sac in _context.SalesAgentCommisions on sap.Id equals sac.SalesAgentProductId
                              join p in _context.Products on sap.ProductId equals p.Id
                              join pm in _context.PayOutMasters on sac.PayOutMasterId equals pm.Id
                              where sa.IsActive && !sa.IsDeleted && sap.IsActive && !sap.IsDeleted && sac.IsActive && !sac.IsDeleted && p.IsActive && !p.IsDeleted && pm.IsActive && !pm.IsDeleted
                              && p.Type == ProductTypeConstants.BusinessLoan && pm.Type == PayOutMasterTypeConstants.Disbursment
                              && sa.UserId == request.Request
                              select new GetDSASalesAgentListResponseDc
                              {
                                  UserId = sa.UserId,
                                  AnchorCompanyId = sa.AnchorCompanyId,
                                  FullName = sa.FullName,
                                  MobileNo = sa.MobileNo,
                                  PayoutPercenatge = sac.CommisionPercentage,
                                  Role = sa.Role,
                                  Type = sa.Type,
                                  AgreementStartDate = sa.AgreementStartDate,
                                  AgreementEndDate = sa.AgreementEndDate,
                                  DSACode = sa.DSACode
                              }).FirstOrDefault();
            if (salesAgent != null)
            {
                res.Response = salesAgent;
                res.Status = true;
                res.Message = "Data Found";
            }
            return res;
        }

        public async Task<GRPCReply<GetproductCommissionConfigDC>> GetCommissionConfigByProductId(GRPCRequest<List<long>> request)
        {
            GRPCReply<GetproductCommissionConfigDC> res = new GRPCReply<GetproductCommissionConfigDC> { Message = "Data Not Found" };

            var query = await (from pc in _context.ProductCommissionConfigs
                               join po in _context.PayOutMasters on pc.PayOutMasterId equals po.Id
                               join pr in _context.Products on pc.ProductId equals pr.Id
                               where pc.IsActive && !pc.IsDeleted
                                     && po.IsActive && !po.IsDeleted
                                     && po.Type == PayOutMasterTypeConstants.Disbursment
                                     && pr.Type == ProductTypeConstants.BusinessLoan
                               //&& request.Request.Contains(pc.ProductId)
                               select new GetproductCommissionConfigDC
                               {
                                   CommissionValue = pc.CommisionValue,
                                   ProductId = pc.ProductId
                               }).FirstOrDefaultAsync();

            if (query != null)
            {
                res.Response = query;
                res.Status = true;
                res.Message = "Data Found";
            }
            return res;
        }

        public async Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetSalesAgentDetailsByUserIds(GRPCRequest<List<string>> request)
        {
            GRPCReply<List<GetDSASalesAgentListResponseDc>> res = new GRPCReply<List<GetDSASalesAgentListResponseDc>> { Message = "Data Not Found" };
            var salesAgentList = (from sa in _context.SalesAgents
                                  join sap in _context.SalesAgentProducts on sa.Id equals sap.SalesAgentId
                                  join sac in _context.SalesAgentCommisions on sap.Id equals sac.SalesAgentProductId
                                  join p in _context.Products on sap.ProductId equals p.Id
                                  join pm in _context.PayOutMasters on sac.PayOutMasterId equals pm.Id
                                  where sa.IsActive && !sa.IsDeleted && sap.IsActive && !sap.IsDeleted && sac.IsActive && !sac.IsDeleted && p.IsActive && !p.IsDeleted && pm.IsActive && !pm.IsDeleted
                                  && p.Type == ProductTypeConstants.BusinessLoan && pm.Type == PayOutMasterTypeConstants.Disbursment
                                  && request.Request.Contains(sa.UserId)
                                  select new GetDSASalesAgentListResponseDc
                                  {
                                      UserId = sa.UserId,
                                      AnchorCompanyId = sa.AnchorCompanyId,
                                      FullName = sa.FullName,
                                      MobileNo = sa.MobileNo,
                                      PayoutPercenatge = sac.CommisionPercentage,
                                      Role = sa.Role,
                                      Type = sa.Type,
                                      AgreementStartDate = sa.AgreementStartDate,
                                      AgreementEndDate = sa.AgreementEndDate,
                                      DSACode = sa.DSACode,
                                      CreatedDate = sa.Created
                                  }).ToList();

            if (salesAgentList != null && salesAgentList.Any())
            {
                res.Response = salesAgentList;
                res.Status = true;
                res.Message = "Data Found";
            }
            return res;
        }
        [AllowAnonymous]
        public async Task<GRPCReply<List<SalesAgentDetailDC>>> GetSalesAgentByUserId(GRPCRequest<List<string>> request)
        {
            GRPCReply<List<SalesAgentDetailDC>> res = new GRPCReply<List<SalesAgentDetailDC>> { Message = "Data Not Found" };

            List<SalesAgentDetailDC> result = new List<SalesAgentDetailDC>();

            if (request != null && request.Request.Any())
            {
                List<long> Ids = new List<long>();

                var saleagents = await _context.SalesAgents.Where(x => request.Request.Contains(x.UserId) && x.IsActive && !x.IsDeleted).Select(x => new SalesAgentDetailDC
                {
                    SalesAgentId = x.Id,
                    Type = x.Type,
                    UserId = x.UserId,
                    AnchorCompanyId = x.AnchorCompanyId,
                    GstnNo = x.GstnNo

                }).ToListAsync();
                Ids.AddRange(saleagents.Select(x => x.SalesAgentId).ToList());

                List<long> AnchorCompanyIds = saleagents.Where(x => x.Type == DSAProfileTypeConstants.DSAUser).Select(x => x.AnchorCompanyId).Distinct().ToList();
                var DSaOrConnectors = new List<SalesAgentDetailDC>();
                if (AnchorCompanyIds != null && AnchorCompanyIds.Any())
                {
                    DSaOrConnectors = await _context.SalesAgents.Where(x => AnchorCompanyIds.Contains(x.AnchorCompanyId) &&
                    (x.Type == DSAProfileTypeConstants.DSA) && x.IsActive && !x.IsDeleted).Select(x => new SalesAgentDetailDC
                    {
                        SalesAgentId = x.Id,
                        Type = x.Type,
                        UserId = x.UserId,
                        AnchorCompanyId = x.AnchorCompanyId,
                        GstnNo = x.GstnNo
                    }
                    ).ToListAsync();
                    Ids.AddRange(DSaOrConnectors.Select(x => x.SalesAgentId).ToList());
                }

                var salesAgentCommisionList = (from sa in Ids
                                               join sap in _context.SalesAgentProducts on sa equals sap.SalesAgentId
                                               join sac in _context.SalesAgentCommisions on sap.Id equals sac.SalesAgentProductId
                                               join p in _context.Products on sap.ProductId equals p.Id
                                               join pm in _context.PayOutMasters on sac.PayOutMasterId equals pm.Id
                                               where sap.IsActive && !sap.IsDeleted && sac.IsActive && !sac.IsDeleted && p.IsActive && !p.IsDeleted && pm.IsActive && !pm.IsDeleted
                                               && p.Type == ProductTypeConstants.BusinessLoan && pm.Type == PayOutMasterTypeConstants.Disbursment
                                               select new
                                               {
                                                   SaleagentId = sa,
                                                   sac.CommisionPercentage,
                                                   sap.ProductId,
                                                   sac.MinAmount,
                                                   sac.MaxAmount
                                               }).Distinct().ToList();

                if (salesAgentCommisionList != null && salesAgentCommisionList.Any())
                {
                    foreach (var Id in Ids)
                    {
                        var found = new SalesAgentDetailDC();
                        if (saleagents != null && saleagents.Any(x => x.SalesAgentId == Id))
                        {
                            found = saleagents.First(x => x.SalesAgentId == Id);
                        }
                        else if (DSaOrConnectors != null && DSaOrConnectors.Any(x => x.SalesAgentId == Id))
                        {
                            found = DSaOrConnectors.First(x => x.SalesAgentId == Id);
                        }
                        if (found != null && !string.IsNullOrEmpty(found.UserId))
                        {
                            result.Add(new SalesAgentDetailDC
                            {
                                AnchorCompanyId = found.AnchorCompanyId,
                                Type = found.Type,
                                UserId = found.UserId,
                                GstnNo = found.GstnNo,
                                SalesAgentId = Id,

                                SalesAgentProductPayouts = salesAgentCommisionList.Where(x => x.SaleagentId == Id).Select(x => new SalesAgentProductPayoutDC
                                {
                                    PayOutPercentage = x.CommisionPercentage,
                                    ProductId = x.ProductId,
                                    MaxAmount = x.MaxAmount,
                                    MinAmount = x.MinAmount
                                }).ToList()
                            }); ;
                        }
                    }
                }
                res.Response = result;
                res.Status = true;
                res.Message = "Data Found";
                return res;
            }
            return res;
        }


        public async Task<GRPCReply<string>> GetSalesAgentNameByUserId(GRPCRequest<string> request)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            var saleagentName = await _context.SalesAgents.Where(x => x.UserId == request.Request && x.IsActive && !x.IsDeleted).Select(x => x.FullName).FirstOrDefaultAsync();
            if (saleagentName != null)
            {
                gRPCReply.Status = true;
                gRPCReply.Response = saleagentName;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<ProductSlabConfigResponse>>> GetProductSlabConfigs(GRPCRequest<ProductSlabConfigRequest> request)
        {
            GRPCReply<List<ProductSlabConfigResponse>> reply = new GRPCReply<List<ProductSlabConfigResponse>> { Message = "Data Not Found" };
            reply.Response = await _context.ProductSlabConfigurations.Where(x => x.CompanyId == request.Request.CompanyId && x.ProductId == request.Request.ProductId
            && (request.Request.SlabTypes == null || !request.Request.SlabTypes.Any() || request.Request.SlabTypes.Contains(x.SlabType)) && x.IsActive && !x.IsDeleted)
                .Select(x => new ProductSlabConfigResponse
                {
                    CompanyId = x.CompanyId,
                    ProductId = x.ProductId,
                    SlabType = x.SlabType,
                    MinLoanAmount = x.MinLoanAmount,
                    MaxLoanAmount = x.MaxLoanAmount,
                    ValueType = x.ValueType,
                    MinValue = x.MinValue,
                    MaxValue = x.MaxValue,
                    SharePercentage = x.SharePercentage,
                    IsFixed = x.IsFixed
                }).ToListAsync();
            if (reply.Response != null && reply.Response.Any())
            {
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }
        public async Task<GRPCReply<bool>> DSADeactivate(GRPCRequest<ActivatDeActivateDSALeadRequest> request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            if (request.Request.LeadId > 0)
            {
                var salesAgent = await _context.SalesAgents.FirstOrDefaultAsync(x => x.UserId == request.Request.UserId && !x.IsDeleted);
                if (salesAgent != null)
                {
                    if (request.Request.isReject)
                    {
                        salesAgent.IsActive = request.Request.isActive;
                        salesAgent.IsDeleted = request.Request.isReject;
                        salesAgent.LastModifiedBy = request.Request.UserId;
                        salesAgent.Status = "Rejected";
                    }
                    else
                    {
                        salesAgent.IsActive = request.Request.isActive;
                        salesAgent.Status = request.Request.isActive ? "Activated" : "DeActivated";
                        salesAgent.LastModifiedBy = request.Request.UserId;
                    }
                    _context.Entry(salesAgent).State = EntityState.Modified;
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        gRPCReply.Response = true;
                        gRPCReply.Status = true;
                    }
                }
                else
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Data not found";
                }
            }
            return gRPCReply;
        }


        public async Task<GRPCReply<bool>> GetSalesLeadActivationSatus(string request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            var salesAgent = await _context.SalesAgents.FirstOrDefaultAsync(x => x.UserId == request && !x.IsDeleted);
            if (salesAgent != null && salesAgent.IsActive)
            {
                gRPCReply.Response = true;
                gRPCReply.Status = true;
            }
            else
            {
                gRPCReply.Response = false;
                gRPCReply.Status = false;
                gRPCReply.Message = "Data not found";
            }
            return gRPCReply;
        }


        public async Task<GRPCReply<List<GetDSASalesAgentListResponseDc>>> GetALLDSASalesAgentList(GRPCRequest<string> request)
        {
            GRPCReply<List<GetDSASalesAgentListResponseDc>> res = new GRPCReply<List<GetDSASalesAgentListResponseDc>> { Message = "Data Not Found" };
            var dsaAdmin = await _context.SalesAgents.Where(x => x.UserId == request.Request).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            if (dsaAdmin != null)
            {
                res.Response = await _context.SalesAgents.Where(x => x.AnchorCompanyId == dsaAdmin.AnchorCompanyId).Select(x => new GetDSASalesAgentListResponseDc
                {
                    UserId = x.UserId,
                    AnchorCompanyId = x.AnchorCompanyId,
                    FullName = x.FullName,
                    MobileNo = x.MobileNo,
                    //PayoutPercenatge = x.PayoutPercenatge,
                    Role = x.Role,
                    Type = x.Type,
                    IsActive = x.IsActive,
                    IsDeleted = x.IsDeleted
                }).ToListAsync();
                if (res.Response != null && res.Response.Any())
                {
                    res.Status = true;
                    res.Message = "Data Found";
                }
            }
            return res;
        }
        #endregion

        public async Task<GRPCReply<List<long>>> GetCompanyListByProduct(GRPCRequest<GetCompanyListByProductRequest> request)
        {
            GRPCReply<List<long>> reply = new GRPCReply<List<long>> { Message = "Data Not Found!!!", Response = new List<long>() };
            List<long> productIds = new List<long>();
            if (request.Request.CompanyType == CompanyTypeEnum.Anchor.ToString())
            {
                productIds = await _context.ProductAnchorCompany.Where(x => x.CompanyId == request.Request.CompanyId && x.IsActive && !x.IsDeleted).Select(x => x.ProductId).Distinct().ToListAsync();
            }
            else
            {
                productIds = await _context.ProductNBFCCompany.Where(x => x.CompanyId == request.Request.CompanyId && x.IsActive && !x.IsDeleted).Select(x => x.ProductId).Distinct().ToListAsync();
            }
            if (productIds != null && productIds.Any())
            {
                var companies = await _context.ProductAnchorCompany.Where(x => productIds.Contains(x.ProductId) && x.IsActive && !x.IsDeleted).Select(x => x.CompanyId).Distinct().ToListAsync();
                if (companies != null && companies.Any())
                    reply.Response.AddRange(companies);

                companies = await _context.ProductNBFCCompany.Where(x => productIds.Contains(x.ProductId) && x.IsActive && !x.IsDeleted).Select(x => x.CompanyId).Distinct().ToListAsync();
                if (companies != null && companies.Any())
                    reply.Response.AddRange(companies);

                if (reply.Response != null && reply.Response.Any())
                {
                    reply.Message = "Data Found";
                    reply.Status = true;
                }
            }
            return reply;
        }

        public async Task<GRPCReply<string>> GetDSACurrentCode(GRPCRequest<DSAEntityDc> req)
        {
            GRPCReply<string> res = new GRPCReply<string>();
            if (req != null && !string.IsNullOrEmpty(req.Request.EntityName) && !string.IsNullOrEmpty(req.Request.DSAType))
            {
                var entity_name = new SqlParameter("EntityName", req.Request.EntityName);
                var dsa_Type = new SqlParameter("DSAType", req.Request.DSAType);
                var result = _context.Database.SqlQueryRaw<string>("exec GetDSACode @EntityName,@DSAType ", entity_name, dsa_Type).AsEnumerable().FirstOrDefault();
                if (result != null)
                {
                    res.Status = true;
                    res.Response = result;
                }
            }
            else { res.Message = "Request can not be empty"; }
            return res;
        }

        public async Task<GRPCReply<List<BLNBFCConfigs>>> GetBLNBFCConfigByCompanyIds(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<BLNBFCConfigs>> gRPCReply = new GRPCReply<List<BLNBFCConfigs>> { Message = "Data Not Found!!!" };
            gRPCReply.Response = await _context.ProductNBFCCompany.Where(x => x.IsActive && !x.IsDeleted && x.Product.Type == ProductTypeConstants.BusinessLoan && request.Request.Contains(x.CompanyId)).Select(x => new BLNBFCConfigs
            {
                CompanyId = x.CompanyId,
                ProductId = x.ProductId,
                BounceCharge = x.BounceCharges,
                MinPenalPercent = x.PenaltyCharges,
                MaxPenalPercent = Convert.ToDouble(x.MaxPenaltyCharges ?? 0)
            }).ToListAsync();
            if (gRPCReply.Response != null && gRPCReply.Response.Any())
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Data found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> AddUpdateTokenNBFCCompany(AddUpdateTokenNBFCCompany addUpdateTokenNBFCCompany)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string> { Message = "Token not updated !!!", Status = false };

            var productNBFCCompany = await _context.ProductNBFCCompany.Where(x => x.IsActive && !x.IsDeleted && x.CompanyId == addUpdateTokenNBFCCompany.companyId && x.Product.Type == addUpdateTokenNBFCCompany.productType).FirstOrDefaultAsync();

            if (productNBFCCompany != null)
            {
                productNBFCCompany.Token = addUpdateTokenNBFCCompany.token;
                productNBFCCompany.TokenCreatedDate = DateTime.Now;
                _context.Entry(productNBFCCompany).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                gRPCReply.Status = true;
                gRPCReply.Message = "Token updated successfully";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<string>> GetNBFCProductToken(AddUpdateTokenNBFCCompany addUpdateTokenNBFCCompany)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string> { Message = "Data not found !!!", Status = false };

            var productNBFCCompany = await _context.ProductNBFCCompany.Where(x => x.IsActive && !x.IsDeleted && x.CompanyId == addUpdateTokenNBFCCompany.companyId && x.Product.Type == addUpdateTokenNBFCCompany.productType).FirstOrDefaultAsync();

            if (productNBFCCompany != null && !string.IsNullOrEmpty(productNBFCCompany.Token))
            {

                gRPCReply.Response = productNBFCCompany.Token;
                gRPCReply.Status = true;
            }

            return gRPCReply;
        }

    }
}
