﻿using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountModels.Master
{
    public class TransactionDetailHead : BaseAuditableEntity
    {
        [StringLength(100)]
        public string Code { get; set; }
        public int? SequenceNo { get; set; }
    }
}
