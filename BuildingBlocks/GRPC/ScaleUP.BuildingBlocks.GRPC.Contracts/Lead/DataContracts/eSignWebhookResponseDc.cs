using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class eSignWebhookResponseDc
    {
        [DataMember(Order = 1)]
        public List<string> files { get; set; }
        [DataMember(Order = 2)]
        public string auditTrail { get; set; }
        [DataMember(Order = 3)]
        public string clientId { get; set; }
        [DataMember(Order = 4)]
        public string documentId { get; set; }
        [DataMember(Order = 5)]
        public string irn { get; set; }
        [DataMember(Order = 6)]
        public List<User> users { get; set; }
        [DataMember(Order = 7)]
        public Messages messages { get; set; }
        [DataMember(Order = 8)]
        public ClientData clientData { get; set; }
        [DataMember(Order = 9)]
        public FileDc fileDc { get; set; }
    }
    [DataContract]
    public class ClientData
    {
        [DataMember(Order = 1)]
        public string caseId { get; set; }
    }
    [DataContract]
    public class Messages
    {
        [DataMember(Order = 1)]
        public string errorCode { get; set; }
        [DataMember(Order = 2)]
        public string message { get; set; }
    }
    [DataContract]
    public class Request
    {
        [DataMember(Order = 1)]
        public string name { get; set; }
        [DataMember(Order = 2)]
        public string email { get; set; }
        [DataMember(Order = 3)]
        public string mobile { get; set; }
        [DataMember(Order = 4)]
        public string status { get; set; }
        [DataMember(Order = 5)]
        public string expiryTime { get; set; }
        [DataMember(Order = 6)]
        public string signUrl { get; set; }
        [DataMember(Order = 7)]
        public bool urlActive { get; set; }
        [DataMember(Order = 8)]
        public bool signed { get; set; }
        [DataMember(Order = 9)]
        public bool rejected { get; set; }
        [DataMember(Order = 10)]
        public bool expired { get; set; }
        [DataMember(Order = 11)]
        public string signType { get; set; }
        [DataMember(Order = 12)]
        public string rejectionReason { get; set; }
    }
    [DataContract]
    public class Signer
    {
        [DataMember(Order = 1)]
        public string name { get; set; }
        [DataMember(Order = 2)]
        public string gender { get; set; }
        [DataMember(Order = 3)]
        public string yob { get; set; }
        [DataMember(Order = 4)]
        public string title { get; set; }
        [DataMember(Order = 5)]
        public string country { get; set; }
        [DataMember(Order = 6)]
        public string state { get; set; }
        [DataMember(Order = 7)]
        public string pincode { get; set; }
    }
    [DataContract]
    public class User
    {
        [DataMember(Order = 1)]
        public Request request { get; set; }
        [DataMember(Order = 2)]
        public Signer signer { get; set; }
        [DataMember(Order = 3)]
        public VerificationResult verificationResult { get; set; }
    }
    [DataContract]
    public class VerificationResult
    {
        [DataMember(Order = 1)]
        public int nameScore { get; set; }
        [DataMember(Order = 2)]
        public bool genderStatus { get; set; }
        [DataMember(Order = 3)]
        public bool yobStatus { get; set; }
        [DataMember(Order = 4)]
        public bool matchStatus { get; set; }
    }

    [DataContract]
    public class FileDc
    {
        [DataMember(Order = 1)]
        public long DocId { get; set; }
        [DataMember(Order = 2)]
        public string filePath { get; set; }
    }
}
