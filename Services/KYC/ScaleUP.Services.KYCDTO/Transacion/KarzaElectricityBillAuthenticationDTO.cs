using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class KarzaElectricityBillAuthenticationDTO
    {
        public Result result { get; set; }
        public string request_id { get; set; }

        [JsonProperty("status-code")]
        public string StatusCode { get; set; }
        public ClientData clientData { get; set; }
        public string? error {get;set;}
    }


    public class ClientData
    {
        public string caseId { get; set; }
    }

    public class Result
    {
        public string bill_no { get; set; }
        public string bill_due_date { get; set; }
        public string consumer_number { get; set; }
        public string bill_amount { get; set; }
        public string bill_issue_date { get; set; }
        public string consumer_name { get; set; }
        public string mobile_number { get; set; }
        public string amount_payable { get; set; }
        public string total_amount { get; set; }
        public string address { get; set; }
        public string email_address { get; set; }
        public string bill_date { get; set; }
    }


}
