using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response
{
    public class LeadResponseDc
    {
        //public bool success { get; set; }
        //public string message { get; set; }
        //public string errorCode { get; set; }
        public ArthmateLoanDc data { get; set; }
        //public List<LeadDatum> LeadDatum { get; set; }
    }
    public class ArthmateLoanDc
    {
        //public List<ExactErrorRow> exactErrorRows { get; set; }
        //public List<ErrorRow> errorRows { get; set; }
        public List<PreparedbiTmpl> preparedbiTmpl { get; set; }
    }
    //public class ExactErrorRow
    //{
    //    public string doi { get; set; }
    //    public string bus_pan { get; set; }
    //    public string pincode { get; set; }
    //    public string per_pincode { get; set; }
    //    public string appl_phone { get; set; }
    //    public string appl_pan { get; set; }
    //    public string email_id { get; set; }
    //    public string aadhar_card_num { get; set; }
    //    public string dob { get; set; }
    //    public string age { get; set; }
    //}
    //public class ErrorRow
    //{
    //    public string partner_loan_app_id { get; set; }
    //    public string partner_borrower_id { get; set; }
    //    public string bus_name { get; set; }
    //    public string doi { get; set; }
    //    public string bus_entity_type { get; set; }
    //    public string bus_pan { get; set; }
    //    public string bus_add_corr_line1 { get; set; }
    //    public string bus_add_corr_line2 { get; set; }
    //    public string bus_add_corr_city { get; set; }
    //    public string bus_add_corr_state { get; set; }
    //    public string bus_add_corr_pincode { get; set; }
    //    public string bus_add_per_line1 { get; set; }
    //    public string bus_add_per_line2 { get; set; }
    //    public string bus_add_per_city { get; set; }
    //    public string bus_add_per_state { get; set; }
    //    public string bus_add_per_pincode { get; set; }
    //    public string first_name { get; set; }
    //    public string last_name { get; set; }
    //    public string father_fname { get; set; }
    //    public string father_lname { get; set; }
    //    public string type_of_addr { get; set; }
    //    public string resi_addr_ln1 { get; set; }
    //    public string resi_addr_ln2 { get; set; }
    //    public string city { get; set; }
    //    public string state { get; set; }
    //    public string pincode { get; set; }
    //    public string per_addr_ln1 { get; set; }
    //    public string per_addr_ln2 { get; set; }
    //    public string per_city { get; set; }
    //    public string per_state { get; set; }
    //    public string per_pincode { get; set; }
    //    public string appl_phone { get; set; }
    //    public string appl_pan { get; set; }
    //    public string email_id { get; set; }
    //    public string aadhar_card_num { get; set; }
    //    public string dob { get; set; }
    //    public string gender { get; set; }
    //    public string age { get; set; }
    //    public string residence_status { get; set; }
    //    public string bureau_pull_consent { get; set; }
    //}
    public class PreparedbiTmpl
    {
        //public string partner_loan_app_id { get; set; }
        //public string partner_borrower_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
    }
    //public class LeadDatum
    //{
    //    public int? address_same { get; set; }
    //    public List<string> borrowers_id { get; set; }
    //    public List<string> guarantors_id { get; set; }
    //    public int? _id { get; set; }
    //    public int? product_id { get; set; }
    //    public int? company_id { get; set; }
    //    public int? loan_schema_id { get; set; }
    //    public string loan_app_id { get; set; }
    //    public string borrower_id { get; set; }
    //    public string partner_loan_app_id { get; set; }
    //    public string partner_borrower_id { get; set; }
    //    public string first_name { get; set; }
    //    public string last_name { get; set; }
    //    public string type_of_addr { get; set; }
    //    public string resi_addr_ln1 { get; set; }
    //    public string resi_addr_ln2 { get; set; }
    //    public string city { get; set; }
    //    public string state { get; set; }
    //    public int? pincode { get; set; }
    //    public string per_addr_ln1 { get; set; }
    //    public string per_addr_ln2 { get; set; }
    //    public string per_city { get; set; }
    //    public string per_state { get; set; }
    //    public int? per_pincode { get; set; }
    //    public string appl_phone { get; set; }
    //    public string appl_pan { get; set; }
    //    public string email_id { get; set; }
    //    public string aadhar_card_num { get; set; }
    //    public string dob { get; set; }
    //    public string gender { get; set; }
    //    public string addr_id_num { get; set; }
    //    public string age { get; set; }
    //    public string lead_status { get; set; }
    //    public string residence_status { get; set; }
    //    public string cust_id { get; set; }
    //    public string loan_status { get; set; }
    //    public string status { get; set; }
    //    public int? is_deleted { get; set; }
    //    public string bureau_pull_consent { get; set; }
    //    public string aadhaar_fname { get; set; }
    //    public string aadhaar_lname { get; set; }
    //    public string aadhaar_dob { get; set; }
    //    public string aadhaar_pincode { get; set; }
    //    public string parsed_aadhaar_number { get; set; }
    //    public string pan_fname { get; set; }
    //    public string pan_lname { get; set; }
    //    public string pan_dob { get; set; }
    //    public string pan_father_fname { get; set; }
    //    public string pan_father_lname { get; set; }
    //    public string parsed_pan_number { get; set; }
    //    public string father_fname { get; set; }
    //    public string father_lname { get; set; }
    //    public object urc_parsing_data { get; set; }
    //    public object urc_parsing_status { get; set; }
    //    public string bus_add_corr_line1 { get; set; }
    //    public string bus_add_corr_line2 { get; set; }
    //    public string bus_add_corr_city { get; set; }
    //    public string bus_add_corr_state { get; set; }
    //    public string bus_add_corr_pincode { get; set; }
    //    public string bus_add_per_line1 { get; set; }
    //    public string bus_add_per_line2 { get; set; }
    //    public string bus_add_per_city { get; set; }
    //    public string bus_pan { get; set; }
    //    public string bus_add_per_state { get; set; }
    //    public string bus_add_per_pincode { get; set; }
    //    public string bus_name { get; set; }
    //    public DateTime doi { get; set; }
    //    public string bus_entity_type { get; set; }
    //    public List<string> coborrower { get; set; }
    //    public List<string> guarantor { get; set; }
    //    public DateTime created_at { get; set; }
    //    public List<string> additional_docs { get; set; }
    //    public DateTime updated_at { get; set; }
    //    public int? __v { get; set; }
    //    public string scr_match_count { get; set; }
    //    public string scr_match_result { get; set; }
    //}
    public class CeplrBasicReportResponse
    {
        //public int code { get; set; }
        public List<Datum> data { get; set; }
    }
    public class Datum
    {
        //public string name { get; set; }
        //public string mobile { get; set; }
        //public string email { get; set; }
        //public object dob { get; set; }
        //public object address { get; set; }
        //public object pan { get; set; }
        //public string masked_account_number { get; set; }
        //public object account_type { get; set; }
        //public object holding_type { get; set; }
        //public string current_balance { get; set; }
        public Analytics analytics { get; set; }
    }
    public class Analytics
    {
        //public int transaction_count { get; set; }
        //public string start_date { get; set; }
        //public string end_date { get; set; }
        //public string avg_daily_closing_balance { get; set; }
        //public string avg_monthly_debits { get; set; }
        //public string avg_monthly_credits { get; set; }
        //public CreditOverallSummary credit_overall_summary { get; set; }
        //public OutwardBouncesSummary outward_bounces_summary { get; set; }
        public SalarySummary salary_summary { get; set; }
        //public BalanceSummary balance_summary { get; set; }
        //public CashWithdrawalsSummary cash_withdrawals_summary { get; set; }
        //public CashDepositSummary cash_deposit_summary { get; set; }
        //public DebitsSummary debits_summary { get; set; }
        //public RegularDebitsSummary regular_debits_summary { get; set; }
        //public DebitToCreditRatio debit_to_credit_ratio { get; set; }
        //public AvgMonthlySurplus avg_monthly_surplus { get; set; }
        //public CountOfNegativeIncedent count_of_negative_incedent { get; set; }
    }

    public class AvgMonthlySurplus
    {
        public string total_surplus { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class BalanceSummary
    {
        public string average_balance { get; set; }
        public string last_balance { get; set; }
        public string max_balance { get; set; }
        public string min_balance { get; set; }
        public string opening_balance { get; set; }
        public string average_balance_at_1st { get; set; }
        public string average_balance_at_5th { get; set; }
        public string average_balance_at_15th { get; set; }
        public string average_balance_at_25th { get; set; }
        public string average_balance_at_30th { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class CashDepositSummary
    {
        public int deposit_count { get; set; }
        public string total_deposit { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class CashWithdrawalsSummary
    {
        public int cash_withdrawals_count { get; set; }
        public string total_cash_withdrawal { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class CountOfNegativeIncedent
    {
        public int total_negative_incedent_count { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class CreditOverallSummary
    {
        public int cash_credits_count { get; set; }
        public string credit_median { get; set; }
        public string total_credits { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }


    public class DebitsSummary
    {
        public int total_debits_count { get; set; }
        public string total_debits { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class DebitToCreditRatio
    {
        public string total_debit_to_credit_ratio { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class MonthlyAnalysis
    {
        public int cash_credit_counts { get; set; }
        public object total_credit { get; set; }
        public object monthly_credit_median { get; set; }
        public int total_negative_incedent_count { get; set; }
        public int outward_bounces_count { get; set; }
        public int outward_bounce_amount { get; set; }
        public object month_average { get; set; }
        public object balance_on_1st { get; set; }
        public object balance_on_5th { get; set; }
        public object balance_on_15th { get; set; }
        public object balance_on_25th { get; set; }
        public object balance_on_30th { get; set; }
        public int cash_withdrawals_count { get; set; }
        public object total_cash_withdrawal { get; set; }
        public int deposit_count { get; set; }
        public string total_deposit { get; set; }
        public int total_debits_count { get; set; }
        public object total_debits { get; set; }
        public int regular_debits_count { get; set; }
        public object total_regular_debits { get; set; }
        public string total_debit_to_credit_ratio { get; set; }
        public string total_surplus { get; set; }
    }

    public class OutwardBouncesSummary
    {
        public int outward_bounces_count { get; set; }
        public int outward_bounce_amount { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }

    public class RegularDebitsSummary
    {
        public int regular_debits_count { get; set; }
        public string total_regular_debits { get; set; }
        public List<MonthlyAnalysis> monthly_analysis { get; set; }
    }



    public class SalarySummary
    {
        //public int salary_flag { get; set; }
        //public List<object> salary_dates { get; set; }
        //public string stable_monthly_inflow { get; set; }
        public string total_salary { get; set; }
    }


    //Retry Api Dc
    public class RetryApiDc
    {
        public long LeadMasterId { get; set; }
        //public string CustomerUid { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string ApiName { get; set; }
    }
    public class GetLeadResponse
    {
        public int _id { get; set; }
        public int product_id { get; set; }
        public int company_id { get; set; }
        public int loan_schema_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string resi_addr_ln1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public int pincode { get; set; }
        public string appl_phone { get; set; }
        public string appl_pan { get; set; }
        public string aadhar_card_num { get; set; }
        public string dob { get; set; }
        public string addr_id_num { get; set; }
        public DateTime created_at { get; set; }
        public int __v { get; set; }
        public string loan_id { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }
}
