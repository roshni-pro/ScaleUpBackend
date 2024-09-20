using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.LeadNBFC
{
    public class LeadNBFCSubActivity : BaseAuditableEntity
    {
        public required long NBFCCompanyId { get; set; }
        public required long ActivityMasterId { get; set; }
        public string ActivityName { get; set; }
        public long? SubActivityMasterId { get; set; }
        [StringLength(200)]
        public string Code { get; set; } //SubActivityName
        [StringLength(200)]
        public string Status { get; set; }
        public long LeadId { get; set; }
        public ICollection<LeadNBFCApi> LeadNBFCApis { get; set; }

        [StringLength(50)]
        public string IdentificationCode { get; set; }
        public int SubActivitySequence { get; set; }
    }
}
