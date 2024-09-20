using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts
{
    [DataContract]
    public class CustomerAddressRequest
    {
        [DataMember(Order = 1)]
        public long addressId { get; set; }
    }
}
