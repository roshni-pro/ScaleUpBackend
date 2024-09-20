using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.KYCAPI.KYCFactory;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Enum;
using ScaleUP.Services.KYCDTO.Transacion;
using ScaleUP.Services.KYCModels.Master;
using static IdentityServer4.Models.IdentityResources;

namespace ScaleUP.Services.KYCAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MasterEntryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MasterEntryManager _MasterEntryManager;

        public MasterEntryController(IHostEnvironment hostingEnvironment, ApplicationDbContext context, MasterEntryManager MasterEntryManager)
        {
            _context = context;
            _MasterEntryManager = MasterEntryManager;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("SaveKYCMasterData")]
        public async Task SaveKYCMasterData()
        {
            _MasterEntryManager.EnterPANMaster();
            _MasterEntryManager.EnterAadharMaster();
            _MasterEntryManager.EnterSelfieMaster();
            _MasterEntryManager.EnterPersonalDetailMaster();
            _MasterEntryManager.EnterBuisnessDetailMaster();
            _MasterEntryManager.EnterMSMEMaster();
            _MasterEntryManager.EnterBankStatementCreditLendingMaster();
            _MasterEntryManager.EnterDSAPersonalDetailMaster();
            _MasterEntryManager.EnterConnectorPersonalDetailMaster();
            _MasterEntryManager.EnterDSAProfileTypeMaster();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveThirdApiConfigMasterData")]
        public async Task SaveThirdApiConfigMasterData()
        {
            _MasterEntryManager.EnterKarzaPANValidation();
            _MasterEntryManager.EnterKarzaPANOCRInfo();
            _MasterEntryManager.EnterAppyFlowGSTInfo();
            _MasterEntryManager.EnterKarzaAdhaarVerification();
            _MasterEntryManager.EnterKarzaAdhaarOtp();
            _MasterEntryManager.EnterKarzaPANProfile();
            _MasterEntryManager.KarzaElectricityBillAuthentication();
        }
    }
}
