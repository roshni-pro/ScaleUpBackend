using ScaleUP.Global.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ScaleUP.Services.LoanAccountModels.Transaction
{
    public class CompanyInvoice : BaseAuditableEntity
    {
        public long CompanyId { get; set; }
        [StringLength(100)]
        public string InvoiceNo { get; set; }      
        public DateTime InvoiceDate { get; set; }
        public double InvoiceAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentReferenceNo { get; set; }
        public int Status { get; set; } //0-Inprocess, 1-MakerApproved, 2-CheckerApproved, 3- MakerReject, 4- CheckerReject

        [StringLength(100)]
        public string? PaymentCheckerUser { get; set; } // MakerUser
        [StringLength(100)]
        public string? PaymentVerifierUser { get; set; }// checkerUser

        [StringLength(1000)]
        public string? InvoiceUrl { get; set; }
        public ICollection<CompanyInvoiceDetail> CompanyInvoiceDetails { get; set; }
    }
}
