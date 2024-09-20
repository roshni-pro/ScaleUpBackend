using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome
{
    public class AppHome : BaseAuditableEntity
    {
        public string Name { get; set; }
        public ICollection<AppHomeItem> AppHomeItem { get; set; }
    }
}
