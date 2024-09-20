using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class CompanyApiReply
    {
        [DataMember(Order = 1)]
        public string Code { get; set; }
        [DataMember(Order = 2)]
        public string? APIUrl { get; set; }
        [DataMember(Order = 3)]
        public string? TAPIKey { get; set; }
        [DataMember(Order = 4)]
        public string? TAPISecretKey { get; set; }
        [DataMember(Order = 5)]
        public string? TReferralCode { get; set; }
        [DataMember(Order = 6)]
        public long? CompanyId { get; set; }    
    }
}
