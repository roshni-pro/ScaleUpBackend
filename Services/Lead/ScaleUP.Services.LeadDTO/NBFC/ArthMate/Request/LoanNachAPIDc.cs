using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request
{
    public class LoanNachAPIDc
    {
        public string umrn { get; set; } //Mandatory
        public string mandate_ref_no { get; set; }
        public string nach_amount { get; set; }
        public string nach_registration_status { get; set; }
        public string nach_status_desc { get; set; }
        public string nach_account_holder_name { get; set; }
        public string nach_account_num { get; set; }
        public string nach_ifsc { get; set; }
        public string nach_start { get; set; }
        public string nach_end { get; set; }
    }

    public class LoanNachResponseDC
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class SlaLbaStampDc
    {
        //Used For, Partner Name, Stamp Amoun, Purpose, DateofUtilisation, Stamp Paper No, Loan ID on 21-02-2024
        public long? Id { get; set; }
        public int? StampPaperNo { get; set; }
        public string UsedFor { get; set; }
        public string PartnerName { get; set; }
        public string Purpose { get; set; }
        public double StampAmount { get; set; }
        public string LoanId { get; set; }
        public string StampUrl { get; set; }
        public bool? IsStampUsed { get; set; }
        public DateTime? DateofUtilisation { get; set; }
        public long? LeadmasterId { get; set; }


    }

    public class loandc
    {
        public long leadid { get; set; }
        public long id { get; set; }
        public string Response { get; set; }
    }
    public class BeneficiaryBankDetailDc
    {
        public long LeadMasterId { get; set; }
        public string Beneficiary_AccountNumber { get; set; }
        public string Beneficiary_Accountholdername { get; set; }
        public string Beneficiary_Typeofaccount { get; set; }
        public string Beneficiary_Bankname { get; set; }
        public string Beneficiary_IFSCCode { get; set; }
        public long BankDetailId { get; set; }
    }
}
