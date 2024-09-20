using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity
{
    public class KYCActivityPAN
    {
        public long? DocumentId { get; set; }
        public string ImagePath { get; set; }
        public string UniqueId { get; set; }
        public string FathersName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Name{ get; set; }
    }
}
