using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome
{
    public class Test : BaseAuditableEntity
    {
     //public long Id { get; set; }
        [StringLength(500)]
        public string Name { get; set; }
    }
}
