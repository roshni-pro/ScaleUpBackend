using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(ProductCompanyActivityMasters))]
    public class ProductCompanyActivityMasters : BaseAuditableEntity
    {
        public required long CompanyId { get; set; }
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
