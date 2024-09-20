using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadCommonRequestResponseDTO
    {
        public long? LeadId { get; set; }
        public long? CommonRequestResponseId { get; set; }
    }

    public class LeadCommonRequestInput
    {
        public long? LeadId { get; set; }
        public long? CommonRequestResponseId { get; set; }
    }
}
