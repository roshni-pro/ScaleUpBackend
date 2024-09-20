using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request
{
    public class ArthMateLeadPostDc
    {
        public long Id { get; set; }
        public string SkCode { get; set; }
        public string MobileNo { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string bus_name { get; set; }
        public string doi { get; set; }
        public string bus_entity_type { get; set; }
        public string bus_pan { get; set; }
        public string bus_add_corr_line1 { get; set; }
        //IncomeSlab
        public string IncomeSlab { get; set; }
        public string bus_add_corr_line2 { get; set; }
        public string bus_add_corr_city { get; set; }
        public string bus_add_corr_state { get; set; }
        public string bus_add_corr_pincode { get; set; }
        public string bus_add_per_line1 { get; set; }
        public string bus_add_per_line2 { get; set; }
        public string bus_add_per_city { get; set; }
        public string bus_add_per_state { get; set; }
        public string bus_add_per_pincode { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string father_fname { get; set; }
        public string father_lname { get; set; }
        public string type_of_addr { get; set; }
        public string resi_addr_ln1 { get; set; }
        public string resi_addr_ln2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string per_addr_ln1 { get; set; }
        public string per_addr_ln2 { get; set; }
        public string per_city { get; set; }
        public string per_state { get; set; }
        public string per_pincode { get; set; }
        public string appl_phone { get; set; }
        public string appl_pan { get; set; }
        public string email_id { get; set; }
        public string aadhar_card_num { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string age { get; set; }
        public string residence_status { get; set; }
        public string bureau_pull_consent { get; set; }
        public bool IsLeadGenerate { get; set; }
        public string CompletionStage { get; set; }

        //new added
        public string borro_bank_name { get; set; }
        public string borro_bank_ifsc { get; set; }
        public string borro_bank_acc_num { get; set; }
        public string GSTStatement { get; set; }
        public string BankStatement { get; set; }

        public string marital_status { get; set; }
        public int SequenceNo { get; set; }
        public string qualification { get; set; }
    }

    public class LeadPostdc 
    {
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string bus_name { get; set; }
        public string doi { get; set; }
        public string bus_entity_type { get; set; }
        public string bus_pan { get; set; }
        public string bus_add_corr_line1 { get; set; }
        public string bus_add_corr_line2 { get; set; }
        public string bus_add_corr_city { get; set; }
        public string bus_add_corr_state { get; set; }
        public string bus_add_corr_pincode { get; set; }
        public string bus_add_per_line1 { get; set; }
        public string bus_add_per_line2 { get; set; }
        public string bus_add_per_city { get; set; }
        public string bus_add_per_state { get; set; }
        public string bus_add_per_pincode { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; } //new added
        public string last_name { get; set; }
        public string father_fname { get; set; }
        public string father_lname { get; set; }
        public string type_of_addr { get; set; }
        public string resi_addr_ln1 { get; set; }
        public string resi_addr_ln2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string per_addr_ln1 { get; set; }
        public string per_addr_ln2 { get; set; }
        public string per_city { get; set; }
        public string per_state { get; set; }
        public string per_pincode { get; set; }
        public string appl_phone { get; set; }
        public string appl_pan { get; set; }
        public string email_id { get; set; }
        public string aadhar_card_num { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string age { get; set; }
        public string residence_status { get; set; }
        public string bureau_pull_consent { get; set; }

        //public string borro_bank_name { get; set; }
        //public string borro_bank_ifsc { get; set; }
        //public string borro_bank_acc_num { get; set; }
        //public string marital_status { get; set; }
        //public int SequenceNo { get; set; }
        //public string qualification { get; set; }
    }
    public class LoanDocumentPostDc
    {
        public long LeadMasterId { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string file_url { get; set; }
        public string code { get; set; }
        public string base64pdfencodedfile { get; set; }
        public string FrontUrl { get; set; }
        public string PdfPassword { get; set; }
        public bool IsBankStatement { get; set; }
    }
    public class LoanDocumentBankDc
    {
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string code { get; set; }
        public string base64pdfencodedfile { get; set; }
        public string doc_key { get; set; } //doc_key==pdfPassword \
        public string fileType { get; set; } //==bank_stmnts \

    }
    public class LoanDocumentDc
    {
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        // public string file_url { get; set; }
        public string code { get; set; }
        public string base64pdfencodedfile { get; set; }
    }
    public class AScoreAPIRequest
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string dob { get; set; }
        public string pan { get; set; }
        public string gender { get; set; }
        public string mobile_number { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string pin_code { get; set; }
        public string state_code { get; set; }
        public string enquiry_stage { get; set; }
        public string enquiry_purpose { get; set; }
        public string enquiry_amount { get; set; }
        public string en_acc_account_number_1 { get; set; }
        public string bureau_type { get; set; }
        public int tenure { get; set; } // 36 default
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string product_type { get; set; }
        public string consent_timestamp { get; set; }
    }

    public class CeplrPostDc
    {
        public long Id { get; set; }

        public string filepath { get; set; }
        public string email { get; set; }
        public string ifsc_code { get; set; }
        public string fip_id { get; set; }
        public string callback_url { get; set; }
        public string mobile { get; set; }
        public string name { get; set; }
        public string file_password { get; set; }
        public string configuration_uuid { get; set; }
        public bool allow_multiple { get; set; }
        public int request_id { get; set; }
        public string token { get; set; }
        public bool last_file { get; set; }
    }
    public class CeplrPdfReportDc
    {
        //public long LeadMasterId { get; set; }
        public string email { get; set; }
        public string file { get; set; }
        public string ifsc_code { get; set; }
        public string fip_id { get; set; }
        public string mobile { get; set; }
        public string name { get; set; }
        public string file_password { get; set; } //if pass. protected file is uploaded
        public string configuration_uuid { get; set; }
        public string callback_url { get; set; }
    }
    public class CeplrPdfResponse
    {
        public string customer_id { get; set; }
        public int request_id { get; set; }
        public string token { get; set; }
    }

    public class PdfResDcCeplr
    {
        //public int code { get; set; }
        public CeplrPdfResponse data { get; set; }
        //public string message { get; set; }
    }
    public class PanVerificationRequestJson
    {
        public string pan { get; set; }
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }
    }
    public class PanVerificationRequestJsonV3
    {
        public string pan { get; set; }
        public string name { get; set; }
        public string father_name { get; set; }
        public string dob { get; set; } //"1990-07-15"
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }
    }
    public class CoLenderRequest
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string dob { get; set; }
        public string appl_pan { get; set; }
        public string gender { get; set; }
        public string appl_phone { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string enquiry_purpose { get; set; }
        public string bureau_type { get; set; }
        public int tenure { get; set; }
        public string request_id_a_score { get; set; }
        public string request_id_b_score { get; set; }
        public string ceplr_cust_id { get; set; }
        public string interest_rate { get; set; }
        public string product_type_code { get; set; }
        public double sanction_amount { get; set; }
        public int dscr { get; set; }
        public double monthly_income { get; set; }
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }
    }
    public class FirstAadharXMLPost
    {
        public string aadhaar_no { get; set; }
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }
    }
    public class SecondAadharXMLPost
    {
        public string request_id { get; set; }
        public string aadhaar_no { get; set; }
        public int otp { get; set; }
        public string loan_app_id { get; set; }
        public string consent { get; set; }
        public string consent_timestamp { get; set; }
    }
    //public class SecondAadharXMLDc
    //{
    //    public long LeadMasterId { get; set; }
    //    public string request_id { get; set; }
    //    public int otp { get; set; }
    //    public double loan_amt { get; set; }
    //    public bool insurance_applied { get; set; }
    //}
    public class LoanApiRequestDc
    {
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_id { get; set; }
        public string a_score_request_id { get; set; }
        public string co_lender_assignment_id { get; set; }
        public string marital_status { get; set; }
        public string residence_vintage { get; set; }
        public string loan_app_date { get; set; }
        public string loan_amount_requested { get; set; }
        public string sanction_amount { get; set; }
        public string processing_fees_perc { get; set; }
        public string processing_fees_amt { get; set; }
        public string gst_on_pf_perc { get; set; }
        public string gst_on_pf_amt { get; set; }
        public string conv_fees { get; set; }
        public string insurance_amount { get; set; }
        public string net_disbur_amt { get; set; }
        public string int_type { get; set; }
        public string loan_int_rate { get; set; }
        public string loan_int_amt { get; set; }
        public string broken_period_int_amt { get; set; }
        public string repayment_type { get; set; }
        public string tenure_type { get; set; }
        public string tenure { get; set; }
        public string first_inst_date { get; set; }
        public string emi_amount { get; set; }
        public string emi_count { get; set; }
        public string final_approve_date { get; set; }
        public string final_remarks { get; set; }
        public string borro_bank_name { get; set; }
        public string borro_bank_acc_num { get; set; }
        public string borro_bank_ifsc { get; set; }
        public string borro_bank_account_holder_name { get; set; }
        public string borro_bank_account_type { get; set; }
        public string bene_bank_name { get; set; }
        public string bene_bank_acc_num { get; set; }
        public string bene_bank_ifsc { get; set; }
        public string bene_bank_account_holder_name { get; set; }
        public string bene_bank_account_type { get; set; }
        public string itr_ack_no { get; set; }
        public string business_name { get; set; }
        public string business_address_ownership { get; set; }
        public string program_type { get; set; }
        public string business_entity_type { get; set; }
        public string business_pan { get; set; }
        public string gst_number { get; set; }
        public string udyam_reg_no { get; set; }
        public string other_business_reg_no { get; set; }
        public string business_vintage_overall { get; set; }
        public string business_establishment_proof_type { get; set; }
        public string co_app_or_guar_name { get; set; }
        public string co_app_or_guar_dob { get; set; }
        public string co_app_or_guar_gender { get; set; }
        public string co_app_or_guar_address { get; set; }
        public string co_app_or_guar_mobile_no { get; set; }
        public string co_app_or_guar_pan { get; set; }
        public string relation_with_applicant { get; set; }
        public string co_app_or_guar_bureau_type { get; set; }
        public string co_app_or_guar_bureau_score { get; set; }
        public string co_app_or_guar_ntc { get; set; }
        public string insurance_company { get; set; }
        public string purpose_of_loan { get; set; }
        public string emi_obligation { get; set; }

    }
    public class Postrepayment_scheduleDc
    {
        public string loan_id { get; set; }
        public long company_id { get; set; }
        public string product_id { get; set; }
    }
    public class LoanStatusChangeAPIReq
    {
        public string loan_app_id { get; set; }
        public string loan_id { get; set; }
        public string borrower_id { get; set; }
        public string partner_loan_app_id { get; set; }
        public string partner_loan_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string status { get; set; }
    }
    public class CeplrBasicReportDc
    {
        public string start_date { get; set; }
        public string end_date { get; set; }

    }
    public class JsonXmlRequest
    {
        public string partner_loan_app_id { get; set; }
        public string partner_borrower_id { get; set; }
        public string loan_app_id { get; set; }
        public string borrower_id { get; set; }
        public string code { get; set; }
        public string base64pdfencodedfile { get; set; }
    }
    public class PanResponsev3
    {
        //public string pan_number { get; set; }
        //public string seeding_status { get; set; }
        //public string name_match { get; set; }
        //public string dob_match { get; set; }
        //public string status { get; set; }
        public string msg { get; set; }
    }

    public class PanValidationRspnsV3
    {
        public string kyc_id { get; set; }
        public PanResponsev3 data { get; set; }
        //public Message messages { get; set; }
        public bool success { get; set; }
        //public string message { get; set; }
        //public string KYCResponse { get; set; }
    }
    //public class Message
    //{
    //    public string type { get; set; }
    //    public string validationmsg { get; set; }
    //    public string @checked { get; set; }
    //    public string field { get; set; }
    //}
}
