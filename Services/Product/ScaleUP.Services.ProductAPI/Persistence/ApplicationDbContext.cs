using MediatR;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Persistence;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using System.Reflection;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.ProductModels.Master;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.ProductModels.DSA;

namespace ScaleUP.Services.ProductAPI.Persistence
{
    public class ProductApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly MediatR.IMediator _mediator;
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

        public ProductApplicationDbContext(
            DbContextOptions<ProductApplicationDbContext> options,
            MediatR.IMediator mediator,
            AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
            : base(options)
        {
            _mediator = mediator;
            _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        }
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ActivityMasters> ActivityMasters => Set<ActivityMasters>();
        public DbSet<ProductActivityMasters> ProductActivityMasters => Set<ProductActivityMasters>();
        public DbSet<ProductCompanyActivityMasters> ProductCompanyActivityMasters => Set<ProductCompanyActivityMasters>();
        public DbSet<SubActivityMasters> SubActivityMasters => Set<SubActivityMasters>();
        public DbSet<ProductNBFCCompany> ProductNBFCCompany => Set<ProductNBFCCompany>();
        public DbSet<ProductAnchorCompany> ProductAnchorCompany => Set<ProductAnchorCompany>();
        public DbSet<CreditDayMasters> CreditDayMasters => Set<CreditDayMasters>();
        public DbSet<EMIOptionMasters> EMIOptionMasters => Set<EMIOptionMasters>();
        public DbSet<CompanyCreditDays> CompanyCreditDays => Set<CompanyCreditDays>();
        public DbSet<CompanyEMIOptions> CompanyEMIOptions => Set<CompanyEMIOptions>();

        public DbSet<CompanyApi> CompanyApis => Set<CompanyApi>();
        public DbSet<NBFCCompanyApiType> NBFCCompanyApiTypes => Set<NBFCCompanyApiType>();
        public DbSet<NBFCSubActivityApi> NBFCSubActivityApis => Set<NBFCSubActivityApi>();
        public DbSet<ProductTemplateMaster> ProductTemplateMasters => Set<ProductTemplateMaster>();


        #region DSA Implementation

        public DbSet<PayOutMaster> PayOutMasters => Set<PayOutMaster>();
        public DbSet<SalesAgent> SalesAgents => Set<SalesAgent>();
        public DbSet<SalesAgentProduct> SalesAgentProducts => Set<SalesAgentProduct>();
        public DbSet<ProductCommissionConfig> ProductCommissionConfigs => Set<ProductCommissionConfig>();
        public DbSet<SalesAgentCommision> SalesAgentCommisions => Set<SalesAgentCommision>();
        public DbSet<ProductSlabConfiguration> ProductSlabConfigurations => Set<ProductSlabConfiguration>();
        public DbSet<EntityMaster> EntityMaster => Set<EntityMaster>();
        public DbSet<EntitySerialMaster> EntitySerialMaster => Set<EntitySerialMaster>();


        #endregion



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
    }
}
