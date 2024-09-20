using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    [DataContract]
    public class PaymentRequestDetailDc
    {
        public long LoanAccountId { get; set; }
        public long LeadId { get; set; }
        //public double WithGSTConvenionFee { get; set; }
        //public string TransactionReqNo { get; set; }
        //public double TransactionAmount { get; set; }
        //public DateTime DueDate { get; set; }
        //public string PaymentStatus { get; set; }
        //public string MobileNo { get; set; }
        //public double ConvenionFee { get; set; }
        //public double GstRate { get; set; }
        //public string PaymentMode { get; set; }
        //public long NBFCCompanyId { get; set; }
        //public long AnchorCompanyId { get; set; }
        //public long OrderId { get; set; }
    }
}
