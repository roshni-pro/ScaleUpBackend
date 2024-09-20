using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScaleUP.BuildingBlocks.EventBus.Constants;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.KYCAPI.Constants;
using ScaleUP.Services.KYCAPI.Consumers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Helpers;
using ScaleUP.Global.Infrastructure.Common.MassTransitMiddleware;

namespace ScaleUP.Services.KYCAPI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddScoped<AuditableEntitySaveChangesInterceptor>();

            services.AddScoped<Token>();

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
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.KYCAPI")
                    ));
            ////}

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            //services.AddScoped<ApplicationDbContextInitialiser>();

            //services.AddTransient<IDateTime, DateTimeService>();
            // services.AddTransient<IIdentityService, IdentityService>(); 

            //services.AddAuthentication()
            //    .AddIdentityServerJwt();

            //services.AddAuthorization(options =>
            //    options.AddPolicy("CanPurge", policy => policy.RequireRole("Administrator")));

            //services.AddMassTransit(x =>
            services.AddMassTransit(x =>
            {
                

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseSendFilter(typeof(TokenSendFilter<>), context);
                    cfg.UsePublishFilter(typeof(TokenPublishFilter<>), context);
                    cfg.UseConsumeFilter(typeof(TokenConsumeFilter<>), context);

                    if (EnvironmentConstants.EnvironmentName == "Development")
                    {
                        cfg.Host(EnvironmentConstants.RabbitMQUrl, Convert.ToUInt16(EnvironmentConstants.RabbitPort), EnvironmentConstants.RabbitVHost, host =>
                        {
                            host.Username(EnvironmentConstants.RabbitUser);
                            host.Password(EnvironmentConstants.RabbitPwd);
                        });
                    }
                    else
                    {
                        cfg.Host(EnvironmentConstants.RabbitMQUrl, EnvironmentConstants.RabbitVHost, host =>
                        {
                            host.Username(EnvironmentConstants.RabbitUser);
                            host.Password(EnvironmentConstants.RabbitPwd);
                        });
                    }
                    cfg.ReceiveEndpoint(QueuesConsts.LeadActivityCreatedEventQueueName, x =>
                    {
                        x.ConfigureConsumer<LeadActivityCreatedEventConsumer>(context);
                    });
                    cfg.ReceiveEndpoint(QueuesConsts.UpdatingAadharAddressEventQueueName, x =>
                    {
                        x.ConfigureConsumer<LeadUpdateAadharEventConsumer>(context);
                    });

                    //cfg.ReceiveEndpoint(QueuesConsts.OrderFailedEventtQueueName, x =>
                    //{
                    //    x.ConfigureConsumer<OrderFailedEventConsumer>(context);
                    //});
                });

                x.AddConsumer<LeadActivityCreatedEventConsumer>(cfg =>
                {
                    cfg.ConcurrentMessageLimit = 100;
                });

                x.AddConsumer<LeadUpdateAadharEventConsumer>(cfg =>
                {
                    cfg.ConcurrentMessageLimit = 100;
                });
                x.AddRequestClient<ICheckStatus>();
            });

            services.AddScoped<IMassTransitService, MassTransitService>();

            services.AddScoped<KycGrpcManager>();
            services.AddScoped<KYCMasterInfoManager>();
            services.AddScoped<ThirdPartyAPIConfigManager>();
            services.AddScoped<KarzaPanProfileHelper>();
            services.AddScoped<MasterEntryManager>();
            services.AddScoped<KYCHistoryManager>();
            return services;
        }
    }
}
