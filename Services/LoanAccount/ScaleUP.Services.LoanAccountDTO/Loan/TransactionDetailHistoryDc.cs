using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    [DataContract]
    public class TransactionDetailHistoryDc
    {
        //public string TransactionId { get; set; }
        public string PaymentMode { get; set; }
        public string PaymentReferenceNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public double Amount { get; set; }
        public double RemainingAmount { get; set; }
    }
}
