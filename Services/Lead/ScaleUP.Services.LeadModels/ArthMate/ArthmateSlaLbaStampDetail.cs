using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class ArthmateSlaLbaStampDetail : BaseAuditableEntity
    {
        //Used For, Partner Name, Stamp Amoun, Purpose, DateofUtilisation, Stamp Paper No, Loan ID on 21-02-2024
        public int? StampPaperNo { get; set; }
        [StringLength(100)]
        public string UsedFor { get; set; }
        [StringLength(50)]
        public string PartnerName { get; set; }
        [StringLength(50)]
        public string Purpose { get; set; }
        public double StampAmount { get; set; }
        [StringLength(100)]
        public string LoanId { get; set; }
        [StringLength(200)]
        public string StampUrl { get; set; }
        public bool? IsStampUsed { get; set; }
        public DateTime? DateofUtilisation { get; set; }
        public long? LeadmasterId { get; set; }


    }
}
