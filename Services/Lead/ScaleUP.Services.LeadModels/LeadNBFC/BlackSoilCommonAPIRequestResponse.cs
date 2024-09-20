using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.LeadNBFC
{
    public class BlackSoilCommonAPIRequestResponse : CommonAPIRequestResponse
    {
        public long? LeadId { get; set; }
        public long? LeadNBFCApiId { get; set; }
    }

}
