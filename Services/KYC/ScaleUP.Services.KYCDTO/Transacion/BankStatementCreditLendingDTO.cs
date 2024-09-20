using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class BankStatementCreditLendingDTO
    {
        //public string? DocumentId { get; set; }
        public string? BankDocumentId { get; set; }
        public string? SarrogateDocId { get; set; }

        //public string? ITRDocumentId { get; set; }
        public string? SurrogateType { get; set; }
    }
}
