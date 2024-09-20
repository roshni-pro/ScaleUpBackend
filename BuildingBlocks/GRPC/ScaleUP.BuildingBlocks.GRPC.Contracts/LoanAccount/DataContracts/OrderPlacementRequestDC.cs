using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class OrderPlacementRequestDC
    {

        [DataMember(Order = 1)]
        public string TransactionReqNo { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }

        [DataMember(Order = 3)]
        public long LoanAccountId { get; set; }
        [DataMember(Order = 4)]
        public double Amount { get; set; }    
       
        [DataMember(Order = 7)]
        public string InterestPayableBy { get; set; } //ConvenienceFeePayableBy

        [DataMember(Order = 8)]
        public string? InterestType { get; set; } //ConvenienceFeeType
        [DataMember(Order = 9)]
        public double? GstRate { get; set; }
        [DataMember(Order = 10)]
        public double? InterestRate { get; set; }  //ConvenienceFeeRate
        [DataMember(Order = 11)]
        public int CreditDay { get; set; }
        [DataMember(Order = 12)]
        public double? BounceCharge { get; set; }
        [DataMember(Order = 13)]
        public double? DelayPenaltyRate { get; set; }
        [DataMember(Order = 14)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 15)]
        public string? Token { get; set; }
    }
    [DataContract]
    public class OrderPlacementWithOtp
    {

        [DataMember(Order = 1)]
        public long AnchorCompanyId { get; set; }       

        [DataMember(Order = 2)]
        public long LoanAccountId { get; set; }
        [DataMember(Order = 3)]
        public double TransactionAmount { get; set; }
        [DataMember(Order = 4)]
        public string OrderNo { get; set; }       
        [DataMember(Order = 5)]
        public double OrderAmount { get; set; }
        [DataMember(Order = 6)]
        public string? Token { get; set; }

    }

    [DataContract]
    public class AyeSCFCOrderInitiateDTO
    {
        [DataMember(Order = 1)]
        public long invoiceId { get; set; }
        [DataMember(Order = 2)]
        public long accountId { get; set; }
        [DataMember(Order = 3)]
        public string OrderNo { get; set; }
        [DataMember(Order = 4)]
        public double OrderAmount { get; set; }
        [DataMember(Order = 5)]
        public string? Token { get; set; }
        [DataMember(Order = 6)]
        public double? TotalLimit { get; set; }
        [DataMember(Order = 7)]
        public double AvailableLimit { get; set; }
    }
}
