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
    public class ProductCompanyConfigDc
    {
        [DataMember(Order = 1)]
        public double PF { get; set; }
        [DataMember(Order = 2)]
        public double InterestRate { get; set;}
        [DataMember(Order = 3)]
        public double GST { get; set;}
        [DataMember(Order = 4)]
        public double BounceCharge { get; set; }
        [DataMember(Order = 5)]
        public double MaxInterestRate { get; set; }
        [DataMember(Order = 6)]
        public bool IseSignEnable { get; set; }
        [DataMember(Order = 7)]
        public double ODCharges { get; set; }
        [DataMember(Order = 8)]
        public double PenalPercent { get;set; }
        [DataMember(Order = 9)] 
        public int ODdays { get; set;}
        [DataMember(Order = 10)]
        public double? PlatFormFee { get; set; }
        [DataMember(Order = 11)]
        public double? MaxTenure { get; set; }
        [DataMember(Order = 12)]
        public List<ProductSlabConfigResponse> ProductSlabConfigs { get; set; }
    }
}
