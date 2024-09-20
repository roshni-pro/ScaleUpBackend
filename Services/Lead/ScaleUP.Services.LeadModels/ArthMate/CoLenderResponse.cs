using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class CoLenderResponse : BaseAuditableEntity
    {
        public long LeadMasterId { get; set; }
        [StringLength(100)]
        public string request_id { get; set; }
        public double loan_amount { get; set; }
        public double pricing { get; set; }
        [StringLength(50)]
        public string co_lender_shortcode { get; set; }
        [StringLength(100)]
        public string loan_app_id { get; set; }
        public int co_lender_assignment_id { get; set; }
        [StringLength(100)]
        public string co_lender_full_name { get; set; }
        [StringLength(50)]
        public string status { get; set; }
        [StringLength(200)]
        public string AScoreRequest_id { get; set; }
        [StringLength(200)]
        public string ceplr_cust_id { get; set; }
        public double SanctionAmount { get; set; }
        [StringLength(50)]
        public string ProgramType { get; set; }
    }
}
