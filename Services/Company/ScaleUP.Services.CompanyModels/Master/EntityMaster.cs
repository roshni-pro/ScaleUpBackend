using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CompanyModels.Master
{
    public class EntityMaster : BaseAuditableEntity
    {
        [StringLength(200)]
        public required string EntityName { get; set; }
        public required int DefaultNo { get; set; }
        [StringLength(500)]
        public required string EntityQuery { get; set; }
        [StringLength(50)]
        public required string Separator { get; set; }

    }
}
