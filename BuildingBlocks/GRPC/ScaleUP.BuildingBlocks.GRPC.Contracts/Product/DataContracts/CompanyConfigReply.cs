using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class CompanyConfigReply
    {
        [DataMember(Order = 1)]
        public AnchorCompanyConfig AnchorCompanyConfig { get; set; }
        [DataMember(Order = 2)]
        public NBFCCompanyConfig NBFCCompanyConfig { get; set; }

        [DataMember(Order = 3)]
        public bool IsDefaultNBFC { get; set; }
        [DataMember(Order = 4)]
        public List<ProductSlabConfigResponse> NBFCSlabConfigs { get; set; }
        [DataMember(Order = 5)]
        public List<ProductSlabConfigResponse> AnchorSlabConfigs { get; set; }

    }

    //eSign

    [DataContract]
    public class eSignReply
    {
        [DataMember(Order = 1)]
        public string AgreementURL { get; set; }
        //[DataMember(Order = 2)]
        //public string DSAAgreeURL { get; set; }

        [DataMember(Order = 2)]
        public long ProductId { get; set; }

    }




    [DataContract]
    public class NBFCCompanyConfig
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProcessingFee { get; set; }
        [DataMember(Order = 3)]
        public double InterestRate { get; set; }
        [DataMember(Order = 4)]
        public long PenaltyCharges { get; set; }
        [DataMember(Order = 5)]
        public long BounceCharges { get; set; }
        [DataMember(Order = 6)]
        public long PlatformFee { get; set; }
        [DataMember(Order = 7)]
        public long CustomerAgreementDocId { get; set; }

        [DataMember(Order = 8)]
        public string CustomerAgreementURL { get; set; }
        [DataMember(Order = 9)]
        public string? ProcessingFeeType { get; set; }
        [DataMember(Order = 10)]
        public long ProductNBFCCompanyId { get; set; }
        [DataMember(Order = 11)]
        public string? CustomerAgreementType { get; set; }
        [DataMember(Order = 12)]
        public string DisbursementType { get; set; }
        [DataMember(Order = 13)]
        public long? Tenure { get; set; }
        [DataMember(Order = 14)]
        public long ProductId { get; set; }
        [DataMember(Order = 15)]
        public bool IsInterestRateCoSharing { get; set; }
        [DataMember(Order = 16)]
        public bool IsBounceChargeCoSharing { get; set; }
        [DataMember(Order = 17)]
        public bool IsPenaltyChargeCoSharing { get; set; }
    }

    [DataContract]
    public class AnchorCompanyConfig
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string ProcessingFeePayableBy { get; set; }
        [DataMember(Order = 3)]
        public string ProcessingFeeType { get; set; }
        [DataMember(Order = 4)]
        public double ProcessingFeeRate { get; set; }
        [DataMember(Order = 5)]
        public string? AnnualInterestPayableBy { get; set; } //CreditLine
        [DataMember(Order = 6)]
        public long? AgreementDocId { get; set; }
        //public string? AnnualInterestFeeType { get; set; } //CreditLine
        [DataMember(Order = 7)]
        public long ProductAnchorCompanyId { get; set; }
        //public double? TransactionFeeRate { get; set; } //CreditLine
        [DataMember(Order = 8)]
        public double DelayPenaltyRate { get; set; }
        [DataMember(Order = 9)]
        public long BounceCharge { get; set; }
        [DataMember(Order = 10)]
        public long? CreditDays { get; set; } //CreditLine
        [DataMember(Order = 11)]
        public long? DisbursementTAT { get; set; } //CreditLine
        [DataMember(Order = 12)]
        public double? AnnualInterestRate { get; set; } //BusinessLoan
        [DataMember(Order = 13)]
        public long? MinTenureInMonth { get; set; } //BusinessLoan
        [DataMember(Order = 14)]
        public long? MaxTenureInMonth { get; set; } //BusinessLoan
        public double? EMIRate { get; set; } //CreditLine
        [DataMember(Order = 15)]
        public double? EMIProcessingFeeRate { get; set; } //CreditLine
        [DataMember(Order = 16)]
        public long? EMIBounceCharge { get; set; } //CreditLine
        [DataMember(Order = 17)]
        public double? EMIPenaltyRate { get; set; } //CreditLine
        [DataMember(Order = 18)]
        public double? CommissionPayout { get; set; } //BusinessLoan
        [DataMember(Order = 19)]
        public double? ConsiderationFee { get; set; } //BusinessLoan
        [DataMember(Order = 20)]
        public double? DisbursementSharingCommission { get; set; } //BusinessLoan
        [DataMember(Order = 21)]
        public long? MinLoanAmount { get; set; } //BusinessLoan
        [DataMember(Order = 22)]
        public long? MaxLoanAmount { get; set; } //BusinessLoan
        [DataMember(Order = 23)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 24)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 25)]
        public string? AgreementURL { get; set; }

        [DataMember(Order = 26)]
        public double? MaxInterestRate { get; set; }
        [DataMember(Order = 27)]
        public bool? IseSignEnable { get; set; }

        [DataMember(Order = 28)]
        public double? PlatFormFee { get; set; }
        [DataMember(Order = 29)]
        public long ProductId { get; set; }
    }

}
