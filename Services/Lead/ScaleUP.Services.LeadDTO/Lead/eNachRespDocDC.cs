using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class eNachRespDocDC
    {
        public string Status { get; set; }
        public string MsgId { get; set; }
        public string RefId { get; set; }
        public List<Error> Errors { get; set; }
        public string Filler1 { get; set; }
        public string Filler2 { get; set; }
        public string Filler3 { get; set; }
        public string Filler4 { get; set; }
        public string Filler5 { get; set; }
        public string Filler6 { get; set; }
        public string Filler7 { get; set; }
        public string Filler8 { get; set; }
        public string Filler9 { get; set; }
        public string Filler10 { get; set; }
    }

    public class Error
    {
        public string Error_Code { get; set; }
        public string Error_Message { get; set; }
    }
}
