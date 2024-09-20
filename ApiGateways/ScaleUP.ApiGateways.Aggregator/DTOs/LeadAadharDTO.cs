namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadAadharDTO
    {
        public string FrontImageUrl { get; set; }
        public string BackImageUrl { get; set; }
        public string DocumentNumber { get; set; }
        public int? FrontDocumentId { get; set; }
        public int? BackDocumentId { get; set; }
    }
}
