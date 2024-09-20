using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Services.LeadModels.AyeFinance;
using ScaleUP.Services.LeadModels.BusinessLoan;
using ScaleUP.Services.LeadModels.Cashfree;
using ScaleUP.Services.LeadModels.DSA;
using ScaleUP.Services.LeadModels.FinBox;
using ScaleUP.Services.LeadModels.LeadNBFC;
using System.Reflection;

namespace ScaleUP.Services.LeadAPI.Persistence
{
    public class LeadApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly MediatR.IMediator _mediator;
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

        public LeadApplicationDbContext(
            DbContextOptions<LeadApplicationDbContext> options,
            MediatR.IMediator mediator,
            AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
            : base(options)
        {
            _mediator = mediator;
            _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _mediator.DispatchDomainEvents(this);

            return await base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<Leads> Leads => Set<Leads>();
        public DbSet<LeadOffers> LeadOffers => Set<LeadOffers>();

        public DbSet<CompanyLead> CompanyLead => Set<CompanyLead>();
        public DbSet<LeadActivityMasterProgresses> LeadActivityMasterProgresses => Set<LeadActivityMasterProgresses>();
        public DbSet<ThirdPartyApiConfig> ThirdPartyApiConfigs => Set<ThirdPartyApiConfig>();
        public DbSet<ThirdPartyRequest> ThirdPartyRequests => Set<ThirdPartyRequest>();
        public DbSet<ExperianState> ExperianStates => Set<ExperianState>();
        public DbSet<EntityMaster> EntityMasters => Set<EntityMaster>();

        public DbSet<EntitySerialMaster> EntitySerialMasters => Set<EntitySerialMaster>();
        public DbSet<LeadAgreement> leadAgreements => Set<LeadAgreement>();

        public DbSet<eNachBankDetail> eNachBankDetails => Set<eNachBankDetail>();
        public DbSet<BankList> BankLists => Set<BankList>();

        public DbSet<BusinessDetail> BusinessDetails => Set<BusinessDetail>();
        public DbSet<PersonalDetail> PersonalDetails => Set<PersonalDetail>();

        public DbSet<LeadNBFCSubActivity> LeadNBFCSubActivitys => Set<LeadNBFCSubActivity>();
        public DbSet<LeadNBFCApi> LeadNBFCApis => Set<LeadNBFCApi>();
        public DbSet<WebhookResponse> WebhookResponses => Set<WebhookResponse>();
        public DbSet<LeadBankDetail> LeadBankDetails => Set<LeadBankDetail>();
        public DbSet<LeadConsentLog> LeadConsentLogs => Set<LeadConsentLog>();

        //public DbSet<CommonAPIRequestResponse> CommonAPIRequestResponses => Set<CommonAPIRequestResponse>();

        public DbSet<BlackSoilUpdate> BlackSoilUpdates => Set<BlackSoilUpdate>();
        public DbSet<DefaultOfferSelfConfiguration> DefaultOfferSelfConfigurations => Set<DefaultOfferSelfConfiguration>();
        public DbSet<LeadCommonRequestResponse> LeadCommonRequestResponses => Set<LeadCommonRequestResponse>();

        public DbSet<BlackSoilWebhookResponse> BlackSoilWebhookResponses => Set<BlackSoilWebhookResponse>();
        public DbSet<LeadTemplateMaster> LeadTemplateMasters => Set<LeadTemplateMaster>();
        public DbSet<BlackSoilCommonAPIRequestResponse> BlackSoilCommonAPIRequestResponses => Set<BlackSoilCommonAPIRequestResponse>();
        public DbSet<LeadCompanyBuyingHistory> LeadCompanyBuyingHistorys => Set<LeadCompanyBuyingHistory>();
        public DbSet<LeadDocumentDetail> LeadDocumentDetails => Set<LeadDocumentDetail>();
        public DbSet<LeadHistory> LeadHistories => Set<LeadHistory>();
        public DbSet<LeadUpdateHistory> LeadUpdateHistories => Set<LeadUpdateHistory>();

        public DbSet<BlackSoilPFCollection> BlackSoilPFCollections => Set<BlackSoilPFCollection>();



        #region ArthMate 
        public DbSet<CeplrPdfReports> CeplrPdfReports => Set<CeplrPdfReports>();
        public DbSet<CoLenderResponse> CoLenderResponse => Set<CoLenderResponse>();
        public DbSet<KYCValidationResponse> KYCValidationResponse => Set<KYCValidationResponse>();
        public DbSet<ArthMateCommonAPIRequestResponse> ArthMateCommonAPIRequestResponses => Set<ArthMateCommonAPIRequestResponse>();
        public DbSet<ArthMateUpdates> ArthMateUpdates => Set<ArthMateUpdates>();
        public DbSet<ArthMateDocumentMaster> ArthMateDocumentMaster => Set<ArthMateDocumentMaster>();
        public DbSet<ArthMateStateCode> ArthMateStateCode => Set<ArthMateStateCode>();
        public DbSet<CeplrBankList> CeplrBankList => Set<CeplrBankList>();
        public DbSet<ArthMateWebhookResponse> ArthMateWebhookResponse => Set<ArthMateWebhookResponse>();
        public DbSet<NBFCApiToken> NBFCApiTokens => Set<NBFCApiToken>();
        public DbSet<ArthmateSlaLbaStampDetail> ArthmateSlaLbaStampDetail => Set<ArthmateSlaLbaStampDetail>();
        public DbSet<LeadLoan> LeadLoan => Set<LeadLoan>();
        public DbSet<LoanInsuranceConfiguration> LoanInsuranceConfiguration => Set<LoanInsuranceConfiguration>();
        public DbSet<ArthmateDisbursement> ArthmateDisbursements => Set<ArthmateDisbursement>();
        public DbSet<CompositeDisbursementWebhookResponse> CompositeDisbursementWebhookResponse => Set<CompositeDisbursementWebhookResponse>();
        public DbSet<LoanConfiguration> LoanConfiguration => Set<LoanConfiguration>();
        public DbSet<EmailRecipient> EmailRecipients => Set<EmailRecipient>();

        public DbSet<CashfreeEnach> CashfreeEnachs => Set<CashfreeEnach>();
        public DbSet<CashfreeCommonAPIRequestResponse> CashfreeCommonAPIRequestResponses => Set<CashfreeCommonAPIRequestResponse>();
        public DbSet<CashFreeEnachconfiguration> cashFreeEnachconfigurations => Set<CashFreeEnachconfiguration>();

        #endregion

        public DbSet<eSignDocumentResponse> eSignDocumentResponse => Set<eSignDocumentResponse>();
        public DbSet<DSAPayout> DSAPayouts => Set<DSAPayout>();
        public DbSet<eSignResponseDocumentId> eSignResponseDocumentIds => Set<eSignResponseDocumentId>();
        public DbSet<eSignKarzaCommonAPIRequestResponse> eSignKarzaCommonAPIRequestResponse => Set<eSignKarzaCommonAPIRequestResponse>();
        public DbSet<eSignWebhookResponse> eSignWebhookResponses => Set<eSignWebhookResponse>();
        public DbSet<BusinessLoanNBFCUpdate> BusinessLoanNBFCUpdate => Set<BusinessLoanNBFCUpdate>();

        public DbSet<CommonApiRequestResponse> ApiRequestResponse => Set<CommonApiRequestResponse>();
        public DbSet<OfferConfigurationByLead> OfferConfigurationByLead => Set<OfferConfigurationByLead>();
        public DbSet<nbfcOfferUpdate> nbfcOfferUpdate => Set<nbfcOfferUpdate>();
        public DbSet<AyeFinanceSCFCommonAPIRequestResponse> AyeFinanceSCFCommonAPIRequestResponses => Set<AyeFinanceSCFCommonAPIRequestResponse>();
        public DbSet<AyeFinanceUpdate> AyeFinanceUpdates => Set<AyeFinanceUpdate>();

        public DbSet<FinBoxApiConfig> finBoxApiConfigs => Set<FinBoxApiConfig>();
        public DbSet<FinBoxCommonAPIRequestResponse> FinBoxCommonAPIRequestResponse => Set<FinBoxCommonAPIRequestResponse>();

    }
}
