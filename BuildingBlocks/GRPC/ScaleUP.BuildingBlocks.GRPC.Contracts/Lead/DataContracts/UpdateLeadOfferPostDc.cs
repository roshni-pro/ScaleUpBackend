using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class UpdateLeadOfferPostDc
    {
        [DataMember(Order =1)]
        public long LeadOfferId { get; set; }
        [DataMember(Order = 2)]
        public double interestRate { get; set; }
        [DataMember(Order = 3)]
        public double newOfferAmout { get; set; }
    }
}
