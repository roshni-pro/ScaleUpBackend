namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class PANDTO
    {
        public string panCard { get; set; }
        public string panImagePath { get; set; }
        public long? documentId { get; set; }
        public string? fatherName { get; set; }
        public DateTime dob { get; set; }
        public string? NameOnCard { get; set;}
    }
    public class LeadPANDTO
    {
        public string PANNo { get; set; }
        public string ImageURL { get; set; }
        public string DocumentId { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
