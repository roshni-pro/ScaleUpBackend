using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class CompanyListResponse
    {
        public List<Companylist> Companylist { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }

    }

    public class Companylist
    {
        public long Id { get; set; }
        public string LendingName { get; set; }
        public string BusinessName { get; set; }
        public string GSTNumber { get; set; }
        public string CompanyType { get; set; }
        public bool IsActive { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        //public string AgreementUrl { get; set; }
        //public long? AgreementDocId { get; set; }
        //public DateTime? AgreementStartDate { get; set; }
        //public DateTime? AgreementEndDate { get; set; }
        public string Pan { get; set; }
        public string BusinessPanURL { get; set; }
        public long? BusinessPanDocId { get; set; }
        public bool IsDefault { get; set; }
        public double GstRate { get; set; }
        public long? CustomerAgreementDocId { get; set; }
        public string CustomerAgreementURL { get; set; }
        public long? CancelChequeDocId { get; set; }
        public string CancelChequeURL { get; set; }
        public int BusinessTypeId { get; set; }
        public string CompanyCode { get; set; }
        public List<PartnerList> PartnerDetailsList { get; set; }
        public List<Address> Addresses { get; set; }
        public List<configuration> Configuration { get; set; }
        public int TotalRecords { get; set; }

    }

    public class Address
    {
        public string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public int? ZipCode { get; set; }
        public string CityName { get; set; }
        public long CityId { get; set; }
        public string StateName { get; set; }
        public long StateId { get; set; }
        public string CountryName { get; set; }
        public long CountryId { get; set; }
        public long Id { get; set; }
        public long AddressTypeId { get; set; }
        public string AddressTypeName { get; set; }

        public string AddressComplete { get {
                string _AddressComplete = "";
                _AddressComplete += !string.IsNullOrEmpty(AddressTypeName) ? (AddressTypeName + "- ") : "";
                _AddressComplete += !string.IsNullOrEmpty(AddressLineOne) ? (AddressLineOne + ", ") : "";
                _AddressComplete += !string.IsNullOrEmpty(AddressLineTwo) ? (AddressLineTwo + ", ") : "";
                _AddressComplete += !string.IsNullOrEmpty(AddressLineThree) ? (AddressLineThree + ", ") : "";
                _AddressComplete += !string.IsNullOrEmpty(CityName) ? (CityName + ", ") : "";
                _AddressComplete += !string.IsNullOrEmpty(StateName) ? (StateName + ", ") : "";
                _AddressComplete += !string.IsNullOrEmpty(CountryName) ? (CountryName) : "";
                _AddressComplete += (ZipCode!=null && ZipCode.Value>0) ? ("-" + ZipCode.ToString()) : "";
                return _AddressComplete;
            } }

    }
    public class configuration
    {
        public long CompanyProductId { get; set; }
        public long CompanyId { get; set; }
        public double GstRate { get; set; }
        public double AnnualInterestRate { get; set; }
        public double ProcessingFee { get; set; }
        public double DelayPenaltyFee { get; set; }
        public double BounceCharges { get; set; }
        public double ProcessingCreditDays { get; set; }
        public int CreditDays { get; set; }
        public long ProductId { get; set; }
        public string AgreementUrl { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public long? AgreementDocId { get; set; }
        public string ProductName { get; set; }
    }
    public class companyLocationDto
    {
        public long companyId { get; set; }
        public List<long> location { get; set; }
    }

    public class PartnerList
    {
        public string PartnerName { get; set; }
        public string PartnerMobileNo { get; set; }
        public long PartnerCompanyId { get; set; }

    }
}
