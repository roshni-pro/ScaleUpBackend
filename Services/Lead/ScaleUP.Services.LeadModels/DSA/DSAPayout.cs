using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.DSA
{
    public class DSAPayout : BaseAuditableEntity
    {

        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }
        public required long LeadId { get; set; }
        public required double PayoutPercenatge { get; set; }
        public required int MinAmount { get; set; }
        public required int MaxAmount { get; set; }
        public required long ProductId  { get; set; }
    }
}
