using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    public class WaiveOffPenaltyBounceRequestDTO
    {
        public string TransactionId { get; set; }
        public string PenaltyType { get; set; }
        public double DiscountAmount { get; set; }
        public double DiscountGst { get; set; }
    }

    public class WaiveOffPenaltyBounceReplytDTO
    {
        public bool Status { get; set; }
        public string  Message{ get; set; }
    }
}
