using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master.NBFC.ArthMate
{
    public class RepaymentSchedule : BaseAuditableEntity
    {
        public long LoanAccountId { get; set; }
        public int RepayScheduleId { get; set; }
        public int CompanyId { get; set; }
        public string ProductId { get; set; }
        public string loanId { get; set; }
        public int EMINo { get; set; }
        public double EMIAmount { get; set; }
        public double Principal { get; set; }
        public double InterestAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public double PrincipalBalance { get; set; }
        public double PrincipalOutstanding { get; set; }
    }
}
