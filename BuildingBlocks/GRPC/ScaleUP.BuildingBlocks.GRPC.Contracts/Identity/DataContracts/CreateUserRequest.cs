using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts
{
    [DataContract]
    public class CreateUserRequest
    {
        [DataMember(Order = 1)]
        public string UserType { get; set; }
        [DataMember(Order = 2)]
        public string UserName { get; set; }
        [DataMember(Order = 3)]
        public string Password { get; set; }
        [DataMember(Order = 4)]
        public string MobileNo { get; set; }
        [DataMember(Order = 5)]
        public string EmailId { get; set; }
        [DataMember(Order = 6)]
        public List<KeyValuePair<string, string>> Claims { get; set; }
        [DataMember(Order = 7)]
        public List<string> UserRoles { get; set; }
    }
}
