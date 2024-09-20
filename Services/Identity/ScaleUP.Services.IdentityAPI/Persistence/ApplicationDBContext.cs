using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Common.Models.Identity;
using ScaleUP.Services.IdentityModels.Master;
//using ScaleUP.Global.Infrastructure.Persistence;

namespace ScaleUP.Services.IdentityAPI.Persistence
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(
           DbContextOptions<ApplicationDBContext> options)
           : base(options)
        {
        }
        public DbSet<AspNetPageMaster> AspNetPageMasters => Set<AspNetPageMaster>();
        public DbSet<AspNetRolePagePermission> AspNetRolePagePermission => Set<AspNetRolePagePermission>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
