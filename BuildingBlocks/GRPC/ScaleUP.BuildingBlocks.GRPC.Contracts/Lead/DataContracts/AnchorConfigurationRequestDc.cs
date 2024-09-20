using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class GenerateOfferByFinanceRequestDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long NbfcCompanyId { get; set;}
        [DataMember(Order = 3)]
        public double OfferAmount { get; set;}
    }
    [DataContract]
    public class GetGenerateOfferByFinanceRequestDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long NbfcCompanyId { get; set; }
        //[DataMember(Order = 3)]
        //public string? Role { get; set;}
    }
    [DataContract]
    public class AddLeadOfferConfigRequestDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public List<ProductSlabConfigResponse> ProductSlabConfigs { get; set; }
    }
}
