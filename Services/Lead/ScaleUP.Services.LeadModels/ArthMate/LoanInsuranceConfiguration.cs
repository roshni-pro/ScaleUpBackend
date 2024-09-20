using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class LoanInsuranceConfiguration : BaseAuditableEntity
    {
        public int MonthDuration { get; set; }
        public double RateOfInterestInPer { get; set; }
        [StringLength(100)]
        public string Remarks { get; set; }

    }
}
