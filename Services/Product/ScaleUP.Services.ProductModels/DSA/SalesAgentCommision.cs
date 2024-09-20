using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.ProductModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductModels.DSA
{
    [Table(nameof(SalesAgentCommision))]
    public class SalesAgentCommision : BaseAuditableEntity
    {
        [ForeignKey("SalesAgentProductId")]
        public SalesAgentProduct SalesAgentProducts { get; set; }
        public required long SalesAgentProductId { get; set; }

        [ForeignKey("PayOutMasterId")]
        public PayOutMaster PayOutMasters { get; set; }
        public required long PayOutMasterId { get; set; }
        public required double CommisionPercentage { get; set; }
        public required int MinAmount { get; set; }
        public required int MaxAmount { get; set; }
    }
}
