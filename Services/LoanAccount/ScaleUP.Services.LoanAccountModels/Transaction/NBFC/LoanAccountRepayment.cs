using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LoanAccountModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Transaction.NBFC
{
    public class LoanAccountRepayment : BaseAuditableEntity
    {
        [StringLength(200)]
        public string ThirdPartyPaymentId { get; set; }
        public long LoanAccountId { get; set; }
        [ForeignKey("LoanAccountId")]
        public LoanAccount LoanAccount { get; set; }

        [StringLength(100)]
        public string? ThirdPartyLoanAccountId { get; set; }
        [StringLength(100)]
        public string PaymentMode { get; set; }
        [StringLength(200)]
        public string? BankRefNo { get; set; }
        [StringLength(200)]
        public string? ThirdPartyTxnId { get; set; }
        [StringLength(100)]
        public string Status { get; set; }
        public double Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double TotalAmount { get; set; }
        public double InterestAmount { get; set; }
        public double ProcessingFees { get; set; }
        public double PenalInterest { get; set; }
        public double OverdueInterest { get; set; }
        public double PrincipalAmount { get; set; }
        public double ExtraPaymentAmount { get; set; }




        public double? RemainingInterestAmount { get; set; }
        public double? RemainingProcessingFees { get; set; }
        public double? RemainingPenalInterest { get; set; }
        public double? RemainingOverdueInterest { get; set; }
        public double? RemainingPrincipalAmount { get; set; }
        public double? RemainingExtraPaymentAmount { get; set; }


        public bool? IsRunning { get; set; }

    }
}
