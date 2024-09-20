using System.ComponentModel.DataAnnotations;

namespace ScaleUP.Services.CompanyDTO.Master
{
    public class CompanyDTO
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
        public List<PartnerListDc> PartnerList { get; set; }

    }

    public class PartnerListDc
    {
        public long? PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string MobileNo { get; set; }
    }
    public class GetCompanyRes
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public CompanyDc? ReturnObject { get; set; }
    }
    public class CompanyDc
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
        public List<PartnerListDc> CompanyPartnerDc { get; set; } //Anchor, NBFC
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string? AgreementURL { get; set; }
        public string CustomerAgreementURL { get; set; }
        public string? BusinessPanURL { get; set; }
        public string? WhitelistIRL { get; set; }
        public string? CancelChequeURL { get; set; }
        public double? InterestRate { get; set; }
        public double? OfferMaxRate { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIFSC { get; set; }
        public bool IsDefault { get; set; }
        public long? AgreementDocId { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        public long? CancelChequeDocId { get; set; }
        public long? BusinessPanDocId { get; set; }
        public bool? IsSelfConfiguration { get; set; }
        public long? GSTDocId { get; set; }
        public string? GSTDocumentURL { get; set; }
        public long? MSMEDocId { get; set; }
        public string? MSMEDocumentURL { get; set; }
        public string? ContactPersonName { get; set; }

    }

    public class CompanyLogDc
    {
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIFSC { get; set; }
        public string? CancelChequeDocId { get; set; }
        public string? CancelChequeURL { get; set; }
        public string? ContactPersonName { get; set; }
        public string? BusinessContactEmail { get; set; }
        public string? BusinessContactNo { get; set; }
        public string? BusinessHelpline { get; set; }
        public string? LogoURL { get; set; }
        public string? IsActive { get; set; }
        public string? IsDeleted { get; set; }
        public string? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class CompanyListByCompanyTypeDc
    {
        public long Id { get; set; }
        public string BusinessName { get; set; }
    }
    public class GetEducationMasterListDc
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

}
