using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class GetPaymentReqByTxnNo
    {
        [DataMember(Order = 1)]
        public string TransactionReqNo { get; set; }
        [DataMember(Order = 2)]
        public double GstRate { get; set; }
        [DataMember(Order = 3)]
        public double AnnualInterestRate { get; set; }
        [DataMember(Order = 4)]
        public string AnnualInterestPayableBy { get; set; }
    }
    [DataContract]
    public class PaymentResponseDc
    {
        [DataMember(Order = 1)]
        public string TransactionReqNo { get; set; }
        [DataMember(Order = 2)]
        public double TransactionAmount { get; set; }
        [DataMember(Order = 3)]
        public string MobileNo { get; set; }
        [DataMember(Order = 4)]
        public double ConvenionFee { get; set; }
        [DataMember(Order = 5)]
        public double GSTConvenionFee { get; set; }
        [DataMember(Order = 6)]
        public double TotalAmount { get; set; }
        [DataMember(Order = 7)]
        public long LoanAccountId { get; set; }
        [DataMember(Order = 8)]
        public string OrderNo { get; set; }
        [DataMember(Order = 9)]
        public List<int> CreditDays { get; set; }
        [DataMember(Order = 10)]
        public double AvailableCreditLimit { get; set; }
        [DataMember(Order = 11)]
        public double UtilizateLimit { get; set; }
        [DataMember(Order = 12)]
        public double InvoiceAmount { get; set; }
        [DataMember(Order = 13)]
        public string AnchorName { get; set; }
        [DataMember(Order = 14)]
        public bool IsPayableByCustomer { get; set; } // customer-true, subventedbyAnchor-false
        [DataMember(Order = 15)]
        public double InterestRate { get; set; }
        [DataMember(Order = 16)]
        public List<CreditDayWiseAmount> CreditDayWiseAmounts { get; set; }
        [DataMember(Order = 17)]
        public string? TransactionStatus { get; set; }
        [DataMember(Order = 18)]
        public bool IsAccountActive { get; set; }
        [DataMember(Order = 19)]
        public bool IsBlock { get; set; }
        [DataMember(Order = 20)]
        public string? IsBlockComment { get; set; }
        [DataMember(Order = 21)]
        public double AnchorCompanyId { get; set; }
    }

    [DataContract]
    public class CreditDayWiseAmount
    {
        [DataMember(Order = 1)]
        public int days { get; set; }
        [DataMember(Order = 2)]
        public double Amount { get; set; }
        [DataMember(Order = 3)]
        public CreditDayAmountCal CreditDayAmountCals { get; set; }
        [DataMember(Order = 4)]
        public double FinalAmount { get; set; }

    }
    [DataContract]
    public class CreditDayAmountCal
    {
        [DataMember(Order = 1)]
        public double InterestRate { get; set; }
        [DataMember(Order = 2)]
        public double InterestAmount { get; set; }
        [DataMember(Order = 3)]
        public double GSTAmount { get; set; }
        [DataMember(Order = 4)]
        public double TotalAmount { get; set; }
        [DataMember(Order = 5)]
        public double InvoiceAmount { get; set; }
        [DataMember(Order = 6)]
        public double AnnualInterestRate { get; set; }
    }

    [DataContract]
    public class OrderInitiateResponse
    {
        [DataMember(Order = 1)]
        public string RedirectUrl { get; set; }
        [DataMember(Order = 2)]
        public string OtptxnNo { get; set; }
        [DataMember(Order = 3)]
        public string MobileNo { get; set; }
        [DataMember(Order = 4)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 5)]
        public string AnchorName { get; set;}
        [DataMember(Order = 6)]
        public string CustomerName { get; set;}
    }
}
