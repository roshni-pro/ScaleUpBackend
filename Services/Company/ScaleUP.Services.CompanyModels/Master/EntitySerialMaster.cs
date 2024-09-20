using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CompanyModels.Master
{
    public class EntitySerialMaster : BaseAuditableEntity
    {
        public required long EntityId { get; set; }
        [ForeignKey("EntityId")]
        public EntityMaster EntityMasters { get; set; }
        [StringLength(200)]
        public string? Prefix { get; set; }
        [StringLength(200)]
        public string? Suffix { get; set; }
        public required long StartFrom { get; set; }
        public required long NextNumber { get; set; }
        public required long StateId { get; set; }
        [StringLength(50)]
        public string? CompanyType { get; set; } //NBFC, ANCHOR 
    }
}
