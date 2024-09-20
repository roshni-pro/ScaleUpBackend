using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction
{
    public class ThirdPartyRequest : BaseAuditableEntity
    {
        public long CompanyId { get; set; }
        [MaxLength(1000)]
        public string? Request { get; set; }
        [MaxLength(1000)]
        public string? Response { get; set; }
        public bool IsError { get; set; }
        public long? CompanyAPI { get; set; }
    }
}
