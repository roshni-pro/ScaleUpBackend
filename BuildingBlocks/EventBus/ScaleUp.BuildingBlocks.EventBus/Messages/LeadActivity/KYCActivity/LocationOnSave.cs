using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity
{
    public class LocationOnSave
    {
        public required string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public required int ZipCode { get; set; }

        public long CityId { get; set; }
        public string AddressType { get; set; }
    }

    public class LocationRawOnSave
    {
        public required string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public required int ZipCode { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string AddressType { get; set; }
    }

    public class LocationOnSaveResponse
    {
        public long Id { get; set; }
        public string AddressType { get; set; }
    }
}
