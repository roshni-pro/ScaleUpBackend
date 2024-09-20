using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts
{
    [DataContract]
    public class CreateRoleRequest
    {
        [DataMember(Order = 1)]
        public string RoleName { get; set; }
    }
}
