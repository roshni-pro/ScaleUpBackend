using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class GetOverdueAccountsResponse
    {
        [DataMember(Order = 1)]
        public string CustomerName { get; set; }
        [DataMember(Order = 2)]
        public double? Amount { get; set; }

        [DataMember(Order = 3)]
        public string OrderNo { get; set; }

        [DataMember(Order = 4)]
        public string AnchorName { get; set; }

        [DataMember(Order = 5)]
        public string MobileNo { get; set; }
    }
}
