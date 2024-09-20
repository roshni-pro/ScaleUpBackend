using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LoanAccountModels.Master;
using ScaleUP.Services.LoanAccountModels.Transaction;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LoanAccountModels.Master.NBFC;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountModels.Master.NBFC.ArthMate;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC.Arthmate;
using ScaleUP.Services.LoanAccountModels.DSA;
using ScaleUP.Services.LoanAccountModels.Master.BusinessLoan;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC.AyeFinance;

namespace ScaleUP.Services.LoanAccountAPI.Persistence
{
    public class LoanAccountApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly MediatR.IMediator _mediator;
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

        public LoanAccountApplicationDbContext(
            DbContextOptions<LoanAccountApplicationDbContext> options,
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

        public DbSet<LoanAccount> LoanAccounts => Set<LoanAccount>();
        public DbSet<AccountTransactionDetail> AccountTransactionDetails => Set<AccountTransactionDetail>();
        public DbSet<AccountTransaction> AccountTransactions => Set<AccountTransaction>();
        public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
        public DbSet<TransactionStatus> TransactionStatuses => Set<TransactionStatus>();
        public DbSet<TransactionDetailHead> TransactionDetailHeads => Set<TransactionDetailHead>();
        public DbSet<LoanAccountCredit> LoanAccountCredits => Set<LoanAccountCredit>();
        public DbSet<EntityMaster> EntityMasters => Set<EntityMaster>();
        public DbSet<EntitySerialMaster> EntitySerialMasters => Set<EntitySerialMaster>();
        public DbSet<PaymentRequest> PaymentRequest => Set<PaymentRequest>();
        public DbSet<OTPMaster> OTPMaster => Set<OTPMaster>();
        public DbSet<LoanAccountCompanyLead> LoanAccountCompanyLead => Set<LoanAccountCompanyLead>();
        public DbSet<CompanyAPI> CompanyAPIs => Set<CompanyAPI>();
        public DbSet<ThirdPartyRequest> ThirdPartyRequest => Set<ThirdPartyRequest>();
        //public DbSet<CommonAPIRequestResponse> CommonAPIRequestResponses => Set<CommonAPIRequestResponse>();
        public DbSet<NBFCCompanyAPI> NBFCCompanyAPIs => Set<NBFCCompanyAPI>();
        public DbSet<NBFCCompanyAPIFlow> NBFCCompanyAPIFlows => Set<NBFCCompanyAPIFlow>();
        public DbSet<BlackSoilAccountDetail> BlackSoilAccountDetails => Set<BlackSoilAccountDetail>();
        public DbSet<BlackSoilAccountTransaction> BlackSoilAccountTransactions => Set<BlackSoilAccountTransaction>();
        public DbSet<BlackSoilCommonAPIRequestResponse> BlackSoilCommonAPIRequestResponses => Set<BlackSoilCommonAPIRequestResponse>();
        public DbSet<NBFCCompanyAPIMaster> NBFCComapnyAPIMasters => Set<NBFCCompanyAPIMaster>();
        public DbSet<NBFCCompanyApiDetail> NBFCComapnyApiDetails => Set<NBFCCompanyApiDetail>();
        public DbSet<BlackSoilWebhookResponse> BlackSoilWebhookResponses => Set<BlackSoilWebhookResponse>();
        public DbSet<LoanAccountRepayment> LoanAccountRepayments => Set<LoanAccountRepayment>();
        public DbSet<LoanAccountRepaymentRefresh> LoanAccountRepaymentRefreshs => Set<LoanAccountRepaymentRefresh>();
        public DbSet<InvoiceDisbursalProcessed> InvoiceDisbursalProcesseds => Set<InvoiceDisbursalProcessed>();
        public DbSet<LoanAccountTemplateMaster> LoanAccountTemplateMasters => Set<LoanAccountTemplateMaster>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<LoanBankDetail> LoanBankDetails => Set<LoanBankDetail>();

        //public DbSet<LeadLoan> LeadLoans => Set<LeadLoan>();
        public DbSet<BusinessLoan> BusinessLoans => Set<BusinessLoan>();
        public DbSet<CompanyInvoice> CompanyInvoices => Set<CompanyInvoice>();
        public DbSet<CompanyInvoiceDetail> CompanyInvoiceDetails => Set<CompanyInvoiceDetail>();
        public DbSet<ProductAnchorCompany> ProductAnchorCompany => Set<ProductAnchorCompany>();
        public DbSet<ProductNBFCCompany> ProductNBFCCompany => Set<ProductNBFCCompany>();
        public DbSet<ArthmateDisbursement> ArthmateDisbursement => Set<ArthmateDisbursement>();
        public DbSet<ArthMateCommonAPIRequestResponse> ArthMateCommonAPIRequestResponses => Set<ArthMateCommonAPIRequestResponse>();
        public DbSet<SalesAgentLoanDisbursment> SalesAgentLoanDisbursments => Set<SalesAgentLoanDisbursment>();
        public DbSet<RepaymentSchedule> repaymentSchedules => Set<RepaymentSchedule>();
        public DbSet<OfferConfigurationByLoanAccount> OfferConfigurationByLoanAccount => Set<OfferConfigurationByLoanAccount>();
        public DbSet<BusinessLoanDisbursementDetail> BusinessLoanDisbursementDetail => Set<BusinessLoanDisbursementDetail>();
        public DbSet<CompanyInvoiceSettlement> CompanyInvoiceSettlements => Set<CompanyInvoiceSettlement>();
        public DbSet<HopDashboardData> HopDashboardData => Set<HopDashboardData>();
        public DbSet<BLPaymentUpload> BLPaymentUploads => Set<BLPaymentUpload>();
        public DbSet<AyeFinanceSCFCommonAPIRequestResponse> AyeFinanceSCFCommonAPIRequestResponses => Set<AyeFinanceSCFCommonAPIRequestResponse>();
        public DbSet<HopDashboardRevenue> HopDashboardRevenue => Set<HopDashboardRevenue>();
        public DbSet<AyeFinanceUpdate> AyeFinanceUpdates => Set<AyeFinanceUpdate>();
    }

}
    