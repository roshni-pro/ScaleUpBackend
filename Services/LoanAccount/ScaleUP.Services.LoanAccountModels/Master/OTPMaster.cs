using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class OTPMaster : BaseAuditableEntity
    {
        public int OTPno { get; set; }
        [StringLength(10)]
        public string MobileNo { get; set; }
        public bool IsVerified { get; set; }
        public string OtpTxnNo { get; set; }
    }
}
