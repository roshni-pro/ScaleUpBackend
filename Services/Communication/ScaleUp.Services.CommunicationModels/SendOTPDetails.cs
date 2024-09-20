
using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.CommunicationModels
{
    [Table(nameof(SendOTPDetails))]
    public class SendOTPDetails : BaseAuditableEntity
    {
        [StringLength(20)]
        public string MobileNo { get; set; }
        [StringLength(1000)]
        public string SMS { get; set; }
        [StringLength(10)]
        public string OTP { get; set; }
        public int? ExpiredInMin { get; set; }
        public bool IsValidated { get; set; }
        public bool IsSent { get; set; }
        [StringLength(1000)]
        public string Comment { get; set; }

    }
}
