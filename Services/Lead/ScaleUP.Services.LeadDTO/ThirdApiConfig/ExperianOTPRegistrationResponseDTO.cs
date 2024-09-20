using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.ThirdApiConfig
{
    public class ExperianOTPRegistrationResponseDTO
    {
        public object errorString { get; set; }
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
    }
   
    public class ExperianOTPRegistrationResponseDC
    {
        public object errorString { get; set; }
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
    }

    public class ExperianOTPGenerationResponseDC
    {
        public object errorString { get; set; }
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public string otpGenerationStatus { get; set; }
    }
    public class ExperianOTPGenerationRequestDC
    {
        public string mobileNo { get; set; }
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public long CompanyId { get; set; }
    }

    public class ExperianOTPValidationResponseDC
    {
        public string errorString { get; set; }
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public string showHtmlReportForCreditReport { get; set; }
        public string CreditScore { get; set; }
        public GetQualifiedCreditDc GetQualifiedCredit { get; set; }
        public string FilePath { get; set; }
    }

    public class ExperianOTPValidationRequestDC
    {
        public string stgOneHitId { get; set; }
        public string stgTwoHitId { get; set; }
        public string otp { get; set; }
        public string mobileNo { get; set; }

        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public long CompanyId { get; set; }
    }

    public class GetQualifiedCreditDc
    {
        public string UniqueCode { set; get; }//
        public double CreditLimit { set; get; }
        public string CreditScore { set; get; }
    }
}
