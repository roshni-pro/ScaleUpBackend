using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class KarzaPanProfileResponseDTO
    {
        public string requestId { get; set; }
        public ProfileResultDTO result { get; set; }
        public int statusCode { get; set; }
        public string message { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class ProfileResultDTO
    {
        public string pan { get; set; }
        public string name { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public bool aadhaarLinked { get; set; }
        public string fathersName { get; set; }
        public ProfileAddressDTO adddress { get; set; }
    }

    public class ProfileAddressDTO
    {
        public string buildingName { get; set; }
        public string locality { get; set; }
        public string streetName { get; set; }
        public string pinCode { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
    }
}
