using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class GetCustomerDetailReply
    {
        [DataMember(Order = 1)]
        public string CustomerCareMobile { get; set; }
        [DataMember(Order = 2)]
        public string CustomerCareEmail { get; set; }
    }
}
