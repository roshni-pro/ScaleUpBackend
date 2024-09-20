using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Transaction
{
    public class InvoiceDTO
    {
        public long InvoiceId { get; set; }
        public long LoanAccountId { get; set; }
        public string LoanNo { get; set; }
        public string CustomerName { get; set; }
        public string AnchoreName { get; set; }
        public string OrderNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string Status { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? InvoicePdfUrl { get; set; }
        public double OrderAmount { get; set; }
        public double InvoiceAmount { get; set; }
        public double TotalTransAmount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class InvoiceListDTO
    {
        public List<InvoiceDTO> Invoices { get; set; }
        public int TotalRecord { get; set; }
    }

    public class InvoiceRequest
    {
        public string Search { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

    }

    public class InvoiceNBFCReqRes
    {
        public string URL { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CompanyInvoiceStatusDc
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class CompanyInvoiceDetailsByTypeDc
    {
        public long AccountTransactionId { get; set; }
        public long TransactionTypeId { get; set; }
        public double Amount { get; set; }
    }
}
