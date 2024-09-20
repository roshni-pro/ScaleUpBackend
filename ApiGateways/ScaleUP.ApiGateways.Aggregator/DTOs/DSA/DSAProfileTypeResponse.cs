using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs.DSA
{
    public class DSAProfileTypeResponse
    {
        public string DSAType { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public ConnectorPersonalDetailDc ConnectorPersonalDetail { get; set; }
        public DSAPersonalDetailDc DSAPersonalDetail { get; set; }
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
        public double PayoutPercentage { get; set; }

    }
    public class GetDSADashboardDetailResponse
    {
        public LeadOverviewDataDc LeadOverviewData { get; set; }
        public LoanOverviewDataDc LoanOverviewData { get; set; }
        public PayoutOverviewDataDc PayoutOverviewData { get; set; }
    }
    public class LeadOverviewDataDc
    {
        public long TotalLeads { get; set; }
        public long Pending { get; set; }
        public long Rejected { get; set; }
        public long Submitted { get; set; }
        public double SuccessRate { get; set; }
    }
    public class LoanOverviewDataDc
    {
        public long TotalLoans { get; set; }
        public long Pending { get; set; }
        public long Approved { get; set; }
        public long Rejected { get; set; }
        public double SuccessRate { get; set; }
    }
    public class PayoutOverviewDataDc
    {
        public double TotalDisbursedAmount { get; set; }
        public double PayoutAmount { get; set; }
    }

}
