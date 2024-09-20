using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class KYCAadharReply
    {
        [DataMember(Order = 1)]
        public long? FrontDocumentId { get; set; }

        [DataMember(Order = 2)]
        public long? BackDocumentId { get; set; }

        [DataMember(Order = 3)]
        public string UniqueId { get; set; }

        [DataMember(Order = 4)]
        public bool Status { get; set; }
    }
}
