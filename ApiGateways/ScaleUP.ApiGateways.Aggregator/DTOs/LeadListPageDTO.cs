using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{

    public class LeadListPageListDTO
    {
        public List<LeadListPageDTO> leadListPageDTO { get; set; }
        public int TotalCount { get; set; }
    }
    public class LeadListPageDTO
    {
        public string UserId { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ScreenName { get; set; }
        public int SequenceNo { get; set; }
        public string? AlternatePhoneNo { get; set; }
        public string? EmailId { get; set; }
        public long LeadId { get; set; }
        public DateTime? LastModified{ get; set; }  
        public string? LeadCode { get; set; }
        public double? CreditScore { get; set; }
        public string? Status { get; set; }
        public string? BusinessName { get; set; }
        public string? AnchorName { get; set; }
        public string? UniqueCode { get; set; }
        public long? CityId { get; set; }
        public string? CityName { get; set; }
        public string? LeadGenerator { get; set; }
        public string? LeadConvertor { get; set; }
        public double? CreditLimit { get; set; }
        public string? Loan_app_id { get; set; }
        public string? Partner_Loan_app_id { get; set; }
        public string? ProductCode { get; set; }
        public string? RejectionReasons  { get; set;}
        public string? OwnershipType { get; set; }
        public string? BusEntityType { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public double? OrderAmount { get; set; }
        public string? SurrogateType { get; set; }
        public long? AvgMonthlyBuying { get; set; }
        public long? VintageDays { get; set; }
        public string? RejectMessage { get; set; }
    }

    public class DSALeadListPageListDTO
    {
        public List<DSALeadListPageDTO> leadListPageDTO { get; set; }
        public int TotalCount { get; set; }
    }
    public class DSALeadListPageDTO
    {
        public string UserId { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ScreenName { get; set; }
        public int SequenceNo { get; set; }
        public string? AlternatePhoneNo { get; set; }
        public string? EmailId { get; set; }
        public long LeadId { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LeadCode { get; set; }
        public string? Status { get; set; }
        public string? CompanyName { get; set; }
        public string? AnchorName { get; set; }
        public string? UniqueCode { get; set; }
        public long? CityId { get; set; }
        public string? CityName { get; set; }
        public string? LeadGenerator { get; set; }
        public string? LeadConvertor { get; set; }
        public string? ProductCode { get; set; }
        public string? RejectionReasons { get; set; }
        public string? FirmType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AnchorCompanyId { get; set; }
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
        public string? profileType { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; }
        public string? WorkingLocation { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public long? AddressId { get; set; }
        public string? SelfieImage { get; set; }
        public string? DSALeadCode { get; set; }
        public long? WorkingLocationId { get; set; }

    }
}
