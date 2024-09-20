using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    public class eSignResponseDc
    {
        public string requestId { get; set; }
        public Result result { get; set; }
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
        public DateTime creationDate { get; set; }
        public DateTime expiryDate { get; set; }
        public object signDate { get; set; }
        public object failureReason { get; set; }
    }

    public class Result
    {
        public string file { get; set; }
        public object auditTrail { get; set; }
        public List<VerificationDetail> verificationDetails { get; set; }
    }

    public class VerificationDetail
    {
        public string name { get; set; }
        public string email { get; set; }
        public InvitationStatus invitationStatus { get; set; }
        public CertificateData certificateData { get; set; }
        public VerificationResponse verificationResponse { get; set; }
        public object usedSignatureType { get; set; }
    }

    public class VerificationResponse
    {
        public object smartNamePercentage { get; set; }
        public object nameVerification { get; set; }
        public object yobVerification { get; set; }
        public object genderVerification { get; set; }
    }
}
