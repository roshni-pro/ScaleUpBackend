using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public class LeadActivityCompletedEvent : ILeadActivityCompletedEvent
    {
        public Guid CorrelationId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public long Id { get; set; }
    }
}
