using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.DSA
{
    public class KYCConnectorPersonalDetailActivity
    {
        public long LeadMasterId { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        //public DateTime DOB { get; set; }
        //public int Age { get; set; }
        //public string Address { get; set; }
        public string AlternatePhoneNo { get; set; }
        public string EmailId { get; set; }
        public string PresentEmployment { get; set; }
        public string LanguagesKnown { get; set; }
        public string WorkingWithOther { get; set; }
        public string ReferenceName { get; set; }
        public string ReferneceContact { get; set; }
        public string WorkingLocation { get; set; }
        public long CurrentAddressId { get; set; }
        public string MobileNo { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public long? CurentLocationId { get; set; }
        public long? PermanentLocationId { get; set; }
    }
}
