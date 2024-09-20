using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    [DataContract]
    public class LoanAccountDTO
    {
        public long LeadId { get; set; }
        public long ProductId { get; set; }
        public string UserId { get; set; }
        public string AccountCode { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public long NBFCCompanyId { get; set; }
        public long? AnchorCompanyId { get; set; }
    }

    
    public class blackSoilAccountTransactionsDC
    {
        public long WithdrawalId { get; set; }
        public long AccountTransactionId { get; set; }
        public string InvoiceNo { get; set; }
        public string StatusCode { get; set; }
    }
}
