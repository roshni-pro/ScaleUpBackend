using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class LoanRepaymentAccountDetailRequestDC
    {
        [DataMember(Order = 1)]
        public string VirtualAccountNumber { get; set; }
        [DataMember(Order = 2)]
        public string VirtualBankName { get; set; }
        [DataMember(Order = 3)]
        public string VirtualIFSCCode { get; set; }
        [DataMember(Order = 4)]
        public string VirtualUPIId { get; set; }
        [DataMember(Order = 5)]
        public long LeadAccountId { get; set; }
        [DataMember(Order = 6)]
        public string UserId { get; set; }
    }


    [DataContract]
    public class LoanRepaymentAccountDetailResponseDC
    {
        [DataMember(Order = 1)]
        public string VirtualAccountNumber { get; set; }
        [DataMember(Order = 2)]
        public string VirtualBankName { get; set; }
        [DataMember(Order = 3)]
        public string VirtualIFSCCode { get; set; }
        [DataMember(Order = 4)]
        public string VirtualUPIId { get; set; }
    }
}
