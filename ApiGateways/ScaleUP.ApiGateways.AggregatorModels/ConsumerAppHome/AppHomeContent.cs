using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome
{
    public class AppHomeContent : BaseAuditableEntity
    {
        [StringLength(500)]
        public string ImageUrl { get; set; }
        public int Sequence { get; set;}
        [StringLength(500)]
        public string CallBackUrl { get; set; }

        [ForeignKey("AppHomeItemId")]
        public AppHomeItem AppHomeItem { get; set; }
        public long AppHomeItemId { get; set; }

        [ForeignKey("AppHomeFunctionId")]
        public AppHomeFunction AppHomeFunction { get; set; }
        public long? AppHomeFunctionId { get; set; }  
    }
}
