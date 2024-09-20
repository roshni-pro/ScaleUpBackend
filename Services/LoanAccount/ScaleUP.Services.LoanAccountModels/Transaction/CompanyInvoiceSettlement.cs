using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScaleUP.Global.Infrastructure.Common.Models;
using System.Runtime.Serialization;

namespace ScaleUP.Services.LoanAccountModels.Transaction
{
    public class CompanyInvoiceSettlement : BaseAuditableEntity
    {
        public required long CompanyInvoiceId { get; set; }
        [ForeignKey("CompanyInvoiceId")]
        public CompanyInvoice CompanyInvoice { get; set; }
        public required double Amount { get; set; }
        public required double TDSAmount { get; set; }
        public required DateTime PaymentDate { get; set; }
        [StringLength(100)]
        public required string UTRNumber { get; set; }
        public required bool IsSattled { get; set; }
    }
}
