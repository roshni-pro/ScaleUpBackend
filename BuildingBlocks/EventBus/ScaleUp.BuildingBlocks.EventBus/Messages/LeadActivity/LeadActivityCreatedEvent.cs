using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public class LeadActivityCreatedEvent: ILeadActivityCreatedEvent
    {
        public Guid CorrelationId { get; set; }
        public string KYCMasterCode { get; set; }
        public string UserId { get; set; }
        //public int CurrentSequence { get; set; }
        //public int NextSequence { get; set; }
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long SubActivityId { get; set; }
        public string JSONString { get; set; }
        public long? ComapanyId { get; set; }
        public string ProductCode { get; set; }
    }


    public class LeadUpdateAadharEvent : ILeadUpdateAadharEvent
    {
        public Guid CorrelationId { get; set; }
        public string KYCMasterCode { get; set; }
        public string UserId { get; set; }
        //public int CurrentSequence { get; set; }
        //public int NextSequence { get; set; }
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long SubActivityId { get; set; }
        public string JSONString { get; set; }
        public long? ComapanyId { get; set; }
        public string ProductCode { get; set; }
    }
    
}
