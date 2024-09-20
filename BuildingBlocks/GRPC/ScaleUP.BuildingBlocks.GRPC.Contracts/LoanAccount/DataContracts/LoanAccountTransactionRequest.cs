using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{

    [DataContract]
    public class LoanAccountTransactionRequest
    {
        [DataMember(Order = 1)]
        public string CustomerUniqueCode { get; set; }
        [DataMember(Order = 2)]
        public long LoanAccountId { get; set; }
        [DataMember(Order = 3)]
        public long? AnchorCompanyId { get; set; }
        [DataMember(Order = 4)]
        public string ReferenceId { get; set; }
        [DataMember(Order = 5)]
        public string TransactionTypeCode { get; set; }
        [DataMember(Order = 6)]
        public double TransactionAmount { get; set; }
        [DataMember(Order = 7)]
        public double OrderAmount { get; set; }
        [DataMember(Order = 8)]
        public double GSTAmount { get; set; }

        [DataMember(Order = 9)]
        public double PaidAmount { get; set; }
        [DataMember(Order = 10)]
        public long ParentAccountTransactionsID { get; set; }

        [DataMember(Order = 11)]
        public string ProcessingFeeType { get; set; }
        [DataMember(Order = 12)]
        public double ProcessingFeeRate { get; set; }
        [DataMember(Order = 13)]
        public double GstRate { get; set; }
        [DataMember(Order = 14)]
        public string InterestType { get; set; } //ConvenienceFeeType
        [DataMember(Order = 15)]
        public double InterestRate { get; set; } //ConvenienceFeeRate
        [DataMember(Order = 16)]
        public long? CreditDays { get; set; }
        [DataMember(Order = 17)]
        public double BounceCharge { get; set; }
        [DataMember(Order = 18)]
        public double DelayPenaltyRate { get; set; }
        [DataMember(Order = 19)]
        public string PayableBy { get; set; }
        [DataMember(Order = 20)]
        public string PaymentRefNo { get; set; }
        [DataMember(Order = 21)]
        public double FeeAmount { get; set; }
    }
}
