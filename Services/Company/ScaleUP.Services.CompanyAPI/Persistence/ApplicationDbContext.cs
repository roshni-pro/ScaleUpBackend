using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.CompanyModels;
using ScaleUP.Services.CompanyModels.Master;
using System.Reflection;

namespace ScaleUP.Services.CompanyAPI.Persistence
{
    public class CompanyApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly MediatR.IMediator _mediator;
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

        public CompanyApplicationDbContext(
            DbContextOptions<CompanyApplicationDbContext> options,
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

        public DbSet<Companies> Companies => Set<Companies>();

        public DbSet<CompanyLocation> CompanyLocations => Set<CompanyLocation>();

        public DbSet<CompanyUsers> CompanyUsers => Set<CompanyUsers>();
        public DbSet<CompanyPartnerDetail> CompanyPartnerDetails => Set<CompanyPartnerDetail>();
        public DbSet<BusinessTypeMaster> BusinessTypeMasters => Set<BusinessTypeMaster>();

        public DbSet<GstMaster> GSTMasters => Set<GstMaster>();

        public DbSet<EntityMaster> EntityMasters => Set<EntityMaster>();
        public DbSet<EntitySerialMaster> EntitySerialMasters => Set<EntitySerialMaster>();
        public DbSet<GSTverifiedRequest> GSTverifiedRequests => Set<GSTverifiedRequest>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<CompanyTemplateMaster> CompanyTemplateMasters => Set<CompanyTemplateMaster>();
        public DbSet<FinancialLiaisonDetails> FinancialLiaisonDetails => Set<FinancialLiaisonDetails>();
        public DbSet<EducationMaster> EducationMasters => Set<EducationMaster>();
        public DbSet<LanguageMaster> LanguageMasters => Set<LanguageMaster>();
    }
}
