using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class ProductDetailRespnose
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public string ProductType { get; set; }
        [DataMember(Order = 4)]
        public long ProductId { get; set; }
        [DataMember(Order = 5)]
        public string ProductCode { get; set; }
        [DataMember(Order = 6)]
        public List<GRPCLeadProductActivity> LeadProductActivities { get; set; }
    }
   
}
