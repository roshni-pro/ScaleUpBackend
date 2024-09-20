using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class DisbursementResponse
    {
        [DataMember(Order = 1)]
        public long AccountId { get; set; }
        [DataMember(Order = 2)]
        public long LeadId { get; set; }
        [DataMember(Order = 3)]
        public string MobileNo { get; set; }
        [DataMember(Order = 4)]
        public string LeadCode { get; set; }
        [DataMember(Order = 5)]
        public string UserName { get; set; }
        [DataMember(Order = 6)]
        public double DisbursalAmount { get; set; }

        [DataMember(Order = 7)]
        public double ConvenienceFeeRate { get; set; }
        [DataMember(Order = 8)]
        public double ProcessingFeeRate { get; set; }

        [DataMember(Order = 9)]
        public string AccountCode { get; set; }
        [DataMember(Order = 10)]
        public double GstRate { get; set; }

        [DataMember(Order = 11)]
        public double ProcessingFeeAmount { get; set; }

        [DataMember(Order = 12)]
        public double GSTProcessingFeeAmount { get; set; }

        [DataMember(Order = 13)]
        public string? ProcessingFeeType { get; set; }

        [DataMember(Order = 14)]
        public string? PayableBy { get; set; }

        [DataMember(Order = 15)]
        public string? ThirdPartyLoanCode { get; set; }

        [DataMember(Order = 16)]
        public string? AnchorName { get; set; }

        [DataMember(Order = 17)]
        public string? ProductType { get; set; }
        [DataMember(Order = 18)]
        public double? InterestRate { get; set; }
        [DataMember(Order = 19)]
        public string? InterestRateType { get; set; }
    }
    [DataContract]
    public class LoanAccountDisbursementResponse
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public double? OrderAmount { get; set; }
        [DataMember(Order = 3)]
        public DateTime? DisbursementDate { get; set; }
    }
}
