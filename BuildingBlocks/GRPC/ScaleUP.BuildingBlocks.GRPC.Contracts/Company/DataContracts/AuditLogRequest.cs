using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class AuditLogRequest
    {
        [DataMember(Order = 1)]
        public string DatabaseName { get; set; }
        [DataMember(Order = 2)]
        public string EntityName { get; set; }
        [DataMember(Order = 3)]
        public long EntityId { get; set; }
        [DataMember(Order = 4)]
        public int Skip { get; set; }
        [DataMember(Order = 5)]
        public int Take { get; set; }
    }
}
