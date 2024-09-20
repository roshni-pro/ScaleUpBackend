using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class NbfcMisListResponseDc
    {
        [DataMember(Order = 1)]
        public string? InvoiceNo { get; set; }
        [DataMember(Order = 2)]
        public string? TransactionNo { get; set; }
        [DataMember(Order = 3)]
        public long? AccountTransactionId { get; set; }
        [DataMember(Order = 4)]
        public string? InvoiceTranType { get; set; }
        [DataMember(Order = 5)]
        public DateTime? DisbursementDate { get; set; }
        [DataMember(Order = 6)]
        public DateTime? DueDate { get; set; }
        [DataMember(Order = 7)]
        public DateTime? SettlementDate { get; set; }
        [DataMember(Order = 8)]
        public double? PrincipleAmount { get; set; }
        [DataMember(Order = 9)]
        public double? TransAmount { get; set; }
        [DataMember(Order = 10)]
        public double? PaidAmount { get; set; }
        [DataMember(Order = 11)]
        public double? NBFCRate { get; set; }
        [DataMember(Order = 12)]
        public double? AnchorRate { get; }
        [DataMember(Order = 13)]
        public double? ScaleupRate { get; set; }
        [DataMember(Order = 14)]
        public bool IsSharable { get; set; }
        [DataMember(Order = 15)]
        public double? ScaleupShare { get; set; }
        [DataMember(Order = 16)]
        public double? SharePercent { get; set; }
    }

    [DataContract]
    public class NbfcMisListRequestDc
    {
        [DataMember(Order = 1)]
        public DateTime? FromDate { get; set; }
        [DataMember(Order = 2)]
        public DateTime? ToDate { get; set;}
        [DataMember(Order = 3)]
        public long NbfcCompanyId { get; set; }
    }
}
