using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class CeplrPdfReports : BaseAuditableEntity
    {
        public long LeadMasterId { get; set; }
        [StringLength(200)]
        public string FileName { get; set; }
        [StringLength(100)]
        public string fip_id { get; set; }
        [StringLength(200)]
        public string callback_url { get; set; }
        [StringLength(100)]
        public string file_password { get; set; }
        [StringLength(100)]
        public string customer_id { get; set; }
        public int request_id { get; set; }

        public int ApiRequest_id { get; set; }
        [StringLength(500)]
        public string ApiToken { get; set; }
    }
}
