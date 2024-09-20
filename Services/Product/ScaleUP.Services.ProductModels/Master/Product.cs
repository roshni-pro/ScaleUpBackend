using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(Product))]
    public class Product : BaseAuditableEntity
    {
        [StringLength(200)]
        public required string Name { get; set; }

        [StringLength(100)]
        public required string Type { get; set; } //CreditLine, Business Loan

        [StringLength(100)]
        public required string ProductCode { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

       // public ICollection<ProductCompanies> ProductCompanies { get; set; }

        public ICollection<ProductActivityMasters> ProductActivityMasters { get; set; }

        public ICollection<ProductCompanyActivityMasters> ProductCompanyActivityMasters { get; set; }

    }
}
