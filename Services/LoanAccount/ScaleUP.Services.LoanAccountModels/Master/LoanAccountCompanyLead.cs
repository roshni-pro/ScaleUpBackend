using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class LoanAccountCompanyLead : BaseAuditableEntity
    {
        public required long LoanAccountId { get; set; }
        public long CompanyId { get; set; }
        public int? LeadProcessStatus { get; set; } //0-Initiated, 1-InProcessed, 2-Completed
        public string? UserUniqueCode { get; set; }

        [ForeignKey("LoanAccountId")]
        public LoanAccount LoanAccount { get; set; }
        public string? AnchorName { get; set; }
        public string? LogoURL { get; set; }

    }
}
