using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LoanAccountModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction
{
    public class AccountTransaction : BaseAuditableEntity
    {
        [StringLength(100)]
        public string? CustomerUniqueCode { get; set; }
        public long LoanAccountId { get; set; }
        public DateTime? SettlementDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public long? AnchorCompanyId { get; set; }
        public long? ParentAccountTransactionsID { get; set; }


        //public string? PaymentRefNo { get; set; }
        public string PayableBy { get; set; }
        public string InterestType { get; set; } //ConvenienceFeeType : Percentage / Amount
        public string ProcessingFeeType { get; set; }
        public double ProcessingFeeRate { get; set; }
        public double GstRate { get; set; }
        public double InterestRate { get; set; } //ConvenienceFeeRate
        public long? CreditDays { get; set; }
        public double BounceCharge { get; set; }
        public double DelayPenaltyRate { get; set; }

        public double OrderAmount { get; set; }
        public double TransactionAmount { get; set; }
        public double? InterestAmountCalculated { get; set; }//ConvenienceFeeAmount
        public double GSTAmount { get; set; }
        public double PaidAmount { get; set; }
        public double? DiscountAmount { get; set; }

        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? InvoicePdfURL { get; set; }

        [StringLength(100)]
        public string? ReferenceId { get; set; }
        public long TransactionTypeId { get; set; }
        public long CompanyProductId { get; set; }
        public long TransactionStatusId { get; set; }

        public DateTime? OrderDate { get; set; }
        public long? InvoiceId { get; set; }

        [ForeignKey("TransactionStatusId")]
        public TransactionStatus TransactionStatus { get; set; }
        [ForeignKey("TransactionTypeId")]
        public TransactionType TransactionType { get; set; }

        [ForeignKey("LoanAccountId")]
        public LoanAccount LoanAccount { get; set; }

        public ICollection<AccountTransactionDetail> AccountTransactionDetails { get; set; }
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }
    }
}
