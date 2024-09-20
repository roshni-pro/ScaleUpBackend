using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public interface ILeadUpdateHistoryEvent
    {
        public string Narretion { get; set; }
        //public string Action { get; set; }
        public long? LeadId { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string EventName { get; set; }
        public string NarretionHTML { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
    }

    public class LeadUpdateHistoryEvent : ILeadUpdateHistoryEvent
    {
        public string Narretion { get; set; }
        //public string Action { get; set; }
        public long? LeadId { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string EventName { get; set; }
        public string NarretionHTML { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
    }
}
