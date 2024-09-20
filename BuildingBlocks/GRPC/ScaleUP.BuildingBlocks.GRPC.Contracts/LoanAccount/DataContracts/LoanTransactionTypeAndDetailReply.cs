using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class LoanTransactionTypeAndDetailReply
    {
        [DataMember(Order = 1)]
        public long Id { get; set; }
        [DataMember(Order = 2)]
        public string Code { get; set; }
    }
}
