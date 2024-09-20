using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class OfferCompanyRequest
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
        [DataMember(Order = 2)]
        public List<long> CompanyIds { get; set; }
        [DataMember(Order = 3)]
        public long? AnchorCompanyId { get; set; }

    }
    [DataContract]
    public class OfferCompanyReply
    {
        [DataMember(Order = 1)]
        public List<long> CompanyIds { get; set; }
        [DataMember(Order = 2)]
        public List<LeadNBFCSubActivityRequestDc> LeadNBFCSubActivity { get; set; }

    }
}
