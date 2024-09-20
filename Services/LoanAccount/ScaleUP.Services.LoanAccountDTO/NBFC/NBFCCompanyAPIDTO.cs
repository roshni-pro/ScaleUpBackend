using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LoanAccountDTO.NBFC
{
    public class NBFCCompanyAPIMasterDTO
    {
        public long NBFCCompanyAPIMasterId { get; set; }
        public string IdentificationCode { get; set; }
        public string TransactionStatuCode { get; set; }
        public string Status { get; set; }
        public long InvoiceId { get; set; }

        public List<NBFCCompanyAPIDetailDTO> NBFCCompanyAPIDetailDTOList { get; set; }
    }

    public class NBFCCompanyAPIDetailDTO
    {
        public long NBFCCompanyAPIDetailId { get; set; }
        public string APIUrl { get; set; }    
        public string Code { get; set; }
        public string TAPIKey { get; set; }
        public string TAPISecretKey { get; set; }
        public string? TReferralCode { get; set; }
        public int Sequence { get; set; }
        public string Status { get; set; }
    }

   
}
