using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadAadharDTO : LeadBaseDTO
    {
        public string DocumentNumber { get; set; }
        public string FrontDocumentId { get; set; }
        public string BackDocumentId { get; set; }
        public string otp { get; set; }
        public string requestId { get; set; }
    }
}
