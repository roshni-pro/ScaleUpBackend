using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Cashfree
{
    public class CashfreeCreateSubscriptionDetailResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public CreateSubscriptionDetail data { get; set; }
    }
}

public class CreateSubscriptionDetail
{
    public string subReferenceId { get; set; }
    public string authLink { get; set; }
}

