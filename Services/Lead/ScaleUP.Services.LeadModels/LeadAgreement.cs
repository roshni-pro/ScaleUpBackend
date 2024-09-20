using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class LeadAgreement : BaseAuditableEntity
    {
        [StringLength(500)]
        public required string DocUnSignUrl { get; set; }
        [StringLength(500)]
        public string? DocSignedUrl { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ExpiredOn { get; set; }
        public required long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }

        [StringLength(300)]
        public string DocumentId { get; set; }
        [StringLength(500)]
        public string? eSignUrl { get; set; }
        [StringLength(200)]
        public string? Status { get; set; } //Signed, Pending,Failed, Expired
    }
}
