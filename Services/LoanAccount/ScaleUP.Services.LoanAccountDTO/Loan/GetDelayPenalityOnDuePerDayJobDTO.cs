using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    [DataContract]
    public class GetDelayPenalityOnDuePerDayJobDTO
    {
        public string? transactionReqNo { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public long? AccountTransactionId { get; set; }
        public double? DelayPenalityCharge { get; set; }
        public double? PenalityGstCharge { get; set; }
        public double? DebitAmount { get; set; }
        public double? CreditAmount { get; set; }
        public double? GstRate { get; set; }
        public long LoanAccountId { get; set; }
        public string? PayableBy { get; set; }
        public long TransactionStatusesID_Overdue { get; set; }
        public long TransactionTypeId_OrderPlacement { get; set; }
        public long TransactionTypeId_PenaltyCharges { get; set; }
        public long TransactionDetailHeadId_DelayPenalty { get; set; }

        //public string? TransactionTypesCode { get; set; }
        //public string? TransactionDetailHeadCode { get; set; }
    }


    public class GetOutstandingTransactionsDTO
    {
        public long Id { get; set; }
        public string transactionReqNo { get; set; }
        public double? DelayPenaltyRate { get; set; }
        public double? GstRate { get; set; }
        public string? PayableBy { get; set; }
        public string? InvoiceNo { get; set; }
        public long? WithdrawlId { get; set; }
        public double? PricipleAmount { get; set; }
        public double? InterestAmount { get; set; }
        public double? PenaltyChargesAmount { get; set; }
        public double? PaymentAmount { get; set; }
        public double? ExtraPaymentAmount { get; set; }
        public double? InterestPaymentAmount { get; set; }
        public double? BouncePaymentAmount { get; set; }
        public double? PenalPaymentAmount { get; set; }
        public double? Outstanding { get; set; }
        public long PaneltyTxnId { get; set; }
        public double DelayPenalityAmount { get; set; }
        public double DelayPenalityGstAmount { get; set; }
        public long? InvoiceId { get; set; }
        public double? OverduePaymentAmount { get; set; }
        public double? OverdueInterestAmount { get; set; }



    }

    public class OverDueInterestCharge
    {
        public long LoanAccountId { get; set; }
        public double PricipleAmount { get; set; }
        public double InterestRate { get; set; }
        public double PrincipleOutstanding { get; set; }
        public double PaymentAmount { get; set; }
        public long AccountTransactionId { get; set; }
        public string NBFCIdentificationCode { get; set; }
    }
    public class OverDueDelayPenaltyRateDc
    {
        public long LoanAccountId { get; set; }
        public double PricipleAmount { get; set; }
        public double DelayPenaltyRate { get; set; }
        public double PrincipleOutstanding { get; set; }
        public double PaymentAmount { get; set; }
        public long AccountTransactionId { get; set; }
        public string NBFCIdentificationCode { get; set; }
    }


    public class Ledger_RetailerStatementDC
    {
        public long Id { get; set; }
        public string transactionReqNo { get; set; }
        public double? DelayPenaltyRate { get; set; }
        public double? GstRate { get; set; }
        public string? PayableBy { get; set; }
        public string? InvoiceNo { get; set; }
        public long? WithdrawlId { get; set; }
        public long? InvoiceId { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public DateTime? DueDate { get; set; }
        public double? SanctionedLimit { get; set; }
        public double? AvailableLimit { get; set; }
        public string CustomerName { get; set; }
        public string Lender { get; set; }
        public string ServicePartner { get; set; }
        public string TransStatus { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double? Outstanding { get; set; }
        public long PaneltyTxnId { get; set; }

        public double? PrincipleAmount { get; set; }
        public double? PaymentAmount { get; set; }
        public double? InterestAmount { get; set; }
        public double? InterestPaymentAmount { get; set; }
        public double? BouncePaymentAmount { get; set; }
        public double? ExtraPaymentAmount { get; set; }

        public double? PenaltyChargesAmount { get; set; }
        public double? PenalPaymentAmount { get; set; }

        public double DelayPenalityAmount { get; set; }
        public double DelayPenalityGstAmount { get; set; }
        public double? OverduePaymentAmount { get; set; }
        public double? OverdueInterestAmount { get; set; }
    }


    public class GetAccountTransactionPaymentInfoDC
    {
        public long Id { get; set; }
        public long LoanAccountId { get; set; }
        public string InvoiceNo { get; set; }
        public long ParentAccountTransactionsID { get; set; }
        public double? PaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? NewPaymentDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public bool IsActive { get; set; }
    }

}
