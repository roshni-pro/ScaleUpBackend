namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadPersonalDetailDTO
    {
        public long LeadMasterId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string FatherLastName { get; set; }
        public string Gender { get; set; }
        public string AlternatePhoneNo { get; set; }
        public string EmailId { get; set; }
        public string TypeOfAddress { get; set; }
        public string ResAddress1 { get; set; }
        public string ResAddress2 { get; set; }
        public int? Pincode { get; set; }
        public long? City { get; set; }
        public long? State { get; set; }
        public string PermanentAddressLine1 { get; set; }
        public string PermanentAddressLine2 { get; set; }
        public int? PermanentPincode { get; set; }
        public long? PermanentCity { get; set; }
        public long? PermanentState { get; set; }
        public string ResidenceStatus { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }
        public string? MobileNo { get; set; }
        public string BusGSTNO { get; set;}
        public string OwnershipType { get; set; }
        public string Marital { get; set; }
        public string? OwnershipTypeProof { get; set; }
        public long? ElectricityBillDocumentId { get; set; }
        public string? IVRSNumber { get; set; }
        public string? OwnershipTypeName { get; set; }
        public string? OwnershipTypeAddress { get; set; }
        public string? OwnershipTypeResponseId { get; set; }
        public string ManulaElectrictyBillImage { get; set; }
        public string? ElectricityServiceProvider { get; set; }
        public string? ElectricityState { get; set; }
    }

    public class DSAPersonalDetailDTO
    {
        public string GSTStatus { get; set; }
        public string GSTNumber { get; set; }
        public string FirmType { get; set; }
        public string BuisnessDocument { get; set; }
        public string DocumentId { get; set; }
        public string CompanyName { get; set; }
        public string FullName { get; set; }
        public string FatherOrHusbandName { get; set; }
        public DateTime? DOB { get; set; }
        public int? Age { get; set; }
        public string Address { get; set; }
        public string? PinCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string AlternatePhoneNo { get; set; }
        public string EmailId { get; set; }
        public string PresentOccupation { get; set; }
        public string NoOfYearsInCurrentEmployment { get; set; }
        public string Qualification { get; set; }
        public string LanguagesKnown { get; set; }
        public string WorkingWithOther { get; set; }
        public string ReferneceName { get; set; }
        public string ReferneceContact { get; set; }
        public string WorkingLocation { get; set; }
        public string? CityId { get; set; }
        public string? StateId { get; set; }
        public string CompanyAddress { get; set; }
        public string? CompanyPinCode { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyState { get; set; }
        public string BuisnessDocImg { get; set; }
        public string? CompanyCityId { get; set; }
        public string? CompanyStateId { get; set; }
        public long? WorkingLocationCityId { get; set; }
        public long? WorkingLocationStateId { get; set; }
        public string? WorkingLocationStateName { get; set; }
    }

    public class ConnectorPersonalDetailDTO
    {
        public string FullName { get; set; }
        public string FatherName { get; set; }
        public DateTime? DOB { get; set; }
        public int? Age { get; set; }
        public string Address { get; set; }
        public string AlternatePhoneNo { get; set; }
        public string EmailId { get; set; }
        public string PresentEmployment { get; set; }
        public string LanguagesKnown { get; set; }
        public string WorkingWithOther { get; set; }
        public string ReferenceName { get; set; }
        public string ReferneceContact { get; set; }
        public string WorkingLocation { get; set; }
        public string? Pincode { get; set; }
        public string? CityId { get; set; }
        public string? StateId { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public long? WorkingLocationCityId { get; set; }
        public long? WorkingLocationStateId { get; set; }
        public string? WorkingLocationStateName { get; set; }
    }
}
