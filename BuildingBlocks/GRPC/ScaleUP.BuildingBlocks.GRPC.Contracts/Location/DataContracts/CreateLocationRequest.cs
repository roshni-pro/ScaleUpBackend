using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts
{
    [DataContract]
    public class CreateLocationRequest
    {
        [DataMember(Order =1)]
        public long AddressTypeId { get; set; }
        [DataMember(Order = 2)]
        public string AddressLineOne { get; set; }
        [DataMember(Order = 3)]
        public string? AddressLineTwo { get; set; }
        [DataMember(Order = 4)]
        public string? AddressLineThree { get; set; }
        [DataMember(Order = 5)]
        public int ZipCode { get; set; }
        [DataMember(Order = 6)]
        public long CityId { get; set; }
        [DataMember(Order = 7)]
        public List<long> ExistingLocationIds { get; set; }
    }
}
