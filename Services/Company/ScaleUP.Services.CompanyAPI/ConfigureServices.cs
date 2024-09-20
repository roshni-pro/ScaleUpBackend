using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.CompanyAPI.Persistence;
using ScaleUP.Services.CompanyAPI.Constants;
using ScaleUP.Services.CompanyAPI.Manager;
using ScaleUP.Services.CompanyModels.Master;
using ScaleUP.Services.CompanyModels;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ScaleUP.Services.CompanyAPI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();

            services.AddScoped<AuditableEntitySaveChangesInterceptor>();
            services.AddDbContext<CompanyApplicationDbContext>(options =>
                options.UseSqlServer(EnvironmentConstants.DbContext
                ,
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.CompanyAPI")
                    ));


            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<CompanyApplicationDbContext>());
            services.AddScoped<CompanyGrpcManager>();
            services.AddScoped<CompanyManager>();

            return services;
        }
    }

    public static class SeedData
    {
        public static void InsertDefaultData(IApplicationBuilder builder)
        {
            CompanyApplicationDbContext context = builder.ApplicationServices
                                         .CreateScope().ServiceProvider.GetRequiredService<CompanyApplicationDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            if (!context.BusinessTypeMasters.Any())

            {
                context.BusinessTypeMasters.AddRange(
                 new BusinessTypeMaster { Name = "Sole proprietorship Business", Value = "proprietorship", IsActive = true, IsDeleted = false },
                 new BusinessTypeMaster { Name = "Partnership Business", Value = "partnership", IsActive = true, IsDeleted = false },
                 new BusinessTypeMaster { Name = "Limited Liability partnership", Value = "llp", IsActive = true, IsDeleted = false },
                 new BusinessTypeMaster { Name = "Private Limited Company", Value = "private_ltd_company", IsActive = true, IsDeleted = false },
                 new BusinessTypeMaster { Name = "Public limited company", Value = "public_ltd_company", IsActive = true, IsDeleted = false },
                 new BusinessTypeMaster { Name = "Hindu Undivided Family Business", Value = "huf", IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }

            if (!context.Companies.Any())
            {
                var bustypeId = context.BusinessTypeMasters.FirstOrDefault(x => x.Value == "private_ltd_company").Id;
                context.Companies.Add(new Companies
                {
                    GSTNo = "23ABMCS3131N1Z5",
                    BusinessContactEmail = "customer.care@scaleupfin.com",
                    BusinessContactNo = "9981531563",
                    BusinessName = "SCALEUPFINCAP PRIVATE LIMITED",
                    BusinessTypeId = Convert.ToInt32(bustypeId),
                    CompanyType = "NBFC",
                    IsDefault = true,
                    IsSelfConfiguration = true,
                    LendingName = "Scaleup",
                    IsActive = true,
                    IsDeleted = false,
                    BusinessHelpline = "0000000001",
                    CompanyCode = "CN/1"
                });
                context.SaveChanges();
            }


            if (!context.EntityMasters.Any())
            {
                context.EntityMasters.Add(new EntityMaster
                {
                    EntityName = "Company",
                    DefaultNo = 1,
                    EntityQuery = "select @numCnt= count(Id) from Companies where CompanyCode = @numberStr",
                    Separator = "",
                    IsActive = true,
                    IsDeleted = false
                });
                context.SaveChanges();
            }

            if (!context.EntitySerialMasters.Any())
            {
                var entitymasterid = context.EntityMasters.FirstOrDefault(x => x.EntityName == "Company").Id;
                context.EntitySerialMasters.AddRange(new EntitySerialMaster
                {
                    EntityId = entitymasterid,
                    Prefix = "SCA0",
                    StartFrom = 1,
                    NextNumber = 1,
                    StateId = 0,
                    IsActive = true,
                    IsDeleted = false,
                    Suffix = "",
                    CompanyType = "ANCHOR"
                },
                new EntitySerialMaster
                {
                    EntityId = entitymasterid,
                    Prefix = "SCN0",
                    StartFrom = 1,
                    NextNumber = 1,
                    StateId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    Suffix = "",
                    CompanyType = "NBFC"
                }
                );
                context.SaveChanges();
            }

            if (!context.GSTMasters.Any())
            {
                context.GSTMasters.Add(new GstMaster
                {
                    GSTRate = 18,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddYears(1),
                    IsActive = true,
                    IsDeleted = false
                });
                context.SaveChanges();
            }
        }
    }
}
