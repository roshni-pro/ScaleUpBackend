using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScaleUP.Services.LoanAccountModels.Master;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC.BlackSoil
{
    public class BlackSoilWebhookResponse : BaseAuditableEntity
    {
        public string Response { get; set; }
        [StringLength(200)]
        public string EventName { get; set; }
        public string? Status { get; set; }
        public long? LoanAccountId { get; set; }
        [ForeignKey("LoanAccountId")]
        public LoanAccount LoanAccount { get; set; }
        public long? BlackSoilAccountTransactionId { get; set; }

    }
}
