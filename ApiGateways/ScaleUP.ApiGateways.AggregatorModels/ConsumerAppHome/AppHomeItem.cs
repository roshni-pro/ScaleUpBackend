using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome
{
    public class AppHomeItem : BaseAuditableEntity
    {
        public string ItemType { get; set; }
        public int Sequence { get; set; }

        [ForeignKey("AppHomeId")]
        public AppHome AppHome { get; set; }
        public long AppHomeId { get; set; }
        public ICollection<AppHomeContent> AppHomeContent { get; set; }
    }
}
