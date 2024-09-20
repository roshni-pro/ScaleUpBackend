using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels.ArthMate
{
    public class ArthMateUpdates : BaseAuditableEntity
    {
        public long LeadId { get; set; }
        [StringLength(100)]
        public string PartnerloanAppId { get; set; }
        [StringLength(100)]
        public string PartnerborrowerId { get; set; }
        [StringLength(100)]
        public string LoanAppId { get; set; }
        [StringLength(100)]
        public string borrowerId { get; set; }
        [StringLength(500)]
        public string? AScoreRequestId { get; set; }
        [StringLength(500)]
        public string? CeplerRequestId { get; set; }
        [StringLength(500)]
        public string? CeplerCustomerId { get; set; }

        [StringLength(500)]
        public string? ColenderRequestId { get; set; }
        public double? ColenderLoanAmount { get; set; }
        public double? ColenderPricing { get; set; }
        public int? ColenderAssignmentId { get; set; }

        [StringLength(200)]
        public string? ColenderProgramType { get; set; }
        [StringLength(100)]
        public string? ColenderStatus { get; set; }
        public DateTime? ColenderCreatedDate { get; set; }
        public bool IsAScoreWebhookHit { get; set; }
        [StringLength(100)]
        public string? Tenure { get; set; }
        [StringLength(1000)]
        public string? AgreementESignDocumentURL { get; set; }
        [StringLength(1000)]
        public string? AgreementPdfURL { get; set; }
        [StringLength(1000)]
        public string? OfferEmiDetailPdfURL { get; set; }
        public long? NBFCCompanyId { get; set; }
        public DateTime? AScoreCreatedDate { get; set; }
        public bool? IsOfferRejected { get; set; }
        //[StringLength(100)]
        //public string eSignRequestId { get; set; }
        //[StringLength(100)]
        //public string eSignDocumentId { get; set; }
        //public bool IseSignComplete { get; set; }
    }
}
