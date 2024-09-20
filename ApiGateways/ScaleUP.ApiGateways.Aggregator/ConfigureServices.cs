using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.Logging;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using Serilog;
using ProtoBuf.Grpc.Client;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using ScaleUP.ApiGateways.Aggregator.Extensions;
using ScaleUP.ApiGateways.Aggregator.Constants;
using Grpc.Net.Client;
using Grpc.Core;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.Interfaces;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Ledger.Interfaces;
using ScaleUP.ApiGateways.Aggregator.Managers.NBFC;
using System.Threading.Channels;
using RabbitMQ.Client;
using Grpc.Net.Client.Configuration;
using ProtoBuf.Grpc.ClientFactory;
using System;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using MassTransit;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Common.MassTransitMiddleware;
using ScaleUP.ApiGateways.Aggregator.Persistence;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.ApiGateways.Aggregator.Managers.ConsumerAppHome;


namespace ScaleUP.ApiGateways.Aggregator
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddOtherServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<AuditableEntitySaveChangesInterceptor>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(EnvironmentConstants.DbContext
                ,
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.ApiGateways.Aggregator")
                    ));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());



            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ILocationService, LocationService>();
            services.AddSingleton<ILeadService, LeadService>();
            services.AddSingleton<IKycService, KycService>();
            services.AddSingleton<IMediaService, MediaService>();
            services.AddSingleton<ICommunicationService, CommunicationService>();
            services.AddSingleton<IIdentityService, IdentityService>();
            services.AddSingleton<ICompanyService, CompanyService>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<INBFCService, NBFCService>();
            services.AddSingleton<ILoanAccountService, LoanAccountService>();
            services.AddSingleton<ILedgerService, LedgerService>();

            services.AddScoped<LeadListDetailsManager>();
            services.AddScoped<LeadMobileValidateManager>();
            services.AddScoped<KYCUserDetailManager>();
            services.AddScoped<CompanyManager>();
            services.AddScoped<eNachAggManager>();
            services.AddScoped<LoanAccountManager>();
            services.AddScoped<BlackSoilManager>();
            services.AddScoped<MediaManager>();
            services.AddScoped<ProductManager>();
            services.AddScoped<ArthMateManager>();
            services.AddScoped<DSAManager>();
            services.AddScoped<TestManager>();
            services.AddScoped<ConsumerAppHomeManager>();
            services.AddScoped<AyeFinanceSCFManger>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Aggregator", Version = "v1" });
            });

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

                    //cfg.ReceiveEndpoint(QueuesConsts.OrderFailedEventtQueueName, x =>
                    //{
                    //    x.ConfigureConsumer<OrderFailedEventConsumer>(context);
                    //});
                });

                x.AddRequestClient<ICheckStatus>();
            });

            services.AddScoped<IMassTransitService, MassTransitService>();



            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // In this case will wait for
            //  2 ^ 1 = 2 seconds then
            //  2 ^ 2 = 4 seconds then
            //  2 ^ 3 = 8 seconds then
            //  2 ^ 4 = 16 seconds then
            //  2 ^ 5 = 32 seconds

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, retryCount, context) =>
                    {
                        Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );
        }


        public static IServiceCollection AddAllServiceAsGrpc(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<ITokenProvider, AppTokenProvider>();

            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true
            };


            services.AddCodeFirstGrpcClient<IProductGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.ProductUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });


            services.AddCodeFirstGrpcClient<ICompanyGrpcService>((provider, o) =>
            {


                o.Address = new Uri(EnvironmentConstants.CompanyUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });




            services.AddCodeFirstGrpcClient<ICommunicationGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.CommunicationUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });

            services.AddCodeFirstGrpcClient<ILeadGrpcService>((provider, o) =>
            {


                o.Address = new Uri(EnvironmentConstants.LeadUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });


            services.AddCodeFirstGrpcClient<IMediaGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.MediaUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });



            services.AddCodeFirstGrpcClient<ILoanAccountGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.LoanAccountUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });



            services.AddCodeFirstGrpcClient<IIdentityGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.IdentityUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });



            services.AddCodeFirstGrpcClient<ILocationGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.LocationUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });



            services.AddCodeFirstGrpcClient<IKycGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.KYCUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });


            services.AddCodeFirstGrpcClient<ILedgerGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.LedgerUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });


            services.AddCodeFirstGrpcClient<INBFCGrpcService>((provider, o) =>
            {

                o.Address = new Uri(EnvironmentConstants.NBFCUrl);
                o.ChannelOptionsActions.Add(options =>
                {
                    options.HttpHandler = new SocketsHttpHandler()
                    {
                        // keeps connection alive
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                        // allows channel to add additional HTTP/2 connections
                        EnableMultipleHttp2Connections = true
                    };

                    var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
                    {
                        var token = string.Empty;
                        using (var scope = provider.CreateScope())
                        {
                            var op = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                            token = op.GetTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(token))
                        {
                            metadata.Add("Authorization", $"{token}");
                        }
                        return Task.CompletedTask;
                    });
                    var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
                    options.Credentials = channelCredentials;
                });


            });


            #region MyRegion old code
            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.CompanyUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<ICompanyGrpcService>();
            //});

            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.ProductUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<IProductGrpcService>();
            //});

            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.LeadUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<ILeadGrpcService>();
            //});

            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.KYCUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<IKycGrpcService>();
            //});

            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.IdentityUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<IIdentityGrpcService>();
            //});

            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.CommunicationUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<ICommunicationGrpcService>();
            //});

            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.MediaUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<IMediaGrpcService>();
            //});

            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.LocationUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<ILocationGrpcService>();
            //});
            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.NBFCUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<INBFCGrpcService>();
            //});
            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.LoanAccountUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<ILoanAccountGrpcService>();
            //});
            //services.AddTransient(sp =>
            //{
            //    var provider = sp.GetRequiredService<ITokenProvider>();
            //    var token = provider.GetTokenAsync();

            //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    {
            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            metadata.Add("Authorization", $"{token}");
            //        }
            //        return Task.CompletedTask;
            //    });
            //    var channel = GrpcChannel.ForAddress(EnvironmentConstants.LedgerUrl, new GrpcChannelOptions
            //    {
            //        Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            //        HttpHandler = handler,
            //        ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            //    });
            //    return channel.CreateGrpcService<ILedgerGrpcService>();
            //});
            #endregion

            return services;
        }

        public static T GetCustomeChennle<T>(string token, string url) where T : class
        {
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!token.Contains("Bearer"))
                    token = $"Bearer {token}";

                metadata.Add("Authorization", $"{token}");
                return Task.CompletedTask;
            });
            var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
            return channel.CreateGrpcService<T>();
        }
    }


    //public sealed class GrpcChannelSingleton
    //{

    //    private static readonly Lazy<GrpcChannel> Lazy = new Lazy<GrpcChannel>(() =>

    //        connectionFactory.CreateConnection()
    //    );

    //    public static IConnection Instance
    //    {
    //        get { return Lazy.Value; }
    //    }
    //}

    public static class SeedData
    {
        public static void InsertDefaultData(IApplicationBuilder builder)
        {
            ApplicationDbContext context = builder.ApplicationServices
                                         .CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                // context.Database.Migrate();
            }
        }
    }

}
