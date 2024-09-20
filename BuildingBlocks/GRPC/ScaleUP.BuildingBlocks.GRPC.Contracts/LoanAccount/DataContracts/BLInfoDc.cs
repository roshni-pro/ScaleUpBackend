using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class BLDetailsDc
    {
        [DataMember(Order =1)]
        public long? LeadMasterId { get; set; }
        [DataMember(Order =2)]
        public long LoanAccountId { get; set; }
        [DataMember(Order =3)]
        public BLProfileDetailDc ProfileDetail {  get; set; }
        [DataMember(Order =4)]
        public BLInfoDc BLInfo { get; set; }
        [DataMember(Order =5)]
        public BLRepaymentDc Repayment { get; set; }
        [DataMember(Order =6)]
        public BLOutstandingDc Outstanding { get; set; }
    }
    [DataContract]
    public class BLInfoDc
    {
        [DataMember(Order =1)]
        public double? loan_int_amt { get; set; }
        [DataMember(Order =2)]
        public double? sanction_amount { get; set; }
        [DataMember(Order =3)]
        public string loan_int_rate { get; set; }
        [DataMember(Order =4)]
        public double? processing_fees_amt { get; set; }
        [DataMember(Order =5)]
        public double? processing_fees_perc { get; set; }
        [DataMember(Order =6)]
        public double? PfDiscount { get; set; }
        [DataMember(Order =7)]
        public double? net_disbur_amt { get; set; }
        [DataMember(Order =8)]
        public double? gst_on_pf_amt { get; set; }
        [DataMember(Order =9)]
        public double? gst_on_pf_perc { get; set; }
        [DataMember(Order =10)]
        public double? InsuranceAmount { get; set; }
        [DataMember(Order =11)]
        public double? Othercharges { get; set; }
        [DataMember(Order =12)]
        public double? emi_amount { get; set; }
        [DataMember(Order =13)]
        public string tenure { get; set; }
        [DataMember(Order =14)]
        public double? broken_period_int_amt { get; set; }
        [DataMember(Order =15)]
        public DateTime? first_inst_date { get; set; }
        [DataMember(Order =16)]
        public DateTime? final_approve_date { get; set; }
        [DataMember(Order =17)]
        public string status { get; set; }
        [DataMember(Order =18)]
        public double? conv_fees { get; set; }
    }

    [DataContract]
    public class BLProfileDetailDc
    {
        [DataMember(Order =1)]
        public string AccountCode { get; set; }
        [DataMember(Order =2)]
        public string CustomerName { get; set; }
        [DataMember(Order =3)]
        public string ProductType { get; set; }
        [DataMember(Order =4)]
        public string MobileNo { get; set; }
        [DataMember(Order = 5)]
        public string ShopName { get; set; }
        [DataMember(Order = 6)]
        public string CityName { get; set; }
        [DataMember(Order = 7)]
        public string NBFCIdentificationCode { get; set; }
        [DataMember(Order = 8)]
        public string ThirdPartyLoanCode { get; set; }
        [DataMember(Order = 9)]
        public string loan_id { get; set; }
        [DataMember(Order = 10)]
        public string first_name { get; set; }
    }
    [DataContract]
    public class BLRepaymentDc
    {
        [DataMember(Order = 1)]
        public double TotalRepayment { get; set; }
        [DataMember(Order = 2)]
        public double PrincipleAmount { get; set; }
        [DataMember(Order = 3)]
        public double InterestAmount { get; set; }
        [DataMember(Order = 4)]
        public double BounceCharges { get; set; }
        [DataMember(Order = 5)]
        public double PenalCharges { get; set; }
        [DataMember(Order = 6)]
        public double OverdueRePaymentAmount { get; set; }

    }

    [DataContract]
    public class BLOutstandingDc
    {
        [DataMember(Order = 1)]
        public double TotalOutstanding { get; set; }
        [DataMember(Order = 2)]
        public double PrincipleOutstanding { get; set; }
        [DataMember(Order = 3)]
        public double InterestOutstanding { get; set; }
        [DataMember(Order = 4)]
        public double BounceCharges { get; set; }
        [DataMember(Order = 5)]
        public double PenalCharges { get; set; }
        [DataMember(Order = 6)]
        public double OverdueInterestOutStanding { get; set; }

    }

    [DataContract]
    public class GetBLAccountDetailDTO
    {
        [DataMember(Order = 1)]
        public double TotalOutStanding { get; set; }
        [DataMember(Order = 2)]
        public double PrincipleOutstanding { get; set; }
        [DataMember(Order = 3)]
        public double InterestOutstanding { get; set; }
        [DataMember(Order = 4)]
        public double PenalOutStanding { get; set; }
        [DataMember(Order = 5)]
        public double BounceOutStanding { get; set; }
        [DataMember(Order = 6)]
        public double OverdueInterestOutStanding { get; set; }
        [DataMember(Order = 7)]
        public double TotalRepayment { get; set; }
        [DataMember(Order = 8)]
        public double PrincipalRepaymentAmount { get; set; }
        [DataMember(Order = 9)]
        public double InterestRepaymentAmount { get; set; }
        [DataMember(Order = 10)]
        public double PenalRePaymentAmount { get; set; }
        [DataMember(Order = 11)]
        public double BounceRePaymentAmount { get; set; }
        [DataMember(Order = 12)]
        public double OverdueRePaymentAmount { get; set; }
    }

}