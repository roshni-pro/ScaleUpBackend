namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class eNachBankDetailAggDc
    {
        public long LeadMasterId { set; get; }
        public string Name { set; get; }
        public string BankName { set; get; }
        public string AccountNo { set; get; }
        public string IfscCode { set; get; }
        public string AccountType { set; get; } // “S” for Savings , “C” for Current or “O” “Other”.
        public string RelationshipTypes { set; get; } //SOW,JOF,NRE,JOO,NRO ()
        public string Channel { set; get; } //"Debit" for Debit Card, "Net" for Net-banking.
        public string UMRN { set; get; }
    }
}
