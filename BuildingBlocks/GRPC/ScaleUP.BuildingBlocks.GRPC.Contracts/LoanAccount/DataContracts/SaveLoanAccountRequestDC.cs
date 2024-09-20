using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class SaveLoanAccountRequestDC
    {
        [DataMember(Order = 1)]
        public required long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public string UserId { get; set; }
        [DataMember(Order = 4)]
        public string AccountCode { get; set; }
        [DataMember(Order = 5)]
        public string CustomerName { get; set; }
        [DataMember(Order = 6)]
        public string MobileNo { get; set; }
        [DataMember(Order = 7)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 8)]
        public long? AnchorCompanyId { get; set; }
        [DataMember(Order = 9)]
        public string LeadCode { get; set; }
        [DataMember(Order = 10)]
        public DateTime? ApplicationDate { get; set; }
        [DataMember(Order = 11)]
        public DateTime? AgreementRenewalDate { get; set; }
        [DataMember(Order = 12)]
        public bool IsDefaultNBFC { get; set; }
        [DataMember(Order = 13)]
        public string? CityName { get; set; }
        [DataMember(Order = 14)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 15)]
        public string? ProductType { get; set; }
        [DataMember(Order = 16)]
        public bool? IsAccountActive { get; set; }
        [DataMember(Order = 17)]
        public bool? IsBlock { get; set; }

        [DataMember(Order = 18)]
        public string NBFCCompanyCode { get; set; }
        [DataMember(Order = 19)]
        public string? Webhookresposne { get; set; }
        [DataMember(Order = 20)]
        public string? ShopName { get; set; }
        [DataMember(Order = 21)]
        public string? CustomerImage { get; set; }
        [DataMember(Order = 22)]
        public string? ApplicationId { get; set; }
        [DataMember(Order = 23)]
        public string? BusinessId { get; set; }
        [DataMember(Order = 24)]
        public string? BusinessCode { get; set; }
        [DataMember(Order = 25)]
        public string? ApplicationCode { get; set; } 
        [DataMember(Order = 26)]
        public string? token { get; set; }
    }
}
