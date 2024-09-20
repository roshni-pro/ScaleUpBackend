namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadMobileValidate
    {
        public long ProductId { get; set; }
        public long CompanyId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public string MobileNo { get; set;}
        public string OTP { get; set;}
        public int? VintageDays { get; set; }
        public int? MonthlyAvgBuying { get; set; }
        public string ProductCode { get; set; }
        public string CompanyCode { get; set; }
    }
    public class LeadEmailValidate
    {
        public string Email { get; set; }
        public string OTP { get; set; }
    }
    public class CustomerMobileValidate
    {
        public string MobileNo { get; set; }
        public string OTP { get; set; }
    }
}
