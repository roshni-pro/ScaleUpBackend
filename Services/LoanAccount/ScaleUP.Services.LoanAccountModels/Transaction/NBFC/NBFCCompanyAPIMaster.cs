using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC
{
    public class NBFCCompanyAPIMaster : BaseAuditableEntity
    {
        [StringLength(300)]
        public string IdentificationCode { get; set; }

        [StringLength(300)]
        public string TransactionTypeCode { get; set; }

        [StringLength(300)]
        public string TransactionStatuCode { get; set; }

        public long InvoiceId { get; set; }

        [StringLength(300)]
        public string Status { get; set; }
        public ICollection<NBFCCompanyApiDetail> NBFCComapnyApiDetails { get; set; }

    }
}
