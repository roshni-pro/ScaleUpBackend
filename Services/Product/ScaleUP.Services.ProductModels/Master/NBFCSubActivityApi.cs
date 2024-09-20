using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductModels.Master
{
    public class NBFCSubActivityApi : BaseAuditableEntity
    {
        public long NBFCCompanyApiId { get; set; }

        [ForeignKey("NBFCCompanyApiId")]
        public CompanyApi NBFCCompanyApis { get; set; }

        public required long ActivityMasterId { get; set; }
        [ForeignKey("ActivityMasterId")]
        public ActivityMasters ActivityMasters { get; set; }

        public long? SubActivityMasterId { get; set; }
        [ForeignKey("SubActivityMasterId")]
        public SubActivityMasters SubActivityMasters { get; set; }
        public required int Sequence { get; set; }
        public long? ProductCompanyActivityMasterId { get; set; }

    }
}
