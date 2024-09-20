using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts
{
    [DataContract]
    public class CompanyAddressReply
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public List<GetAddressDTO> GetAddressDTO { get; set; } = null;
    }

    [DataContract]
    public class GetAddressDTO
    {
        [DataMember(Order = 1)]
        public string AddressLineOne { get; set; }
        [DataMember(Order = 2)]
        public string? AddressLineTwo { get; set; }
        [DataMember(Order = 3)]
        public string? AddressLineThree { get; set; }
        [DataMember(Order = 4)]
        public int ZipCode { get; set; }
        [DataMember(Order = 5)]
        public string CityName { get; set; }
        [DataMember(Order = 6)]
        public long CityId { get; set; }
        [DataMember(Order = 7)]
        public string StateName { get; set; }
        [DataMember(Order = 8)]
        public long StateId { get; set; }
        [DataMember(Order = 9)]
        public string CountryName { get; set; }
        [DataMember(Order = 10)]
        public long CountryId { get; set; }
        [DataMember(Order = 11)]
        public long Id { get; set; }
        [DataMember(Order = 12)]
        public long AddressTypeId { get; set; }
        [DataMember(Order = 13)]
        public string AddressTypeName { get; set; }
    }

}
