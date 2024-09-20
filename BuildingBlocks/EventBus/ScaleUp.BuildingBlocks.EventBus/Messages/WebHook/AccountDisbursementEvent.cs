using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.WebHook
{
    public class AccountDisbursementEvent : IAccountDisbursementEvent
    {       
        public long AccountId { get; set; }
       
    }
}
