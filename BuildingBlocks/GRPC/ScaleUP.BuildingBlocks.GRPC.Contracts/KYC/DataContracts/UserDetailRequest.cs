using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class UserDetailRequest
    {
        [DataMember(Order = 1)]
        public List<string> Usersid { get; set; }
    }


}
