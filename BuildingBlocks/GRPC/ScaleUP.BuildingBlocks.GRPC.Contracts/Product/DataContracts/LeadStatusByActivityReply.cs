using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class LeadStatusByActivityReply
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        //public string ScreenName { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public List<LeadStatusActivityList> LeadStatusActivityList { get; set; }

    }

    [DataContract]
    public class LeadStatusActivityList
    {
        [DataMember(Order = 1)]
        public long ActivityId { get; set; }
        [DataMember(Order = 2)]
        public long? SubActivityId { get; set; }
        [DataMember(Order = 3)]
        public string ActivityName { get; set; }
        [DataMember(Order = 4)]
        public string? SubActivityName { get; set; }
        [DataMember(Order = 5)]
        public long? ActivityMasterId { get; set; }
    }
}
