using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public class KYCFailEvent: IKYCFailEvent
    {
        public Guid CorrelationId { get; set; }
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
