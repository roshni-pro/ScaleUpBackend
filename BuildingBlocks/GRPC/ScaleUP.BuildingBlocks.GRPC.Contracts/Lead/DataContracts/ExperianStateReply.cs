using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class ExperianStateReply
    {
        [DataMember(Order = 1)]
        public long LocationStateId { get; set; }
        [DataMember(Order = 2)]
        public long ExperianStateId { get; set; }
    }
}
