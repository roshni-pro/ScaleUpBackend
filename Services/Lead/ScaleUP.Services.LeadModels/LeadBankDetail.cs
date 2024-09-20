using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class LeadBankDetail : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }

        [StringLength(100)]
        public string Type { get; set; } //borrower,beneficiary, 
        [StringLength(200)]
        public string BankName { get; set; }
        [StringLength(20)]
        public string IFSCCode { get; set; }
        [StringLength(100)]
        public string AccountType { get; set; }
        [StringLength(200)]
        public string AccountNumber { get; set; }
        [StringLength(500)]
        public string AccountHolderName { get; set; }

        [StringLength(200)]
        public string? PdfPassword { get; set; }
        [StringLength(200)]
        public string? SurrogateType { get; set; }
    }
}
