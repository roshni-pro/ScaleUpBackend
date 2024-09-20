using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class eSignResponseDocumentId  : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        public string DocumentId { get; set;}
        public DateTime ExpiryTime { get; set; }
        public string? eSignRemark { get; set; }
    }
}
