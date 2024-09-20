using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationDTO.Transaction
{
    public class AddAddressDTO
    {
        public long AddressTypeId { get; set; }
        public string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public int ZipCode { get; set; }
        public long CityId { get; set; }
    }

    public class AddressDto
    {
        public long AddressId { get; set; }
        public string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public int ZipCode { get; set; }
        public int CityId { get; set; }
    }
}
