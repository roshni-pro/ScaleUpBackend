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
    public class BlackSoilPFCollection : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }
        public double processing_fee { get; set; }
        public double processing_fee_tax { get; set; }
        public double total_processing_fee { get; set; }
        [StringLength(300)]
        public string status { get; set; }
    }
}
