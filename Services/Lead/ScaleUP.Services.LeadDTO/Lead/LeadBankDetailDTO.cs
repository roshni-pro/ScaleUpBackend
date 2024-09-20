using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadBankDetailDTO
    {
        public long LeadId { get; set; }
        public string Type { get; set; } ////borrower,beneficiary,
        public string BankName { get; set; }
        public string IFSCCode { get; set; }
        public string AccountType { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
        public string? PdfPassword { get; set; }
        public string SurrogateType { get; set; }
    }

    public class LeadBankInfoDTO
    {
        public List<LeadBankDetailDTO> leadBankDetailDTOs { get; set; }
        public bool isScaleUp { get; set; }
        public List<BankDoc> BankDocs { get; set; }
    }


    public class BankDoc
    {
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public string FileURL { get; set; }
        public int? Sequence { get; set; }
        public string? PdfPassword { get; set; }
        public string DocumentNumber { get; set; }
        public long? DocId { get; set; }
    }
}
