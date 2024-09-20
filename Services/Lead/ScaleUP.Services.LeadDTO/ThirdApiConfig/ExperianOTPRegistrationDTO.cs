using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.ThirdApiConfig
{
    public class ExperianOTPRegistrationDTO
    {
        public string ApiUrl { get; set; }
        public string ApiSecret { get; set; }
        public string ApiKey { get; set; }
        public string Other { get; set; }
        public string Header { get; set; }
        public long ApiMasterId { get; set; }
    }
}
