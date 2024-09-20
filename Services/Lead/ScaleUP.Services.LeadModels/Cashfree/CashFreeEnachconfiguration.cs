using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.Cashfree
{
    public class CashFreeEnachconfiguration : BaseAuditableEntity
    {
        public int linkExpiryDay { get; set; }
        public int expiresOnYear { get; set; }
        public int maxAmountMultiplier { get; set; }
        public string type { get; set; }
        public string intervalType { get; set; }
        public int intervals { get; set; }
        public string returnUrl { get; set; }
    }
}
