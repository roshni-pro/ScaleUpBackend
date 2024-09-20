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
    [Table(nameof(CompanyLead))]
    public class CompanyLead : BaseAuditableEntity
    {
        public required long CompanyId { get; set; }
        [StringLength(100)]
        public required string CompanyCode { get; set; }
        public required long LeadId { get; set; }
        public int? VintageDays { get; set; }
        public int? MonthlyAvgBuying { get; set; }
        [StringLength(100)]
        public string? UserUniqueCode { get; set; }
        [StringLength(500)]
        public string? Email { get; set; }
        public int? LeadProcessStatus { get; set; } //0-Initiated, 1-InProcessed, 2-Completed
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }
        public ICollection<LeadCompanyBuyingHistory> LeadCompanyBuyingHistories { get; set; }
        public string? AnchorName { get; set; }
        public int? BusinessVintageDays { get; set; }
    }
}
