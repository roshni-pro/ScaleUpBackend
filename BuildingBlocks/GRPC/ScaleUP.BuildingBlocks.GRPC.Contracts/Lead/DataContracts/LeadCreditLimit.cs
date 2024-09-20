using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadCreditLimit
    {
        [DataMember(Order = 1)]
        public double? CreditLimit { get; set; }
        [DataMember(Order = 2)]
        public string UserId { get; set; }
        [DataMember(Order = 3)]
        public string LeadCode { get; set; }
        [DataMember(Order = 4)]
        public long? OfferCompanyId { get; set; }
        [DataMember(Order = 5)]
        public long? AnchorCompanyId { get; set; }
        [DataMember(Order = 6)]
        public string? ProductCode { get; set; }

        [DataMember(Order = 7)]
        public int? VintageDays { get; set;}
        [DataMember(Order = 8)]
        public string? AnchorCompanyName { get; set; }
        [DataMember(Order = 9)]
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
        [DataMember(Order = 10)]
        public double? InterestRate { get; set; }
        [DataMember(Order = 11)]
        public int? BusinessVintageDays { get; set; }
    }
}
