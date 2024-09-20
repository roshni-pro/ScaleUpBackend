using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    public class AccountDisbursementNotify
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public long AccountId { get; set; }
        public long LeadId { get; set; }
        public double CreditLimit { get; set; }
        public string CustomerUniqueCode { get; set; }
    }

    public class DisbursementAnchorResponse
    {
        public int StatusCode { get; set; }
        public string Response { get; set; }
    }
}
