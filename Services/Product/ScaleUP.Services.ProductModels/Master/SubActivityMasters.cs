using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductModels.Master
{
    [Table(nameof(SubActivityMasters))]
    public class SubActivityMasters : BaseAuditableEntity
    {
        [StringLength(300)]
        public required string Name { get; set; }
        public required int Sequence { get; set; }
        [StringLength(100)]
        public string KycMasterCode { get; set; } 
        public long ActivityMasterId { get; set; }
        [ForeignKey("ActivityMasterId")]
        public ActivityMasters ActivityMasters { get; set; }

    }
}
