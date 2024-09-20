using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.LoanAccountAPI.Common.Enum;
using ScaleUP.Services.LoanAccountAPI.Managers;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Services.LoanAccountDTO;
using ScaleUP.Services.LoanAccountDTO.Loan;
using ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using ScaleUP.Services.LoanAccountModels.Master;
using ScaleUP.Services.LoanAccountModels.Transaction;

namespace ScaleUP.Services.LoanAccountAPI.Controllers
{
    //[Route("api/LoanAccount")]
    [ApiController]
    public class LoanAccountController : BaseController
    {
        private readonly LoanAccountManager _LoanAccountManager;
        private readonly AccountTransactionManager _AccountTransactionManager;
        private readonly OrderPlacementManager _OrderPlacementManager;
        private readonly TransactionSettlementManager _TransactionSettlementManager;
        private readonly DelayPenalityOnDuePerDayJobManager _DelayPenalityOnDuePerDayJobManager;
        private readonly LoanNBFCFactory _loanNBFCFactory;
        public LoanAccountController(LoanAccountManager LoanAccountManager, AccountTransactionManager accountTransactionManager, OrderPlacementManager orderPlacementManager, TransactionSettlementManager transactionSettlementManager, DelayPenalityOnDuePerDayJobManager delayPenalityOnDuePerDayJobManager, LoanNBFCFactory loanNBFCFactory)
        {
            _LoanAccountManager = LoanAccountManager;
            _AccountTransactionManager = accountTransactionManager;
            _OrderPlacementManager = orderPlacementManager;
            _TransactionSettlementManager = transactionSettlementManager;
            _DelayPenalityOnDuePerDayJobManager = delayPenalityOnDuePerDayJobManager;
            _loanNBFCFactory = loanNBFCFactory;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetLoanAccountList")]
        public async Task<CommonResponse> GetLoanAccountList(LoanAccountListRequest request)
        {
            return await _LoanAccountManager.GetLoanAccountList(request);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetAccountTransaction")]
        public async Task<CommonResponse> GetAccountTransaction(TransactionFilterDTO request)
        {
            return await _AccountTransactionManager.GetAccountTransaction(request);
        }

        //[HttpPost]
        //[Route("OrderInitiate")]
        //public async Task<string> OrderInitiate(GRPCRequest<PaymentRequestdc> request)
        //{
        //    return await _OrderPlacementManager.PaymentInitiate(request);
        //}

        //[HttpPost]
        //[Route("SendTransactionOtpAsync")]
        //public async Task<string> SendTransactionOtpAsync(GRPCRequest<SendTransactionOtp> request)
        //{
        //    return await _OrderPlacementManager.SendTransactionOtpAsync(request);
        //}

        //[HttpPost]
        //[Route("ConfirmTransactionOtpAsync")]
        //public async Task<GRPCReply<long>> ConfirmTransactionOtpAsync(GRPCRequest<OrderPlacementRequestDC> request)
        //{
        //    return await _OrderPlacementManager.PostOrderPlacement(request);
        //}
        [HttpGet]
        [Route("GetTransactionDetailById")]
        public async Task<CommonResponse> GetTransactionDetailById(long AccountTransactionId)
        {
            return await _AccountTransactionManager.GetTransactionDetailById(AccountTransactionId);
        }

        [HttpGet]
        [Route("TransactionDetail")]
        public async Task<CommonResponse> TransactionDetail(long AccountTransactionsID)
        {
            return await _AccountTransactionManager.TransactionDetail(AccountTransactionsID);
        }
        [HttpGet]
        [Route("GetPenaltyBounceCharges")]
        public async Task<CommonResponse> GetPenaltyBounceCharges(string ReferenceId, string Penaltytype)
        {
            return await _AccountTransactionManager.GetPenaltyBounceCharges(ReferenceId, Penaltytype);
        }

        [HttpGet]
        [Route("GetCityNameList")]
        public async Task<CommonResponse> GetCityNameList()
        {
            return await _AccountTransactionManager.GetCityNameList();
        }


        [HttpGet]
        [Route("GetAnchorNameList")]
        public async Task<CommonResponse> GetAnchorNameList()
        {
            return await _AccountTransactionManager.GetAnchorNameList();
        }

        [HttpPost]
        [Route("WaiveOffPenaltyBounce")]
        public async Task<WaiveOffPenaltyBounceReplytDTO> WaiveOffPenaltyBounce(WaiveOffPenaltyBounceRequestDTO waiveOffPenaltyBounceRequestDTO)
        {
            return await _TransactionSettlementManager.WaiveOffPenaltyBounce(waiveOffPenaltyBounceRequestDTO);
        }

        [HttpPost]
        [Route("TransactionSettleByManual")]
        public async Task<TransactionSettlementByManualReplytDTO> TransactionSettleByManual(TransactionSettlementByManualDTO TransactionSettleByManualDc)
        {
            return await _TransactionSettlementManager.TransactionSettleByManual(TransactionSettleByManualDc);
        }

        [HttpGet]
        [Route("AnchorCityProductList")]
        public async Task<CommonResponse> AnchorCityProductList()
        {
            List<long>? ids = null;
            if (UserType.ToLower() != UserTypeConstants.SuperAdmin)
            {
                ids = new List<long>(CompanyIds);
            }
            return await _LoanAccountManager.AnchorCityProductList(ids);

        }
        [HttpGet]
        [Route("CustomerBlock")]
        public async Task<CommonResponse> CustomerBlock(long LoanAccountId, string Comment, bool IsHideLimit, string username)
        {
            return await _AccountTransactionManager.CustomerBlock(LoanAccountId, Comment, IsHideLimit, username);
        }


        [HttpGet]
        [Route("BounceChargeInsert")]
        public async Task<CommonResponse> BounceChargeInsert(string TransactionNumber, string CreatedBy)
        {
            return await _TransactionSettlementManager.BounceChargeInsert(TransactionNumber, CreatedBy);
        }

        [HttpGet]
        [Route("TransactionDetailHistory")]
        public async Task<List<TransactionDetailHistoryDc>> TransactionDetailHistory(string TransactionId)
        {
            return await _TransactionSettlementManager.TransactionDetailHistory(TransactionId);
        }

        [HttpGet]
        [Route("CustomerActiveInActive")]
        public async Task<CommonResponse> CustomerActiveInActive(long LoanAccountId, bool AccountActiveInActive)
        {
            return await _AccountTransactionManager.CustomerActiveInActive(LoanAccountId, AccountActiveInActive);
        }

        #region Transaction Export

        [AllowAnonymous]
        [HttpPost]
        [Route("GetAccountTransactionExport")]
        public async Task<CommonResponse> GetAccountTransactionExport(TransactionFilterDTO obj)
        {
            return await _AccountTransactionManager.GetAccountTransactionExport(obj);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetLoanAccountListExport")]
        public async Task<CommonResponse> GetLoanAccountListExport(LoanAccountListRequest obj)
        {
            return await _AccountTransactionManager.GetLoanAccountListExport(obj);
        }
        #endregion

        //[HttpPost]
        //[Route("UpdateInvoiceInformation")]
        //public async Task<InvoiceResponseDC> UpdateInvoiceInformation(InvoiceRequestDTO request)
        //{
        //    return await _OrderPlacementManager.UpdateInvoiceInformation(request);
        //}

        //[HttpGet]
        //[Route("AnchorOrderCompleted")]
        //public async Task<CommonResponse> AnchorOrderCompleted(string transactionNo, bool transStatus)
        //{
        //    return await _OrderPlacementManager.AnchorOrderCompleted(transactionNo, transStatus);
        //}


        [HttpGet]
        [Route("RefundTransaction")]
        public async Task<CommonResponse> RefundTransaction(string orderNo, double refundAmount)
        {
            return await _OrderPlacementManager.RefundTransactionold(orderNo, refundAmount);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SaveModifyTemplateMaster")]
        public async Task<TemplateResponseDc> SaveModifyTemplateMaster(TemplateDc templatedc)
        {
            return await _LoanAccountManager.SaveModifyTemplateMaster(templatedc);
        }


        [HttpGet]
        [Route("PostLoanAccountToAnchor")]
        public async Task<CommonResponse> PostLoanAccountToAnchor(long AccountId)
        {
            return await _LoanAccountManager.PostLoanAccountToAnchor(AccountId);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetCustomerTransactionList")]
        public async Task<List<CustomerInvoiceDTO>> GetCustomerTransactionList(CustomerTransactionInput customerTransactionInput)
        {
            return await _AccountTransactionManager.GetCustomerTransactionList(customerTransactionInput);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetCustomerOrderSummary")]
        public async Task<CustomerOrderSummaryDTO> GetCustomerOrderSummary(long LeadId)
        {
            long AnchorCompanyId = 0;
            return await _AccountTransactionManager.GetCustomerOrderSummary(AnchorCompanyId, LeadId);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetCustomerOrderSummaryForAnchor")]
        public async Task<CustomerOrderSummaryDTO> GetCustomerOrderSummaryForAnchor(long LeadId, long AnchorCompanyId, int UnPaidOnePaidTwo)
        {
            string TransactioType = "";
            if (UnPaidOnePaidTwo == 1)
            {
                TransactioType = "Unpaid";
            }
            else
            {
                TransactioType = "Paid";
            }
            return await _AccountTransactionManager.GetCustomerOrderSummary(AnchorCompanyId, LeadId, TransactioType);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetLoanAccountDetail")]
        public async Task<LoanAccountDetailResponse> GetLoanAccountDetail(long loanAccountId)
        {
            return await _LoanAccountManager.GetLoanAccountDetail(loanAccountId);
        }

        [HttpPost]
        [Route("GetInvoiceList")]
        [AllowAnonymous]
        public async Task<InvoiceListDTO> GetInvoiceList(InvoiceRequest invoiceRequest)
        {
            return await _OrderPlacementManager.GetInvoiceList(invoiceRequest);

        }

        [HttpGet]
        [Route("PostNBFCInvoice")]
        [AllowAnonymous]
        public async Task<CommonResponse> PostNBFCInvoice(long invoiceId, long loanAccountId)
        {
            return await _OrderPlacementManager.PostNBFCInvoice(invoiceId, loanAccountId);
        }

        [HttpGet]
        [Route("GetInvoiceRequestResponse")]
        [AllowAnonymous]
        public async Task<List<InvoiceNBFCReqRes>> GetInvoiceRequestResponse(long invoiceId, long loanAccountId)
        {
            return await _OrderPlacementManager.GetInvoiceRequestResponse(invoiceId, loanAccountId);
        }


        [HttpGet]
        [Route("BlockUnblockAccount")]
        public async Task<CommonResponse> BlockUnblockAccount(long loanAccountId, bool isBlock, string comment)
        {
            return await _LoanAccountManager.BlockUnblockAccount(loanAccountId, isBlock, comment);
        }

        [HttpGet]
        [Route("ActiveInActiveAccount")]
        public async Task<CommonResponse> ActiveInActiveAccount(long loanAccountId, bool IsAccountActive)
        {
            return await _LoanAccountManager.ActiveInActiveAccount(loanAccountId, IsAccountActive);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateTransactionStatusJob")]
        public async Task<bool> UpdateTransactionStatusJob()
        {
            var res = await _DelayPenalityOnDuePerDayJobManager.UpdateTransactionStatusJob();
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("OverDueInterestCharge")]
        public async Task<bool> OverDueInterestCharge(DateTime? CalculationDate = null, string? TransactionNo = null, long? LoanAccountId = null)
        {
            return await _DelayPenalityOnDuePerDayJobManager.OverDueInterestCharge(CalculationDate, TransactionNo, LoanAccountId);
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("InsertDailyPenaltyCharges")]
        public async Task<bool> InsertDailyPenaltyCharges(DateTime? CalculationDate = null, string? TransactionNo = null, long? LoanAccountId = null)
        {
            return await _DelayPenalityOnDuePerDayJobManager.InsertDailyPenaltyCharges(CalculationDate, TransactionNo, LoanAccountId);
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("SettlePaymentLater")]
        public async Task<bool> SettlePaymentLater()
        {
            var service = _loanNBFCFactory.GetService(CompanyIdentificationCodeConstants.BlackSoil);

            await service.SettlePaymentLater(new GRPCRequest<SettlePaymentJobRequest>());
            return true;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetTransactionBreakup")]
        public async Task<TransactionBreakupDc> GetTransactionBreakup(long InvoiceId)
        {
            return await _AccountTransactionManager.GetTransactionBreakup(InvoiceId);
        }

        [HttpGet]
        [Route("NotifyAnchorOrderCanceled")]
        public async Task<CommonResponse> NotifyAnchorOrderCanceled(long AccountId)
        {
            return await _LoanAccountManager.NotifyAnchorOrderCanceled(AccountId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ClearInitiateLimit")]
        public async Task<CommonResponse> ClearInitiateLimit(long LeadAccountId, long AccountTransactionId)
        {
            return await _LoanAccountManager.ClearInitiateLimit(LeadAccountId, AccountTransactionId);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetCustomerTransactionListTwo")]
        public async Task<List<CustomerInvoiceDTO>> GetCustomerTransactionListTwo(CustomerTransactionTwoInput customerTransactionTwoInput)
        {
            return await _AccountTransactionManager.GetCustomerTransactionListTwo(customerTransactionTwoInput);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetInvoiceDetail")]
        public async Task<CommonResponse> GetInvoiceDetail(TransactionFilterDTO request)
        {
            return await _AccountTransactionManager.GetInvoiceDetail(request);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("GetInvoiceDetailExport")]
        public async Task<CommonResponse> GetInvoiceDetailExport(TransactionFilterDTO request)
        {
            return await _AccountTransactionManager.GetInvoiceDetailExport(request);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("TransactionDetailByInvoiceId")]
        public async Task<CommonResponse> TransactionDetailByInvoiceId(long InvoiceId, string HeadType)
        {
            return await _AccountTransactionManager.TransactionDetailByInvoiceId(InvoiceId, HeadType);
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("InvoiceDetail")]
        public async Task<CommonResponse> InvoiceDetail(long InvoiceId)
        {
            return await _AccountTransactionManager.InvoiceDetail(InvoiceId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("InsertSkippedRePaymentsList")]
        public async Task<bool> InsertSkippedRePaymentsList(long loanAccountId = 0)
        {
            var service = _loanNBFCFactory.GetService(CompanyIdentificationCodeConstants.BlackSoil);

            await service.InsertSkippedPayments(loanAccountId);
            return true;
        }

        [HttpGet]
        [Route("DisbursementReprocess")]
        [AllowAnonymous]
        public async Task<bool> DisbursementReprocess(long loanAccountId = 0, long invoiceId = 0)
        {
            var result = await _LoanAccountManager.DisbursementReprocess(loanAccountId, invoiceId);
            return true;
        }

        [HttpGet]
        [Route("DiscountOverdueInterestAmtDelayPenaltyAmt")]
        public async Task<CommonResponse> DiscountOverdueInterestAmtDelayPenaltyAmt(string InvoiceNo, double DiscountAmount, string DiscountTransactionType)
        {
            return await _TransactionSettlementManager.DiscountOverdueInterestAmtDelayPenaltyAmt(InvoiceNo, DiscountAmount, DiscountTransactionType);
        }

        [HttpGet]
        [Route("GetCompanyInvoiceStatusList")]
        public async Task<ResultViewModel<List<CompanyInvoiceStatusDc>>> GetCompanyInvoiceStatusList()
        {
            ResultViewModel<List<CompanyInvoiceStatusDc>> result = new ResultViewModel<List<CompanyInvoiceStatusDc>>
            {
                IsSuccess = true,
                Message = "Data Found",
                Result = new List<CompanyInvoiceStatusDc>
                {
                    new CompanyInvoiceStatusDc{Name=CompanyInvoiceStatusEnum.All.ToString(),Value=Convert.ToInt32(CompanyInvoiceStatusEnum.All)},
                    new CompanyInvoiceStatusDc{Name=CompanyInvoiceStatusEnum.Inprocess.ToString(),Value=Convert.ToInt32(CompanyInvoiceStatusEnum.Inprocess)},
                    new CompanyInvoiceStatusDc{Name=CompanyInvoiceStatusEnum.MakerApproved.ToString(),Value=Convert.ToInt32(CompanyInvoiceStatusEnum.MakerApproved)},
                    new CompanyInvoiceStatusDc{Name=CompanyInvoiceStatusEnum.CheckerApproved.ToString(),Value=Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerApproved)},
                    new CompanyInvoiceStatusDc{Name=CompanyInvoiceStatusEnum.MakerReject.ToString(),Value=Convert.ToInt32(CompanyInvoiceStatusEnum.MakerReject)},
                    new CompanyInvoiceStatusDc{Name=CompanyInvoiceStatusEnum.CheckerReject.ToString(),Value=Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerReject)}
                }
            };
            return result;
        }


        [HttpGet]
        [Route("GetCompanyInvoiceDetailsByType")]
        public async Task<ResultViewModel<List<CompanyInvoiceDetailsByTypeDc>>> GetCompanyInvoiceDetailsByType(long AccountTransactionId)
        {
            var result = await _LoanAccountManager.GetCompanyInvoiceDetailsByType(AccountTransactionId);
            return result;
        }
        #region Arthmate

        [HttpGet]
        [Route("LoanRepaymentScheduleDetails")]
        public async Task<ResultViewModel<List<LoanRepaymentScheduleDetailDc>>> LoanRepaymentScheduleDetails(long loanAccountId)
        {
            ResultViewModel<List<LoanRepaymentScheduleDetailDc>> res = new ResultViewModel<List<LoanRepaymentScheduleDetailDc>>();
            var service = _loanNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (service != null)
            {
                res = await service.LoanRepaymentScheduleDetails(loanAccountId);
            }
            return res;
        }

        [HttpGet]
        [Route("GetBusinessLoanDetails")]
        public async Task<ResultViewModel<BusinessLoanDetailDc>> GetBusinessLoanDetails(long loanAccountId)
        {
            var res = await _LoanAccountManager.GetBusinessLoanDetails(loanAccountId);
            return res;
        }
        #endregion

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDisbursedLoanDetail")]
        public async Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            RePaymentScheduleDataDc res = new RePaymentScheduleDataDc();
            var service = _loanNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (service != null)
            {
                res = await service.GetDisbursedLoanDetail(Leadmasterid);
            }
            return res;
        }

        [HttpGet]
        [Route("GetLoanDetail")]
        [AllowAnonymous]
        public async Task<CommonResponse> getloandetail(long leadid)
        {
            var res = await _LoanAccountManager.GetLoanDetail(leadid);
            return res;
        }


        [HttpPost]
        [Route("GetOverdueLoanAccountList")]
        [AllowAnonymous]
        public async Task<List<GetOverdueLoanAccountResponse>> GetOverdueLoanAccountList(List<GetOverdueLoanAccountIdRequest> loanAccointIdRequest)
        {
            List<GetOverdueLoanAccountResponse> reply = new List<GetOverdueLoanAccountResponse>();

            reply = await _LoanAccountManager.GetOverdueLoanAccount(loanAccointIdRequest);
            return reply;
        }


        [HttpGet]
        [Route("CompanyInvoiceSettlement")]
        [AllowAnonymous]
        public async Task<GRPCReply<List<CompanyInvoiceSettlement>>> CompanyInvoiceSettlement(long CompanyInvoiceId)
        {
            var reply = await _LoanAccountManager.CompanyInvoiceSettlement(CompanyInvoiceId);
            return reply;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ClearInitiateLimitByReferenceId")]
        public async Task<CommonResponse> ClearInitiateLimitByReferenceId(long LeadAccountId, string ReferenceId)
        {
            return await _LoanAccountManager.ClearInitiateLimitByReferenceId(LeadAccountId, ReferenceId);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddRepaymentAccountDetails")]
        public async Task<CommonResponse> AddRepaymentAccountDetails(LoanRepaymentAccountDetailRequestDC loanRepaymentAccountDetailRequestDC)
        {
            return await _LoanAccountManager.AddRepaymentAccountDetails(loanRepaymentAccountDetailRequestDC, UserId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetRepaymentAccountDetails")]
        public async Task<CommonResponse> GetRepaymentAccountDetails(long LeadId)
        {
            return await _LoanAccountManager.GetRepaymentAccountDetails(LeadId);
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("GetRepaymentAccountDetailsForAdmin")]
        public async Task<CommonResponse> GetRepaymentAccountDetailsForAdmin(long LoanAccountId)
        {
            return await _LoanAccountManager.GetRepaymentAccountDetailsForAdmin(LoanAccountId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("InsertTopupNumber_Utility")]
        public async Task<bool> InsertTopupNumber(long loanAccountId)
        {
            return await _LoanAccountManager.InsertTopupNumber(loanAccountId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ValidateLoanPayments_Utility")]
        public async Task<LoanPaymentsResultDC> ValidateLoanPayments(long loanAccountId)
        {
            return await _LoanAccountManager.validateLoanPayments(loanAccountId);

        }
        //[AllowAnonymous]
        //[HttpGet]
        //[Route("GetLedger_RetailerStatement")]
        //public async Task<LoanPaymentsResultDC> GetLedger_RetailerStatement(long loanAccountId, DateTime FromDate, DateTime ToDate)
        //{
        //    return await _LoanAccountManager.GetLedger_RetailerStatement( loanAccountId,  FromDate,  ToDate);

        //}

        [AllowAnonymous]
        [HttpGet]
        [Route("UploadBLInvoiceExcel")]
        public async Task<GRPCReply<bool>> UploadBLInvoiceExcel(UploadBLInvoiceExcelReq request)
        {
            var res = await _LoanAccountManager.UploadBLInvoiceExcel(request);
            return res;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetBLPaymentUploads")]
        public async Task<GRPCReply<List<GetBLPaymentUploadsDTO>>> GetBLPaymentUploads(int Skip, int Take)
        {
            var res = await _LoanAccountManager.GetBLPaymentUploads(Skip,Take);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("EMIPenaltyPerDayTransactionsJob")]
        public async Task<bool> EMIPenaltyJob(DateTime? CalculationDate = null, string? TransactionNo = null, long? LoanAccountId = null)
        {
            return await _DelayPenalityOnDuePerDayJobManager.EMIPenaltyJob(CalculationDate, TransactionNo, LoanAccountId);
        }

    }

}
