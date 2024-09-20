using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA
{
    [DataContract]
    public class DSADashboardLeadResponse
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string Status { get; set; }
        [DataMember(Order = 3)]
        public string LeadCode { get; set; }
        [DataMember(Order = 4)]
        public DateTime CreatedDate { get; set; }
        [DataMember(Order = 5)]
        public string FullName { get; set; }
        [DataMember(Order = 6)]
        public string MobileNo { get; set; }
        [DataMember(Order = 7)]
        public int TotalRecords { get; set; }
        [DataMember(Order = 8)]
        public string ProfileImage { get; set; }
        [DataMember(Order = 9)]
        public string AgentUserId { get; set; }
        [DataMember(Order = 10)]
        public string AgentFullName { get; set; }
    }

    [DataContract]
    public class GetDSALeadPayoutDetailsResponse
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public double PayoutPercentage { get; set; }
    }
    [DataContract]
    public class DSADashboardPayoutResponse
    {
        [DataMember(Order = 1)]
        public double TotalDisbursedAmount { get; set; }
        [DataMember(Order = 2)]
        public double TotalPayoutAmount { get; set; }
        [DataMember(Order = 3)]
        public int TotalRecords { get; set; }
        [DataMember(Order = 4)]
        public List<LoanPayoutDetailDc> LoanPayoutDetailList { get; set; }
    }
    [DataContract]
    public class LoanPayoutDetailDc
    {
        [DataMember(Order = 1)]
        public string LoanId { get; set; }
        [DataMember(Order = 2)]
        public DateTime DisbursmentDate { get; set; }
        [DataMember(Order = 3)]
        public string FullName { get; set; }
        [DataMember(Order = 4)]
        public string Status { get; set; }
        [DataMember(Order = 5)]
        public string MobileNo { get; set; }
        [DataMember(Order = 6)]
        public double? DisbursmentAmount { get; set; }
        [DataMember(Order = 7)]
        public double PayoutAmount { get; set; }
        [DataMember(Order = 8)]
        public string? ProfileImage { get; set; }
        [DataMember(Order = 9)]
        public long LeadId { get; set; }

    }
}
