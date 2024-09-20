using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CompanyModels.Master
{
    [Table(nameof(CompanyUsers))]
    public class CompanyUsers : BaseAuditableEntity
    {
        public required long CompanyId { get; set; }
        [StringLength(500)]
        public required string UserId { get; set;}
        [ForeignKey("CompanyId")]
        public Companies Companies { get; set; }
    }
}
