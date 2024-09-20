using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Cashfree
{
    public class CashfreeGetSubscriptionResponse
    {
        public CashfreeSubscriptionDetail subscription { set; get; }
        public string message { get; set; }
        public string status { get; set; }
    }
    public class CashfreeSubscriptionDetail
    {
        public string subReferenceId { get; set; }
        public string subscriptionId { get; set; }
        public string status { get; set; }
        public string umn { get; set; }
    }
}
