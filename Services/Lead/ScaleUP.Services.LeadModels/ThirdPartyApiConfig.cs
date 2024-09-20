using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ScaleUP.Services.LeadModels
{
    public class ThirdPartyApiConfig : BaseAuditableEntity
    {
        [MaxLength(50)]
        public string Code { get; set; }
        [MaxLength(5000)]
        public string ConfigJson { get; set; }
    }
}
