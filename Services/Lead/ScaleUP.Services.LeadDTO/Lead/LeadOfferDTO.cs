using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadOfferDTO
    {
        public double CreditLimit { get; set; }
        public double? InterestRate { get; set; }
        public int? Tenure { get; set; }
    }
}
