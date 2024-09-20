using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadModels;

namespace ScaleUP.Services.LeadAPI.Consumers
{
    public class LeadUpdateHistoryEventConsumer : IConsumer<LeadUpdateHistoryEvent>
    {
        private readonly LeadApplicationDbContext _context;

        private readonly ILogger<KYCSuccessEventConsumer> _logger;
        private readonly IMassTransitService _massTransitService;
        public LeadUpdateHistoryEventConsumer(LeadApplicationDbContext context, ILogger<KYCSuccessEventConsumer> logger, IMassTransitService massTransitService)
        {
            _context = context;
            _logger = logger;
            _massTransitService = massTransitService;

        }


        public async Task Consume(ConsumeContext<LeadUpdateHistoryEvent> context)
        {

            //var leadUpdateHistories = await _context.LeadUpdateHistories.FirstOrDefaultAsync(x => x.LeadId == context.Message.LeadId &&
            //                                                                            x.EventName == context.Message.EventName &&
            //                                                                            x.IsActive && !x.IsDeleted);
            //if (leadUpdateHistories == null)
            //{
            if (context != null && !String.IsNullOrEmpty(context.Message.Narretion) )
            {
                LeadUpdateHistory luh = new LeadUpdateHistory
                {
                    LeadId = context.Message.LeadId ?? 0,
                    UserId = context.Message.UserID,
                    UserName = context.Message.UserName,
                    EventName = context.Message.EventName,
                    Narration = context.Message.Narretion,
                    NarrationHTML = context.Message.NarretionHTML,

                    IsActive = true,
                    IsDeleted = false,
                    Created = context.Message.CreatedTimeStamp    //DateTime.Now
                };
            _context.LeadUpdateHistories.Add(luh);
            _context.SaveChanges();
            }

            //}
        }

    }
}

