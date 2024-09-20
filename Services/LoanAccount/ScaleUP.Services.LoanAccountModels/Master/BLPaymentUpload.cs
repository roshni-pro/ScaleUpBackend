using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class BLPaymentUpload : BaseAuditableEntity
    {
        public required long LoanAccountId { get; set; }

        [ForeignKey("LoanAccountId")]
        public LoanAccount LoanAccount { get; set; }
        [StringLength(100)]
        public required string LoanId { get; set; }
        public required double RepaymentAmount { get; set; }
        public required double PrincipalPaid { get; set; }
        public required double InterestPaid { get; set; }
        public required double BouncePaid { get; set; }
        public required double PenalPaid { get; set; }
        public required double OverduePaid { get; set; }
        public double LpiPaid { get; set; }
        public required double LoanAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        [StringLength(200)]
        public string? PaymentReqNo { get; set; }
        [StringLength(100)]
        public required string Status { get; set; }
        public required bool IsProcess { get; set; }
        [StringLength(1000)]
        public required string FileUrl { get; set; }
        public required DateTime EmiMonth { get; set; }
    }
}
