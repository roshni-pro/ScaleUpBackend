namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadDocuments
    {
        public long LeadId { get; set; }
        public string? DocumentNumber { get; set; }
        public string DocumentName { get; set; }
        public string? FileUrl { get; set; }
        public string? PdfPassword { get; set; }
        public long LeadDocDetailId { get; set; }
        public int? Sequence { get; set; }

    }
    public static class BlackSoilBusinessDocNameConstants
    {
        public const string GstCertificate = "gst_certificate";
        public const string UdyogAadhaar = "udyog_aadhaar";
        public const string Other = "other";
        public const string Statement = "bank_statement";
        public const string AadhaarFrontImage = "AadhaarFrontImage";
        public const string AadhaarBackImage = "AadhaarBackImage";
        public const string PANImage = "PANImage";
        public const string SurrogateGstCertificate = "surrogate_gst";
        public const string SurrogateITRCertificate = "surrogate_itr";
        public const string BusinessPhotos = "BusinessPhotos";

    }
}
