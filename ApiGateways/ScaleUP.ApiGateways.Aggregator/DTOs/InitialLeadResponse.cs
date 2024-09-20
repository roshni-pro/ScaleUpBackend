using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class InitialLeadResponse
    {
        public int CurrentSequence { get; set; }
        public List<LeadProductActivity> LeadProductActivity { get; set; }

    }

    public class LeadProductActivity
    {    
        public long ActivityMasterId { get; set; }       
        public long? SubActivityMasterId { get; set; }
        public string? KycMasterCode { get; set; }
        public string ActivityName { get; set; }
        public string? SubActivityName { get; set; }
        public int Sequence { get; set; }
        public long LeadId { get; set; }
        public bool IsEditable { get; set; }
        public string RejectedReason { get; set; }
    }
}
