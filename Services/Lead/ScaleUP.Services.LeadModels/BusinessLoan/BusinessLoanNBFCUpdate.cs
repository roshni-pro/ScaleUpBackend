using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.BusinessLoan
{
    public class BusinessLoanNBFCUpdate : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        public long NBFCCompanyId { get; set; }
        public string CompanyIdentificationCode { get; set; } // CompanyIdentificationCodeConstants
        public string? LoanId { get; set; }
        public int? Tenure { get; set; }
        public int? OfferStatus { get; set; } // 1- accepted, 2- rejected
        public double? LoanAmount { get; set; }
        public double? LoanInterestAmount { get; set; }
        public double? InterestRate { get; set; }
        public double? MonthlyEMI { get; set; }
        public double? ProcessingFeeRate { get; set; }
        public double? ProcessingFeeAmount { get; set; }
        public double? PFDiscount { get; set; }
        public double? GST { get; set; }
        public double? ProcessingFeeTax { get; set; }
        public string? NBFCRemark { get;set; }
        //public double? nbfcInterestRate { get; set; } // roi during generate offer by NBFC
        public double? Commission { get; set; }
        public string? CommissionType { get; set; } // amount/percent
        public string? PFType { get; set; } // amount/percent
        public double? brokenPeriodinterestAmount { get; set; }
        public double? Bounce { get; set; }
        public double? Penal { get; set; }
        public string ArrangementType { get; set; }
        public double? NBFCBounce { get; set; }
        public double? NBFCPenal { get; set; }
        public double NBFCInterest { get; set; }
        public double NBFCProcessingFee { get; set; }
        public string NBFCProcessingFeeType { get; set; }
    }
}
