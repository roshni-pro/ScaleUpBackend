using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil
{
    public class BlackSoilRepaymentCaptured
    {
        public BlackSoilRepaymentCapturedData data {  get; set; }
    }
    public class BlackSoilRepaymentCapturedData
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? doc_id { get; set; }
        public string? txn_id { get; set; }
        public string? type { get; set; }
        public string? date { get; set; }
        public string? status { get; set; }
        public string? lender_settlement_bank { get; set; }
        public string? settlement_type { get; set; }
        public string? payment_mode { get; set; }
        public string? settlement_date { get; set; }
        public string? actual_amount { get; set; }
        public string? amount { get; set; }
        public string? interest { get; set; }
        public string? pf { get; set; }
        public string? penal_interest { get; set; }
        public string? overdue_interest { get; set; }
        public string? principal { get; set; }
        public string? extra_payment { get; set; }
        public bool is_decentro_already_processed { get; set; }
        public string? amount_paid_to_distributor { get; set; }
        public string? amount_paid_to_blacksoil { get; set; }
        public string? decentro_txn_id_distributor { get; set; }
        public string? decentro_txn_id_blacksoil { get; set; }
        public string? decentro_distributor_paid_at { get; set; }
        public string? decentro_blacksoil_paid_at { get; set; }
        public string? decentro_reference_id { get; set; }
        public string? digio_debit_id { get; set; }
        public string? bounce_reason { get; set; }
        public string? ref_file { get; set; }
        public string? ref_no { get; set; }
        public string? razorpay_merchant_utr { get; set; }
        public string? razorpay_settlement_utr { get; set; }
        public string? manual_utr { get; set; }
        public bool is_from_bulk_repayment { get; set; }
        public string? refund_razorpay_payout_id { get; set; }
        public string? refund_razorpay_payout_status { get; set; }
        public string? refund_payout_date { get; set; }
        public string? refund_payout_processed_date { get; set; }
        public string? refund_payout_utr { get; set; }
        public bool repayment_is_processing { get; set; }
        public long? loan_account { get; set; }
        public string? user { get; set; }
        public string? decentro_settlement { get; set; }
        public string? castler_transaction { get; set; }
        public string? reversal_topup { get; set; }
        public string? pdc { get; set; }
        public BlackSoilRepaymentCapturedIdentifiers identifiers { get; set; }
    }

    public class BlackSoilRepaymentCapturedIdentifiers
    {
        public string? business_id { get; set; }
        public string? partner_lead_id { get; set; }
        public string? mobile { get; set; }
    }
}
