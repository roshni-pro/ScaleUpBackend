using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class UserResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
    }
}
