using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts
{
    [DataContract]
    public class CompanyAddressRequest
    {
        [DataMember(Order = 1)]
        public long companyId { get; set; }
        [DataMember(Order = 2)]
        public List<long> location { get; set; }
    }

}
