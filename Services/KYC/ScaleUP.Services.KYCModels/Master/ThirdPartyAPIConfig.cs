using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCModels.Master
{
    public class ThirdPartyAPIConfig : BaseAuditableEntity
    {
        [StringLength(500)]
        public string Code { get; set; }
        [StringLength(500)]
        public string ProviderName { get; set; }
        [StringLength(1000)]
        public string URL { get; set; }
        [StringLength(1000)]
        public string Secret { get; set; }
        [StringLength(2000)]
        public string Token { get; set; }
    }
}
