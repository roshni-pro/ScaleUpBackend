using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LoanAccountModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC
{
    public class LoanAccountRepaymentRefresh : BaseAuditableEntity
    {
        public long LoanAccountId { get; set; }
        public bool IsRunning { get; set; }
        public DateTime? LastRunningDate { get; set; }
        public bool? IsError { get; set; }
        public string? ErrorMsg { get; set; }
    }
}
