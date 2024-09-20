using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.Loan
{
    public class InvoiceRequestDTO
    {
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoicePdfURL { get; set; } 
        public string OrderNo { get; set; }
        public string StatusMsg { get; set; }
        public double InvoiceAmount { get; set; }
        public string? ayeFinNBFCToken { get; set; } = "";
    }
}
