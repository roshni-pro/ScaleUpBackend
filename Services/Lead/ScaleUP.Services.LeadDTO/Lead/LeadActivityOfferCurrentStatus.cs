using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadActivityOfferCurrentStatus
    {
        public long? NBFCCompanyId { get; set; }
        public double? CreditLimit { get; set; }
        public double? ProcessingFee { get; set; }
        [StringLength(100)]
        public string? Status { get; set; }
        [StringLength(2000)]
        public string? Comment { get; set; }
        public long? LeadId { get; set; }
    }
}
