using System.ComponentModel.DataAnnotations;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class TemplateMasterResponseDTO
    {
        public string TemplateCode { get; set; }
        public string TemplateType { get; set; }
        public string? DLTID { get; set; }
        public string Template { get; set; }
        public string TemplateFor { get; set; }
        public long TemplateId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TemplatesReplyDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<TemplateMasterResponseDTO> Response { get; set; }
    }

    public class TemplateByIdResDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public TemplateMasterResponseDTO Response { get; set; }
    }
}
