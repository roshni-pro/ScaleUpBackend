using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Consumers;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUp.BuildingBlocks.EventBus;
using ScaleUP.BuildingBlocks.EventBus.Constants;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Services.LeadAPI.Constants;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadAPI.NBFCFactory.Implementation;
using ScaleUP.Services.LeadModels;
using System;
using ScaleUP.Services.LeadModels.LeadNBFC;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Common.MassTransitMiddleware;
using System.ComponentModel;
using ScaleUP.Services.LeadModels.Cashfree;
using ScaleUP.Services.LeadAPI.Helper;
using ScaleUP.Services.LeadAPI.Helper.NBFC;

namespace ScaleUP.Services.LeadAPI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<AuditableEntitySaveChangesInterceptor>();
            services.AddDbContext<LeadApplicationDbContext>(options =>
                options.UseSqlServer(EnvironmentConstants.DbContext
                ,
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.LeadAPI")
                    ));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<LeadApplicationDbContext>());



            services.AddMassTransit(x =>
            {
                x.AddConsumer<KYCSuccessEventConsumer>();
                x.AddConsumer<KYCFailEventConsumer>();
                x.AddConsumer<LeadActivityCompletedEventConsumer>();
                x.AddConsumer<LeadUpdateHistoryEventConsumer>();
                x.AddRequestClient<ICreateLeadActivityMessage>(new Uri($"queue:{QueuesConsts.CreateLeadActivityMessageQueueName}"), timeout: 1000000);
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
                    cfg.ReceiveEndpoint(QueuesConsts.KYCSuccessEventQueueName, x =>
                    {
                        x.ConfigureConsumer<KYCSuccessEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(QueuesConsts.KYCFailedEventQueueName, x =>
                    {
                        x.ConfigureConsumer<KYCFailEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(QueuesConsts.CompleteLeadActivityMessageQueueName, x =>
                    {
                        x.ConfigureConsumer<LeadActivityCompletedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(QueuesConsts.LeadUpdateHistoryMessageQueueName, x =>
                    {
                        x.ConfigureConsumer<LeadUpdateHistoryEventConsumer>(context);
                    });
                });
            });
            services.AddScoped<Token>();

            services.AddScoped<LeadNBFCFactory>();

            services.AddScoped<BlackSoilLeadNBFCService>()
                 .AddScoped<ILeadNBFCService, BlackSoilLeadNBFCService>(s => s.GetService<BlackSoilLeadNBFCService>());

            services.AddScoped<ArthMateNBFCService>()
                 .AddScoped<ILeadNBFCService, ArthMateNBFCService>(s => s.GetService<ArthMateNBFCService>());

            services.AddScoped<DefaultLeadNBFCService>()
                 .AddScoped<ILeadNBFCService, DefaultLeadNBFCService>(s => s.GetService<DefaultLeadNBFCService>());

            services.AddScoped<AyeFinanceSCFLeadNBFCService>()
                 .AddScoped<ILeadNBFCService, AyeFinanceSCFLeadNBFCService>(s => s.GetService<AyeFinanceSCFLeadNBFCService>());



            services.AddScoped<IMassTransitService, MassTransitService>();
            services.AddScoped<LeadGrpcManager>();
            services.AddScoped<LeadManager>();
            services.AddScoped<ExperianManager>();
            services.AddScoped<eNachManager>();
            services.AddScoped<LeadBankDetailManager>();
            services.AddScoped<LeadNBFCSubActivityManager>();
            services.AddScoped<NBFCSchedular>();
            services.AddScoped<LeadCommonRequestResponseManager>();
            services.AddScoped<BlackSoilUpdateManager>();
            services.AddScoped<ArthMateGrpcManager>();
            services.AddScoped<ArthMateNBFCSchedular>();
            services.AddScoped<eSignKarzaHelper>();
            services.AddScoped<KarzaAadharHelper>();
            services.AddScoped<AyeFinanceSCFNBFCHelper>();
            services.AddScoped<LeadHistoryManager>();
            services.AddScoped<FinBoxHelper>();
            services.AddScoped<FinBoxManager>();

            return services;
        }
    }

    public static class SeedData
    {
        public static void InsertDefaultData(IApplicationBuilder builder)
        {
            LeadApplicationDbContext context = builder.ApplicationServices
                                         .CreateScope().ServiceProvider.GetRequiredService<LeadApplicationDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                // context.Database.Migrate();
            }
            if (!context.ThirdPartyApiConfigs.Any())
            {
                context.ThirdPartyApiConfigs.AddRange(
                new ThirdPartyApiConfig
                {
                    Code = "ExperianOTPRegistration",
                    ConfigJson = "{\"ApiUrl\":\"https://ecvuat.experian.in/ECV-P2/content/registerSingleActionMobileOTP.action\",\"ApiSecret\":\"SHOPKIRANA_FM\",\"ApiKey\":\"ExperianOTPRegister\",\"Other\":\"ShopKiranaz7C3j\",\"Header\":\"a\",\"ApiMasterId\":18}",
                    IsActive = true,
                    IsDeleted = false
                },
                new ThirdPartyApiConfig
                {
                    Code = "ExperianOTPGeneration",
                    ConfigJson = "{\"ApiUrl\":\"https://ecvuat.experian.in/ECV-P2/content/generateMobileOTP.action\",\"Other\":\"NORMAL\"}",
                    IsActive = true,
                    IsDeleted = false
                },
                new ThirdPartyApiConfig
                {
                    Code = "ExperianOTPValidation",
                    ConfigJson = "{\"ApiUrl\":\"https://ecvuat.experian.in/ECV-P2/content/validateMobileOTP.action\",\"Other\":\"NORMAL\"}",
                    IsActive = true,
                    IsDeleted = false
                },
                new ThirdPartyApiConfig
                {
                    Code = "MaskedMobileGeneration",
                    ConfigJson = "{\"ApiUrl\":\"https://ecvuat.experian.in/ECV-P2/content/generateMaskedMobile.action\",\"Other\":\"SHOPKIRANA_FM\"}",
                    IsActive = true,
                    IsDeleted = false
                },
                new ThirdPartyApiConfig
                {
                    Code = "MaskedMobileOTPGeneration",
                    ConfigJson = "{\"ApiUrl\":\"https://ecvuat.experian.in/ECV-P2/content/generateMobileOTP.action\",\"Other\":\"MASKED\"}",
                    IsActive = true,
                    IsDeleted = false
                },
                new ThirdPartyApiConfig
                {
                    Code = "MaskedMobileOTPValidation",
                    ConfigJson = "{\"ApiUrl\":\"https://ecvuat.experian.in/ECV-P2/content/validateMobileOTP.action\",\"Other\":\"MASKED\"}",
                    IsActive = true,
                    IsDeleted = false
                },
                new ThirdPartyApiConfig
                {
                    Code = "eNachBankList",
                    ConfigJson = "{\"ApiUrl\":\"https://enachuat.npci.org.in:8086/apiservices_new/getLiveBankDtls\",\"ApiMasterId\":\"14\"}",
                    IsActive = true,
                    IsDeleted = false
                },
                new ThirdPartyApiConfig
                {
                    Code = "eNachConfig",
                    ConfigJson = "{\"ShortCode\":\"SKETPL\",\"CustomerSequenceType\":\"RCUR\",\"MerchantCategoryCode\":\"U099\",\"UtilCode\":\"NACH00000000000020\",\"CustomerDebitFrequency\":\"ADHO\",\"CustomerMaxAmount\":\"100000.00\",\"CreditLimitMultiplier\":\"3\",\"eNachPostApiUrl\": \"https://emandateut.hdfcbank.com/Emandate.aspx\",\"eNachPostApiMasterId\": 15,\"eNachKey\": \"k2hLr4X0ozNyZByj5DT66edtCEee1x+6\"}",
                    IsActive = true,
                    IsDeleted = false
                },
                new ThirdPartyApiConfig
                {
                    Code = "Shopkirana",
                    ConfigJson = "{\"ApiUrl\":\"https://uat.shopkirana.in/api/HtmlToPdf/ConvertHtmlToPdf\",\"ApiMasterId\":\"16\"}",
                    IsActive = true,
                    IsDeleted = false
                }
                );
                context.SaveChanges();
            }



            if (!context.EntityMasters.Any())
            {
                context.EntityMasters.AddRange(new EntityMaster
                {
                    EntityName = "LeadNo",
                    DefaultNo = 1,
                    EntityQuery = "select @numCnt= count(Id) from Leads where LeadCode = @numberStr",
                    Separator = "/",
                    IsActive = true,
                    IsDeleted = false
                });
                context.SaveChanges();
            }

            if (!context.EntitySerialMasters.Any())
            {
                var allEntitySerialMasters = context.EntityMasters.ToList().Select(x => new EntitySerialMaster
                {
                    EntityId = x.Id,
                    Prefix = DateTime.Now.Year.ToString(),
                    StartFrom = 1,
                    NextNumber = 1,
                    StateId = 0,
                    IsActive = true,
                    IsDeleted = false,
                }).ToList();
                context.EntitySerialMasters.AddRange(allEntitySerialMasters);
                context.SaveChanges();
            }

            if (!context.ExperianStates.Any())
            {
                context.ExperianStates.AddRange(
                    new ExperianState { LocationStateId = 18, ExperianStateId = 27, IsActive = true, IsDeleted = false },
                    new ExperianState { LocationStateId = 27, ExperianStateId = 27, IsActive = true, IsDeleted = false },
                    new ExperianState { LocationStateId = 27, ExperianStateId = 23, IsActive = true, IsDeleted = false }
                    );
                context.SaveChanges();
            }

            if (!context.BankLists.Any())
            {
                context.BankLists.AddRange(
                    new BankList { aadhaarActiveFrom = "2023-09-15", aadhaarFlag = "Active", bankId = "AIRP", activeFrm = "2020-12-09", debitcardFlag = "Active", bankName = "AIRTEL PAYMENTS BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-08-22", aadhaarFlag = "Active", bankId = "AKOX", activeFrm = "2023-08-24", debitcardFlag = "Inactive", bankName = "THE AKOLA URBAN CO OP BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-11-22", aadhaarFlag = "Active", bankId = "AUBL", activeFrm = "2020-10-15", debitcardFlag = "Active", bankName = "AU SMALL FINANCE BANK", dcActiveFrom = "8/2/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "BARB", activeFrm = "2018-11-01", debitcardFlag = "Active", bankName = "BANK OF BARODA", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "BDBL", activeFrm = "2020-01-31", debitcardFlag = "Inactive", bankName = "BANDHAN BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-08-23", aadhaarFlag = "Active", bankId = "BKID", activeFrm = "2019-04-15", debitcardFlag = "Active", bankName = "BANK OF INDIA", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-08-22", aadhaarFlag = "Active", bankId = "CBIN", activeFrm = "2023-04-04", debitcardFlag = "Active", bankName = "CENTRAL BANK OF INDIA", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "CITI", activeFrm = "NULL", debitcardFlag = "Inactive", bankName = "CITIBANK N A", dcActiveFrom = "NULL", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "CIUB", activeFrm = "2019-05-30", debitcardFlag = "Active", bankName = "CITY UNION BANK LTD", dcActiveFrom = "7/18/2023", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-12-20", aadhaarFlag = "Active", bankId = "CNRB", activeFrm = "2019-08-03", debitcardFlag = "Active", bankName = "CANARA BANK", dcActiveFrom = "9/23/2020", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-11-08", aadhaarFlag = "Active", bankId = "CNSX", activeFrm = "NULL", debitcardFlag = "Inactive", bankName = "THE CHEMBUR NAGARIK SAHAKARI BANK", dcActiveFrom = "NULL", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "COSB", activeFrm = "2019-05-03", debitcardFlag = "Active", bankName = "THE COSMOS CO-OPERATIVE BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "DBSS", activeFrm = "2020-01-30", debitcardFlag = "Active", bankName = "DBS BANK INDIA LTD", dcActiveFrom = "2/17/2020", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-07-18", aadhaarFlag = "Active", bankId = "DCBL", activeFrm = "2019-08-05", debitcardFlag = "Active", bankName = "DCB BANK LTD", dcActiveFrom = "9/5/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "EDBX", activeFrm = "NULL", debitcardFlag = "Active", bankName = "ELLAQUAI DEHATI BANK", dcActiveFrom = "8/4/2023", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-08-16", aadhaarFlag = "Active", bankId = "ESAF", activeFrm = "2021-05-25", debitcardFlag = "Active", bankName = "ESAF SMALL FINANCE BANK LTD", dcActiveFrom = "2/16/2023", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-06-12", aadhaarFlag = "Active", bankId = "ESFB", activeFrm = "2019-03-01", debitcardFlag = "Active", bankName = "EQUITAS SMALL FINANCE BANK LTD", dcActiveFrom = "3/1/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-12-05", aadhaarFlag = "Active", bankId = "FDRL", activeFrm = "2019-05-29", debitcardFlag = "Active", bankName = "FEDERAL BANK", dcActiveFrom = "5/29/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-03-24", aadhaarFlag = "Active", bankId = "FINO", activeFrm = "NULL", debitcardFlag = "Active", bankName = "FINO PAYMENTS BANK LTD", dcActiveFrom = "7/14/2022", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-12-22", aadhaarFlag = "Active", bankId = "HDFC", activeFrm = "2019-04-16", debitcardFlag = "Active", bankName = "HDFC BANK LTD", dcActiveFrom = "4/16/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "HSBC", activeFrm = "2020-02-18", debitcardFlag = "Inactive", bankName = "THE HONGKONG AND SHANGHAI BANKING CORPORATION LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-11-29", aadhaarFlag = "Active", bankId = "HUTX", activeFrm = "NULL", debitcardFlag = "Inactive", bankName = "HUTATMA SAHAKARI BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-01-01", aadhaarFlag = "Active", bankId = "ICIC", activeFrm = "2019-03-12", debitcardFlag = "Active", bankName = "ICICI BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-02-20", aadhaarFlag = "Active", bankId = "IDFB", activeFrm = "2017-05-07", debitcardFlag = "Active", bankName = "IDFC FIRST BANK LTD", dcActiveFrom = "6/20/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-09-15", aadhaarFlag = "Active", bankId = "IDIB", activeFrm = "2019-09-26", debitcardFlag = "Active", bankName = "INDIAN BANK", dcActiveFrom = "11/12/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-01-01", aadhaarFlag = "Active", bankId = "INDB", activeFrm = "2019-02-06", debitcardFlag = "Active", bankName = "INDUSIND BANK", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-11-08", aadhaarFlag = "Active", bankId = "IOBA", activeFrm = "2019-02-20", debitcardFlag = "Active", bankName = "INDIAN OVERSEAS BANK", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "JAKA", activeFrm = "NULL", debitcardFlag = "Active", bankName = "THE JAMMU AND KASHMIR BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-04-20", aadhaarFlag = "Active", bankId = "KJSB", activeFrm = "NULL", debitcardFlag = "Active", bankName = "THE KALYAN JANATA SAHAKARI BANK LTD", dcActiveFrom = "7/14/2022", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-05-04", aadhaarFlag = "Active", bankId = "KKBK", activeFrm = "2019-02-26", debitcardFlag = "Active", bankName = "KOTAK MAHINDRA BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "KNSB", activeFrm = "NULL", debitcardFlag = "Active", bankName = "KURLA NAGARIK SAHAKARI BANK LTD", dcActiveFrom = "9/11/2023", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-08-17", aadhaarFlag = "Active", bankId = "MAHB", activeFrm = "2019-04-16", debitcardFlag = "Active", bankName = "BANK OF MAHARASHTRA", dcActiveFrom = "10/20/2020", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "MDGX", activeFrm = "NULL", debitcardFlag = "Active", bankName = "RAJASTHAN MARUDHARA GRAMIN BANK", dcActiveFrom = "8/23/2023", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2021-12-04", aadhaarFlag = "Active", bankId = "NCBL", activeFrm = "2022-02-06", debitcardFlag = "Active", bankName = "THE NATIONAL CO OP BANK LTD", dcActiveFrom = "1/5/2022", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-02-07", aadhaarFlag = "Active", bankId = "NSDL", activeFrm = "2020-09-16", debitcardFlag = "Active", bankName = "NATIONAL SECURITIES DEPOSITORY LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2021-12-04", aadhaarFlag = "Active", bankId = "ONMG", activeFrm = "NULL", debitcardFlag = "Active", bankName = "ONMAGS Test Bank", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "PDSX", activeFrm = "NULL", debitcardFlag = "Active", bankName = "PRIYADARSHANI NAGARI SAHAKARI BANK LTD JALNA", dcActiveFrom = "11/3/2023", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-03-01", aadhaarFlag = "Active", bankId = "PSIB", activeFrm = "2019-11-01", debitcardFlag = "Active", bankName = "PUNJAB AND SIND BANK", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-05-02", aadhaarFlag = "Active", bankId = "PUNB", activeFrm = "2017-05-07", debitcardFlag = "Active", bankName = "PUNJAB NATIONAL BANK", dcActiveFrom = "7/4/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "PYTM", activeFrm = "2019-04-18", debitcardFlag = "Active", bankName = "PAYTM PAYMENTS BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "RATN", activeFrm = "2019-02-13", debitcardFlag = "Active", bankName = "RBL BANK LIMITED", dcActiveFrom = "10/11/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-06-15", aadhaarFlag = "Active", bankId = "SBIN", activeFrm = "2018-03-21", debitcardFlag = "Active", bankName = "STATE BANK OF INDIA", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "SCBL", activeFrm = "2019-07-15", debitcardFlag = "Active", bankName = "STANDARD CHARTERED BANK", dcActiveFrom = "12/11/2019", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-05-16", aadhaarFlag = "Active", bankId = "SIBL", activeFrm = "2019-03-18", debitcardFlag = "Active", bankName = "SOUTH INDIAN BANK", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-05-03", aadhaarFlag = "Active", bankId = "SRCB", activeFrm = "NULL", debitcardFlag = "Active", bankName = "SARASWAT BANK", dcActiveFrom = "5/3/2023", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "SURY", activeFrm = "NULL", debitcardFlag = "Inactive", bankName = "SURYODAY SMALL FINANCE BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "NULL", aadhaarFlag = "Inactive", bankId = "SVCB", activeFrm = "NULL", debitcardFlag = "Active", bankName = "SVC CO OP BANK LTD", dcActiveFrom = "12/19/2023", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-11-28", aadhaarFlag = "Active", bankId = "UBIN", activeFrm = "2019-04-25", debitcardFlag = "Active", bankName = "UNION BANK OF INDIA", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-11-17", aadhaarFlag = "Active", bankId = "USFB", activeFrm = "2019-03-07", debitcardFlag = "Active", bankName = "UJJIVAN SMALL FINANCE BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2023-05-05", aadhaarFlag = "Active", bankId = "UTIB", activeFrm = "2020-06-26", debitcardFlag = "Active", bankName = "AXIS BANK", dcActiveFrom = "6/26/2020", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2020-02-25", aadhaarFlag = "Active", bankId = "UTKS", activeFrm = "NULL", debitcardFlag = "Inactive", bankName = "UTKARSH SMALL FINANCE BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Inactive", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-05-02", aadhaarFlag = "Active", bankId = "VARA", activeFrm = "2019-10-20", debitcardFlag = "Inactive", bankName = "THE VARACHHA CO OP BANK LTD", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false },
                    new BankList { aadhaarActiveFrom = "2022-12-28", aadhaarFlag = "Active", bankId = "YESB", activeFrm = "2017-05-07", debitcardFlag = "Active", bankName = "YES BANK", dcActiveFrom = "NULL", netbankFlag = "Active", IsActive = true, IsDeleted = false }
                    );
                context.SaveChanges();
            }

            if (!context.LeadTemplateMasters.Any())
            {
                context.LeadTemplateMasters.AddRange(
                    new LeadTemplateMaster { TemplateCode = "LoginOTPSMS", TemplateType = "SMS", DLTID = "1707170962195644808", Template = "Dear Customer, {#var1#} is the OTP to Login your Scaleup Pay application. Valid for 15 Mins. For any assistance please call +91-9981531563", IsActive = true, IsDeleted = false },
                    new LeadTemplateMaster { TemplateCode = "DSALoginOTPSMS", TemplateType = "SMS", DLTID = "1707171826878083003", Template = "Dear Partner,  {#var1#} is the OTP to Login your Scaleupfin DSA application. Valid for 15 Mins. For any assistance please call +91-9981531563", IsActive = true, IsDeleted = false }
                    );
                context.SaveChanges();
            }

            if (!context.LeadNBFCApis.Any())
            {
                context.LeadNBFCApis.AddRange(
                    new LeadNBFCApi
                    {
                        APIUrl = "null",
                        Code = "BlackSoilCommonApiForSecret",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/los/applications/{{APPLICATION_ID}}?expand=loan_data",
                        Code = "BlackSoilCommonApplicationDetail",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/service/ifsc/?code={{IFSC_CODE}}",
                        Code = "BlackSoilGetBankDetailByIFSCCode",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://uat-apiorigin.arthmate.com/api/kz-aadhaar-xml-otp",
                        Code = "ArthmateAadhaarOtpSend",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://uat-apiorigin.arthmate.com/api/kz-aadhaar-xml-file-v2",
                        Code = "ArthmateAadhaarOtpVerify",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://uat-apiorigin.arthmate.com/api/loan",
                        Code = "ArthmateLoanGenerate",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://uat-apiorigin.arthmate.com/api/repayment_schedule",
                        Code = "ArthmateRepaymentSchedule",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://uat-apiorigin.arthmate.com/api/disbursement_status",
                        Code = "ArthmateGetDisbursement",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://uat-apiorigin.arthmate.com/api/loan_nach",
                        Code = "ArthmateLoanNachPatch",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://api-demo.ceplr.com/customers/{{customer_uuid}}/report",
                        Code = "ArthmateGetBasicReports",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LeadNBFCApi
                    {
                        APIUrl = "https://uat-apiorigin.arthmate.com/api/borrowerinfostatusupdate/{loan_id}",
                        Code = "ArthmateUpdateLoanStatus",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        Sequence = 0,
                        LeadNBFCSubActivityId = null,
                        Status = "NotStarted",
                        RequestId = null,
                        ResponseId = null,
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    }
                    );
                context.SaveChanges();
            }
            if (!context.NBFCApiTokens.Any())
            {
                context.NBFCApiTokens.AddRange(
                    new NBFCApiToken
                    {
                        IdentificationCode = CompanyIdentificationCodeConstants.ArthMate,
                        TokenType = NBFCApiTokenTypeConstants.configuration_uuid,
                        TokenValue = "Configuration-5245-8577",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCApiToken
                    {
                        IdentificationCode = CompanyIdentificationCodeConstants.ArthMate,
                        TokenType = NBFCApiTokenTypeConstants.CeplarKey,
                        TokenValue = "F73nkLIiSD4s16lyVvzLv1ibEAKEwdFI85il7Aph",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCApiToken
                    {
                        IdentificationCode = CompanyIdentificationCodeConstants.ArthMate,
                        TokenType = NBFCApiTokenTypeConstants.CallbackURL,
                        TokenValue = "https://uat.shopkirana.com",
                        IsActive = true,
                        IsDeleted = false
                    }
                    );
                context.SaveChanges();
            }
            if (!context.LoanInsuranceConfiguration.Any())
            {
                context.LoanInsuranceConfiguration.AddRange(
                    new LoanInsuranceConfiguration
                    {
                        MonthDuration = 3,
                        RateOfInterestInPer = 0.0625,
                        Remarks = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LoanInsuranceConfiguration
                    {
                        MonthDuration = 6,
                        RateOfInterestInPer = 0.125,
                        Remarks = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LoanInsuranceConfiguration
                    {
                        MonthDuration = 9,
                        RateOfInterestInPer = 0.187999993562698,
                        Remarks = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LoanInsuranceConfiguration
                    {
                        MonthDuration = 12,
                        RateOfInterestInPer = 0.25,
                        Remarks = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LoanInsuranceConfiguration
                    {
                        MonthDuration = 24,
                        RateOfInterestInPer = 0.5,
                        Remarks = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LoanInsuranceConfiguration
                    {
                        MonthDuration = 36,
                        RateOfInterestInPer = 0.75,
                        Remarks = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LoanInsuranceConfiguration
                    {
                        MonthDuration = 48,
                        RateOfInterestInPer = 1,
                        Remarks = "",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new LoanInsuranceConfiguration
                    {
                        MonthDuration = 60,
                        RateOfInterestInPer = 1.25,
                        Remarks = "",
                        IsActive = true,
                        IsDeleted = false
                    }
                    );
                context.SaveChanges();
            }


            //if (!context.LoanConfiguration.Any())
            //{
            //    context.LoanConfiguration.AddRange(
            //        new LoanConfiguration
            //        {
            //            PF = 2,
            //            GST = 18,
            //            ODCharges = 4,
            //            ODdays = 1,
            //            InterestRate = 24,
            //            MaxInterestRate = 28,
            //            BounceCharge = 12,
            //            PenalPercent = 4,
            //            IsActive = true,
            //            IsDeleted = false
            //        }
            //        );
            //    context.SaveChanges();
            //}

            if (!context.EmailRecipients.Any())
            {
                context.EmailRecipients.AddRange(
                    new EmailRecipient
                    {
                        EmailType = EmailTypeConstants.StampRemainder,
                        To = "mayur.jain@shopkirana.com",
                        LimitValue = 5,
                        IsActive = true,
                        IsDeleted = false
                    }
                    );
                context.SaveChanges();
            }

            if (!context.cashFreeEnachconfigurations.Any())
            {
                context.cashFreeEnachconfigurations.AddRange(
                    new CashFreeEnachconfiguration
                    {
                        intervals = 1,
                        expiresOnYear = 3,
                        linkExpiryDay = 5,
                        intervalType = "MONTH",
                        maxAmountMultiplier = 3,
                        type = "PERIODIC",
                        returnUrl = "",
                        IsActive = true,
                        IsDeleted = false
                    }
                    );
                context.SaveChanges();
            }
            if (!context.ArthMateDocumentMaster.Any())
            {
                context.ArthMateDocumentMaster.AddRange(
                    new ArthMateDocumentMaster { DocumentName = "agreement", DocumentTypeCode = "001", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "loan_sanction_letter", DocumentTypeCode = "002", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "selfie", DocumentTypeCode = "003", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "address_proof", DocumentTypeCode = "004", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "pan_card", DocumentTypeCode = "005", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "aadhar_card", DocumentTypeCode = "006", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "cheques", DocumentTypeCode = "007", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "nach_form", DocumentTypeCode = "008", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "other", DocumentTypeCode = "009", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "msp_receipt", DocumentTypeCode = "010", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_perm_addr_prf", DocumentTypeCode = "011", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_ebill", DocumentTypeCode = "012", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_photo_id_prf", DocumentTypeCode = "013", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_rent_agrmnt", DocumentTypeCode = "014", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_gas_bill", DocumentTypeCode = "015", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_other", DocumentTypeCode = "016", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_addr_prf", DocumentTypeCode = "017", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_perm_addr_prf", DocumentTypeCode = "018", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_pan_card", DocumentTypeCode = "019", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_aadhar_card", DocumentTypeCode = "020", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_ebill", DocumentTypeCode = "021", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_photo_id_prf", DocumentTypeCode = "022", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_rent_agrmnt", DocumentTypeCode = "023", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_gas_bill", DocumentTypeCode = "024", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_other", DocumentTypeCode = "025", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_addr_prf", DocumentTypeCode = "026", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_perm_addr_prf", DocumentTypeCode = "027", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_pan_card", DocumentTypeCode = "028", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_aadhar_card", DocumentTypeCode = "029", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_ebill", DocumentTypeCode = "030", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_photo_id_prf", DocumentTypeCode = "031", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_rent_agrmnt", DocumentTypeCode = "032", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_gas_bill", DocumentTypeCode = "033", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "guar_other", DocumentTypeCode = "034", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_addr_prf", DocumentTypeCode = "035", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_perm_addr_prf", DocumentTypeCode = "036", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_pan_card", DocumentTypeCode = "037", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_ebill", DocumentTypeCode = "038", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_rent_agrmnt", DocumentTypeCode = "039", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_shop_act", DocumentTypeCode = "040", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_gst_certifi", DocumentTypeCode = "041", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_itr_1", DocumentTypeCode = "042", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_itr_2", DocumentTypeCode = "043", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_audit_reprt", DocumentTypeCode = "044", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_bank_stmnts", DocumentTypeCode = "045", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "buss_other", DocumentTypeCode = "046", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "cibil_brwr", DocumentTypeCode = "047", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "cibil_co_brwr", DocumentTypeCode = "048", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "repayment_track", DocumentTypeCode = "049", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "fl_reprt", DocumentTypeCode = "050", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "insurance_frm", DocumentTypeCode = "051", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "insurance_calculator_page", DocumentTypeCode = "052", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "dpn", DocumentTypeCode = "053", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "id_proof", DocumentTypeCode = "054", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_perm_addr_prf", DocumentTypeCode = "055", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_ebill", DocumentTypeCode = "056", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_photo_id_prf", DocumentTypeCode = "057", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_rent_agree", DocumentTypeCode = "058", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_gas_bill", DocumentTypeCode = "059", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "app_other", DocumentTypeCode = "060", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "borro_itr_1", DocumentTypeCode = "061", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "borro_itr_2", DocumentTypeCode = "062", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "borro_bank_stmt", DocumentTypeCode = "063", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "borro_other", DocumentTypeCode = "064", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "borro_sal_slip", DocumentTypeCode = "065", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "employee_id_card", DocumentTypeCode = "066", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "cibil_borro", DocumentTypeCode = "067", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "repay_track", DocumentTypeCode = "068", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "fi_report", DocumentTypeCode = "069", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "ins_form", DocumentTypeCode = "070", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "ins_cal_page", DocumentTypeCode = "071", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_addr_prf", DocumentTypeCode = "072", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_perm_addr_prf", DocumentTypeCode = "073", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_pan_card", DocumentTypeCode = "074", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_aadhar_card", DocumentTypeCode = "075", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_ebill", DocumentTypeCode = "076", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_photo_id_prf", DocumentTypeCode = "077", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_rent_agree", DocumentTypeCode = "078", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_gas_bill", DocumentTypeCode = "079", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_other", DocumentTypeCode = "080", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "cibil_co_borro", DocumentTypeCode = "081", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "co_app_bank stmt", DocumentTypeCode = "082", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "other_1", DocumentTypeCode = "083", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "other_2", DocumentTypeCode = "084", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "other_3", DocumentTypeCode = "085", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_addr_prf", DocumentTypeCode = "086", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_perm_addr_prf", DocumentTypeCode = "087", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_pan_card", DocumentTypeCode = "088", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_ebill", DocumentTypeCode = "089", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_rent_agree", DocumentTypeCode = "090", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_shop_act", DocumentTypeCode = "091", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_itr_1", DocumentTypeCode = "093", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_itr_2", DocumentTypeCode = "094", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_audit_report", DocumentTypeCode = "095", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_bank_stmt", DocumentTypeCode = "096", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "business_other", DocumentTypeCode = "097", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "cibil", DocumentTypeCode = "098", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bank_stmnts", DocumentTypeCode = "099", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "gst_returns", DocumentTypeCode = "100", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "shop_est_cert", DocumentTypeCode = "101", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "drawdown_agreement", DocumentTypeCode = "102", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "revised_agreement", DocumentTypeCode = "103", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "proforma_invoice", DocumentTypeCode = "104", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "drawdown_receipt", DocumentTypeCode = "105", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "udyam_reg_cert", DocumentTypeCode = "106", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "cam", DocumentTypeCode = "107", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "part_deed", DocumentTypeCode = "108", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_reg_prf", DocumentTypeCode = "109", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "sales_ledger", DocumentTypeCode = "110", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_photos", DocumentTypeCode = "111", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "rc", DocumentTypeCode = "112", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bureau consent", DocumentTypeCode = "113", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "aadhaar_xml", DocumentTypeCode = "114", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "aadhaar_card_back", DocumentTypeCode = "115", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "pan_xml", DocumentTypeCode = "116", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "aadhaar_ovd", DocumentTypeCode = "117", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "title_search_report", DocumentTypeCode = "118", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "valuation_report", DocumentTypeCode = "119", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "sale_deed", DocumentTypeCode = "120", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "mortgage_deed", DocumentTypeCode = "121", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "index_2_with_lein_mark", DocumentTypeCode = "122", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "list_of_documents", DocumentTypeCode = "123", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "screening_report", DocumentTypeCode = "124", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "pd_report", DocumentTypeCode = "125", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "vehicle_invoice", DocumentTypeCode = "126", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "vehical_margin_money_receipt", DocumentTypeCode = "127", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "rto_form", DocumentTypeCode = "128", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "driving_license", DocumentTypeCode = "129", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bank_statement_analysis", DocumentTypeCode = "130", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "signed_agreement", DocumentTypeCode = "131", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "signed_loan_sanction_letter", DocumentTypeCode = "132", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "technical_report", DocumentTypeCode = "133", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "legal_vetting_report", DocumentTypeCode = "134", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "noc", DocumentTypeCode = "999", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_utility_bill", DocumentTypeCode = "135", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_municipal_tax_bill", DocumentTypeCode = "136", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_ownership_proof", DocumentTypeCode = "137", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_fdcc_cert", DocumentTypeCode = "138", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_financial_stmt", DocumentTypeCode = "139", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_board_reso", DocumentTypeCode = "140", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_list_director", DocumentTypeCode = "141", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_list_shareholder", DocumentTypeCode = "142", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "moa", DocumentTypeCode = "143", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "aoa", DocumentTypeCode = "144", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "coi", DocumentTypeCode = "145", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_list_member", DocumentTypeCode = "146", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "bus_authority_letter", DocumentTypeCode = "147", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "settlement_letter", DocumentTypeCode = "997", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "gst", DocumentTypeCode = "148", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "MSME_Certificate", DocumentTypeCode = "149", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "PersonalDetail", DocumentTypeCode = "0", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "BusinessDetail", DocumentTypeCode = "0", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false },
                    new ArthMateDocumentMaster { DocumentName = "BankDetail", DocumentTypeCode = "0", IsFrontrequired = true, IsMandatory = false, IsBackrequired = false, IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }
            if (!context.ArthMateStateCode.Any())
            {
                context.ArthMateStateCode.AddRange(
                    new ArthMateStateCode { StateCode = 1, State = "Jammu & Kashmir", PINprefixMin = 18, PINprefixMax = 19, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 2, State = "Himachal Pradesh", PINprefixMin = 17, PINprefixMax = 17, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 3, State = "Punjab", PINprefixMin = 14, PINprefixMax = 16, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 4, State = "Chandigarh", PINprefixMin = 14, PINprefixMax = 16, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 5, State = "Uttaranchal", PINprefixMin = 24, PINprefixMax = 26, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 6, State = "Haryana", PINprefixMin = 12, PINprefixMax = 13, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 7, State = "Delhi", PINprefixMin = 11, PINprefixMax = 11, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 8, State = "Rajasthan", PINprefixMin = 30, PINprefixMax = 34, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 9, State = "Uttar Pradesh", PINprefixMin = 20, PINprefixMax = 28, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 10, State = "Bihar", PINprefixMin = 80, PINprefixMax = 85, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 11, State = "Sikkim", PINprefixMin = 73, PINprefixMax = 73, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 12, State = "Arunachal Pradesh", PINprefixMin = 78, PINprefixMax = 79, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 13, State = "Nagaland", PINprefixMin = 79, PINprefixMax = 79, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 14, State = "Manipur", PINprefixMin = 79, PINprefixMax = 79, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 15, State = "Mizoram", PINprefixMin = 79, PINprefixMax = 79, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 16, State = "Tripura", PINprefixMin = 72, PINprefixMax = 79, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 17, State = "Meghalaya", PINprefixMin = 79, PINprefixMax = 79, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 18, State = "Assam", PINprefixMin = 78, PINprefixMax = 79, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 19, State = "West Bengal", PINprefixMin = 70, PINprefixMax = 74, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 20, State = "Jharkhand", PINprefixMin = 81, PINprefixMax = 83, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 21, State = "Orissa", PINprefixMin = 75, PINprefixMax = 77, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 22, State = "Chhattisgarh", PINprefixMin = 46, PINprefixMax = 49, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 23, State = "Madhya Pradesh", PINprefixMin = 45, PINprefixMax = 48, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 24, State = "Gujarat", PINprefixMin = 36, PINprefixMax = 39, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 25, State = "Daman & Diu", PINprefixMin = 36, PINprefixMax = 39, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 26, State = "Dadra & Nagar Haveli & Daman & Diu", PINprefixMin = 36, PINprefixMax = 39, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 27, State = "Maharashtra", PINprefixMin = 40, PINprefixMax = 44, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 28, State = "Andhra Pradesh", PINprefixMin = 50, PINprefixMax = 56, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 29, State = "Karnataka", PINprefixMin = 53, PINprefixMax = 59, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 30, State = "Goa", PINprefixMin = 40, PINprefixMax = 40, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 31, State = "Lakshadweep", PINprefixMin = 67, PINprefixMax = 68, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 32, State = "Kerala", PINprefixMin = 67, PINprefixMax = 69, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 33, State = "Tamil Nadu", PINprefixMin = 53, PINprefixMax = 66, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 34, State = "Pondicherry", PINprefixMin = 53, PINprefixMax = 67, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 35, State = "Andaman & Nicobar Islands", PINprefixMin = 74, PINprefixMax = 74, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 36, State = "Telangana", PINprefixMin = 50, PINprefixMax = 56, IsActive = true, IsDeleted = false },
                    new ArthMateStateCode { StateCode = 99, State = "APO Address", PINprefixMin = 90, PINprefixMax = 99, IsActive = true, IsDeleted = false }

                    );
                context.SaveChanges();
            }
        }
    }
}
