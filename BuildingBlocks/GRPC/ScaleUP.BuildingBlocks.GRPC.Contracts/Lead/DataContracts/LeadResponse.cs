using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadResponse
    {

        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string MobileNo { get; set; }
        [DataMember(Order = 3)]
        public string LeadCode { get; set; }
        [DataMember(Order = 4)]
        public string UserName { get; set; }
        [DataMember(Order = 5)]
        public long ProductId { get; set; }
        [DataMember(Order = 6)]
        public long? OfferCompanyId { get; set; }
        [DataMember(Order = 7)]
        public double? CreditLimit { get; set; }
        [DataMember(Order = 8)]
        public DateTime? AgreementDate { get; set; }
        [DataMember(Order = 9)]
        public DateTime? ApplicationDate { get; set; }
        [DataMember(Order = 10)]
        public List<LeadCompany>? LeadCompanies { get; set; }
        [DataMember(Order = 11)]
        public string? CustomerImage { get; set; }
        [DataMember(Order = 12)]
        public string? ShopName { get; set; }
        [DataMember(Order = 13)]
        public string? CustomerCurrentCityName { get; set; }
        [DataMember(Order = 14)]
        public string? CustomerName { get; set; }

        [DataMember(Order = 15)]
        public BlackSoilPfCollectionDc? BlackSoilPfCollection { get; set; }
        [DataMember(Order = 16)]
        public string? ProductCode {  get; set; }
        [DataMember(Order = 17)]
        public string? Status { get; set;}
        [DataMember(Order = 18)]
        public string? CreatedBy { get; set; }


    }
    [DataContract]
    public class LeadCompany
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string? UserUniqueCode { get; set; }
        [DataMember(Order = 3)]
        public int? LeadProcessStatus { get; set; }
        [DataMember(Order = 4)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 5)]
        public string? CustUniqueCode { get; set;}
        [DataMember(Order = 6)]
        public string? LogoURL { get;set; }
    }

    [DataContract]
    public class LeadCityIds
    {
        [DataMember(Order = 1)]
       public List<long?> CityIds { get; set; }
    }

}
