using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class AddCibilDetailsRequestDTO
    {
        public long LeadId { get; set; }
        public string ProductCode { get; set; }
        public double CibilScore { get; set; }
        public string CibilReport { get; set; }
    }
}
