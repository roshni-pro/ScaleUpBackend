using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadOfferStatusDTO
    {
        public long LeadId { get; set; }
        public long LeadOfferId { get; set; }
        public long NbfcId { get; set; }
        public bool IsOfferGenerated { get; set; }
        public string CompanyIdentificationCode { get; set; }

    }
}
