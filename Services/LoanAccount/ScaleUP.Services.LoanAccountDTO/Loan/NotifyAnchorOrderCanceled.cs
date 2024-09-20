using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    public class NotifyAnchorOrderCanceled
    {
        public string OrderNo { get; set; }
        public string TransactionNo { get; set; }
        public double amount { get; set; }
        public string Comment { get; set; }

    }
}
