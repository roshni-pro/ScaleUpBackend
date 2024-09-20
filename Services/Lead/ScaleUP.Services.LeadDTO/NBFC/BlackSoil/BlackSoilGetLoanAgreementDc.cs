using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public class BlackSoilGetLoanAgreementDc
    {
        public int count { get; set; }
        public List<Result> results { get; set; }
    }
    public class Result
    {
        public int id { get; set; }
        //public string update_url { get; set; }
        //public DateTime created_at { get; set; }
        //public DateTime updated_at { get; set; }
        //public string nach_amount { get; set; }
        //public string doc_id { get; set; }
        //public string generated_file { get; set; }
        //public DateTime generated_at { get; set; }
        //public string signed_at { get; set; }
        //public string file { get; set; }
        public string doc_name { get; set; }
        //public string stamp_reference_id { get; set; }
        //public string stamp_transaction_id { get; set; }
        //public string stamp_numbers { get; set; }
        public bool is_verified { get; set; }
        public string comment { get; set; }
        public int application { get; set; }
        //public string stamp_document { get; set; }
    }




}
