using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction
{
    public class InvoiceDisbursalProcessed : BaseAuditableEntity
    {
        public long LoanAccountId { get; set; }
        public long AccountTransactionId { get; set; }
        [StringLength(100)]
        public string Status { get; set; }
        public double amount { get; set; }
        public DateTime InvoiceDisbursalDate { get; set; }
        [StringLength(100)]
        public string? TopUpNumber { get; set; }
    }
}
