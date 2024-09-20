using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class KYCSpecificDetailRequest
    {
        [DataMember(Order = 1)]
        public List<KYCSpecificDetailUserRequest> UserIdList { get; set; }
        [DataMember(Order = 2)]
        public Dictionary<string, List<string>> KYCReqiredFieldList { get; set; }
    }


    [DataContract]
    public class KYCSpecificDetailUserRequest
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }
        [DataMember(Order = 2)]
        public string ProductCode { get; set; }
    }
}
