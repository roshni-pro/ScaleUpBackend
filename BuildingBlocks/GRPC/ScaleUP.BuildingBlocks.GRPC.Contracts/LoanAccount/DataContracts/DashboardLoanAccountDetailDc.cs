using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class DashboardLoanAccountDetailDc
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
        public List<int> AnchorId { get; set; }
        [DataMember(Order = 6)]
        public List<string> CityName { get; set; }
        [DataMember(Order = 7)]
        public List<int> CityId { get; set; }
    }
    [DataContract]
    public class ScaleupleadDashboardReplyDc
    {

        [DataMember(Order = 1)]
        public double CreditLimitAmount { get; set; } = 0;
        [DataMember(Order = 2)]
        public double TotalSanctionedAmount { get; set; }
        [DataMember(Order = 3)]
        public double UtilizedAmount { get; set; }
        [DataMember(Order = 4)]
        public double LTDUtilizedAmount { get; set; }
        [DataMember(Order = 5)]
        public double TotalOutStanding { get; set; }
        [DataMember(Order = 6)]
        public double PrincipleOutstanding { get; set; }
        [DataMember(Order = 7)]
        public double InterestOutstanding { get; set; }
        [DataMember(Order = 8)]
        public double PenalOutStanding { get; set; }
        [DataMember(Order = 9)]
        public double OverdueInterestOutStanding { get; set; }
        [DataMember(Order = 10)]
        public double TotalRepayment { get; set; }

        [DataMember(Order = 11)]
        public double PrincipalRepaymentAmount { get; set; }
        [DataMember(Order = 12)]
        public double InterestRepaymentAmount { get; set; }
        [DataMember(Order = 13)]
        public double OverdueInterestPaymentAmount { get; set; }
        [DataMember(Order = 14)]
        public double PenalRePaymentAmount { get; set; }

        [DataMember(Order = 15)]
        public double BounceRePaymentAmount { get; set; }
        [DataMember(Order = 16)]
        public double ExtraPaymentAmount { get; set; }

        [DataMember(Order = 17)]
        public double PenalAmount { get; set; }
        [DataMember(Order = 18)]
        public DateTime Created { get; set; }

    }
    [DataContract]
    public class LoanAccountDashboardResponse
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public CreditLineInfoDC CreditLineInfo { get; set; }
        [DataMember(Order = 4)]
        public RepaymentsDC Repayments { get; set; }
        [DataMember(Order = 5)]
        public OutstandingDC Outstanding { get; set; }
        [DataMember(Order = 6)]
        public List<loanAccountDc> loanAccountData { get; set; }

    }
    [DataContract]
    public class loanAccountDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }

        [DataMember(Order = 2)]
        public string ProductType { get; set; }
        [DataMember(Order = 3)]
        public bool IsBlock { get; set; }
    }
    [DataContract]
    public class DashboardCreditLimitGrapth
    {
        [DataMember(Order = 1)]
        public string XValue { get; set; }
        [DataMember(Order = 2)]
        public double YValue { get; set; }
    }
    [DataContract]
    public class DashboardUtilizedAmounttGrapth
    {
        [DataMember(Order = 1)]
        public string XValue { get; set; }
        [DataMember(Order = 2)]
        public double YValue { get; set; }
    }
    [DataContract]
    public class DashboardAvailableLimitGrapth
    {
        [DataMember(Order = 1)]
        public string XValue { get; set; }
        [DataMember(Order = 2)]
        public double YValue { get; set; }
    }
    [DataContract]
    public class CreditLineInfoDC
    {
        [DataMember(Order = 1)]
        public double TotalSanctionedAmount { get; set; }
        [DataMember(Order = 2)]
        public double TotalCreditLimit { get; set; }
        [DataMember(Order = 3)]
        public double UtilizedAmount { get; set; }
        [DataMember(Order = 4)]
        public double AvailableLimit { get; set; }
        [DataMember(Order = 5)]
        public double AvailableLimitPercentage { get; set; }
        [DataMember(Order = 6)]

        public List<DashboardCreditLimitGrapth> DashboardCreditLimitGrapthDc { get; set; }
        [DataMember(Order = 7)]
        public List<DashboardUtilizedAmounttGrapth> DashboardUtilizedAmounttGrapthDc { get; set; }
        [DataMember(Order = 8)]

        public List<DashboardAvailableLimitGrapth> DashboardAvailableLimitGrapthDc { get; set; }


    }
    [DataContract]
    public class RepaymentsDC
    {
        [DataMember(Order = 1)]
        public double TotalPaidAmount { get; set; }
        [DataMember(Order = 2)]
        public double PrincipalAmount { get; set; }
        [DataMember(Order = 3)]
        public double InterestAmount { get; set; }
        [DataMember(Order = 4)]
        public double OverdueInterestAmount { get; set; }
        [DataMember(Order = 5)]
        public double PenalInterestAmount { get; set; }
        [DataMember(Order = 6)]
        public double BounceRePaymentAmount { get; set; }
        [DataMember(Order = 7)]
        public double PFpaid { get; set; }
        [DataMember(Order = 8)]
        public double GSTAmount { get; set; }
        [DataMember(Order = 9)]
        public List<DashboardCreditLimitGrapth> RepaymentsCreditLimitGrapthDc { get; set; }
        [DataMember(Order = 10)]
        public List<DashboardUtilizedAmounttGrapth> RepaymentsUtilizedAmounttGrapthDc { get; set; }
        [DataMember(Order = 11)]
        public List<DashboardAvailableLimitGrapth> RepaymentsAvailableLimitGrapthDc { get; set; }
    }
    [DataContract]
    public class OutstandingDC
    {
        [DataMember(Order = 1)]
        public double TotalOutstandingAmount { get; set; }
        [DataMember(Order = 2)]
        public double PrincipalAmount { get; set; }
        [DataMember(Order = 3)]
        public double InterestAmount { get; set; }
        [DataMember(Order = 4)]
        public double OverdueInterestAmount { get; set; }
        [DataMember(Order = 5)]
        public double PenalInterestAmount { get; set; }
        [DataMember(Order = 6)]
        public double PFOutStanding { get; set; }
        [DataMember(Order = 7)]
        public double Bounce { get; set; }
        [DataMember(Order = 8)]
        public List<DashboardCreditLimitGrapth> OutstandingCreditLimitGrapthDc { get; set; }
        [DataMember(Order = 9)]
        public List<DashboardUtilizedAmounttGrapth> OutstandingUtilizedAmounttGrapthDc { get; set; }
        [DataMember(Order = 10)]
        public List<DashboardAvailableLimitGrapth> OutstandingAvailableLimitGrapthDc { get; set; }
    }
    [DataContract]
    public class CreditLineDC
    {
        [DataMember(Order = 1)]
        public double Percentage { get; set; }
        [DataMember(Order = 2)]
        public double UtilizedAmount { get; set; }
        [DataMember(Order = 3)]
        public double TotalCreditLimit { get; set; }
    }
    [DataContract]
    public class TransactionDC
    {
        [DataMember(Order = 1)]
        public string TransactionNumber { get; set; }
        [DataMember(Order = 2)]
        public DateTime? TransactionDate { get; set; }
        [DataMember(Order = 3)]
        public string Status { get; set; }
    }

    [DataContract]
    public class scaleupDashboardResponseDc
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
        [DataMember(Order = 2)]
        public bool Staus { get; set; }
        [DataMember(Order = 3)]
        public leadDashboardResponseDc leadResponse { get; set; }
        [DataMember(Order = 4)]
        public AccountDashboardResponseDc AccResponseDc { get; set; }
        [DataMember(Order = 5)]
        public LoanDashboardResponseDc LoanResponseDc { get; set; }
        [DataMember(Order = 6)]
        public LoanAccountDashboardResponse DashboardResponse { get; set; }

    }
    [DataContract]
    public class AccountDashboardResponseDc
    {
        [DataMember(Order = 1)]
        public long CreditApproved { get; set; }
        [DataMember(Order = 2)]
        public long ApprovalPending { get; set; }
        [DataMember(Order = 3)]
        public long CreditRejected { get; set; }
        [DataMember(Order = 4)]
        public long OfferRejected { get; set; }
        [DataMember(Order = 5)]
        public long Rejected { get; set; }
        [DataMember(Order = 6)]
        public long TotalAccounts { get; set; }
        [DataMember(Order = 7)]
        public double CreditApprovalPercentage { get; set; }
    }
    [DataContract]
    public class LoanDashboardResponseDc
    {
        [DataMember(Order = 1)]
        public long DisbursementApproved { get; set; }
        [DataMember(Order = 2)]
        public long DisbursementPending { get; set; }
        [DataMember(Order = 3)]
        public long DisbursementRejected { get; set; }
        [DataMember(Order = 4)]
        public long OfferRejected { get; set; }
        [DataMember(Order = 5)]
        public long Rejected { get; set; }
        [DataMember(Order = 6)]
        public long TotalLoan { get; set; }
        [DataMember(Order = 7)]
        public double DisbursementApprovalPercentage { get; set; }
        [DataMember(Order = 8)]
        public long CreditLineApproved { get; set; }
        [DataMember(Order = 9)]
        public long CreditLinePending { get; set; }
        [DataMember(Order = 10)]
        public long CreditLineRejected { get; set; }
        [DataMember(Order = 11)]
        public long CreditLineOfferRejected { get; set; }
        [DataMember(Order = 12)]
        public long CLRejected { get; set; }
        [DataMember(Order = 13)]
        public long CreditLineTotalLoan { get; set; }
        [DataMember(Order = 14)]
        public double CreditLineApprovalPercentage { get; set; }
    }

    [DataContract]
    public class DateDC
    {
        [DataMember(Order = 1)]
        public DateTime StartDate { get; set; }
        [DataMember(Order = 2)]
        public DateTime EndDate { get; set; }
    }

    [DataContract]
    public class GetLedger_RetailerStatementDC
    {
        [DataMember(Order = 1)]
        public long loanAccountId { get; set; }
        [DataMember(Order = 2)]
        public DateTime FromDate { get; set; }
        [DataMember(Order = 3)]
        public DateTime ToDate { get; set; }
    }
}
