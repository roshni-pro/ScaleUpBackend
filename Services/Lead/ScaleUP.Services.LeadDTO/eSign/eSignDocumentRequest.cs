using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.eSign
{
    public class eSignDocumentRequest
    {
        public string documentId { get; set; }
        public string verificationDetailsRequired { get; set; }
    }
}
