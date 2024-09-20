using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class SelfieDTO
    {
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long SubActivityId { get; set; }
        public string UserId { get; set; }
        public long? ComapnyId { get; set; }
        public int? FrontDocumentId { get; set; }
       
    }
}
