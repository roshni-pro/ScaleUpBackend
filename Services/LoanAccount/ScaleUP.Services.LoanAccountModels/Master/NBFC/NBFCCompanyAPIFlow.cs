using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master.NBFC
{
    public class NBFCCompanyAPIFlow : BaseAuditableEntity
    {
        public long NBFCCompanyAPIId { get; set; }
        [StringLength(300)]
        public string NBFCIdentificationCode { get; set; }
        [StringLength(300)]
        public string TransactionStatusCode { get; set; }
        [StringLength(300)]
        public string TransactionTypeCode { get; set; }
        public int Sequence { get; set; }        
    }
}
