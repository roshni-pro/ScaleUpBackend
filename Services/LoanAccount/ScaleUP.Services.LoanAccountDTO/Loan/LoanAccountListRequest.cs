using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    [DataContract]
    public class LoanAccountListRequest
    {
        public string? ProductType { get; set; }
        public string Keyword { get; set; }
        public long AnchorId { get; set; }
        public int Status { get; set; } //block, active, inactive
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string CityName { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    [DataContract]
    public class LoanAccountListDc
    {
        public long LeadId { get; set; }
        public long ProductId { get; set; }
        public string UserId { get; set; }
        public string CustomerName { get; set; }
        public string LeadCode { get; set; }
        public string MobileNo { get; set; }
        public long NBFCCompanyId { get; set; }
        public long? AnchorCompanyId { get; set; }
        public double CreditScore { get; set; }
        public DateTime AgreementRenewalDate { get; set; }
        public DateTime DisbursalDate { get; set; }
        public double UtilizeAmount { get; set; }
        public double AvailableCreditLimit { get; set; }
        public double DisbursalAmount { get; set; }
        public long LoanAccountId { set; get; }
        public string ProductType { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string CityName { get; set; }
        public string AnchorName { get; set; }
        public double UtilizePercent { get; set; }
        public bool Status { get; set; } //IsAccountActive
        public long TotalCount { get; set; }
        public double TotalActive { get; set; }
        public double TotalInActive { get; set; }
        public double TotalDisbursal { get; set; }
        public bool IsBlock { get; set; }
        public string AccountStatus { get; set; }
        public string NBFCIdentificationCode { get; set; }
    }
}
