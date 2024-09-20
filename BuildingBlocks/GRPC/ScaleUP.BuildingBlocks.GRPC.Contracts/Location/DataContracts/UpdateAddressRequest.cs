using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts
{
    [DataContract]
    public class UpdateAddressRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string AddCorrLine1 { get; set; }
        [DataMember(Order = 3)]
        public string AddCorrLine2 { get; set; }
        [DataMember(Order = 4)]
        public string AddCorrLine3 { get; set; }
        [DataMember(Order = 5)]
        public string AddCorrPincode { get; set; }
        [DataMember(Order = 6)]
        public string AddCorrCity { get; set; }
        [DataMember(Order = 7)]
        public string UserId { get; set; }
        [DataMember(Order = 8)]
        public long AddressId { get; set; }
        [DataMember(Order = 9)]
        public string AddressType { get; set; }
        [DataMember(Order = 10)]
        public long CurrentAddressId { get; set; }
        [DataMember(Order = 11)]
        public string AddCorrStateName { get; set; }
        [DataMember(Order = 12)]
        public string AddCorrCityName { get; set; }
        [DataMember(Order = 13)]
        public string? ProductCode { get; set; }
    }
}
