using System.Runtime.Serialization;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class GetProductRequest
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
    }
    [DataContract]
    public class CompanyProductRequest
    {
        [DataMember(Order = 1)]
        public List<long> CompanyIds { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public long AnchorCompanyId { get; set; }


    }
}
