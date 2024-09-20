using MassTransit;

namespace ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity
{
    public interface IKYCFailEvent : CorrelatedBy<Guid>
    {
        public long LeadId { get; set; }
        public long ActivityId { get; set; }
        public long? SubActivityId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
