using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class AuditLogReply
    {
        [DataMember(Order = 1)]
        public string ModifiedBy { get; set; }
        [DataMember(Order = 2)]
        public DateTime ModifiedDate { get; set; }
        [DataMember(Order = 3)]
        public string Changes { get; set; }
        [DataMember(Order = 4)]
        public int TotalRecords { get; set; }
        [DataMember(Order = 5)]
        public string ActionType { get; set; }
    }
}
