﻿using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class CommonApiRequestResponse : BaseAuditableEntity
    {
        [StringLength(100)]
        public string UserId { get; set; }
        public long APIConfigId { get; set; }
        public long CompanyId { get; set; }

        [MaxLength(200)]
        public string? Request { get; set; }
        [MaxLength(200)]
        public string? Response { get; set; }
        [MaxLength(200)]
        public string? ProcessedResponse { get; set; }
        public bool IsError { get; set; }
    }
}