using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class CompanyAPI : BaseAuditableEntity
    {
        public long CompanyId { get; set; }
        [StringLength(300)]
        public string Code { get; set; }
        public bool IsWebhook { get; set; }
        [StringLength(300)]
        public string ApiType { get; set; }

        [StringLength(1000)]
        public string APIUrl { get; set; }
        [StringLength(50)]
        public string Authtype { get; set; }

        [StringLength(100)]
        public string APIKey { get; set; }
        [StringLength(1000)]
        public string APISecret { get; set; }
    }
}
