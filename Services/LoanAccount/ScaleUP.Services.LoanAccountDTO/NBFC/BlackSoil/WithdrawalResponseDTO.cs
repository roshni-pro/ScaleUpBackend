using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil
{
    public class WithdrawalResponseDTO
    {
        public int id { get; set; }
        public string update_url { get; set; }
        public string status { get; set; }
        public string invoice_id { get; set; }
    }
}
