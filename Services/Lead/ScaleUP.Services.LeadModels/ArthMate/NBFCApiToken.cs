using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class NBFCApiToken : BaseAuditableEntity
    {
        [StringLength(50)]
        public required string IdentificationCode { get; set; }
        [StringLength(50)]
        public required string TokenType { get; set; }
        [StringLength(5000)]
        public required string TokenValue { get; set; }
    }
}
