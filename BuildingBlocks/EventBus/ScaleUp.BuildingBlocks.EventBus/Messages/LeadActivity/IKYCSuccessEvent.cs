using MassTransit;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public interface IKYCSuccessEvent : CorrelatedBy<Guid>
    {
        public long LeadId { get; set; }
        public long KycMasterId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public long? ComapanyId { get; set; }
    }
}
