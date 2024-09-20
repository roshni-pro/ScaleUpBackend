using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class AddLeadGeneratorConvertorDTO
    {
        public long LeadId { get; set; }
        public string LeadGenerator { get; set; }
        public string LeadConvertor { get; set; }
        public string? UserName { get; set; }
    }
}
