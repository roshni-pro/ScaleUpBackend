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
    public class AccountTransactionDetail : BaseAuditableEntity
    {
        public long AccountTransactionId { get; set; }
        [ForeignKey("AccountTransactionId")]
        public AccountTransaction AccountTransaction { get; set; }
        public double Amount { get; set; }
        public bool IsPayableBy { get; set; }
        public long TransactionDetailHeadId { get; set; }
        [ForeignKey("TransactionDetailHeadId")]
        public TransactionDetailHead TransactionDetailHead { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? PaymentReqNo { get; set; }
        public string? PaymentMode { get; set; }
        public DateTime? PaymentDate { get; set; }
        [StringLength(100)]
        public string Status { get; set; } //Initiate, Failed, Success
        [StringLength(1000)]
        public string? UTRNumber { get; set; }
    }
}
