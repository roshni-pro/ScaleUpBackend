using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil
{

    public class BlackSoilRepaymentList
    {
        public int count { get; set; }
        public string next { get; set; }
        public string previous { get; set; }
        public List<BlackSoilRepaymentListResult> results { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class BlackSoilRepaymentListResult
    {
        public int id { get; set; }
        public string update_url { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string doc_id { get; set; }
        public string txn_id { get; set; }
        public string type { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        //public object lender_settlement_bank { get; set; }
        public string component_order { get; set; }
        public string settlement_type { get; set; }
        public string payment_mode { get; set; }
        public DateTime settlement_date { get; set; }
        public string actual_amount { get; set; }
        public string amount { get; set; }
        public string interest { get; set; }
        public string pf { get; set; }
        public string penal_interest { get; set; }
        public string overdue_interest { get; set; }
        public string principal { get; set; }
        public string extra_payment { get; set; }
        public bool is_decentro_already_processed { get; set; }
        public string amount_paid_to_distributor { get; set; }
        public string amount_paid_to_blacksoil { get; set; }
        //public object decentro_txn_id_distributor { get; set; }
        //public object decentro_txn_id_blacksoil { get; set; }
        //public object decentro_distributor_paid_at { get; set; }
        //public object decentro_blacksoil_paid_at { get; set; }
        //public object decentro_reference_id { get; set; }
        //public object digio_debit_id { get; set; }
        //public object bounce_reason { get; set; }
        //public object ref_file { get; set; }
        public string ref_no { get; set; }
        //public object razorpay_merchant_utr { get; set; }
        //public object razorpay_settlement_utr { get; set; }
        //public object manual_utr { get; set; }
        public bool is_from_bulk_repayment { get; set; }
        //public object refund_razorpay_payout_id { get; set; }
        //public object refund_razorpay_payout_status { get; set; }
        //public object refund_payout_date { get; set; }
        //public object refund_payout_processed_date { get; set; }
        //public object refund_payout_utr { get; set; }
        public bool repayment_is_processing { get; set; }
        public int loan_account { get; set; }
        //public object user { get; set; }
        //public object decentro_settlement { get; set; }
        //public object castler_transaction { get; set; }
        //public object reversal_topup { get; set; }
        //public object pdc { get; set; }
    }

   
}
