namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadMobileResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public long LeadId { get; set; }
        public string ProductType { get; set; }
        public string UserId { get; set; }
        public string UserTokan { get; set; }
    }
    public class LeadEmailResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class CustomerMobileResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public string UserTokan { get; set; }
    }
}
