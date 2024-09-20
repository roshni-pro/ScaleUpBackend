using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class HopDashboardRevenue: BaseAuditableEntity
    {
        public long NBFCCompanyId { get; set; }
        public long ProductId { get; set; }
        [StringLength(100)]
        public string ProductType { get; set; }
        [StringLength(100)]
        public string CityName { get; set; }
        public double ScaleupShare { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
