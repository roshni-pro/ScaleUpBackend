using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public class BlackSoilBusinessAddressUpdateInput
    {
        public string update_url { get; set; }
        public string full_address { get; set; } //full address include city state pincode
        public string address_line { get; set; }
        public string locality { get; set; }
        public string landmark { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string country { get; set; }
        public string address_type { get; set; }
        public string address_name { get; set; }
        public string business { get; set; }
    }
}
