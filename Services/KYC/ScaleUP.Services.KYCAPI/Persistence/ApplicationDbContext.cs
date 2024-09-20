using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.KYCModels.Master;
using ScaleUP.Services.KYCModels.Transaction;
using System.Reflection;

namespace ScaleUP.Services.KYCAPI.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly MediatR.IMediator _mediator;
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            MediatR.IMediator mediator,
            AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
            : base(options)
        {
            _mediator = mediator;
            _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        }
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<KYCMaster> KYCMasters => Set<KYCMaster>();
        public DbSet<KYCDetail> KYCDetails => Set<KYCDetail>();
        public DbSet<KYCMasterInfo> KYCMasterInfos => Set<KYCMasterInfo>();
        public DbSet<KYCDetailInfo> KYCDetailInfos => Set<KYCDetailInfo>();
        public DbSet<ThirdPartyAPIConfig> ThirdPartyAPIConfigs => Set<ThirdPartyAPIConfig>();

        public DbSet<ApiRequestResponse> ApiRequestResponse=>Set<ApiRequestResponse>();
        public DbSet<ElectricityBillServiceProvider> ElectricityBillServiceProvider => Set<ElectricityBillServiceProvider>();
        public DbSet<KarzaElectricityDistrict> karzaElectricityDistricts => Set<KarzaElectricityDistrict>();

        //public DbSet<AddressType> AddressTypes => Set<AddressType>();
        //public DbSet<AddressTypeMapping> AddressTypeMappings => Set<AddressTypeMapping>();


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
