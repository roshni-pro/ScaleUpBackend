using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request
{
    public class GenerateToken
    {
        public string refId { get; set; }
        public string partnerId { get; set; }
        public string apiKey { get; set; }

    }

    public class AddLead
    {

        public string refId { get; set; }
        public string leadId { get; set; }
        public string salutation { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string mobileNumber { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string fatherName { get; set; }
        public string address { get; set; }
        public string addressLine1 { get; set; }
        public string landmark { get; set; }
        public string pincode { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string storeSize { get; set; }
        public string gstNo { get; set; }
        public string shopName { get; set; }
        public string udyogAadhaar { get; set; }
        public string businessType { get; set; }
        public string storeLength { get; set; }
        public string storeWidth { get; set; }
        public string signBoardLength { get; set; }
        public string signBoardWidth { get; set; }
        public string signBoardType { get; set; }
        public string customerAttendingCapacity { get; set; }
        public string storeArea { get; set; }
        public string userName { get; set; }


    }

    public class CheckCreditLine
    {
        public string refId { get; set; }
        public string leadId { get; set; }

    }

    public class TransactionVerifyOtp
    {
        public string refId { get; set; }
        public string leadId { get; set; }
        public string otp { get; set; }

    }
}