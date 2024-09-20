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
    public class LoanAccount : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        public long ProductId { get; set; }
        [StringLength(100)]
        public string UserId { get; set; }
        [StringLength(100)]
        public string AccountCode { get; set; }
        [StringLength(500)]
        public string CustomerName { get; set; }
        [StringLength(100)]
        public required string LeadCode { get; set; }
        [StringLength(50)]
        public string MobileNo { get; set; }
        public long NBFCCompanyId { get; set; }
        public bool IsDefaultNBFC { get; set; }
        public long? AnchorCompanyId { get; set; }
        public double CreditScore { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime AgreementRenewalDate { get; set; }
        public DateTime DisbursalDate { get; set; }
        public virtual LoanAccountCredit LoanAccountCredits { get; set; }

        //public ICollection<LoanAccountCompanyLead> LoanAccountCompanyLeads { get; set; }
        public string? CityName { get; set; }
        public string? AnchorName { get; set; }
        public bool IsAccountActive { get; set; } // Active-1,InActive-0,Blocked-2
        public string ProductType { get; set; }
        public bool IsBlock { get; set; }
        public string IsBlockComment { get; set; }
        public bool IsBlockHideLimit { get; set; }
        public string NBFCIdentificationCode { get; set; }

        public ICollection<LoanAccountCompanyLead> LoanAccountCompanyLeads { get; set; }
        public ICollection<Invoice> Invoices { get; set; }


        [StringLength(200)]
        public string? ThirdPartyLoanCode { get; set; }
        public string? ShopName { get; set; }
        public string? CustomerImage { get; set; }

        [StringLength(100)]
        public string? Type { get; set; } //Customer, DSA, DSAUser, Connector
    }
}
