using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class SaleAgentRequest
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }

        [DataMember(Order = 2)]
        public string CompanyCode { get; set; }
        [DataMember(Order = 3)]
        public List<long> ProductIds { get; set; }
        [DataMember(Order = 4)]
        public long CompanyId { get; set; }
    }
}
