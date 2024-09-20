using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class ArthmateDisbursement
    {
        [Key]
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        [StringLength(100)]
        public string loan_id { get; set; }
        [StringLength(100)]
        public string partner_loan_id { get; set; }
        [StringLength(100)]
        public string status_code { get; set; }
        public double net_disbur_amt { get; set; }
        [StringLength(100)]
        public string utr_number { get; set; }
        [StringLength(100)]
        public string utr_date_time { get; set; }

        public long LeadId { get; set; }


    }
}
