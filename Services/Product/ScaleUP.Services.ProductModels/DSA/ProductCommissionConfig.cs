using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.ProductModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductModels.DSA
{
    [Table(nameof(ProductCommissionConfig))]
    public class ProductCommissionConfig : BaseAuditableEntity
    {
        [ForeignKey("PayOutMasterId")]
        public PayOutMaster PayOutMasters { get; set; }
        public required long PayOutMasterId { get; set; }
        [ForeignKey("ProductId")]
        public Product Products { get; set; }
        public required long ProductId { get; set; }
        public required string Type { get; set; } //Amount, Percentage
        public required double CommisionValue { get; set; }

        //new add for DSA and connector
        public string? DSAAgreement { get; set; }
        public string? ConnectorAgreement { get; set; }

    }
}
