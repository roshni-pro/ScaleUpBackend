using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadBankStatementCreditLending :  LeadBaseDTO
    {
        public string? BankDocumentId { get; set; }
        public string? SarrogateDocId { get; set; }
        public string? SurrogateType { get; set; }   

    }
}
