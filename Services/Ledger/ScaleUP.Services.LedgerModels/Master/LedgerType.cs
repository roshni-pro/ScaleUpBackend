using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LedgerModels.Master
{
    public class LedgerType : BaseAuditableEntity
    {
        [MaxLength(100)]
        public string Name { get; set; }
        public ICollection<Ledger> Ledgers { get; set; }
    }
}
