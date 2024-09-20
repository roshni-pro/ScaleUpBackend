using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate
{
    public class Postrepayment_scheduleDc
    {
        public string loan_id { get; set; }
        public long company_id { get; set; }
        public string product_id { get; set; }
    }

    public class repay_scheduleDc
    {
        public bool error { get; set; }
        public bool success { get; set; }
        public repay_scheduleData data { get; set; }
        public string message { get; set; }


    }
    public class repay_scheduleData
    {
        public List<repay_scheduleDetails> rows { get; set; }
        public int count { get; set; }
    }


    public class repay_scheduleDetails
    {
        //public int _id { get; set; }
        public int repay_schedule_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        //public string loan_id { get; set; }
        public int emi_no { get; set; }
        public DateTime? due_date { get; set; }
        public double emi_amount { get; set; }
        public double prin { get; set; }
        public double int_amount { get; set; }
        //public int __v { get; set; }
        public double principal_bal { get; set; }
        public double principal_outstanding { get; set; }
        //public DateTime? created_at { get; set; }
        //public DateTime? updated_at { get; set; }


    }

    public class LoanRepaymentScheduleDetailDc
    {
        public int repay_schedule_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        public int emi_no { get; set; }
        public DateTime? due_date { get; set; }
        public double emi_amount { get; set; }
        public double prin { get; set; }
        public double int_amount { get; set; }
        public double principal_bal { get; set; }
        public double principal_outstanding { get; set; }
    }

    public class BusinessLoanDetailDc
    {
        public long? LeadMasterId { get; set; }
        public long LoanAccountId { get; set; }
        public string CustomerName { get; set; }
        public string AccountCode { get; set; }
        public string ProductType { get; set; }
        public string MobileNo { get; set; }
        public string ShopName { get; set; }
        public string CityName { get; set; }
        public string NBFCIdentificationCode { get; set; }
        public string ThirdPartyLoanCode { get; set; }
        public int? co_lender_assignment_id { get; set; }
        public string loan_app_id { get; set; }
        public string loan_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public int? company_id { get; set; }
        //public int? co_lender_id { get; set; }
        //public string co_lend_flag { get; set; }
        public string product_id { get; set; }
        //public string itr_ack_no { get; set; }
        public string loan_app_date { get; set; }
        public int? penal_interest { get; set; }
        public int? bounce_charges { get; set; }
        public double? sanction_amount { get; set; }
        public double? gst_on_pf_amt { get; set; }
        public double? gst_on_pf_perc { get; set; }
        //public string repayment_type { get; set; }
        public DateTime? first_inst_date { get; set; }
        public double? net_disbur_amt { get; set; }
        public DateTime? final_approve_date { get; set; }
        //public string final_remarks { get; set; }
        //public string foir { get; set; }
        public string status { get; set; }
        //public int? stage { get; set; }
        //public string upfront_interest { get; set; }
        //public string exclude_interest_till_grace_period { get; set; }
        //public string customer_type_ntc { get; set; }
        public string borro_bank_account_type { get; set; }
        public string borro_bank_account_holder_name { get; set; }
        public string business_vintage_overall { get; set; }
        //public string gst_number { get; set; }
        //public string abb { get; set; }
        public double? loan_int_amt { get; set; }
        public string loan_int_rate { get; set; }
        public double? conv_fees { get; set; }
        public double? processing_fees_amt { get; set; }
        public double? processing_fees_perc { get; set; }
        public string tenure { get; set; }
        //public string tenure_type { get; set; }
        //public string int_type { get; set; }
        public string borro_bank_ifsc { get; set; }
        public string borro_bank_acc_num { get; set; }
        public string borro_bank_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        //public string ninety_plus_dpd_in_last_24_months { get; set; }
        //public int? current_overdue_value { get; set; }
        //public string dpd_in_last_9_months { get; set; }
        //public string dpd_in_last_3_months { get; set; }
        //public string dpd_in_last_6_months { get; set; }
        public string bureau_score { get; set; }
        //public string monthly_income { get; set; }
        //public string bounces_in_three_month { get; set; }
        public double? loan_amount_requested { get; set; }
        //public string insurance_company { get; set; }
        //public double credit_card_settlement_amount { get; set; }
        public double? emi_amount { get; set; }
        //public string emi_allowed { get; set; }
        public string bene_bank_name { get; set; }
        public string bene_bank_acc_num { get; set; }
        public string bene_bank_ifsc { get; set; }
        public string bene_bank_account_holder_name { get; set; }
        //public double? igst_amount { get; set; }
        //public double? cgst_amount { get; set; }
        //public double? sgst_amount { get; set; }
        //public int? emi_count { get; set; }
        public double? broken_interest { get; set; }
        //public int? dpd_in_last_12_months { get; set; }
        //public int? dpd_in_last_3_months_credit_card { get; set; }
        //public int? dpd_in_last_3_months_unsecured { get; set; }
        public double? broken_period_int_amt { get; set; }
        //public int? dpd_in_last_24_months { get; set; }
        //public int? avg_banking_turnover_6_months { get; set; }
        //public int? enquiries_bureau_30_days { get; set; }
        //public int? cnt_active_unsecured_loans { get; set; }
        //public int? total_overdues_in_cc { get; set; }
        public double? insurance_amount { get; set; }
        //public double? bureau_outstanding_loan_amt { get; set; }
        //public string subvention_fees_amount { get; set; }
        //public string gst_on_subvention_fees { get; set; }
        //public string cgst_on_subvention_fees { get; set; }
        //public string sgst_on_subvention_fees { get; set; }
        //public string igst_on_subvention_fees { get; set; }
        //public string purpose_of_loan { get; set; }
        public string business_name { get; set; }
        //public string co_app_or_guar_name { get; set; }
        //public string co_app_or_guar_address { get; set; }
        //public string co_app_or_guar_mobile_no { get; set; }
        //public string co_app_or_guar_pan { get; set; }
        //public string co_app_or_guar_bureau_score { get; set; }
        //public string business_address { get; set; }
        //public string business_state { get; set; }
        //public string business_city { get; set; }
        //public string business_pin_code { get; set; }
        //public string business_address_ownership { get; set; }
        public string business_pan { get; set; }
        //public DateTime? bureau_fetch_date { get; set; }
        //public int? enquiries_in_last_3_months { get; set; }
        //public double? gst_on_conv_fees { get; set; } //05/03/2024
        //public double? cgst_on_conv_fees { get; set; }//05/03/2024
        //public double? sgst_on_conv_fees { get; set; }//05/03/2024
        //public double? igst_on_conv_fees { get; set; }//05/03/2024
        //public string gst_on_application_fees { get; set; }
        //public string cgst_on_application_fees { get; set; }
        //public string sgst_on_application_fees { get; set; }
        //public string igst_on_application_fees { get; set; }
        //public string interest_type { get; set; }
        //public double? conv_fees_excluding_gst { get; set; } //05/03/2024
        //public string application_fees_excluding_gst { get; set; }
        //public string emi_obligation { get; set; }
        public string a_score_request_id { get; set; }
        public int? a_score { get; set; }
        public int? b_score { get; set; }

        //public double? offered_amount { get; set; }
        //public double offered_int_rate { get; set; }
        //public double monthly_average_balance { get; set; }
        //public double monthly_imputed_income { get; set; }
        //public string party_type { get; set; }
        //public DateTime? co_app_or_guar_dob { get; set; }
        //public string co_app_or_guar_gender { get; set; }
        //public string co_app_or_guar_ntc { get; set; }
        //public string udyam_reg_no { get; set; }
        //public string program_type { get; set; }
        //public int? written_off_settled { get; set; }
        //public string upi_handle { get; set; }
        //public string upi_reference { get; set; }
        //public int? fc_offer_days { get; set; }
        //public string foreclosure_charge { get; set; }
        //public int? eligible_loan_amount { get; set; }
        //public DateTime? created_at { get; set; }
        //public DateTime? updated_at { get; set; }
        //public int? v { get; set; }
        //public string UrlSlaDocument { get; set; }
        //public string UrlSlaUploadSignedDocument { get; set; }
        //public bool? IsUpload { get; set; }
        //public string UrlSlaUploadDocument_id { get; set; }
        public string UMRN { get; set; }
        public double? PlatFormFee { get; set; }
        public double? PfDiscount { get; set; }
        public double? InsuranceAmount { get; set; }
        public double? Othercharges { get; set; }
    }

    public class RePaymentScheduleDataDc
    {
        public double sanction_amount { get; set; }
        public double InsurancePremium { get; set; }
        public double MonthlyEMI { get; set; }
        public string borro_bank_acc_num { get; set; }
        public double loan_int_amt { get; set; }
        //newly added
        public double processing_fees_amt { get; set; }
        public double net_disbur_amt { get; set; }
        public string loan_id { get; set; }
        public List<repay_scheduleDetails> rows { get; set; }
        public string CompanyIdentificationCode { get; set; }
        public double? InsuranceAmount { get; set; }
        public double? Othercharges { get; set; }
    }
    public class DisbursedDetailDc
    {
        //loan_id,  sanction_amount, InsurancePremium, MonthlyEMI, borro_bank_acc_num
        public string loan_id { get; set; }
        public double sanction_amount { get; set; }
        public double InsurancePremium { get; set; }
        public double MonthlyEMI { get; set; }
        public string borro_bank_acc_num { get; set; }
        public double loan_int_amt { get; set; }
        public int company_id { get; set; }
        public string product_id { get; set; }
        public double processing_fees_amt { get; set; }
        public double net_disbur_amt { get; set; }
    }


    public class BLInvoiceExcelData
    {
        public long LoanAccountId { get; set; }
        public string LoanId { get; set; }
        //public int? PartnerId { get; set; }
        //public int? ProductId { get; set; }
        public DateTime EmiMonth { get; set; }
        public double RepaymentAmount { get; set; }
        public double PrincipalPaid { get; set; }
        public double InterestPaid { get; set; }
        public double BouncePaid { get; set; }
        public double PenalPaid { get; set; }
        public double LpiPaid { get; set; }
        public double OverduePaid { get; set; }
        public double LoanAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentReqNo { get; set; }
        public bool IsProcess { get; set; }
        public string FileUrl { get; set; }
    }
}
