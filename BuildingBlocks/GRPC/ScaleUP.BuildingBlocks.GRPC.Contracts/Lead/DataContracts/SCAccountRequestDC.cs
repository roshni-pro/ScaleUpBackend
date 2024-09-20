using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class SCAccountRequestDC
    {
        [DataMember(Order = 1)]
        public long? AnchorId { get; set; }
        [DataMember(Order = 2)]
        public string? CityName { get; set; }
        [DataMember(Order = 3)]
        public string? Keyword { get; set; }
        [DataMember(Order = 4)]
        public DateTime? FromDate { get; set; }
        [DataMember(Order = 5)]
        public DateTime? ToDate { get; set; }
        [DataMember(Order = 6)]
        public int Skip { get; set; }
        [DataMember(Order = 7)]
        public int Take { get; set; }
        [DataMember(Order = 8)]
        public string? Status { get; set; }
        [DataMember(Order = 9)]
        public long? CityId { get; set; }
    }
    [DataContract]
    public class SCAccountResponseDc
    {
        [DataMember(Order = 1)]
        public string LeadCode { get; set; }
        [DataMember(Order = 2)]
        public string? ApplicantName { get; set; }
        [DataMember(Order = 3)]
        public string MobileNo { get; set; }
        [DataMember(Order = 4)]
        public DateTime? CreatedDate { get; set; }
        [DataMember(Order = 5)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 6)]
        public string? Status { get; set; }
        [DataMember(Order = 7)]
        public string CityName { get; set; }
        [DataMember(Order = 8)]
        public DateTime? ModifiedDate { get; set; }
        [DataMember(Order = 9)]
        public double OfferAmount { get; set; }
        [DataMember(Order = 10)]
        public string? AnchorCode { get; set; }
        [DataMember(Order = 11)]
        public long? CityId { get; set; }
        [DataMember(Order = 12)]
        public int? TotalCount { get; set; }
        [DataMember(Order = 13)]
        public long leadId { get; set;}
        [DataMember(Order = 14)]
        public string userId { get; set;}
    }
}
