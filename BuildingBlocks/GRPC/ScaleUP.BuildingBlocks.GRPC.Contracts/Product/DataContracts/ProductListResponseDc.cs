using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class ProductListResponseDc
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
        [DataMember(Order = 2)]
        public string ProductName { get; set; }
    }
}
