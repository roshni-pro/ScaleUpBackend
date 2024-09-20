using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.MediaAPI.Persistence;
using ScaleUP.Services.MediaAPI.Constants;
using ScaleUP.Services.MediaAPI.Manager;

namespace ScaleUP.Services.MediaAPI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();

            services.AddScoped<AuditableEntitySaveChangesInterceptor>();

            //if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            //{
            //    services.AddDbContext<ApplicationDbContext>(options =>
            //        options.UseInMemoryDatabase("OrderDb"));
            //}
            //else
            //{
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(EnvironmentConstants.DbContext
                ,
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.MediaAPI")
                    ));

            

            ////}

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<MediaGrpcManager>();
            //services.AddScoped<ApplicationDbContextInitialiser>();

            //services.AddTransient<IDateTime, DateTimeService>();
            // services.AddTransient<IIdentityService, IdentityService>(); 

            //services.AddAuthentication()
            //    .AddIdentityServerJwt();

            //services.AddAuthorization(options =>
            //    options.AddPolicy("CanPurge", policy => policy.RequireRole("Administrator")));

            //services.AddMassTransit(x =>
            //{
            //    x.AddConsumer<OrderCompletedEventConsumer>();
            //    x.AddConsumer<OrderFailedEventConsumer>();

            //    x.UsingRabbitMq((context, cfg) =>
            //    {
            //        cfg.Host(configuration.GetSection("AppSettings")["RabbitMQUrl"], configuration.GetSection("AppSettings")["RabbitVHost"], host =>
            //        {
            //            host.Username(configuration.GetSection("AppSettings")["RabbitUser"]);
            //            host.Password(EnvironmentConstants.RabbitPwd);
            //        });

            //        cfg.ReceiveEndpoint(QueuesConsts.OrderCompletedEventtQueueName, x =>
            //        {
            //            x.ConfigureConsumer<OrderCompletedEventConsumer>(context);
            //        });

            //        cfg.ReceiveEndpoint(QueuesConsts.OrderFailedEventtQueueName, x =>
            //        {
            //            x.ConfigureConsumer<OrderFailedEventConsumer>(context);
            //        });
            //    });
            //});

            //services.AddScoped<IMassTransitService, MassTransitService>();

            return services;
        }
    }
}
