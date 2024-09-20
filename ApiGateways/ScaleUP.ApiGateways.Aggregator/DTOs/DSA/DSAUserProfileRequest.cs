using System.ComponentModel.DataAnnotations;

namespace ScaleUP.ApiGateways.Aggregator.DTOs.DSA
{
    public class DSAUserProfileRequest
    {
        [StringLength(10)]
        public string MobileNumber { get; set; }
        public long LeadId { get; set; }

    }
}
