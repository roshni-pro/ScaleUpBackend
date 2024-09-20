using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{

    public class SlaLbaStampDetailsData
    {
        public long Id { get; set; }
        public string UsedFor { get; set; }
        public string PartnerName { get; set; }
        public double StampAmount { get; set; }
        public DateTime? DateofUtilisation { get; set; }
        public int StampPaperNo { get; set; }
        public long LeadmasterId { get; set; }
        public bool IsStampUsed { get; set; }
        public DateTime Created { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string StampUrl { get; set; }
        public string LeadName { get; set; }
        public string MobileNo { get; set; }
        public string LeadCode { get; set; }
        public int TotalRecord { get; set; }
    }

    public class GetAcceptedLoanDetailDC
    {
        public string LoanId { get; set; }
        public double LoanAmount { get; set; }
        public double InterestRate { get; set; }
        public int Tenure { get; set; }
        public double MonthlyEMI { get; set; }
        public double LoanInterestAmount { get; set; }
        public double ProcessingFeeRate { get; set; }
        public double ProcessingFeeAmount { get; set; }
        public double ProcessingFeeTax { get; set; }
        public double PFDiscount { get; set; }
        public string CompanyIdentificationCode { get; set; }
        public int OfferStatus { get; set; }
        public double GST { get; set; }
    }
    public class RejectNBFCOfferDC
    {
        public long LeadId { get; set; }
        public string? Role { get; set; }
        public string RejectMessage { get; set; }
        public long nbfcCompanyId { get; set; }
    }
}
 