using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class CompanyAddressDetailsDTO
    {
        public bool Status { get; set; }
        public string Message  { get; set; }
        public CompanyAddressDetails Response { get; set; }

    }
    public class CompanyAddressDetails
    {
        public string GSTNo { get; set; }
        public string? PanNo { get; set; }
        public string BusinessName { get; set; }
        public string? LandingName { get; set; }
        public string BusinessContactEmail { get; set; }
        public string BusinessContactNo { get; set; }
        public string? APIKey { get; set; }
        public string? APISecretKey { get; set; }
        public string? LogoURL { get; set; }
        public string? BusinessHelpline { get; set; }
        public int BusinessTypeId { get; set; }
        public string? CompanyCode { get; set; }
        public string CompanyType { get; set; } //Anchor, NBFC
        public List<CompanyPartnersDc> PartnerList { get; set; } //Anchor, NBFC
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        public string CustomerAgreementURL { get; set; }
        public string? BusinessPanURL { get; set; }
        public long? BusinessPanDocId { get; set; }
        public string? WhitelistURL { get; set; }
        public string? CancelChequeURL { get; set; }
        public double? InterestRate { get; set; }
        public double? OfferMaxRate { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIFSC { get; set; }
        public bool IsDefault { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        public long? CancelChequeDocId { get; set; }
        public bool? IsSelfConfiguration { get; set; }
        public long? GSTDocId { get; set; }
        public string? GSTDocumentURL { get; set; }
        public long? MSMEDocId { get; set; }
        public string? MSMEDocumentURL { get; set; }
        public string? ContactPersonName { get; set; }
        public bool CompanyStatus { get; set; }
        public string AccountType { get; set; }
        public long? PanDocId { get; set; }
        public string? PanURL { get; set; }
        public string? AccountHolderName { get; set; }
        public string? BranchName { get; set; }
        public List<GetAddressdc> CompanyAddress { get; set; }
        public FinancialLiaisonDetailDc FinancialLiaisonDetails { get; set; }
    }

    public class GetAddressdc
    {
        public string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public int ZipCode { get; set; }
        public string CityName { get; set; }
        public long CityId { get; set; }
        public string StateName { get; set; }
        public long StateId { get; set; }
        public string CountryName { get; set; }
        public long CountryId { get; set; }
        public long Id { get; set; }
        public long AddressTypeId { get; set; }
        public string AddressTypeName { get; set; }

    }

    public class CompanyPartnersDc
    {
        public long? PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string MobileNo { get; set; }
    }
    public class FinancialLiaisonDetailDc
    {
        //public string? FirstName { get; set; }
        //public string? LastName { get; set; }
        //public string? ContactNo { get; set; }
        //public string? EmailAddress { get; set; }
        public string? FinancialLiaisonFirstName { get; set; }
        public string? FinancialLiaisonLastName { get; set; }
        public string? FinancialLiaisonContactNo { get; set; }
        public string? FinancialLiaisonEmailAddress { get; set; }
    }
}
