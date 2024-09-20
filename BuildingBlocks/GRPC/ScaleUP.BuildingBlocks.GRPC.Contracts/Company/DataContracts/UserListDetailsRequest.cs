using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class UserListDetailsRequest
    {
        [DataMember(Order = 1)]
        public List<string> userIds { get; set; }
        [DataMember(Order = 2)]
        public string? keyword { get; set; }

        [DataMember(Order = 3)]
        public int Skip { get; set; }
        [DataMember(Order = 4)]
        public int Take { get; set; }
    }
}
