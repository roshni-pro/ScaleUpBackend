using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response
{
    public class repay_scheduleDc
    {
        public bool error { get; set; }
        public bool success { get; set; }
        public repay_scheduleData data { get; set; }
        public string message { get; set; }


    }
    public class repay_scheduleData
    {
        public List<repay_scheduleDetails> rows { get; set; }
        public int count { get; set; }
    }


    public class repay_scheduleDetails
    {
        //public int _id { get; set; }
        public int repay_schedule_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        //public string loan_id { get; set; }
        public int emi_no { get; set; }
        //public DateTime? due_date { get; set; }
        public double emi_amount { get; set; }
        public double prin { get; set; }
        public double int_amount { get; set; }
        //public int __v { get; set; }
        public double principal_bal { get; set; }
        public double principal_outstanding { get; set; }
        //public DateTime? created_at { get; set; }
        //public DateTime? updated_at { get; set; }


    }
    public class CommonResponseDc
    {
        public string Msg { get; set; }
        public bool Status { get; set; }
        public object Data { get; set; }
        public bool IsNotEditable { get; set; }
        public string NameOnCard { get; set; }
        public ArthMateOfferDc ArthMateOffer { get; set; }
        public string CompanyIdentificationCode { get; set; }
        public bool isActivation { get; set; }
        public long NBFCCompanyId { get; set; }

    }
    public class ArthMateOfferDc
    {
        public double loan_amt { get; set; }
        public double interest_rt { get; set; }
        public int loan_tnr { get; set; }
        public string loan_tnr_type { get; set; } //Month, Year
        public double Orignal_loan_amt { get; set; }
        public string Name { get; set; }

    }

    public class LeadRepaymentScheduleDetailDc
    {
        public int repay_schedule_id { get; set; }
        public int company_id { get; set; }
        public int product_id { get; set; }
        public int emi_no { get; set; }
        public DateTime? due_date { get; set; }
        public double emi_amount { get; set; }
        public double prin { get; set; }
        public double int_amount { get; set; }
        public double principal_bal { get; set; }
        public double principal_outstanding { get; set; }
    }
    public class DisbursementDataDc
    {
        public bool success { get; set; }
        public DisbursementData data { get; set; }
        public string message { get; set; }
    }
    public class DisbursementData
    {
        public string loan_id { get; set; }
        public string partner_loan_id { get; set; }
        public string status_code { get; set; }
        public double net_disbur_amt { get; set; }
        public string utr_number { get; set; }
        public string utr_date_time { get; set; }
    }

}
