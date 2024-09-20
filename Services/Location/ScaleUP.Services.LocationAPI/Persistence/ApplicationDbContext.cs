using MediatR;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Persistence;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using System.Reflection;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LocationModels.Master;
using ScaleUP.Services.LocationModels.Transaction;
using ScaleUP.Global.Infrastructure.Common.Models;

namespace ScaleUP.Services.LocationAPI.Persistence
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

        public DbSet<Country> Countries => Set<Country>();
        public DbSet<State> States => Set<State>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<AddressType> AddressTypes => Set<AddressType>();


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
