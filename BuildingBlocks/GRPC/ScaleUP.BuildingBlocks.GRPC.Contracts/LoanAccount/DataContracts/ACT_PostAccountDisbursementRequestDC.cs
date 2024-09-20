using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class ACT_PostAccountDisbursementRequestDC
    {

        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public double? Amount { get; set; }

        [DataMember(Order = 3)]
        public double? DiscountAmount { get; set; }

        [DataMember(Order = 4)]
        public string TransactionTypeCode { get; set; }

        [DataMember(Order = 5)]
        public string TransactionReqNo { get; set; }

        [DataMember(Order = 6)]
        public string? ProcessingFeeType { get; set; }
        [DataMember(Order = 7)]
        public double? ProcessingFeeRate { get; set; }
        [DataMember(Order = 8)]
        public double? GstRate { get; set; }
        [DataMember(Order = 9)]
        public double? ConvenienceFeeRate { get; set; }
        [DataMember(Order = 10)]
        public long? CreditDays { get; set; }
        [DataMember(Order = 11)]
        public double? BounceCharge { get; set; }
        [DataMember(Order = 12)]
        public double? DelayPenaltyRate { get; set; }
        [DataMember(Order = 13)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 14)]
        public string ProcessingFeePayableBy { get; set; }

        [DataMember(Order = 15)]
        public string? ConvenienceFeeType { get; set; }
        [DataMember(Order = 16)]
        public string ConvenienceFeePayableBy { get; set; }
        [DataMember(Order = 17)]
        public string CustomerUniqueCode { get; set; }
        [DataMember(Order = 18)]
        public string DisbursementType { get; set; }

        //[DataMember(Order = 6)]
        //public AnchorCompanyConfig AnchorCompanyConfig { get; set; }
        //[DataMember(Order = 7)]
        //public NBFCCompanyConfig NBFCCompanyConfig { get; set; }
        [DataMember(Order = 19)]
        public BlackSoilPfCollectionDc? BlackSoilPfCollection { get; set; }
    }


    [DataContract]
    public class NBFCCompanyConfig
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProcessingFee { get; set; }
        [DataMember(Order = 3)]
        public double InterestRate { get; set; }
        [DataMember(Order = 4)]
        public long PenaltyCharges { get; set; }
        [DataMember(Order = 5)]
        public long BounceCharges { get; set; }
        [DataMember(Order = 6)]
        public long PlatformFee { get; set; }
        [DataMember(Order = 7)]
        public long CustomerAgreementDocId { get; set; }

        [DataMember(Order = 8)]
        public string CustomerAgreementURL { get; set; }
    }

    [DataContract]
    public class AnchorCompanyConfig
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string ProcessingFeePayableBy { get; set; }
        [DataMember(Order = 3)]
        public string ProcessingFeeType { get; set; }
        [DataMember(Order = 4)]
        public double ProcessingFeeRate { get; set; }
        [DataMember(Order = 5)]
        public string? TransactionFeePayableBy { get; set; } //CreditLine
        [DataMember(Order = 6)]
        public string? TransactionFeeType { get; set; } //CreditLine
        [DataMember(Order = 7)]
        public double? TransactionFeeRate { get; set; } //CreditLine
        [DataMember(Order = 8)]
        public double DelayPenaltyRate { get; set; }
        [DataMember(Order = 9)]
        public long BounceCharge { get; set; }
        [DataMember(Order = 10)]
        public long? CreditDays { get; set; } //CreditLine
        [DataMember(Order = 11)]
        public long? DisbursementTAT { get; set; } //CreditLine
        [DataMember(Order = 12)]
        public double? AnnualInterestRate { get; set; } //BusinessLoan
        [DataMember(Order = 13)]
        public long? MinTenureInMonth { get; set; } //BusinessLoan
        [DataMember(Order = 14)]
        public long? MaxTenureInMonth { get; set; } //BusinessLoan
        public double? EMIRate { get; set; } //CreditLine
        [DataMember(Order = 15)]
        public double? EMIProcessingFeeRate { get; set; } //CreditLine
        [DataMember(Order = 16)]
        public long? EMIBounceCharge { get; set; } //CreditLine
        [DataMember(Order = 17)]
        public double? EMIPenaltyRate { get; set; } //CreditLine
        [DataMember(Order = 18)]
        public double? CommissionPayout { get; set; } //BusinessLoan
        [DataMember(Order = 19)]
        public double? ConsiderationFee { get; set; } //BusinessLoan
        [DataMember(Order = 20)]
        public double? DisbursementSharingCommission { get; set; } //BusinessLoan
        [DataMember(Order = 21)]
        public long? MinLoanAmount { get; set; } //BusinessLoan
        [DataMember(Order = 22)]
        public long? MaxLoanAmount { get; set; } //BusinessLoan
        [DataMember(Order = 23)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 24)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 25)]
        public string? AgreementURL { get; set; }
        [DataMember(Order = 26)]
        public long? AgreementDocId { get; set; }
    }

    [DataContract]
    public class ScaleupSettleTransactionRequestDC
    {
        [DataMember(Order = 1)]
        public long LoanAccountId { get; set; }
        [DataMember(Order = 2)]
        public string PaymentReqNo { get; set; }
        [DataMember(Order = 3)]
        public string TransactionTypeCode { get; set; }
        [DataMember(Order = 4)]
        public long ParentAccountTransactionId { get; set; }
        [DataMember(Order = 5)]
        public double OverDuePaymentAmount { get; set; }
        [DataMember(Order = 6)]
        public double PenalPaymentAmount { get; set; }
        [DataMember(Order = 7)]
        public double InterestPaymentAmount { get; set; }
        [DataMember(Order = 8)]
        public double BouncePaymentAmount { get; set; }
        [DataMember(Order = 9)]
        public SettlePaymentJobRequest ProductConfigs { get; set; }
        [DataMember(Order = 10)]
        public required DateTime? PaymentDate { get; set; }
    }

    [DataContract]
    public class AddInvoiceSettlementDataRequestDC
    {
        [DataMember(Order = 1)]
        public long ParentAccountTransactionId { get; set; }
        [DataMember(Order = 2)]
        public string TransactionTypeCode { get; set; }
        [DataMember(Order = 3)]
        public double Amount { get; set; }
        [DataMember(Order = 4)]
        public DateTime PaymentDate { get; set; }
        [DataMember(Order = 5)]
        public string UTRNumber { get; set; }
        [DataMember(Order = 6)]
        public bool IsFullPayment { get; set; }
    }
}
