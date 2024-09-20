using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScaleUP.BuildingBlocks.Logging;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.ProductAPI.Constants;
using ScaleUP.Services.ProductAPI.Manager;
using ScaleUP.Services.ProductAPI.Persistence;
using ScaleUP.Services.ProductModels.DSA;
using ScaleUP.Services.ProductModels.Master;

namespace ScaleUP.Services.ProductAPI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddScoped<AuditableEntitySaveChangesInterceptor>();

            services.AddDbContext<ProductApplicationDbContext>(options =>
                options.UseSqlServer(EnvironmentConstants.DbContext
                ,
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.ProductAPI")
                    ));
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ProductApplicationDbContext>());
            services.AddScoped<NBFCCompanyApiManager>();
            services.AddScoped<NBFCSubActivityApiManager>();

            return services;
        }
    }


    public static class SeedData
    {
        public static void InsertDefaultData(IApplicationBuilder builder)
        {
            ProductApplicationDbContext context = builder.ApplicationServices
                                         .CreateScope().ServiceProvider.GetRequiredService<ProductApplicationDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                new Product { Name = "Business Loan", Type = "BusinessLoan", Description = "Business Loan Description", IsActive = true, IsDeleted = false, ProductCode = "BusinessLoan01" },
                new Product { Name = "CreditLine", Type = "CreditLine", Description = "CreditLine Description", IsActive = true, IsDeleted = false, ProductCode = "CreditLine01" }
                );
                context.SaveChanges();
            }
            if (!context.ActivityMasters.Any())
            {
                context.ActivityMasters.AddRange(
                new ActivityMasters { ActivityName = "MobileOtp", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 1, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "KYC", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 2, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "PersonalInfo", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 3, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "BusinessInfo", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 4, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Bank Detail", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 5, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Statement", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 6, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "CreditBureau", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 7, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Inprogress", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 8, IsActive = true, IsDeleted = false },
                 new ActivityMasters { ActivityName = "Generate Offer", CompanyType = "NBFC", FrontOrBack = "Back", Sequence = 9, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Show Offer", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 10, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Rejected", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 11, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Emandate", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 12, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Agreement", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 13, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Disbursement", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 14, IsActive = true, IsDeleted = false },
                new ActivityMasters { ActivityName = "Disbursement Completed", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 15, IsActive = true, IsDeleted = false },
                 new ActivityMasters { ActivityName = "MyAccount", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 16, IsActive = true, IsDeleted = false },

                 new ActivityMasters { ActivityName = "MSME", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 17, IsActive = true, IsDeleted = false },
                 new ActivityMasters { ActivityName = "Arthmate Show Offer", CompanyType = "FinTech", FrontOrBack = "Front", Sequence = 18, IsActive = true, IsDeleted = false },
                 new ActivityMasters { ActivityName = "Arthmate Agreement", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 19, IsActive = true, IsDeleted = false },
                 new ActivityMasters { ActivityName = "Congratulations", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 20, IsActive = true, IsDeleted = false },
                 new ActivityMasters { ActivityName = "LoanDetail", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 21, IsActive = true, IsDeleted = false },
                 new ActivityMasters { ActivityName = "PFCollection", CompanyType = "NBFC", FrontOrBack = "Front", Sequence = 22, IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }
            if (!context.SubActivityMasters.Any())
            {
                var KycActivityId = context.ActivityMasters.FirstOrDefault(x => x.ActivityName == "KYC").Id;
                var GenOffActivityId = context.ActivityMasters.FirstOrDefault(x => x.ActivityName == "Generate Offer").Id;
                var AgreementActivityId = context.ActivityMasters.FirstOrDefault(x => x.ActivityName == "Agreement").Id;
                context.SubActivityMasters.AddRange(
                new SubActivityMasters { Name = "Pan", ActivityMasterId = KycActivityId, KycMasterCode = " ", Sequence = 1, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "Aadhar", ActivityMasterId = KycActivityId, KycMasterCode = " ", Sequence = 2, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "Selfie", ActivityMasterId = KycActivityId, KycMasterCode = " ", Sequence = 3, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "PersonalInfo", ActivityMasterId = KycActivityId, KycMasterCode = " ", Sequence = 4, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "BusinessInfo", ActivityMasterId = KycActivityId, KycMasterCode = " ", Sequence = 5, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "MSME", ActivityMasterId = KycActivityId, KycMasterCode = " ", Sequence = 6, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "Statement", ActivityMasterId = KycActivityId, KycMasterCode = " ", Sequence = 7, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "CreateLead", ActivityMasterId = GenOffActivityId, KycMasterCode = " ", Sequence = 1, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "SendToLos", ActivityMasterId = GenOffActivityId, KycMasterCode = " ", Sequence = 2, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "AScore", ActivityMasterId = GenOffActivityId, KycMasterCode = " ", Sequence = 3, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "Ceplar", ActivityMasterId = GenOffActivityId, KycMasterCode = " ", Sequence = 4, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "Offer", ActivityMasterId = GenOffActivityId, KycMasterCode = " ", Sequence = 5, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "Self Offer", ActivityMasterId = GenOffActivityId, KycMasterCode = " ", Sequence = 6, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "PrepareAgreement", ActivityMasterId = AgreementActivityId, KycMasterCode = " ", Sequence = 1, IsActive = true, IsDeleted = false },
                new SubActivityMasters { Name = "AgreementEsign", ActivityMasterId = AgreementActivityId, KycMasterCode = " ", Sequence = 2, IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }

            if (!context.ProductActivityMasters.Any())
            {
                var allProd = context.Products.Where(x => x.Type == "BusinessLoan" || x.Type == "CreditLine").Select(x => new { x.Id, x.Type }).ToList();
                if (allProd.Any())
                {
                    var AllActivity = (from b in context.ActivityMasters
                                       join c in context.SubActivityMasters
                                       on b.Id equals c.ActivityMasterId into activitygroup
                                       from allactivity in activitygroup.DefaultIfEmpty()
                                       select new
                                       {
                                           ActivityId = b.Id,
                                           ActivityName = b.ActivityName,
                                           SubactivityId = allactivity == null ? (long?)null : allactivity.Id,
                                           SubactivityName = allactivity == null ? (string?)null : allactivity.Name,
                                           ActivitySeq = b.Sequence,
                                           SubActivitySeq = allactivity == null ? (int?)null : allactivity.Sequence
                                       }).ToList();
                    List<string> creditLineActivities = new List<string> { "MobileOtp", "KYC", "PersonalInfo", "BusinessInfo", "Bank Detail", "Statement", "Inprogress", "Generate Offer", "Show Offer", "Agreement", "Rejected", "Disbursement", "Disbursement Completed", "MyAccount" };
                    List<string> creditLineSubActivities = new List<string> { "Pan", "Aadhar", "Selfie", "CreateLead", "SendToLos", "Self Offer", "PrepareAgreement", "AgreementEsign" };

                    List<string> businessLoanActivities = new List<string> { "MobileOtp", "KYC", "PersonalInfo", "BusinessInfo", "Bank Detail", "MSME", "Inprogress", "Generate Offer", "Arthmate Show Offer", "Arthmate Agreement", "Rejected", "Congratulations", "LoanDetail" };
                    List<string> businessLoanSubActivities = new List<string> { "Pan", "Aadhar", "Selfie", "CreateLead", "AScore", "Ceplar", "Offer" };
                    var prodActivities = (from a in allProd
                                          from b in AllActivity
                                          select new
                                          {
                                              ProductId = a.Id,
                                              ProductType = a.Type,
                                              ActivityId = b.ActivityId,
                                              ActivityName = b.ActivityName,
                                              SubactivityId = b.SubactivityId,
                                              SubactivityName = b.SubactivityName,
                                              ActivitySeq = b.ActivitySeq,
                                              SubActivitySeq = b.SubActivitySeq
                                          }).Where(x => (x.ProductType == "BusinessLoan" && businessLoanActivities.Contains(x.ActivityName) && (string.IsNullOrEmpty(x.SubactivityName) || businessLoanSubActivities.Contains(x.SubactivityName)))
                                          || (x.ProductType == "CreditLine" && creditLineActivities.Contains(x.ActivityName) && (string.IsNullOrEmpty(x.SubactivityName) || creditLineSubActivities.Contains(x.SubactivityName)))).ToList();


                    if (prodActivities.Any())
                    {
                        foreach (var prodActivitie in prodActivities.GroupBy(x => x.ProductId))
                        {
                            var dbprodActivities = prodActivitie.ToList().OrderBy(x => x.ActivitySeq).ThenBy(x => x.SubActivitySeq).Select((val, indx) => new ProductActivityMasters
                            {
                                ActivityMasterId = val.ActivityId,
                                ProductId = val.ProductId,
                                Sequence = indx + 1,
                                IsActive = true,
                                IsDeleted = false,
                                SubActivityMasterId = val.SubactivityId
                            }).ToList();
                            context.ProductActivityMasters.AddRange(dbprodActivities);
                        }

                        context.SaveChanges();
                    }
                }
            }

            if (!context.ProductCompanyActivityMasters.Any())
            {
                var Prod = context.Products.Where(x => x.Type == "CreditLine").Select(x => new { x.Id, x.Type }).FirstOrDefault();
                if (Prod != null)
                {
                    var AllActivity = (from b in context.ActivityMasters
                                       join c in context.SubActivityMasters
                                       on b.Id equals c.ActivityMasterId into activitygroup
                                       from allactivity in activitygroup.DefaultIfEmpty()
                                       select new
                                       {
                                           ActivityId = b.Id,
                                           ActivityName = b.ActivityName,
                                           SubactivityId = allactivity == null ? (long?)null : allactivity.Id,
                                           SubactivityName = allactivity == null ? (string?)null : allactivity.Name,
                                           ActivitySeq = b.Sequence,
                                           SubActivitySeq = allactivity == null ? (int?)null : allactivity.Sequence
                                       }).ToList();

                    List<string> creditLineActivities = new List<string> { "MobileOtp", "KYC", "PersonalInfo", "BusinessInfo", "Bank Detail", "Inprogress", "Generate Offer", "Show Offer", "Agreement", "Rejected", "Disbursement", "Disbursement Completed", "MyAccount" };
                    List<string> creditLineSubActivities = new List<string> { "Pan", "Aadhar", "Selfie", "Self Offer" };
                    var prodActivities = (from b in AllActivity
                                          select new
                                          {
                                              ProductId = Prod.Id,
                                              ActivityId = b.ActivityId,
                                              ActivityName = b.ActivityName,
                                              SubactivityId = b.SubactivityId,
                                              SubactivityName = b.SubactivityName,
                                              ActivitySeq = b.ActivitySeq,
                                              SubActivitySeq = b.SubActivitySeq
                                          }).Where(x => (creditLineActivities.Contains(x.ActivityName) && (string.IsNullOrEmpty(x.SubactivityName) || creditLineSubActivities.Contains(x.SubactivityName)))).ToList();


                    if (prodActivities.Any())
                    {
                        foreach (var prodActivitie in prodActivities.GroupBy(x => x.ProductId))
                        {
                            var dbprodActivities = prodActivitie.OrderBy(x => x.ActivitySeq).ThenBy(x => x.SubActivitySeq).Select((val, indx) => new ProductCompanyActivityMasters
                            {
                                ActivityMasterId = val.ActivityId,
                                ProductId = val.ProductId,
                                CompanyId = 1,
                                Sequence = indx + 1,
                                IsActive = true,
                                IsDeleted = false,
                                SubActivityMasterId = val.SubactivityId
                            }).ToList();
                            context.ProductCompanyActivityMasters.AddRange(dbprodActivities);
                        }
                        context.SaveChanges();
                    }
                }
            }

            if (!context.CreditDayMasters.Any())
            {
                context.CreditDayMasters.AddRange(
                new CreditDayMasters { Name = "Custom", Days = 0, IsActive = true, IsDeleted = false },
                new CreditDayMasters { Name = "2 Days", Days = 2, IsActive = true, IsDeleted = false },
                new CreditDayMasters { Name = "7 Days", Days = 7, IsActive = true, IsDeleted = false },
                new CreditDayMasters { Name = "14 Days", Days = 14, IsActive = true, IsDeleted = false },
                new CreditDayMasters { Name = "21 Days", Days = 21, IsActive = true, IsDeleted = false },
                new CreditDayMasters { Name = "30 Days", Days = 30, IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }
            if (!context.EMIOptionMasters.Any())
            {
                context.EMIOptionMasters.AddRange(
                new EMIOptionMasters { Name = "Fifteen Days", IsActive = true, IsDeleted = false },
                new EMIOptionMasters { Name = "Monthly", IsActive = true, IsDeleted = false },
                new EMIOptionMasters { Name = "Quaterly", IsActive = true, IsDeleted = false },
                new EMIOptionMasters { Name = "Half-Yearly", IsActive = true, IsDeleted = false },
                new EMIOptionMasters { Name = "Yearly", IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }
            if (!context.PayOutMasters.Any())
            {
                context.PayOutMasters.AddRange(
                new PayOutMaster { Type = PayOutMasterTypeConstants.Disbursment, IsActive = true, IsDeleted = false },
                new PayOutMaster { Type = PayOutMasterTypeConstants.PF, IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }
        }
    }
}
