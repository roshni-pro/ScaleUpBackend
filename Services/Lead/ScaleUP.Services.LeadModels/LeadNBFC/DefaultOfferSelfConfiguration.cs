using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.LeadNBFC
{
    public class DefaultOfferSelfConfiguration : BaseAuditableEntity
    {
        public required long CompanyId { get; set; }
        public required int MinCibilScore { get; set; }
        public required int MaxCibilScore { get; set; }
        public required double MultiPlier { get; set; }
        public required double MaxCreditLimit { get; set; }
        public required double MinCreditLimit { get; set; }
        public required int MinVintageDays { get; set; }
        public required int MaxVintageDays { get; set; }
        [StringLength(20)]
        public required string CustomerType { get; set; }
    }
}
