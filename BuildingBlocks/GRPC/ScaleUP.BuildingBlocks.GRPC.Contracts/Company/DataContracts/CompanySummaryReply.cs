using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class CompanySummaryReply
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set;}
        [DataMember(Order = 2)]
        public string BusinessName { get; set;}
        [DataMember(Order = 3)]
        public string? LogoURL { get; set;}
    }

    [DataContract]
    public class CompanySummaryRequest
    {
        [DataMember(Order = 1)]
        public int Skip { get; set; }
        [DataMember(Order = 2)]
        public int Take { get; set; }
    }
}
