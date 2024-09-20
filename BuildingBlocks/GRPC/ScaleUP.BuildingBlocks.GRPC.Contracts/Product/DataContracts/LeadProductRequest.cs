using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class LeadProductRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 4)]
        public string CompanyType { get; set; }
        [DataMember(Order = 5)]
        public string? ActivityName { get; set; }
    }
}
