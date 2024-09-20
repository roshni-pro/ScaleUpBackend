using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadAnchorProductRequest
    {
        [DataMember(Order = 1)]
        public long? AnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public string UserId { get; set; }
        [DataMember(Order = 4)]
        public string ProductCode { get; set; }
        [DataMember(Order = 5)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 6)]
        public string? LastModifyBy { get; set; }
        [DataMember(Order = 7)]
        public DateTime Created { get;  set; }
        [DataMember(Order = 8)]
        public string? RejectionReason { get; set; }
    }

    
}
