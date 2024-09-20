using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.eSign
{
    public class eSignDocumentResponseDc
    {
        public string requestId { get; set; }

        [JsonPropertyName("Result")]
        public DocumentResult result { get; set; }
        public int statusCode { get; set; }
    }
    public class CertificateData
    {
        public object name { get; set; }
        public object yob { get; set; }
        public object gender { get; set; }
        public object title { get; set; }
        public object state { get; set; }
    }
    public class InvitationStatus
    {
        public bool active { get; set; }
        public bool signed { get; set; }
        public bool rejected { get; set; }
        public string rejectionReason { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime expiryDate { get; set; }
        public string signDate { get; set; }
        public string failureReason { get; set; }

    }
    public class DocumentResult
    {
        public string file { get; set; }
        public string auditTrail { get; set; }
        public List<VerificationDetail> verificationDetails { get; set; }
    }
    public class VerificationDetail
    {
        public string name { get; set; }
        public string email { get; set; }
        public InvitationStatus invitationStatus { get; set; }
        public CertificateData certificateData { get; set; }
        public VerificationResponse verificationResponse { get; set; }
        public string usedSignatureType { get; set; }
    }
    public class VerificationResponse
    {
        public string smartNamePercentage { get; set; }
        public string nameVerification { get; set; }
        public string yobVerification { get; set; }
        public string genderVerification { get; set; }
    }
}
