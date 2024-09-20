using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class PersonalDetailDTO
    {
        public long LeadMasterId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string FatherLastName { get; set; }
        public string Gender { get; set; }
        public string AlternatePhoneNo { get; set; }
        public string EmailId { get; set; }
     
        public long? PermanentAddressId { get; set; }
        public long? CurrentAddressId { get; set; }
        public string? MobileNo { get; set; }
        public string OwnershipType { get; set; }
        public string Marital { get; set; }
        public string? OwnershipTypeProof { get; set; }
        public long? ElectricityBillDocumentId { get; set; }
        public string? IVRSNumber { get; set; }
        public string? OwnershipTypeName { get; set; }
        public string? OwnershipTypeAddress { get; set; }
        public string? OwnershipTypeResponseId { get; set; }
        public string? ElectricityServiceProvider { get; set; }
        public string? ElectricityState { get; set; }

        //extra field added
    }
    //public class AddPersonalDetailDc
    //{
    //    public long LeadMasterId { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string FatherName { get; set; }
    //    public string FatherLastName { get; set; }
    //    public string DOB { get; set; }
    //    public string Gender { get; set; }
    //    public string AlternatePhoneNo { get; set; }
    //    public string EmailId { get; set; }
    //    public string TypeOfAddress { get; set; }
    //    public string ResAddress1 { get; set; }
    //    public string ResAddress2 { get; set; }
    //    public string City { get; set; }
    //    public string State { get; set; }
    //    public string Pincode { get; set; }
    //    public string PermanentAddressLine1 { get; set; }
    //    public string PermanentAddressLine2 { get; set; }
    //    public string PermanentCity { get; set; }
    //    public string PermanentState { get; set; }
    //    public string PermanentPincode { get; set; }
    //    public string ResidenceStatus { get; set; }
    //}
    //public class CommonResponseDc
    //{
    //    public string Msg { get; set; }
    //    public bool Status { get; set; }
    //    public object Data { get; set; }
    //    public bool IsNotEditable { get; set; }
    //    public List<LeadDocUrlDc> leadDocUrlDcs { get; set; }
    //    public ArthMateOfferDc ArthMateOffer { get; set; }
    //    public string NameOnCard { get; set; }

    //}

    //public class LeadDocUrlDc
    //{
    //    public string DocumentName { get; set; }
    //    public string FrontFileUrl { get; set; }
    //    public string BackFileUrl { get; set; }
    //    public string DocumentNumber { get; set; }
    //    public string Selfie { get; set; }
    //    public string OtherInfo { get; set; }
    //}
    //public class ArthMateOfferDc
    //{
    //    public double loan_amt { get; set; }
    //    public double interest_rt { get; set; }
    //    public int loan_tnr { get; set; }
    //    public string loan_tnr_type { get; set; } //Month, Year
    //    public double Orignal_loan_amt { get; set; }

    //}

    //public class LeadResponseDc
    //{
    //    public bool success { get; set; }
    //    public string message { get; set; }
    //    public string errorCode { get; set; }
    //    public ArthmateLoanDc data { get; set; }
    //    public List<LeadDatum> LeadDatum { get; set; }
    //}
    //public class ArthmateLoanDc
    //{
    //    public List<ExactErrorRow> exactErrorRows { get; set; }
    //    public List<ErrorRow> errorRows { get; set; }
    //    public List<PreparedbiTmpl> preparedbiTmpl { get; set; }

    //}
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
    //public class PreparedbiTmpl
    //{
    //    public string partner_loan_app_id { get; set; }
    //    public string partner_borrower_id { get; set; }
    //    public string loan_app_id { get; set; }
    //    public string borrower_id { get; set; }
    //}
}

