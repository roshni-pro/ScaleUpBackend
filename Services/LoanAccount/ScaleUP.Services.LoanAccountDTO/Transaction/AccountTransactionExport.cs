using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Transaction
{
    [DataContract]
    public class AccountTransactionExport
    {
        public long LoanAccountId { get; set; }
        public string LeadCode { get; set; }
        public string AccountCode { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public string CityName { get; set; }
        public string ThirdPartyLoanCode { get; set; }
        public string Status { get; set; }
        public double ActualOrderAmount { get; set; }
        public double PayableAmount { get; set; }
        public double PaidAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string ReferenceId { get; set; }
        public string UtilizationAnchor { get; set; }
        public string OrderNo { get; set; }
        public string InvoiceNo { get; set; }
        public int Aging { get; set; }
    }
}
