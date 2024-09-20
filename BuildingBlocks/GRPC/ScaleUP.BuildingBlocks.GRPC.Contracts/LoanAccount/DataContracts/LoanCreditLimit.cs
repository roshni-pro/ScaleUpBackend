using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class LoanCreditLimit
    {
        [DataMember(Order = 1)]
        public double CreditLimit { get; set; }

        [DataMember(Order = 2)]
        public bool IsBlock { get; set; }

        [DataMember(Order = 3)]
        public bool IsBlockHideLimit { get; set; }
        [DataMember(Order = 4)]
        public double? TotalCreditLimit { get; set; }
    }

}
