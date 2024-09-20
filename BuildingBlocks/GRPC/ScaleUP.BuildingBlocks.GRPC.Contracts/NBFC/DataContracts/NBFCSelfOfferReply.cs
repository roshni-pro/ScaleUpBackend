using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts
{


    [DataContract]
    public class NBFCSelfOfferReply
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }

        [DataMember(Order = 2)]
        public long CompanyId { get; set; }

        [DataMember(Order = 3)]
        public double OfferAmount { get; set; }
       
    }
}
