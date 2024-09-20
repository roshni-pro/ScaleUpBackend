using Microsoft.AspNetCore.Mvc.RazorPages;
using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class PaymentRequest: BaseAuditableEntity
    {
        public long AnchorCompanyId { get; set; }      
        public long LoanAccountId { get; set; }
        [StringLength(100)]
        public string PaymentStatus { get; set; }
        [StringLength(500)]
        public string Comment { get; set; }
        [StringLength(100)]
        public string TransactionReqNo { get; set; }
        [StringLength(100)]
        public string OrderNo { get; set; }
        public double TransactionAmount { get; set; }
        public double OrderAmount { get; set; }
    }
}
