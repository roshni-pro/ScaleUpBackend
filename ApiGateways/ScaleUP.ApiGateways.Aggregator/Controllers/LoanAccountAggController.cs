using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using MassTransit;
using System.Collections.Generic;
using IdentityServer4.Models;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.ApiGateways.Aggregator.Managers.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;


namespace ScaleUP.ApiGateways.Aggregator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoanAccountAggController : BaseController
    {
        private LoanAccountManager _loanAccountManager;

        public LoanAccountAggController(LoanAccountManager loanAccountManager)
        {
            _loanAccountManager = loanAccountManager;
        }

        [HttpGet]
        [Route("GetByTransactionReqNo")]
        public async Task<GRPCReply<PaymentResponseDc>> GetByTransactionReqNo(string TransactionReqNo)
        {
            var result = await _loanAccountManager.GetByTransactionReqNo(TransactionReqNo);
            return result;
        }

        [HttpGet]
        [Route("getToten")]
        [AllowAnonymous]
        public async Task<string> getToten(string mobileNo)
        {
            var result = await _loanAccountManager.getToten(mobileNo);
            return result;
        }

        [HttpGet]
        [Route("GetAnchoreCompanyDetail")]
        public async Task<GetCompanyProductConfigReply> GetAnchoreCompanyDetail(long companyId, long productId)
        {
            return await _loanAccountManager.GetAnchoreCompanyDetail(companyId, productId);
        }

        [HttpGet]
        [Route("ValidateOrderOTPGetToken")]
        [AllowAnonymous]
        public async Task<GRPCReply<OrderToken>> ValidateOrderOTPGetToken(string TransactionReqNo, string otp)
        {
            var result = await _loanAccountManager.ValidateOrderOTPGetToken(TransactionReqNo, otp);
            return result;
        }
        [HttpGet]
        [Route("ResentOrderOTP")]
        [AllowAnonymous]
        public async Task<bool> ResentOrderOTP(string MobileNo, string TransactionNo)
        {
            return await _loanAccountManager.ResentOrderOTP(MobileNo, TransactionNo);
        }
        [HttpGet]
        [Route("GetByTransactionReqNoForOTP")]
        [AllowAnonymous]
        public async Task<GRPCReply<OrderOTPScreenResponse>> GetByTransactionReqNoForOTP(string TransactionReqNo)
        {
            GRPCRequest<string> request = new GRPCRequest<string>();
            request.Request = TransactionReqNo;
            var result = await _loanAccountManager.GetByTransactionReqNoForOTP(request);
            return result;
        }

        // Confirm order by otp    
        [HttpPost]
        [Route("PostOrderPlacement")]
        public async Task<GRPCReply<string>> PostOrderPlacement(OrderPlacementRequestDTO order)
        {
            //return new GRPCReply<string> { Status = false, Message = "", Response = "" };
            var result = await _loanAccountManager.PostOrderPlacement(order);
            return result;
        }

        [HttpGet]
        [Route("GetAvailableCreditLimitByLeadId")]
        public async Task<GRPCReply<LoanCreditLimit>> GetAvailableCreditLimitByLeadId(long LoanAccountId)
        {
            GRPCRequest<long> request = new GRPCRequest<long>() { Request = LoanAccountId };
            return await _loanAccountManager.GetAvailableCreditLimitByLeadId(request);
        }
        [HttpPost]
        [Route("OrderInitiate")]
        public async Task<GRPCReply<string>> OrderInitiate(PaymentRequestdc obj)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            var result = await _loanAccountManager.OrderInitiate(obj);
            if (result.Status)
            {
                gRPCReply.Response = result.Response.RedirectUrl;
                gRPCReply.Message = result.Response.OtptxnNo;
                gRPCReply.Status = true;
            }
            else
            {
                gRPCReply.Message = result.Message; //"Invalid request, Please corrrect and resend again";
            }
            return gRPCReply;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("SendOverdueMessageJob")]
        public async Task<bool> SendOverdueMessageJob()
        {
            return await _loanAccountManager.SendOverdueMessageJob();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SendDueDisbursmentMessageJob")]
        public async Task<bool> SendDueDisbursmentMessageJob()
        {
            return await _loanAccountManager.SendDueDisbursmentMessageJob();
        }

        [HttpGet]
        [Route("AnchorOrderCompleted")]
        public async Task<GRPCReply<string>> AnchorOrderCompleted(string transactionNo, bool transStatus)
        {
            OrderCompletedRequest orderCompletedRequest = new OrderCompletedRequest { transactionNo = transactionNo, transStatus = transStatus };
            return await _loanAccountManager.AnchorOrderCompleted(orderCompletedRequest);
        }

        [HttpPost]
        [Route("UpdateInvoiceInformation")]
        public async Task<LoanInvoiceReply> UpdateInvoiceInformation(InvoiceRequestDC request)
        {
            return await _loanAccountManager.UpdateInvoiceInformation(request);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateTransactionStatusJob")]
        public async Task<bool> UpdateTransactionStatusJob()
        {
            var res = await _loanAccountManager.UpdateTransactionStatusJob();
            return res;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetAnchorMISList")]
        public async Task<GRPCReply<List<AnchorMISListResponseDC>>> GetAnchorMISList(AnchorMISRequestDC obj)
        {
            var res = await _loanAccountManager.GetAnchorMISList(obj);
            return res;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetNbfcMISList")]
        public async Task<GRPCReply<List<NbfcMisListResponseDc>>> GetNbfcMISList(NbfcMisListRequestDc obj)
        {
            var res = await _loanAccountManager.GetNbfcMISList(obj);
            return res;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("GetDSAMISList")]
        public async Task<GRPCReply<List<DSAMISListResponseDC>>> GetDSAMISList(DSAMISRequestDC request)
        {
            var res = await _loanAccountManager.GetDSAMISList(request);
            return res;
        }

        [HttpPost]
        [Route("GetBusinessLoanAccountList")]
        public async Task<GRPCReply<List<LoanAccountListResponseDc>>> GetBusinessLoanAccountList(LoanAccountListRequestDc obj)
        {
            obj.UserType = UserType;
            var res = await _loanAccountManager.GetBusinessLoanAccountList(obj);
            return res;
        }

        #region Product Configuration

        [HttpPost]
        [Route("AddUpdateAnchorProductConfig")]
        public async Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request)
        {
            var res = await _loanAccountManager.AddUpdateAnchorProductConfig(request);
            return res;
        }

        [HttpPost]
        [Route("AddUpdateNBFCProductConfig")]
        public async Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request)
        {
            var res = await _loanAccountManager.AddUpdateNBFCProductConfig(request);
            return res;
        }
        #endregion


        [HttpPost]
        [Route("GetCompanyInvoiceCharges")]
        public async Task<GRPCReply<List<CompanyInvoicesChargesResponseDc>>> GetCompanyInvoiceCharges(CompanyInvoiceChargesRequestDc request)
        {
            GRPCReply<List<CompanyInvoicesChargesResponseDc>> reply = new GRPCReply<List<CompanyInvoicesChargesResponseDc>>();
            if (UserRoles != null && (
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceExecutive)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceLead)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.CompanyAdmin)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SuperAdmin)))
                )
            {
                reply = await _loanAccountManager.GetCompanyInvoiceCharges(request);
            }
            else
            {
                reply.Status = false;
                reply.Message = "You are not authorize to access this functionality.";
            }

            return reply;
        }


        [HttpPost]
        [Route("GetCompanyInvoiceList")]
        public async Task<GRPCReply<List<CompanyInvoicesListResponseDc>>> GetCompanyInvoiceList(CompanyInvoiceRequestDc request)
        {
            GRPCReply<List<CompanyInvoicesListResponseDc>> reply = new GRPCReply<List<CompanyInvoicesListResponseDc>>();
            if (UserRoles != null && (
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceExecutive)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceLead)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.CompanyAdmin)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SuperAdmin)))
                )
            {
                string userType = "";
                if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceLead)))
                    userType = CompanyInvoiceUserTypeConstants.CheckerUser;
                else if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceExecutive)))
                    userType = CompanyInvoiceUserTypeConstants.MakerUser;
                reply = await _loanAccountManager.GetCompanyInvoiceList(request);
                if (reply != null && reply.Response != null && reply.Response.Any())
                {
                    reply.Response.ForEach(x => x.UserType = userType);
                }
            }
            else
            {
                reply.Status = false;
                reply.Message = "You are not authorize to access this functionality.";
            }

            return reply;
        }


        [HttpPost]
        [Route("GetCompanyInvoiceDetails")]
        public async Task<GRPCReply<List<CompanyInvoiceDetailsResponseDc>>> GetCompanyInvoiceDetails(CompanyInvoiceDetailsRequestDC request)
        {
            GRPCReply<List<CompanyInvoiceDetailsResponseDc>> reply = new GRPCReply<List<CompanyInvoiceDetailsResponseDc>>();
            if (UserRoles != null && (
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceExecutive)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceLead)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.CompanyAdmin)) ||
                UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SuperAdmin)))
                )
            {
                if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceLead)))
                    request.RoleName = CompanyInvoiceUserTypeConstants.CheckerUser;
                else if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceExecutive)))
                    request.RoleName = CompanyInvoiceUserTypeConstants.MakerUser;
                else if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.CompanyAdmin)) ||
                         UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SuperAdmin)))
                    request.RoleName = CompanyInvoiceUserTypeConstants.CheckerUser;

                reply = await _loanAccountManager.GetCompanyInvoiceDetails(request);
                if (request.RoleName == CompanyInvoiceUserTypeConstants.MakerUser && reply != null && reply.Response != null && reply.Response.Any())
                    reply.Response.ForEach(x => x.IsCheckboxVisible = true);
            }
            else
            {
                reply.Status = false;
                reply.Message = "You are not authorize to access this functionality.";
            }
            return reply;
        }

        [HttpPost]
        [Route("UpdateCompanyInvoiceDetails")]
        public async Task<GRPCReply<string>> UpdateCompanyInvoiceDetails(GRPCRequest<UpdateCompanyInvoiceRequestDC> request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            if (!(UserRoles != null && (
               UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceExecutive)) ||
               UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceLead))
               )))
            {
                reply.Status = false;
                reply.Message = "You are not authorize to perform this action.";
                return reply;
            }


            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceLead)))
                request.Request.UserType = CompanyInvoiceUserTypeConstants.CheckerUser;
            else if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.FinanceExecutive)))
                request.Request.UserType = CompanyInvoiceUserTypeConstants.MakerUser;

            request.Request.UserId = UserId;
            var res = await _loanAccountManager.UpdateCompanyInvoiceDetails(request);

            reply.Status = res.Status;
            reply.Message = res.Message;
            return reply;
        }

        [HttpPost]
        [Route("SendInvoiceEmail")]
        public async Task<GRPCReply<string>> SendInvoiceEmail(SendInvoiceEmailDC request)
        {
            var res = await _loanAccountManager.SendInvoiceEmail(request);
            return res;
        }


        [HttpPost]
        [Route("GetInvoiceRegisterData")]
        public async Task<GRPCReply<List<GetInvoiceRegisterDataResponse>>> GetInvoiceRegisterData(GetInvoiceRegisterDataRequest request)
        {
            var reply = await _loanAccountManager.GetInvoiceRegisterData(request);
            return reply;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SalesAgentLoanDisbursmentJob")]
        public async Task<bool> SalesAgentLoanDisbursmentJob()
        {
            return await _loanAccountManager.SalesAgentLoanDisbursmentJob();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("EmailAnchorMISDataJob")]
        public async Task<bool> EmailAnchorMISDataJob()
        {
            var res = await _loanAccountManager.EmailAnchorMISDataJob();
            return res;
        }

        [HttpGet]
        [Route("BLEMIDownloadPdf")]
        public async Task<GRPCReply<string>> BLEMIDownloadPdf(long leadId)
        {
            GRPCRequest<long> request = new GRPCRequest<long> { Request = leadId };
            var reply = await _loanAccountManager.BLoanEMIPdf(request);
            return reply;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ValidateLoginPinGetToken")]
        public async Task<GRPCReply<OrderToken>> ValidateLoginPinGetToken(string mobileNo, string pinNumber)
        {
            var result = await _loanAccountManager.ValidateLoginPinGetToken(mobileNo, pinNumber);
            return result;
        }

        [HttpPost]
        [Route("ValidateOrderOtp")]
        public async Task<GRPCReply<string>> ValidateOrderOtp(PostOrderPlacementRequestDTO order)
        {
            var result = await _loanAccountManager.ValidateOrderOtp(order);
            return result;
        }

        [HttpGet]
        [Route("ResentOrderLoginOTP")]
        [AllowAnonymous]
        public async Task<bool> ResentOrderLoginOTP(string TransactionReqNo)
        {
            return await _loanAccountManager.ResentOrderLoginOTP(TransactionReqNo);
        }
        [HttpPost]
        [Route("SettleCompanyInvoiceTransactions")]
        public async Task<GRPCReply<bool>> SettleCompanyInvoiceTransactions(SettleCompanyInvoiceTransactionsRequest request)
        {
            var reply = await _loanAccountManager.SettleCompanyInvoiceTransactions(request);
            return reply;
        }

        [HttpGet]
        [Route("SettlePaymentLaterJob")]
        [AllowAnonymous]
        public async Task<bool> SettlePaymentLaterJob()
        {
            var reply = await _loanAccountManager.SettlePaymentLaterJob();
            return reply;
        }

        [HttpGet]
        [Route("InsertScaleUpShareTransactions")]
        [AllowAnonymous]
        public async Task<bool> InsertScaleUpShareTransactions()
        {
            var reply = await _loanAccountManager.InsertScaleUpShareTransactions();
            return reply;
        }

        [HttpPost]
        [Route("GetNBFCBusinessLoanAccountList")]
        public async Task<GRPCReply<List<LoanAccountListResponseDc>>> GetNBFCBusinessLoanAccountList(LoanAccountListRequestDc obj)
        {
            var res = await _loanAccountManager.GetNBFCBusinessLoanAccountList(obj, CompanyId);
            return res;
        }

        [Route("GetHopTrendDashboardData")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<DisbursementDashboardDataResponseDc> GetHopTrendDashboardData(HopDashboardRequestDc req)
        {
            var LoanAccDetails = await _loanAccountManager.GetHopTrendDashboardData(req);
            return LoanAccDetails;
        }

        [Route("GetHopDisbursementDashboard")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HopDisbursementDashboardResponseDc> GetHopDisbursementDashboard(HopDashboardRequestDc req)
        {
            var LoanAccDetails = await _loanAccountManager.GetHopDisbursementDashboard(req);
            return LoanAccDetails;
        }


        [HttpGet]
        [Route("GetLedger_RetailerStatement")]
        public async Task<GRPCReply<LoanPaymentsResultDC>> GetLedger_RetailerStatement(long loanAccountId, DateTime FromDate, DateTime ToDate)
        {
            var LoanAccDetails = await _loanAccountManager.GetLedger_RetailerStatement(loanAccountId, FromDate, ToDate);
            return LoanAccDetails;
        }


        [Route("InsertBLEmiTransactionsJob")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> InsertBLEmiTransactionsJob()
        {
            var LoanAccDetails = await _loanAccountManager.InsertBLEmiTransactionsJob();
            return LoanAccDetails;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UploadBLInvoiceExcel")]
        public async Task<GRPCReply<bool>> UploadBLInvoiceExcel(string fileUrl)
        {
            var res = await _loanAccountManager.UploadBLInvoiceExcel(fileUrl);
            return res;
        }

        [HttpGet]
        [Route("GetBLDetails")]
        public async Task<GRPCReply<BLDetailsDc>> GetBLDetails(long loanAccountId)
        {
            var res = await _loanAccountManager.GetBLDetails(loanAccountId);
            return res;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("HtmlToPdfConvert")]
        public async Task<string> HtmlToPdfConvert(string fileUrl)
        {
            var res = await _loanAccountManager.HtmlToPdfConvert(fileUrl);
            return res;
        }
    }
}
