using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class OrderToken
    {
        public string Token { get; set; }
        public string TransactionReqNo { get; set; }
        public string CustomerName { get; set; }
        public string ImageUrl { get; set; }
        public string? CustomerCareMoblie { get; set; }
        public string? CustomerCareEmail { get; set; }
    }
}
