using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadOfferInitiatedStatusResponseDTO
    {
        public long NbfcCompanyId { get; set; }
        public string Status { get; set; }
        public long LeadId { get; set; }
    }
}
