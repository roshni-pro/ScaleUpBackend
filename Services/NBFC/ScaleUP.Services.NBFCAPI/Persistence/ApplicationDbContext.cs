using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.NBFCModels.Master;
using ScaleUP.Global.Infrastructure.Common.Models;


namespace ScaleUP.Services.NBFCAPI.Persistence
{
    public class NBFCApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly MediatR.IMediator _mediator;
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

        public NBFCApplicationDbContext(
            DbContextOptions<NBFCApplicationDbContext> options,
            MediatR.IMediator mediator,
            AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
            : base(options)
        {
            _mediator = mediator;
            _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        }
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<OfferSelfConfiguration> OfferSelfConfigurations => Set<OfferSelfConfiguration>();



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
