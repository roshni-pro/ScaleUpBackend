using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LoanAccountModels.Transaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class HopDashboardData : BaseAuditableEntity
    {
        [StringLength(100)]
        public string ProductType { get; set; }
        public long ProductId { get; set; }
        //public long AnchorCompanyId { get; set; }
        //[StringLength(200)]
        //public string AnchorName { get; set; }
        public long NBFCCompanyId { get; set; }
        [StringLength(100)]
        public string CityName { get; set; }
        public double LTDUtilizedAmount { get; set; }
        public double CreditLimitAmount { get; set; }
        public double OverdueAmount { get; set; }
        public double Outstanding { get; set; }
        //public double PrincipleOutstanding { get; set; }
        //public double InterestOutstanding { get; set; }
        //public double PenalOutStanding { get; set; }
        //public double OverdueInterestOutStanding { get; set; }
        //public double ODPrincipleOutstanding { get; set; }
        //public double ODInterestOutstanding { get; set; }
        //public double ODPenalOutStanding { get; set; }
        //public double ODOverdueInterestOutStanding { get; set; }
        //public double ScaleupShareAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        //public double TotalRepayment { get; set; }
        //public double PrincipalRepayment { get; set; }
        //public double InterestRepayment { get; set; }
        //public double OverdueInterestPayment { get; set; }
        //public double PenalRePayment { get; set; }
        //public double BounceRePayment { get; set; }
        //public double ExtraPayment { get; set; }
    }
}
