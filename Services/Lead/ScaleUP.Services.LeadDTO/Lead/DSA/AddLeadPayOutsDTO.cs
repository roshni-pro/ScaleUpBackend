using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead.DSA
{
    public class AddLeadPayOutsDTO
    {
        public long leadId { get; set; }
        public double payoutPerc { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public long ProductId { get; set; }
    }
}
