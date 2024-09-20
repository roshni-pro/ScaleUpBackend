using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class KYCMSMEReply
    {
        [DataMember(Order = 1)]
        public long? DocumentId { get; set; }
        [DataMember(Order = 2)]
        public string UniqueId { get; set; }

        [DataMember(Order = 3)]
        public bool Status { get; set; }
        [DataMember(Order = 4)]
        public string BusinessName { get; set; }
        [DataMember(Order = 5)]
        public string BusinessType { get; set; }
        [DataMember(Order = 6)]
        public int Vintage { get; set; }
    }
}
