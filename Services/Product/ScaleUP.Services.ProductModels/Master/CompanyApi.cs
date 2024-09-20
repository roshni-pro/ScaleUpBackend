using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ScaleUP.Services.ProductModels.Master
{
    public class CompanyApi : BaseAuditableEntity
    {
        public long CompanyId { get; set; }
        public bool IsWebhook { get; set; }
  
        [StringLength(3000)]
        public string APIUrl { get; set; }
        [StringLength(300)]
        public string Code { get; set; }
        public ICollection<NBFCSubActivityApi> nBFCSubActivityApis { get; set; }
    }
}
