using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class SendTransactionOtp
    {
        [DataMember(Order = 1)]
        public string TransactionReqNo { get; set; }
        [DataMember(Order = 2)]
        public double TransactionAmount { get; set; }
        [DataMember(Order = 3)]
        public string MobileNo { get; set; }
    }
}
