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
    public class Ledger : BaseAuditableEntity
    {
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string? Alias { get; set; }
        public long? ObjectId { get; set; }
        public long LedgerTypeId { get; set; }
        [ForeignKey("LedgerTypeId")]
        public LedgerType LedgerType { get; set; }
        public ICollection<LedgerEntry> LedgerEntries { get; set; }
    }
}
