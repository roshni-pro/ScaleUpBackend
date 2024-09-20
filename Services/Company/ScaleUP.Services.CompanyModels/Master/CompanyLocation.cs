using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CompanyModels.Master
{
    [Table(nameof(CompanyLocation))]
    public class CompanyLocation : BaseAuditableEntity
    {
        public long CompanyId { get; set; }
        public long LocationId { get; set; }

        [ForeignKey("CompanyId")]
        public Companies Companies { get; set; }

    }
}
