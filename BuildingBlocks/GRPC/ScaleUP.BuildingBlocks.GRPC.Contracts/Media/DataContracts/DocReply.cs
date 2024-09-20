using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts
{
    [DataContract]
    public class DocReply
    {
        [DataMember(Order = 1)]
        public string ImagePath { get; set; }
        [DataMember(Order = 2)]
        public bool Status { get; set; }
        [DataMember(Order = 3)]
        public long? Id { get; set; }

    }
}
