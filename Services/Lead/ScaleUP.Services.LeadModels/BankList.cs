using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{

    public class BankList : BaseAuditableEntity
    {
        public string? aadhaarActiveFrom { get; set; }
        public string? aadhaarFlag { get; set; }
        public string bankId { get; set; }
        public string activeFrm { get; set; }
        public string debitcardFlag { get; set; }
        public string bankName { get; set; }
        public string dcActiveFrom { get; set; }
        public string netbankFlag { get; set; }


    }

}
