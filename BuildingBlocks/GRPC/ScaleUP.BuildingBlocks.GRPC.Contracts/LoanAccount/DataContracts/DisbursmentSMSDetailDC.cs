using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class DisbursmentSMSDetailDC
    {
        [DataMember(Order = 1)]
        public double amount { get; set; }
        [DataMember(Order = 2)]
        public string OrderNo { get; set; }
        [DataMember(Order = 3)]
        public DateTime? DueDate { get; set; }
        [DataMember(Order = 4)]
        public string MobileNo { get; set; }
        [DataMember(Order = 5)]
        public string BankAccountNo { get; set; }
        [DataMember(Order = 6)]
        public string UserName { get; set; }

    }
}
