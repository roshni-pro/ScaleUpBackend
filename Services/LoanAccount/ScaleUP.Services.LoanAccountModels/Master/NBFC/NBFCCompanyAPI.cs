using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master.NBFC
{
    public class NBFCCompanyAPI : BaseAuditableEntity
    {

        [StringLength(300)]
        public string APIUrl { get; set; }
        [StringLength(300)]
        public string Code { get; set; }
        [StringLength(1000)]

        public string TAPIKey { get; set; }
        [StringLength(1000)]
        public string TAPISecretKey { get; set; }

        [StringLength(200)]
        public string? TReferralCode { get; set; }

        public ICollection<NBFCCompanyAPIFlow> NBFCCompanyAPIFlows { get; set; }
    }
}
