using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCModels.Master
{
    public class KarzaElectricityDistrict : BaseAuditableEntity
    {
        [StringLength(100)]
        public string DistrictName { get; set; }
        public int DistrictCode { get; set; }
        [StringLength(100)]
        public string State { get; set; }
    }
}
