using System.Reflection.Emit;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Global.Infrastructure.Persistence.Interceptors;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.CommunicationModels;
using ScaleUP.Global.Infrastructure.Common.Models;

namespace ScaleUp.Services.CommunicationAPI.Persistence
{
    public class CommunicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly MediatR.IMediator _mediator;
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

        public CommunicationDbContext(
            DbContextOptions<CommunicationDbContext> options,
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
            foreach (var property in builder.Model.GetEntityTypes())
            {
                if (property.ClrType.IsAssignableTo(typeof(BaseAuditableEntity)))
                {
                    property.SetIsTemporal(false);
                }
            }
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

        public DbSet<SendOTPDetails> SendOTPDetails => Set<SendOTPDetails>();
        public DbSet<SendEmailOtpDetails> SendEmailOtpDetails => Set<SendEmailOtpDetails>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    }
}
