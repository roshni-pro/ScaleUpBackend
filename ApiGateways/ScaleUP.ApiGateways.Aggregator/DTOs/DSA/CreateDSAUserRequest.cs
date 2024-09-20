using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs.DSA
{
    public class CreateDSAUserRequest
    {
        [StringLength(10)]
        public string MobileNumber { get; set; }
        public string FullName { get; set; }
        public string EmailId { get; set; }
        public double? PayoutPercenatge { get; set; }
    }
}
