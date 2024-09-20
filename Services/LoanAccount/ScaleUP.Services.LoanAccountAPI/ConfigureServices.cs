using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScaleUP.Global.Infrastructure.Common.Interfaces;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory;
using ScaleUP.Services.LoanAccountAPI.AccountTransactionFactory.Implementations;
using ScaleUP.Services.LoanAccountAPI.Managers;
using ScaleUP.Services.LoanAccountAPI.Constants;
using MassTransit;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.BuildingBlocks.EventBus.Constants;
using ScaleUP.Services.LoanAccountAPI.Consumer;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory.Implementation;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Services.LoanAccountAPI.Helpers;
using ScaleUP.Services.LoanAccountModels.Master;
using System.ServiceModel.Channels;
using ScaleUP.Services.LoanAccountModels.Master.NBFC;
using IdentityServer4.EntityFramework.Stores;
using ScaleUP.Services.LoanAccountAPI.Helpers.NBFC;

namespace ScaleUP.Services.LoanAccountAPI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<AuditableEntitySaveChangesInterceptor>();

            services.AddDbContext<LoanAccountApplicationDbContext>(options =>
                options.UseSqlServer(EnvironmentConstants.DbContext
                ,
                    builder => builder.MigrationsAssembly(/*typeof(ApplicationDbContext).Assembly.FullName*/ "ScaleUP.Services.LoanAccountAPI")
                    ));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<LoanAccountApplicationDbContext>());


            services.AddScoped<TransactionFactory>();

            services.AddScoped<DisbursementAccountTransactionType>()
                 .AddScoped<IAccountTransactionType, DisbursementAccountTransactionType>(s => s.GetService<DisbursementAccountTransactionType>());

            services.AddScoped<OrderAccountTransactionType>()
                 .AddScoped<IAccountTransactionType, OrderAccountTransactionType>(s => s.GetService<OrderAccountTransactionType>());

            services.AddMassTransit(x =>
            {
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
                    cfg.ReceiveEndpoint(QueuesConsts.AccountDisbursementEventQueueName, x =>
                    {
                        x.ConfigureConsumer<AccountDisbursementConsumer>(context);
                    });
                });
                x.AddConsumer<AccountDisbursementConsumer>(cfg =>
                {
                    cfg.ConcurrentMessageLimit = 100;
                });

            });
            services.AddScoped<LoanNBFCFactory>();

            services.AddScoped<BlackSoilLoanNBFCService>()
                 .AddScoped<ILoanNBFCService, BlackSoilLoanNBFCService>(s => s.GetService<BlackSoilLoanNBFCService>());

            services.AddScoped<ArthmateLoanNBFCService>()
                 .AddScoped<ILoanNBFCService, ArthmateLoanNBFCService>(s => s.GetService<ArthmateLoanNBFCService>());

            services.AddScoped<DefaultLoanNBFCService>()
                 .AddScoped<ILoanNBFCService, DefaultLoanNBFCService>(s => s.GetService<DefaultLoanNBFCService>());

            services.AddScoped<AyeFinanceSCFLoanNBFCService>()
                 .AddScoped<ILoanNBFCService, AyeFinanceSCFLoanNBFCService>(s => s.GetService<AyeFinanceSCFLoanNBFCService>());





            services.AddScoped<IMassTransitService, MassTransitService>();


            services.AddScoped<TransactionDetailHeadManager>();
            services.AddScoped<TransactionTypeManager>();
            services.AddScoped<LoanAccountManager>();
            services.AddScoped<AccountTransactionManager>();
            services.AddScoped<PostDisbursementManager>();
            services.AddScoped<OrderPlacementManager>();
            services.AddScoped<TransactionSettlementManager>();
            services.AddScoped<LoanAccountHelper>();
            services.AddScoped<ArthmateNBFCHelper>();
            services.AddScoped<AyeFinanceSCFNBFCHelper>();
            services.AddScoped<DelayPenalityOnDuePerDayJobManager>();
            services.AddScoped<LoanAccountHistoryManager>();

            return services;
        }

    }

    public static class SeedData
    {
        public static void InsertDefaultData(IApplicationBuilder builder)
        {
            LoanAccountApplicationDbContext context = builder.ApplicationServices
                                         .CreateScope().ServiceProvider.GetRequiredService<LoanAccountApplicationDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            if (!context.CompanyAPIs.Any())
            {
                context.CompanyAPIs.AddRange(
                new CompanyAPI
                {
                    CompanyId = 2,
                    Code = "Disbursement",
                    IsWebhook = true,
                    ApiType = "POST",
                    APIUrl = "https://uat.shopkirana.in//api/ScaleUpIntegration/UpdateCustomerAccount",
                    Authtype = "",
                    APIKey = "",
                    APISecret = "",
                    IsActive = true,
                    IsDeleted = false
                },
                new CompanyAPI
                {
                    CompanyId = 2,
                    Code = "NotifyAnchorOrderCanceled",
                    IsWebhook = true,
                    ApiType = "POST",
                    APIUrl = "https://uat.shopkirana.in//api/ScaleUpIntegration/NotifyAnchorOrderCanceled",
                    Authtype = "",
                    APIKey = "",
                    APISecret = "",
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
                    EntityName = "AccountCode",
                    DefaultNo = 1,
                    EntityQuery = "select @numCnt= count(Id) from LoanAccounts where AccountCode = @numberStr",
                    Separator = "/",
                    IsActive = true,
                    IsDeleted = false
                },
                new EntityMaster
                {
                    EntityName = "Transaction",
                    DefaultNo = 1,
                    EntityQuery = "select @numCnt= count(Id) from AccountTransactions where ReferenceId = @numberStr",
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

            if (!context.TransactionDetailHeads.Any())
            {
                context.TransactionDetailHeads.AddRange(
                        new TransactionDetailHead { Code = "DisbursementAmount", SequenceNo = 1, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "Order", SequenceNo = 2, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "Interest", SequenceNo = 3, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "Payment", SequenceNo = 4, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "InterestPaymentAmount", SequenceNo = 5, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "OverdueInterestAmount", SequenceNo = 6, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "Refund", SequenceNo = 7, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "PartialRefund", SequenceNo = 8, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "DelayPenalty", SequenceNo = 9, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "ExtraPaymentAmount", SequenceNo = 10, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "OverduePaymentAmount", SequenceNo = 11, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "PenalPaymentAmount", SequenceNo = 12, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "Discount", SequenceNo = 13, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "BouncePaymentAmount", SequenceNo = 14, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "Canceled", SequenceNo = 15, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "GST18", SequenceNo = 16, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "ProcessingFee", SequenceNo = 17, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "ConvenienceFee", SequenceNo = 18, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "BounceCharge", SequenceNo = 19, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "ProcessingFeePaymentAmount", SequenceNo = 20, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "GSTPaymentAmount", SequenceNo = 21, IsActive = true, IsDeleted = false },
                        new TransactionDetailHead { Code = "Gst", SequenceNo = 22, IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }


            if (!context.TransactionStatuses.Any())
            {
                context.TransactionStatuses.AddRange(
                    new TransactionStatus { Code = "Pending", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "Due", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "Overdue", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "Paid", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "Delinquent", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "Initiate", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "Failed", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "Intransit", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "NBFCPostingFailed", IsActive = true, IsDeleted = false },
                    new TransactionStatus { Code = "Canceled", IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }

            if (!context.TransactionTypes.Any())
            {
                context.TransactionTypes.AddRange(
                    new TransactionType { IsDetailHead = false, Code = "Disbursement", IsActive = true, IsDeleted = false },
                    new TransactionType { IsDetailHead = false, Code = "OrderPlacement", IsActive = true, IsDeleted = false },
                    new TransactionType { IsDetailHead = false, Code = "PenaltyCharges", IsActive = true, IsDeleted = false },
                    new TransactionType { IsDetailHead = false, Code = "BounceCharges", IsActive = true, IsDeleted = false },
                    new TransactionType { IsDetailHead = false, Code = "OrderPayment", IsActive = true, IsDeleted = false },
                    new TransactionType { IsDetailHead = false, Code = "Refund", IsActive = true, IsDeleted = false },
                    new TransactionType { IsDetailHead = false, Code = "OverdueInterest", IsActive = true, IsDeleted = false },
                    new TransactionType { IsDetailHead = false, Code = "ProcessingFee", IsActive = true, IsDeleted = false },
                    new TransactionType { IsDetailHead = false, Code = "ProcessingFeePayment", IsActive = true, IsDeleted = false }
                );
                context.SaveChanges();
            }

            if (!context.NBFCCompanyAPIs.Any())
            {
                context.NBFCCompanyAPIs.AddRange(
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/los/applications/{{APPLICATION_ID}}/invoices/",
                        Code = "BlackSoilWithdrawalRequest",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/los/applications/{{APPLICATION_ID}}/invoices/{{ID}}/files/",
                        Code = "BlackSoilWithdrawalRequestUpdateDoc",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/credit_line/",
                        Code = "BlackSoilGetCreditLine",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/available_limit/",
                        Code = "BlackSoilGetAvailableCreditLimit",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/lms/loan_accounts/{{accountid}}/repayments/{{repaymentid}}/",
                        Code = "BlackSoilLoanRepayment",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/lms/loan_accounts/{{accountid}}/?expand=topups.loan_data,business,extra,topups.invoice.files,topups.extra",
                        Code = "BlackSoilLoanAccountDetail",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/los/invoices_bulk/bulk_invoice_files_update/",
                        Code = "BlackSoilBulkInvoicesApprove",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/lms/loan_accounts/{{loan_account_id}}/recalculate_accounting/",
                        Code = "BlackSoilRecalculateAccounting",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://stag.saraloan.in/api/v1/lms/loan_accounts/{{LOAN_ACCOUNT_ID}}/repayments/",
                        Code = "BlackSoilListRepayments",
                        TAPIKey = "shopkirana@saraloan.in",
                        TAPISecretKey = "shopkirana@saraloan.in",
                        TReferralCode = "SHOP0154",
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPI
                    {
                        APIUrl = "https://uat-apiorigin.arthmate.com/api/repayment_schedule",
                        Code = "ArthmateRepaymentSchedule",
                        TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE",
                        TAPISecretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6InNlcnZpY2UiLCJ0b2tlbl9pZCI6IjQxMzcyODYtU0hPMDE2MS0xNjk3MDk3MjY5ODg4IiwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiaWF0IjoxNjk3MDk3MjY5fQ.xJplj4jNgn_NbNnpcOJ1IywR_nwj21YoEldRHxF-NRA",
                        TReferralCode = "",
                        IsActive = true,
                        IsDeleted = false
                    }
                    );
                context.SaveChanges();
            }

            if (!context.NBFCCompanyAPIFlows.Any())
            {
                var apis = context.NBFCCompanyAPIs.ToList();
                context.NBFCCompanyAPIFlows.AddRange(
                    new NBFCCompanyAPIFlow
                    {
                        NBFCCompanyAPIId = apis.FirstOrDefault(x => x.Code == "BlackSoilGetAvailableCreditLimit").Id,
                        NBFCIdentificationCode = "BlackSoil",
                        TransactionStatusCode = "Initiate",
                        TransactionTypeCode = "OrderPlacement",
                        Sequence = 1,
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPIFlow
                    {
                        NBFCCompanyAPIId = apis.FirstOrDefault(x => x.Code == "BlackSoilWithdrawalRequest").Id,
                        NBFCIdentificationCode = "BlackSoil",
                        TransactionStatusCode = "Initiate",
                        TransactionTypeCode = "OrderPlacement",
                        Sequence = 2,
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPIFlow
                    {
                        NBFCCompanyAPIId = apis.FirstOrDefault(x => x.Code == "BlackSoilGetAvailableCreditLimit").Id,
                        NBFCIdentificationCode = "BlackSoil",
                        TransactionStatusCode = "Captured",
                        TransactionTypeCode = "OrderPlacement",
                        Sequence = 1,
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPIFlow
                    {
                        NBFCCompanyAPIId = apis.FirstOrDefault(x => x.Code == "BlackSoilWithdrawalRequestUpdateDoc").Id,
                        NBFCIdentificationCode = "BlackSoil",
                        TransactionStatusCode = "Captured",
                        TransactionTypeCode = "OrderPlacement",
                        Sequence = 2,
                        IsActive = true,
                        IsDeleted = false
                    },
                    new NBFCCompanyAPIFlow
                    {
                        NBFCCompanyAPIId = apis.FirstOrDefault(x => x.Code == "BlackSoilBulkInvoicesApprove").Id,
                        NBFCIdentificationCode = "BlackSoil",
                        TransactionStatusCode = "Captured",
                        TransactionTypeCode = "OrderPlacement",
                        Sequence = 3,
                        IsActive = true,
                        IsDeleted = false
                    }
                );
                context.SaveChanges();
            }

            if (!context.LoanAccountTemplateMasters.Any())
            {
                context.LoanAccountTemplateMasters.AddRange(
                    new LoanAccountTemplateMaster { TemplateCode = "OrderPlaceOTPSMS", TemplateType = "SMS", DLTID = "1707170935813998088", Template = "Dear {#var1#}, {#var2#} is your One Time Password for payment of Rs. {#var3#}/- at {#var4#} Via Scaleup Pay. Do not share OTP for security reasons. -ScaleupFin", IsActive = true, IsDeleted = false },
                    new LoanAccountTemplateMaster { TemplateCode = "OverdueSMS", TemplateType = "SMS", DLTID = "1707170935826848249", Template = "Dear {#var1#},   INR {#var2#} is overdue for Order {#var3#} at {#var4#}, Pay immediately to avoid any charges. Please ignore if already Paid. -ScaleupFin", IsActive = true, IsDeleted = false },
                    new LoanAccountTemplateMaster { TemplateCode = "SendInvoiceDisbursment", TemplateType = "SMS", DLTID = "1707170935820956185", Template = "Dear {#var5#}, Payment of INR {#var1#}/- for your Order {#var2#} is due on {#var3#}. Keep a/c ending {#var4#} funded to avoid charges. -ScaleupFin", IsActive = true, IsDeleted = false },
                    new LoanAccountTemplateMaster { TemplateCode = "OrderDeliveryDisbursementSMS", TemplateType = "SMS", DLTID = "1707170903489016945", Template = "Dear {#var1#} Your Order {#var2#} at {#var3#} has been delivered successfully. Due Date for Repayment shall be notified soon via SMS -ScaleupFin", IsActive = true, IsDeleted = false }
                    );
                context.SaveChanges();
            }
        }
    }
}
