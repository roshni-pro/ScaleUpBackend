using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Common.MassTransitMiddleware;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Persistence;

namespace ScaleUP.Services.LeadAPI.Consumers
{
    public class KYCFailEventConsumer : IConsumer<IKYCFailEvent>
    {
        private readonly LeadApplicationDbContext _context;

        private readonly ILogger<KYCSuccessEventConsumer> _logger;
        private readonly IMassTransitService _massTransitService;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        readonly Token _user;
        public KYCFailEventConsumer(LeadApplicationDbContext context, ILogger<KYCSuccessEventConsumer> logger, IMassTransitService massTransitService)
        {
            _context = context;
            _logger = logger;
            _massTransitService = massTransitService;

        }

        public async Task Consume(ConsumeContext<IKYCFailEvent> context)
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

            LeadActivityCompletedEvent kycFailEvent = new LeadActivityCompletedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                IsSuccess = false,
                Message = context.Message.ErrorMessage,
                Id = 0
            };
            await _massTransitService.Publish(kycFailEvent);
        }
    }
}
