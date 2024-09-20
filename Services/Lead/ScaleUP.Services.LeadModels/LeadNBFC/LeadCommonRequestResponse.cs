using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.LeadNBFC
{
    public class LeadCommonRequestResponse : BaseAuditableEntity
    {
        public long? LeadId { get; set; }
        public long? CommonRequestResponseId { get; set; }
        public long? LeadNBFCApiId { get; set; }
    }
}
