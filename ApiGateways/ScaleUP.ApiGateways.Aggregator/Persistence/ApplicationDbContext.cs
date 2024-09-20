using Microsoft.EntityFrameworkCore;
using ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using System.Reflection;

namespace ScaleUP.ApiGateways.Aggregator.Persistence
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
        public DbSet<Test> TestMaster => Set<Test>();
        public DbSet<AppHome> AppHomeDb => Set<AppHome>();
        public DbSet<AppHomeItem> AppHomeItemDb => Set<AppHomeItem>();
        public DbSet<AppHomeContent> AppHomeContentDb => Set<AppHomeContent>();
        public DbSet<AppHomeFunction> AppHomeFunctionDb => Set<AppHomeFunction>();
    }
}
