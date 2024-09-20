namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class CompanyListDTO
    {
        public string CompanyType { get; set; }
        public string? keyword { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class CompanyDTO
    {
        public List<long> companyIds { get; set; }
        public string? keyword { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }


    }
    public class AuditLogAggRequestDc
    {
        public string DatabaseName { get; set; }
        public string EntityName { get; set; }
        public long EntityId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class AuditLogAggResponseDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<AuditLog> AuditLogs { get; set; }
        public int TotalRecords { get; set; }
    }
    public class AuditLog
    {
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ActionType { get; set; }
        public string Changes { get; set; }
    }
}
