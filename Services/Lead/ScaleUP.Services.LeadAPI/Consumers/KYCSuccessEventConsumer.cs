using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Common.MassTransitMiddleware;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadModels;

namespace ScaleUP.Services.LeadAPI.Consumers
{
    public class KYCSuccessEventConsumer : IConsumer<IKYCSuccessEvent>
    {
        private readonly LeadApplicationDbContext _context;
        private IHostEnvironment _hostingEnvironment;

        private readonly ILogger<KYCSuccessEventConsumer> _logger;

        private readonly IMassTransitService _massTransitService;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        readonly Token _user;

        private readonly LeadHistoryManager _leadHistoryManager;

        public KYCSuccessEventConsumer(LeadApplicationDbContext context,
        ILogger<KYCSuccessEventConsumer> logger, IMassTransitService massTransitService
        , Token user, IHostEnvironment hostingEnvironment
        , LeadHistoryManager leadHistoryManager)
        {
            _context = context;
            _logger = logger;
            _massTransitService = massTransitService;
            _user = user;
            _hostingEnvironment = hostingEnvironment;
            _leadHistoryManager = leadHistoryManager;
        }

        public async Task Consume(ConsumeContext<IKYCSuccessEvent> context)
        {

            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

            var leadActivityMasterProgresses = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == context.Message.LeadId &&
                                                                                        x.ActivityMasterId == context.Message.ActivityId &&
                                                                                        (context.Message.SubActivityId == 0 || x.SubActivityMasterId == context.Message.SubActivityId) &&
                                                                                        x.IsActive && !x.IsDeleted);
            if (leadActivityMasterProgresses != null)
            {
                leadActivityMasterProgresses.IsCompleted = true;
                leadActivityMasterProgresses.IsApproved = ((ActivityEnum.KYC.ToString() == leadActivityMasterProgresses.ActivityMasterName || ActivityEnum.DSATypeSelection.ToString() == leadActivityMasterProgresses.ActivityMasterName) && context.Message.IsSuccess ? 1 : 0);
                leadActivityMasterProgresses.KycMasterInfoId = context.Message.KycMasterId;
                leadActivityMasterProgresses.LastModifiedBy = _user.Value;
                _context.Entry(leadActivityMasterProgresses).State = EntityState.Modified;
                var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == context.Message.LeadId);



                //if (ActivityEnum.KYC.ToString() == leadActivityMasterProgresses.ActivityMasterName)
                //{
                if (ActivityEnum.KYC.ToString().ToUpper() == leadActivityMasterProgresses.ActivityMasterName.ToUpper() && (leadActivityMasterProgresses.SubActivityMasterName.ToUpper() == KYCMasterConstants.PAN || leadActivityMasterProgresses.SubActivityMasterName.ToUpper() == KYCMasterConstants.Aadhar))
                {
                    if(string.IsNullOrEmpty(lead.Status) || lead.Status == "Initiate")
                    {
                        lead.Status = leadActivityMasterProgresses.IsApproved == 1 ? LeadStatusEnum.KYCInProcess.ToString() : LeadStatusEnum.KYCFailed.ToString();
                    } 
                    if(leadActivityMasterProgresses.IsApproved == 2)
                    {
                        lead.Status = LeadStatusEnum.KYCFailed.ToString();
                    }
                    //else if(string.IsNullOrEmpty(lead.Status) && lead.ProductCode != "DSA")
                    //{
                    //    lead.Status = leadActivityMasterProgresses.IsApproved == 1 ? LeadStatusEnum.KYCInProcess.ToString() : LeadStatusEnum.KYCFailed.ToString();
                    //}
                }
                if (leadActivityMasterProgresses.ActivityMasterName.ToUpper() == "PERSONALINFO" || (leadActivityMasterProgresses.ActivityMasterName.ToUpper() == KYCMasterConstants.DSAPersonalDetail.ToUpper() || leadActivityMasterProgresses.ActivityMasterName.ToUpper() == KYCMasterConstants.DSAPersonalInfo.ToUpper()) || leadActivityMasterProgresses.ActivityMasterName.ToUpper() == KYCMasterConstants.ConnectorPersonalDetail.ToUpper())
                {
                    if (lead.Status != LeadStatusEnum.Submitted.ToString())
                    {
                        lead.Status = leadActivityMasterProgresses.IsCompleted == true ? LeadStatusEnum.KYCSuccess.ToString() : LeadStatusEnum.KYCFailed.ToString();
                    }
                }
                if (leadActivityMasterProgresses.ActivityMasterName.ToUpper() == "BUSINESSINFO")
                {
                    lead.Status = leadActivityMasterProgresses.IsCompleted == true ? LeadStatusEnum.Submitted.ToString() : LeadStatusEnum.BusinessKycFailed.ToString();

                    if(leadActivityMasterProgresses.IsCompleted == true)
                    lead.SubmittedDate = indianTime;
                }

                lead.LastModified = indianTime;
                lead.LastModifiedBy = _user.Value;
                _context.Entry(lead).State = EntityState.Modified;
                //}

                await _context.SaveChangesAsync();
                bool IsKYCCompleted = false;
                if (context.Message.ComapanyId.HasValue && context.Message.ComapanyId.Value > 0)
                {
                    IsKYCCompleted = (await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == context.Message.LeadId &&
                                                                                        x.ActivityMasterName == ActivityEnum.KYC.ToString() &&
                                                                                        x.IsActive && !x.IsDeleted).ToListAsync()).All(x => x.IsCompleted);
                    if (IsKYCCompleted)
                    {
                        var CompanyLead = await _context.CompanyLead.FirstOrDefaultAsync(x => x.LeadId == context.Message.LeadId && x.CompanyId == context.Message.ComapanyId.Value);
                        if (CompanyLead != null)
                        {
                            CompanyLead.LeadProcessStatus = 2;
                            CompanyLead.LastModifiedBy = _user.Value;
                            _context.Entry(CompanyLead).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                        }

                    }

                    LeadManager leadManager = new LeadManager(_context, _hostingEnvironment, _leadHistoryManager , _massTransitService);
                    await leadManager.ManageRejectActivity(new ManageRejectActivityDc
                    {
                        ActivityMasterId = context.Message.ActivityId,
                        SubActivityMasterId = context.Message.SubActivityId,
                        IsRejected = context.Message.IsSuccess,
                        Message = context.Message.Message,
                        LeadId = context.Message.LeadId,
                        IsKYCCompleted = IsKYCCompleted
                    });


                }
                _logger.LogInformation("Lead: {leadId} with ActivityId: {ActivityId} or SubActivityId: {SubActivityId} completed successfully", context.Message.LeadId, context.Message.ActivityId, context.Message.SubActivityId);
            }
            else
            {
                _logger.LogError("Lead: {leadId} Activeity with ActivityId: {ActivityId} not found or SubActivityId: {SubActivityId} not found", context.Message.LeadId, context.Message.ActivityId, context.Message.SubActivityId);
            }

            LeadActivityCompletedEvent kycFailEvent = new LeadActivityCompletedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                IsSuccess = true,
                Id = context.Message.KycMasterId,
                Message = context.Message.Message
            };
            await _massTransitService.Publish(kycFailEvent);
        }
    }
}
