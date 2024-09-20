using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.ThirdApiConfig
{
    public class MaskedMobileGenerationResponseDC
    {
        public object errorString { get; set; }
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public List<string> maskMobileno { get; set; }
    }
    public class MaskedMobileGenerationRequestDC
    {
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public long CompanyId { get; set; }
    }
    public class AddRequestResponseDc
    {
        public long ApiMasterId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string RequestResponseMsg { get; set; }
        public string Header { get; set; }
        public string PersonId { get; set; }//SpringVerify  api

    }
    public class MaskedMobileOTPGenerationResponseDC
    {
        public object errorString { get; set; }
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public string otpGenerationStatus { get; set; }
    }
    public class MaskedMobileOTPGenerationRequestDC
    {
        public string mobileNo { get; set; }
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public long CompanyId { get; set; }
        //public string type { get; set; }
    }
    public class MaskedMobileOTPValidationRequestDC
    {
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public string otp { get; set; }
        public string maskedMobile { get; set; }
        public long LeadMasterId { get; set; }
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public long CompanyId { get; set; }
    }
}
