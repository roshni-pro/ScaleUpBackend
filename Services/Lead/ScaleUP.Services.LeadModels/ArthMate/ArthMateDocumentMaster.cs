using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class ArthMateDocumentMaster : BaseAuditableEntity
    {
        [StringLength(200)]
        public string DocumentName { get; set; } // Pan/adhar/ gst/ Gumasta etc other
        public bool IsFrontrequired { get; set; }
        public bool IsBackrequired { get; set; }
        public bool IsMandatory { get; set; }
        [StringLength(200)]
        public string DocumentTypeCode { get; set; }
        //public long MappingArthMateActivitySequenceId { get; set; }
    }
}
