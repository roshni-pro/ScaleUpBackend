using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class LeadActivityMasterProgressListReply
    {
            public bool Status { get; set; }
            public string Message { get; set; }
            public List<LeadActivityProgress> LeadActivityProgress { get; set; }
    }

    public class LeadActivityProgress
    {
        public long ActivityMasterId { get; set; }
        public long? SubActivityMasterId { get; set; }
        public string ActivityName { get; set; }
        public string? SubActivityName { get; set; }
        public int Sequence { get; set; }
        public bool IsCompleted { get; set; }
        public int IsApproved { get; set; }
        public string leadUserId { get; set; }
        public string? RejectMessage { get; set; }
    }
}
