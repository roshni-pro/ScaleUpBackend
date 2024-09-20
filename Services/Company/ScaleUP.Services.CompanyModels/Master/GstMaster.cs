using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CompanyModels.Master
{
    public class GstMaster : BaseAuditableEntity
    {
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required double GSTRate { get; set; }
        [StringLength(100)]
        public string? Code { get; set; } //like Gst18, Tds5

    }
}
