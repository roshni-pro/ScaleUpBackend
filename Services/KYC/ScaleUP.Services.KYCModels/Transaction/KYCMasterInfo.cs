using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.KYCModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCModels.Transaction
{
    public class KYCMasterInfo : BaseAuditableEntity
    {
        [StringLength(500)]
        public string UniqueId { get; set; }
        public long KYCMasterId { get; set; }
        public string? ResponseJSON{ get; set; }
        public string UserId { get; set; }
       
        [StringLength(5000)]
        public string? UniqueIdHash { get; set; }

        [StringLength(200)]
        public string? ProductCode { get; set; }

        public ICollection<KYCDetailInfo> KYCDetailInfoList { get; set; }
    }
}
