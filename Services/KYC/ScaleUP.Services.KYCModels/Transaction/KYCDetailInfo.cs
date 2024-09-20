using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.KYCModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCModels.Transaction
{
    public class KYCDetailInfo : BaseAuditableEntity
    {
        public string FieldValue{ get; set; }
        public long KYCDetailId { get; set; }
        [ForeignKey("KYCDetailId")]

        public KYCDetail KYCDetail { get; set; }
        public long KYCMasterInfoId { get; set; }
        [ForeignKey("KYCMasterInfoId")]
        public KYCMasterInfo KYCMasterInfo { get; set; }
    }
}
