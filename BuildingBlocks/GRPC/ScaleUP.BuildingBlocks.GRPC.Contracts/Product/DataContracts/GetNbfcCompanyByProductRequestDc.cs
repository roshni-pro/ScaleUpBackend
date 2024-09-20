using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class GetCompanyByProductRequestDc
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
        [DataMember(Order = 2)]
        public List<long> CompanyIds { get; set; }
    }
    [DataContract]
    public class GetProductNBFCConfigRequestDc
    {
        [DataMember(Order = 1)]
        public List<long> NBFCCompanyIds { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public double OfferAmount { get; set; }
    }
    [DataContract]
    public class GetProductNBFCConfigResponseDc
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)] 
        public string? ArrangementType { get; set; }//ArrangementTypeConstants
        [DataMember(Order = 4)]
        public double? PFSharePercentage { get; set; }
        [DataMember(Order = 5)]
        public long? Tenure { get; set; }
        [DataMember(Order = 6)]
        public double PF { get; set; }
        [DataMember(Order = 7)]
        public double InterestRate { get; set; }
        [DataMember(Order = 8)]
        public double GST { get; set; }
        [DataMember(Order = 9)]
        public double BounceCharge { get; set; }
        [DataMember(Order = 10)]
        public double MaxInterestRate { get; set; }
        [DataMember(Order = 11)]
        public bool IseSignEnable { get; set; }
        [DataMember(Order = 12)]
        public double ODCharges { get; set; }
        [DataMember(Order = 13)]
        public double PenalPercent { get; set; }
        [DataMember(Order = 14)]
        public int ODdays { get; set; }
        [DataMember(Order = 15)]
        public double? PlatFormFee { get; set; }
        [DataMember(Order = 16)]
        public double? MaxTenure { get; set; }
        [DataMember(Order = 17)]
        public List<ProductSlabConfigResponse> ProductSlabConfigs { get; set; }
        [DataMember(Order = 18)]
        public long? MaxBounceCharge { get; set; }
        [DataMember(Order = 19)]
        public double MaxPenalPercent { get; set; }
        [DataMember(Order = 20)]
        public string CustomerAgreementURL { get; set; }
        [DataMember(Order = 21)]
        public double NBFCInterest { get; set; }
        [DataMember(Order = 22)]
        public double NBFCProcessingFee { get; set; }
        [DataMember(Order = 23)]
        public string NBFCProcessingFeeType { get; set; }
    }
    [DataContract]
    public class BLNBFCConfigs
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public double BounceCharge { get; set; }
        [DataMember(Order = 4)]
        public double MinPenalPercent { get; set; }
        [DataMember(Order = 5)]
        public double MaxPenalPercent { get; set; }
    }
    [DataContract]
    public class UploadBLInvoiceExcelReq
    {
        [DataMember(Order = 1)]
        public string FileUrl { get; set; }
        [DataMember(Order = 2)]
        public long FinTechCompanyId { get; set; }
    }
}
