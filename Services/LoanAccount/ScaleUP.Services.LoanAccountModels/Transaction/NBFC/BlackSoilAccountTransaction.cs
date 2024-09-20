using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC
{
    public class BlackSoilAccountTransaction : BaseAuditableEntity
    {

        public long LoanInvoiceId { get; set; }

        public long InvoiceId { get; set; } //ThirdParty NBFC InvoiceId
        [StringLength(1000)]
        public string? InvoiceUrl { get; set; }
        [StringLength(300)]
        public string? Status { get; set; }

        public long? WithdrawalId { get; set; }
        [StringLength(1000)]
        public string? WithdrawalUrl { get; set; }
        [StringLength(100)]
        public string? TopUpNumber { get; set; }

    }
}
