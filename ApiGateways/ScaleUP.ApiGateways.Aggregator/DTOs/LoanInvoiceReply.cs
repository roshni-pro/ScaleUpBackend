using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LoanInvoiceReply
    {
        public bool status { get; set; }       
        public string Message { get; set; }
        public object Result { get; set; }
    }
}
