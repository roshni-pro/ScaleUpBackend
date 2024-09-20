using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadActivitySequenceResponse
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public int CurrentSequence { get; set; }
        [DataMember(Order = 4)]
        public List<LeadActivityReply> LeadActivityReply { get; set; }
    }

    [DataContract]
    public class LeadActivityReply
    {
        [DataMember(Order = 1)]
        public long ActivityId { get; set; }
        [DataMember(Order = 2)]
        public long? SubActivityId { get; set; }
        [DataMember(Order = 3)]
        public int Sequence { get; set; }
        [DataMember(Order = 4)]
        public bool IsCompleted { get; set; }
        [DataMember(Order = 5)]
        public int IsApprove { get; set; }
        [DataMember(Order = 6)]
        public string RejectedReason { get; set;}
    }


    [DataContract]
    public class LeadActivityProgressListReply
    {
        [DataMember(Order = 1)]
        public long ActivityId { get; set; }
        [DataMember(Order = 2)]
        public long? SubActivityId { get; set; }
        [DataMember(Order = 3)]
        public int Sequence { get; set; }
        [DataMember(Order = 4)]
        public bool IsCompleted { get; set; }
        [DataMember(Order = 5)]
        public int IsApproved { get; set; }
        [DataMember(Order = 6)]
        public string leadUserId { get; set; }
        [DataMember(Order = 7)]
        public string ActivityName { get; set; }
        [DataMember(Order = 8)]
        public string SubActivityName { get; set; }
        [DataMember(Order = 9)]
        public string? RejectMessage { get; set; }
    }
}
