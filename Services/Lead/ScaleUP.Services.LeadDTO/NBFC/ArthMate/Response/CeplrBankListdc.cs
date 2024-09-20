using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response
{
    public class CeplrBankListdc
    {
        public int code { get; set; }
        public List<CeplrBank> data { get; set; }
    }
    public class CeplrBank
    {
        public string aa_fip_id { get; set; }
        public int? pdf_fip_id { get; set; }
        public string fip_name { get; set; }
        public string enable { get; set; }
        public string fip_logo_uri { get; set; }
    }

}
