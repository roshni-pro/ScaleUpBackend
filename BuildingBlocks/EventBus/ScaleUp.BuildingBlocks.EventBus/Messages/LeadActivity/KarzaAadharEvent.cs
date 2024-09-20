using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public class KarzaAadharEvent
    {
        public string? DocumentNumber { get; set; }
        public string? otp { get; set; }
        public string? requestId { get; set; }
        public KarzaAadharDTO? aadharInfo { get; set; }
        public long? aadharAddressId { get; set; }
        public string UserId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public long LeadMasterId { get; set; }
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
    public class eAadhaarDigilockerRequesTVerifyOTPDCXml
    {
        public string otp { get; set; }
        public string requestId { get; set; }
        public string consent { get; set; }
        public string? aadhaarNo { get; set; }
        public long LeadMasterId { get; set; }
    }

}
