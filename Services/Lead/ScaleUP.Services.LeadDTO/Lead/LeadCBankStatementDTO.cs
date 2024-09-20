using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadCBankStatementDTO : LeadBaseDTO
    {
        //public long DocumentMasterId { get; set; }
        public string DocumentNumber { get; set; }
        //public string FrontFileUrl { get; set; }
        //public string NameOnCard { get; set; }
        //public string DOB { get; set; }
        //public string IssuedDate { get; set; }
        //public string OtherInfo { get; set; }
        public string pdfPassword { get; set; }
        public string documentId { get; set; }
        public string borroBankName { get; set; }
        public string borroBankIFSC { get; set; }
        public string borroBankAccNum { get; set; }
        //public string GSTStatement { get; set; } //url
        public string bankStatement { get; set; } //url
        public double enquiryAmount { get; set; }
        //public int SequenceNo { get; set; }
        public string accType { get; set; }

    }
}
