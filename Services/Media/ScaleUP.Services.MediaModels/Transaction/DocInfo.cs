using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.MediaModels.Transaction
{
    public class DocInfo : BaseAuditableEntity
    {
        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(50)]
        public string FileExtension { get; set; }


        [StringLength(500)]
        public string RelativePath { get; set; }

        public bool IsValidForLifeTime { get; set; }
        public int? ValidityInDays { get; set; }
    }
}
