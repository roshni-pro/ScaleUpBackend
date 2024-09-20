using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class NBFCCompanyNameResponseDC
    {
        [DataMember(Order = 1)]
        public long NBFCCompanyId { get; set;}
        [DataMember(Order = 2)]
        public string NBFCCompanyName { get; set; }
        [DataMember(Order = 3)]
        public string BusinessContactEmail { get; set; }
        [DataMember(Order = 4)]
        public string? NBFCCompanyShortName { get; set; }
    }
}
