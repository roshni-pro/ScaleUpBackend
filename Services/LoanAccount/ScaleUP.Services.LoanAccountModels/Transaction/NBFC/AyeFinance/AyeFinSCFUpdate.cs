using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC.AyeFinance
{
    public class AyeFinanceUpdate : BaseAuditableEntity
    {
        public long LoanAccountId { get; set; }
        [StringLength(100)]
        public string? refId { get; set; }
        [StringLength(100)]
        public string? leadCode { get; set; }
        [StringLength(100)]
        public string? switchpeReferenceId { get; set; }
        [StringLength(100)]

        public string? invoiceNo { get; set; }
        [StringLength(100)]

        public string? orderId { get; set; }
        public int  status { get; set; } // onorder = 0 , cancel=1 and repayment=2
        [StringLength(100)]
        public string? transactionId { get; set; }
        public double? availablelLimit { get; set; }
        public double? totallimit { get; set; }
    }
}
