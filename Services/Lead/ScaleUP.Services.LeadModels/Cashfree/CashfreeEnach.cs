using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.Cashfree
{
    public class CashfreeEnach : BaseAuditableEntity
    {
        public required long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }
        [StringLength(250)]
        public string subscriptionId { get; set; } //unique
        [StringLength(40)]
        public string planName { get; set; } //arthmate loan number
        [StringLength(300)]
        public string? subReferenceId { get; set; }
        [StringLength(250)]
        public string? authLink { get; set; }
        [StringLength(300)]
        public string? Umrn { get; set; }
        public DateTime linkExpiryDate { get; set; }  //In Minute
        public DateTime expiresOn { get; set; }
        [StringLength(200)]
        public string status { get; set; } // "status": "ACTIVE", Deafult Intiiate
        public double recurringAmount { get; set; }
        public int maxCycles { get; set; }
        [StringLength(300)]
        public string? Comment { get; set; }


    }
}
