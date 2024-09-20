using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil
{
    public class BlackSoilInvoiceDisbursed
    {
        public BlackSoilInvoiceDisbursedData data {  get; set; }
    }
    public class BlackSoilInvoiceDisbursedData
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public string? total_invoice_amount { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? invoice_id { get; set; }
        public string? status { get; set; }
        public string? reject_reason { get; set; }
        public string? razorpay_payout_id { get; set; }
        public string? razorpay_payout_status { get; set; }
        public bool is_delivered { get; set; }
        public string? razorpay_payout_failure_reason { get; set; }
        public string? distributor_split_razorpay_payout_id { get; set; }
        public string? distributor_split_razorpay_payout_status { get; set; }
        public string? distributor_split_payout_date { get; set; }
        public string? distributor_split_payout_processed_date { get; set; }
        public string? distributor_split_payout_utr { get; set; }
        public string? hold_back_razorpay_payout_id { get; set; }
        public string? hold_back_razorpay_payout_status { get; set; }
        public string? hold_back_payout_date { get; set; }
        public string? hold_back_payout_processed_date { get; set; }
        public string? hold_back_payout_utr { get; set; }
        public string? hold_back_razorpay_payout_failure_reason { get; set; }
        public string? cashback_razorpay_payout_id { get; set; }
        public string? cashback_razorpay_payout_status { get; set; }
        public string? cashback_payout_date { get; set; }
        public string? cashback_payout_processed_date { get; set; }
        public string? cashback_payout_utr { get; set; }
        public string? cashback_razorpay_payout_failure_reason { get; set; }
        public string? disbursement_bank { get; set; }
        public string? hold_back_bank { get; set; }
        public string? cashback_bank { get; set; }
        public string? mail_approval_file { get; set; }
        public string? invoice_disbursal_type { get; set; }
        public string? invoice_brand_type { get; set; }
        public string? disbursal_type { get; set; }
        public bool is_verified { get; set; }
        public string? approved_at { get; set; }
        public string? uploaded_by { get; set; }
        public string? failure_reason { get; set; }
        public bool invoice_approve_is_processing { get; set; }
        public bool invoice_disbursed_is_processing { get; set; }
        public bool invoice_repayment_is_processing { get; set; }
        public long? application { get; set; }
        public BlackSoilInvoiceDisbursedLoanData loan_data { get; set; }
        public BlackSoilInvoiceDisbursedTopup topup { get; set; }
        public BlackSoilInvoiceDisbursedIdentifiers identifiers { get; set; }
    }

    public class BlackSoilInvoiceDisbursedIdentifiers
    {
        public string? business_id { get; set; }
        public string? partner_lead_id { get; set; }
        public string? mobile { get; set; }
        public string? application_id { get; set; }
        public string? invoice_id { get; set; }
    }

    public class BlackSoilInvoiceDisbursedLoanData
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public string? annual_interest_rate { get; set; }
        public double? tenure_in_months { get; set; }
        public string? daily_interest_rate { get; set; }
        public string? current_cashback_amount { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? sanctioned_amount { get; set; }
        public string? disbursement_amount { get; set; }
        public string? monthly_interest_rate { get; set; }
        public string? annual_365_interest_rate { get; set; }
        public string? monthly_penal_interest_rate { get; set; }
        public long? step_1_monthly_penal_interest_rate_days { get; set; }
        public string step_1_monthly_penal_interest_rate { get; set; }
        public long? tenure_in_days { get; set; }
        public string? total_repayment { get; set; }
        public string? interest_calculation_type { get; set; }
        public string? pre_emi { get; set; }
        public string? emi { get; set; }
        public string? emi_start_date { get; set; }
        public long? penal_lag_days { get; set; }
        public bool is_branded_invoice_enabled { get; set; }
        public long? branded_invoice_due_date { get; set; }
        public bool is_discounting_allowed { get; set; }
        public string? discounted_amount { get; set; }
        public string? expected_disbursement_date { get; set; }
        public bool is_subvention_allowed { get; set; }
        public long? subvention_days { get; set; }
        public string subvented_amount { get; set; }
        public bool is_hold_back_allowed { get; set; }
        public long? hold_back_percentage { get; set; }
        public string? hold_back_amount { get; set; }
        public bool is_dehaat_farmer_product_enabled { get; set; }
        public long? dehaat_farmer_due_date { get; set; }
        public long? dehaat_farmer_pf_amount { get; set; }
        public bool is_cashback_allowed { get; set; }
        public long? cashback_fix_deduction_percentage { get; set; }
        public long? cashback_percentage { get; set; }
        public long? invoice_to_disbursement_lag_days { get; set; }
        public string? subvention_per_day { get; set; }
        public string? allowed_cashback_amount { get; set; }
        public string? actual_cashback_amount { get; set; }
    }
    public class BlackSoilInvoiceDisbursedTopup
    {
        public long? id { get; set; }
        public string? update_url { get; set; }
        public string? days_onboard { get; set; }
        public string? start_date { get; set; }
        public string? end_date { get; set; }
        public string? alert_level { get; set; }
        public bool is_reversal_allowed { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? topup_id { get; set; }
        public string? status { get; set; }
        public string? disbursement_date { get; set; }
        public string? topup_over_date { get; set; }
        public string? product_type { get; set; }
        public string? utr { get; set; }
        public bool is_settled { get; set; }
        public string? settled_at { get; set; }
        public long? settlement_amount { get; set; }
        public string? subvention_end_date { get; set; }
        public string? interest_start_date { get; set; }
        public string? penal_start_date { get; set; }
        public string? due_date { get; set; }
        public string? is_settlement_remark { get; set; }
        public long? application { get; set; }
        public long? invoice { get; set; }
        public long? loan_account { get; set; }
        public long? loan_data { get; set; }
    }


}
