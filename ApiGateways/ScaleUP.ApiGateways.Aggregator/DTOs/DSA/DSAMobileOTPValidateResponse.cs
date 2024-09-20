namespace ScaleUP.ApiGateways.Aggregator.DTOs.DSA
{
    public class DSAMobileOTPValidateResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
    }
}
