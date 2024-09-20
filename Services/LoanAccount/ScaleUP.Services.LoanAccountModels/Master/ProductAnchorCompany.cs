using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    [Table(nameof(ProductAnchorCompany))]
    public class ProductAnchorCompany : BaseAuditableEntity
    {
        public required long CompanyId { get; set; }
        public required long ProductId { get; set; }
        [StringLength(100)]
        public required string ProcessingFeePayableBy { get; set; }
        [StringLength(100)]
        public required string ProcessingFeeType { get; set; }
        public required double ProcessingFeeRate { get; set; }
        [StringLength(100)]
        public string? AnnualInterestPayableBy { get; set; } //CreditLine
        public double? AnnualInterestRate { get; set; } 
        public required double DelayPenaltyRate { get; set; }
        public required long BounceCharge { get; set; }
        public long? DisbursementTAT { get; set; } //CreditLine
        public long? MinTenureInMonth { get; set; } //BusinessLoan
        public long? MaxTenureInMonth { get; set; } //BusinessLoan
        public double? EMIRate { get; set; } //CreditLine
        public double? EMIProcessingFeeRate { get; set; } //CreditLine
        public long? EMIBounceCharge { get; set; } //CreditLine
        public double? EMIPenaltyRate { get; set; } //CreditLine
        public double? CommissionPayout { get; set; } //BusinessLoan
        public double? ConsiderationFee { get; set; } //BusinessLoan
        public double? DisbursementSharingCommission { get; set; } //BusinessLoan
        public long? MinLoanAmount { get; set; } //BusinessLoan
        public long? MaxLoanAmount { get; set; } //BusinessLoan
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        [StringLength(1000)]
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        public double? OfferMaxRate { get; set; }
        public long? CustomCreditDays { get; set; }
        //public ICollection<CompanyEMIOptions> CompanyEMIOptions { get; set; } //CreditLine
        //public ICollection<CompanyCreditDays> CompanyCreditDays { get; set; } //CreditLine

        [StringLength(200)]
        public string? BlackSoilReferralCode { get; set; }

    }
}
