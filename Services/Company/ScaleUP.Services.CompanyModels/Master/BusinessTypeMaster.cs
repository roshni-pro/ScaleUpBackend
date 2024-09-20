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
    public class BusinessTypeMaster : BaseAuditableEntity
    {
        [StringLength(50)]
        public required string Name { get; set; }
        [StringLength(50)]
        public string? Value { get; set; }
    }
}




