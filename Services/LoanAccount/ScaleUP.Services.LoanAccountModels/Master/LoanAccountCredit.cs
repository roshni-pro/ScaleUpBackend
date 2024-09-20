using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LoanAccountModels.Transaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class LoanAccountCredit : BaseAuditableEntity
    {
        //public double GstRate { get; set; }
        //public double ProcessingFeeRate { get; set; }
        // public double ConvenienceFeeRate { get; set; }
        public double DisbursalAmount { get; set; }
        public long LoanAccountId { set; get; }
        public double CreditLimitAmount { get; set; }
        //public long? CreditDays { get; set; }
        //public double BounceCharge { get; set; }
        //public double DelayPenaltyRate { get; set; }
        [ForeignKey("LoanAccountId")]
        public LoanAccount LoanAccount { get; set; }


    }
}
