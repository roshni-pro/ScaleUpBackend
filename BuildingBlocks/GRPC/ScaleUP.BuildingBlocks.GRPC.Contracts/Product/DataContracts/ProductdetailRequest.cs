using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class ProductdetailRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string CompanyType { get; set; }
        [DataMember(Order = 3)]
        public string ProductCode { get; set; }
    }
    [DataContract]
    public class ProductSlabConfigRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public List<string>? SlabTypes { get; set; } //SlabTypeConstants (null for all slabs)
    }
    [DataContract]
    public class ProductSlabConfigResponse
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public string SlabType { get; set; } //SlabTypeConstants
        [DataMember(Order = 4)]
        public double MinLoanAmount { get; set; }
        [DataMember(Order = 5)]
        public double MaxLoanAmount { get; set; }
        [DataMember(Order = 6)]
        public double MinValue { get; set; }
        [DataMember(Order = 7)]
        public double MaxValue { get; set; }
        [DataMember(Order = 8)]
        public string ValueType { get; set; }
        [DataMember(Order = 9)]
        public bool IsFixed { get; set; }
        [DataMember(Order = 10)]
        public double? SharePercentage { get; set; } //(Over and Above Share %)
    }
    [DataContract]
    public class GetCompanyListByProductRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string CompanyType { get; set; }
    }

    [DataContract]
    public class AddUpdateTokenNBFCCompany
    {
        [DataMember(Order = 1)]
        public string token { get; set; }
        [DataMember(Order = 2)]
        public long companyId { get; set; }
        [DataMember(Order = 3)]
        public string productType { get; set; }

    }
}
