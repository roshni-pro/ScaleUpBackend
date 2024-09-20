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
    public class PersonalDetail : BaseAuditableEntity
    {
        [StringLength(200)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string? LastName { get; set; }
        [StringLength(200)]
        public string FatherName { get; set; }
        [StringLength(100)]
        public string? FatherLastName { get; set; }
        [StringLength(100)]
        public string? MiddleName { get; set; }
        [StringLength(13)]
        public string? MobileNo { get; set; }
        [StringLength(13)]
        public string PanMaskNO { get; set; }
        [StringLength(15)]
        public string AadhaarMaskNO { get; set; }
        public DateTime DOB { get; set; }
        [StringLength(10)]
        public string Gender { get; set; }
        [StringLength(13)]
        public string? AlternatePhoneNo { get; set; }
        [StringLength(200)]
        public string EmailId { get; set; }
        [StringLength(300)]
        public string CurrentAddressLineOne { get; set; }
        [StringLength(300)]
        public string? CurrentAddressLineTwo { get; set; }
        [StringLength(300)]
        public string? CurrentAddressLineThree { get; set; }
        public int CurrentZipCode { get; set; }
        [StringLength(200)]
        public string CurrentCityName { get; set; }
        [StringLength(100)]
        public string CurrentStateName { get; set; }
        [StringLength(100)]
        public string CurrentCountryName { get; set; }
        [StringLength(300)]
        public string PermanentAddressLineOne { get; set; }
        [StringLength(300)]
        public string? PermanentAddressLineTwo { get; set; }
        [StringLength(300)]
        public string? PermanentAddressLineThree { get; set; }
        public int PermanentZipCode { get; set; }
        [StringLength(200)]
        public string PermanentCityName { get; set; }
        [StringLength(200)]
        public string PermanentStateName { get; set; }
        [StringLength(200)]
        public string PermanentCountryName { get; set; }
        public required long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }
        [StringLength(1000)]
        public string AadharFrontImage { get; set; }
        [StringLength(1000)]
        public string AadharBackImage { get; set; }
        [StringLength(1000)]
        public string PanFrontImage { get; set; }
        [StringLength(1000)]
        public string? PanNameOnCard { get; set;}
        [StringLength(1000)]
        public string? SelfieImageUrl { get;set; }
        public string Marital { get; set; }

    }
}
