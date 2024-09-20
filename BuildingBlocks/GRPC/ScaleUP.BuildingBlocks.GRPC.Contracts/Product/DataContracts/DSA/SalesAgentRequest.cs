using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA
{
    [DataContract]
    public class SalesAgentRequest
    {
        [DataMember(Order = 1)]
        public string MobileNo { get; set; }
        [DataMember(Order = 2)]
        public string UserId { get; set; }
        [DataMember(Order = 3)]
        public string PanNo { get; set; }
        [DataMember(Order = 4)]
        public string PanUrl { get; set; }
        [DataMember(Order = 5)]
        public string AadharNo { get; set; }
        [DataMember(Order = 6)]
        public string AadharFrontUrl { get; set; }
        [DataMember(Order = 7)]
        public string AadharBackUrl { get; set; }
        [DataMember(Order = 8)]
        public string? GstnNo { get; set; }
        [DataMember(Order = 9)]
        public string? GstnUrl { get; set; }
        [DataMember(Order = 10)]
        public string? AgreementUrl { get; set; }
        [DataMember(Order = 11)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 12)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 13)]
        public string Status { get; set; }
        [DataMember(Order = 14)]
        public required long AnchorCompanyId { get; set; } // Anchor Company
        [DataMember(Order = 15)]
        public string Type { get; set; }  // DSA , Connector (Individual)
        [DataMember(Order = 16)]
        public string FullName { get; set; }
        [DataMember(Order = 17)]
        public string CityName { get; set; }
        [DataMember(Order = 18)]
        public string StateName { get; set; }
        [DataMember(Order = 19)]
        public List<long> ProductIds { get; set; }
        [DataMember(Order = 20)]
        public long SalesAgentId { get; set; }
        
        [DataMember(Order = 21)]
        public string Role { get; set; }
        [DataMember(Order = 22)]
        public string? SelfieUrl { get; set; }
        [DataMember(Order = 23)]
        public string? AadharAddress { get; set; }
        [DataMember(Order = 24)]
        public string? WorkingLocation { get; set; }
        [DataMember(Order = 25)]
        public string? EmailId { get; set; }
        [DataMember(Order = 26)]
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set;}

    }
    [DataContract]
    public class SalesAgentCommissionList
    {
        [DataMember(Order = 1)]
        public double PayoutPercentage { get; set; }
        [DataMember(Order = 2)]
        public required int MinAmount { get; set; }
        [DataMember(Order = 3)]
        public required int MaxAmount { get; set; }
        [DataMember(Order = 4)]
        public required long ProductId { get; set; }
    }
}
