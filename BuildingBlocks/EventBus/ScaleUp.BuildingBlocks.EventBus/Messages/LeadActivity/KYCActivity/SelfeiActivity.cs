using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity
{
    public class SelfeiActivity
    {
        //public string FrontImageUrl { get; set; }
        public int? FrontDocumentId { get; set; }
        public long LeadId { get; set; }
    }
}
