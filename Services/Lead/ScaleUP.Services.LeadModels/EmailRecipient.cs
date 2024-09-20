using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class EmailRecipient : BaseAuditableEntity
    {
        [StringLength(500)]
        public required string EmailType { get; set; }
        [StringLength(2000)]
        public required string To { get; set; }
        [StringLength(2000)]
        public string? From { get; set; }
        [StringLength(2000)]
        public string? Bcc { get; set; }
        public long? LimitValue { get; set; }
    }
}
