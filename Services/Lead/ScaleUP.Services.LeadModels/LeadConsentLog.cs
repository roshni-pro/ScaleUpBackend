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
    public class LeadConsentLog : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        [StringLength(100)]
        public string Type { get; set; } //borrower,beneficiary, 

        public bool IsChecked { get; set; }


    }
}
