using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class CompanyAddressAndDetailsResponse
    {
        [DataMember(Order = 1)]
        public string GSTNo { get; set; }
        [DataMember(Order = 2)]
        public string? PanNo { get; set; }
        [DataMember(Order = 3)]
        public string BusinessName { get; set; }
        [DataMember(Order = 4)]
        public string? LandingName { get; set; }
        [DataMember(Order = 5)]
        public string BusinessContactEmail { get; set; }
        [DataMember(Order = 6)]
        public string BusinessContactNo { get; set; }
        [DataMember(Order = 7)]
        public string? APIKey { get; set; }
        [DataMember(Order = 8)]
        public string? APISecretKey { get; set; }
        [DataMember(Order = 9)]
        public string? LogoURL { get; set; }
        [DataMember(Order = 10)]
        public string? BusinessHelpline { get; set; }
        [DataMember(Order = 11)]
        public int BusinessTypeId { get; set; }
        [DataMember(Order = 12)]
        public string? CompanyCode { get; set; }
        [DataMember(Order = 13)]
        public string CompanyType { get; set; } //Anchor, NBFC
        [DataMember(Order = 14)]
        public List<PartnerListDTO> CompanyPartnerDc { get; set; } //Anchor, NBFC
        [DataMember(Order = 15)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 16)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 17)]
        public string? AgreementURL { get; set; }
        [DataMember(Order = 18)]
        public string CustomerAgreementURL { get; set; }
        [DataMember(Order = 19)]
        public string? BusinessPanURL { get; set; }
        [DataMember(Order = 20)]
        public string? WhitelistIRL { get; set; }
        [DataMember(Order = 21)]
        public string? CancelChequeURL { get; set; }
        [DataMember(Order = 22)]
        public double? InterestRate { get; set; }
        [DataMember(Order = 23)]
        public double? OfferMaxRate { get; set; }
        [DataMember(Order = 24)]
        public string? BankName { get; set; }
        [DataMember(Order = 25)]
        public string? BankAccountNumber { get; set; }
        [DataMember(Order = 26)]
        public string? BankIFSC { get; set; }
        [DataMember(Order = 27)]
        public bool IsDefault { get; set; }
        [DataMember(Order = 28)]
        public long? AgreementDocId { get; set; }
        [DataMember(Order = 29)]
        public long? CustomerAgreementDocId { get; set; }
        [DataMember(Order = 30)]
        public long? CancelChequeDocId { get; set; }
        [DataMember(Order = 31)]
        public long? BusinessPanDocId { get; set; }
        [DataMember(Order = 32)]
        public bool? IsSelfConfiguration { get; set; }
        [DataMember(Order = 33)]
        public long? GSTDocId { get; set; }
        [DataMember(Order = 34)]
        public string? GSTDocumentURL { get; set; }
        [DataMember(Order = 35)]
        public long? MSMEDocId { get; set; }
        [DataMember(Order = 36)]
        public string? MSMEDocumentURL { get; set; }
        [DataMember(Order = 37)]
        public string? ContactPersonName { get; set; }
        [DataMember(Order = 38)]
        public List<long> companyLocationIds { get; set; }
        [DataMember(Order = 39)]
        public bool CompanyStatus { get; set; }
        [DataMember(Order = 40)]
        public string AccountType { get; set; }
        [DataMember(Order = 41)]
        public long? PanDocId { get; set; }
        [DataMember(Order = 42)]
        public string? PanURL { get; set; }
        [DataMember(Order = 43)]
        public string? AccountHolderName { get; set;}
        [DataMember(Order = 44)]
        public string? BranchName { get; set; }
        [DataMember(Order = 45)]
        public FinancialLiaisonDetailDTO? FinancialLiaisonDetailDTO { get; set; }
    }

    [DataContract]
    public class PartnerListDTO
    {
        [DataMember(Order = 1)]
        public long? PartnerId { get; set; }
        [DataMember(Order = 2)]
        public string PartnerName { get; set; }
        [DataMember(Order = 3)]
        public string MobileNo { get; set; }
    }

    [DataContract]
    public class FinancialLiaisonDetailDTO
    {
        [DataMember(Order = 1)]
        public string? FirstName { get; set; }
        [DataMember(Order = 2)]
        public string? LastName { get; set; }
        [DataMember(Order = 3)]
        public string? ContactNo { get; set; }
        [DataMember(Order = 4)]
        public string? EmailAddress { get; set;}
    }
}
