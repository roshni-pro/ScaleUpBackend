using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class NBFCLoanAccountDetailResponseDTO
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public string LoanAccountNumber { get; set; }
        [DataMember(Order = 4)]
        public string? ShopName { get; set; }
        [DataMember(Order = 5)]
        public string UserName { get; set; }
        [DataMember(Order = 6)]
        public string PhoneNumber { get; set; }
        [DataMember(Order = 7)]
        public string UserId { get; set; }
        [DataMember(Order = 8)]
        public string MobileNumber { get; set; }
        [DataMember(Order = 9)]
        public string CityName { get; set; }
        [DataMember(Order = 10)]
        public string ProductType { get; set; }
        [DataMember(Order = 11)]
        public string? LoanImage { get; set; }
        [DataMember(Order = 12)]
        public bool IsAccountActive { get; set; }
        [DataMember(Order = 13)]
        public bool IsBlock { get; set; }
        [DataMember(Order = 14)]
        public string IsBlockComment { get; set; }
        [DataMember(Order = 15)]
        public string NBFCIdentificationCode { get; set; }
        [DataMember(Order = 16)]
        public string? ThirdPartyLoanCode { get; set; }
        [DataMember(Order = 17)]
        public CreditLineInfoDTO CreditLineInfo { get; set; }
        [DataMember(Order = 18)]
        public RepaymentsDTO Repayments { get; set; }
        [DataMember(Order = 19)]
        public OutstandingDTO Outstanding { get; set; }
        [DataMember(Order = 20)]
        public CreditLineDTO CreditLine { get; set; }
        [DataMember(Order = 21)]
        public List<TransactionDTO> Transactions { get; set; }
    }

    [DataContract]
    public class CreditLineInfoDTO
    {
        [DataMember(Order = 1)]
        public double TotalSanctionedAmount { get; set; }
        [DataMember(Order = 2)]
        public double TotalCreditLimit { get; set; }
        [DataMember(Order = 3)]
        public double UtilizedAmount { get; set; }
        [DataMember(Order = 4)]
        public double LTDUtilizedAmount { get; set; }
        [DataMember(Order = 5)]
        public double AvailableLimit { get; set; }
        [DataMember(Order = 6)]
        public double AvailableLimitPercentage { get; set; }
        [DataMember(Order = 7)]
        public double PenalAmount { get; set; }
        [DataMember(Order = 8)]
        public double ProcessingFee { get; set; }
    }
    [DataContract]
    public class RepaymentsDTO
    {
        [DataMember(Order = 1)]
        public double TotalPaidAmount { get; set; }
        [DataMember(Order = 2)]
        public double PrincipalAmount { get; set; }
        [DataMember(Order = 3)]
        public double InterestAmount { get; set; }
        [DataMember(Order = 4)]
        public double PenalInterestAmount { get; set; }
        [DataMember(Order = 5)]
        public double OverdueInterestAmount { get; set; }
        [DataMember(Order = 6)]
        public double ExtraPaymentAmount { get; set; }
        [DataMember(Order = 7)]
        public double BounceRePaymentAmount { get; set; }
    }
    [DataContract]
    public class OutstandingDTO
    {
        [DataMember(Order = 1)]
        public double TotalOutstandingAmount { get; set; }
        [DataMember(Order = 2)]
        public double PrincipalAmount { get; set; }
        [DataMember(Order = 3)]
        public double InterestAmount { get; set; }
        [DataMember(Order = 4)]
        public double PenalInterestAmount { get; set; }
        [DataMember(Order = 5)]
        public double OverdueInterestAmount { get; set; }
    }
    [DataContract]
    public class CreditLineDTO
    {
        [DataMember(Order = 1)]
        public double Percentage { get; set; }
        [DataMember(Order = 2)]
        public double UtilizedAmount { get; set; }
        [DataMember(Order = 3)]
        public double TotalCreditLimit { get; set; }
    }
    [DataContract]
    public class TransactionDTO
    {
        [DataMember(Order = 1)]
        public string TransactionNumber { get; set; }
        [DataMember(Order = 2)]
        public DateTime TransactionDate { get; set; }
        [DataMember(Order = 3)]
        public string Status { get; set; }
    }
}
