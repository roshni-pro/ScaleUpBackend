using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class LoanAccountTransactionDetailRequest
    {
        [DataMember(Order = 1)]
        public required long AccountTransactionId { get; set; }
        [DataMember(Order = 2)]
        public double Amount { get; set; }

        [DataMember(Order = 3)]
        public string TransactionDetailHeadCode { get; set; }
        [DataMember(Order = 4)]
        public bool IsPayableBy { get; set; }

    }
}
