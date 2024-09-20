using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class UserListPageInfoReply
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }
        [DataMember(Order = 2)]
        public string? CustomerName { get; set; }
        [DataMember(Order = 3)]
        public string? AlternatePhoneNo { get; set; }
        [DataMember(Order = 4)]
        public string? EmailId { get; set; }
    }
}
