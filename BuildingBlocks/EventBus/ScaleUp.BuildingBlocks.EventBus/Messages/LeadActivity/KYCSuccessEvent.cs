using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public class KYCSuccessEvent: IKYCSuccessEvent
    {
        public Guid CorrelationId { get; set; }
        public long LeadId { get; set; }
        public long KycMasterId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public long? ComapanyId { get; set; }

    }
}
