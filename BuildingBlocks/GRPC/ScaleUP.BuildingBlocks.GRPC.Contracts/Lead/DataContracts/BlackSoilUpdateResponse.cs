using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class BlackSoilUpdateResponse
    {
        [DataMember(Order = 1)]
        public string? ApplicationId { get; set; }
        [DataMember(Order = 2)]
        public string? BusinessId { get; set; }
        [DataMember(Order = 3)]
        public string? BusinessCode { get; set; }
        [DataMember(Order = 4)]
        public string? ApplicationCode { get; set; }
    }
}
