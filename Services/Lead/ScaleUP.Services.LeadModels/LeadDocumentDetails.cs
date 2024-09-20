using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class LeadDocumentDetail : BaseAuditableEntity
    {
        public required long LeadId { get; set; }
        [StringLength(100)]
        public required string DocumentNumber { get; set; }
        [StringLength(50)]
        public required string DocumentName { get; set; }
        [StringLength(50)]
        public required string DocumentType { get; set; }
        public string? FileUrl { get; set; }
        public string? PdfPassword { get; set; }
        public int? Sequence { get; set; }

    }
}
