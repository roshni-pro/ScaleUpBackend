using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaleUP.Services.LoanAccountModels.Transaction
{
    public class CompanyInvoiceDetail : BaseAuditableEntity
    {
        public long CompanyInvoiceId { get; set; }
        public long AccountTransactionId { get; set; }       
        public int InvoiceTransactionType { get; set; }//1=PF,2=Interest,3=OD Interest,4=Penal,5=Bounce
        public double InvoiceAmt { get; set; }
        public double TotalAmount { get; set; }        
        public double PayableAmount { get; set; }
        public double SharePercent { get; set; }
        public double ScaleupShare { get; set; }

        [ForeignKey("AccountTransactionId")]
        public AccountTransaction AccountTransaction { get; set; }

        [ForeignKey("CompanyInvoiceId")]
        public CompanyInvoice CompanyInvoice { get; set; }        
        [StringLength(100)]
        public string? Status { get; set; } // TransactionStatuseConstants  Pending,Paid

    }
}
