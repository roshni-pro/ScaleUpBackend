using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.ArthMate
{
    [DataContract]
    public class AScoreWebhookDc
    {
        [DataMember(Order = 1)]
        public string request_id { get; set; }
        [DataMember(Order = 2)]
        public string status { get; set; }
    }
    [DataContract]
    public class CompositeDisbursementWebhookDc
    {
        [DataMember(Order = 1)]
        public string event_key { get; set; }
        [DataMember(Order = 2)]
        //[JsonProperty("data")]
        public CallBackData data { get; set; }
    }
    [DataContract]
    public class CallBackData
    {
        [DataMember(Order = 1)]
        public string status_code { get; set; }
        [DataMember(Order = 2)]
        public string loan_id { get; set; }
        [DataMember(Order = 3)]
        public string partner_loan_id { get; set; }
        [DataMember(Order = 4)]
        public double net_disbur_amt { get; set; }
        [DataMember(Order = 5)]
        public string utr_number { get; set; }
        [DataMember(Order = 6)]
        public string utr_date_time { get; set; }
        [DataMember(Order = 7)]
        public string txn_id { get; set; }


    }

    [DataContract]
    public class ArthmatDisbursementWebhookRequest
    {

        [DataMember(Order = 1)]
        public string Data { get; set; }
    }


}
