using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    public class TransactionSettlementRequestDC
    {
        [DataMember(Order = 1)]
        public string TransactionReqNo { get; set; }
        [DataMember(Order = 2)]
        public string TransactionTypeCode { get; set; }
        [DataMember(Order = 3)]
        public long LeadId { get; set; }
        [DataMember(Order = 4)]
        public double Amount { get; set; }
        [DataMember(Order = 5)]
        public string PaymentRefNo { get; set; }
        [DataMember(Order = 6)]
        public DateTime PaymentDate { get; set; }

        [DataMember(Order = 7)]
        public string UserName { get; set; }
    }
}
