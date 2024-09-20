using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil
{
    public class BlackSoilWithdrawalRequestInput
    {
        public long NBFCCompanyApiDetailId { get; set; }
        public string invoice_date { get; set; }
        public string invoice_number { get; set; }
        public double disbursed_amount { get; set; }  
        public double amount { get; set; } 
        public string file { get; set; }
    }
}
