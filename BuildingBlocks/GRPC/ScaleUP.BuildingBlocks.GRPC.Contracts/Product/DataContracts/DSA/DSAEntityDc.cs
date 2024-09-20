using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA
{
    [DataContract]
    public class DSAEntityDc
    {
        [DataMember(Order =1)]
        public string EntityName { get; set; }
        [DataMember(Order =2)]
        public string DSAType { get; set; }
    }
}
