using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(EMIOptionMasters))]
    public class EMIOptionMasters : BaseAuditableEntity
    {
        [StringLength(100)]
        public required string Name { get; set; }
        public ICollection<CompanyEMIOptions> CompanyEMIOptions { get; set; }
    }
}
