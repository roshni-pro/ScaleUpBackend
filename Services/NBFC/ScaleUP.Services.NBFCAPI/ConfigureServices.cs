using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.NBFCAPI.Persistence;
using ScaleUP.Services.NBFCAPI.Constants;
using ScaleUP.Services.NBFCAPI.Manager;

namespace ScaleUP.Services.NBFCAPI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
           
            services.AddScoped<AuditableEntitySaveChangesInterceptor>();
            services.AddDbContext<NBFCApplicationDbContext>(options =>
                options.UseSqlServer(EnvironmentConstants.DbContext
                ,
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.NBFCAPI")
                    ));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<NBFCApplicationDbContext>());

            services.AddScoped<NBFCGrpcManager>();

            return services;
        }

    }
}
