using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.ApiGateways.AggregatorModels.ConsumerAppHome
{
    public class AppHomeFunction : BaseAuditableEntity
    {
        public string FunctionName { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public ICollection<AppHomeContent> AppHomeContent { get; set; }
    }
}
