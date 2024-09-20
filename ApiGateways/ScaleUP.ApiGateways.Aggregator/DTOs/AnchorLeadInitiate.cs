using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class AnchorLeadInitiate
    {
        public string ProductCode { get; set; }       
        public string AnchorCompanyCode { get; set; }        
        [StringLength(10)]
        public string MobileNumber { get; set; }
        [StringLength(500)]
        public string Email { get; set; }
        [StringLength(500)]
        public string CustomerReferenceNo { get; set; }     
        public int VintageDays { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public List<BuyingHistories> BuyingHistories { get; set; }
    }


    public class CompanyProductDetail
    {
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public long CompanyId { get; set; }
        public string CompanyCode { get; set; }
    }
    
    public class BuyingHistories
    {        
        public DateTime MonthFirstBuyingDate { get; set; }       
        public int TotalMonthInvoice { get; set; }       
        public int MonthTotalAmount { get; set; }
    }
}
