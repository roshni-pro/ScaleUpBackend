using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil
{
    public class BlackSoilLoanAccountExpandDc
    {
        public int? id { get; set; }
        public string? update_url { get; set; }
        public double? alert_level_amount { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? loan_account_id { get; set; }
        public string? status { get; set; }
        public string? doc_id { get; set; }
        public int? pool_amount { get; set; }
        public string? nach_debit_amount { get; set; }
        //public object? loan_recall_notice_file { get; set; }
        //public object? cheque_legal_notice_file { get; set; }
        //public object? nach_legal_notice_file { get; set; }
        public bool? do_not_refresh { get; set; }
        public int? dehaat_farmer_due_date { get; set; }
        public BlackSoilLoanAccountExpandBusiness business { get; set; }
        public object? account_manager { get; set; }
        public BlackSoilLoanAccountExpandExtra extra { get; set; }
        public List<BlackSoilLoanAccountTopup> topups { get; set; }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class BlackSoilLoanAccountExpandBusiness
    {
        public int? id { get; set; }
        public string? update_url { get; set; }
        public string? number_of_active_years { get; set; }
        public bool? sanity_checks_status { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? business_id { get; set; }
        public string? name { get; set; }
        public string? business_type { get; set; }
        public string? product_type { get; set; }
        //public object? website { get; set; }
        //public object? date_of_incorporation { get; set; }
        public string? doc_id { get; set; }
        public string? status { get; set; }
        public bool? is_deleted { get; set; }
        public bool? is_duplicate { get; set; }
        //public object? referral_link { get; set; }
        public string? is_new_to_distributor { get; set; }
        public string? component_order { get; set; }
        //public object? close_line_reasons { get; set; }
        public string? loan_status { get; set; }
        //public object? reopen_comment { get; set; }
        public string? business_detail { get; set; }
        public bool? is_msme_registered { get; set; }
        public bool? is_cibil_check_enabled { get; set; }
        public object? los_failure_reason { get; set; }
        public int? business_vertical { get; set; }
        //public object? dehaat_center { get; set; }
        //public object? business_instance { get; set; }
    }

    public class BlackSoilLoanAccountExpandExtra
    {
        public int? id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? total_outstanding { get; set; }
        public string? nach_debit_amount { get; set; }
        public string? principal_outstanding { get; set; }
        public string? principal_paid { get; set; }
        public string? interest_outstanding { get; set; }
        public string? interest_paid { get; set; }
        public string? penal_interest_outstanding { get; set; }
        public string? penal_interest_paid { get; set; }
        public string? overdue_interest_outstanding { get; set; }
        public string? overdue_interest_paid { get; set; }
        public string? total_repayment { get; set; }
        public string? total_sanctioned_amount { get; set; }
        public string? total_subvented_amount { get; set; }
        public string? extra_payment { get; set; }
        public string? pf_paid { get; set; }
        public string? pf_outstanding { get; set; }
        public int? dpd { get; set; }
        public int? loan_account { get; set; }
        public string? subvented_amount { get; set; }
        public string? interest_outstanding_till_last_payment { get; set; }
        public string? penal_interest_outstanding_till_last_payment { get; set; }
        public string? overdue_interest_outstanding_till_last_payment { get; set; }
        public int? topup { get; set; }
    }

    public class BlackSoilLoanAccountExpandFile
    {
        public int? id { get; set; }
        public string? update_url { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? invoice_date { get; set; }
        public DateTime? due_date { get; set; }
        public string? amount { get; set; }
        public string? disbursed_amount { get; set; }
        public string? invoice_number { get; set; }
        public string? is_verified_by_retailer { get; set; }
        public string? file { get; set; }
        public string? doc_id { get; set; }
        public object? issue_comment { get; set; }
        public bool? is_approved_by_distributor { get; set; }
        public BlackSoilLoanAccountExpandInvoiceFileDetails invoice_file_details { get; set; }
        public int? invoice { get; set; }
    }

    public class BlackSoilLoanAccountExpandInvoice
    {
        public int? id { get; set; }
        public string? update_url { get; set; }
        public double? total_invoice_amount { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? invoice_id { get; set; }
        public string? status { get; set; }
        //public object? reject_reason { get; set; }
        //public object? razorpay_payout_id { get; set; }
        //public object? razorpay_payout_status { get; set; }
        public bool? is_delivered { get; set; }
        //public object? razorpay_payout_failure_reason { get; set; }
        //public object? distributor_split_razorpay_payout_id { get; set; }
        //public object? distributor_split_razorpay_payout_status { get; set; }
        //public object? distributor_split_payout_date { get; set; }
        //public object? distributor_split_payout_processed_date { get; set; }
        //public object? distributor_split_payout_utr { get; set; }
        //public object? hold_back_razorpay_payout_id { get; set; }
        //public object? hold_back_razorpay_payout_status { get; set; }
        //public object? hold_back_payout_date { get; set; }
        //public object? hold_back_payout_processed_date { get; set; }
        //public object? hold_back_payout_utr { get; set; }
        //public object? hold_back_razorpay_payout_failure_reason { get; set; }
        //public object? cashback_razorpay_payout_id { get; set; }
        //public object? cashback_razorpay_payout_status { get; set; }
        //public object? cashback_payout_date { get; set; }
        //public object? cashback_payout_processed_date { get; set; }
        //public object? cashback_payout_utr { get; set; }
        //public object? cashback_razorpay_payout_failure_reason { get; set; }
        //public object? disbursement_bank { get; set; }
        //public object? hold_back_bank { get; set; }
        //public object? cashback_bank { get; set; }
        //public object? mail_approval_file { get; set; }
        public string? invoice_disbursal_type { get; set; }
        //public object? invoice_brand_type { get; set; }
        public string? disbursal_type { get; set; }
        public bool is_verified { get; set; }
        public DateTime? approved_at { get; set; }
        public string? uploaded_by { get; set; }
        public string? failure_reason { get; set; }
        public bool? invoice_approve_is_processing { get; set; }
        public bool? invoice_disbursed_is_processing { get; set; }
        public bool? invoice_repayment_is_processing { get; set; }
        public int? application { get; set; }
        public int? loan_data { get; set; }
        public object? bank { get; set; }
        public List<BlackSoilLoanAccountExpandFile> files { get; set; }
    }

    public class BlackSoilLoanAccountExpandInvoiceFileDetails
    {
    }

    public class BlackSoilLoanAccountLoanData
    {
        public int? id { get; set; }
        public string? update_url { get; set; }
        public double? annual_interest_rate { get; set; }
        public double? tenure_in_months { get; set; }
        public double? daily_interest_rate { get; set; }
        public double? current_cashback_amount { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? sanctioned_amount { get; set; }
        public string? disbursement_amount { get; set; }
        public string? monthly_interest_rate { get; set; }
        public string? annual_365_interest_rate { get; set; }
        public string? monthly_penal_interest_rate { get; set; }
        public int? step_1_monthly_penal_interest_rate_days { get; set; }
        public string? step_1_monthly_penal_interest_rate { get; set; }
        public int? tenure_in_days { get; set; }
        public string? total_repayment { get; set; }
        public string? interest_calculation_type { get; set; }
        public string? pre_emi { get; set; }
        public string? emi { get; set; }
        //public object?    emi_start_date { get; set; }
        public int? penal_lag_days { get; set; }
        public bool? is_branded_invoice_enabled { get; set; }
        public int? branded_invoice_due_date { get; set; }
        public bool? is_discounting_allowed { get; set; }
        public string? discounted_amount { get; set; }
        public object? expected_disbursement_date { get; set; }
        public bool? is_subvention_allowed { get; set; }
        public int? subvention_days { get; set; }
        public string? subvented_amount { get; set; }
        public bool? is_hold_back_allowed { get; set; }
        public double? hold_back_percentage { get; set; }
        public string? hold_back_amount { get; set; }
        public bool? is_dehaat_farmer_product_enabled { get; set; }
        public int? dehaat_farmer_due_date { get; set; }
        public double? dehaat_farmer_pf_amount { get; set; }
        public double? upfront_pf_amount { get; set; }
        public bool? credflow_emi_product_enable { get; set; }
        public int? broken_period_days { get; set; }
        public string? broken_period_amount { get; set; }
        public string? platform_fee { get; set; }
        public string? platform_amount { get; set; }
        public int? installment_count { get; set; }
        public bool? is_cashback_allowed { get; set; }
        public double? cashback_fix_deduction_percentage { get; set; }
        public double? cashback_percentage { get; set; }
        public int? invoice_to_disbursement_lag_days { get; set; }
        public string? subvention_per_day { get; set; }
        public string? allowed_cashback_amount { get; set; }
        public string? actual_cashback_amount { get; set; }
    }

    public class BlackSoilLoanAccountTopup
    {
        public int? id { get; set; }
        public string? update_url { get; set; }
        public double? days_onboard { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public string? alert_level { get; set; }
        public bool? is_reversal_allowed { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? topup_id { get; set; }
        public string? status { get; set; }
        public DateTime? disbursement_date { get; set; }
        public object? topup_over_date { get; set; }
        public string? product_type { get; set; }
        public object? utr { get; set; }
        public bool? is_settled { get; set; }
        public object? settled_at { get; set; }
        public double? settlement_amount { get; set; }
        public double? recovery_amount { get; set; }
        public DateTime? subvention_end_date { get; set; }
        public DateTime? interest_start_date { get; set; }
        public DateTime? penal_start_date { get; set; }
        public DateTime? due_date { get; set; }
        //public object? is_settlement_remark { get; set; }
        //public object? recovery_remark { get; set; }
        public int? application { get; set; }
        public BlackSoilLoanAccountExpandInvoice invoice { get; set; }
        public int? loan_account { get; set; }
        //public object? unadjusted_accounts { get; set; }
        public BlackSoilLoanAccountLoanData loan_data { get; set; }
        public BlackSoilLoanAccountExpandExtra extra { get; set; }
    }


}
