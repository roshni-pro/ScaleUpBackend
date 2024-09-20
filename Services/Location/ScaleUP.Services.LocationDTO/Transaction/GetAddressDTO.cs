using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationDTO.Transaction
{
    public class GetAddressDTO
    {
        public string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public int ZipCode { get; set; }
        public string CityName { get; set; }
        public long CityId { get; set; }
        public string StateName { get; set; }
        public long StateId { get; set; }
        public string CountryName { get; set; }
        public long CountryId { get; set; }
        public long Id { get; set; }
        public long AddressTypeId{ get; set; }
        public string AddressTypeName { get; set; }


    }
}
