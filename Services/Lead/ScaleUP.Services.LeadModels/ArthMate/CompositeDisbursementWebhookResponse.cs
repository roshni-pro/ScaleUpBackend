using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class CompositeDisbursementWebhookResponse : BaseAuditableEntity
    {
        public long LeadMasterId { get; set; }
        [StringLength(50)]
        public string status_code { get; set; }
        [StringLength(200)]
        public string loan_id { get; set; }
        [StringLength(200)]
        public string partner_loan_id { get; set; }
        public double net_disbur_amt { get; set; }
        [StringLength(100)]
        public string utr_number { get; set; }
        [StringLength(100)]
        public string utr_date_time { get; set; }
        [StringLength(200)]
        public string txn_id { get; set; }
        public string Response { get; set; }
    }
}
