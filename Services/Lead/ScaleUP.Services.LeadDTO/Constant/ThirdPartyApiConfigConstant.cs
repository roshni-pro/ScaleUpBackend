using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Constant
{
    public class ThirdPartyApiConfigConstant
    {
        public static string ExperianOTPRegistration { get; set; } = "ExperianOTPRegistration";
        public static string ExperianOTPGeneration { get; set; } = "ExperianOTPGeneration";
        public static string ExperianOTPValidation { get; set; } = "ExperianOTPValidation";
        public static string MaskedMobileGeneration { get; set; } = "MaskedMobileGeneration";
        public static string MaskedMobileOTPGeneration { get; set; } = "MaskedMobileOTPGeneration";
        public static string MaskedMobileOTPValidation { get; set; } = "MaskedMobileOTPValidation";
        public static string eNachBankList { get; set; } = "eNachBankList";
        //public static string eNachPost { get; set; } = "eNachPost";
        //public static string eNachKey { get; set; } = "eNachKey";
        public static string eNachConfig { get; set; } = "eNachConfig";
        public static string Shopkirana { get; set; } = "Shopkirana";
        public static string KarzaAdhaarVerification { get; set; } = "KarzaAdhaarVerification";
        public static string KarzaAdhaarOtp { get; set; } = "KarzaAdhaarOtp";
    }
}
