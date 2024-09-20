using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class ConfirmTransactionOtp
    {
        [DataMember(Order = 1)]
        public string TransactionReqNo { get; set; }
        [DataMember(Order = 2)]
        public string OtpTxnNo { get; set; }
        [DataMember(Order = 3)]
        public int OtpNo { get; set; }
        [DataMember(Order = 4)]
        public double TransactionAmount { get; set; }// TransactionAmount
        [DataMember(Order = 5)]
        public double ConvenionFee { get; set; }
        [DataMember(Order = 6)]
        public double WithGSTConvenionFee { get; set; }
      
    }
}
