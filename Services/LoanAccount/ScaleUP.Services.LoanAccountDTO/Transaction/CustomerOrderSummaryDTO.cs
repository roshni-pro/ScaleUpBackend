using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Transaction
{
    [DataContract]
    public class CustomerOrderSummaryDTO
    {
        public double TotalOutStanding { get; set; }
        public double AvailableLimit { get; set; }
        public double TotalPayableAmount { get; set; }
        public double TotalPendingInvoiceCount { get; set; }
        public List<CustomerInvoiceDTO> CustomerInvoice { get; set; }
        public string CustomerName { get; set; }   
        public string CustomerImage { get; set; }
    }

    public class CustomerInvoiceDTO
    {
        public string? AnchorName { get; set; }
        public DateTime? DueDate { get; set; }
        public string? OrderId { get; set; }
        public string? Status { get; set; }
        public double Amount { get; set; }
        public long TransactionId { get; set; }
        public long? InvoiceId { get; set; }
        public double? PaidAmount { get; set; }
        public string? InvoiceNo { get; set; }
    }

    public class CustomerTransactionInput
    {
        public long LeadId { get; set; }
        public long AnchorCompanyID { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public string TransactionType { get; set; }
    }
    public class PaymentOutstandingDc
    {
        public double TotalPayableAmount { get; set; }
        public int TotalPendingInvoiceCount { get; set; }
    }
    public class TransactionBreakupDc
    {
        public double TotalPayableAmount { get; set; }
        public List<TransactionBreakupPartDc> TransactionList { get; set; }
    }
    public class TransactionBreakupPartDc 
    {
        public double Amount { get; set; }
        public string TransactionType { get; set; }
    }
    public class CustomerTransactionTwoInput
    {
        public long LeadId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class LoanAccountSummaryDc
    {
        public double? UtilizedAmount { get; set; }
        public double? AvailableLimit { get; set; }
    }
}
