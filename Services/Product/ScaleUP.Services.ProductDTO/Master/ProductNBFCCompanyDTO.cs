using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductDTO.Master
{
    public class AddUpdateNBFCProductConfigDTO
    {
        public long? Id { get; set; }
        public long CompanyId { get; set; }
        public long ProductId { get; set; }
        public string? ProcessingFeeType { get; set; }
        public long ProcessingFee { get; set; }
        public double InterestRate { get; set; }
        public long PenaltyCharges { get; set; }
        public long BounceCharges { get; set; }
        public long PlatformFee { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        public string? CustomerAgreementType { get; set; }
        public string? CustomerAgreementURL { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        public long? SanctionLetterDocId { get; set; }
        public string? SanctionLetterURL { get; set; }
        public bool? IsInterestRateCoSharing { get; set; }
        public bool? IsPenaltyChargeCoSharing { get; set; }
        public bool? IsBounceChargeCoSharing { get; set; }
        public bool? IsPlatformFeeCoSharing { get; set; }
        public string? DisbursementType { get; set; }
        public string? ArrangementType { get; set; }//ArrangementTypeConstants
        public double? PFSharePercentage { get; set; }
        public long? Tenure { get; set; }
        public long? MaxBounceCharges { get; set; }
        public long? MaxPenaltyCharges { get; set; }
        public bool? IseSignEnable { get; set; }
        public List<ProductSlabConfigDTO>? ProductSlabConfigs { get; set; }
    }

    public class GetNBFCProductConfigDTO
    {
        public long? Id { get; set; }
        public long CompanyId { get; set; }
        public long ProductId { get; set; }
        public string? ProcessingFeeType { get; set; }
        public long ProcessingFee { get; set; }
        public double AnnualInterestRate { get; set; }
        public long PenaltyCharges { get; set; }
        public long BounceCharges { get; set; }
        public long PlatformFee { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        public string? CustomerAgreementType { get; set; }
        public string? CustomerAgreementURL { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        public long? SanctionLetterDocId { get; set; }
        public string? SanctionLetterURL { get; set; }
        public bool? IsInterestRateCoSharing { get; set; }
        public bool? IsPenaltyChargeCoSharing { get; set; }
        public bool? IsBounceChargeCoSharing { get; set; }
        public bool? IsPlatformFeeCoSharing { get; set; }
        public string? DisbursementType { get; set; }
        public string? ArrangementType { get; set; }//ArrangementTypeConstants
        public double? PFSharePercentage { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public long? Tenure { get; set; }
        public long? MaxBounceCharges { get; set; }
        public long? MaxPenaltyCharges { get; set; }
        public bool? IseSignEnable { get; set; }
        public List<ProductSlabConfigDTO>? ProductSlabConfigs { get; set; }
    }
    public class NBFCProductListDTO
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string? ProcessingFeeType { get; set; }
        public long ProcessingFee { get; set; }
        public double InterestRate { get; set; }
        public long PenaltyCharges { get; set; }
        public long BounceCharges { get; set; }
        public long PlatformFee { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        public string? CustomerAgreementType { get; set; }
        public string? CustomerAgreementURL { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        public long? SanctionLetterDocId { get; set; }
        public string? SanctionLetterURL { get; set; }
        public bool? IsInterestRateCoSharing { get; set; }
        public bool? IsPenaltyChargeCoSharing { get; set; }
        public bool? IsBounceChargeCoSharing { get; set; }
        public bool? IsPlatformFeeCoSharing { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? DisbursementType { get; set; }
    }
}
