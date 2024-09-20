using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CompanyDTO.Master
{
    public class CompanyTemplateDc
    {

            public bool Status { get; set; }
            public string TemplateCode { get; set; }
            public string TemplateType { get; set; }
            public string Template { get; set; }
            public string? DLTID { get; set; }
            public long? TemplateID { get; set; }
       
    }
}
