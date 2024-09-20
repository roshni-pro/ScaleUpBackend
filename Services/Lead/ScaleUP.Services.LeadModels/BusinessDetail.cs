using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class BusinessDetail : BaseAuditableEntity
    {
        [StringLength(300)]
        public string BusinessName { get; set; }
        public DateTime DOI { get; set; }
        [StringLength(50)]
        public string? BusGSTNO { get; set; }
        [StringLength(100)]
        public string BusEntityType { get; set; }
        [StringLength(15)]
        public string? BusMaskPan { get; set; }
        [StringLength(100)]
        public string IncomeSlab { get; set; }
        [StringLength(100)]
        public string OwnershipType { get; set; }
        [StringLength(100)]
        public string? ElectricityNumber { get; set; }
        [StringLength(300)]
        public string? ElectricityOwnerName { get; set; }
        [StringLength(500)]
        public string? ElectricityOwnerAddress { get; set; }
        public double? BuisnessMonthlySalary { get; set; }
        [StringLength(300)]
        public string? AddressLineOne { get; set; }
        [StringLength(300)]
        public string? AddressLineTwo { get; set; }
        [StringLength(300)]
        public string? AddressLineThree { get; set; }
        public int ZipCode { get; set; }
        [StringLength(200)]
        public string CityName { get; set; }
        [StringLength(200)]
        public string StateName { get; set; }
        [StringLength(200)]
        public string CountryName { get; set; }
        [StringLength(200)]
        public required long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }
        public double? InquiryAmount { get;set; }
    }
}
