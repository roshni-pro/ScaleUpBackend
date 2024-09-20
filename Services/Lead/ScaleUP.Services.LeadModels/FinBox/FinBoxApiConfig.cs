using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.FinBox
{
    public class FinBoxApiConfig : BaseAuditableEntity
    {
        [StringLength(300)]
        public string ApiURL { get; set; }

        [StringLength(100)]
        public string Code { get; set; }

        [StringLength(500)]
        public string APIKey { get; set; }

        [StringLength(500)]
        public string ServerHash { get; set; }
    }
}
