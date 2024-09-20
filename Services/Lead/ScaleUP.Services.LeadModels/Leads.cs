using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.LeadModels
{
    [Table(nameof(Leads))]
    public class Leads : BaseAuditableEntity
    {
        public required long ProductId { get; set; }
        [StringLength(100)]
        public required string ProductCode { get; set; }
        public required string UserName { get; set; }
        [StringLength(10)]
        public required string MobileNo { get; set; }
        [StringLength(100)]
        public required string LeadCode { get; set; }
        public long? OfferCompanyId { get; set; }
        public double? CreditLimit { get; set; }
        public double? ProcessingFee { get; set; }
        public double? CreditScore { get; set; }
        public string CibilReport { get; set; }

        public ICollection<CompanyLead> CompanyLeads { get; set; }

        public ICollection<LeadActivityMasterProgresses> LeadActivityMasterProgresses { get; set; }

        public ICollection<LeadOffers> LeadOffers { get; set; }
        public bool? IsAgreementAccept { get; set; }
        public DateTime? AgreementDate { get; set; }
        [StringLength(50)]
        public string? Status { get; set; }
        public ICollection<LeadBankDetail> LeadBankDetail { get; set; }
        public long? CityId { get; set; }
        public string? ApplicantName { get; set; }
        [StringLength(500)]
        public string? LeadGenerator { get; set; }
        [StringLength(500)]
        public string? LeadConverter { get; set; }

        //New add for loan interest rate 

        public double? InterestRate { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }
}
