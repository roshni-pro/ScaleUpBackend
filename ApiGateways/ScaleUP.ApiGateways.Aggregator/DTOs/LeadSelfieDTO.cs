namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadSelfieDTO
    {
        public string FrontImageUrl { get; set; }
        public int? FrontDocumentId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
