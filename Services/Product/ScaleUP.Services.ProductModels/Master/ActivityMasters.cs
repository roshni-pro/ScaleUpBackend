using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(ActivityMasters))]
    public class ActivityMasters : BaseAuditableEntity
    {
        [StringLength(300)]
        public required string ActivityName { get; set; }
        public required int Sequence { get; set; }
        public required string FrontOrBack { get; set; }
        public required string CompanyType { get; set; }
    }
}
