using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadListPageReply
    {
        [DataMember(Order = 1)]
        public List<LeadListDetail> LeadListDetails { get; set; } 
        [DataMember(Order = 2)]
        public int TotalCount { get; set; }
    }

    [DataContract]
    public class LeadListDetail
    {
        [DataMember(Order = 1)]
        public long Id { get; set; }
        [DataMember(Order = 2)]
        public string MobileNo { get; set; }
        [DataMember(Order = 3)]
        public DateTime CreatedDate { get; set; }
        [DataMember(Order = 4)]
        public string? ScreenName { get; set; }
        [DataMember(Order = 5)]
        public int SequenceNo { get; set; }
        [DataMember(Order = 6)]
        public long ActivityId { get; set; }
        [DataMember(Order = 7)]
        public long? SubActivityId { get; set; }
        [DataMember(Order = 8)]
        public string UserId { get; set; }
        [DataMember(Order = 9)]
        public DateTime? LastModified { get; set; }
        [DataMember(Order = 10)]
        public string? LeadCode { get; set; }
        [DataMember(Order = 11)]
        public double? CreditScore { get; set; }
        [DataMember(Order = 12)]
        public string? Status { get; set; }
        [DataMember(Order = 13)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 14)]
        public string? CustomerName { get; set; }
        [DataMember(Order = 15)]
        public string? EmailId { get; set; }
        [DataMember(Order = 16)]
        public string? AlternatePhoneNumber { get; set; }
        [DataMember(Order = 17)]
        public string? UniqueCode { get; set; }
        [DataMember(Order = 18)]
        public long? CityId { get; set; }
        [DataMember(Order = 19)]
        public string? LeadGenerator { get; set; }
        [DataMember(Order = 20)]
        public string? LeadConvertor { get; set; }
        [DataMember(Order = 21)]
        public double? CreditLimit { get; set; }
        [DataMember(Order = 22)]
        public string? Loan_app_id { get; set; }
        [DataMember(Order = 23)]
        public string? Partner_Loan_app_id { get; set; }
        [DataMember(Order = 24)]
        public string? ProductCode { get; set; }
        [DataMember(Order = 25)]
        public string? ArthmateResponse { get; set; }
        [DataMember(Order = 26)]
        public bool IsActive { get; set; }
        [DataMember(Order = 27)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 28)]
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
        [DataMember(Order = 29)]
        public long? VintageDays { get; set; }
        [DataMember(Order = 30)]
        public string? RejectionMessage { get; set; }
        [DataMember(Order = 31)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 32)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 33)]
        public string? CibilReport { get; set; }
        [DataMember(Order = 34)]
        public long ProductId { get; set; }
        [DataMember(Order = 35)]
        public long? OfferCompanyId { get; set; }
    }
}
