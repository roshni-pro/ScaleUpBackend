using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class GenerateOTPResponse
    {
        public bool Status { get; set; }        
        public string Message { get; set; }
        public string OTP { get; set; }
    }

    public class ForgotPasswordResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
