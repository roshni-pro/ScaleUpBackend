
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Constants;

using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;

using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.ApiGateways.Aggregator.Services;

namespace ScaleUP.ApiGateways.Aggregator.Managers.NBFC
{
    public class AyeFinanceSCFManger
    {
        private IProductService _productService;
        private ILeadService _leadService;
        private IIdentityService _identityService;
        private ICompanyService _iCompanyService;
        private ILoanAccountService _iloanAccountService;


        public AyeFinanceSCFManger(
            IProductService productService,
            ILeadService leadService,
            IIdentityService identityService,
            ICompanyService iCompanyService,
             ILoanAccountService iloanAccountService
            )
        {

            _productService = productService;
            _leadService = leadService;
            _identityService = identityService;
            _iCompanyService = iCompanyService;
            _iloanAccountService = iloanAccountService;
        }
        public async Task<GRPCReply<string>> GenerateAyeToken()
        {
            GRPCReply<string> reply = new GRPCReply<string>();

            var companyReply = await _iCompanyService.GetAllNBFCCompany();
            if (companyReply != null && companyReply.Status && companyReply.Response != null)
            {
                var ayefin = companyReply.Response.FirstOrDefault(x => x.CompanyIdentificationCode == CompanyIdentificationCodeConstants.AyeFinanceSCF).NbfcId;
                if (ayefin != null)
                {
                    reply = await _leadService.GenerateAyeToken();
                    if (reply != null)
                    {

                        var productReply = await _productService.AddUpdateTokenNBFCCompany(new AddUpdateTokenNBFCCompany { token = reply.Response, companyId = ayefin, productType = ProductCodeConstants.CreditLine });

                    }
                }

            }
            return reply;

        }
        public async Task<GRPCReply<string>> AddLead(AyeleadReq request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            var token = await GetNbfcToken();
            reply = await _leadService.AddLead(new GRPCRequest<AyeleadReq> { Request = new AyeleadReq { LeadId = request.LeadId, token = token.Response } });
            return reply;

        }

        public async Task<GRPCReply<string>> GetWebUrl(AyeleadReq request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            var token = await GetNbfcToken();

            reply = await _leadService.GetWebUrl(new GRPCRequest<AyeleadReq> { Request = new AyeleadReq { LeadId = request.LeadId, token = token.Response } });


            return reply;

        }
        public async Task<GRPCReply<string>> GetNbfcToken()
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            var companyReply = await _iCompanyService.GetAllNBFCCompany();
            if (companyReply != null && companyReply.Status && companyReply.Response != null)
            {
                var ayefin = companyReply.Response.FirstOrDefault(x => x.CompanyIdentificationCode == CompanyIdentificationCodeConstants.AyeFinanceSCF);
                if (ayefin != null)
                {

                    var token = await _productService.GetNBFCProductToken(new AddUpdateTokenNBFCCompany { companyId = ayefin.NbfcId, productType = ProductCodeConstants.CreditLine });
                    if (token != null)
                    {
                        reply.Response = token.Response;
                        reply.Status = true;
                    }
                }

            }

            return reply;

        }

        public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(AyeleadReq request)
        {
            GRPCReply<CheckCreditLineData> reply = new GRPCReply<CheckCreditLineData>();

            var token = await GetNbfcToken();
            reply = await _leadService.CheckCreditLine(new GRPCRequest<AyeleadReq> { Request = new AyeleadReq { LeadId = request.LeadId, token = token.Response } });


            return reply;

        }

        public async Task<GRPCReply<string>> TransactionSendOtp(AyeleadReq request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            var token = await GetNbfcToken();

            reply = await _leadService.TransactionSendOtp(new GRPCRequest<AyeleadReq> { Request = new AyeleadReq { LeadId = request.LeadId, token = token.Response } });


            return reply;

        }
        public async Task<GRPCReply<string>> TransactionVerifyOtp(TransactionVerifyOtpReqDc request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            var token = await GetNbfcToken();

            reply = await _leadService.TransactionVerifyOtp(new GRPCRequest<TransactionVerifyOtpReqDc> { Request = new TransactionVerifyOtpReqDc { leadId = request.leadId, token = token.Response, otp = request.otp } });
            if (reply.Status)
            {

                var leadData = await _leadService.GetLeadInfoById(new GRPCRequest<long> { Request = request.leadId });
                if(leadData != null) {
                    var response = await _iloanAccountService.SaveLoanAccountDetails(new GRPCRequest<SaveLoanAccountRequestDC>
                    {
                        Request = new SaveLoanAccountRequestDC
                        {
                            LeadId = request.leadId,
                            LeadCode = leadData.Response.LeadCode,
                            MobileNo = leadData.Response.MobileNo,
                            NBFCCompanyId = leadData.Response.OfferCompanyId ?? 0,
                            ProductId = leadData.Response.ProductId,
                            UserId = leadData.Response.UserName,
                            CustomerName = leadData.Response.CustomerName,
                            AgreementRenewalDate = leadData.Response.ApplicationDate.Value.AddYears(1),
                            ApplicationDate = leadData.Response.ApplicationDate,
                            CityName = leadData.Response.CustomerCurrentCityName,
                            ShopName = leadData.Response.ShopName,
                            CustomerImage = leadData.Response.CustomerImage,
                            IsAccountActive = true,
                            IsBlock = false,
                            ProductType = leadData.Response.ProductCode,
                            //AnchorCompanyId = CompanyDetail.AnchorCompanyConfig.CompanyId,
                            //IsDefaultNBFC = CompanyDetail.IsDefaultNBFC,
                            //AnchorName = anchorcompany.Response.CompanyName,
                            NBFCCompanyCode = LeadNBFCConstants.AyeFinanceSCF,
                            Webhookresposne = null,
                            BusinessId = null,
                            ApplicationId = null,
                            BusinessCode = null,
                            ApplicationCode = null,
                            token = request.token
                        }
                    });
                    if (response.Status)
                    {
                        await _leadService.UpdateActivityForAyeFin(new GRPCRequest<long> { Request = leadData.Response.LeadId });
                    }
                }
                else
                {
                    reply.Response = "Lead not found!";
                }
            }
            return reply;

        }

