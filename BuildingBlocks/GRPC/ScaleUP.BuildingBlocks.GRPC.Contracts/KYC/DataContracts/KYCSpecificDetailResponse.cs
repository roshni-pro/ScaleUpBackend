using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{

    [DataContract]
    public class KYCSpecificDetailResponse
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }
        [DataMember(Order = 2)]
        public string MasterCode { get; set; }
        [DataMember(Order = 3)]
        public string FieldName { get; set; }
        [DataMember(Order = 4)]
        public string FieldValue { get; set; }
    }
}
