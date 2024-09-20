using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.LeadModels
{
    [Table(nameof(LeadActivityMasterProgresses))]
    public class LeadActivityMasterProgresses : BaseAuditableEntity
    {
        public required long ActivityMasterId { get; set; }
        public long? SubActivityMasterId { get; set; }
        [StringLength(300)]
        public required string ActivityMasterName { get; set; }
        [StringLength(300)]
        public string? SubActivityMasterName { get; set; }
        public required int Sequence { get; set; }
        public required bool IsCompleted { get; set; }
        public required int IsApproved { get; set; }// 0=Inprogress , 1=Approved, 2=Rejected
        [StringLength(1000)]
        public string? Comment { get; set; }
        public required long LeadMasterId { get; set; }
        public long? KycMasterInfoId { get; set; }
        [ForeignKey("LeadMasterId")]
        public Leads Leads { get; set; }

        [StringLength(1000)]
        public string? RejectMessage { get; set; }

    }
}
