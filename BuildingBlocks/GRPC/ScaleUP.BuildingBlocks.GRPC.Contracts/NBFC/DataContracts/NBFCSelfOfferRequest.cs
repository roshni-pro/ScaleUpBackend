using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts
{
    [DataContract]
    public class NBFCSelfOfferRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
    }

}
