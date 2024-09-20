using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC
{
    public class BlackSoilAccountDetail : BaseAuditableEntity
    {
        public long LoanAccountId { get; set; }

        public long ApplicationId { get; set; }

        public long BusinessId { get; set; }

        public long BlackSoilLoanId { get; set; }

        [StringLength(200)]
        public string? BusinessCode { get; set; }
    }
}
