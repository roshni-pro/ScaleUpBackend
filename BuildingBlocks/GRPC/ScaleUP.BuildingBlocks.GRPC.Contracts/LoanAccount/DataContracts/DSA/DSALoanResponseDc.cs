using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA
{
    [DataContract]
    public class DSADashboardLoanResponse
    {
        [DataMember(Order = 1)]
        public double TotalDisbursedAmount { get; set; }
        [DataMember(Order = 2)]
        public double TotalPayoutAmount { get; set; }
    }
}
