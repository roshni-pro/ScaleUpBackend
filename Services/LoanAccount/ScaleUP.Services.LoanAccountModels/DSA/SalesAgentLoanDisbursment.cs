using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScaleUP.Services.LoanAccountModels.Master;
using ScaleUP.Global.Infrastructure.Common.Models;

namespace ScaleUP.Services.LoanAccountModels.DSA
{
    public class SalesAgentLoanDisbursment : BaseAuditableEntity
    {
        [ForeignKey("DisbursedLoanAccountId")]
        public LoanAccount LoanAccounts { get; set; }
        public required long DisbursedLoanAccountId { get; set; } // Customer Disburesment loan AccountId
        [StringLength(300)]
        public string LeadCreatedUserId { set; get; }
        public bool IsProcess { set; get; }

    }
}
