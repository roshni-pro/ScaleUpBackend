using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class OfferRejectDc
    {
        public long LeadId { get; set; }
        public long NBFCCompanyId { get; set; }
        public string CompanyIdentificationCode { get; set; }
        public string Message { get; set; }
    }
    public class NbfcOfferRequestDc
    {
        public long LeadId { get; set; }
        public string CompanyIdentificationCode { get; set; }
        public long? NbfcCompanyId { get; set; }
    }
}
