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
    public class KycHistroyResponse
    {
        [DataMember(Order = 1)]
        public long Id { get; set; }
        [DataMember(Order = 2)]
        public string UserId { get; set; }
        [DataMember(Order = 3)]
        public long EntityId { get; set; }

        [DataMember(Order = 4)]
        public string DatabaseName { get; set; }
        [DataMember(Order = 5)]
        public string EntityName { get; set; }
        [DataMember(Order = 6)]
        public string Action { get; set; }

        [DataMember(Order = 7)]
        public DateTime  Timestamp { get; set; }
        
        [DataMember(Order = 8)]
        public string Changes { get; set; }
        

    }


    [DataContract]
    public class KYCDetailInfoDC
    {
        [DataMember(Order = 1)]
        public long kycDetailID { get; set; }
        [DataMember(Order = 2)]
        public string FieldValue { get; set; }
        [DataMember(Order = 3)]
        public string KYCDetails_Field { get; set; }
        [DataMember(Order = 4)]
        public int FieldInfoType { get; set; }
        [DataMember(Order = 5)]
        public string KYCMasters_ActivityType { get; set; }
        [DataMember(Order = 6)]
        public long ID { get; set; }
        [DataMember(Order = 7)]
        public string Changes { get; set; }
        [DataMember(Order = 8)]
        public string Action { get; set; }
    }
}
