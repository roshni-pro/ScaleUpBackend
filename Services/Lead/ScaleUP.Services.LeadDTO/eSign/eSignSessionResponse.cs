using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.eSign
{

    public class eSignSessionResponse
    {
        public string requestId { get; set; }
        public Result result { get; set; }
        public int statusCode { get; set; }
        public string errorString { get; set; }
    }
    public class Result
    {
        public string documentId { get; set; }
        public List<SigningDetail> signingDetails { get; set; }
    }
    public class SigningDetail
    {
        public string name { get; set; }
        public string signUrl { get; set; }
        public int expiryTime { get; set; }

    }
}
