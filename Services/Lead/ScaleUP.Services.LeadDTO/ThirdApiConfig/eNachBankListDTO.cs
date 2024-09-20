using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.ThirdApiConfig
{
    public class eNachBankListDTO
    {
        public string ApiUrl { get; set; }
        public string ApiMasterId { get; set; }
    }

    public class eNachKeyDTO
    {
        public string value { get; set; }
    }

    public class eNachConfigDTO
    {
        public string ShortCode { get; set; }
        public string CustomerSequenceType { get; set; }
        public string MerchantCategoryCode { get; set; }
        public string UtilCode { get; set; }
        public string CustomerDebitFrequency { get; set; }
        public string CustomerMaxAmount { get; set; }
        public string CreditLimitMultiplier { get; set; }
        public string eNachPostApiUrl { get; set; }
        public long eNachPostApiMasterId { get; set; }
        public string eNachKey { get; set; }
    }


}
