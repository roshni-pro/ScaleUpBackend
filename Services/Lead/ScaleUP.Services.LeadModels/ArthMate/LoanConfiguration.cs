using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class LoanConfiguration : BaseAuditableEntity
    {
        public double PF { get; set; } 
        public double GST { get; set; }
        public double ODCharges { get; set; } 
        public int ODdays { get; set; }
        public double InterestRate { get; set; } //MinInterestRate
        public double BounceCharge { get; set; }
        public double PenalPercent { get; set; }

        //new fields added for Interest Rate min and max 16-05-2024
        public double? MaxInterestRate { get; set; }
        public bool IseSignEnable { get; set; }

    }
}
