using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class LeadProductReply
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public string ProductType { get; set; }
        [DataMember(Order = 4)]
        public List<GRPCLeadProductActivity> LeadProductActivity { get; set; }
    }

    [DataContract]
    public class GRPCLeadProductActivity
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
        [DataMember(Order = 2)]
        public long ActivityMasterId { get; set; }
        [DataMember(Order = 3)]
        public long? SubActivityMasterId { get; set; }
        [DataMember(Order = 4)]
        public string ActivityName { get; set; }
        [DataMember(Order = 5)]
        public string? SubActivityName { get; set; }
        [DataMember(Order = 6)]
        public int Sequence { get; set; }
        [DataMember(Order = 7)]
        public string? KycMasterCode { get; set; }
        [DataMember(Order = 8)]
        public long LeadId { get; set; }
    }
}
