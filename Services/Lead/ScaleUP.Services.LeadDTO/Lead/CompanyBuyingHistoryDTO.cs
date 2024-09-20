using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class CompanyBuyingHistoryDTO
    {
        public DateTime MonthFirstBuyingDate { get; set; }
        public int TotalMonthInvoice { get; set; }
        public int MonthTotalAmount { get; set; }
    }
}
