using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]

    public class CompanyLocationDTO
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long LocationId { get; set; }
    }

    [DataContract]
    public class AddCompanyDTO
    {
        [DataMember(Order = 1)]
        public long? CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string GSTNo { get; set; }
        [DataMember(Order = 3)]
        public string? PanNo { get; set; }
        [DataMember(Order = 4)]
        public string BusinessName { get; set; }
        [DataMember(Order = 5)]
        public string? LandingName { get; set; }
        [DataMember(Order = 6)]
        public string BusinessContactEmail { get; set; }
        [DataMember(Order = 7)]
        public string BusinessContactNo { get; set; }
        [DataMember(Order = 8)]
        public string? APIKey { get; set; }
        [DataMember(Order = 9)]
        public string? APISecretKey { get; set; }
        [DataMember(Order = 10)]
        public string? LogoURL { get; set; }
        [DataMember(Order = 11)]
        public string? BusinessHelpline { get; set; }
        [DataMember(Order = 12)]
        public int BusinessTypeId { get; set; }
        [DataMember(Order = 13)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 14)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 15)]
        public string? AgreementURL { get; set; }
        [DataMember(Order = 16)]
        public string? CustomerAgreementURL { get; set; }
        [DataMember(Order = 17)]
        public string? BusinessPanURL { get; set; }
        [DataMember(Order = 18)]
        public string? WhitelistURL { get; set; }
        [DataMember(Order = 19)]
        public string? CancelChequeURL { get; set; }

        [DataMember(Order = 20)]
        public string? BankName { get; set; }
        [DataMember(Order = 21)]
        public string? BankAccountNumber { get; set; }
        [DataMember(Order = 22)]
        public string? BankIFSC { get; set; }
        [DataMember(Order = 23)]
        public string CompanyType { get; set; }
        [DataMember(Order = 24)]
        public List<PartnerListDc> PartnerList { get; set; }
        [DataMember(Order = 25)]
        public long? CancelChequeDocId { get; set; }
        [DataMember(Order = 26)]
        public long? BusinessPanDocId { get; set; }
        [DataMember(Order = 27)]
        public long? CustomerAgreementDocId { get; set; }
        [DataMember(Order = 28)]
        public long? AgreementDocId { get; set; }
        [DataMember(Order = 29)]
        public string CompanyCode { get; set; }
        [DataMember(Order = 30)]
        public bool? IsSelfConfiguration { get; set; }
        [DataMember(Order = 31)]
        public long? GSTDocId { get; set; }
        [DataMember(Order = 32)]
        public string? GSTDocumentURL { get; set; }
        [DataMember(Order = 33)]
        public long? MSMEDocId { get; set; }
        [DataMember(Order = 34)]
        public string? MSMEDocumentURL { get; set; }
        [DataMember(Order = 35)]
        public string? ContactPersonName { get; set; }
        [DataMember(Order = 36)]
        public bool CompanyStatus { get; set; }
        [DataMember(Order = 37)]
        public string? AccountType { get; set; }
        [DataMember(Order = 38)]
        public long? PanDocId { get; set; }
        [DataMember(Order = 39)]
        public string? PanURL { get; set; }
        [DataMember(Order = 40)]
        public bool IsDSA { get; set; }

    }
    [DataContract]
    public class PartnerListDc
    {
        [DataMember(Order = 1)]
        public long? PartnerId { get; set; }
        [DataMember(Order = 2)]
        public string PartnerName { get; set; }
        [DataMember(Order = 3)]
        public string MobileNo { get; set; }
    }

    [DataContract]
    public class AddfinancialLiaisonDetailsDTO
    {
        [DataMember(Order = 1)]
        public string FirstName { get; set; }
        [DataMember(Order = 2)]
        public string LastName { get; set; }
        [DataMember(Order = 3)]
        public string ContactNo { get; set; }
        [DataMember(Order = 4)]
        public string EmailAddress { get; set; }
        [DataMember(Order = 5)]
        public long CompanyId { get; set; }
    }
}
