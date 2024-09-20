using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class LoanAccountReplyDC
    {
        [DataMember(Order = 1)]
        public long LoanAccountId { get; set; }
        [DataMember(Order = 2)]
        public long LeadId { get; set; }
        [DataMember(Order = 3)]
        public bool IsAccountActive { get; set; }
        [DataMember(Order = 4)]
        public bool IsBlock { get; set; }
        [DataMember(Order = 5)]
        public string? IsBlockComment { get; set; }
        [DataMember(Order = 6)]
        public bool IsBlockHideLimit { get; set; }
    }
}
