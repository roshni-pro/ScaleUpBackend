using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    public class LoanAccountDetailResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string LoanAccountNumber { get; set; }
        public string? ShopName { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string UserId { get; set; }
        public string MobileNumber { get; set; }
        public string CityName { get; set; }
        public string ProductType { get; set; }
        public string? LoanImage { get; set; }
        public bool IsAccountActive { get; set; }
        public bool IsBlock { get; set; }
        public string IsBlockComment { get; set; }
        public string NBFCIdentificationCode { get; set; }
        public string? ThirdPartyLoanCode { get; set; }
        public CreditLineInfo CreditLineInfo { get; set; }
        public Repayments Repayments { get; set; }
        public Outstanding Outstanding { get; set; }
        public CreditLine CreditLine { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
    public class CreditLineInfo
    {
        public double TotalSanctionedAmount { get; set; }
        public double TotalCreditLimit { get; set; }
        public double UtilizedAmount { get; set; }
        public double LTDUtilizedAmount { get; set; }
        public double AvailableLimit { get; set; }
        public double AvailableLimitPercentage { get; set; }
        public double PenalAmount { get; set; }
        public double ProcessingFee { get; set; }
    }
    public class Repayments
    {
        public double TotalPaidAmount { get; set; }
        public double PrincipalAmount { get; set; }
        public double InterestAmount { get; set; }
        public double PenalInterestAmount { get; set; }
        public double OverdueInterestAmount { get; set; }
        public double ExtraPaymentAmount { get; set; }
        public double BounceRePaymentAmount { get; set; }
    }
    public class Outstanding
    {
        public double TotalOutstandingAmount { get; set; }
        public double PrincipalAmount { get; set; }
        public double InterestAmount { get; set; }
        public double PenalInterestAmount { get; set; }
        public double OverdueInterestAmount { get; set; }
    }
    public class CreditLine
    {
        public double Percentage { get; set; }
        public double UtilizedAmount { get; set; }
        public double TotalCreditLimit { get; set; }
    }
    public class Transaction
    {
        public string TransactionNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
    }

    public class GetLoanAccountDetailDTO
    {
        public string AccountCode { get; set; }
        public string MobileNo { get; set; }
        public string CustomerName { get; set; }
        public string CityName { get; set; }
        public string ProductType { get; set; }
        public string? ShopName { get; set; }
        public string? LoanImage { get; set; }
        public bool IsAccountActive { get; set; }
        public bool IsBlock { get; set; }
        public string IsBlockComment { get; set; }
        public string NBFCIdentificationCode { get; set; }
        public string? ThirdPartyLoanCode { get; set; }
        public bool IsDefaultNBFC { get; set; }
        public double CreditLimitAmount { get; set; }
        public double TotalSanctionedAmount { get; set; }
        public double UtilizedAmount { get; set; }
        public double LTDUtilizedAmount { get; set; }
        public double PenalAmount { get; set; }
        public double PrincipalRepaymentAmount { get; set; }
        public double ExtraPaymentAmount { get; set; }
        public double InterestRepaymentAmount { get; set; }
        public double BounceRePaymentAmount { get; set; }
        public double PenalRePaymentAmount { get; set; }
        public double OverdueInterestPaymentAmount { get; set; }
        public double TotalRepayment { get; set; }
        public double PrincipleOutstanding { get; set; }
        public double InterestOutstanding { get; set; }
        public double PenalOutStanding { get; set; }
        public double OverdueInterestOutStanding { get; set; }
        public double TotalOutStanding { get; set; }
        public double ProcessingFee { get; set; }
    }
}
