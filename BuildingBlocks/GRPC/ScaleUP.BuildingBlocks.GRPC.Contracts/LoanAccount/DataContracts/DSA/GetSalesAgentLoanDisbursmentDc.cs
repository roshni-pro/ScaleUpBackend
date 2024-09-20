using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA
{
    [DataContract]
    public class GetSalesAgentLoanDisbursmentDc
    {
        [DataMember(Order = 1)]
        public long DisbursedLoanAccountId { get; set; }
        [DataMember(Order = 2)]
        public string LeadCreatedUserId { get; set; }
    }
}
