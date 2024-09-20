namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LoanAccount_DisplayDisbursalAggDTO
    {
        public string LeadNo { get; set; }
        public DateTime AppliedDate { get; set; }
        public double CreditLimit { get; set; }
        public double ProcessingFeeAmount { get; set; }
        public double GSTAmount { get; set; }

        public string ProcessingFeePayableBy { get; set; }
        public double ConvenionFeeRate { get; set; }
        public double ConvenionGSTAmount { get; set; }
        public string ConvenionFeePayableBy { get; set; }
        //public double ProcessingFee { get; set; }
        //public double ProcessingFeeTax { get; set; }
        //public int Status { get; set; }
    }
}
