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
    public class CompanyPartnerDetail : BaseAuditableEntity
    {
        [StringLength(30)]
        public required string PartnerName { get; set; }
        [StringLength(12)]
        public required string MobileNo { get; set; }
        [ForeignKey("CompanyId")]
        public Companies Companies { get; set; }       
        public long CompanyId { get; set; }

    }
}
