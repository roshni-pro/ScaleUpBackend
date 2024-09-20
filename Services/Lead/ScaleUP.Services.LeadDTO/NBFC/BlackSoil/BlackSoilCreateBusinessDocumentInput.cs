using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public class BlackSoilCreateBusinessDocumentInput
    {
        public string doc_type { get; set; }
        public string doc_name { get; set; }
        public string doc_number { get; set; }
        public string files { get; set; }
    }
}
