using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class GetOverdueLoanAccountResponse
    {
        [DataMember(Order = 1)]
        public long LoanAccountID { get; set; }
        [DataMember(Order = 2)]
        public bool TransactionStatus { get; set; }
    }

    [DataContract]
    public class GetOverdueLoanAccountIdRequest
    {
        [DataMember(Order = 1)]
        public long LoanAccountID { get; set; }
    }
}
