using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class OrderCompletedRequest
    {
        [DataMember(Order = 1)]
        public string transactionNo { get; set; }
        [DataMember(Order = 2)]
        public bool transStatus { get; set; }
    }
}
