using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response
{
    public class CoLenderResponseDc
    {
        public string request_id { get; set; }
        public object loan_amount { get; set; }
        public object pricing { get; set; }
        public string co_lender_shortcode { get; set; }
        public string loan_app_id { get; set; }
        public int co_lender_assignment_id { get; set; }
        public string co_lender_full_name { get; set; }
        public string status { get; set; }
        //public string message { get; set; }
        public string program_type { get; set; }
    }
    public class AadharOtpDataRes
    {
        public string requestId { get; set; }
        public AadharOtpResult result { get; set; }
        //public int statusCode { get; set; }
    }

    public class AadharOtpResult
    {
        public string message { get; set; }
    }

    public class AadharOtpGenerateRes
    {
        //public string kyc_id { get; set; }
        public AadharOtpDataRes data { get; set; }
        public bool success { get; set; } //
        public string message { get; set; }
    }
    public class SecondAadharXMLResponseDc
    {
        public string kyc_id { get; set; }
        public Data data { get; set; }
        public bool success { get; set; }
        //public string message { get; set; }
        //public string KYCResponse { get; set; }
    }
    public class Data
    {
        //public string requestId { get; set; }
        public Result result { get; set; }
        //public int statusCode { get; set; }
    }
    public class Result
    {
        //public DataFromAadhaar dataFromAadhaar { get; set; }
        public string message { get; set; }
        //public string shareCode { get; set; }
    }
    public class DataFromAadhaar
    {
        public string generatedDateTime { get; set; }
        public string maskedAadhaarNumber { get; set; }
        public string name { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string mobileHash { get; set; }
        public string emailHash { get; set; }
        public string relativeName { get; set; }
        public Address address { get; set; }
        public string image { get; set; }
        public string maskedVID { get; set; }
        public string file { get; set; }
    }
    public class Address
    {
        public SplitAddress splitAddress { get; set; }
        public string combinedAddress { get; set; }
    }
    public class SplitAddress
    {
        public string houseNumber { get; set; }
        public string street { get; set; }
        public string landmark { get; set; }
        public string subdistrict { get; set; }
        public string district { get; set; }
        public string vtcName { get; set; }
        public string location { get; set; }
        public string postOffice { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string pincode { get; set; }
    }

    public class OfferEmiDetailDC
    {
        public DateTime DueDate { get; set; }
        public double OutStandingAmount { get; set; }
        public double Prin { get; set; }
        public double InterestAmount { get; set; }
        public double EMIAmount { get; set; }
        public double PrincipalAmount { get; set; }
    }
}
