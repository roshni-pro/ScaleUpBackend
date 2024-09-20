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
    public class Invoice : BaseAuditableEntity
    {
        public long LoanAccountId { get; set; }
        [StringLength(100)]
        public string Status { get; set; }
        [StringLength(500)]
        public string Comment { get; set; }
        [StringLength(100)]
        public string OrderNo { get; set; }
        [StringLength(100)]
        public string? InvoiceNo { get; set; }
        [StringLength(100)]
        public DateTime? InvoiceDate { get; set; }
        [StringLength(1000)]
        public string? InvoicePdfUrl { get; set; }
        public double OrderAmount { get; set; }
        public double InvoiceAmount { get; set; }
        public double TotalTransAmount { get; set; }
        public ICollection<AccountTransaction> AccountTransactions { get; set; }
        [ForeignKey("LoanAccountId")]
        public LoanAccount LoanAccount { get; set; }
        [StringLength(100)]
        public string? NBFCStatus { get; set; }
        [StringLength(100)]
        public string? NBFCUTR { get; set; }
    }
}

