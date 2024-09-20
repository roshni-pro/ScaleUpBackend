using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadAgreementDc
    {
        [DataMember(Order = 1)]
        public required string DocUrl { get; set; }
        [DataMember(Order = 2)]
        public DateTime ExpiredOn { get; set; }
        [DataMember(Order = 3)]
        public required long LeadId { get; set; }
        [DataMember(Order = 4)]
        public string DocumentId { get; set; }
    }
}
