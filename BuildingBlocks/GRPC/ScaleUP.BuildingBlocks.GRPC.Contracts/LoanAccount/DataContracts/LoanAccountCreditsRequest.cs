using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class LoanAccountCreditsRequest
    {
        [DataMember(Order = 1)]
        public double  GstRate { get; set; }
        [DataMember(Order = 2)]
        public double ProcessingFeeRate { get; set; }
        [DataMember(Order = 3)]
        public double AnnualInterestRate { get; set; }
        [DataMember(Order = 4)]
        public double DisbursalAmount { get; set; }
        [DataMember(Order = 5)]
        public long LoanAccountId { get; set; }
        [DataMember(Order = 6)]
        public double? CreditLimitAmount { get; set; }
        [DataMember(Order = 7)]
        public long? CreditDays { get; set; }
        [DataMember(Order = 8)]
        public double BounceCharge { get; set; }
        [DataMember(Order = 9)]
        public double DelayPenaltyRate { get; set; }
    }
}
