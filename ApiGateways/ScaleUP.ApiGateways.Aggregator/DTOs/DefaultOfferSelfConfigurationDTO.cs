using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class DefaultOfferSelfConfigurationDTO
    {
        public long? Id { get; set; }
        public long CompanyId { get; set; }
        public int MinCibilScore { get; set; }
        public int MaxCibilScore { get; set; }
        public double MultiPlier { get; set; }
        public double MaxCreditLimit { get; set; }
        public double MinCreditLimit { get; set; }
        public int MinVintageDays { get; set; }
        public int MaxVintageDays { get; set; }
        public string CustomerType { get; set; }
        public bool IsActive { get; set; }
    }
}
