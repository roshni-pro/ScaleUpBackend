using ScaleUP.Global.Infrastructure.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadModels
{
    public class eNachBankDetail : BaseAuditableEntity
    {
        public string BankName { set; get; }
        public string AccountNo { set; get; }
        public string IfscCode { set; get; }
        public string AccountType { set; get; } // “S” for Savings , “C” for Current or “O” “Other”.
        public string Channel { set; get; } //"Debit" for Debit Card, "Net" for Net-banking.
        public string MsgId { set; get; }
        public string UMRN { set; get; }
        public string? responseJSON { set; get; }
        

        public required long LeadId { get; set; }
        [ForeignKey("LeadId")]
        public Leads Leads { get; set; }
    }
}
