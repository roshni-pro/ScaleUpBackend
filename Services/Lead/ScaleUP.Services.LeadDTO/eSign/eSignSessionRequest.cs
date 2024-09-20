using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.eSign
{
    public class eSignSessionRequest
    {
        public string documentUrl { get; set; }
        public string documentB64 { get; set; }
        public string referenceNumber { get; set; }
        public string documentName { get; set; }
        public bool isSequence { get; set; }
        public string reviewerMail { get; set; }
        public string templateId { get; set; }
        public string redirectUrl { get; set; }
        public IList<SignerInfo> signerInfo { get; set; }
        public ClientData clientData { get; set; }

    }
    public class SigningInfo
    {
        public string pageNo { get; set; }
        public string xCordinate { get; set; }
        public string yCordinate { get; set; }

    }
    public class SignerInfo
    {
        public string name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public IList<SigningInfo> signingInfo { get; set; }

    }
    public class ClientData
    {
        public string caseId { get; set; }
    }
}

