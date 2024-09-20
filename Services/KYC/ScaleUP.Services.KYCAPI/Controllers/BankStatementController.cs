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
    public class BankStatementController : BaseController
    {
        private readonly string _basePath;
        private readonly ApplicationDbContext _context;
        //private IDocType<BankStatementDTO, KYCBankStatementActivity> docType;

        //private readonly IPublisher _publisher;
        public BankStatementController(ApplicationDbContext context)
        {
            _context = context;
            //KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context);
            //IDocType<BankStatementDTO, KYCBankStatementActivity> docType = kYCDocFactory.GetDocType<BankStatementDTO, KYCBankStatementActivity>(Global.Infrastructure.Constants.KYCMasterConstants.BankStatement);
            ////_publisher = publisher;
        }

        [HttpPost]
        [Route("SaveDoc")]
        public async Task<ActionResult<long>> SaveDoc(BankStatementDTO command)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", _basePath);
            IDocType<BankStatementDTO, KYCBankStatementActivity> docType = kYCDocFactory.GetDocType<BankStatementDTO, KYCBankStatementActivity>(Global.Infrastructure.Constants.KYCMasterConstants.BankStatement);


            ResultViewModel<long> returnId = await docType.SaveDoc(command, "2",UserId, "");
            return returnId.Result;


        }
    }
}
