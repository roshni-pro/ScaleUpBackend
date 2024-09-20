using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;

namespace ScaleUP.ApiGateways.Aggregator.DTOs.DSA
{
    public class DSAUserProfileResponse
    {

        public bool Status { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public string UserToken { get; set; }
        public bool IsActivated { get; set; }
        public string CompanyCode { get; set; }
        public long CompanyId { get; set; }
        public string ProductCode { get; set; }
        public long ProductId { get; set; }
        public string Role { get; set; }
        public string Type { get; set; }
        public userData userData { get; set; }
        public string? DSALeadCode { get; set; }
    }
    public class userData
    {
        public string Name { get; set; }
        public string PanNumber { get; set; }
        public string AadharNumber { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string WorkingLocation { get; set; }
        public string selfie { get; set; }
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
        public string? DocSignedUrl { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? ExpiredOn { get; set; }
    }
}
