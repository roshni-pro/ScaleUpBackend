using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class eSignDocumentResponse : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        public string documentId { get; set; }
        public string auditTrail { get; set; }
        public string irn { get; set; }
        public string messages { get; set; }
        public string clientData { get; set; }
        public string File { get; set; }
        public string users { get; set; }

        //public string verification { get; set; }
        //public string signer { get; set; }
        //public bool webhookStatus { get; set; }
        //public int webhookStatusCode { get; set; }
    }
}
