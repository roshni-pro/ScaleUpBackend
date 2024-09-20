using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadCompanyConfigProdReply
    {
        [DataMember(Order = 1)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
    }


    [DataContract]
    public class LeadCompanyConfigProdRequest
    {
        [DataMember(Order = 1)]
        public long? AnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long LeadId { get; set; }
    }

    [DataContract]
    public class StampRemainderEmailReply
    {
        [DataMember(Order = 1)]
        public required string To { get; set; }
        [DataMember(Order = 2)]
        public string? From { get; set; }
        [DataMember(Order = 3)]
        public string? Bcc { get; set; }
        [DataMember(Order = 4)]
        public int StampCount { get; set; }
    }
}
