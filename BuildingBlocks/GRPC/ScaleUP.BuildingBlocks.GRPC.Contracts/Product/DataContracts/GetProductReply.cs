using System.Runtime.Serialization;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]

    public class GetProductReply
    {
        [DataMember(Order = 1)]
        public string ProductName { get; set; }
    }
    [DataContract]
    public class GetProductDataReply
    {
        [DataMember(Order = 1)]
        public string ProductName { get; set; }
        [DataMember(Order = 2)]
        public string ProductType { get; set; }
    }
}
