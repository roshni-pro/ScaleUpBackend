
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class DisbursementDashboardDataResponseDc
    {
        [DataMember(Order = 1)]
        public GraphData disbursementGrapth { get; set; }
        [DataMember(Order = 2)]
        public GraphData utilizedAmounttGrapth { get; set; }
        [DataMember(Order = 3)]
        public GraphData overDueAmountGrapth { get; set; }
        [DataMember(Order = 4)]
        public GraphData LoanBook { get; set; }
        [DataMember(Order = 5)]
        public GraphData SCRevenueGraph { get; set; }
        [DataMember(Order = 6)]
        public GraphData BLRevenueGraph { get; set; }
        [DataMember(Order = 7)]
        public GraphData BLoverDueAmountGrapth { get; set; }
        //[DataMember(Order = 4)]
        //public GraphData SCOutstandingAmountGrapth { get; set; }
        //[DataMember(Order = 5)]
        //public double AvgUtilization { get; set; }
        //[DataMember(Order = 6)]
        //public double TotalOutstanding { get; set; }
        //[DataMember(Order = 7)]
        //public double SCTotalLimit { get; set; }
        //[DataMember(Order = 10)]
        //public double AvgDailyOutstanding { get; set; }
        //[DataMember(Order = 11)]
        //public double AvgDailyLimit { get; set; }
        //[DataMember(Order = 12)]
        //public List<DashboardDPD> SCDPD { get; set; }
        //[DataMember(Order = 13)]
        //public GraphData BLOutstandingAmountGrapth { get; set; }
        //[DataMember(Order = 15)]
        //public double BLTotalLimit { get; set; }
        //[DataMember(Order = 16)]
        //public double BLOutstanding { get; set; }
        //[DataMember(Order = 17)]
        //public double SCOutstanding { get; set; }
        //[DataMember(Order = 18)]
        //public List<DashboardDPD> BLDPD { get; set; }
        //[DataMember(Order = 19)]
        //public int SCActiveCustomer { get; set; }
        //[DataMember(Order = 20)]
        //public int BLActiveCustomer { get; set; }
        //[DataMember(Order = 21)]
        //public double TotalRevenue { get; set; }
        //[DataMember(Order = 22)]
        //public double AvgSCRevenue { get; set; }
        //[DataMember(Order = 23)]
        //public double AvgBLRevenue { get; set; }
        //[DataMember(Order = 24)]
        //public double TotalOverDueAmount { get; set; }
        //[DataMember(Order = 25)]
        //public RetentionPercentage RetentionPercentage { get; set; }
        //[DataMember(Order = 26)]
        //public CibilData cibilData { get; set; }
        //[DataMember(Order = 27)]
        //public CohortData cohortData { get; set; }
    }

    [DataContract]
    public class HopDashboardRequestDc
    {
        [DataMember(Order = 1)]
        public string ProductType { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public DateTime FromDate { get; set; }
        [DataMember(Order = 4)]
        public DateTime ToDate { get; set; }
        [DataMember(Order = 5)]
        public List<int> NbfcCompanyId { get; set; }
        [DataMember(Order = 6)]
        public List<string> CityName { get; set; }
        [DataMember(Order = 7)]
        public List<int> CityId { get; set; }
    }
    [DataContract]
    public class HopDashboardDc
    {
        [DataMember(Order = 1)]
        public double LTDUtilizedAmount { get; set; }
        [DataMember(Order = 2)]
        public double CreditLimitAmount { get; set; }
        [DataMember(Order = 3)]
        public double OverDueAmount { get; set; }
        [DataMember(Order = 4)]
        public double OutStanding { get; set; }
        [DataMember(Order = 5)]
        public DateTime TransactionDate { get; set; }
    }
    [DataContract]
    public class HopDashboardRevenueDc
    {
        [DataMember(Order = 1)]
        public double ScaleupShare { get; set; }
        [DataMember(Order = 2)]
        public DateTime TransactionDate { get; set; }
    }
    [DataContract]
    public class HopDashboardOverDueDaysDc
    {
        [DataMember(Order = 1)]
        public long NoOfInvoice { get; set; }
        [DataMember(Order = 2)]
        public double OverdueAmount { get; set; }
        [DataMember(Order = 3)]
        public long Aging { get; set; }
        [DataMember(Order = 4)]
        public string ProductType { get; set; }
        [DataMember(Order = 5)]
        public DateTime TransactionDate { get; set; }
    }

    [DataContract]
    public class DashboardDPD
    {
        [DataMember(Order = 1)]
        public string Range { get; set; }
        [DataMember(Order = 2)]
        public double Amount { get; set; }
    }
    [DataContract]
    public class GraphData
    {
        [DataMember(Order = 1)]
        public List<string> XValue { get; set; } = new List<string>();
        [DataMember(Order = 2)]
        public List<int> YValue { get; set; } = new List<int>();
    }

    [DataContract]
    public class CohortData
    {
        [DataMember(Order = 1)]
        public string InitiateSubmittedTime { get; set; }
        [DataMember(Order = 2)]
        public string SubmittedToAllApprovedTime { get; set; }
        [DataMember(Order = 3)]
        public string AllApprovedToSendToLosTime { get; set; }
        [DataMember(Order = 4)]
        public string SendToLosToOfferAcceptTime { get; set; }
        [DataMember(Order = 5)]
        public string OfferAcceptToAgreementTime { get; set; }
        [DataMember(Order = 6)]
        public string AgreementToAgreementAcceptTime { get; set; }
        [DataMember(Order = 7)]
        public double InitiateSubmittedTimeInHours { get; set; }
        [DataMember(Order = 8)]
        public double SubmittedToAllApprovedTimeInHours { get; set; }
        [DataMember(Order = 9)]
        public double AllApprovedToSendToLosTimeInHours { get; set; }
        [DataMember(Order = 10)]
        public double SendToLosToOfferAcceptTimeInHours { get; set; }
        [DataMember(Order = 11)]
        public double OfferAcceptToAgreementTimeInHours { get; set; }
        [DataMember(Order = 12)]
        public double AgreementToAgreementAcceptTimeInHours { get; set; }
    }
    [DataContract]
    public class RetentionPercentage
    {
        [DataMember(Order = 1)]
        public double MOMRetentionPercentage { get; set; }
        [DataMember(Order = 2)]
        public double QOQRetentionPercentage { get; set; }
    }
    [DataContract]
    public class CibilData
    {
        [DataMember(Order = 1)]
        public double CibilGreaterPercentage { get; set; }
        [DataMember(Order = 2)]
        public double CibilLessPercentage { get; set; }
        [DataMember(Order = 3)]
        public double CibilZeroPercentage { get; set; }
    }
    [DataContract]
    public class HopDashboardAllDataDc
    {
        [DataMember(Order = 1)]
        public double LTDUtilizedAmount { get; set; } = 0;
        [DataMember(Order = 2)]
        public double OutStanding { get; set; }
        [DataMember(Order = 3)]
        public double ODOutStanding { get; set; }
        [DataMember(Order = 4)]
        public double ScaleupShareAmount { get; set; }
        [DataMember(Order = 5)] 
        public DateTime TransactionDate { get; set; }
    }
    [DataContract]
    public class LoanPaymentsResultDC
    {
        [DataMember(Order = 1)]
        public string CustomerName { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public List<LoanPaymentsResultListDC> loanPaymentsResultList { get; set; }
        [DataMember(Order = 4)]
        public string HtmlContent { get; set; }
        [DataMember(Order = 5)]
        public string InvoicePdfPath { get; set; }

    }
    [DataContract]
    public class LoanPaymentsResultListDC
    {
        [DataMember(Order = 1)]
        public string InvoiceNo { get; set; }
        [DataMember(Order = 2)]
        public string WithdrawalId { get; set; }
        [DataMember(Order = 3)]
        public string FieldInfo { get; set; }
        [DataMember(Order = 4)]
        public double FieldOldValue { get; set; }
        [DataMember(Order = 5)]
        public double FieldNewValue { get; set; }
    }

    [DataContract]
    public class HopDisbursementDashboardResponseDc
    {
        [DataMember(Order = 1)]
        public double AvgUtilization { get; set; }
        [DataMember(Order = 2)]
        public double TotalOutstanding { get; set; }
        [DataMember(Order = 3)]
        public double SCTotalLimit { get; set; }
        [DataMember(Order = 4)]
        public GraphData SCRevenueGraph { get; set; }
        [DataMember(Order = 5)]
        public double AvgDailyOutstanding { get; set; }
        [DataMember(Order = 6)]
        public double AvgDailyLimit { get; set; }
        [DataMember(Order = 7)]
        public List<DashboardDPD> SCDPD { get; set; }
        [DataMember(Order = 8)]
        public double BLTotalLimit { get; set; }
        [DataMember(Order = 9)]
        public double BLOutstanding { get; set; }
        [DataMember(Order = 10)]
        public double SCOutstanding { get; set; }
        [DataMember(Order = 11)]
        public List<DashboardDPD> BLDPD { get; set; }
        [DataMember(Order = 12)]
        public int SCActiveCustomer { get; set; }
        [DataMember(Order = 13)]
        public int BLActiveCustomer { get; set; }
        [DataMember(Order = 14)]
        public double TotalRevenue { get; set; }
        [DataMember(Order = 15)]
        public double AvgSCRevenue { get; set; }
        [DataMember(Order = 16)]
        public double AvgBLRevenue { get; set; }
        [DataMember(Order = 17)]
        public double TotalOverDueAmount { get; set; }
        [DataMember(Order = 18)]
        public RetentionPercentage RetentionPercentage { get; set; }
        [DataMember(Order = 19)]
        public CibilData cibilData { get; set; }
        [DataMember(Order = 20)]
        public CohortData cohortData { get; set; }
    }
}
