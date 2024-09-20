using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class TemplateResponseDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic Response { get; set; }
    }
}
