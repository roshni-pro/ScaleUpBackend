using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class ActivitySubActivityNameRequest
    {
        [DataMember(Order = 1)]
        public long ActivityId { get; set; }
        [DataMember(Order = 2)]
        public long? SubActivityId { get; set; }
        
    }

    [DataContract]
    public class ActivitySubActivityNameReply
    {
        [DataMember(Order = 1)]
        public long ActivityId { get; set; }
        [DataMember(Order = 2)]
        public long? SubActivityId { get; set; }

        [DataMember(Order = 3)]
        public string ActivityName { get; set; }
        [DataMember(Order = 4)]
        public string SubActivityName { get; set; }
    }
}
