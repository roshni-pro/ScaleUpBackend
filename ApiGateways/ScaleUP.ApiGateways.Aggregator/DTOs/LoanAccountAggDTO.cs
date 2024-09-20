using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LoanAccountAggDTO
    {
        public required long LeadId { get; set; }
        public long ProductId { get; set; }
        public string UserId { get; set; }
        public string AccountCode { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public long NBFCCompanyId { get; set; }
        public long? AnchorCompanyId { get; set; }
    }
}
