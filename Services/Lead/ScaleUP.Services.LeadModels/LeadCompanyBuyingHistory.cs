using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    [Table(nameof(LeadCompanyBuyingHistory))]
    public class LeadCompanyBuyingHistory : BaseAuditableEntity
    {
        public long CompanyLeadId { get; set; }
        public DateTime MonthFirstBuyingDate { get; set; }       
        public int TotalMonthInvoice { get; set; }
        public int MonthTotalAmount { get; set; }
        [ForeignKey("CompanyLeadId")]
        public CompanyLead CompanyLead { get; set; }
    }
}
