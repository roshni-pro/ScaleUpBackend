﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    public class TemplateDc
    {
        public bool Status { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateType { get; set; }
        public string Template { get; set; }
        public string? DLTID { get; set; }
        public long? TemplateID { get; set; }
    }
    public class TemplateResponseDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic Response { get; set; }
    }
}
