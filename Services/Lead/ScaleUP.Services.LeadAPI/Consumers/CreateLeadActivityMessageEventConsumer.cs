using MassTransit;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Persistence;

namespace ScaleUP.Services.LeadAPI.Consumers
{
    public class CreateLeadActivityMessageEventConsumer : IConsumer<CreateLeadActivityMessage>
    {
        private readonly LeadApplicationDbContext _context;

        private readonly ILogger<KYCSuccessEventConsumer> _logger;
        private readonly IMassTransitService _massTransitService;
        public CreateLeadActivityMessageEventConsumer(LeadApplicationDbContext context, ILogger<KYCSuccessEventConsumer> logger, IMassTransitService massTransitService)
        {
            _context = context;
            _logger = logger;
            _massTransitService = massTransitService;

        }

        public async Task Consume(ConsumeContext<CreateLeadActivityMessage> context)
        {
            //var leadActivityMasterProgresses = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == context.Message.LeadId &&
            //                                                                            x.ActivityMasterId == context.Message.ActivityId &&
            //                                                                            x.SubActivityMasterId == context.Message.SubActivityId &&
            //                                                                            x.IsActive && !x.IsDeleted);
            //if (leadActivityMasterProgresses != null)
            //{
            //    leadActivityMasterProgresses.IsCompleted = true;
            //    await _context.SaveChangesAsync();
            //    _logger.LogInformation("Lead: {leadId} with ActivityId: {ActivityId} or SubActivityId: {SubActivityId} completed successfully", context.Message.LeadId, context.Message.ActivityId, context.Message.SubActivityId);
            //}
            //else
            //{
            //  _logger.LogError("Lead: {leadId} Activeity with ActivityId: {ActivityId} not found or SubActivityId: {SubActivityId} not found", context.Message.LeadId, context.Message.ActivityId, context.Message.SubActivityId);
            //}
            _logger.LogInformation("Fail Kyc for Lead: {leadId} with ActivityId: {ActivityId} or SubActivityId: {SubActivityId} completed successfully", context.Message.LeadId, context.Message.ActivityId, context.Message.SubActivityId);

            //LeadActivityCompletedEvent kycFailEvent = new LeadActivityCompletedEvent
            //{
            //    CorrelationId = context.Message.CorrelationId
            //};
            //await _massTransitService.Publish(kycFailEvent);
        }

        
    }
}
