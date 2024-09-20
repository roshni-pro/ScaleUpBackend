using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadPersonalDetailDTO : LeadBaseDTO
    {
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string FatherLastName { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string AlternatePhoneNo { get; set; }
        public string EmailId { get; set; }
        public string TypeOfAddress { get; set; }
        public string ResAddress1 { get; set; }
        public string ResAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string PermanentAddressLine1 { get; set; }
        public string PermanentAddressLine2 { get; set; }
        public string PermanentCity { get; set; }
        public string PermanentState { get; set; }
        public string PermanentPincode { get; set; }
        public string ResidenceStatus { get; set; }
        public string? MobileNo { get; set; }
        public string OwnershipType { get; set; }
        public string Marital { get; set; }
        public string? OwnershipTypeProof { get; set; }
        public long? ElectricityBillDocumentId { get; set; }
        public string? IVRSNumber { get; set; }
        public string? OwnershipTypeName { get; set; }
        public string? OwnershipTypeAddress { get; set; }
        public string? OwnershipTypeResponseId { get; set; }
        public string? ElectricityServiceProvider { get; set; }
        public string? ElectricityState { get; set; }
    }
}
