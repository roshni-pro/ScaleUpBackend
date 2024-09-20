using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.AyeFinance
{
    public class AyeFinanceUpdate: BaseAuditableEntity
    {
        public long? leadId { get; set; }
        [StringLength(100)]

        public string? refId { get; set; }
        [StringLength(100)]

        public string? leadCode { get; set; }
        [StringLength(100)]

        public string? switchpeReferenceId { get; set; }

        public double? totalLimit { get; set; }
        public double? availablelLimit { get; set; }
        [StringLength(100)]
        public string? status{ get; set; }
    }
}
