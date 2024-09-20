using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class ManageRejectActivityDc
    {
        public bool IsRejected { get; set; }
        public long LeadId { get; set; }

        public long ActivityMasterId { get; set; }
        public long? SubActivityMasterId { get; set; }
        public string Message { get; set; }
        public bool IsKYCCompleted { get; set; }

    }
}
