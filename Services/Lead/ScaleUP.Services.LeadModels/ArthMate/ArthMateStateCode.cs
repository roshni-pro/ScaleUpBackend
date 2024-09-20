using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class ArthMateStateCode : BaseAuditableEntity
    {
        public int StateCode { get; set; }
        [StringLength(100)]
        public string State { get; set; }
        public double PINprefixMin { get; set; }
        public double PINprefixMax { get; set; }
    }
}
