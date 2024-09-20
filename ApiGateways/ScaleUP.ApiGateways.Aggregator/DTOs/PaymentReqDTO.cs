using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class PaymentReqDTO
    {
        public string TransactionReqNo { get; set; }
        //public long AnchorCompanyId { get; set; }
        //public long ProductId { get; set;}
    }

    public class OrderPlacementRequestDTO
    {
        public string TransactionReqNo { get; set; }
        public double Amount { get; set; }
        public long LoanAccountId { get; set; }
        public int CreditDay { get; set; }
    }

    public class PostOrderPlacementRequestDTO
    {
        public string MobileNo { get; set; }
        public string otp { get; set; }
        public string TransactionReqNo { get; set; }
        public double Amount { get; set; }
        public long LoanAccountId { get; set; }
        public int CreditDay { get; set; }
    }
}
