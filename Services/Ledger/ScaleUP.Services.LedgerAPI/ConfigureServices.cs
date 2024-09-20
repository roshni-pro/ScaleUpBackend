using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LedgerAPI.Persistence;

namespace ScaleUP.Services.LedgerAPI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddScoped<AuditableEntitySaveChangesInterceptor>();

            services.AddDbContext<LedgerApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("LedgerAPIContext")
                ,
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.LedgerAPI")
                    ));


            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<LedgerApplicationDbContext>());


            return services;
        }
    }
}
