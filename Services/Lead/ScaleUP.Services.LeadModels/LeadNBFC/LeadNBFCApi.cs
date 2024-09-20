using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.LeadNBFC
{
    public class LeadNBFCApi : BaseAuditableEntity
    {
        [StringLength(300)]
        public string APIUrl { get; set; }
        [StringLength(300)]
        public string Code { get; set; }
        [StringLength(200)]
        public string Status { get; set; }
        [StringLength(500)]
        public string TAPIKey { get; set; }
        [StringLength(500)]
        public string TAPISecretKey { get; set; }
        public required int Sequence { get; set; }

        public long? LeadNBFCSubActivityId { get; set; }
        [ForeignKey("LeadNBFCSubActivityId")]
        public LeadNBFCSubActivity LeadNBFCSubActivitys { get; set; }

        public long? RequestId { get; set; }
        public long? ResponseId { get; set; }

        [StringLength(200)]
        public string? TReferralCode { get; set; }

    }
}
