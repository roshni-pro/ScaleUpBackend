using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts
{
    [DataContract]
    public class UserUpdateByIdRequestDTO
    {
        [DataMember(Order =1)]
        public string Id { get; set; }
        [DataMember(Order =2)]
        public string MobileNo { get; set; }
        [DataMember(Order =3)]
        public string Email { get; set; }
        [DataMember(Order =4)]
        public List<string> Roles { get; set; }
        [DataMember(Order =5)]
        public List<long> CompanyIds { get; set; }
        [DataMember(Order =6)]
        public string UserType { get; set; }
        [DataMember(Order = 7)]
        public long FinTechId { get; set; }
    }
}
