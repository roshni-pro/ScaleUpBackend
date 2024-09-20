using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class SalesAgentReply
    {
        [DataMember(Order = 1)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public string AnchorCompanyCode { get; set; }

        [DataMember(Order = 3)]
        public long ProductId { get; set; }

        [DataMember(Order = 4)]
        public string ProductCode { get; set; }

        [DataMember(Order = 5)]
        public string UserId { get; set; }

        [DataMember(Order = 6)]
        public string Role { get; set; }

        [DataMember(Order = 7)]
        public string Type { get; set; }
        [DataMember(Order = 8)]
        public string Name { get; set; }
        [DataMember(Order = 9)]
        public string PanNumber { get; set; }
        [DataMember(Order = 10)]
        public string AadharNumber { get; set; }
        [DataMember(Order = 11)]
        public string Mobile { get; set; }
        [DataMember(Order = 12)]
        public string Address { get; set; }
        [DataMember(Order = 13)]
        public string WorkingLocation { get; set; }
        [DataMember(Order = 14)]
        public string selfie { get; set; }
        [DataMember(Order = 15)]
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
        [DataMember(Order = 16)]
        public string? DocSignedUrl { get; set; }
        [DataMember(Order = 17)]
        public DateTime? StartedOn { get; set; }
        [DataMember(Order = 18)]
        public DateTime? ExpiredOn { get; set; }
        [DataMember(Order = 19)]
        public string? CreatedBy { get; set; }
        [DataMember(Order = 20)]
        public string  DSACode { get; set; }
    }
}
