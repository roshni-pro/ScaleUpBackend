using Grpc.Core;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadAPI.NBFCFactory.Implementation;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadModels.ArthMate;
using Serilog;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace ScaleUP.Services.LeadAPI.Controllers.NBFC
{
    [Route("[controller]")]
    [ApiController]
    public class ArthMateController : ControllerBase
    {
        private IHostEnvironment _hostingEnvironment;
        private readonly LeadApplicationDbContext _context;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly LeadCommonRequestResponseManager _leadCommonRequestResponseManager;
        private readonly ArthMateNBFCHelper _ArthMateNBFCHelper;
        private readonly LeadNBFCFactory _leadNBFCFactory;
        private readonly ArthMateNBFCSchedular _arthMateNBFCSchedular;
        ILogger _logger = Log.ForContext<ArthMateController>();

        private readonly ArthMateGrpcManager _ArthMateGrpcManager;

        public ArthMateController(LeadApplicationDbContext context, LeadNBFCSubActivityManager leadNBFCSubActivityManager, LeadCommonRequestResponseManager leadCommonRequestResponseManager
            , IHostEnvironment hostingEnvironment, LeadNBFCFactory leadNBFCFactory, ArthMateGrpcManager arthMateGrpcManager, ArthMateNBFCSchedular arthMateNBFCSchedular)
        {
            _context = context;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _leadCommonRequestResponseManager = leadCommonRequestResponseManager;
            _ArthMateNBFCHelper = new ArthMateNBFCHelper();
            _hostingEnvironment = hostingEnvironment;
            _leadNBFCFactory = leadNBFCFactory;
            _ArthMateGrpcManager = arthMateGrpcManager;
            _arthMateNBFCSchedular = arthMateNBFCSchedular;
        }

        [Route("AadhaarOtpGenerate")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CommonResponseDc> AadhaarOtpGenerate(long leadid)
        {
            var res = await _arthMateNBFCSchedular.AadhaarOtpGenerate(leadid);
            return res;
        }

        //[Route("AadhaarOtpVerify")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<CommonResponseDc> AadhaarOtpVerify(SecondAadharXMLDc AadharObj)
        //{
        //    var res = await _arthMateNBFCSchedular.AadhaarOtpVerify(AadharObj);
        //    _logger.Information("AadhaarOtpVerify :" + res.ToString());
        //    return res;
        //}
        //Show Offer
        [Route("GetLeadMasterByLeadId")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CommonResponseDc> GetLeadMasterByLeadId(long leadid)
        {
            var res = await _arthMateNBFCSchedular.GetLeadMasterByLeadId(leadid);
            return res;
        }

        [Route("GetRateOfInterest")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<LoanInsuranceConfiguration> GetRateOfInterest(int tenure)
        {
            var res = await _arthMateNBFCSchedular.GetRateOfInterest(tenure);
            return res;
        }

        [Route("OfferRegenerate")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CommonResponseDc> OfferRegenerate(int leadId, int tenure, double sactionAmount)
        {
            var res = await _arthMateNBFCSchedular.OfferRegenerate(leadId, tenure, sactionAmount);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetLoanByLoanId")]
        public async Task<CommonResponseDc> GetLoanByLoanId(long Leadmasterid)
        {
            var res = await _arthMateNBFCSchedular.GetLoanByLoanId(Leadmasterid);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDisbursement")]
        public async Task<CommonResponseDc> GetDisbursementAPI(long Leadmasterid)
        {
            var res = await _arthMateNBFCSchedular.GetDisbursementAPI(Leadmasterid);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetLoan")]
        public async Task<List<LeadLoanDataDc>> GetLoan(long LeadMasterId)
        {
            var res = await _arthMateNBFCSchedular.GetLoan(LeadMasterId);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("LoanRepaymentScheduleDetails")]
        public Task<CommonResponseDc> LoanRepaymentScheduleDetails(long LeadMasterId)
        {
            var res = _arthMateNBFCSchedular.LoanRepaymentScheduleDetails(LeadMasterId);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("LoanNach")]
        public Task<CommonResponseDc> LoanNach(string UMRN, long Leadmasterid)
        {
            var res = _arthMateNBFCSchedular.LoanNach(UMRN, Leadmasterid);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ChangeLoanStatus")]
        public Task<CommonResponseDc> ChangeLoanStatus(long LeadMasterId, string Status)
        {
            var res = _arthMateNBFCSchedular.ChangeLoanStatus(LeadMasterId, Status);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDisbursedLoanDetail")]
        public Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            var res = _arthMateNBFCSchedular.GetDisbursedLoanDetail(Leadmasterid);
            return res;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddSlaLbaStamp")]
        public async Task<GRPCReply<bool>> AddSlaLbaStamp(SlaLbaStampDc request)
        {
            var res = await _ArthMateGrpcManager.AddSlaLbaStamp(request);
            return res;
        }


        [HttpGet]
        [Route("SaveAgreementESignDocument")]
        public Task<CommonResponseDc> UploadSignedSla(long leadmasterid, string eSignDocumentURL)
        {
            var res = _arthMateNBFCSchedular.UploadSignedSla(leadmasterid, eSignDocumentURL);
            return res;
        }

        [HttpPost]
        [Route("LoanDataSave")]
        [AllowAnonymous]
        public async Task<CommonResponseDc> LoanDataSave(loandc obj)
        {
            var res = await _arthMateNBFCSchedular.LoanDataSave(obj);
            return res;
        }


        [HttpPost]
        [Route("UpdateBeneficiaryBankDetail")]
        public async Task<CommonResponseDc> UpdateBeneficiaryBankDetail(BeneficiaryBankDetailDc Obj)
        {
            var res = await _arthMateNBFCSchedular.UpdateBeneficiaryBankDetail(Obj);
            return res;
        }

        [HttpGet]
        [Route("GetOfferEmiDetails")]
        public Task<ResultViewModel<List<OfferEmiDetailDC>>> GetOfferEmiDetails(long leadId, int ReqTenure = 0)
        {
            var res = _arthMateNBFCSchedular.GetOfferEmiDetails(leadId, ReqTenure);
            return res;
        }
        //[HttpGet]
        //[Route("GetOfferEmiDetailsDownloadPdf")]
        //public Task<ResultViewModel<string>> GetOfferEmiDetailsDownloadPdf(long leadId)
        //{
        //    var res = _arthMateNBFCSchedular.GetOfferEmiDetailsDownloadPdf(leadId);
        //    return res;
        //}

        [HttpPost]
        [Route("InsertCeplrBanklist")]
        [AllowAnonymous]
        public async Task<CommonResponseDc> InsertCeplrBanklist()
        {
            var res = await _arthMateNBFCSchedular.InsertCeplrBanklist();
            return res;
        }
        [HttpPost]
        [Route("InsertLoanDataByArthmateTest")]
        [AllowAnonymous]
        public async Task<CommonResponseDc> InsertLoanDataByArthmateTest(long leadid, string loanid)
        {
            var res = await _arthMateNBFCSchedular.InsertLoanDataByArthmateTest(leadid, loanid);
            return res;
        }

        //[HttpPost]
        //[Route("CheckeSignDocumentStatus")]
        //public async Task<CommonResponseDc> eSignDocumentsAsync(long LeadId)
        //{
        //    var res = await _ArthMateGrpcManager.eSignDocumentsAsync(LeadId);
        //    return res;
        //}

    }
}
