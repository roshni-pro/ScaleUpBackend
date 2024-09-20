using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class LoanBankDetail : BaseAuditableEntity
    {
        public long LoanAccountId { get; set; }
        [ForeignKey("LoanAccountId")]
        public LoanAccount LoanAccounts { get; set; }
        public string Type { get; set; }//////borrower,beneficiary,
        public string BankName { get; set; }
        public string IFSCCode { get; set; }
        public string AccountType { get; set; }
        //public string? Channel { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
        public string VirtualAccountNumber { get; set; }
        public string VirtualBankName { get; set; }
        public string VirtualIFSCCode { get; set;}
        public string VirtualUPIId { get; set;}

    }
}
