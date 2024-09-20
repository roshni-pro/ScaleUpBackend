using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts
{
    [DataContract]
    public class UserTokenReply
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string UserToken { get; set; }
        [DataMember(Order = 3)]
        public string Message { get; set; }
    }
}
