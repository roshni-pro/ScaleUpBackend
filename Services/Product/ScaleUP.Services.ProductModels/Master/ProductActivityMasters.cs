using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(ProductActivityMasters))]
    public class ProductActivityMasters : BaseAuditableEntity
    {
        public required long ProductId { get; set; }
        public required long ActivityMasterId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        [ForeignKey("ActivityMasterId")]
        public ActivityMasters ActivityMasters { get; set; }
        public long? SubActivityMasterId { get; set; }

        [ForeignKey("SubActivityMasterId")]
        public SubActivityMasters SubActivityMasters { get; set; }
        public required int Sequence { get; set; }


    }
}
