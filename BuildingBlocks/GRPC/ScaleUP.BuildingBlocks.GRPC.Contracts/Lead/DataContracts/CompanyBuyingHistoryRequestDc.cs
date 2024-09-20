using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class CompanyBuyingHistoryRequestDc
    {
        [DataMember(Order = 1)]
        public List<long> CompanyId { get; set; }
        [DataMember(Order = 2)]
        public List<long> LeadId { get; set;}
    }
}
