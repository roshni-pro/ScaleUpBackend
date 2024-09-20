using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.ProductModels.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductModels.DSA
{
    [Table(nameof(SalesAgent))]
    public class SalesAgent : BaseAuditableEntity
    {
        [StringLength(13)]
        public required string MobileNo { get; set; }
        [StringLength(300)]
        public required string UserId { get; set; }
        [StringLength(11)]
        public string? PanNo { get; set; }
        [StringLength(300)]
        public string? PanUrl { get; set; }

        [StringLength(16)]
        public string? AadharNo { get; set; }
        [StringLength(300)]
        public required string AadharFrontUrl { get; set; }
        [StringLength(300)]
        public string? AadharBackUrl { get; set; }

        [StringLength(20)]
        public string? GstnNo { get; set; }
        [StringLength(300)]
        public string? GstnUrl { get; set; }

        [StringLength(300)]
        public string? AgreementUrl { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; }
        public long AnchorCompanyId { get; set; } // Anchor Company
        [StringLength(20)]
        public required string Type { get; set; }  // DSA  , Connector (Individual), DSAUser

        [StringLength(300)]
        public required string FullName { get; set; }

        [StringLength(300)]
        public required string CityName { get; set; }
        [StringLength(100)]
        public required string StateName { get; set; }
        [StringLength(200)]
        public string? EmailId { get; set; }

        [StringLength(100)]
        public string? Role { get; set; }// roles

        [StringLength(300)]
        public string? SelfieUrl { get; set; }

        [StringLength(500)]
        public string? AadharAddress { get; set; }
        [StringLength(200)]
        public string? WorkingLocation { get; set; }
        [StringLength(100)]
        public string DSACode { get; set; }
    }
}
