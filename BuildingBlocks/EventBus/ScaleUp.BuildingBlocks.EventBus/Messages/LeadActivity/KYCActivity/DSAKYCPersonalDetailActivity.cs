﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity
{
    public class DSAKYCPersonalDetailActivity
    {
        public long LeadMasterId { get; set; }
        public  string GSTRegistrationStatus { get; set; }
        public  string GSTNumber { get; set; }
        public  string FirmType { get; set; }
        public  string BuisnessDocument { get; set; }
        public  string DocumentId { get; set; }
        public  string CompanyName { get; set; }
        public  string FullName { get; set; }
        public  string FatherOrHusbandName { get; set; }
        //public DateTime DOB { get; set; }
        //public int Age { get; set; }
        //public  string Address { get; set; }
        //public  string PinCode { get; set; }
        //public  string City { get; set; }
        //public  string State { get; set; }
        public  string AlternatePhoneNo { get; set; }
        public  string EmailId { get; set; }
        public  string PresentOccupation { get; set; }
        public  string NoOfYearsInCurrentEmployment { get; set; }
        public  string Qualification { get; set; }
        public  string LanguagesKnown { get; set; }
        public  string WorkingWithOther { get; set; }
        public  string ReferneceName { get; set; }
        public  string ReferneceContact { get; set; }
        public  string ReferneceLocation { get; set; }
    }
}