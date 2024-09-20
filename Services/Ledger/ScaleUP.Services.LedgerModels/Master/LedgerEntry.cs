using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LedgerModels.Master
{
    public class LedgerEntry : BaseAuditableEntity
    {
        public double? Debit { get; set; }
        public double? Credit { get; set; }
        public long? ObjectId { get; set; }
        [MaxLength(100)]
        public string ObjectName { get; set; }
        [MaxLength(100)]
        public string? RefNo { get; set; }
        public long VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public VoucherType VoucherTypes { get; set; }
        public long LedgerId { get; set; }
        [ForeignKey("LedgerId")]
        public Ledger Ledgers { get; set; }

        public long AffectedLedgerId { get; set; }
        [ForeignKey("AffectedLedgerId")]
        public Ledger AffectedLedger { get; set; }
    }
}
