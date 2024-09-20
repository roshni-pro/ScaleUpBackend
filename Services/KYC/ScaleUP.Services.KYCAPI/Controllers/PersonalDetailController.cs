using Microsoft.AspNetCore.Mvc;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.KYCAPI.KYCFactory;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Transacion;

namespace ScaleUP.Services.KYCAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PersonalDetailController : BaseController
    {
        private string _basePath;
        private readonly ApplicationDbContext _context;
        //private IDocType<PersonalDetailDTO, KYCPersonalDetailActivity> docType;
        //private IDocType<BusinessDetailDTO, BusinessActivityDetail> BusinessType;
        //private IDocType<MSMEDTO, MSMEActivity> MSMEType;

        //private readonly IPublisher _publisher;
        public PersonalDetailController(ApplicationDbContext context)
        {
            _context = context;
            //KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context);
            //IDocType<PersonalDetailDTO, KYCPersonalDetailActivity> docType = kYCDocFactory.GetDocType<PersonalDetailDTO, KYCPersonalDetailActivity>(Global.Infrastructure.Constants.KYCMasterConstants.PersonalDetail);
            //IDocType<BusinessDetailDTO, BusinessActivityDetail> BusinessType = kYCDocFactory.GetDocType<BusinessDetailDTO, BusinessActivityDetail>(Global.Infrastructure.Constants.KYCMasterConstants.BuisnessDetail);
            //IDocType<MSMEDTO, MSMEActivity> MSMEType = kYCDocFactory.GetDocType<MSMEDTO, MSMEActivity>(Global.Infrastructure.Constants.KYCMasterConstants.MSME);
            ////_publisher = publisher;
        }

        //[HttpPost]
        //[Route("SaveDoc")]
        //public async Task<ActionResult<long>> SaveDoc(PersonalDetailDTO command)
        //{

        //    long returnId = await docType.SaveDoc(command, "2");
        //    return returnId;


        //}
        [HttpPost]
        [Route("SaveDoc")]
        public async Task<ActionResult<long>> SaveDoc(BusinessDetailDTO command)
        {

            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", _basePath);
            IDocType<BusinessDetailDTO, BusinessActivityDetail> BusinessType = kYCDocFactory.GetDocType<BusinessDetailDTO, BusinessActivityDetail>(Global.Infrastructure.Constants.KYCMasterConstants.BuisnessDetail);
            ResultViewModel<long> returnId = await BusinessType.SaveDoc(command, "2", UserId, "");
            return returnId.Result;


        }
        [HttpGet]
        [Route("GetPersonalDetailDoc")]
        public async Task<dynamic> GetPersonalDetailDoc(string userId)
        {

            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, userId, _basePath);
            IDocType<PersonalDetailDTO, KYCPersonalDetailActivity> docType = kYCDocFactory.GetDocType<PersonalDetailDTO, KYCPersonalDetailActivity>(Global.Infrastructure.Constants.KYCMasterConstants.PersonalDetail);
            var docDetail = await docType.GetDoc(userId);

            return docDetail;
        }
        [HttpGet]
        [Route("GetBuisnessDoc")]
        public async Task<dynamic> GetDoc(string userId)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, userId, _basePath);
            IDocType<BusinessDetailDTO, BusinessActivityDetail> BusinessType = kYCDocFactory.GetDocType<BusinessDetailDTO, BusinessActivityDetail>(Global.Infrastructure.Constants.KYCMasterConstants.BuisnessDetail);
            var docDetail = await BusinessType.GetDoc(userId);

            return docDetail;
        }
        [HttpGet]
        [Route("GetMSMEDoc")]
        public async Task<dynamic> GetMSMEDoc(string userId)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, userId, _basePath);
            IDocType<MSMEDTO, MSMEActivity> MSMEType = kYCDocFactory.GetDocType<MSMEDTO, MSMEActivity>(Global.Infrastructure.Constants.KYCMasterConstants.MSME);

            var docDetail = await MSMEType.GetDoc(userId);

            return docDetail;
        }
        [HttpGet]
        [Route("GetPersonalDetails")]
        public async Task<dynamic> GetPersonalDetails(string userId)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, userId, _basePath);
            IDocType<PersonalDetailDTO, KYCPersonalDetailActivity> docType = kYCDocFactory.GetDocType<PersonalDetailDTO, KYCPersonalDetailActivity>(Global.Infrastructure.Constants.KYCMasterConstants.PersonalDetail);
            var docDetail = await docType.GetDoc(userId);

            return docDetail;
        }

        [HttpPost]
        [Route("GetAndSaveDoc")]
        public async Task<ActionResult<long>> GetAndSaveDoc(BusinessActivityDetail input)
        {
            
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, input.UserId, _basePath);
            IDocType<BusinessDetailDTO, BusinessActivityDetail> BusinessType = kYCDocFactory.GetDocType<BusinessDetailDTO, BusinessActivityDetail>(Global.Infrastructure.Constants.KYCMasterConstants.BuisnessDetail);

            var res = await BusinessType.GetAndSaveByUniqueId(input, input.UserId, UserId, "");
            return res.Result;


        }
    }
}
