using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadAcceptOffer
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
        [DataMember(Order = 2)]
        public long OfferCompanyId { get; set; }

        [DataMember(Order = 3)]
        public long AnchorCompanyId { get; set; }
    }
}
