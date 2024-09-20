using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity
{
    public class KYCActivityAadhar
    {
        public string DocumentNumber { get; set; }
        //public long? FrontDocumentId { get; set; }
        //public long? BackDocumentId { get; set; }
        public string FrontDocumentId { get; set; }
        public string BackDocumentId { get; set; }
        public string? otp { get; set; }
        public string? requestId { get; set; }

        public KarzaAadharDTO? aadharInfo { get; set; }
        public long? aadharAddressId{ get; set; }

    }


    public class KarzaAadharDTO
    {
        public string GeneratedDateTime { get; set; }
        public string MaskedAadhaarNumber { get; set; }
        public string Name { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string MobileHash { get; set; }
        public string EmailHash { get; set; }
        public string FatherName { get; set; }
        public Addressess address { get; set; }
        public string Image { get; set; }
        public string MaskedVID { get; set; }
        public string File { get; set; }
        public string FrontImageUrl { get; set; }
        public string BackImageUrl { get; set; }
        public string DocumentNumber { get; set; }
        public string FrontDocumentId { get; set; }
        public string BackDocumentId { get; set; }
        public long? GeneratedAddressId { get; set; }
    }
    public class Addressess
    {
        public SplitAddressess splitAddress { get; set; }
        public string CombinedAddress { get; set; }
    }
    public class SplitAddressess
    {
        public string HouseNumber { get; set; }
        public string Street { get; set; }
        public string Landmark { get; set; }
        public string Subdistrict { get; set; }
        public string District { get; set; }
        public string VtcName { get; set; }
        public string Location { get; set; }
        public string PostOffice { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
    }
    public class eAadhaarDigilockerResponseDc
    {
        public string requestId { get; set; }
        public res_msg result { get; set; }
        public int? statusCode { get; set; }
        public ErrorResponse error { get; set; }
        public string personId { get; set; }
    }
    public class ErrorResponse
    {
        public Error error { get; set; }
    }
    public class res_msg
    {
        public string message { get; set; }
    }
    public class Error
    {
        public string requestId { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string error { get; set; }
    }
    public class eAadhaarDigilockerRequestDc
    {

        public string consent { get; set; }
        public string aadhaarNo { get; set; }

    }
    public class eAdhaarDigilockerVerifyOTPResponseDcXml
    {
        public string requestId { get; set; }
        public res_message result { get; set; }
        public int? statusCode { get; set; }
        public ErrorResponse error { get; set; }
        public string? statusMessage { get; set; }     
    }
    public class res_message
    {
        public KarzaAadharDTO dataFromAadhaar { get; set; }
        public string message { get; set; }
        public string shareCode { get; set; }
    }
    public class DataFromAdhaar
    {
        public string generatedDateTime { get; set; }
        public string maskedAadhaarNumber { get; set; }
        public string name { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string mobileHash { get; set; }
        public string emailHash { get; set; }
        public string fatherName { get; set; }
        public Addressess address { get; set; }
        public string image { get; set; }
        public string maskedVID { get; set; }
        public string file { get; set; }

    }
    public class eAadhaarDigilockerRequesTVerifyOTPDCXml
    {
        public string otp { get; set; }
        public string requestId { get; set; }
        public string consent { get; set; }
        public string aadhaarNo { get; set; }
    }

}
