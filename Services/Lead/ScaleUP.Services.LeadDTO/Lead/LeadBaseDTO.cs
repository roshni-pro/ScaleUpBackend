using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadBaseDTO
    {
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long SubActivityId { get; set; }
        public string UserId { get; set; }
        public long? CompanyId { get; set; }
    }
}
