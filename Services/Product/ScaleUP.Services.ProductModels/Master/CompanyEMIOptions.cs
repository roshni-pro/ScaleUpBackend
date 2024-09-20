using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(CompanyEMIOptions))]
    public class CompanyEMIOptions : BaseAuditableEntity
    {
        [ForeignKey("ProductAnchorCompanyId")]
        public ProductAnchorCompany ProductAnchorCompany { get; set; }
        public required long ProductAnchorCompanyId { get; set; }

        [ForeignKey("EMIOptionMasterId")]
        public EMIOptionMasters EMIOptionMasters { get; set; }
        public required long EMIOptionMasterId { get; set; }
    }
}
