using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCModels.Master
{
    public class KYCMaster : BaseAuditableEntity
    {
        [StringLength(100)]
        public string Code { get; set; }

        public int ValidityDays{ get; set; }
        public ICollection<KYCDetail> KYCDetails { get; set; }
    }
}
