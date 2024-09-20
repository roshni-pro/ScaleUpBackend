using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    [DataContract]
    public class TransactionSettlementDTO
    {
        public string? TransactionStatus { get; set; }
        public double? TrnAmount { get; set; }
        public double? PaidAmount { get; set; }
        public double? Amounts { get; set; }
    }



    public class TransactionSettlementDetailDTO
    {
        public long ParentAccountTransactionId { get; set; } //ParentAccountTransaction
        public string TransactionNo { get; set; }
        public string ModeOfPaymentSourceType { get; set; }
        public double OrderPrincipalAmount { get; set; }
        public double OrderPrincipleAlreadyPaidAmount { get; set; }
        public double PrincipalAmount { get; set; }
        public double InterestAmount { get; set; }
        public double OverdueAmount { get; set; }
        public double ExtraAmount { get; set; }
        public double PenalAmount { get; set; }
        public string? username { get; set; }
        public string PaymentRefNo { get; set; }
        public DateTime PaymentDate { get; set; }

        public double? InterestAmountOfOrder { get; set; }
        public double? InterestPaymentAlreadyPaidAmount { get; set; }
        public double? OverdueInterestAmountOfOrder { get; set; }
        public double? OverduePaymentAlreadyPaidAmount { get; set; }
        public double? DelayPenalityAmountOfOrder { get; set; }
        public double? PenalPaymentAlreadyPaidAmount { get; set; }
    }



    public class TransactionSettlementByManualDTO
    {
        public long ParentAccountTransactionId { get; set; } //ParentAccountTransaction
        public string TransactionNo { get; set; }
        public string ModeOfPaymentSourceType { get; set; }
        public double SettleAmount { get; set; }
        public string? username { get; set; }
        public string PaymentRefNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public double ExtraPaymentAmount { get; set; }
        public double OverduePaymentAmount { get; set; }   //Penallty Charges
        public double PenalPaymentAmount { get; set; }      // Bounce Charges

    }

    public class TransactionSettlementByManualReplytDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class PaymentUploadDc
    {
        public string ReferenceNo { set; get; }  //Cl TransactionId
        public string BankReferenceNo { set; get; } //Bank TransactionId
        public double Amount { set; get; }
        public double? BounceAmount { set; get; }
        public string Comment { set; get; }
        public string ReasonCode { get; set; }
        public string UMRN { get; set; }
    }

    public class DuePaymentDc
    {
        public long AccountId { set; get; }
        public string TrasanctionId { set; get; }
        public DateTime DueDate { set; get; }
        public double Amount { set; get; }
        public DateTime TransactionDate { set; get; }
        public string status { set; get; }
        public double BounceCharge { get; set; }
        public double GstRate { get; set; }
        public string UMRN { get; set; }
        public string TrasanctionType { get; set; }
    }

    public class GetUploadedPaymentDc
    {
        public long Id { get; set; }
        public int Status { set; get; }   // Success 1 , Failed 0
        public string ReferenceNo { set; get; }  //Cl TransactionId
        public string BankReferenceNo { set; get; }  //Bank TransactionId
        public double Amount { set; get; }
        public string ModifiedUser { set; get; }


    }

    public class GetAllTransactionDC
    {
        public string TransactionId { get; set; }
        public double totalAmount { set; get; }
        public double PaidAmount { set; get; }
        public double paneltyAmount { set; get; }
        public long WithdrawlId { get; set; }
        public double ExtraPaymentPaidAmount { get; set; }
        public double OverduePaymentPaidAmount { get; set; }   //Penallty Charges
        public double PenalPaymentPaidAmount { get; set; }      // Bounce Charges
    }
}