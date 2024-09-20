using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA
{
    [DataContract]
    public class AddDSALoanAccountRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
        [DataMember(Order = 3)]
        public string UserId { get; set; }
        [DataMember(Order = 4)]
        public DateTime DisbursalDate { get; set; }
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
        public DateTime ApplicationDate { get; set; }
        [DataMember(Order = 11)]
        public DateTime AgreementRenewalDate { get; set; }
        [DataMember(Order = 12)]
        public bool IsDefaultNBFC { get; set; }
        [DataMember(Order = 13)]
        public string? CityName { get; set; }
        [DataMember(Order = 14)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 15)]
        public string ProductType { get; set; }
        [DataMember(Order = 16)]
        public bool IsAccountActive { get; set; }
        [DataMember(Order = 17)]
        public bool IsBlock { get; set; }
        [DataMember(Order = 18)]
        public string NBFCCompanyCode { get; set; }
        [DataMember(Order = 19)]
        public string? ThirdPartyLoanCode { get; set; }
        [DataMember(Order = 20)]
        public string? ShopName { get; set; }
        [DataMember(Order = 21)]
        public string? CustomerImage { get; set; }
        [DataMember(Order = 22)]
        public string Type { get; set; }

    }
    [DataContract]
    public class SalesAgentDisbursmentDataDc
    {
        [DataMember(Order = 1)]
        public List<GetSalesAgentLoanDisbursmentDc> GetSalesAgentLoanDisbursments { get; set; }
        [DataMember(Order = 2)]
        public List<SalesAgentDetailDC> SalesAgentDetails { get; set; }
        [DataMember(Order = 3)]
        public double GstRate { get; set; }
        [DataMember(Order = 4)]
        public double TDSRate { get; set; }
    }

    [DataContract]
    public class DSALoanDashboardDataRequest
    {
        [DataMember(Order = 1)]
        public List<long> LeadIds { get; set; }
        [DataMember(Order = 2)]
        public string AgentUserId { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
        [DataMember(Order = 4)]
        public int Skip { get; set; }
        [DataMember(Order = 5)]
        public int Take { get; set; }

    }
    [DataContract]
    public class DSALoanPayoutDataDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long AccountTransactionId { get; set; }
        [DataMember(Order = 3)]
        public double PayoutAmount { get; set; }

    }
}
