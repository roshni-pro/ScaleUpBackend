using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class GetLeadStatusByActivityRequest
    {
        [DataMember(Order = 1)]
        public long ActivityId { get; set; }
        [DataMember(Order = 2)]
        public long? SubActivityId { get; set; }
    }
}
