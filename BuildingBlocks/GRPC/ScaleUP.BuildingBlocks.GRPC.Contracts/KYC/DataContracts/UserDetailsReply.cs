using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class UserDetailsReply
    {
        [DataMember(Order = 1)]
        public AadharDetailsDc aadharDetail { get; set; }
        [DataMember(Order = 2)]
        public PanDetailsDc panDetail { get; set; }
        [DataMember(Order = 3)]
        public PersonalDetailsDc PersonalDetail { get; set; }
        [DataMember(Order = 4)]
        public BuisnessDetailDc BuisnessDetail { get; set; }
        [DataMember(Order = 5)]
        public List<BankStatementDetailDc> BankStatementDetail { get; set; }
        [DataMember(Order = 6)]
        public MSMEDetailDc MSMEDetail { get; set; }
        [DataMember(Order = 7)]
        public SelfieDetailDc SelfieDetail { get; set; }
        [DataMember(Order = 8)]
        public BankDetailList BankStatementCreditLendingDeail { get; set; }
        [DataMember(Order = 9)]
        public CreditBureauListDc CreditBureauDetails { get; set; }
        [DataMember(Order = 10)]
        public AgreementDetailDc AgreementDetail { get; set; }
        [DataMember(Order = 11)]
        public string? UserId { get; set; }
        [DataMember(Order = 12)]
        public LoanAccountReplyDC LoanAccount { get; set; }
        [DataMember(Order = 13)]
        public long? LeadId { get; set; }
        [DataMember(Order = 14)]
        public DSAprofileInfoDC DSAProfileInfo { get; set; }
        [DataMember(Order = 15)]
        public ConnectorPersonalDetailDc ConnectorPersonalDetail { get; set; }
        [DataMember(Order = 16)]
        public DSAPersonalDetailDc DSAPersonalDetail { get; set; }
        [DataMember(Order = 17)]
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }

    }

    [DataContract]
    public class DSAprofileInfoDC
    {
        [DataMember(Order = 1)]
        public string DSAType { get; set; }
    }

    [DataContract]
    public class AadharDetailsDc
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }
        [DataMember(Order = 2)]
        public DateTime DOB { get; set; }
        [DataMember(Order = 3)]
        public string Gender { get; set; }
        [DataMember(Order = 4)]
        public string HouseNumber { get; set; }
        [DataMember(Order = 5)]
        public string Street { get; set; }
        [DataMember(Order = 6)]
        public string State { get; set; }
        [DataMember(Order = 7)]
        public string Country { get; set; }
        [DataMember(Order = 8)]
        public int? Pincode { get; set; }
        [DataMember(Order = 9)]
        public string CombinedAddress { get; set; }
        [DataMember(Order = 10)]
        public string FrontImageUrl { get; set; }
        [DataMember(Order = 11)]
        public string BackImageUrl { get; set; }
        [DataMember(Order = 12)]
        public string MaskedAadhaarNumber { get; set; }
        [DataMember(Order = 13)]
        public DateTime GeneratedDateTime { get; set; }
        [DataMember(Order = 14)]
        public string Subdistrict { get; set; }
        [DataMember(Order = 15)]
        public string EmailHash { get; set; }
        [DataMember(Order = 16)]
        public string MobileHash { get; set; }
        [DataMember(Order = 17)]
        public int FrontDocumentId { get; set; }
        [DataMember(Order = 18)]
        public int BackDocumentId { get; set; }

        [DataMember(Order = 19)]
        public string UniqueId { get; set; }
        [DataMember(Order = 20)]
        public long? LocationID { get; set; }
        [DataMember(Order = 21)]
        public GetAddressDTO LocationAddress { get; set; }
        [DataMember(Order = 22)]
        public string FatherName { get; set; }

    }
    [DataContract]
    public class PanDetailsDc
    {
        [DataMember(Order = 1)]
        public int Age { get; set; }
        [DataMember(Order = 2)]
        public DateTime DOB { get; set; }
        [DataMember(Order = 3)]
        public DateTime DateOfIssue { get; set; }
        [DataMember(Order = 4)]
        public string FatherName { get; set; }
        [DataMember(Order = 5)]
        public string FrontImageUrl { get; set; }
        [DataMember(Order = 6)]
        public bool Minor { get; set; }
        [DataMember(Order = 7)]
        public string NameOnCard { get; set; }
        [DataMember(Order = 8)]
        public string PanType { get; set; }
        //[DataMember(Order = 9)]
        //public string BackImageUrl { get; set; }
        [DataMember(Order = 9)]
        public string UniqueId { get; set; }
        //[DataMember(Order = 11)]
        //public string Imagepath { get; set; }
        [DataMember(Order = 10)]
        public bool IdScanned { get; set; }
        [DataMember(Order = 11)]
        public long? DocumentId { get; set; }
    }
    [DataContract]
    public class PersonalDetailsDc
    {
        [DataMember(Order = 1)]
        public string FirstName { get; set; }
        [DataMember(Order = 2)]
        public string LastName { get; set; }
        [DataMember(Order = 3)]
        public string Gender { get; set; }
        [DataMember(Order = 4)]
        public string AlternatePhoneNo { get; set; }
        [DataMember(Order = 5)]
        public string EmailId { get; set; }
        [DataMember(Order = 6)]
        public int PermanentAddressId { get; set; }
        [DataMember(Order = 7)]
        public int CurrentAddressId { get; set; }
        [DataMember(Order = 8)]
        public GetAddressDTO PermanentAddress { get; set; }
        [DataMember(Order = 9)]
        public GetAddressDTO CurrentAddress { get; set; }

        [DataMember(Order = 10)]
        public string? MobileNo { get; set; }
        [DataMember(Order = 11)]
        public string? OwnershipTypeProof { get; set; }
        [DataMember(Order = 12)]
        public string OwnershipType { get; set; }
        [DataMember(Order = 13)]
        public string Marital { get; set; }
        [DataMember(Order = 14)]
        public long? ElectricityBillDocumentId { get; set; }
        [DataMember(Order = 15)]
        public string? IVRSNumber { get; set; }
        [DataMember(Order = 16)]
        public string? OwnershipTypeName { get; set; }
        [DataMember(Order = 17)]
        public string? OwnershipTypeAddress { get; set; }
        [DataMember(Order = 18)]
        public string? OwnershipTypeResponseId { get; set; }
        [DataMember(Order = 19)]
        public string? ManualElectricityBillImage { get; set; }
        [DataMember(Order = 20)]
        public string? PanNameOnCard { get; set; }

        [DataMember(Order = 21)]
        public string? MiddleName { get; set; }
        [DataMember(Order = 22)]
        public string? ElectricityServiceProvider { get; set; }
        [DataMember(Order = 23)]
        public string? ElectricityState { get; set; }
    }
    [DataContract]
    public class BuisnessDetailDc
    {
        [DataMember(Order = 1)]
        public string BusinessName { get; set; }
        [DataMember(Order = 2)]
        public DateTime DOI { get; set; }
        [DataMember(Order = 3)]
        public string BusGSTNO { get; set; }
        [DataMember(Order = 4)]
        public string BusEntityType { get; set; }
        [DataMember(Order = 5)]
        public string BusPan { get; set; }
        //[DataMember(Order = 6)]
        //public string BusMonthlySalary { get; set; }
        [DataMember(Order = 6)]
        public long? CurrentAddressId { get; set; }
        [DataMember(Order = 7)]
        public GetAddressDTO CurrentAddress { get; set; }
        [DataMember(Order = 8)]
        public string IncomeSlab { get; set; }
        [DataMember(Order = 9)]
        public double? BuisnessMonthlySalary { get; set; }

        [DataMember(Order = 10)]
        public string BuisnessProof { get; set; }
        [DataMember(Order = 11)]
        public long BuisnessProofDocId { get; set; }

        [DataMember(Order = 12)]
        public string BuisnessDocumentNo { get; set; }

        [DataMember(Order = 13)]
        public string BuisnessProofUrl { get; set; }
        [DataMember(Order = 14)]
        public double? InquiryAmount { get; set; }
        [DataMember(Order = 15)]
        public string SurrogateType { get; set; }
        [DataMember(Order = 16)]
        public int? VintageDays { get; set; }
        [DataMember(Order = 17)]
        public string? AnchorCompanyName { get; set; }
        [DataMember(Order = 18)]
        public string? BuisnessPhotoDocId { get; set; }
        [DataMember(Order = 19)]
        public List<string> BuisnessPhotoUrl { get; set; }
        [DataMember(Order = 20)]
        public int? BusinessVintageDays { get; set; }
    }
    [DataContract]
    public class BankStatementDetailDc
    {

        [DataMember(Order = 1)]
        public string PdfPassword { get; set; }

        [DataMember(Order = 2)]
        public long? DocumentId { get; set; }

        [DataMember(Order = 3)]
        public double? EnquiryAmount { get; set; }

        [DataMember(Order = 4)]
        public string BankOrGSTImageUrl { get; set; }
        [DataMember(Order = 5)]
        public string Umrn { get; set; }

        [DataMember(Order = 6)]
        public string AccountNumber { get; set; }
        [DataMember(Order = 7)]
        public string Accountholdername { get; set; }
        [DataMember(Order = 8)]
        public string AccountType { get; set; }
        [DataMember(Order = 9)]
        public string Bankname { get; set; }
        [DataMember(Order = 10)]
        public string IFSCCode { get; set; }

        [DataMember(Order = 11)]
        public long BankDetailId { get; set; }
        [DataMember(Order = 12)]
        public string Type { get; set; } ////borrower,beneficiary,
    }
    [DataContract]
    public class MSMEDetailDc
    {
        [DataMember(Order = 1)]
        public string BusinessName { get; set; }
        [DataMember(Order = 2)]
        public string BusinessType { get; set; }
        [DataMember(Order = 3)]
        public int Vintage { get; set; }
        [DataMember(Order = 4)]
        public string MSMERegNum { get; set; }

        [DataMember(Order = 5)]
        public int FrontDocumentId { get; set; }
        [DataMember(Order = 6)]
        public string MSMECertificateUrl { get; set; }

    }

    [DataContract]
    public class SelfieDetailDc
    {
        [DataMember(Order = 1)]
        public int? FrontDocumentId { get; set; }
        [DataMember(Order = 2)]
        public string FrontImageUrl { get; set; }
    }

    [DataContract]
    public class BankStatementCreditLendingDc
    {
        [DataMember(Order = 1)]
        public int? DocumentId { get; set; }
        [DataMember(Order = 2)]
        public string ImageUrl { get; set; }
    }
    [DataContract]
    public class BankDetailList
    {
        [DataMember(Order = 1)]
        public List<BankStatementCreditLendingDc> StatementList { get; set; }
        [DataMember(Order = 2)]
        public List<BankStatementCreditLendingDc> SarrogateStatementList { get; set; }
        //public List<BankStatementCreditLendingDc> GSTStatementList { get; set; }
        //[DataMember(Order = 3)]
        //public List<BankStatementCreditLendingDc> ITRStatementList { get; set; }
        [DataMember(Order = 3)]
        public string? SurrogateType { get; set; }
    }
    [DataContract]
    public class CreditBureauListDc
    {
        [DataMember(Order = 1)]
        public string? cibiljson { get; set; }
        [DataMember(Order = 2)]
        public CreditBureauResponseDc CreditScoreReponse { get; set; } = null;
    }
    [DataContract]
    public class AgreementDetailDc
    {
        [DataMember(Order = 1)]
        public string? AgreementUrl { get; set; }
    }

    [DataContract]
    public class ConnectorPersonalDetailDc
    {
        [DataMember(Order = 1)]
        public string FullName { get; set; }
        [DataMember(Order = 2)]
        public string FatherName { get; set; }
        [DataMember(Order = 3)]
        public DateTime DOB { get; set; }
        [DataMember(Order = 4)]
        public int Age { get; set; }
        [DataMember(Order = 5)]
        public string Address { get; set; }
        [DataMember(Order = 6)]
        public string AlternateContactNo { get; set; }
        [DataMember(Order = 7)]
        public string EmailId { get; set; }
        [DataMember(Order = 8)]
        public string PresentEmployment { get; set; }
        [DataMember(Order = 9)]
        public string LanguagesKnown { get; set; }
        [DataMember(Order = 10)]
        public string WorkingWithOther { get; set; }
        [DataMember(Order = 11)]
        public string ReferenceName { get; set; }
        [DataMember(Order = 12)]
        public string ReferneceContact { get; set; }
        [DataMember(Order = 13)]
        public string WorkingLocation { get; set; }

        [DataMember(Order = 14)]
        public string MobileNo { get; set; }
        [DataMember(Order = 15)]
        public string PinCode { get; set; }
        [DataMember(Order = 16)]
        public string City { get; set; }
        [DataMember(Order = 17)]
        public string State { get; set; }
        [DataMember(Order = 18)]
        public int CurrentAddressId { get; set; }
        [DataMember(Order = 19)]
        public GetAddressDTO CurrentAddress { get; set; }
        [DataMember(Order = 20)]
        public string? Gender { get; set; }
    }

    [DataContract]
    public class DSAPersonalDetailDc
    {
        [DataMember(Order = 1)]
        public string GSTStatus { get; set; }
        [DataMember(Order = 2)]
        public string GSTNumber { get; set; }
        [DataMember(Order = 3)]
        public string FirmType { get; set; }
        [DataMember(Order = 4)]
        public string BuisnessDocument { get; set; }
        [DataMember(Order = 5)]
        public string DocumentId { get; set; }
        [DataMember(Order = 6)]
        public string CompanyName { get; set; }
        [DataMember(Order = 7)]
        public string FullName { get; set; }
        [DataMember(Order = 8)]
        public string FatherOrHusbandName { get; set; }
        [DataMember(Order = 9)]
        public DateTime DOB { get; set; }
        [DataMember(Order = 10)]
        public int Age { get; set; }
        [DataMember(Order = 11)]
        public string Address { get; set; }
        [DataMember(Order = 12)]
        public string PinCode { get; set; }
        [DataMember(Order = 13)]
        public string City { get; set; }
        [DataMember(Order = 14)]
        public string State { get; set; }
        [DataMember(Order = 15)]
        public string AlternatePhoneNo { get; set; }
        [DataMember(Order = 16)]
        public string EmailId { get; set; }
        [DataMember(Order = 17)]
        public string PresentOccupation { get; set; }
        [DataMember(Order = 18)]
        public string NoOfYearsInCurrentEmployment { get; set; }
        [DataMember(Order = 19)]
        public string Qualification { get; set; }
        [DataMember(Order = 20)]
        public string LanguagesKnown { get; set; }
        [DataMember(Order = 21)]
        public string WorkingWithOther { get; set; }
        [DataMember(Order = 22)]
        public string ReferneceName { get; set; }
        [DataMember(Order = 23)]
        public string ReferneceContact { get; set; }
        [DataMember(Order = 24)]
        public string WorkingLocation { get; set; }

        [DataMember(Order = 25)]
        public string MobileNo { get; set; }
        [DataMember(Order = 26)]
        public int CurrentAddressId { get; set; }
        [DataMember(Order = 27)]
        public GetAddressDTO CurrentAddress { get; set; }
        [DataMember(Order = 28)]
        public int PermanentAddressId { get; set; }
        [DataMember(Order = 29)]
        public GetAddressDTO PermanentAddress { get; set; }
        [DataMember(Order = 30)]
        public string BuisnessDocImg { get; set; }
        [DataMember(Order = 31)]
        public string? Gender { get; set; }
    }

}
