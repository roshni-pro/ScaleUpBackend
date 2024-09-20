using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.ThirdApiConfig
{
    public class ThirdPartyRequestDTO
    {
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public long CompanyId { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }

        public string? ProcessedResponse { get; set; }
        public string Code { get; set; }
        public Boolean IsError { get; set; }
        public long? ThirdPartyApiConfigId { get; set; }
    }

    public class FinBoxRequestResponseDTO
    {
        public long LeadId { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }
        public int StatusCode { get; set; }
        public string URL { get; set; }
        public bool IsSuccess { get; set; }
    }

}
