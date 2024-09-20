using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class AddUpdateAnchorProductConfigRequest
    {
        [DataMember(Order = 1)]
        public long? Id { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
        [DataMember(Order = 4)]
        public string? ProductName { get; set; }
        [DataMember(Order = 5)]
        public string? ProductType { get; set; }
        [DataMember(Order = 6)]
        public string ProcessingFeePayableBy { get; set; }
        [DataMember(Order = 7)]
        public string ProcessingFeeType { get; set; }
        [DataMember(Order = 8)]
        public double ProcessingFeeRate { get; set; }
        [DataMember(Order = 9)]
        public string? AnnualInterestPayableBy { get; set; } //CreditLine
        [DataMember(Order = 10)]
        public double DelayPenaltyRate { get; set; }
        [DataMember(Order = 11)]
        public long BounceCharge { get; set; }
        [DataMember(Order = 12)]
        public long? CreditDays { get; set; } //CreditLine
        [DataMember(Order = 13)]
        public long? DisbursementTAT { get; set; } //CreditLine
        [DataMember(Order = 14)]
        public double? AnnualInterestRate { get; set; } //BusinessLoan
        [DataMember(Order = 15)]
        public long? MinTenureInMonth { get; set; } //BusinessLoan
        [DataMember(Order = 16)]
        public long? MaxTenureInMonth { get; set; } //BusinessLoan
        [DataMember(Order = 17)]
        public double? EMIRate { get; set; }
        [DataMember(Order = 18)]
        public double? EMIProcessingFeeRate { get; set; }
        [DataMember(Order = 19)]
        public long? EMIBounceCharge { get; set; }
        [DataMember(Order = 20)]
        public double? EMIPenaltyRate { get; set; }
        [DataMember(Order = 21)]
        public double? CommissionPayout { get; set; } //BusinessLoan
        [DataMember(Order = 22)]
        public double? ConsiderationFee { get; set; } //BusinessLoan
        [DataMember(Order = 23)]
        public double? DisbursementSharingCommission { get; set; } //BusinessLoan
        [DataMember(Order = 24)]
        public long? MinLoanAmount { get; set; } //BusinessLoan
        [DataMember(Order = 25)]
        public long? MaxLoanAmount { get; set; } //BusinessLoan
        [DataMember(Order = 26)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 27)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 28)]
        public string? AgreementURL { get; set; }
        [DataMember(Order = 29)]
        public long? AgreementDocId { get; set; }
        [DataMember(Order = 30)]
        public double? OfferMaxRate { get; set; }
        [DataMember(Order = 31)]
        public long? CustomCreditDays { get; set; }
        [DataMember(Order = 32)]
        public string? BlackSoilReferralCode { get; set; } //new 
        [DataMember(Order = 33)]

        public List<CompanyEMIOptionsRequest>? CompanyEMIOptions { get; set; }
        [DataMember(Order = 34)]
        public List<CompanyCreditDaysRequest>? CompanyCreditDays { get; set; }
    }

    [DataContract]
    public class CompanyEMIOptionsRequest
    {
        [DataMember(Order = 1)]
        public long? ProductAnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long EMIOptionMasterId { get; set; }
        [DataMember(Order = 3)]
        public bool IsActive { get; set; }
        [DataMember(Order = 4)]
        public bool IsDeleted { get; set; }
    }

    [DataContract]
    public class CompanyCreditDaysRequest
    {
        [DataMember(Order = 1)]
        public long? ProductAnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long CreditDaysMasterId { get; set; }
        [DataMember(Order = 3)]
        public bool IsActive { get; set; }
        [DataMember(Order = 4)]
        public bool IsDeleted { get; set; }
    }
    [DataContract]
    public class AddUpdateNBFCProductConfigRequest
    {
        [DataMember(Order = 1)]
        public long? Id { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
        [DataMember(Order = 4)]
        public string? ProcessingFeeType { get; set; }
        [DataMember(Order = 5)]
        public long ProcessingFee { get; set; }
        [DataMember(Order = 6)]
        public double InterestRate { get; set; }
        [DataMember(Order = 7)]
        public long PenaltyCharges { get; set; }
        [DataMember(Order = 8)]
        public long BounceCharges { get; set; }
        [DataMember(Order = 9)]
        public long PlatformFee { get; set; }
        [DataMember(Order = 10)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 11)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 12)]
        public string? AgreementURL { get; set; }
        [DataMember(Order = 13)]
        public long? AgreementDocId { get; set; }
        [DataMember(Order = 14)]
        public string? CustomerAgreementType { get; set; }
        [DataMember(Order = 15)]
        public string? CustomerAgreementURL { get; set; }
        [DataMember(Order = 16)]
        public long? CustomerAgreementDocId { get; set; }
        [DataMember(Order = 17)]
        public long? SanctionLetterDocId { get; set; }
        [DataMember(Order = 18)]
        public string? SanctionLetterURL { get; set; }
        [DataMember(Order = 19)]
        public bool? IsInterestRateCoSharing { get; set; }
        [DataMember(Order = 20)]
        public bool? IsPenaltyChargeCoSharing { get; set; }
        [DataMember(Order = 21)]
        public bool? IsBounceChargeCoSharing { get; set; }
        [DataMember(Order = 22)]
        public bool? IsPlatformFeeCoSharing { get; set; }
        [DataMember(Order = 23)]
        public string? DisbursementType { get; set; }
    }
}
