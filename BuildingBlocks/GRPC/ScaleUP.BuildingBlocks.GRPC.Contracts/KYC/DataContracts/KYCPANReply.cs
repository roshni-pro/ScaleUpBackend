using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class KYCPANReply
    {
        [DataMember(Order = 1)]
        public long? DocumentId { get; set; }
        [DataMember(Order = 2)]
        public string UniqueId { get; set; }

        [DataMember(Order = 3)]
        public bool Status { get; set; }
    }
}
