using MassTransit;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.Global.Infrastructure.Common.MassTransitMiddleware;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.KYCAPI.KYCFactory;
using ScaleUP.Services.KYCAPI.Persistence;

namespace ScaleUP.Services.KYCAPI.Consumers
{
    public class LeadUpdateAadharEventConsumer : IConsumer<LeadUpdateAadharEvent>
    {

        private string _basePath;
        private readonly ApplicationDbContext _context;
        private readonly IHostEnvironment _hostingEnvironment;
        //private IDocType<KarzaPANDTO, KYCActivityPAN> docType;
        private readonly IMassTransitService _massTransitService;
        private KYCDocFactory kYCDocFactory;
        readonly Token _user;

        //private readonly ILogger<KYCSuccessEventConsumer> _logger;

        public LeadUpdateAadharEventConsumer(ApplicationDbContext context
            //, ILogger<KYCSuccessEventConsumer> logger
            , IMassTransitService massTransitService
            , IHostEnvironment hostingEnvironment
            , Token User
            )
        {
            _context = context;
            //_logger = logger;
            _massTransitService = massTransitService;
            _hostingEnvironment = hostingEnvironment;
            _basePath = hostingEnvironment.ContentRootPath;
            _user = User;
        }
        public async Task Consume(ConsumeContext<LeadUpdateAadharEvent> context)
        {
            if (!string.IsNullOrEmpty(context.Message.KYCMasterCode))
            {

                kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                var KarzaAadharDocType = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCActivityAadhar>(context.Message.JSONString);
                IDocType<KarzaAadharDTO, KYCActivityAadhar> docTypeAadhar = kYCDocFactory.GetDocType<KarzaAadharDTO, KYCActivityAadhar>(context.Message.KYCMasterCode);
                var aadharInfo1 = await docTypeAadhar.GetByUniqueId(KarzaAadharDocType);
                var aadharInfo = aadharInfo1.Result;
               KarzaAadharDocType.aadharInfo = aadharInfo;

                if (aadharInfo != null && aadharInfo.address != null && aadharInfo.address.splitAddress != null)
                {
                    aadharInfo.DocumentNumber = KarzaAadharDocType.DocumentNumber;

                    var updatingAddressEvent  = new UpdatingAddressEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        JSONString = JsonConvert.SerializeObject(KarzaAadharDocType),
                        KYCMasterCode = context.Message.KYCMasterCode,
                        UserId = context.Message.UserId,
                        ActivityId = context.Message.ActivityId,
                        LeadId = context.Message.LeadId,
                        SubActivityId = context.Message.SubActivityId,
                        ComapanyId = context.Message.ComapanyId,
                        ProductCode = context.Message.ProductCode
                    };
                    await _massTransitService.Publish(updatingAddressEvent);
                }
                else
                {
                    KYCFailEvent kycFailEvent = new KYCFailEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        ActivityId = context.Message.ActivityId,
                        LeadId = context.Message.LeadId,
                        SubActivityId = context.Message.SubActivityId,
                        ErrorMessage = aadharInfo1.Message,
                    };

                    await _massTransitService.Publish(kycFailEvent);
                }
            }

        }
    }
}
