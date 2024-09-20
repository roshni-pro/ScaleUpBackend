using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    public class GetBLPaymentUploadsDTO
    {
        public long Id { get; set; }
        public long LoanAccountId { get; set; }
        public string LoanId { get; set; }
        //public int? PartnerId { get; set; }
        //public int? ProductId { get; set; }
        public double RepaymentAmount { get; set; }
        public double PrincipalPaid { get; set; }
        public double InterestPaid { get; set; }
        public double BouncePaid { get; set; }
        public double PenalPaid { get; set; }
        public double OverduePaid { get; set; }
        public double LoanAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentReqNo { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? Deleted { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public  bool IsProcess { get; set; }
        public double LpiPaid { get; set; }
        public string FileUrl { get; set; }
        public DateTime EmiMonth { get; set; }
        public int TotalRecords { get; set; }
    }
}
