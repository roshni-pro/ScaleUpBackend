using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System.Runtime.Serialization;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadMobileRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string MobileNo { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
        [DataMember(Order = 4)]
        public long ActivityId { get; set; }
        [DataMember(Order = 5)]
        public long? SubActivityId { get; set; }
        [DataMember(Order = 6)]
        public string UserId { get; set; }
        [DataMember(Order = 7)]
        public int? VintageDays { get; set; }
        [DataMember(Order = 8)]
        public int? MonthlyAvgBuying { get; set; }
        [DataMember(Order = 9)]
        public List<GRPCLeadProductActivity>? ProductActivities { get; set; }
        [DataMember(Order = 10)]
        public string ProductCode { get; set; }
        [DataMember(Order = 11)]
        public string CompanyCode { get; set; }

    }
}
