using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadBankDetailResponse
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string Type { get; set; }
        [DataMember(Order = 3)]
        public string BankName { get; set; }
        [DataMember(Order = 4)]
        public string IFSCCode { get; set; }
        [DataMember(Order = 5)]
        public string AccountType { get; set; }
        [DataMember(Order = 6)]
        public string AccountNumber { get; set; }
        [DataMember(Order = 7)]
        public string AccountHolderName { get; set; }
        [DataMember(Order = 8)]
        public long LoanAccountId { get; set; }
    }
}
