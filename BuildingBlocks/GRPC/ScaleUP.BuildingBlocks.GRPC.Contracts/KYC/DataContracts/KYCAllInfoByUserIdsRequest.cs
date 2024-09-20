using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class KYCAllInfoByUserIdsRequest
    {
        [DataMember(Order = 1)]
        public List<string> UserId { get; set; }
        [DataMember(Order = 2)]
        public long KycMasterCode { get; set; }
        [DataMember(Order = 3)]
        public string ProductCode { get; set; }
        [DataMember(Order = 4)]
        public string CreatedBy { get; set; }
    }
}
