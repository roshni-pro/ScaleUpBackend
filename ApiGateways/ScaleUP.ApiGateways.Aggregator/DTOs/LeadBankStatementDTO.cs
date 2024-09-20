namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadBankStatementDTO
    {
        public string DocumentNumber { get; set; }
        public string FrontFileUrl { get; set; }
        public string DocumentId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class BankStatementDTO
    {
        public string pdfPassword { get; set; }
        public double? enquiryAmount { get; set; }
        public string borroBankName { get; set; }
        public string borroBankIFSC { get; set; }
        public string borroBankAccNum { get; set; }
        public string bankStatement { get; set; }
        public string accType { get; set; }
        public long? documentId { get; set; }
    }
}
