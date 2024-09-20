using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Transaction
{
    [DataContract]
    public class AccountTransactionDTO
    {
        public long LoanAccountId { get; set; }
        public string LeadCode { get; set; }
        public string AccountCode { get; set; }
        public string CustomerName { get; set; }
        public string PaymentMode { get; set; }
        public string Code { get; set; }
        public long ParentID { get; set; }
        public double ActualOrderAmount { get; set; }
        public double PayableAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string ReferenceId { get; set; }
        public string MobileNo { get; set; }
        public long TotalCount { get; set; }
        public string UtilizationAnchor { get; set; }
        public string OrderId { get; set; }
        public double ReceivedPayment { get; set; }
        public int Aging { get; set; }
        public string InvoiceNo { get; set; }
        public string ThirdPartyLoanCode { get; set; }
        public DateTime? DisbursementDate { get; set; }
    }

    [DataContract]
    public class TransactionFilterDTO
    {
        public long LoanAccountId { get; set; }
        public string SearchKeyward { get; set; }
        public string Status { get; set; } //Pendin, Paid, OverDue
        public int Skip { get; set; }
        public int Take { get; set; }
        public string CityName { get; set; }
        public long AnchorCompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string TabType { get; set; }
    }
    [DataContract]
    public class TransactionDetailDTO
    {
        public long AccountTransactionId { get; set; }
        public string Code { get; set; }
        public double Amount { get; set; }
    }

    [DataContract]
    public class TransactionDetailDc
    {
        public long AccountTransactionId { get; set; }
        public string TransactionType { get; set; }
        public double TxnAmount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public double TotalAmount { get; set; }

    }

    [DataContract]
    public class PenaltyBounceChargesDc
    {
        public string Code { get; set; }
        public string ReferenceId { get; set; }
        public double PaidAmount { get; set; }
        public double GSTAmount { get; set; }
        public double TotalAmount { get; set; }
        public string Type { get; set; }
        public long TransactionTypeId { get; set; }
    }
    [DataContract]
    public class AnchorNameDc
    {
        public long? AnchorId { get; set; }
        public string? AnchorName { get; set; }
    }

    [DataContract]
    public class CityNameDc
    {
        // public long CityId { get; set; }
        public string? CityName { get; set; }

    }
    [DataContract]
    public class ProductDc
    {
        public long? ProductId { get; set; }
        public string? ProductType { get; set; }
    }

    [DataContract]
    public class AnchorCityProductdc
    {
        public string? CityName { get; set; }
        public long? AnchorId { get; set; }
        public string? AnchorName { get; set; }
        public long? ProductId { get; set; }
        public string? ProductType { get; set; }
    }

    [DataContract]
    public class AnchorCityProductListDc
    {
        public List<ProductDc> ProductDcs { get; set; }
        public List<AnchorNameDc> AnchorNameDcs { get; set; }
        public List<CityNameDc> CityNameDcs { get; set; }
    }

    [DataContract]
    public class InvoiceDetailListDc
    {
        public long LoanAccountId { get; set; }
        //public string LeadCode { get; set; }
        public string AccountCode { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public double ActualOrderAmount { get; set; }
        public double PayableAmount { get; set; }
        public DateTime? DueDate { get; set; }
        //public DateTime? TransactionDate { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string MobileNo { get; set; }
        public long TotalCount { get; set; }
        public string UtilizationAnchor { get; set; }
        public string OrderNo { get; set; }
        //public double ReceivedPayment { get; set; }
        public string? InvoiceNo { get; set; }
        public string ThirdPartyLoanCode { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public long InvoiceId { get; set; }
        public DateTime? PaymentDate { get; set; }

    }

    [DataContract]
    public class TransactionDetailByTransactionIdDc
    {
        public string ReferenceId { get; set; }
        public string Head { get; set; }
        public double Amount { get; set; }
        public DateTime? TransactionDate { get; set; }
    }

    [DataContract]
    public class InvoiceDetailDc
    {
        public string ReferenceId { get; set; }
        public long AccountTransactionId { get; set; }
        public long InvoiceId { get; set; }
        public string TransactionType { get; set; }
        public double TxnAmount { get; set; }
        public DateTime? TransactionDate { get; set; }
    }
    public class EmiPaymentRequestDc
    {
        public long ParentAccountTransactionsID { get; set; }
        public double PrincipalAmount { get; set; }
        public double InterestAmount { get; set; }
        public double BounceAmount { get; set; }
        public double PenalAmount { get; set; }
        public double OverdueAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentReqNo { get; set; }
        public long TransactionTypeId { get; set; }
        public long TransactionStatusId { get; set; }
    }
    public class EmiScaleUpShareRequestDc
    {
        public long ParentAccountTransactionsID { get; set; }
        //public double PrincipalAmount { get; set; }
        public double InterestAmount { get; set; }
        public double NBFCInterestRate { get; set; }
        public double CustomerInterestRate { get; set; }
        public double BounceAmount { get; set; }
        public double NBFCBounceCharge { get; set; }
        public double CustomerBounceCharge { get; set; }
        public double PenalAmount { get; set; }
        public double NBFCPenalRate { get; set; }
        public double CustomerPenalRate { get; set; }
        public double OverdueAmount { get; set; }
        public double NBFCOverdueRate { get; set; }
        public double CustomerOverdueRate { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentReqNo { get; set; }
        public long TransactionTypeId { get; set; }
        public long TransactionStatusId { get; set; }
        public long FinTechCompanyId { get; set; }
    }
}
