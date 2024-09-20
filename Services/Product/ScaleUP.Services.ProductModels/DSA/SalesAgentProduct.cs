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
    [Table(nameof(SalesAgentProduct))]
    public class SalesAgentProduct : BaseAuditableEntity
    {
        [ForeignKey("SalesAgentId")]
        public SalesAgent SalesAgents { get; set; }
        public required long SalesAgentId { get; set; }

        [ForeignKey("ProductId")]
        public Product Products { get; set; }
        public required long ProductId { get; set; } //BussinessLoan
    }
}
