using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public interface ICheckStatus
    {
        string LeadGuild { get; }
    }



    public interface ILeadStatus
    {
        string LeadGuild { get; }
        string State { get; }
    }
    public class Leadresponse
    {
        
        public string LeadGuild { get; set; }
        public string State { get; set; }
        public bool IsSuccess{ get; set; }
    }

    public interface ILeadNotFound
    {
        public string LeadGuild { get; }
    }
}
