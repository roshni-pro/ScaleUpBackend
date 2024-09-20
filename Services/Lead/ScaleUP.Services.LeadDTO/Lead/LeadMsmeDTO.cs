using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class LeadMsmeDTO : LeadBaseDTO
    {
        //public long LeadMasterId { get; set; }
        //public long DocumentMasterId { get; set; }
        //public string DocumentNumber { get; set; }
        //public string FrontFileUrl { get; set; }
        [Required]
        public long frontDocumentId { get; set; }
        //public string NameOnCard { get; set; }
        //public string DOB { get; set; }
        //public string IssuedDate { get; set; }
        //public string OtherInfo { get; set; }
        //public string PdfPassword { get; set; }
        public int vintage { get; set; }
        [Required]
        public string businessName { get; set; }
        [Required]
        public string businessType { get; set; }
        [Required]
        public string msmeRegNum { get; set; }
        [Required]
        public string msmeCertificateUrl { get; set; }
    }
}
