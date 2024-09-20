using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadBusinessDetailDTO
    {
        public string businessName { get; set; }
        public DateTime doi { get; set; }
        public string busGSTNO { get; set; }
        public string busEntityType { get; set; }
        public string busPan { get; set; }
        public string addressLineOne { get; set; }
        public string? addressLineTwo { get; set; }
        public string? addressLineThree { get; set; }
        public int zipCode { get; set; }
        public long cityId { get; set; }
        public long stateId { get; set; }
        public double? buisnessMonthlySalary { get; set; }
        public string incomeSlab { get; set; }

        public string buisnessProof { get; set; }
        public string buisnessProofUrl { get; set; }
        public long buisnessProofDocId { get; set; }
        public string buisnessDocumentNo { get; set; }
        public double? InquiryAmount { get; set; }
        public string SurrogateType { get; set; }
        //public string ownershipType { get; set; }
        //public string? customerElectricityNumber { get; set; }
        public bool? Status { get; set; }
        public string? Message { get; set; }
   
    }
}