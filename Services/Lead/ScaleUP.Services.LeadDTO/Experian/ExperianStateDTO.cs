using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Experian
{
    public class ExperianStateDTO
    {
    }

    public class ExperianStateRequest
    {
        public long LocationStateId { get; set; }
        public long ExperianStateId { get; set; }
    }
}
