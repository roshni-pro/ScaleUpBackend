using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.ProductAPI.Persistence;
using ScaleUP.Services.ProductDTO.DSA;
using ScaleUP.Services.ProductDTO.Master;
using ScaleUP.Services.ProductModels.Master;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ScaleUP.Services.ProductAPI.Manager
{
    public class ProductManager
    {
        private readonly ProductApplicationDbContext _context;

        public ProductManager(ProductApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductResponse> InsertProduct(ProductDTO.Master.ProductDTO product)
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "Issue during product adding.";

            Product _product = new Product
            {
                Description = product.Description,
                Name = product.Name,
                Type = product.Type,
                Created = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                ProductCode = product.Type + "01"
            };
            _context.Products.Add(_product);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                product.Id = _product.Id;

                CommonResponse.Status = true;
                CommonResponse.Message = "Product added successfully.";
                CommonResponse.ReturnObject = product;
            }
            return CommonResponse;
        }

        public async Task<ProductResponse> UpdateProduct(ProductDTO.Master.ProductDTO product)
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "Issue during product updating.";
            Product _product = await _context.Products.FirstOrDefaultAsync(x => x.Id == product.Id);
            if (_product != null)
            {
                _product = new Product
                {
                    Description = product.Description,
                    Name = product.Name,
                    Type = product.Type,
                    Created = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    ProductCode = _product.ProductCode
                };
                _product.LastModified = DateTime.Now;
                _context.Entry(_product).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    CommonResponse.Status = true;
                    CommonResponse.Message = "Product updated successfully.";
                    CommonResponse.ReturnObject = product;
                }
            }
            return CommonResponse;
        }

        public async Task<ProductResponse> GetProduct(long ProductId)
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "Issue in fetching product.";
            var product = await _context.Products.Where(x => x.Id == ProductId && x.IsActive && !x.IsDeleted).AsNoTracking().FirstOrDefaultAsync();
            if (product != null)
            {
                CommonResponse.ReturnObject = new ProductDTO.Master.ProductDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Type = product.Type
                };
                CommonResponse.Status = true;
                CommonResponse.Message = "Record found";
            }
            return CommonResponse;

        }
        public async Task<ProductResponse> GetProductList()
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "Issue in fetching product list.";
            var products = await _context.Products.Where(x => x.IsActive && !x.IsDeleted).AsNoTracking().ToListAsync();
            if (products != null && products.Any())
            {
                CommonResponse.ReturnObject = products;
                CommonResponse.Status = true;
                CommonResponse.Message = "Records found";
            }
            return CommonResponse;
        }
        public async Task<ProductResponse> GetProductMasterListById(long CompanyId, string CompanyType)
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "No Data Found.";
            if (!string.IsNullOrEmpty(CompanyType) && CompanyType.ToLower() == "anchor")
            {
                var CompanyProducts = await _context.ProductAnchorCompany.Where(x => x.CompanyId == CompanyId && !x.IsDeleted).AsNoTracking().Select(y => y.ProductId).ToListAsync();
                var products = await _context.Products.Where(x => x.IsActive && !x.IsDeleted && (CompanyProducts == null || !CompanyProducts.Contains(x.Id))).AsNoTracking().ToListAsync();
                if (products != null && products.Any())
                {
                    CommonResponse.ReturnObject = products.Select(x => new ProductDTO.Master.ProductDTO
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type
                    }).ToList();
                    CommonResponse.Status = true;
                    CommonResponse.Message = "Record found";
                }
            }
            else
            {
                var CompanyProducts = await _context.ProductNBFCCompany.Where(x => x.CompanyId == CompanyId && !x.IsDeleted).AsNoTracking().Select(y => y.ProductId).ToListAsync();
                var products = await _context.Products.Where(x => x.IsActive && !x.IsDeleted && (CompanyProducts == null || !CompanyProducts.Contains(x.Id))).AsNoTracking().ToListAsync();
                if (products != null && products.Any())
                {
                    CommonResponse.ReturnObject = products.Select(x => new ProductDTO.Master.ProductDTO
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type
                    }).ToList();
                    CommonResponse.Status = true;
                    CommonResponse.Message = "Record found";
                }
            }
            return CommonResponse;

        }
        public async Task<ProductResponse> ProductActivityMasterList(long CompanyId, long ProductId)
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "Issue in fetching Product Activity Master list.";
            List<ProductActivitySubActivityListDTO> list = new List<ProductActivitySubActivityListDTO>();
            //var productactivitymasters2 = await _context.ProductCompanyActivityMasters.Where(x => x.ProductId == ProductId && x.CompanyId == CompanyId && !x.IsDeleted).Include(y => y.ActivityMasters).AsNoTracking().Include(c => c.SubActivityMasters).ToListAsync();
            var query = from p in _context.ProductActivityMasters
                        join pc in _context.ProductCompanyActivityMasters
                        on new { p.ProductId, p.ActivityMasterId, p.SubActivityMasterId, CompanyId, IsDeleted = false } equals new { pc.ProductId, pc.ActivityMasterId, pc.SubActivityMasterId, pc.CompanyId, pc.IsDeleted } into pj
                        from pc in pj.DefaultIfEmpty()
                        where p.ProductId == ProductId && p.IsActive && !p.IsDeleted && p.ActivityMasters.CompanyType.ToLower() == "nbfc"
                        select new
                        {
                            Id = pc != null ? pc.Id : 0,
                            p.ProductId,
                            p.ActivityMasterId,
                            p.SubActivityMasterId,
                            p.Sequence,
                            p.ActivityMasters,
                            p.SubActivityMasters,
                            IsActive = pc != null ? pc.IsActive : false
                        };
            var productactivitymasters = query.Distinct().ToList();
            if (productactivitymasters != null && productactivitymasters.Any())
            {
                int i = 1, j = 1;

                foreach (var ProductActivityMasterDTO in productactivitymasters.GroupBy(x => x.ActivityMasterId).Select(x => new { activityid = x.Key, Sequence = x.Min(y => y.Sequence) }).OrderBy(x => x.Sequence))
                {
                    j = 1;
                    var activityDcs = productactivitymasters.Where(x => x.ActivityMasterId == ProductActivityMasterDTO.activityid).ToList();
                    List<SubActivityMastersDTO> subactivityDcs = new List<SubActivityMastersDTO>();
                    foreach (var item in activityDcs.OrderBy(x => x.Sequence))
                    {
                        if (item.SubActivityMasterId.HasValue)
                        {
                            subactivityDcs.Add(new SubActivityMastersDTO
                            {
                                ProductCompanyActivityMasterId = item.Id,
                                SubActivityMasterId = item.SubActivityMasterId.Value,
                                Sequence = j,
                                Name = item.SubActivityMasters.Name,
                                IsActive = item.IsActive,
                                ApiCount = _context.CompanyApis.Where(x => x.CompanyId == CompanyId && x.IsActive && !x.IsDeleted).Count()
                            });
                            j++;
                        }
                    }
                    list.Add(new ProductActivitySubActivityListDTO
                    {
                        ActivityMasterId = activityDcs[0].ActivityMasterId,
                        Sequence = i,
                        Activity = activityDcs[0].ActivityMasters.ActivityName,
                        SubActivity = subactivityDcs,
                        IsActive = activityDcs[0].IsActive
                    });
                    i++;
                }

                CommonResponse.ReturnObject = list;
                CommonResponse.Status = true;
                CommonResponse.Message = "Record found";
            }
            return CommonResponse;

        }

        public async Task<ProductResponse> GetProductActivitySubActivityMasterList(string CompanyType, bool IsDefault)
        {
            ProductResponse CommonResponse = new ProductResponse();

            List<ProductActivitySubActivityListDTO> list = new List<ProductActivitySubActivityListDTO>();

            var ActivityList = await _context.ActivityMasters.Where(x => x.IsActive && !x.IsDeleted && (IsDefault || x.CompanyType == CompanyType)).AsNoTracking().OrderBy(x => x.Sequence).ToListAsync();//&& x.FrontOrBack == "Front"
            if (ActivityList != null && ActivityList.Any())
            {
                foreach (var activity in ActivityList)
                {
                    ProductActivitySubActivityListDTO productActivitySubActivityListDTO = new ProductActivitySubActivityListDTO();
                    productActivitySubActivityListDTO.Activity = activity.ActivityName;
                    productActivitySubActivityListDTO.Sequence = activity.Sequence;
                    productActivitySubActivityListDTO.ActivityMasterId = activity.Id;
                    productActivitySubActivityListDTO.SubActivity = await _context.SubActivityMasters.Where(x => x.IsActive && !x.IsDeleted && x.ActivityMasterId == activity.Id).AsNoTracking().OrderBy(x => x.Sequence)
                        .Select(y => new SubActivityMastersDTO
                        {
                            Name = y.Name,
                            Sequence = y.Sequence,
                            SubActivityMasterId = y.Id,

                        }).ToListAsync();

                    list.Add(productActivitySubActivityListDTO);
                }
                CommonResponse.Status = true;
                CommonResponse.ReturnObject = list;
            }
            else
            {
                CommonResponse.Status = false;
                CommonResponse.Message = "Company Activity Not Exist";
            }
            return CommonResponse;
        }
        public async Task<ProductResponse> GetProductActivityMasterList(long ProductId, bool IsDefault)
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "Product Activities Not Found.";
            var activityMasterList = await _context.ActivityMasters.Where(x => (IsDefault || x.CompanyType == "NBFC") && x.IsActive && !x.IsDeleted).AsNoTracking().Select(x => x.Id).ToListAsync();
            if (activityMasterList != null && activityMasterList.Any())
            {
                List<ProductActivitySubActivityListDTO> list = new List<ProductActivitySubActivityListDTO>();
                var productactivitymasters = await _context.ProductActivityMasters.Where(x => x.ProductId == ProductId && x.IsActive && !x.IsDeleted && activityMasterList.Contains(x.ActivityMasterId)).AsNoTracking().Include(y => y.ActivityMasters).Include(c => c.SubActivityMasters).ToListAsync();
                if (productactivitymasters != null && productactivitymasters.Any())
                {
                    int i = 1, j = 1;
                    foreach (var ProductActivityMasterDTO in productactivitymasters.GroupBy(x => x.ActivityMasterId).Select(x => new { activityid = x.Key, Sequence = x.Min(y => y.Sequence) }).OrderBy(x => x.Sequence))
                    {
                        j = 1;
                        var activityDcs = productactivitymasters.Where(x => x.ActivityMasterId == ProductActivityMasterDTO.activityid).ToList();
                        List<SubActivityMastersDTO> subactivityDcs = new List<SubActivityMastersDTO>();
                        foreach (var item in activityDcs.OrderBy(x => x.Sequence))
                        {
                            if (item.SubActivityMasterId.HasValue)
                            {
                                subactivityDcs.Add(new SubActivityMastersDTO
                                {
                                    SubActivityMasterId = item.SubActivityMasterId.Value,
                                    Sequence = j,
                                    Name = item.SubActivityMasters.Name,
                                    IsActive = item.IsActive,
                                    ApiCount = 0//_context.CompanyApis.Where(x => x.CompanyId == CompanyId && x.IsActive && !x.IsDeleted).Count()
                                });
                                j++;
                            }
                        }
                        list.Add(new ProductActivitySubActivityListDTO
                        {
                            ActivityMasterId = activityDcs[0].ActivityMasterId,
                            Sequence = i,
                            Activity = activityDcs[0].ActivityMasters.ActivityName,
                            SubActivity = subactivityDcs,
                            IsActive = activityDcs[0].IsActive
                        });
                        i++;
                    }

                    CommonResponse.ReturnObject = list;
                    CommonResponse.Status = true;
                    CommonResponse.Message = "Record found";
                }
            }
            return CommonResponse;

        }

        public async Task<ProductResponse> AddProductActivityMaster(ProductActivityMasterDTO productactivitymaster)
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "Issue in Adding Product Activity Master.";

            var product = _context.Products.Where(x => x.Id == productactivitymaster.ProductId && x.IsActive && !x.IsActive).AsNoTracking().FirstOrDefaultAsync();

            if (product != null)
            {
                ProductCompanyActivityMasters _productactivitymasters = new ProductCompanyActivityMasters
                {
                    ProductId = productactivitymaster.ProductId,
                    ActivityMasterId = productactivitymaster.ActivityMasterId,
                    CompanyId = productactivitymaster.CompanyId,
                    SubActivityMasterId = productactivitymaster.SubActivityMasterId,
                    Sequence = productactivitymaster.Sequence,
                    IsActive = true,
                    IsDeleted = false
                };
                _context.ProductCompanyActivityMasters.Add(_productactivitymasters);
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    productactivitymaster.Id = _productactivitymasters.Id;
                    CommonResponse.Status = true;
                    CommonResponse.Message = "Product Activity Master added successfully.";
                    CommonResponse.ReturnObject = productactivitymaster;
                }
            }
            return CommonResponse;

        }

        public async Task<ProductResponse> AddUpdateProductActivityMaster(List<ProductActivityMasterDTO> productActivityMasterList)
        {
            ProductResponse CommonResponse = new ProductResponse();
            foreach (var item in productActivityMasterList)
            {
                item.SubActivityMasterId = item.SubActivityMasterId == 0 ? null : item.SubActivityMasterId;
                var product = await _context.ProductCompanyActivityMasters.FirstOrDefaultAsync(x => x.CompanyId == item.CompanyId && x.ProductId == item.ProductId && x.ActivityMasterId == item.ActivityMasterId && x.SubActivityMasterId == item.SubActivityMasterId && !x.IsDeleted);//&& x.IsActive
                if (product != null)
                {
                    product.Sequence = item.Sequence;
                    product.IsActive = item.IsActive;
                    _context.Entry(product).State = EntityState.Modified;
                }
                else
                {
                    ProductCompanyActivityMasters _productactivitymasters = new ProductCompanyActivityMasters
                    {
                        ProductId = item.ProductId,
                        ActivityMasterId = item.ActivityMasterId,
                        CompanyId = item.CompanyId,
                        SubActivityMasterId = item.SubActivityMasterId,
                        Sequence = item.Sequence,
                        IsActive = item.IsActive,
                        IsDeleted = false
                    };
                    _context.ProductCompanyActivityMasters.Add(_productactivitymasters);
                }
            }
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                CommonResponse.Status = true;
                CommonResponse.Message = "Product Activity Master Updated successfully.";
                CommonResponse.ReturnObject = productActivityMasterList;
            }
            else
            {
                CommonResponse.Status = false;
                CommonResponse.Message = "Issue in Updating Product Activity Master.";
            }
            return CommonResponse;
        }

        public async Task<ProductResponse> UpdateProductActivityMaster(ProductActivityMasterDTO productactivitymaster)
        {
            ProductResponse CommonResponse = new ProductResponse();
            CommonResponse.Status = false;
            CommonResponse.Message = "Issue in updating Product Activity Master.";

            ProductCompanyActivityMasters? _productactivitymasters = await _context.ProductCompanyActivityMasters.FirstOrDefaultAsync(x => x.Id == productactivitymaster.Id && x.IsActive && !x.IsActive);
            if (_productactivitymasters != null)
            {
                _productactivitymasters = new ProductCompanyActivityMasters
                {
                    ProductId = productactivitymaster.ProductId,
                    ActivityMasterId = productactivitymaster.ActivityMasterId,
                    CompanyId = productactivitymaster.CompanyId,
                    SubActivityMasterId = productactivitymaster.SubActivityMasterId,
                    Sequence = productactivitymaster.Sequence,
                };
                _context.Entry(_productactivitymasters).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    CommonResponse.Status = true;
                    CommonResponse.Message = "Product Activity Master Updated successfully.";
                    CommonResponse.ReturnObject = productactivitymaster;
                }
            }
            return CommonResponse;

        }

        #region Product Company
        public async Task<ProductResponse<List<EMIOptionMasters>>> GetEMIOptionMasterList()
        {
            ProductResponse<List<EMIOptionMasters>> CommonResponse = new ProductResponse<List<EMIOptionMasters>>();
            var list = await _context.EMIOptionMasters.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
            if (list != null && list.Any())
            {
                CommonResponse.Status = true;
                CommonResponse.Message = "Data Found";
                CommonResponse.ReturnObject = list;
            }
            else
            {
                CommonResponse.Status = false;
                CommonResponse.Message = "No Data Found";
            }
            return CommonResponse;
        }

        public async Task<ProductResponse<List<CreditDayMasters>>> GetCreditDayMastersList()
        {
            ProductResponse<List<CreditDayMasters>> CommonResponse = new ProductResponse<List<CreditDayMasters>>();
            var list = await _context.CreditDayMasters.Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.Days).ToListAsync();
            if (list != null && list.Any())
            {
                CommonResponse.Status = true;
                CommonResponse.Message = "Data Found";
                CommonResponse.ReturnObject = list;
            }
            else
            {
                CommonResponse.Status = false;
                CommonResponse.Message = "No Data Found";
            }
            return CommonResponse;
        }

        public async Task<ProductResponse<AddUpdateAnchorProductConfigDTO>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigDTO request)
        {
            ProductResponse<AddUpdateAnchorProductConfigDTO> response = new ProductResponse<AddUpdateAnchorProductConfigDTO>();
            var existing = await _context.ProductAnchorCompany.Where(x => x.Id == request.Id && x.CompanyId == request.CompanyId && x.ProductId == request.ProductId).FirstOrDefaultAsync();

            if (existing == null && _context.ProductAnchorCompany.Any(x => x.CompanyId == request.CompanyId && x.ProductId == request.ProductId && !x.IsDeleted))
            {
                response.Status = false;
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
                var existingSlabConfigurations = await _context.ProductSlabConfigurations.Where(x => x.ProductId == existing.ProductId && x.CompanyId == existing.CompanyId && !x.IsDeleted).ToListAsync();
                if (existingSlabConfigurations != null && existingSlabConfigurations.Any())
                {
                    foreach (var item in existingSlabConfigurations)
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
                    existing.MaxInterestRate = request.MaxInterestRate; //new
                    existing.IseSignEnable = request.IseSignEnable; //new
                    existing.PlatFormFee = request.PlatFormFee; //new


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
                    //if (request.ProductSlabConfigs != null && request.ProductSlabConfigs.Any())
                    //{
                    //    List<ProductSlabConfiguration> slabConfigurationList = new List<ProductSlabConfiguration>();
                    //    foreach (var item in request.ProductSlabConfigs)
                    //    {
                    //        ProductSlabConfiguration productSlabConfiguration = new ProductSlabConfiguration
                    //        {
                    //            CompanyId = existing.CompanyId,
                    //            ProductId = existing.ProductId,
                    //            SlabType = item.SlabType,
                    //            MinLoanAmount = item.MinLoanAmount,
                    //            MaxLoanAmount = item.MaxLoanAmount,
                    //            ValueType = item.ValueType,
                    //            MinValue = item.MinValue,
                    //            MaxValue = item.MaxValue,
                    //            IsFixed = item.IsFixed,
                    //            SharePercentage = item.SharePercentage,
                    //            IsActive = true,
                    //            IsDeleted = false
                    //        };
                    //        slabConfigurationList.Add(productSlabConfiguration);
                    //    }
                    //    _context.AddRange(slabConfigurationList);
                    //}
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
                //if (request.ProductSlabConfigs != null && request.ProductSlabConfigs.Any())
                //{
                //    List<ProductSlabConfiguration> slabConfigurationList = new List<ProductSlabConfiguration>();
                //    foreach (var item in request.ProductSlabConfigs)
                //    {
                //        ProductSlabConfiguration productSlabConfiguration = new ProductSlabConfiguration
                //        {
                //            CompanyId = request.CompanyId,
                //            ProductId = request.ProductId,
                //            SlabType = item.SlabType,
                //            MinLoanAmount = item.MinLoanAmount,
                //            MaxLoanAmount = item.MaxLoanAmount,
                //            ValueType = item.ValueType,
                //            MinValue = item.MinValue,
                //            MaxValue = item.MaxValue,
                //            IsFixed = item.IsFixed,
                //            SharePercentage = item.SharePercentage,
                //            IsActive = true,
                //            IsDeleted = false
                //        };
                //        slabConfigurationList.Add(productSlabConfiguration);
                //    }
                //    _context.AddRange(slabConfigurationList);
                //}
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
                    BlackSoilReferralCode = request.BlackSoilReferralCode,
                    MaxInterestRate = request.MaxInterestRate,
                    IseSignEnable = request.IseSignEnable,
                    PlatFormFee = request.PlatFormFee
                };
                _context.Add(productAnchorCompany);
            }
            int rowChanged = await _context.SaveChangesAsync();

            if (rowChanged > 0)
            {
                response.Status = true;
                response.Message = isAdded ? "Product Added Successfully" : "Product Updated Successfully";
                response.ReturnObject = request;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed to Add/Update Product";
            }
            return response;
        }

        public async Task<ProductResponse<List<AddUpdateAnchorProductConfigDTO>>> GetAnchorProductConfig(long CompanyId, long ProductId)
        {
            ProductResponse<List<AddUpdateAnchorProductConfigDTO>> response = new ProductResponse<List<AddUpdateAnchorProductConfigDTO>>();
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == ProductId && !x.IsDeleted);
            var slabConfigs = await _context.ProductSlabConfigurations.Where(x => x.CompanyId == CompanyId && x.ProductId == ProductId && x.IsActive && !x.IsDeleted).ToListAsync();
            var list = await _context.ProductAnchorCompany.Where(x => x.CompanyId == CompanyId && x.ProductId == ProductId && !x.IsDeleted).Include(x => x.CompanyEMIOptions).Include(x => x.CompanyCreditDays).Select
                (x => new AddUpdateAnchorProductConfigDTO
                {
                    AnnualInterestRate = x.AnnualInterestRate,
                    BounceCharge = x.BounceCharge,
                    CompanyId = x.CompanyId,
                    //CreditDays = x.CreditDays,
                    DelayPenaltyRate = x.DelayPenaltyRate,
                    DisbursementTAT = x.DisbursementTAT,
                    Id = x.Id,
                    MaxTenureInMonth = x.MaxTenureInMonth,
                    MinTenureInMonth = x.MinTenureInMonth,
                    ProcessingFeePayableBy = x.ProcessingFeePayableBy,
                    ProcessingFeeRate = x.ProcessingFeeRate,
                    ProcessingFeeType = x.ProcessingFeeType,
                    ProductId = x.ProductId,
                    ProductName = product.Name,
                    ProductType = product.Type,
                    AnnualInterestPayableBy = x.AnnualInterestPayableBy,
                    //TransactionFeeRate = x.TransactionFeeRate,
                    //TransactionFeeType = x.TransactionFeeType,
                    EMIBounceCharge = x.EMIBounceCharge,
                    EMIRate = x.EMIRate,
                    EMIProcessingFeeRate = x.EMIProcessingFeeRate,
                    EMIPenaltyRate = x.EMIPenaltyRate,
                    MinLoanAmount = x.MinLoanAmount,
                    MaxLoanAmount = x.MaxLoanAmount,
                    CommissionPayout = x.CommissionPayout,
                    ConsiderationFee = x.ConsiderationFee,
                    DisbursementSharingCommission = x.DisbursementSharingCommission,
                    AgreementEndDate = x.AgreementEndDate,
                    AgreementStartDate = x.AgreementStartDate,
                    AgreementURL = x.AgreementURL,
                    AgreementDocId = x.AgreementDocId,
                    BlackSoilReferralCode = x.BlackSoilReferralCode,
                    OfferMaxRate = x.OfferMaxRate,
                    CustomCreditDays = x.CustomCreditDays,
                    IseSignEnable = x.IseSignEnable,
                    MaxInterestRate = x.MaxInterestRate,
                    PlatFormFee = x.PlatFormFee,


                    CompanyEMIOptions = x.CompanyEMIOptions.Where(y => y.IsActive && !y.IsDeleted).Select(y => new CompanyEMIOptionsDTO { EMIOptionMasterId = y.EMIOptionMasterId, IsActive = y.IsActive, IsDeleted = y.IsDeleted, ProductAnchorCompanyId = y.ProductAnchorCompanyId }).ToList(),
                    CompanyCreditDays = x.CompanyCreditDays.Where(y => y.IsActive && !y.IsDeleted).Select(y => new CompanyCreditDaysDTO { CreditDaysMasterId = y.CreditDaysMasterId, ProductAnchorCompanyId = y.ProductAnchorCompanyId, IsActive = y.IsActive, IsDeleted = y.IsDeleted }).ToList(),
                    //ProductSlabConfigs = slabConfigs != null && slabConfigs.Any() ? slabConfigs.Select(z => new ProductSlabConfigDTO
                    //{
                    //    SlabType = z.SlabType,
                    //    MinLoanAmount = z.MinLoanAmount,
                    //    MaxLoanAmount = z.MaxLoanAmount,
                    //    ValueType = z.ValueType,
                    //    MinValue = z.MinValue,
                    //    MaxValue = z.MaxValue,
                    //    SharePercentage = z.SharePercentage,
                    //    IsFixed = z.IsFixed
                    //}).ToList() : new List<ProductSlabConfigDTO>()

                }).ToListAsync();
            if (list != null && list.Any())
            {
                response.Status = true;
                response.Message = "Data Found";
                response.ReturnObject = list;
            }
            else
            {
                response.Status = false;
                response.Message = "Data Not Found";
            }
            return response;
        }

        public async Task<ProductResponse<List<AnchorProductListDTO>>> GetAnchorProductList(long CompanyId)
        {
            ProductResponse<List<AnchorProductListDTO>> response = new ProductResponse<List<AnchorProductListDTO>>();
            var query = from pc in _context.ProductAnchorCompany
                        join p in _context.Products on pc.ProductId equals p.Id
                        where !pc.IsDeleted && p.IsActive && !p.IsDeleted && pc.CompanyId == CompanyId
                        select new AnchorProductListDTO
                        {
                            Id = pc.Id,
                            CompanyId = pc.CompanyId,
                            ProductId = pc.ProductId,
                            ProductName = p.Name,
                            ProductType = p.Type,
                            AnnualInterestRate = pc.AnnualInterestRate,
                            BounceCharge = pc.BounceCharge,
                            DelayPenaltyRate = pc.DelayPenaltyRate,
                            DisbursementTAT = pc.DisbursementTAT,
                            MaxTenureInMonth = pc.MaxTenureInMonth,
                            MinTenureInMonth = pc.MinTenureInMonth,
                            ProcessingFeePayableBy = pc.ProcessingFeePayableBy,
                            ProcessingFeeRate = pc.ProcessingFeeRate,
                            ProcessingFeeType = pc.ProcessingFeeType,
                            AnnualInterestPayableBy = pc.AnnualInterestPayableBy,
                            //TransactionFeeRate = pc.TransactionFeeRate,
                            //TransactionFeeType = pc.TransactionFeeType,
                            EMIPenaltyRate = pc.EMIPenaltyRate,
                            EMIProcessingFeeRate = pc.EMIProcessingFeeRate,
                            EMIRate = pc.EMIRate,
                            EMIBounceCharge = pc.EMIBounceCharge,
                            MinLoanAmount = pc.MinLoanAmount,
                            MaxLoanAmount = pc.MaxLoanAmount,
                            CommissionPayout = pc.CommissionPayout,
                            ConsiderationFee = pc.ConsiderationFee,
                            DisbursementSharingCommission = pc.DisbursementSharingCommission,
                            AgreementEndDate = pc.AgreementEndDate,
                            AgreementStartDate = pc.AgreementStartDate,
                            AgreementURL = pc.AgreementURL,
                            AgreementDocId = pc.AgreementDocId,
                            OfferMaxRate = pc.OfferMaxRate,
                            CustomCreditDays = pc.CustomCreditDays,
                            IsActive = pc.IsActive,
                            IsDeleted = pc.IsDeleted,
                            BlackSoilReferralCode = pc.BlackSoilReferralCode,
                        };
            var anchorProducts = await query.ToListAsync();
            if (anchorProducts != null && anchorProducts.Any())
            {
                response.Status = true;
                response.Message = "Data Found";
                response.ReturnObject = anchorProducts;
            }
            else
            {
                response.Status = false;
                response.Message = "Data Not Found";
            }
            return response;
        }

        public async Task<ProductResponse<bool>> AnchorProductActiveInactive(long AnchorProductId, bool IsActive)
        {
            ProductResponse<bool> response = new ProductResponse<bool>();
            var _companyproduct = await _context.ProductAnchorCompany.Where(x => x.Id == AnchorProductId && !x.IsDeleted).FirstOrDefaultAsync();
            if (_companyproduct != null)
            {
                _companyproduct.IsActive = IsActive;

                _context.Entry(_companyproduct).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    response.Status = true;
                    response.Message = "Updated";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Not Updated";
                }
            }
            else
            {
                response.Status = false;
                response.Message = "Not Found";
            }
            return response;
        }

        public async Task<ProductResponse<AddUpdateNBFCProductConfigDTO>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigDTO request)
        {
            ProductResponse<AddUpdateNBFCProductConfigDTO> response = new ProductResponse<AddUpdateNBFCProductConfigDTO>();
            var existing = await _context.ProductNBFCCompany.Where(x => x.Id == request.Id && x.CompanyId == request.CompanyId && x.ProductId == request.ProductId).FirstOrDefaultAsync();
            if (existing == null && _context.ProductNBFCCompany.Any(x => x.CompanyId == request.CompanyId && x.ProductId == request.ProductId && !x.IsDeleted))
            {
                response.Status = false;
                response.Message = "This Type of Product is Already Exists!!!";
                return response;
            }
            var existingSlabConfigurations = await _context.ProductSlabConfigurations.Where(x => x.ProductId == request.ProductId && x.CompanyId == request.CompanyId && !x.IsDeleted).ToListAsync();
            if (existingSlabConfigurations != null && existingSlabConfigurations.Any())
            {
                foreach (var item in existingSlabConfigurations)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    _context.Entry(item).State = EntityState.Modified;
                }
            }
            if (request.ProductSlabConfigs != null && request.ProductSlabConfigs.Any())
            {
                List<ProductSlabConfiguration> slabConfigurationList = new List<ProductSlabConfiguration>();
                foreach (var item in request.ProductSlabConfigs)
                {
                    ProductSlabConfiguration productSlabConfiguration = new ProductSlabConfiguration
                    {
                        CompanyId = request.CompanyId,
                        ProductId = request.ProductId,
                        SlabType = item.SlabType,
                        MinLoanAmount = item.MinLoanAmount,
                        MaxLoanAmount = item.MaxLoanAmount,
                        ValueType = item.ValueType,
                        MinValue = item.MinValue,
                        MaxValue = item.MaxValue,
                        IsFixed = item.IsFixed,
                        SharePercentage = item.SharePercentage,
                        IsActive = true,
                        IsDeleted = false
                    };
                    slabConfigurationList.Add(productSlabConfiguration);
                }
                _context.AddRange(slabConfigurationList);
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
                    //existing.AgreementEndDate = request.AgreementEndDate;
                    //existing.AgreementStartDate = request.AgreementStartDate;
                    //existing.AgreementURL = request.AgreementURL;
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
                    existing.PFSharePercentage = request.PFSharePercentage;
                    existing.ArrangementType = request.ArrangementType;
                    existing.Tenure = request.Tenure;
                    existing.MaxBounceCharges = request.MaxBounceCharges;
                    existing.MaxPenaltyCharges = request.MaxPenaltyCharges;
                    existing.IseSignEnable = request.IseSignEnable;
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
                        PFSharePercentage = request.PFSharePercentage,
                        ArrangementType = request.ArrangementType,
                        Tenure = request.Tenure,
                        MaxPenaltyCharges = request.MaxPenaltyCharges,
                        MaxBounceCharges = request.MaxBounceCharges,
                        IseSignEnable = request.IseSignEnable,
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
                    PFSharePercentage = request.PFSharePercentage,
                    ArrangementType = request.ArrangementType,
                    Tenure = request.Tenure,
                    MaxBounceCharges = request.MaxBounceCharges,
                    MaxPenaltyCharges = request.MaxPenaltyCharges,
                    IseSignEnable = request.IseSignEnable,
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
                response.ReturnObject = request;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed to Add/Update Product";
            }
            return response;
        }

        public async Task<ProductResponse<List<GetNBFCProductConfigDTO>>> GetNBFCProductConfig(long CompanyId, long ProductId)
        {
            ProductResponse<List<GetNBFCProductConfigDTO>> response = new ProductResponse<List<GetNBFCProductConfigDTO>>();
            var slabConfigs = await _context.ProductSlabConfigurations.Where(x => x.CompanyId == CompanyId && x.ProductId == ProductId && x.IsActive && !x.IsDeleted).ToListAsync();
            var list = await _context.ProductNBFCCompany.Where(x => x.CompanyId == CompanyId && x.ProductId == ProductId && !x.IsDeleted).Include(x => x.Product).Select(x => new GetNBFCProductConfigDTO
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                ProductId = x.ProductId,
                AgreementDocId = x.AgreementDocId,
                AgreementStartDate = x.AgreementStartDate,
                AgreementEndDate = x.AgreementEndDate,
                AgreementURL = x.AgreementURL,
                BounceCharges = x.BounceCharges,
                CustomerAgreementDocId = x.CustomerAgreementDocId,
                CustomerAgreementType = x.CustomerAgreementType,
                CustomerAgreementURL = x.CustomerAgreementURL,
                DisbursementType = x.DisbursementType,
                AnnualInterestRate = x.AnnualInterestRate,
                IsBounceChargeCoSharing = x.IsBounceChargeCoSharing,
                IsInterestRateCoSharing = x.IsInterestRateCoSharing,
                IsPenaltyChargeCoSharing = x.IsPenaltyChargeCoSharing,
                IsPlatformFeeCoSharing = x.IsPlatformFeeCoSharing,
                PenaltyCharges = x.PenaltyCharges,
                PlatformFee = x.PlatformFee,
                ProcessingFee = x.ProcessingFee,
                ProcessingFeeType = x.ProcessingFeeType,
                SanctionLetterDocId = x.SanctionLetterDocId,
                SanctionLetterURL = x.SanctionLetterURL,
                ArrangementType = x.ArrangementType,
                PFSharePercentage = x.PFSharePercentage,
                ProductName = x.Product.Name,
                ProductType = x.Product.Type,
                Tenure = x.Tenure,
                MaxPenaltyCharges = x.MaxPenaltyCharges,
                MaxBounceCharges = x.MaxBounceCharges,
                IseSignEnable = x.IseSignEnable,
                ProductSlabConfigs = slabConfigs != null && slabConfigs.Any() ? slabConfigs.Select(z => new ProductSlabConfigDTO
                {
                    SlabType = z.SlabType,
                    MinLoanAmount = z.MinLoanAmount,
                    MaxLoanAmount = z.MaxLoanAmount,
                    ValueType = z.ValueType,
                    MinValue = z.MinValue,
                    MaxValue = z.MaxValue,
                    SharePercentage = z.SharePercentage,
                    IsFixed = z.IsFixed
                }).ToList() : new List<ProductSlabConfigDTO>()
            }).ToListAsync();
            if (list != null && list.Any())
            {
                response.Status = true;
                response.Message = "Data Found";
                response.ReturnObject = list;
            }
            else
            {
                response.Status = false;
                response.Message = "Data Not Found";
            }
            return response;
        }

        public async Task<ProductResponse<List<NBFCProductListDTO>>> GetNBFCProductList(long CompanyId)
        {
            ProductResponse<List<NBFCProductListDTO>> response = new ProductResponse<List<NBFCProductListDTO>>();
            var query = from pc in _context.ProductNBFCCompany
                        join p in _context.Products on pc.ProductId equals p.Id
                        where !pc.IsDeleted && p.IsActive && !p.IsDeleted && pc.CompanyId == CompanyId
                        select new NBFCProductListDTO
                        {
                            Id = pc.Id,
                            CompanyId = pc.CompanyId,
                            ProductId = pc.ProductId,
                            ProductName = p.Name,
                            ProductType = p.Type,
                            BounceCharges = pc.BounceCharges,
                            InterestRate = pc.AnnualInterestRate,
                            PenaltyCharges = pc.PenaltyCharges,
                            ProcessingFeeType = pc.ProcessingFeeType,
                            PlatformFee = pc.PlatformFee,
                            ProcessingFee = pc.ProcessingFee,
                            AgreementEndDate = pc.AgreementEndDate,
                            AgreementStartDate = pc.AgreementStartDate,
                            AgreementURL = pc.AgreementURL,
                            CustomerAgreementType = pc.CustomerAgreementType,
                            CustomerAgreementURL = pc.CustomerAgreementURL,
                            CustomerAgreementDocId = pc.CustomerAgreementDocId,
                            AgreementDocId = pc.AgreementDocId,
                            SanctionLetterDocId = pc.SanctionLetterDocId,
                            SanctionLetterURL = pc.SanctionLetterURL,
                            IsInterestRateCoSharing = pc.IsInterestRateCoSharing,
                            IsBounceChargeCoSharing = pc.IsBounceChargeCoSharing,
                            IsPenaltyChargeCoSharing = pc.IsPenaltyChargeCoSharing,
                            IsPlatformFeeCoSharing = pc.IsPlatformFeeCoSharing,
                            DisbursementType = pc.DisbursementType,
                            IsActive = pc.IsActive,
                            IsDeleted = pc.IsDeleted
                        };
            var nbfcProducts = await query.ToListAsync();
            if (nbfcProducts != null && nbfcProducts.Any())
            {
                response.Status = true;
                response.Message = "Data Found";
                response.ReturnObject = nbfcProducts;
            }
            else
            {
                response.Status = false;
                response.Message = "Data Not Found";
            }
            return response;
        }

        public async Task<ProductResponse<bool>> NBFCProductActiveInactive(long NBFCProductId, bool IsActive)
        {
            ProductResponse<bool> response = new ProductResponse<bool>();
            var _companyproduct = await _context.ProductNBFCCompany.Where(x => x.Id == NBFCProductId && !x.IsDeleted).FirstOrDefaultAsync();
            if (_companyproduct != null)
            {
                _companyproduct.IsActive = IsActive;

                _context.Entry(_companyproduct).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    response.Status = true;
                    response.Message = "Updated";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Not Updated";
                }
            }
            else
            {
                response.Status = false;
                response.Message = "Not Found";
            }
            return response;
        }

        public async Task<ProductTemplateResponseDc> SaveModifyTemplateMaster(ProductTemplateDc templatedc)
        {
            ProductTemplateResponseDc temp = new ProductTemplateResponseDc();

            if (templatedc != null && templatedc.TemplateCode != null && templatedc.TemplateType != null && templatedc.TemplateID == null)
            {
                var exist = await _context.ProductTemplateMasters.Where(x => x.TemplateType == templatedc.TemplateType && x.TemplateCode == templatedc.TemplateCode && x.IsDeleted == false).FirstOrDefaultAsync();
                if (exist == null)
                {
                    var smstemp = new ProductTemplateMaster
                    {
                        DLTID = templatedc.DLTID,
                        TemplateType = templatedc.TemplateType,
                        TemplateCode = templatedc.TemplateCode,
                        Template = templatedc.Template,
                        IsActive = templatedc.Status,
                        IsDeleted = false,
                        Created = DateTime.Now
                    };
                    _context.ProductTemplateMasters.Add(smstemp);
                    await _context.SaveChangesAsync();
                    temp.Status = true;
                    temp.Message = "successfully added!";
                }
                else
                {
                    temp.Message = "Template code already exist!!";
                }
            }
            else
            {

                var data = await _context.ProductTemplateMasters.Where(x => x.Id == templatedc.TemplateID && x.IsDeleted == false).Select(x => x).FirstOrDefaultAsync();
                if (data != null)
                {
                    if (data.TemplateType == "SMS")
                    {
                        data.DLTID = templatedc.DLTID;
                    }
                    data.Template = templatedc.Template;
                    data.IsActive = templatedc.Status;
                    data.LastModified = DateTime.Now;
                    _context.Entry(data).State = EntityState.Modified;
                    int isSaved = await _context.SaveChangesAsync();
                    if (isSaved > 0)
                    {
                        temp.Status = true;
                        temp.Message = "successfully Modified!";
                    }
                    else
                    {
                        temp.Status = false;
                        temp.Message = "failed to Save";
                    }
                }
                else
                {
                    temp.Status = false;
                    temp.Message = "data not found!";
                }
            }
            return temp;
        }
        #endregion


        #region DSA

        public async Task<ResultViewModel<List<GetDSASalesAgentListResponseDc>>> GetDSASalesAgentList(GRPCRequest<string> request)
        {
            ResultViewModel<List<GetDSASalesAgentListResponseDc>> res = new ResultViewModel<List<GetDSASalesAgentListResponseDc>> { Message = "Data Not Found" };
            var dsaAdmin = await _context.SalesAgents.FirstOrDefaultAsync(x => x.UserId == request.Request && x.Type == DSAProfileTypeConstants.DSA && x.IsActive && !x.IsDeleted);
            if (dsaAdmin != null)
            {
                res.Result = await _context.SalesAgents.Where(x => x.AnchorCompanyId == dsaAdmin.AnchorCompanyId && x.Type == DSAProfileTypeConstants.DSAUser && x.IsActive && !x.IsDeleted).Select(x => new GetDSASalesAgentListResponseDc
                {
                    UserId = x.UserId,
                    AnchorCompanyId = x.AnchorCompanyId,
                    FullName = x.FullName,
                    MobileNo = x.MobileNo,
                    Role = x.Role,
                    Type = x.Type
                }).ToListAsync();
                if (res.Result != null && res.Result.Any())
                {
                    res.Result.Add(new GetDSASalesAgentListResponseDc
                    {
                        UserId = "",
                        AnchorCompanyId = 0,
                        FullName = "All Agents",
                        MobileNo = "",
                        Role = "",
                        Type = ""
                    });
                    res.IsSuccess = true;
                    res.Message = "Data Found";
                }
            }
            return res;
        }
        public async Task<ResultViewModel<List<GetDSAAgentUsersListResponseDc>>> GetDSAUsersList(string UserId, int skip, int take)
        {
            ResultViewModel<List<GetDSAAgentUsersListResponseDc>> res = new ResultViewModel<List<GetDSAAgentUsersListResponseDc>> { Message = "Data Not Found" };
            var dsaAdmin = await _context.SalesAgents.FirstOrDefaultAsync(x => x.UserId == UserId && x.Type == DSAProfileTypeConstants.DSA && !x.IsDeleted);
            if (dsaAdmin != null)
            {
                res.Result = (from sa in _context.SalesAgents
                              join sap in _context.SalesAgentProducts on sa.Id equals sap.SalesAgentId
                              join sac in _context.SalesAgentCommisions on sap.Id equals sac.SalesAgentProductId
                              join p in _context.Products on sap.ProductId equals p.Id
                              join pm in _context.PayOutMasters on sac.PayOutMasterId equals pm.Id
                              where !sa.IsDeleted && sap.IsActive && !sap.IsDeleted && sac.IsActive && !sac.IsDeleted && p.IsActive && !p.IsDeleted && pm.IsActive && !pm.IsDeleted
                              && p.Type == ProductTypeConstants.BusinessLoan && pm.Type == PayOutMasterTypeConstants.Disbursment
                              && sa.AnchorCompanyId == dsaAdmin.AnchorCompanyId && sa.Type == DSAProfileTypeConstants.DSAUser
                              select new GetDSAAgentUsersListResponseDc
                              {
                                  UserId = sa.UserId,
                                  AnchorCompanyId = sa.AnchorCompanyId,
                                  FullName = sa.FullName,
                                  MobileNo = sa.MobileNo,
                                  PayoutPercenatge = sac.CommisionPercentage,
                                  Role = sa.Role,
                                  Type = sa.Type,
                                  CreatedDate = sa.Created,
                                  EmailId = sa.EmailId,
                                  Status = sa.IsActive,
                                  AgreementEndDate = sa.AgreementEndDate,
                                  AgreementStatDate = sa.AgreementStartDate,
                                  TotalRecords = 0
                              }).ToList();

                if (res.Result != null && res.Result.Any())
                {
                    if (take > 0)
                    {
                        int TotalRecords = res.Result.Count();
                        res.Result = res.Result.Skip(skip).Take(take).ToList();
                        res.Result.ForEach(x => x.TotalRecords = TotalRecords);
                    }
                    res.IsSuccess = true;
                    res.Message = "Data Found";
                }
            }
            return res;
        }

        public async Task<ResultViewModel<string>> DSAUserStatusChange(DSAUserActivationDc req)
        {
            ResultViewModel<string> res = new ResultViewModel<string>() { Message = "Data not found" };
            var salesAgent = await _context.SalesAgents.FirstOrDefaultAsync(x => x.UserId == req.UserId && x.Type == DSAProfileTypeConstants.DSAUser && !x.IsDeleted);
            if (salesAgent != null)
            {
                salesAgent.IsActive = req.Status;
                _context.Entry(salesAgent).State = EntityState.Modified;
                if (await _context.SaveChangesAsync() > 0)
                {
                    res.IsSuccess = true;
                    res.Message = req.Status ? "User Activated successfully!" : "User Deactivated successfully!";
                }
            }
            return res;
        }


        public async Task<ResultViewModel<string>> SaveDSAPayoutPercentage(SavePayoutDc req)
        {
            ResultViewModel<string> res = new ResultViewModel<string> { Message = "Failed to save" };
            var saCommision = (from sa in _context.SalesAgents
                               join sap in _context.SalesAgentProducts on sa.Id equals sap.SalesAgentId
                               join sac in _context.SalesAgentCommisions on sap.Id equals sac.SalesAgentProductId
                               join p in _context.Products on sap.ProductId equals p.Id
                               join pm in _context.PayOutMasters on sac.PayOutMasterId equals pm.Id
                               where sa.IsActive && !sa.IsDeleted && sap.IsActive && !sap.IsDeleted && sac.IsActive && !sac.IsDeleted && p.IsActive && !p.IsDeleted && pm.IsActive && !pm.IsDeleted
                               && p.Type == ProductTypeConstants.BusinessLoan && pm.Type == PayOutMasterTypeConstants.Disbursment
                               && sa.UserId == req.UserId
                               select sac).FirstOrDefault();
            if (saCommision != null)
            {
                saCommision.CommisionPercentage = req.PayoutPercentage;
                _context.Entry(saCommision).State = EntityState.Modified;
            }
            else
                res.Message = "Existing Commission not Found!!!";
            if (await _context.SaveChangesAsync() > 0)
            {
                res.IsSuccess = true;
                res.Message = "Data saved successfully!";
            }
            return res;
        }

        public async Task<ResultViewModel<List<GetSalesAgentDataDc>>> GetSalesAgentListByAnchorId(long anchorCompanyId)
        {
            ResultViewModel<List<GetSalesAgentDataDc>> reply = new ResultViewModel<List<GetSalesAgentDataDc>> { Message = "Data Not Found" };
            reply.Result = await _context.SalesAgents.Where(x => x.AnchorCompanyId == anchorCompanyId && x.IsActive && !x.IsDeleted).Select(x => new GetSalesAgentDataDc
            {
                UserId = x.UserId,
                FullName = x.FullName,
                Type = x.Type
            }).ToListAsync();
            if (reply.Result != null && reply.Result.Any())
            {
                reply.IsSuccess = true;
                reply.Message = "Data Found";
            }
            return reply;
        }

        public async Task<ResultViewModel<string>> DSAUserName(string UserId)
        {
            ResultViewModel<string> res = new ResultViewModel<string>() { Message = "Data not found" };
            var salesAgent = await _context.SalesAgents.FirstOrDefaultAsync(x => x.UserId == UserId && !x.IsDeleted);
            if (salesAgent != null)
            {
                res.Result = salesAgent.FullName;
                res.IsSuccess = true;
            }
            return res;
        }
        #endregion
    }
}
