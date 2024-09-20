using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class MSMEDTO
    {
        public long LeadMasterId { get; set; }
        //public string FrontFileUrl { get; set; }
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public int Vintage { get; set; }
        //extra fields added
        public string MSMERegNum { get; set; }
        public string MSMECertificate { get; set; }
        public long FrontDocumentId { get; set; }

    }
}
