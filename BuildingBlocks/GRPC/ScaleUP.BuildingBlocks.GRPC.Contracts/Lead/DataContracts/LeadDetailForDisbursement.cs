using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadDetailForDisbursement
    {
        [DataMember(Order = 1)]
        public Leaddc leadinfo { get; set; }
        [DataMember(Order = 2)]
        public DisbursementDetail DisbursementDetail { get; set; }
        [DataMember(Order = 3)]
        public List<EMISchedule> EMISchedule { get; set; }
        [DataMember(Order = 4)]
        public SaveLoanAccountCompanyLeadRequestDC LoanAccountCompanyLead { get; set; }
        [DataMember(Order = 5)]
        public string nbfcUTR { get; set; }
        [DataMember(Order = 6)]
        public List<SlabConfigDC> productSlabConfig { get; set; }
        [DataMember(Order = 7)]
        public DateTime DisbursementDate { get; set; }
        [DataMember(Order = 8)]
        public double? InsuranceAmount { get; set; }
        [DataMember(Order = 9)]
        public double? OtherCharges { get; set; }
        [DataMember(Order = 10)]
        public double? brokenPeriodinterestAmount { get; set; }
        [DataMember(Order = 11)]
        public DateTime FirstEMIDate { get; set; }

    }
    [DataContract]
    public class DisbursementDetail
    {
        [DataMember(Order = 1)]
        public double? LoanAmount { get; set; }
        [DataMember(Order = 2)]
        public double? LoanInterestAmount { get; set; }
        [DataMember(Order = 3)]
        public double? InterestRate { get; set; }
        [DataMember(Order = 4)]
        public double? MonthlyEMI { get; set; }
        [DataMember(Order = 5)]
        public double? ProcessingFeeRate { get; set; }
        [DataMember(Order = 6)]
        public double? ProcessingFeeAmount { get; set; }
        [DataMember(Order = 7)]
        public double? PFDiscount { get; set; }
        [DataMember(Order = 8)]
        public int? Tenure { get; set; }
        [DataMember(Order = 9)]
        public double? GST { get; set; }
        [DataMember(Order = 10)]
        public double? ProcessingFeeTax { get; set; }
        [DataMember(Order = 11)]
        public string LoanId { get; set; }
        [DataMember(Order = 12)]
        public string CompanyIdentificationCode { get; set; }
        [DataMember(Order = 13)]
        public double? Commission { get; set; }
        [DataMember(Order = 14)]
        public string? CommissionType { get; set; }
        [DataMember(Order = 15)]
        public string? PFType { get; set; }
        [DataMember(Order = 16)]
        public long? NBFCCompanyId { get; set; }
        [DataMember(Order = 17)]
        public double NBFCInterest { get; set; }
        [DataMember(Order = 18)]
        public double NBFCProcessingFee { get; set; }
        [DataMember(Order = 19)]
        public string NBFCProcessingFeeType { get; set; }
        [DataMember(Order = 20)]
        public double? NBFCBounce { get; set; }
        [DataMember(Order = 21)]
        public double? NBFCPenal { get; set; }
        [DataMember(Order = 22)]
        public double? Bounce { get; set; }
        [DataMember(Order = 23)]
        public double? Penal { get; set; }
        [DataMember(Order = 24)]
        public string ArrangementType { get; set; }
    }

    [DataContract]
    public class EMISchedule
    {
        [DataMember(Order = 1)]
        public DateTime DueDate { get; set; }
        [DataMember(Order = 2)]
        public double OutStandingAmount { get; set; }
        [DataMember(Order = 3)]
        public double Prin { get; set; }
        [DataMember(Order = 4)]
        public double InterestAmount { get; set; }
        [DataMember(Order = 5)]
        public double EMIAmount { get; set; }
        [DataMember(Order = 6)]
        public double PrincipalAmount { get; set; }
    }

    [DataContract]
    public class SlabConfigDC
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public string SlabType { get; set; } //SlabTypeConstants
        [DataMember(Order = 4)]
        public double MinLoanAmount { get; set; }
        [DataMember(Order = 5)]
        public double MaxLoanAmount { get; set; }
        [DataMember(Order = 6)]
        public double MinValue { get; set; }
        [DataMember(Order = 7)]
        public double MaxValue { get; set; }
        [DataMember(Order = 8)]
        public string ValueType { get; set; }
        [DataMember(Order = 9)]
        public bool IsFixed { get; set; }
        [DataMember(Order = 10)]
        public double? SharePercentage { get; set; } //(Over and Above Share %)
    }
    [DataContract]
    public class NBFCDisbursementPostdc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public DateTime DisbursementDate { get; set; }
        [DataMember(Order = 3)]
        public string UTR { get; set; }
        [DataMember(Order = 4)]
        //public string IdentificationCode { get; set; }
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 5)] 
        public double? InsuranceAmount { get; set; }
        [DataMember(Order = 6)]
        public double? OtherCharges { get; set; }
        [DataMember(Order = 7)]
        public DateTime FirstEMIDate { get; set; }
    }
    //long leadid, string UTR,DateTime DisbursementDate
}
