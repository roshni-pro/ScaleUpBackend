using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public class CreatePANMessage : ICreatePANMessage
    {
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long SubActivityId { get; set; }
        public string UniqueId { get; set; }
        public string ImagePath { get; set; }
        public long DocumentId { get; set; }
    }
}
