using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.CompanyModels.Master;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.CompanyModels
{
    [Table(nameof(Companies))]
    public class Companies : BaseAuditableEntity
    {
        [StringLength(15)]
        public required string GSTNo { get; set; }
        public long? GSTDocId { get; set; }
        [StringLength(1000)]
        public string? GSTDocumentURL { get; set; }
        public long? MSMEDocId { get; set; }
        [StringLength(1000)]
        public string? MSMEDocumentURL { get; set; }
        [MaxLength(10)]
        public string? PanNo { get; set; }
        [StringLength(100)]
        public required string BusinessName { get; set; }
        [StringLength(100)]
        public string? LendingName { get; set; }
        [StringLength(100)]
        public required string BusinessContactEmail { get; set; }
        [StringLength(12)]
        public required string BusinessContactNo { get; set; }
        [StringLength(30)]
        public string? ContactPersonName { get; set; }
        public string? APIKey { get; set; }
        public string? APISecretKey { get; set; }
        [StringLength(1000)]
        public string? LogoURL { get; set; }
        [MaxLength(12)]
        public string? BusinessHelpline { get; set; }
        public required int BusinessTypeId { get; set; }
        public required bool IsDefault { get; set; }
        public long? BusinessPanDocId { get; set; }
        [StringLength(1000)]
        public string? BusinessPanURL { get; set; }
        [StringLength(1000)]
        public string? WhitelistURL { get; set; }
        [StringLength(1000)]
        public string? CancelChequeURL { get; set; }
        public long? CancelChequeDocId { get; set; }
        [StringLength(100)]
        public string? BankName { get; set; }
        [StringLength(35)]
        public string? BankAccountNumber { get; set; }
        [StringLength(12)]
        public string? BankIFSC { get; set; }

        [StringLength(50)]
        public string? CompanyCode { get; set; } //Anchor ,NBFC Code
        public required string CompanyType { get; set; } //Anchor, NBFC
        public bool? IsSelfConfiguration { get; set; }
        public ICollection<CompanyLocation> CompanyLocations { get; set; }
        public ICollection<CompanyPartnerDetail> CompanyPartnerDetails { get; set; }

        [StringLength(50)]
        public string? IdentificationCode { get; set; }

        [StringLength(15)]
        public string? AccountType { get; set; }
        public long? PanDocId { get; set; }
        [StringLength(1000)]
        public string? PanURL { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        [StringLength(1000)]
        public string? CustomerAgreementURL { get; set; }
        public string? AgreementURL { get; set; }
        public long? AgreementDocId { get; set; }
        [StringLength(1000)]
        public string? NBFCInvoiceTemplateURL { get; set; }
        public long? NBFCInvoiceTemplateDocId { get; set; }
        public string? AccountHolderName { get; set; }
        public string? BranchName { get; set; }

        [StringLength(500)]
        public string? DSAAgreementURL { get; set; }
        [StringLength(500)]
        public string? ConnectorAgreementURL { get; set; }
        public bool IsDSA { get; set; } //dsa : true => DSA/Connector anchor Company 

    }

}
