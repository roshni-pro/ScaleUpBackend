using Azure;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LocationModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LocationModels.Transaction
{
    public class Address : BaseAuditableEntity
    {
        public required string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public required int ZipCode { get; set; }

        public long CityId { get; set; }
        [ForeignKey("CityId")]
        public City City { get; set; }
        public ICollection<AddressType> AddressTypeList { get; set; } = null;
    }
}
