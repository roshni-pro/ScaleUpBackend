using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Response
{
    public class GenerateTokenResponse
    {
        public string refId { get; set; }
        public string switchpeReferenceId { get; set; }
        public string status { get; set; }
        public string? token { get; set; }
        public string? message { get; set; }

    }

    //public class AyeCommonResponse
    //{
    //    public string refId { get; set; }
    //    public string switchpeReferenceId { get; set; }
    //    public string status { get; set; }
    //    public string? token { get; set; }
    //    public string? message { get; set; }
    //}
    public class AddLeadResponse    
    {
        public string refId { get; set; }
        public string switchpeReferenceId { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    //public class CheckCreditLineResponse
    //{
    //    public string refId { get; set; }
    //    public string switchpeReferenceId { get; set; }
    //    public string status { get; set; }
    //    public string message { get; set; }
    //    public CheckCreditLineData data { get; set; }

    //}
    //public class CheckCreditLineData
    //{
    //    public string status { get; set; }
    //    public string statusMessage { get; set; }
    //    public bool creditLineExist { get; set; }
    //    public string loanId { get; set; }
    //    public double totalLimit { get; set; }
    //    public double availableLimit { get; set; }

    //}
    public class TransactionVerifyOtpResponse
    {
        public string refId { get; set; }
        public string switchpeReferenceId { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public TransactionVerifyOtpData data { get; set; }

    }
    public class TransactionVerifyOtpData
    {
        public double totalLimit { get; set; }
        public double availableLimit { get; set; }
       

    }
}
