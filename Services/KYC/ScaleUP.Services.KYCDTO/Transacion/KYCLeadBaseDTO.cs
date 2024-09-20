using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class KYCLeadBaseDTO
    {
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long SubActivityId { get; set; }
        public string UserId { get; set; }
        public long? ComapnyId { get; set; }
    }
}
