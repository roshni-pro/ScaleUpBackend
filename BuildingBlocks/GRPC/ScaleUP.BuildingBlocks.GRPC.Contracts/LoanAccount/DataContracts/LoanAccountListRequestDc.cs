using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class LoanAccountListRequestDc
    {
        [DataMember(Order = 1)]
        public string? ProductType { get; set; }
        [DataMember(Order = 2)]
        public string? Keyword { get; set; }
        [DataMember(Order = 3)]
        public List<long> AnchorId { get; set; }
        [DataMember(Order = 4)]
        public int? Status { get; set; } //block, active, inactive
        [DataMember(Order = 5)]
        public DateTime FromDate { get; set; }
        [DataMember(Order = 6)]
        public DateTime ToDate { get; set; }
        [DataMember(Order = 7)]
        public string? CityName { get; set; }
        [DataMember(Order = 8)]
        public int Min { get; set; }
        [DataMember(Order = 9)]
        public int Max { get; set; }
        [DataMember(Order = 10)]
        public int Skip { get; set; }
        [DataMember(Order = 11)]
        public int Take { get; set; }
        [DataMember(Order = 12)]
        public List<string>? UserIds { get; set; }
        [DataMember(Order = 13)]
        public bool IsDSA { get; set; }
        [DataMember(Order = 14)]
        public List<long>? leadIds { get; set; }
        [DataMember(Order = 15)]
        //public string? Role { get; set; }
        public long NbfcCompanyId { get; set; }
        [DataMember(Order = 16)]
        public string? UserType { get; set; }
    }

    [DataContract]
    public class LoanAccountListResponseDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public string UserId { get; set; }
        [DataMember(Order = 4)]
        public string CustomerName { get; set; }
        [DataMember(Order = 5)]
        public string LeadCode { get; set; }
        [DataMember(Order = 6)]
        public string MobileNo { get; set; }
        [DataMember(Order = 7)]
        public long? NBFCCompanyId { get; set; }
        [DataMember(Order = 8)]
        public long? AnchorCompanyId { get; set; }
        [DataMember(Order = 9)]
        public double? CreditScore { get; set; }
        [DataMember(Order = 10)]
        public DateTime? DisbursalDate { get; set; }
        [DataMember(Order = 11)]
        public double? loan_amount { get; set; }
        [DataMember(Order = 12)]
        public long? LoanAccountId { set; get; }
        [DataMember(Order = 13)]
        public string? CityName { get; set; }
        [DataMember(Order = 14)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 15)]
        public bool Status { get; set; } //IsAccountActive

        [DataMember(Order = 16)]
        public bool IsBlock { get; set; }
        [DataMember(Order = 17)]
        public string? AccountStatus { get; set; }
        [DataMember(Order = 18)]
        public string? Loan_app_id { get; set; }
        [DataMember(Order = 19)]
        public string? Partner_loan_app_id { get; set; }
        [DataMember(Order = 20)]
        public double? OutStandingAmount { get; set; }
        [DataMember(Order = 21)]
        public string? business_name { get; set; }
        [DataMember(Order = 22)]
        public int? TotalCount { get; set; }
        [DataMember(Order = 23)]
        public string NBFCname { get; set; }
    }
}
