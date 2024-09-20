using MassTransit;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.DSA;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Common.MassTransitMiddleware;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.MassTransit;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.KYCAPI.KYCFactory;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCDTO.Transacion.DSA;

namespace ScaleUP.Services.KYCAPI.Consumers
{
    public class LeadActivityCreatedEventConsumer : IConsumer<LeadActivityCreatedEvent>
    {
        private string _basePath;
        private readonly ApplicationDbContext _context;
        //private IDocType<KarzaPANDTO, KYCActivityPAN> docType;
        private readonly IMassTransitService _massTransitService;
        private KYCDocFactory kYCDocFactory;
        readonly Token _user;
        //private readonly ILogger<KYCSuccessEventConsumer> _logger;
        private readonly KYCHistoryManager _kYCHistoryManager;

        public LeadActivityCreatedEventConsumer(ApplicationDbContext context
            //, ILogger<KYCSuccessEventConsumer> logger
            , IMassTransitService massTransitService
            , IHostEnvironment hostingEnvironment
            , Token User
            , KYCHistoryManager kYCHistoryManager
            )
        {
            _context = context;
            //_logger = logger;
             _massTransitService = massTransitService;
            _basePath = hostingEnvironment.ContentRootPath;
            _user = User;
            _kYCHistoryManager = kYCHistoryManager;
        }
        public async Task Consume(ConsumeContext<LeadActivityCreatedEvent> context)
        {
            
            if (!string.IsNullOrEmpty(context.Message.KYCMasterCode))
            {
                ResultViewModel<long> res = new ResultViewModel<long>();
                switch (context.Message.KYCMasterCode)
                {
                    case Global.Infrastructure.Constants.KYCMasterConstants.PAN:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var KYCActivityPAN = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCActivityPAN>(context.Message.JSONString);
                        IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(context.Message.KYCMasterCode);
                        res = await docType.GetAndSaveByUniqueId(KYCActivityPAN, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.Aadhar:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var KarzaAadharDocType = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCActivityAadhar>(context.Message.JSONString);
                        IDocType<KarzaAadharDTO, KYCActivityAadhar> docTypeAadhar = kYCDocFactory.GetDocType<KarzaAadharDTO, KYCActivityAadhar>(context.Message.KYCMasterCode);
                        res = await docTypeAadhar.GetAndSaveByUniqueId(KarzaAadharDocType, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.GST:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var KYCActivityGST = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCActivityGST>(context.Message.JSONString);
                        IDocType<AppyFlowGSTDTO, KYCActivityGST> docTypeGst = kYCDocFactory.GetDocType<AppyFlowGSTDTO, KYCActivityGST>(context.Message.KYCMasterCode);
                        res = await docTypeGst.GetAndSaveByUniqueId(KYCActivityGST, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.PersonalDetail:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var KYCPersonalDetailActivity = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCPersonalDetailActivity>(context.Message.JSONString);
                        IDocType<PersonalDetailDTO, KYCPersonalDetailActivity> docTypePersonal = kYCDocFactory.GetDocType<PersonalDetailDTO, KYCPersonalDetailActivity>(context.Message.KYCMasterCode);
                        res = await docTypePersonal.GetAndSaveByUniqueId(KYCPersonalDetailActivity, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.BankStatement:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var BankDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCBankStatementActivity>(context.Message.JSONString);
                        IDocType<BankStatementDTO, KYCBankStatementActivity> docTypeBank = kYCDocFactory.GetDocType<BankStatementDTO, KYCBankStatementActivity>(context.Message.KYCMasterCode);
                        res = await docTypeBank.GetAndSaveByUniqueId(BankDetail, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.BuisnessDetail:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var BusinessDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<BusinessActivityDetail>(context.Message.JSONString);
                        IDocType<BusinessDetailDTO, BusinessActivityDetail> docTypeBusiness = kYCDocFactory.GetDocType<BusinessDetailDTO, BusinessActivityDetail>(context.Message.KYCMasterCode);
                        res = await docTypeBusiness.GetAndSaveByUniqueId(BusinessDetail, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.MSME:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var MSMEDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<MSMEActivity>(context.Message.JSONString);
                        IDocType<MSMEDTO, MSMEActivity> docTypeMSMe = kYCDocFactory.GetDocType<MSMEDTO, MSMEActivity>(context.Message.KYCMasterCode);
                        res = await docTypeMSMe.GetAndSaveByUniqueId(MSMEDetail, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.Selfie:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var selfieDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<SelfeiActivity>(context.Message.JSONString);
                        IDocType<SelfieDTO, SelfeiActivity> selfie = kYCDocFactory.GetDocType<SelfieDTO, SelfeiActivity>(context.Message.KYCMasterCode);
                        res = await selfie.GetAndSaveByUniqueId(selfieDetail, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.BankStatementCreditLending:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var BankStatementCreditLendingDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<BankStatementCreditLendingActivity>(context.Message.JSONString);
                        IDocType<BankStatementCreditLendingDTO, BankStatementCreditLendingActivity> BankStatementCreditLending = kYCDocFactory.GetDocType<BankStatementCreditLendingDTO, BankStatementCreditLendingActivity>(context.Message.KYCMasterCode);
                        res = await BankStatementCreditLending.GetAndSaveByUniqueId(BankStatementCreditLendingDetail, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.DSAProfileType:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var dsaProfileDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCDSAProfileTypeActivity>(context.Message.JSONString);
                        IDocType<DSAProfileTypeDTO, KYCDSAProfileTypeActivity> dsa = kYCDocFactory.GetDocType<DSAProfileTypeDTO, KYCDSAProfileTypeActivity>(context.Message.KYCMasterCode);
                        res = await dsa.GetAndSaveByUniqueId(dsaProfileDetail, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.DSAPersonalDetail:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var dSAPersonalDetailDTO = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCDSAPersonalDetailActivity>(context.Message.JSONString);
                        IDocType<DSAPersonalDetailDTO, KYCDSAPersonalDetailActivity> dSAPersonalDetail = kYCDocFactory.GetDocType<DSAPersonalDetailDTO, KYCDSAPersonalDetailActivity>(context.Message.KYCMasterCode);
                        res = await dSAPersonalDetail.GetAndSaveByUniqueId(dSAPersonalDetailDTO, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    case KYCMasterConstants.ConnectorPersonalDetail:
                        kYCDocFactory = new KYCDocFactoryConcrete(_context, context.Message.UserId, _basePath, _user.Value);
                        var connectorPersonalDetailDTO = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCConnectorPersonalDetailActivity>(context.Message.JSONString);
                        IDocType<ConnectorPersonalDetailDTO, KYCConnectorPersonalDetailActivity> connectorPersonalDetail = kYCDocFactory.GetDocType<ConnectorPersonalDetailDTO, KYCConnectorPersonalDetailActivity>(context.Message.KYCMasterCode);
                        res = await connectorPersonalDetail.GetAndSaveByUniqueId(connectorPersonalDetailDTO, context.Message.UserId, _user.Value, context.Message.ProductCode);
                        break;
                    default: break;
                }

                // do not check IsSuccess beacuse in case of IsSuccess false data is save but lead fails it
                if (res.Result > 0)
                {
                    KYCSuccessEvent kycSuccessEvent = new KYCSuccessEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        KycMasterId = res.Result,
                        SubActivityId = context.Message.SubActivityId,
                        LeadId = context.Message.LeadId,
                        ActivityId = context.Message.ActivityId,
                        IsSuccess = res.IsSuccess,
                        Message = res.Message,
                        ComapanyId = context.Message.ComapanyId,
                    };
                    await _massTransitService.Publish(kycSuccessEvent);



                    #region Make History
                    var result = await _kYCHistoryManager.GetKycHistroy(res.Result, context.Message.KYCMasterCode);
                    LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                    {
                        LeadId = context.Message.LeadId,
                        UserID = result.UserId,
                        UserName = "",
                        EventName = context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                        Narretion = result.Narretion,
                        NarretionHTML= result.NarretionHTML,
                        CreatedTimeStamp = result.CreatedTimeStamp 
                    };
                    await _massTransitService.Publish(histroyEvent);
                    #endregion


                }
                else
                {
                    KYCFailEvent kycFailEvent = new KYCFailEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        ErrorMessage = res.Message,
                        LeadId = context.Message.LeadId
                    };
                    await _massTransitService.Publish(kycFailEvent);
                }
            }

        }
    }
}
