using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductModels.DSA
{
    [Table(nameof(PayOutMaster))]
    public class PayOutMaster : BaseAuditableEntity
    {
        [StringLength(200)]
        public required string Type { get; set; } //PF, Disbursment
    }
}
