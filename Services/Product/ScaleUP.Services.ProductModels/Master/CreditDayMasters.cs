using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(CreditDayMasters))]
    public class CreditDayMasters : BaseAuditableEntity
    {
        [StringLength(100)]
        public required string Name { get; set; }
        public int Days { get; set; }
        public ICollection<CompanyCreditDays> CompanyCreditDays { get; set; }
    }
}
