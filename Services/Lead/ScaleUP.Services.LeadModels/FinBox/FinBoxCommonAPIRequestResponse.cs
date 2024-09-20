using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.FinBox
{
    public class FinBoxCommonAPIRequestResponse : CommonAPIRequestResponse
    {
        public long LeadId { get; set; }
    }
}
