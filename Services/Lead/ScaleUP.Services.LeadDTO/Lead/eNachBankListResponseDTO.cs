using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.LeadDTO.Lead
{
    public class eNachBankListResponseDTO
    {
        public List<LiveBankList> liveBankList { get; set; }
        //public string error { get; set; }
    }


    public class LiveBankList
    {
        public string? aadhaarActiveFrom { get; set; }
        public string aadhaarFlag { get; set; }
        public string bankId { get; set; }
        public string activeFrm { get; set; }
        public string debitcardFlag { get; set; }
        public string bankName { get; set; }
        public string dcActiveFrom { get; set; }
        public string netbankFlag { get; set; }
    }

    public class eNachBankListConfigDc
    {
        public string ApiUrl { get; set; }
        public long ApiMasterId { get; set; }
    }

    public class eNachConfigDc
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
