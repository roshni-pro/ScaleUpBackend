using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadActivitySequenceRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public long CompanyId { get; set; }
        [DataMember(Order = 4)]
        public string MobileNo { get; set; }
    }

    [DataContract]
    public class LeadActivityProgressListRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
    }
}
