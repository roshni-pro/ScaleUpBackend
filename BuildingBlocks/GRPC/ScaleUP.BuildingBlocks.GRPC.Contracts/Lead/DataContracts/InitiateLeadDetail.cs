using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class InitiateLeadDetail
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
        [DataMember(Order = 2)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 3)]
        [StringLength(10)]
        public string MobileNumber { get; set; }
        [DataMember(Order = 4)]
        [StringLength(500)]
        public string CustomerReferenceNo { get; set; }
        [DataMember(Order = 5)]
        public int VintageDays { get; set; }
        [DataMember(Order = 6)]
        public string UserId { get; set; }
        [DataMember(Order = 7)]
        public string Email { get; set; }
        [DataMember(Order = 8)]
        public List<CustomerBuyingHistory> CustomerBuyingHistories { get; set; }
        [DataMember(Order = 9)]
        public List<GRPCLeadProductActivity>? ProductActivities { get; set; }
        [DataMember(Order = 10)]
        public string? AnchorCompanyName { get; set; }
        [DataMember(Order = 11)]
        public string AnchorCompanyCode { get; set; }
        [DataMember(Order = 12)]
        public string ProductCode { get; set; }
        [DataMember(Order = 13)]
        public long? CityId { get; set; }

    }

    [DataContract]
    public class CustomerBuyingHistory
    {
        [DataMember(Order = 1)]
        public DateTime MonthFirstBuyingDate { get; set; }
        [DataMember(Order = 2)]
        public int TotalMonthInvoice { get; set; }
        [DataMember(Order = 3)]
        public int MonthTotalAmount { get; set; }
    }
    [DataContract]
    public class UpdateBuyingHistoryRequest
    {
        [DataMember(Order = 1)]
        public string ProductCode { get; set; }
        [DataMember(Order = 2)]
        public string AnchorCompanyCode { get; set; }
        [DataMember(Order = 3)]
        public string MobileNumber { get; set; }
        [DataMember(Order = 4)]
        public string Email { get; set; }
        [DataMember(Order = 5)]
        public string CustomerReferenceNo { get; set; }
        [DataMember(Order = 6)]
        public int VintageDays { get; set; }
        [DataMember(Order = 7)]
        public string City { get; set; }
        [DataMember(Order = 8)]
        public string State { get; set; }
        [DataMember(Order = 9)]
        public List<CustomerBuyingHistory> BuyingHistories { get; set; }
    }
    [DataContract]
    public class AddLeadOfferConfigRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
    }
}
