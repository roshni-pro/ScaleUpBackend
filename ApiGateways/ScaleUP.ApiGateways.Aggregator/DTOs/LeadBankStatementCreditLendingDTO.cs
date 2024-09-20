namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadBankStatementCreditLendingDTO
    {
        public int? DocumentId { get; set; }
        public string ImageUrl { get; set; }
        //public int? GSTDocumentId { get; set; }
        //public string? GSTImageUrl { get; set; }
        //public int? ITRDocumentId { get; set; }
        //public string? ITRImageUrl { get; set; }
        //public string? SurrogateType { get; set; }

    }
    public class LeadSarrogateStmtCLDTO
    {
        public int? DocumentId { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class LeadITRStatementCreditLendingDTO
    {
        public int? DocumentId { get; set; }
        public string? ImageUrl { get; set; }        
    }
    public class LeadStatementCreditLendingDTO
    {
        public List<LeadBankStatementCreditLendingDTO> LeadBankStatementCreditLendingDTO { get; set; }
        public List<LeadSarrogateStmtCLDTO> LeadSarrogateDocDTO { get; set; }
        public string? SurrogateType { get; set; }
    }

}
