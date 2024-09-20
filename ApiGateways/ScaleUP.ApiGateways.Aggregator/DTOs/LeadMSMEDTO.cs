namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadMSMEDTO
    {
        public string DocumentNumber { get; set; }
        public string FrontFileUrl { get; set; }
        public string NameOnCard { get; set; }
        public long FrontDocumentId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public int Vintage { get; set; }
    }

    public class MSMEDTO
    {
        public DateTime doi { get; set; }
        public string msmeCertificateUrl { get; set; }
        public string msmeRegNum { get; set; }
        public long frontDocumentId { get; set; }
        public string businessName { get; set; }
        public string businessType { get; set; }
        public int vintage { get; set; }
    }
}
