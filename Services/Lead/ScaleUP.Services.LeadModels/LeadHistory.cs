using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class LeadHistory : BaseAuditableEntity
    {
        [MaxLength(150)]
        public string UserId { get; set; }
        public string LeadId { get; set; }
        public string EntityName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Changes { get; set; }
    }
}
