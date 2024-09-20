using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public class BlackSoilCreateBankInput
    {
        public string ifsc { get; set; }
        public string bank_name { get; set; }
        public string account_number { get; set; }
        public string account_holder_name { get; set; }
        public string account_type { get; set; }
        public string password { get; set; }
        public bool is_for_disbursement { get; set; }
        public bool is_for_nach { get; set; }
        public string business_id { get; set; }

    }
}
