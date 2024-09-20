using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class KYCValidationResponse : BaseAuditableEntity
    {
        public long LeadMasterId { get; set; }
        public int DocumentMasterId { get; set; }
        [StringLength(50)]
        public string Status { get; set; }
        [StringLength(200)]
        public string kyc_id { get; set; }
        public string ResponseJson { get; set; }
        [StringLength(100)]
        public string Message { get; set; }
        [StringLength(100)]
        public string Remark { get; set; }
        public bool IsKycVerified { get; set; }
    }
}
