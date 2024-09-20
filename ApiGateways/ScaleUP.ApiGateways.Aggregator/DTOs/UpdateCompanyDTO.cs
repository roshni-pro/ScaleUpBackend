namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class UpdateCompanyDTO
    {
        public long? CompanyId { get; set; }
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
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        public string? CustomerAgreementURL { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        public string? BusinessPanURL { get; set; }
        public long? BusinessPanDocId { get; set; }
        public string? WhitelistURL { get; set; }
        public string? CancelChequeURL { get; set; }
        public long? CancelChequeDocId { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIFSC { get; set; }
        public string CompanyType { get; set; }
        public bool? IsSelfConfiguration { get; set; }
        public long? GSTDocId { get; set; }
        public string? GSTDocumentURL { get; set; }
        public long? MSMEDocId { get; set; }
        public string? MSMEDocumentURL { get; set; }
        public string? ContactPersonName { get; set; }
        public bool CompanyStatus { get; set; }
        public UpdateAddressDc CompanyAddress { get; set; }
        public List<PartnerListDc> PartnerList { get; set; }
    }

    public class UpdateAddressDc
    {
        public long AddressTypeId { get; set; }
        public string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public int ZipCode { get; set; }
        public int CityId { get; set; }
    }

    public class UpdateCompanyResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public UpdateCompanyDTO UpdatedCompanyDTO { get; set; }
        public UpdateAddressDc UpdatedAddressDc { get; set; }
    }
}
