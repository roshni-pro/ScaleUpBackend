using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC
{
    public class NBFCCompanyApiDetail : BaseAuditableEntity
    {

        public long NBFCComapnyApiMasterId { get; set; }

        public long NBFCCompanyAPIId { get; set; }

        public int Sequence { get; set; }

        [StringLength(300)]
        public string Status { get; set; }
        [ForeignKey("NBFCComapnyApiMasterId")]
        public NBFCCompanyAPIMaster NBFCComapnyAPIMaster { get; set; }
    }
}
