
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ScaleUP.Services.ProductDTO.Master
{
    [DataContract]
    public class ProductCompanyDTO
    {
        public long? Id { get; set; }
        public long CompanyId { get; set; }
        public double GstRate { get; set; }
        public double ConvenienceFee { get; set; }
        public double ProcessingFee { get; set; }
        public double DelayPenaltyFee { get; set; } 
        public double BounceCharges { get; set; }
        public double ProcessingCreditDays { get; set; }
        public int CreditDays { get; set; }
        public long? ProductId { get; set; }
        public string ProductName { get; set; }
        public string? BlackSoilReferralCode { get; set; } //new 

    }
    public class AddUpdateAnchorProductConfigDTO
    {
        public long? Id { get; set; }
        public long CompanyId { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public string ProcessingFeePayableBy { get; set; }
        public string ProcessingFeeType { get; set; }
        public double ProcessingFeeRate { get; set; }
        public string? AnnualInterestPayableBy { get; set; } //CreditLine
        //public string? TransactionFeeType { get; set; } //CreditLine
        //public double? TransactionFeeRate { get; set; } //CreditLine
        public double DelayPenaltyRate { get; set; }
        public long BounceCharge { get; set; }
        public long? CreditDays { get; set; } //CreditLine
        public long? DisbursementTAT { get; set; } //CreditLine
        public double? AnnualInterestRate { get; set; } //BusinessLoan
        public long? MinTenureInMonth { get; set; } //BusinessLoan
        public long? MaxTenureInMonth { get; set; } //BusinessLoan
        public double? EMIRate { get; set; }
        public double? EMIProcessingFeeRate { get; set; }
        public long? EMIBounceCharge { get; set; }
        public double? EMIPenaltyRate { get; set; }
        public double? CommissionPayout { get; set; } //BusinessLoan
        public double? ConsiderationFee { get; set; } //BusinessLoan
        public double? DisbursementSharingCommission { get; set; } //BusinessLoan
        public long? MinLoanAmount { get; set; } //BusinessLoan
        public long? MaxLoanAmount { get; set; } //BusinessLoan
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        public double? OfferMaxRate { get; set; }
        public long? CustomCreditDays { get; set; }
        public string? BlackSoilReferralCode { get; set; } //new 

        public bool? IseSignEnable { get; set; } //new 
        public double? MaxInterestRate { get; set; } //new 
        public double? PlatFormFee { get; set; } //new 
        public List<CompanyEMIOptionsDTO>? CompanyEMIOptions { get; set; }
        public List<CompanyCreditDaysDTO>? CompanyCreditDays { get; set; }
        //public List<ProductSlabConfigDTO>? ProductSlabConfigs{ get; set; }
    }
    public class CompanyEMIOptionsDTO
    {
        public long? ProductAnchorCompanyId { get; set; }
        public long EMIOptionMasterId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class CompanyCreditDaysDTO
    {
        public long? ProductAnchorCompanyId { get; set; }
        public long CreditDaysMasterId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class ProductSlabConfigDTO
    {
        public string SlabType { get; set; } //SlabTypeConstants
        public double MinLoanAmount { get; set; }
        public double MaxLoanAmount { get; set; }
        public string ValueType { get; set; } // ValueTypeConstants
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double? SharePercentage { get; set; } //(Over and Above Share %)
        public bool IsFixed { get; set; }
    }
    public class AnchorProductListDTO
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string ProcessingFeePayableBy { get; set; }
        public string ProcessingFeeType { get; set; }
        public double ProcessingFeeRate { get; set; }
        public string? AnnualInterestPayableBy { get; set; } //CreditLine
        //public string? TransactionFeeType { get; set; } //CreditLine
        //public double? TransactionFeeRate { get; set; } //CreditLine
        public double DelayPenaltyRate { get; set; }
        public long BounceCharge { get; set; }
        public long? CreditDays { get; set; } //CreditLine
        public long? DisbursementTAT { get; set; } //CreditLine
        public double? AnnualInterestRate { get; set; } //BusinessLoan
        public long? MinTenureInMonth { get; set; } //BusinessLoan
        public long? MaxTenureInMonth { get; set; } //BusinessLoan
        public double? EMIRate { get; set; }
        public double? EMIProcessingFeeRate { get; set; }
        public long? EMIBounceCharge { get; set; }
        public double? EMIPenaltyRate { get; set; }
        public double? CommissionPayout { get; set; } //BusinessLoan
        public double? ConsiderationFee { get; set; } //BusinessLoan
        public double? DisbursementSharingCommission { get; set; } //BusinessLoan
        public long? MinLoanAmount { get; set; } //BusinessLoan
        public long? MaxLoanAmount { get; set; } //BusinessLoan
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        public double? OfferMaxRate { get; set; }
        public long? CustomCreditDays { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? BlackSoilReferralCode { get; set; } //new 

    }
}
