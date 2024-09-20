using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class OfferListReply
    {
        [DataMember(Order = 1)]
        public long LeadOfferId { get; set; }
        [DataMember(Order = 2)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 3)]
        public long LeadId { get; set; }

        [DataMember(Order = 4)]
        public string Status { get; set; }
        [DataMember(Order = 5)]
        public double? CreditLimit { get; set; }
        [DataMember(Order = 6)]
        public string NBFCName { get; set; }
    }
}
