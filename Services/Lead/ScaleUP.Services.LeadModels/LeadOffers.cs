using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.LeadModels
{
    [Table(nameof(LeadOffers))]
    public class LeadOffers : BaseAuditableEntity
    {
        public required long NBFCCompanyId { get; set; }
        public double? CreditLimit { get; set; }
        public double? ProcessingFee { get; set; }
        [StringLength(100)]
        public string Status { get; set; }
        [StringLength(2000)]
        public string? Comment { get; set; }
        public required long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }

        [StringLength(200)]
        public string? TleadId { get; set; }  //BusinessId

        public string? ErrorMessage { get; set; }
        [StringLength(200)]
        public string? CompanyIdentificationCode { get; set; }


    }
}
