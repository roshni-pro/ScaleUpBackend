using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class CeplrBankList : BaseAuditableEntity
    {
        public string aa_fip_id { get; set; }
        public int? pdf_fip_id { get; set; }
        public string fip_name { get; set; }
        public string enable { get; set; }
        public string fip_logo_uri { get; set; }
    }
}
