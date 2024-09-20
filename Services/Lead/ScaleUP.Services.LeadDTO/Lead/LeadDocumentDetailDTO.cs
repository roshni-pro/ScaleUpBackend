using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadDocumentDetailDTO
    {
        public required long LeadId { get; set; }
        public  string? DocumentNumber { get; set; }
        public required string DocumentName { get; set; }
        public string? FileUrl { get; set; }
        public string? PdfPassword { get; set; }
        public long LeadDocDetailId { get; set; }
        public int? Sequence { get; set; }
        public long? DocId { get; set; } 

    }
}
