using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public class ICreateLeadActivityMessage
    {
        public Guid? LeadRequestId { get; set; }

        public string LeadGuild { get; set; }
        public string KYCMasterCode { get; set; }
        public int CurrentSequence { get; set; }
        public int NextSequence { get; set; }
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long SubActivityId { get; set; }
        public string UserId { get; set; }
        public string JSONString { get; set; }
        public long? CompanyId { get; set; }
        public string ProductCode{ get; set; }

    }
}
