using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA
{
    [DataContract]
    public class SalesAgentDetailDC
    {
        [DataMember(Order = 1)]
        public long SalesAgentId { set; get; }
        [DataMember(Order = 2)]
        public string UserId { set; get; }
        [DataMember(Order = 3)]
        public long AnchorCompanyId { set; get; }
        [DataMember(Order = 4)]
        public string Type { set; get; }

        [DataMember(Order = 5)]
        public string? GstnNo { set; get; }

        [DataMember(Order = 6)]
        public List<SalesAgentProductPayoutDC> SalesAgentProductPayouts { set; get; }
    }
    [DataContract]
    public class SalesAgentProductPayoutDC
    {
        [DataMember(Order = 1)]
        public double PayOutPercentage { set; get; }
        [DataMember(Order = 2)]
        public long ProductId { set; get; }
        [DataMember(Order = 3)]
        public int MinAmount { get; set; }
        [DataMember(Order = 4)]
        public int MaxAmount { get; set; }

    }
}
