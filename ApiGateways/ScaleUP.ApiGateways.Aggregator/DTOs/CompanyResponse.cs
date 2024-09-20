namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class CompanyResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }      
    }
    public class CreateCompanyResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public long CompanyId { get; set; }
    }
}
