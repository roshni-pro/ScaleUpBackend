using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public class BlackSoilCreateLeadResponse
    {
        public long id { get; set; }
        public string update_url { get; set; }
        public string number_of_active_years { get; set; }
        public bool sanity_checks_status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string business_id { get; set; }
        public string name { get; set; }
        public string business_type { get; set; }
        public string product_type { get; set; }
        public string website { get; set; }
        public string date_of_incorporation { get; set; }
        public string doc_id { get; set; }
        public string status { get; set; }
        public bool is_deleted { get; set; }
        public bool is_duplicate { get; set; }
        public string referral_link { get; set; }
        public string is_new_to_distributor { get; set; }
        public string component_order { get; set; }
        public string close_line_reasons { get; set; }
        public string loan_status { get; set; }
        public string reopen_comment { get; set; }
        public string business_detail { get; set; }
        public bool is_msme_registered { get; set; }
        public bool is_cibil_check_enabled { get; set; }
        public string los_failure_reason { get; set; }
        public string business_vertical { get; set; }
        public string dehaat_center { get; set; }
        public string business_instance { get; set; }
        public Pan pan { get; set; }
        public Aadhaar aadhaar { get; set; }
        public PersonAddress person_address { get; set; }
        public BusinessAddress business_address { get; set; }
        public Person person { get; set; }
    }

    public class Aadhaar
    {
        public long id { get; set; }
        public string update_url { get; set; }
        public string masked_doc_number { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string type { get; set; }
        public string doc_id { get; set; }
        public string doc_name { get; set; }
        public string doc_type { get; set; }
        public string doc_number { get; set; }
        public string file { get; set; }
        public string lat { get; set; }
        public string @long { get; set; }
        public string cibil_pinged_at { get; set; }
        public long business { get; set; }
        public string application { get; set; }
        public long person { get; set; }
    }

    public class BusinessAddress
    {
        public long id { get; set; }
        public string update_url { get; set; }
        public string full_address { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string address_type { get; set; }
        public string address_name { get; set; }
        public string ownership { get; set; }
        public string address_line { get; set; }
        public string locality { get; set; }
        public string landmark { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string country { get; set; }
        public string lat { get; set; }
        public string @long { get; set; }
        public string doc_id { get; set; }
        public string doc_name { get; set; }
        public string doc_type { get; set; }
        public string file { get; set; }
        public bool is_primary { get; set; }
        public bool is_same_as_registered { get; set; }
        public bool is_same_as_permanent { get; set; }
        public string residing_since { get; set; }
        public bool is_current_residence_address { get; set; }
        public long business { get; set; }
        public string application { get; set; }
        public long person { get; set; }
    }

    public class Pan
    {
        public long id { get; set; }
        public string update_url { get; set; }
        public string masked_doc_number { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string type { get; set; }
        public string doc_id { get; set; }
        public string doc_name { get; set; }
        public string doc_type { get; set; }
        public string doc_number { get; set; }
        public string file { get; set; }
        public string lat { get; set; }
        public string @long { get; set; }
        public string cibil_pinged_at { get; set; }
        public long business { get; set; }
        public string application { get; set; }
        public long person { get; set; }
    }

    public class Person
    {
        public long id { get; set; }
        public string update_url { get; set; }
        public string full_name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string person_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string marital_status { get; set; }
        public string education_qualification { get; set; }
        public string business_experience { get; set; }
        public string business_experience_comment { get; set; }
        public string decentro_ckyc_id { get; set; }
        public bool is_active { get; set; }
    }

    public class PersonAddress
    {
        public long id { get; set; }
        public string update_url { get; set; }
        public string full_address { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string address_type { get; set; }
        public string address_name { get; set; }
        public string ownership { get; set; }
        public string address_line { get; set; }
        public string locality { get; set; }
        public string landmark { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string country { get; set; }
        public string lat { get; set; }
        public string @long { get; set; }
        public string doc_id { get; set; }
        public string doc_name { get; set; }
        public string doc_type { get; set; }
        public string file { get; set; }
        public bool is_primary { get; set; }
        public bool is_same_as_registered { get; set; }
        public bool is_same_as_permanent { get; set; }
        public string residing_since { get; set; }
        public bool is_current_residence_address { get; set; }
        public long business { get; set; }
        public string application { get; set; }
        public long person { get; set; }
    }
}
