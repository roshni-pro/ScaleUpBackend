using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class KYCAllInfoResponse
    {
        [DataMember(Order = 1)]
        public long? KYCMasterId { get; set; }
        [DataMember(Order = 2)]
        public long? KYCMasterInfoId { get; set; }
        [DataMember(Order = 3)]
        public string KYCMasterCode { get; set; }
        [DataMember(Order = 4)]
        public List<KYCDetailInfoResponse>? KycDetailInfoList { get; set; } 

        [DataMember(Order = 5)]
        public string UniqueId { get; set; }
    }

    [DataContract]
    public class KYCDetailInfoResponse
    {
        [DataMember(Order = 1)]
        public string FieldName { get; set; }
        [DataMember(Order = 2)]
        public string FieldValue { get; set; }
        [DataMember(Order = 3)]
        public int? FieldType { get; set; }
        [DataMember(Order = 4)]
        public int FieldInfoType { get; set; }
        [DataMember(Order = 5)]
        public CompanyAddressReply CustomerAddressReplyDC { get; set; } = null;
        [DataMember(Order = 6)]
        public DocReply Document { get; set; }
        [DataMember(Order = 7)]
        public List<DocReply> DocumentList { get; set; }
    }
}
