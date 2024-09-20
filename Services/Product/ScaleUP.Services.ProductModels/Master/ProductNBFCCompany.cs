using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(ProductNBFCCompany))]
    public class ProductNBFCCompany : BaseAuditableEntity
    {
        public required long CompanyId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public required long ProductId { get; set; }
        [StringLength(100)]
        public string? ProcessingFeeType { get; set; }
        public required long ProcessingFee { get; set; }
        public required double AnnualInterestRate { get; set; }
        public required long PenaltyCharges { get; set; }
        public required long BounceCharges { get; set; }
        public required long PlatformFee { get; set; }
        public long? SanctionLetterDocId { get; set; }
        public string? SanctionLetterURL { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        [StringLength(20)]
        public string? CustomerAgreementType { get; set; } // Template/URL
        [StringLength(1000)]
        public string? CustomerAgreementURL { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        [StringLength(1000)]
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }

        [StringLength(500)]
        public string? TAPIKey { get; set; }
        [StringLength(500)]
        public string? TAPISecretKey { get; set; }

        [StringLength(200)]
        public string? TReferralCode { get; set; }
        public bool? IsInterestRateCoSharing { get; set; }
        public bool? IsPenaltyChargeCoSharing { get; set; }
        public bool? IsBounceChargeCoSharing { get; set; }
        public bool? IsPlatformFeeCoSharing { get; set; }
        [StringLength(50)]
        public string? DisbursementType { get; set; } //FullDisbursement,PFLessDisbursement

        [StringLength(100)]
        public string? ArrangementType { get; set; }//ArrangementTypeConstants
        public double? PFSharePercentage { get; set; }
        public long? Tenure { get; set; }
        public long? MaxBounceCharges { get; set; }
        public long? MaxPenaltyCharges { get; set; }
        public bool? IseSignEnable { get; set; }
        public DateTime? TokenCreatedDate { get; set; }
        public string? Token { get; set; }
    }
}
