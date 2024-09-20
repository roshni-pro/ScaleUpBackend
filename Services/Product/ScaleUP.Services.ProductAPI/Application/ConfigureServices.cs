using ScaleUP.Services.ProductAPI.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ScaleUP.Services.ProductAPI.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //services.AddAutoMapper(Assembly.GetExecutingAssembly());
            //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            //services.AddMediatR()
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

            return services;
        }
        //    public static void AddDefaultData(WebApplication app)
        //    {


        //        using (var serviceScope = app.Services.GetRequiredService<ProductApplicationDbContext>().CreateScope())
        //        {
        //            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        //        }
        //}
    }
}
