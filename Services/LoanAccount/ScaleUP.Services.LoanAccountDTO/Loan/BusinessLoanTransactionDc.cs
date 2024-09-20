using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    public class BusinessLoanTransactionDc
    {
        public string TransactionReqNo { get; set; }
        public long ProductId { get; set; }
        public long LoanAccountId { get; set; }
        public double Amount { get; set; }
        public string InterestPayableBy { get; set; } //ConvenienceFeePayableBy
        public string? InterestType { get; set; } //ConvenienceFeeType
        public double? GstRate { get; set; }
        public double? InterestRate { get; set; }  //ConvenienceFeeRate
        public int CreditDay { get; set; }
        public double? BounceCharge { get; set; }
        public double? DelayPenaltyRate { get; set; }
        public long AnchorCompanyId { get; set; }
        public double ConvenienceFee { get; set; }
        public double ProcessingFee { get; set; }
        public double ProcessingFeeRate { get; set; }
        public double ProcessingFeeGST { get; set; }
        public double? InsuranceAmount { get; set; }
        public double? OtherCharges { get; set; }
        public long? InvoiceId { get; set; }
        public DateTime DisbursementDate { get; set;}
        public string? PFType { get; set; }
    }
}
