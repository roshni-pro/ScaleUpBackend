using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class ValidAuthenticationPanResDTO
    {
        [JsonProperty("result")]
        public dynamic person { get; set; }
        public string request_id { get; set; }
        [JsonProperty("status-code")]
        public int StatusCode { get; set; }
        public string error { get; set; }
    }
    public class ValidAuthenticationPanPost
    {
        public string consent { get; set; }
        public string pan { get; set; }
    }
    public class GetUrlTokenDTO
    {
        public string url { get; set; }
        public string token { get; set; }
        public string CompanyCode { get; set; }
        public long id { get; set; }
        public string ApiSecretKey { get; set; }
    }

    public class ValidPanResDTO
    {
        public string NameOnPancard { get; set; }
        public string? FathersNameOnPancard { get; set; }
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
        public string request_id { get; set; }
        [JsonProperty("status-code")]
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
