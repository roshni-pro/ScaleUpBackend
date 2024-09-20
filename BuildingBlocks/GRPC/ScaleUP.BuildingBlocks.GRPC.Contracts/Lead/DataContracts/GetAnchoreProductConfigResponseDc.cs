using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class GetAnchoreProductConfigResponseDc
    {
        //[DataMember(Order = 1)]
        //public long? CompanyId { get; set; }
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
    }
}
