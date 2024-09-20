using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.LeadNBFC
{
    public class BlackSoilWebhookResponse : BaseAuditableEntity
    {
        public string response { get; set; }
        [StringLength(200)]
        public string eventName { get; set; }
        public string? Status { get; set; }
        public long? LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }


    }
}
