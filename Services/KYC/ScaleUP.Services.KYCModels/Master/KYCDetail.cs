using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCModels.Master
{
    public class KYCDetail : BaseAuditableEntity
    {
        [StringLength(100)]
        public string Field { get; set; }
        public int Sequence { get; set; }

        public long KYCMasterId { get; set; }
        [ForeignKey("KYCMasterId")]
        public KYCMaster KYCMaster { get; set; }
        public int FieldType{ get; set; }
        public bool IsPrimaryField{ get; set; }
        public int FieldInfoType { get; set; }

    }
}
