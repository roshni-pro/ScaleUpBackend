using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadCompanyGenerateOfferNewDTO
    {
        [DataMember(Order = 1)]
        public long NbfcCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long LeadOfferId { get; set; }
        [DataMember(Order = 3)]
        public string LeadOfferStatus { get; set; }
        [DataMember(Order = 4)]
        public double? CreditLimit { get; set; }
        [DataMember(Order = 5)]
        public string ComapanyName { get; set; } //CompanyName

        [DataMember(Order = 6)]
        public string? LeadOfferErrorMessage { get; set; }
        [DataMember(Order = 7)]
        public long LeadId { get; set; }
        [DataMember(Order = 8)]
        public double? Processingfee { get; set; }
        [DataMember(Order = 9)]
        public List<LeadCompanyGenerateOfferSubactivityNewDTO> SubactivityList { get; set; }
        [DataMember(Order = 10)]
        public double? ProcessingfeeTax { get; set; }
        [DataMember(Order = 11)]
        public double? MinInterestRate { get; set; }
        [DataMember(Order = 12)]
        public double? MaxInterestRate { get; set; }
        [DataMember(Order = 13)]
        public double SanctonAmount { get; set; }
        [DataMember(Order = 14)]
        public double MonthlyEMI { get; set; }
        [DataMember(Order = 15)]
        public double LoanInterestAmount { get; set; }
        [DataMember(Order = 16)]
        public int Tenure { get; set; }
        [DataMember(Order = 17)]
        public double? InterestRate { get; set; }
        [DataMember(Order = 18)]
        public bool? OfferApprove { get; set; }
        [DataMember(Order = 19)]
        public double? ProcessingfeeRate { get; set; }
        [DataMember(Order = 20)]
        public double? GSTRate { get; set; }
        [DataMember(Order = 21)]
        public double? PfDiscount { get; set; }
        [DataMember(Order = 22)]
        public string? UserName { get; set; }
        [DataMember(Order = 23)]
        public DateTime? NbfcCreatedDate { get; set; }
        [DataMember(Order = 24)]
        public string? NbfcUserId { get; set; }
        [DataMember(Order = 25)]
        public string? PFType { get; set; } //amount/percent

        [DataMember(Order = 26)]
        public double? PFAfterDiscount { get; set; }
        [DataMember(Order = 27)]
        public double? PFRevisedTax { get; set; }
        [DataMember(Order = 28)]
        public string? LoanId { get; set; }
        [DataMember(Order = 29)]
        public DateTime? OfferInitiateDate { get; set; }

    }
    [DataContract]
    public class LeadCompanyGenerateOfferSubactivityNewDTO
    {
        [DataMember(Order = 1)]
        public long LeadNBFCSubActivityId { get; set; }
        [DataMember(Order = 2)]
        public long ActivityMasterId { get; set; }
        [DataMember(Order = 3)]
        public long? SubActivityMasterId { get; set; }
        [DataMember(Order = 4)]
        public int Sequence { get; set; }
        [DataMember(Order = 5)]
        public string ActivityName { get; set; }
        [DataMember(Order = 6)]
        public string SubActivityName { get; set; }
        [DataMember(Order = 7)]
        public string Status { get; set; }
        [DataMember(Order = 8)]
        public List<LeadCompanyGenerateOfferApiNewDTO> ApiList { get; set; }
    }
    [DataContract]
    public class LeadCompanyGenerateOfferApiNewDTO
    {
        [DataMember(Order = 1)]
        public long? ApiId { get; set; }
        [DataMember(Order = 2)]
        public string Code { get; set; }
        [DataMember(Order = 3)]
        public string? ApiUrl { get; set; }
        [DataMember(Order = 4)]
        public int? Sequence { get; set; }
        [DataMember(Order = 5)]
        public string? ApiStatus { get; set; }
        [DataMember(Order = 6)]
        public string? Request { get; set; }
        [DataMember(Order = 7)]
        public string? Response { get; set; }
    }
    [DataContract]
    public class LeadCompanyGenerateOfferNewDc
    {
        [DataMember(Order = 1)]
        public long NBFCCompanyId { get; set; } //

        [DataMember(Order = 2)]
        public string ActivityName { get; set; } //

        [DataMember(Order = 3)]
        public string ComapanyName { get; set; }
        [DataMember(Order = 4)]
        public long ActivityMasterId { get; set; }
        [DataMember(Order = 5)]
        public long? SubActivityMasterId { get; set; }
        [DataMember(Order = 6)]
        public int SubActivitySequence { get; set; }
        [DataMember(Order = 7)]
        public string Code { get; set; }
        [DataMember(Order = 8)]
        public string SubActivityStatus { get; set; }
        [DataMember(Order = 9)]
        public long LeadOfferId { get; set; }
        [DataMember(Order = 10)]
        public long LeadNBFCSubActivityId { get; set; }

        [DataMember(Order = 11)]
        public string LeadOfferStatus { get; set; }
        [DataMember(Order = 12)]
        public double? CreditLimit { get; set; }
        [DataMember(Order = 13)]
        public string? ErrorMessage { get; set; }
        [DataMember(Order = 14)]
        public long? APIId { get; set; }
        [DataMember(Order = 15)]
        public string? ApiCode { get; set; }
        [DataMember(Order = 16)]
        public string? ApiStatus { get; set; }
        [DataMember(Order = 17)]
        public string? APIUrl { get; set; }
        [DataMember(Order = 18)]
        public int? APISequence { get; set; }
        [DataMember(Order = 19)]
        public string? Request { get; set; }
        [DataMember(Order = 20)]
        public string? Response { get; set; }

    }
    [DataContract]
    public class GenerateOfferStatusPostDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string ProductType { get; set; }
        [DataMember(Order = 3)]
        public string? UserType { get; set; }
    }
    [DataContract]
    public class GenerateOfferStatusPost
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string ProductType { get; set; }
        [DataMember(Order = 3)]
        public ProductCompanyConfigDc ProductCompanyConfigDcs { get; set; }
        [DataMember(Order = 4)]
        public List<ProductSlabConfigResponse> ProductSlabConfigs { get; set; }

        [DataMember(Order = 5)]
        public List<GetProductNBFCConfigResponseDc> GetProductNBFCConfigResponseDcs { get; set; }
        [DataMember(Order = 6)]
        public double GST { get; set; }
    }

    [DataContract]
    public class Loandetaildc
    {
        [DataMember(Order = 1)]
        public double LoanAmount { get; set; }
        [DataMember(Order = 2)]
        public double RateOfInterest { get; set; }
        [DataMember(Order = 3)]
        public int Tenure { get; set; }
        [DataMember(Order = 4)]
        public double monthlyPayment { get; set; } //MonthlyEMI
        [DataMember(Order = 5)]
        public double loanIntAmt { get; set; }
        [DataMember(Order = 6)]
        public double processing_fee { get; set; }
        [DataMember(Order = 7)]
        public double ProcessingfeeTax { get; set; }
        [DataMember(Order = 8)]
        public string CompanyIdentificationCode { get; set; }
        [DataMember(Order = 9)]
        public double? ProcessingFeeRate { get; set; }
        [DataMember(Order = 10)]
        public double? GST { get; set; }
        [DataMember(Order = 11)]
        public string? UserName { get; set; }
        [DataMember(Order = 12)]
        public DateTime? CreatedDate { get; set; }
        [DataMember(Order = 13)]
        public string? RejectionReason { get; set; }

        [DataMember(Order = 14)]
        public string PFType { get; set; } // amount/percent

        [DataMember(Order = 15)]
        public double? Commission { get; set; }
        [DataMember(Order = 16)]
        public string? CommissionType { get; set; } // amount/percent

        [DataMember(Order = 17)]
        public DateTime? offerInitiateDate { get; set; } // createdate from leadoffer table
        [DataMember(Order = 18)]
        public double? NBFCBounce { get; set; }
        [DataMember(Order = 19)]
        public double? NBFCPenal { get; set; }
        [DataMember(Order = 20)]
        public double NBFCInterest { get; set; }
        [DataMember(Order = 21)]
        public double NBFCProcessingFee { get; set; }
        [DataMember(Order = 22)]
        public string NBFCProcessingFeeType { get; set; }
        [DataMember(Order = 23)]
        public double? Bounce { get; set; }
        [DataMember(Order = 24)]
        public double? Penal { get; set; }
        [DataMember(Order = 25)]
        public string ArrangementType { get; set; }
    }
}
