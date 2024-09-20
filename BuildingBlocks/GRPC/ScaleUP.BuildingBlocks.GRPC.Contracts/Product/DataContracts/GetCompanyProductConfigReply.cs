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
    public class GetCompanyProductConfigReply
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public List<GetCompanyProductConfig> GetCompanyProductConfigList { get; set; }
    }
    [DataContract]
    public class GetCompanyProductConfig
    {
        [DataMember(Order = 1)]
        public long ProductCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
        public double GstRate { get; set; }
        [DataMember(Order = 4)]
        public long? AgreementDocId { get; set; }
        [DataMember(Order = 5)]
        public double ProcessingFee { get; set; }
        [DataMember(Order = 6)]
        public double DelayPenaltyFee { get; set; }
        [DataMember(Order = 7)]
        public double BounceCharges { get; set; }
        [DataMember(Order = 8)]
        public double ProcessingCreditDays { get; set; }
        [DataMember(Order = 9)]
        public List<int> CreditDays { get; set; }
        [DataMember(Order = 10)]
        public long ProductId { get; set; }
        [DataMember(Order = 11)]
        public double AnnualInterestRate { get; set; }
        [DataMember(Order = 12)]
        public double PlateFormFee  { get; set; }
        [DataMember(Order = 13)]
        public string? ProcessingFeeType { get; set; }
        [DataMember(Order = 14)]
        public string? AnnualInterestPayableBy { get; set; } //CreditLine
        [DataMember(Order = 15)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 16)]
        public DateTime? AgreementEndDate { get; set; }
        //public double? TransactionFeeRate { get; set; } //CreditLine
        [DataMember(Order = 17)]
        public string AgreementUrl { get; set; }
        [DataMember(Order = 18)]
        public string ProductName { get; set; }
    }
}
