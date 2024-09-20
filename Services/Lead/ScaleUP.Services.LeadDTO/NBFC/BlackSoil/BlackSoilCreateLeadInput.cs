using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public class BlackSoilCreateLeadInput
    {
        public string referral_code { get; set; }
        public string mobile { get; set; }
        public string name { get; set; }
        public string business_type { get; set; }
        public string full_name { get; set; }
        public string dob { get; set; }   //yyyy-mm-dd
        public string gender { get; set; } //male female
        public string pan_file { get; set; } //BASE64_ENCODED_STRING_OF_FILE
        public string aadhaar_file { get; set; } //BASE64_ENCODED_STRING_OF_FILE
        public string aadhaar { get; set; } 

        
        public BlackSoilCreateLeadAddressInput business_address { get; set; }
        public BlackSoilCreateLeadAddressInput person_address { get; set; }
    }

    public class BlackSoilCreateLeadAddressInput
    {
        public string address_line { get; set; }
        public string pincode { get; set; }
        public string city { get; set; }
        public string state { get; set; }
    }

}

