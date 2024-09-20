using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class BLAccountRequestDc
    {
        [DataMember(Order = 1)]
        public List<long>? AnchorId { get; set; }
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
        public long? CityId { get; set; }
        [DataMember(Order = 9)]
        public string? Status { get; set; }
        [DataMember(Order = 10)]
        public List<string>? UserIds { get; set; }
        [DataMember(Order = 11)]
        public bool IsDSA { get; set; }
        [DataMember(Order = 12)]
        public string? Role { get; set; }
        [DataMember(Order = 13)]
        public long? NbfcCompanyId { get; set; }

        [DataMember(Order = 14)]
        public string? UserType { get; set; }

    }
    [DataContract]
    public class BLAccountResponseDC
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
        public string? CityName { get; set; }
        [DataMember(Order = 8)]
        public DateTime? ModifiedDate { get; set; }
        [DataMember(Order = 9)]
        public double OfferAmount { get; set; }
        [DataMember(Order = 10)]
        public string? AnchorCode { get; set; }
        [DataMember(Order = 11)]
        public string? LoanAppId { get; set; }
        [DataMember(Order = 12)]
        public string? PartnerLoanAppId { get; set; }
        [DataMember(Order = 13)]
        public long? CityId { get; set; }
        [DataMember(Order = 14)]
        public int? TotalCount { get; set; }
        [DataMember(Order = 15)]
        public long leadId { get; set; }
        [DataMember(Order = 16)]
        public string userId { get; set; }

        [DataMember(Order = 17)]
        public string? LoanId { get; set; }
        [DataMember(Order = 18)]
        public string NBFCname { get; set; }
        [DataMember(Order = 19)]
        public long? NbfcCompanyId { get; set; }
    }
}