        public async Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(ApplyLoanreq request)
        {
            GRPCReply<ApplyLoanResponseDC> reply = new GRPCReply<ApplyLoanResponseDC>();
            var token = await GetNbfcToken();

            reply = await _iloanAccountService.ApplyLoan(new GRPCRequest<ApplyLoanreq> { Request = new ApplyLoanreq { loanId = request.loanId, orderId = request.orderId, amount = request.amount, token = token.Response } });


            return reply;

        }

        public async Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(AyeloanReq request)
        {
            GRPCReply<CheckTotalAndAvailableLimitResponseDc> reply = new GRPCReply<CheckTotalAndAvailableLimitResponseDc>();
            var token = await GetNbfcToken();
            reply = await _iloanAccountService.CheckTotalAndAvailableLimit(new GRPCRequest<AyeloanReq> { Request = new AyeloanReq { loanId = request.loanId, token = token.Response } });
            return reply;

        }

        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(DeliveryConfirmationreq request)
        {
            GRPCReply<DeliveryConfirmationResponseDC> reply = new GRPCReply<DeliveryConfirmationResponseDC>();
            var token = await GetNbfcToken();
            reply = await _iloanAccountService.DeliveryConfirmation(new GRPCRequest<DeliveryConfirmationreq> { Request = new DeliveryConfirmationreq { loanId = request.loanId, amount = request.amount, invoiceNo = request.invoiceNo, orderId = request.orderId, token = token.Response } });
            return reply;

        }

        public async Task<GRPCReply<CancellationResponseDC>> CancelTransaction(CancelTxnReq request)
        {
            GRPCReply<CancellationResponseDC> reply = new GRPCReply<CancellationResponseDC>();
            var token = await GetNbfcToken();
            reply = await _iloanAccountService.CancelTransaction(new GRPCRequest<CancelTxnReq> { Request = new CancelTxnReq { loanId = request.loanId, amount = request.amount, orderId = request.orderId, cancellationCode = request.cancellationCode, remarks = request.remarks, token = token.Response } });
            return reply;

        }
        public async Task<GRPCReply<string>> RefundTransaction(string orderNo, double refundAmount)
        {
            var token = await GetNbfcToken();

            var loanReply = await _iloanAccountService.RefundTransaction(new GRPCRequest<RefundRequestDTO>
            {
                Request = new RefundRequestDTO
                {
                    OrderNo = orderNo,
                    RefundAmount = refundAmount,
                    NBFCToken = token.Response
                }
            });
            return loanReply;
        }
        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(Repaymentreq request)
        {
            var token = await GetNbfcToken();

            return await _iloanAccountService.Repayment(new GRPCRequest<Repaymentreq>
            {
                Request = new Repaymentreq
                {
                    loanId = request.loanId,
                    orderId = request.orderId,
                    amount = request.amount,
                    token = token.Response,
                    TransactionReqNo = request.TransactionReqNo,
                    adjustment = request.adjustment,
                    modeOfPayment = request.modeOfPayment,
                    receiptId = request.receiptId,
                    refId = request.refId
                }
            });

        }

        public async Task<GRPCReply<NBFCLoanAccountDetailResponseDTO>> GetNBFCLoanAccountDetail(GetNBFCLoanAccountDetailDTO req)
        {
            var token = await GetNbfcToken();
            return await _iloanAccountService.GetNBFCLoanAccountDetail(new GRPCRequest<GetNBFCLoanAccountDetailDTO>
            {
                Request = new GetNBFCLoanAccountDetailDTO
                {
                    loanAccountId = req.loanAccountId,
                    NBFCToken = token.Response,
                }
            });
        }
    }
}
