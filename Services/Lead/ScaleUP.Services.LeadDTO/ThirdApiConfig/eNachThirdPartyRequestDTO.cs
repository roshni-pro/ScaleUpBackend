using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.ThirdApiConfig
{
    public class eNachThirdPartyRequestDTO
    {
        public long ApiMasterId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string RequestResponseMsg { get; set; }
        public string Header { get; set; }
        public string LeadMasterId { get; set; }
        public string CheckSumBeforeData { get; set; }
        public string UMRN_Filler10 { get; set; }
    }
}
