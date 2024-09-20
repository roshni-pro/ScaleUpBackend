using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(CompanyCreditDays))]
    public class CompanyCreditDays : BaseAuditableEntity
    {
        [ForeignKey("ProductAnchorCompanyId")]
        public ProductAnchorCompany ProductAnchorCompany { get; set; }
        public required long ProductAnchorCompanyId { get; set; }

        [ForeignKey("CreditDaysMasterId")]
        public CreditDayMasters CreditDayMasters { get; set; }
        public required long CreditDaysMasterId { get; set; }
    }
}
