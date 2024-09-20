using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.WebHook
{
    public interface IAccountDisbursementEvent
    {
        //public long AnchorCompanyId { get; set; }
        public long AccountId { get; set; }
        //public long LeadId { get; set; }
        //public string CustomerUniqueCode { get; set; }
    }
}
