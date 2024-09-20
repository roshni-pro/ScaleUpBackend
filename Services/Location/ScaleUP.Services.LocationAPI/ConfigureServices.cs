using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProtoBuf.Grpc.Server;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LocationAPI.Persistence;
using MassTransit;
using ScaleUP.Services.LocationAPI.Consumers;
using ScaleUP.BuildingBlocks.EventBus.Constants;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.Services.LocationAPI.Constants;
using ScaleUP.Services.LocationAPI.Managers;
using ScaleUP.Services.LocationModels.Master;

namespace ScaleUP.Services.LocationAPI
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
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.LocationAPI")
                    ));
            ////}

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            services.AddMassTransit(x =>
            {
                x.AddConsumer<UpdatingAddressEventConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
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
                    cfg.ReceiveEndpoint(QueuesConsts.UpdatingAddressEventQueueName, x =>
                    {
                        x.ConfigureConsumer<UpdatingAddressEventConsumer>(context);
                    });

                    //cfg.ReceiveEndpoint(QueuesConsts.OrderFailedEventtQueueName, x =>
                    //{
                    //    x.ConfigureConsumer<OrderFailedEventConsumer>(context);
                    //});
                });

                //x.UsingRabbitMq((context, cfg) =>
                //{
                //    cfg.Host(configuration.GetSection("AppSettings")["RabbitMQUrl"], Convert.ToUInt16(configuration.GetSection("AppSettings")["RabbitPort"]), configuration.GetSection("AppSettings")["RabbitVHost"], host =>
                //    {
                //        host.Username(configuration.GetSection("AppSettings")["RabbitUser"]);
                //        host.Password(configuration.GetSection("AppSettings")["RabbitPwd"]);
                //    });

                //    cfg.ReceiveEndpoint(QueuesConsts.UpdatingAddressEventQueueName, x =>
                //    {
                //        x.ConfigureConsumer<UpdatingAddressEventConsumer>(context);
                //    });

                //});
            });

            services.AddScoped<IMassTransitService, MassTransitService>();

            services.AddScoped<LocationManager>();
            services.AddScoped<StateManager>();
            services.AddScoped<CityManager>();
            services.AddScoped<MasterEntryManager>();

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

    public static class SeedData
    {
        public static void InsertDefaultData(IApplicationBuilder builder)
        {
            ApplicationDbContext context = builder.ApplicationServices
                                         .CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            if (!context.Countries.Any())
            {
                context.Countries.AddRange(
                 new Country { Name = "India", CountryCode = "IND", CurrencyCode = "INR", IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }
            if (!context.States.Any())
            {
                long countryId = context.Countries.FirstOrDefault(x => x.CountryCode == "IND").Id;
                context.States.AddRange(
                 new LocationModels.Master.State { Name = "Andhra Pradesh", StateCode = "01", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Assam", StateCode = "02", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Arunachal Pradesh", StateCode = "03", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Bihar", StateCode = "04", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Chhattisgarh", StateCode = "05", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Goa", StateCode = "06", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Gujarat", StateCode = "07", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Haryana", StateCode = "08", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Himachal Pradesh", StateCode = "09", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Jharkhand", StateCode = "10", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Karnataka", StateCode = "11", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Kerala", StateCode = "12", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Madhya Pradesh", StateCode = "13", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Maharashtra", StateCode = "14", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Manipur", StateCode = "15", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Meghalaya", StateCode = "16", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Mizoram", StateCode = "17", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Nagaland", StateCode = "18", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Odisha", StateCode = "19", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Punjab", StateCode = "20", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Rajasthan", StateCode = "21", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Sikkim", StateCode = "22", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Tamil Nadu", StateCode = "23", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Telangana", StateCode = "24", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Tripura", StateCode = "25", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Uttar Pradesh", StateCode = "26", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Uttarakhand", StateCode = "27", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "West Bengal", StateCode = "28", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Chandigarh", StateCode = "29", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Dadra & Nagar Haveli and Daman & Diu", StateCode = "DNHDD", CountryId = countryId, IsActive = true, IsDeleted = false },
                 new LocationModels.Master.State { Name = "Ladakh", StateCode = "LA", CountryId = countryId, IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }

            if (!context.Cities.Any())
            {
                long stateId = context.States.FirstOrDefault(x => x.Name == "Madhya Pradesh").Id;
                context.Cities.AddRange(
                 new City { Name = "Indore", StateId = stateId, IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }
            if (!context.AddressTypes.Any())
            {
                context.AddressTypes.AddRange(
                 new AddressType { Name= "Business", IsActive = true, IsDeleted = false },
                 new AddressType { Name= "Billing", IsActive = false, IsDeleted = true },
                 new AddressType { Name= "Shipping", IsActive = false, IsDeleted = true },
                 new AddressType { Name= "Current", IsActive = false, IsDeleted = true },
                 new AddressType { Name= "Permanent", IsActive = false, IsDeleted = true }
                );
                context.SaveChanges();
            }
        }
    }
}
