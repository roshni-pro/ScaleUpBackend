using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class ArthMateLoanDataResDc
    {
        [DataMember(Order = 1)]
        public LoanDc Loan { get; set; }
        [DataMember(Order = 2)]
        public Leaddc leaddc { get; set; }
        [DataMember(Order = 3)]
        public ArthmateDisbursementdc arthmateDisbursementdc { get; set; }

    }
    [DataContract]
    public class Leaddc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string MobileNo { get; set; }
        [DataMember(Order = 3)]
        public string LeadCode { get; set; }
        [DataMember(Order = 4)]
        public string UserName { get; set; }
        [DataMember(Order = 5)]
        public long ProductId { get; set; }
        [DataMember(Order = 6)]
        public long? OfferCompanyId { get; set; }
        [DataMember(Order = 7)]
        public double? CreditLimit { get; set; }
        [DataMember(Order = 8)]
        public DateTime? AgreementDate { get; set; }
        [DataMember(Order = 9)]
        public DateTime? ApplicationDate { get; set; }
        [DataMember(Order = 10)]
        public List<LeadCompany>? LeadCompanies { get; set; }
        [DataMember(Order = 11)]
        public string? CustomerImage { get; set; }
        [DataMember(Order = 12)]
        public string? ShopName { get; set; }
        [DataMember(Order = 13)]
        public string? CustomerCurrentCityName { get; set; }
        [DataMember(Order = 14)]
        public string? CustomerName { get; set; }
        [DataMember(Order = 15)]
        public bool IsDefaultNBFC { get; set; }
        [DataMember(Order = 16)]
        public string? CityName { get; set; }
        [DataMember(Order = 17)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 18)]
        public string? ProductType { get; set; }
        [DataMember(Order = 19)]
        public string NBFCCompanyCode { get; set; }
        [DataMember(Order = 20)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 21)]
        public string? LeadCreatedUserId { get; set; }
        [DataMember(Order = 22)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 23)]
        public string? CreatedBy { get; set; }


    }
    [DataContract]
    public class LoanDc
    {
        [DataMember(Order = 1)]
        public long? LeadMasterId { get; set; }
        [DataMember(Order = 2)]
        public long? RequestId { get; set; }

        public long? ReponseId { get; set; }
        [DataMember(Order = 3)]
        public bool? IsSuccess { get; set; }
        [DataMember(Order = 4)]
        public string Message { get; set; }
        [DataMember(Order = 5)]
        public int? co_lender_assignment_id { get; set; }

        [DataMember(Order = 6)]
        public string loan_app_id { get; set; }
        [DataMember(Order = 7)]
        public string loan_id { get; set; }
        [DataMember(Order = 8)]
        public string borrower_id { get; set; }
        [DataMember(Order = 9)]
        public string partner_loan_app_id { get; set; }
        [DataMember(Order = 10)]
        public string partner_loan_id { get; set; }
        [DataMember(Order = 11)]
        public string partner_borrower_id { get; set; }
        [DataMember(Order = 12)]
        public int? company_id { get; set; }
        [DataMember(Order = 13)]
        public int? co_lender_id { get; set; }
        [DataMember(Order = 14)]
        public string co_lend_flag { get; set; }
        [DataMember(Order = 15)]
        public string product_id { get; set; }
        [DataMember(Order = 16)]
        public string itr_ack_no { get; set; }
        [DataMember(Order = 17)]
        public string loan_app_date { get; set; }
        [DataMember(Order = 18)]
        public int? penal_interest { get; set; }
        [DataMember(Order = 19)]
        public int? bounce_charges { get; set; }
        [DataMember(Order = 20)]
        public double? sanction_amount { get; set; }
        [DataMember(Order = 21)]
        public double? gst_on_pf_amt { get; set; }
        [DataMember(Order = 22)]
        public string gst_on_pf_perc { get; set; }
        [DataMember(Order = 23)]
        public string repayment_type { get; set; }
        [DataMember(Order = 24)]
        public DateTime? first_inst_date { get; set; }
        [DataMember(Order = 25)]
        public double? net_disbur_amt { get; set; }
        [DataMember(Order = 26)]
        public DateTime? final_approve_date { get; set; }
        [DataMember(Order = 27)]
        public string final_remarks { get; set; }
        [DataMember(Order = 28)]
        public string foir { get; set; }
        [DataMember(Order = 29)]
        public string status { get; set; }
        [DataMember(Order = 30)]
        public int? stage { get; set; }
        [DataMember(Order = 31)]
        public string upfront_interest { get; set; }
        [DataMember(Order = 32)]
        public string exclude_interest_till_grace_period { get; set; }
        [DataMember(Order = 33)]
        public string customer_type_ntc { get; set; }
        [DataMember(Order = 34)]
        public string borro_bank_account_type { get; set; }
        [DataMember(Order = 35)]
        public string borro_bank_account_holder_name { get; set; }
        [DataMember(Order = 36)]
        public string business_vintage_overall { get; set; }
        [DataMember(Order = 37)]
        public string gst_number { get; set; }
        [DataMember(Order = 38)]
        public string abb { get; set; }
        [DataMember(Order = 39)]
        public double? loan_int_amt { get; set; }
        [DataMember(Order = 40)]
        public string loan_int_rate { get; set; }
        [DataMember(Order = 41)]
        public double? conv_fees { get; set; }
        [DataMember(Order = 42)]
        public double? processing_fees_amt { get; set; }
        [DataMember(Order = 43)]
        public double? processing_fees_perc { get; set; }
        [DataMember(Order = 44)]
        public string tenure { get; set; }
        [DataMember(Order = 45)]
        public string tenure_type { get; set; }
        [DataMember(Order = 46)]
        public string int_type { get; set; }
        [DataMember(Order = 47)]
        public string borro_bank_ifsc { get; set; }
        [DataMember(Order = 48)]
        public string borro_bank_acc_num { get; set; }
        [DataMember(Order = 49)]
        public string borro_bank_name { get; set; }
        [DataMember(Order = 50)]
        public string first_name { get; set; }
        [DataMember(Order = 51)]
        public string last_name { get; set; }
        [DataMember(Order = 52)]
        public string ninety_plus_dpd_in_last_24_months { get; set; }
        [DataMember(Order = 53)]
        public int? current_overdue_value { get; set; }
        [DataMember(Order = 54)]
        public string dpd_in_last_9_months { get; set; }
        [DataMember(Order = 55)]
        public string dpd_in_last_3_months { get; set; }
        [DataMember(Order = 56)]
        public string dpd_in_last_6_months { get; set; }
        [DataMember(Order = 57)]
        public string bureau_score { get; set; }
        [DataMember(Order = 58)]
        public string monthly_income { get; set; }
        [DataMember(Order = 59)]
        public string bounces_in_three_month { get; set; }
        [DataMember(Order = 60)]
        public double? loan_amount_requested { get; set; }
        [DataMember(Order = 61)]
        public string insurance_company { get; set; }
        [DataMember(Order = 62)]
        public double? credit_card_settlement_amount { get; set; }
        [DataMember(Order = 63)]
        public double? emi_amount { get; set; }
        [DataMember(Order = 64)]
        public string emi_allowed { get; set; }
        [DataMember(Order = 65)]
        public string bene_bank_name { get; set; }
        [DataMember(Order = 66)]
        public string bene_bank_acc_num { get; set; }
        [DataMember(Order = 67)]
        public string bene_bank_ifsc { get; set; }
        [DataMember(Order = 68)]
        public string bene_bank_account_holder_name { get; set; }
        [DataMember(Order = 69)]
        public double? igst_amount { get; set; }
        [DataMember(Order = 70)]
        public double? cgst_amount { get; set; }
        [DataMember(Order = 71)]
        public double? sgst_amount { get; set; }
        [DataMember(Order = 72)]
        public int? emi_count { get; set; }
        [DataMember(Order = 73)]
        public double? broken_interest { get; set; }
        [DataMember(Order = 74)]
        public int? dpd_in_last_12_months { get; set; }
        [DataMember(Order = 75)]
        public int? dpd_in_last_3_months_credit_card { get; set; }
        [DataMember(Order = 76)]
        public int? dpd_in_last_3_months_unsecured { get; set; }
        [DataMember(Order = 77)]
        public double? broken_period_int_amt { get; set; }
        [DataMember(Order = 78)]
        public int? dpd_in_last_24_months { get; set; }
        [DataMember(Order = 79)]
        public int? avg_banking_turnover_6_months { get; set; }
        [DataMember(Order = 80)]
        public int? enquiries_bureau_30_days { get; set; }
        [DataMember(Order = 81)]
        public int? cnt_active_unsecured_loans { get; set; }
        [DataMember(Order = 82)]
        public int? total_overdues_in_cc { get; set; }
        [DataMember(Order = 83)]
        public double? insurance_amount { get; set; }
        [DataMember(Order = 84)]
        public double? bureau_outstanding_loan_amt { get; set; }
        [DataMember(Order = 85)]
        public double? subvention_fees_amount { get; set; }
        [DataMember(Order = 86)]
        public string gst_on_subvention_fees { get; set; }
        [DataMember(Order = 87)]
        public string cgst_on_subvention_fees { get; set; }
        [DataMember(Order = 88)]
        public string sgst_on_subvention_fees { get; set; }
        [DataMember(Order = 89)]
        public string igst_on_subvention_fees { get; set; }
        [DataMember(Order = 90)]
        public string purpose_of_loan { get; set; }
        [DataMember(Order = 91)]
        public string business_name { get; set; }
        [DataMember(Order = 92)]
        public string co_app_or_guar_name { get; set; }
        [DataMember(Order = 93)]
        public string co_app_or_guar_address { get; set; }
        [DataMember(Order = 94)]
        public string co_app_or_guar_mobile_no { get; set; }
        [DataMember(Order = 95)]
        public string co_app_or_guar_pan { get; set; }
        [DataMember(Order = 96)]
        public string co_app_or_guar_bureau_score { get; set; }
        [DataMember(Order = 97)]
        public string business_address { get; set; }
        [DataMember(Order = 98)]
        public string business_state { get; set; }
        [DataMember(Order = 99)]
        public string business_city { get; set; }
        [DataMember(Order = 100)]
        public string business_pin_code { get; set; }
        [DataMember(Order = 101)]
        public string business_address_ownership { get; set; }
        [DataMember(Order = 102)]
        public string business_pan { get; set; }
        [DataMember(Order = 103)]
        public DateTime? bureau_fetch_date { get; set; }
        [DataMember(Order = 104)]
        public int? enquiries_in_last_3_months { get; set; }
        [DataMember(Order = 105)]
        public double? gst_on_conv_fees { get; set; } //05/03/2024
        [DataMember(Order = 106)]
        public double? cgst_on_conv_fees { get; set; }//05/03/2024
        [DataMember(Order = 107)]
        public double? sgst_on_conv_fees { get; set; }//05/03/2024
        [DataMember(Order = 108)]
        public double? igst_on_conv_fees { get; set; }//05/03/2024
        [DataMember(Order = 109)]
        public string gst_on_application_fees { get; set; }
        [DataMember(Order = 110)]
        public string cgst_on_application_fees { get; set; }
        [DataMember(Order = 111)]
        public string sgst_on_application_fees { get; set; }
        [DataMember(Order = 112)]
        public string igst_on_application_fees { get; set; }
        [DataMember(Order = 113)]
        public string interest_type { get; set; }
        [DataMember(Order = 114)]
        public double? conv_fees_excluding_gst { get; set; } //05/03/2024
        [DataMember(Order = 115)]
        public string application_fees_excluding_gst { get; set; }
        [DataMember(Order = 116)]
        public string emi_obligation { get; set; }
        [DataMember(Order = 117)]
        public string a_score_request_id { get; set; }
        [DataMember(Order = 118)]
        public int? a_score { get; set; }
        [DataMember(Order = 119)]
        public int? b_score { get; set; }
        [DataMember(Order = 120)]
        public double? offered_amount { get; set; }
        [DataMember(Order = 121)]
        public double offered_int_rate { get; set; }
        [DataMember(Order = 122)]
        public double monthly_average_balance { get; set; }
        [DataMember(Order = 123)]
        public double monthly_imputed_income { get; set; }
        [DataMember(Order = 124)]
        public string party_type { get; set; }
        [DataMember(Order = 125)]
        public DateTime? co_app_or_guar_dob { get; set; }
        [DataMember(Order = 126)]
        public string co_app_or_guar_gender { get; set; }
        [DataMember(Order = 127)]
        public string co_app_or_guar_ntc { get; set; }
        [DataMember(Order = 128)]
        public string udyam_reg_no { get; set; }
        [DataMember(Order = 129)]
        public string program_type { get; set; }
        [DataMember(Order = 130)]
        public int? written_off_settled { get; set; }
        [DataMember(Order = 131)]

        public string upi_handle { get; set; }
        [DataMember(Order = 132)]
        public string upi_reference { get; set; }
        [DataMember(Order = 133)]
        public int? fc_offer_days { get; set; }
        [DataMember(Order = 134)]
        public string foreclosure_charge { get; set; }
        [DataMember(Order = 135)]
        public double? eligible_loan_amount { get; set; }
        [DataMember(Order = 136)]
        public DateTime? created_at { get; set; }
        [DataMember(Order = 137)]
        public DateTime? updated_at { get; set; }
        [DataMember(Order = 138)]
        public int? v { get; set; }
        [DataMember(Order = 139)]
        public string UrlSlaDocument { get; set; }
        [DataMember(Order = 140)]
        public string UrlSlaUploadSignedDocument { get; set; }
        [DataMember(Order = 141)]
        public bool? IsUpload { get; set; }
        [DataMember(Order = 142)]
        public string UrlSlaUploadDocument_id { get; set; }
        [DataMember(Order = 143)]
        public string UMRN { get; set; }
        [DataMember(Order = 144)]
        public bool IsActive { get; set; }
        [DataMember(Order = 145)]
        public bool IsDeleted { get; set; }
        [DataMember(Order = 146)]
        public double? PlatFormFee { get; set; }
    }

    [DataContract]
    public class ArthmateDisbursementdc
    {
        [DataMember(Order = 1)]
        public long Id { get; set; }
        [DataMember(Order = 2)]
        public DateTime CreatedDate { get; set; }
        [DataMember(Order = 3)]
        public string loan_id { get; set; }
        [DataMember(Order = 4)]
        public string partner_loan_id { get; set; }
        [DataMember(Order = 5)]
        public string status_code { get; set; }
        [DataMember(Order = 6)]
        public double net_disbur_amt { get; set; }
        [DataMember(Order = 7)]
        public string utr_number { get; set; }
        [DataMember(Order = 8)]
        public string utr_date_time { get; set; }
    }

}
