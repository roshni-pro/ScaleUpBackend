using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Cashfree
{
    public class CreateSubscriptionwithPlanInfoInput
    {
        public string subscriptionId { get; set; }
        public string customerName { get; set; }
        public string customerPhone { get; set; }
        public string customerEmail { get; set; }
        public string returnUrl { get; set; }
        public double authAmount { get; set; }
        public string expiresOn { get; set; } //{Date,pattern :yyyy-MM-dd HH:mm:ss}
        public Notes notes { get; set; }
        public string firstChargeDate { get; set; } //Date
        public PlanInfo planInfo { get; set; }
        public List<string> notificationChannels { get; set; }

    }

    public class Notes
    {
        public string key1 { get; set; }
        public string key2 { get; set; }
        public string key3 { get; set; }
        public string key4 { get; set; }

    }

    public class PlanInfo
    {
        public string type { get; set; }
        public string intervalType { get; set; }
        public string planName { get; set; }
        public double maxAmount { get; set; } //loan *3
        public int intervals { get; set; }
        public double recurringAmount { get; set; }
        public int maxCycles { get; set; }
        public int linkExpiry { get; set; } //minute 1, Maximum value that can be passed: 43200 (i.e 30 days), Default value: 43200 (i.e 30 days).
    }
}
