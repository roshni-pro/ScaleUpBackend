using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity
{
    public class KYCBankStatementActivity
    {
        public long LeadMasterId { get; set; }
        //public long DocumentMasterId { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentId { get; set; }
        //public long? FrontDocumentId { get; set; }
        //public string? FrontFileUrl { get; set; }
        //public string NameOnCard { get; set; }
        //public string DOB { get; set; }
        //public string IssuedDate { get; set; }
        //public string OtherInfo { get; set; }
        public string PdfPassword { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public string CreatedBy { get; set; }
        //public DateTime ModifiedDate { get; set; }
        //public int? ModifiedBy { get; set; }
        //public bool IsVerified { get; set; }
        //public bool IsActive { get; set; }
        //public bool IsDeleted { get; set; }

        ////Extra Feild
        //public string FipId { get; set; }  //for ceplr
        public string BorroBankName { get; set; }
        public string BorroBankIFSC { get; set; }
        public string BorroBankAccNum { get; set; }
        //public string? GSTStatement { get; set; } //url
        public string BankStatement { get; set; } //url
        public double EnquiryAmount { get; set; }
        //public int SequenceNo { get; set; }
        public string AccType { get; set; }
    }
}
