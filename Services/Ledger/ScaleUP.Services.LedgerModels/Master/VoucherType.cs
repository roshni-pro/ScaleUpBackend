using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LedgerModels.Master
{
    public class VoucherType : BaseAuditableEntity
    {
        [MaxLength(100)]
        public string Code { get; set; }
        public ICollection<LedgerEntry> LedgerEntries { get; set; }
    }
}
