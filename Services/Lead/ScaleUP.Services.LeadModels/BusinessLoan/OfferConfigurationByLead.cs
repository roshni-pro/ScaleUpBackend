﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScaleUP.Global.Infrastructure.Common.Models;

namespace ScaleUP.Services.LeadModels.BusinessLoan
{
    public class OfferConfigurationByLead :BaseAuditableEntity
    {
        public required long LeadId { get; set; }
        public required long CompanyId { get; set; }
        public required long ProductId { get; set; }
        [StringLength(100)]
        public required string SlabType { get; set; } //SlabTypeConstants
        public required double MinLoanAmount { get; set; }
        public required double MaxLoanAmount { get; set; }
        [StringLength(100)]
        public required string ValueType { get; set; } // ValueTypeConstants
        public required double MinValue { get; set; }
        public required double MaxValue { get; set; }
        public double? SharePercentage { get; set; } //(Over and Above Share %)
        public required bool IsFixed { get; set; }
    }
}