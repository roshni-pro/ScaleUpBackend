using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class UpdateKYCStatusDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        //public long ActivityMasterId { get; set; }
        public string ActivityMasterName { get; set; }
        [DataMember(Order = 3)]
        public string? UserName { get; set; }
    }

    [DataContract]
    public class UpdateKYCStatusResponseDc
    {
        [DataMember(Order = 1)]
        public string UserName { get; set; }
        [DataMember(Order = 2)]
        public string ProductCode { get; set; }
    }
}
