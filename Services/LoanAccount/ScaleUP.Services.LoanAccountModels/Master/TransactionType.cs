using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class TransactionType : BaseAuditableEntity
    {
        public bool IsDetailHead { get; set; }
        [StringLength(100)]
        public string Code { get; set; }
    }
}
