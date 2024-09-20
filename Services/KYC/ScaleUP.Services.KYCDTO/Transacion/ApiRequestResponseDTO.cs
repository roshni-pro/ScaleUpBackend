using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class ApiRequestResponseDTO
    {
        public string UserId { get; set; }
        public long APIConfigId { get; set; }
        public string Type { get; set; }
        public string URL { get; set; }
        public string RequestResponse { get; set; }
        public string Header { get; set; }
        public DateTime Created { get; set; }
    }

    public class ThirdPartyApiRequestDTO
    {
        public string UserId { get; set; }
        public long APIConfigId { get; set; }
        public long CompanyId { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }
        public string? ProcessedResponse { get; set; }
        public bool IsError { get; set; }
        public DateTime Created { get; set; }
        public bool IsActive { get; set; }
    }
}
