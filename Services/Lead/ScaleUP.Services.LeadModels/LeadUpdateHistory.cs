using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScaleUP.Services.LeadModels
{
    public class LeadUpdateHistory : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        [MaxLength(150)]
        public string UserId { get; set; }
        public string UserName { get; set; }
        
        [StringLength(100)]
        public string EventName { get; set; }
        [StringLength(10000)]
        public string Narration { get; set; }
        [StringLength(10000)]
        public string NarrationHTML { get; set; }
    }
}
