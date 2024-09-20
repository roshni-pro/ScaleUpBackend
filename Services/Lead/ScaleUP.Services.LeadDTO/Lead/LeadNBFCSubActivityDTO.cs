using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadNBFCSubActivityDTO
    {
        public string APIUrl { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public string TAPIKey { get; set; }
        public string TAPISecretKey { get; set; }
        public required int Sequence { get; set; }
        public long LeadNBFCApiId { get; set; }
        public long? RequestId { get; set; }
        public long? ResponseId { get; set; }
        public string ReferrelCode { get; set; }
        public long LeadNBFCSubActivityId { get; set; }

    }

    public class LeadNBFCSubActivityRequest
    {
        public long LeadId { get; set; }
        public string Code { get; set; }
        public string CompanyIdentificationCode{ get; set; }
    }

    public class NBFCSubactivityCompletedInput
    {
        public long NBFCCompanyId { get; set; }
        public long LeadId { get; set; }
    }
}
