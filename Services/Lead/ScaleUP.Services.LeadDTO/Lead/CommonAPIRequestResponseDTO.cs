using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class CommonAPIRequestResponseDTO
    {
        public string URL { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public long Id { get; set; }
        public long? LeadId { get; set; }
        public long? LeadNBFCApiId { get; set; }
    }

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
    public class GetUrlTokenDTO
    {
        public string url { get; set; }
        public string token { get; set; }
        public string CompanyCode { get; set; }
        public long id { get; set; }
        public string ApiSecretKey { get; set; }
    }

}
