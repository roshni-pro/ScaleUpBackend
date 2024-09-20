using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.BlackSoil
{
    public class BlackSoilInitiateApprovedManualDTO
    {
        public long id { get; set; }
        public string? status { get; set; }
        public CreditLine credit_line { get; set; }
        public List<Application> applications { get; set; }
    }
    public class Application
    {
        public long? id { get; set; }
        public string? application_id { get; set; }
        public string? status { get; set; }
        public string? esign_status { get; set; }

    }

    public class CreditLine
    {
        public DateTime? created_at { get; set; }
        public double? amount { get; set; }
        public string? initiated_date { get; set; }
        public string? activated_date { get; set; }
        public string? line_status { get; set; }
    }

}
