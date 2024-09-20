using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadGSTDTO : LeadBaseDTO
    {
        public long? DocumentId { get; set; }
        public string UniqueId { get; set; }
    }
}
