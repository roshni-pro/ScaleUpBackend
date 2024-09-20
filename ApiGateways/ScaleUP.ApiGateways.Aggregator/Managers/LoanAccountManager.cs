using Azure;
using Elasticsearch.Net;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Models;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Managers.NBFC;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;

using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.LoanAccount;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Helper;

//using ScaleUP.Services.KYCDTO.Transacion;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static MassTransit.ValidationResultExtensions;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class LoanAccountManager
    {
        private ILeadService _iLeadService;
        private IProductService _iProductService;
        private ILoanAccountService _iloanAccountService;
        private ICompanyService _iCompanyService;
        private KYCUserDetailManager _KYCUserDetailManager;
        private ICommunicationService _iCommunicationService;
        private IIdentityService _iIdentityService;
        private ILocationService _iLocationService;
        private IHostEnvironment _hostingEnvironment;
        private IMediaService _iMediaService;
        private AyeFinanceSCFManger _ayeFinanceSCFManger;

        public LoanAccountManager(ILeadService iLeadService, IProductService iProductService, ILoanAccountService iloanAccountService, AyeFinanceSCFManger ayeFinanceSCFManger,
            ICompanyService iCompanyService, KYCUserDetailManager KYCUserDetailManager

            , ICommunicationService iCommunicationService, IIdentityService iIdentityService
            , ILocationService iLocationService, IHostEnvironment hostingEnvironment
            , IMediaService iMediaService
            )
        {
            _iLeadService = iLeadService;
            _iProductService = iProductService;
            _iloanAccountService = iloanAccountService;
            _iCompanyService = iCompanyService;
            _KYCUserDetailManager = KYCUserDetailManager;
            _ayeFinanceSCFManger = ayeFinanceSCFManger;
            _iCommunicationService = iCommunicationService;
            _iIdentityService = iIdentityService;
            _iLocationService = iLocationService;
            _hostingEnvironment = hostingEnvironment;
            _iMediaService = iMediaService;
        }

        public async Task<CompanyConfigReply> LeadCompanyConfig(long leadId, long TranscompanyId = 0)
        {
            CompanyConfigReply companyConfigReply = new CompanyConfigReply();
            var leadRequest = new GRPCRequest<LeadCompanyConfigProdRequest>();
            leadRequest.Request = new LeadCompanyConfigProdRequest
            {
                AnchorCompanyId = TranscompanyId,
                LeadId = leadId
            };
            var leadReply = await _iLeadService.GetLeadAllCompanyAsync(leadRequest);

            if (leadReply != null && leadReply.Status == true)
            {
                companyConfigReply = await _iProductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                {
                    AnchorCompanyId = TranscompanyId > 0 ? TranscompanyId : leadReply.Response.AnchorCompanyId,
                    NBFCCompanyId = leadReply.Response.NBFCCompanyId,
                    ProductId = leadReply.Response.ProductId,
                });

                var defaultcompany = await _iCompanyService.GetDefaultConfigNBFCCompany(new GRPCRequest<List<long>> { Request = new List<long> { companyConfigReply.NBFCCompanyConfig.CompanyId } });
                companyConfigReply.IsDefaultNBFC = defaultcompany?.Response?.Any() ?? false;
            }

            return companyConfigReply;
        }


        public async Task<CompanyConfigReply> LeadAnchoreNBFCCompanyConfig(long leadId, long nbfcCompanyId, long TranscompanyId)
        {
            CompanyConfigReply companyConfigReply = new CompanyConfigReply();

            var leadProd = await _iLeadService.GetLeadProductId(leadId);

            companyConfigReply = await _iProductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
            {
                AnchorCompanyId = TranscompanyId,
                NBFCCompanyId = nbfcCompanyId,
                ProductId = leadProd.Response.ProductId,
            });

            var defaultcompany = await _iCompanyService.GetDefaultConfigNBFCCompany(new GRPCRequest<List<long>> { Request = new List<long> { companyConfigReply.NBFCCompanyConfig.CompanyId } });
            companyConfigReply.IsDefaultNBFC = defaultcompany?.Response?.Any() ?? false;


            return companyConfigReply;
        }
        public async Task<GRPCReply<LoanAccount_DisplayDisbursalAggDTO>> GetAccountDisbursement(long leadId, double DisbursalAmount, string LeadCode, long companyId, long anchoreCompanyId)
        {
            GRPCReply<LoanAccount_DisplayDisbursalAggDTO> reply = new GRPCReply<LoanAccount_DisplayDisbursalAggDTO>();
            reply.Status = false;
            reply.Message = "failed";

            //var leadResponse = await _iLeadService.
            DateTime AggrementApplyDate = DateTime.Now;

            var CompanyDetail = await LeadAnchoreNBFCCompanyConfig(leadId, companyId, anchoreCompanyId);

            if (CompanyDetail != null)
            {
                double ProcessingFee = 0, TransFee = 0;
                double ProcessingFeeTax = 0, ConvenionFeeTax = 0;
                if (CompanyDetail.AnchorCompanyConfig.ProcessingFeePayableBy == "Customer")
                {
                    if (CompanyDetail.AnchorCompanyConfig.ProcessingFeeType == "Percentage")
                    {
                        ProcessingFee = (DisbursalAmount * CompanyDetail.AnchorCompanyConfig.ProcessingFeeRate / 100.0);
                    }
                    else
                    {
                        ProcessingFee = CompanyDetail.AnchorCompanyConfig.ProcessingFeeRate;
                    }
                }

                if (string.IsNullOrEmpty(CompanyDetail.AnchorCompanyConfig.AnnualInterestPayableBy))
                    CompanyDetail.AnchorCompanyConfig.AnnualInterestPayableBy = "Customer";

                if (CompanyDetail.AnchorCompanyConfig.AnnualInterestPayableBy == "Customer")
                {

                    TransFee = CompanyDetail.AnchorCompanyConfig.AnnualInterestRate.HasValue ? (DisbursalAmount * CompanyDetail.AnchorCompanyConfig.AnnualInterestRate.Value / 100.0) : 0;

                }

                var GST = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);

                if (GST.Status)
                {
                    ProcessingFeeTax = ProcessingFee > 0 ? Math.Round(((ProcessingFee) * GST.Response / 100), 2) : 0;
                    ConvenionFeeTax = TransFee > 0 ? Math.Round(((TransFee) * GST.Response / 100), 2) : 0;
                }

                reply.Response = new LoanAccount_DisplayDisbursalAggDTO
                {
                    LeadNo = LeadCode,
                    CreditLimit = DisbursalAmount,
                    ProcessingFeeAmount = ProcessingFee > 0 ? ProcessingFee + ProcessingFeeTax : ProcessingFee,
                    GSTAmount = ProcessingFee > 0 ? ProcessingFeeTax : 0,
                    ProcessingFeePayableBy = CompanyDetail.AnchorCompanyConfig.ProcessingFeePayableBy,


                    AppliedDate = AggrementApplyDate,
                    ConvenionFeeRate = CompanyDetail.AnchorCompanyConfig.AnnualInterestRate ?? 0,
                    ConvenionGSTAmount = ConvenionFeeTax,
                    ConvenionFeePayableBy = CompanyDetail.AnchorCompanyConfig.AnnualInterestPayableBy,
                };
                reply.Status = true;
                reply.Message = "success";
            }
            return reply;
        }

        public async Task<GRPCReply<long>> PostDisbursement(PostDisbursementDTO request)
        {
            var response = new GRPCReply<long>();
            long leadid = request.leadId;

            var leadrequest = new GRPCRequest<long>();
            leadrequest.Request = leadid;
            var lead = await _iLeadService.GetLeadInfoById(leadrequest);
            long LoanAccountId = 0;

            var CompanyDetail = await LeadCompanyConfig(request.leadId);

            if (lead != null)
            {
                GRPCReply<BlackSoilUpdateResponse> applicationdetail = new GRPCReply<BlackSoilUpdateResponse>();
                var com = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = lead.Response.OfferCompanyId ?? 0 });
                if (com.Response == null)
                {
                    response.Status = false;
                    response.Message = $"OfferCompanyId not found for lead : {leadid}";
                    return response;
                }

                if (!string.IsNullOrEmpty(com.Response.IdentificationCode) && com.Response.IdentificationCode == LeadNBFCConstants.BlackSoil.ToString())
                {
                    applicationdetail = await _iLeadService.GetBlackSoilApplicationDetails(leadrequest);
                    if (!applicationdetail.Status)
                    {
                        response.Status = applicationdetail.Status;
                        response.Message = applicationdetail.Message;
                        return response;
                    }
                }

                //var pf = lead.Response.BlackSoilPfCollection;//Abhishek ji


                var pro = await _iProductService.GetProductDataById(new GRPCRequest<long> { Request = lead.Response.ProductId });
                var anchorcompany = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = CompanyDetail.AnchorCompanyConfig.CompanyId });

                response = await _iloanAccountService.SaveLoanAccount(new GRPCRequest<SaveLoanAccountRequestDC>
                {
                    Request = new SaveLoanAccountRequestDC
                    {
                        LeadId = lead.Response.LeadId,
                        LeadCode = lead.Response.LeadCode,
                        MobileNo = lead.Response.MobileNo,
                        NBFCCompanyId = lead.Response.OfferCompanyId ?? 0,
                        ProductId = lead.Response.ProductId,
                        UserId = lead.Response.UserName,
                        AnchorCompanyId = CompanyDetail.AnchorCompanyConfig.CompanyId,
                        //CustomerName = _LeadNameRes.Response,
                        CustomerName = lead.Response.CustomerName,   //CustomerNm,
                        AgreementRenewalDate = lead.Response.ApplicationDate.Value.AddYears(1),
                        ApplicationDate = lead.Response.ApplicationDate,
                        IsDefaultNBFC = CompanyDetail.IsDefaultNBFC,
                        CityName = lead.Response.CustomerCurrentCityName, //(cityData != null && cityData.Status && cityData.Response.cityName!=null) ? cityData.Response.cityName : null,
                        AnchorName = anchorcompany.Response.CompanyName,
                        ProductType = pro.Response.ProductType,
                        IsAccountActive = true,
                        IsBlock = false,
                        NBFCCompanyCode = com.Response.IdentificationCode,
                        Webhookresposne = string.IsNullOrEmpty(request.Webhookresposne) ? null : request.Webhookresposne,
                        BusinessId = !string.IsNullOrEmpty(applicationdetail.Response.BusinessId) ? applicationdetail.Response.BusinessId : null,
                        ApplicationId = !string.IsNullOrEmpty(applicationdetail.Response.ApplicationId) ? applicationdetail.Response.ApplicationId : null,
                        BusinessCode = !string.IsNullOrEmpty(applicationdetail.Response.BusinessCode) ? applicationdetail.Response.BusinessCode : null,
                        ApplicationCode = !string.IsNullOrEmpty(applicationdetail.Response.ApplicationCode) ? applicationdetail.Response.ApplicationCode : null,
                        ShopName = lead.Response.ShopName,
                        CustomerImage = lead.Response.CustomerImage
                    }
                });
                if (response.Status)
                {
                    #region Save Loan BankDetails
                    LoanAccountId = response.Response;

                    var leadBankDetail = await _iLeadService.GetLeadBankDetailByLeadId(new GRPCRequest<long> { Request = lead.Response.LeadId });
                    if (leadBankDetail.Status && leadBankDetail.Response != null)
                    {
                        List<LeadBankDetailResponse> BankDetailList = new List<LeadBankDetailResponse>();

                        foreach (var item in leadBankDetail.Response)
                        {
                            var bank = new LeadBankDetailResponse
                            {
                                LoanAccountId = LoanAccountId,
                                AccountHolderName = item.AccountHolderName,
                                AccountNumber = item.AccountNumber,
                                AccountType = item.AccountType,
                                BankName = item.BankName,
                                Type = item.Type,
                                IFSCCode = item.IFSCCode,
                                LeadId = item.LeadId
                            };
                            BankDetailList.Add(bank);
                        }
                        GRPCRequest<List<LeadBankDetailResponse>> reqest = new GRPCRequest<List<LeadBankDetailResponse>> { Request = BankDetailList };
                        await _iloanAccountService.SaveLoanBankDetails(reqest);
                    }
                    #endregion
                    if (CompanyDetail != null)
                    {
                        var GST = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
                        if (GST.Status == true)
                        {
                            LoanAccountId = response.Response;
                            var CustUniqueCode = "";

                            foreach (var company in lead.Response.LeadCompanies)
                            {
                                //if (company.LeadProcessStatus == 2)
                                //{
                                response = await _iloanAccountService.SaveLoanAccountCompanyLead(new GRPCRequest<SaveLoanAccountCompanyLeadRequestDC>
                                {
                                    Request = new SaveLoanAccountCompanyLeadRequestDC
                                    {
                                        LoanAccountId = LoanAccountId,
                                        CompanyId = company.CompanyId,
                                        LeadProcessStatus = company.LeadProcessStatus,
                                        UserUniqueCode = company.UserUniqueCode != null ? company.UserUniqueCode : "",
                                        AnchorName = anchorcompany.Response.CompanyName,
                                        LogoURL = anchorcompany.Response.LogoURL,
                                    }
                                });
                                CustUniqueCode = company.UserUniqueCode != null ? company.UserUniqueCode : "";
                                //}
                            }


                            response = await _iloanAccountService.AddLoanAccountCredit(new GRPCRequest<LoanAccountCreditsRequest>
                            {
                                Request = new LoanAccountCreditsRequest
                                {
                                    GstRate = GST.Response,
                                    ProcessingFeeRate = CompanyDetail.AnchorCompanyConfig.ProcessingFeeRate,
                                    AnnualInterestRate = CompanyDetail.AnchorCompanyConfig.AnnualInterestRate ?? 0,
                                    DisbursalAmount = lead.Response.CreditLimit ?? 0,
                                    LoanAccountId = LoanAccountId, //response.Response,
                                    BounceCharge = CompanyDetail.AnchorCompanyConfig.BounceCharge,
                                    CreditDays = CompanyDetail.AnchorCompanyConfig.CreditDays,
                                    DelayPenaltyRate = CompanyDetail.AnchorCompanyConfig.DelayPenaltyRate,
                                    CreditLimitAmount = lead.Response.CreditLimit,
                                }
                            });

                            if (response.Status == true && response.Message == "Success")
                            {
                                var UpdateCurrentActivityLeadrequest = new GRPCRequest<long>();
                                UpdateCurrentActivityLeadrequest.Request = leadid;
                                var results = await _iLeadService.UpdateCurrentActivity(UpdateCurrentActivityLeadrequest);//As Per Discussed with Sudeep Sir And Harry Sir


                                /////  ---------- S -------------------  post Disbursement Entry in Account Transaction Tables  ------

                                response = await _iloanAccountService.PostTransaction(new GRPCRequest<ACT_PostAccountDisbursementRequestDC>
                                {
                                    Request = new ACT_PostAccountDisbursementRequestDC
                                    {
                                        LeadId = lead.Response.LeadId,
                                        Amount = lead.Response.CreditLimit,
                                        DiscountAmount = 0,
                                        TransactionTypeCode = TransactionTypesConstants.Disbursement,
                                        TransactionReqNo = "",

                                        ProcessingFeeType = CompanyDetail.AnchorCompanyConfig.ProcessingFeeType,
                                        ProcessingFeeRate = CompanyDetail.AnchorCompanyConfig.ProcessingFeeRate,
                                        GstRate = GST.Response,
                                        //ConvenienceFeeRate = CompanyDetail.AnchorCompanyConfig.TransactionFeeRate ?? 0,
                                        ConvenienceFeeRate = CompanyDetail.AnchorCompanyConfig.AnnualInterestRate ?? 0,
                                        CreditDays = 4, //CompanyDetail.AnchorCompanyConfig.CreditDays,
                                        BounceCharge = CompanyDetail.AnchorCompanyConfig.BounceCharge,
                                        DelayPenaltyRate = CompanyDetail.AnchorCompanyConfig.DelayPenaltyRate,
                                        AnchorCompanyId = CompanyDetail.AnchorCompanyConfig.CompanyId,
                                        ProcessingFeePayableBy = CompanyDetail.AnchorCompanyConfig.ProcessingFeePayableBy,
                                        ConvenienceFeeType = "",//CompanyDetail.AnchorCompanyConfig.con
                                        ConvenienceFeePayableBy = "",//CompanyDetail.AnchorCompanyConfig.Co
                                        CustomerUniqueCode = CustUniqueCode,
                                        DisbursementType = CompanyDetail.NBFCCompanyConfig.DisbursementType,
                                        BlackSoilPfCollection = lead.Response.BlackSoilPfCollection
                                    }
                                });
                                /////  ---------- E -------------------  post Disbursement Entry in Account Transaction Tables  ------
                            }
                        }
                    }
                    else
                    {
                        response.Status = lead.Status;
                        response.Message = lead.Message;
                    }
                }
                else
                {
                    response.Status = response.Status;
                    response.Message = response.Message;
                }
            }
            else
            {
                response.Status = lead.Status;
                response.Message = lead.Message;
            }
            return response;
        }

        public async Task<GRPCReply<DisbursementResponse>> GetDisbursement(GRPCRequest<long> LeadId)
        {
            var res = await _iloanAccountService.GetDisbursement(LeadId);
            return res;
        }
        public async Task<GRPCReply<List<LoanAccountDisbursementResponse>>> GetLoanAccountDisbursement(GRPCRequest<List<long>> LeadId)
        {
            var res = await _iloanAccountService.GetLoanAccountDisbursement(LeadId);
            return res;
        }
        public async Task<GRPCReply<CompanyDetail>> GetCompanyLogo(GRPCRequest<long> request)
        {
            var res = await _iCompanyService.GetCompanyLogo(request);
            return res;
        }

        public async Task<GRPCReply<PaymentResponseDc>> GetByTransactionReqNo(string request)
        {

            GRPCReply<PaymentResponseDc> response = new GRPCReply<PaymentResponseDc>();
            response.Response = new PaymentResponseDc();
            GRPCRequest<string> transnoReq = new GRPCRequest<string> { Request = request };
            //var LoanAccount = await _iloanAccountService.GetLoanAccountDetailByTxnId(transnoReq);
            var tokenRes = await _ayeFinanceSCFManger.GetNbfcToken();
            var NBFCToken = tokenRes.Status ? tokenRes.Response : "";
            var LoanAccount = await _iloanAccountService.GetNBFCLoanAccountDetailByTxnId(new GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO>
            {
                Request = new GetAyeNBFCLoanAccountDetailByTxnIdDTO
                {
                    NBFCToken = NBFCToken,
                    TransactionReqNo = request
                }
            });
            if (LoanAccount != null && LoanAccount.Response.AnchorCompanyId > 0)
            {

                var companyProductReply = await _iProductService.GetAnchoreCompanyProductConfig(new GetAnchoreProductConfigRequest
                {
                    CompanyId = LoanAccount.Response.AnchorCompanyId,
                    ProductId = LoanAccount.Response.ProductId
                });

                if (companyProductReply != null && companyProductReply.Status)
                {
                    var GstResponse = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
                    var companyConfig = companyProductReply.GetCompanyProductConfigList.FirstOrDefault();
                    if (companyConfig != null)
                    {
                        GRPCRequest<GetPaymentReqByTxnNo> InterestRequest = new GRPCRequest<GetPaymentReqByTxnNo>();
                        GetPaymentReqByTxnNo obj = new GetPaymentReqByTxnNo
                        {
                            TransactionReqNo = request,
                            GstRate = GstResponse.Response,
                            AnnualInterestRate = companyConfig.AnnualInterestRate,
                            AnnualInterestPayableBy = companyConfig.AnnualInterestPayableBy
                        };
                        InterestRequest.Request = obj;

                        double perDayInterestRate = 0;

                        var interestResponse = await _iloanAccountService.GetTransactionInterestRate(InterestRequest);
                        if (interestResponse.Status)
                        {
                            perDayInterestRate = interestResponse.Response;
                        }

                        response.Response.MobileNo = LoanAccount.Response.MobileNo;
                        response.Response.CreditDays = companyConfig.CreditDays != null ? companyConfig.CreditDays.ToList() : new List<int>();
                        if (LoanAccount.Response.creditDay.HasValue && LoanAccount.Response.creditDay > 0)
                        {
                            response.Response.CreditDays = response.Response.CreditDays.Where(x => x == LoanAccount.Response.creditDay).ToList();
                        }
                        response.Response.LoanAccountId = LoanAccount.Response.LoanAccountId;
                        response.Response.OrderNo = LoanAccount.Response.OrderNo;
                        response.Response.TransactionAmount = LoanAccount.Response.InvoiceAmount;
                        response.Response.AnchorName = LoanAccount.Response.AnchorName;
                        response.Response.TransactionReqNo = request;
                        response.Response.AvailableCreditLimit = LoanAccount.Response.CreditLimitAmount;
                        response.Response.InterestRate = perDayInterestRate;
                        response.Response.UtilizateLimit = LoanAccount.Response.UtilizateLimit;
                        response.Response.InvoiceAmount = LoanAccount.Response.InvoiceAmount;
                        response.Response.AnchorCompanyId = LoanAccount.Response.AnchorCompanyId;
                        response.Response.IsPayableByCustomer = companyConfig.AnnualInterestPayableBy == "Customer" ? true : false;
                        response.Response.CreditDayWiseAmounts = new List<CreditDayWiseAmount>();

                        List<CreditDayWiseAmount> creditDayWiseAmounts = new List<CreditDayWiseAmount>();

                        double Amount = LoanAccount.Response.InvoiceAmount;

                        foreach (var day in response.Response.CreditDays)
                        {
                            var amount = (Amount * day * perDayInterestRate / 100);
                            var finalAmount = amount + LoanAccount.Response.InvoiceAmount;
                            creditDayWiseAmounts.Add(new CreditDayWiseAmount
                            {
                                Amount = amount,
                                days = day,
                                FinalAmount = finalAmount,
                                CreditDayAmountCals = new CreditDayAmountCal
                                {
                                    InterestRate = perDayInterestRate,
                                    InterestAmount = amount,
                                    TotalAmount = amount + LoanAccount.Response.InvoiceAmount,
                                    InvoiceAmount = LoanAccount.Response.InvoiceAmount,
                                    AnnualInterestRate = companyConfig.AnnualInterestRate
                                }
                            });
                        }
                        response.Response.CreditDayWiseAmounts = creditDayWiseAmounts;
                        response.Response.IsAccountActive = LoanAccount.Response.IsAccountActive;
                        response.Response.IsBlock = LoanAccount.Response.IsBlock;
                        response.Response.TransactionStatus = LoanAccount.Response.TransactionStatus;
                        response.Response.IsBlockComment = LoanAccount.Response.IsBlockComment;
                        response.Status = true;
                        response.Message = "success";
                        await ResentOrderOTP(LoanAccount.Response.MobileNo, request);

                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Company product not found";
                }
            }
            return response;
        }

        public async Task<GetCompanyProductConfigReply> GetAnchoreCompanyDetail(long companyId, long productId)
        {

            var companyProductReply = await _iProductService.GetAnchoreCompanyProductConfig(new GetAnchoreProductConfigRequest
            {
                CompanyId = companyId,
                ProductId = productId
            });

            return companyProductReply;
        }


        public async Task<GRPCReply<string>> PostOrderPlacement(OrderPlacementRequestDTO order)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();

            GRPCRequest<OrderPlacementRequestDC> res = new GRPCRequest<OrderPlacementRequestDC>();

            GRPCReply<string> tokenres = await _ayeFinanceSCFManger.GetNbfcToken();
            var NBFCToken = tokenres.Status ? tokenres.Response : "";

            var LoanAccount = await _iloanAccountService.GetNBFCLoanAccountDetailByTxnId(new GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO>
            {
                Request = new GetAyeNBFCLoanAccountDetailByTxnIdDTO
                {
                    NBFCToken = NBFCToken,
                    TransactionReqNo = order.TransactionReqNo
                }
            });

            //LoanAccount = await _iloanAccountService.GetLoanAccountDetailByTxnId(TxnRequest);

            if (LoanAccount != null && LoanAccount.Response.AnchorCompanyId > 0)
            {
                if (LoanAccount.Response.IsBlock)
                {
                    gRPCReply.Response = "";
                    gRPCReply.Status = false;
                    gRPCReply.Message = "blocked";
                    return gRPCReply;
                }
                if (!LoanAccount.Response.IsAccountActive)
                {
                    gRPCReply.Response = "";
                    gRPCReply.Status = false;
                    gRPCReply.Message = "InActive";
                    return gRPCReply;
                }
                if (LoanAccount.Response.TransactionStatus.ToLower() == "Overdue".ToLower())
                {
                    gRPCReply.Response = "";
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Overdue";
                    return gRPCReply;
                }
                var companyConfigReply = await _iProductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                {
                    AnchorCompanyId = LoanAccount.Response.AnchorCompanyId,
                    NBFCCompanyId = LoanAccount.Response.NBFCCompanyId,
                    ProductId = LoanAccount.Response.ProductId,
                });


                if (companyConfigReply.AnchorCompanyConfig != null)
                {
                    var GstResponse = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
                    var companyConfig = companyConfigReply.AnchorCompanyConfig;

                    OrderPlacementRequestDC obj = new OrderPlacementRequestDC
                    {
                        TransactionReqNo = order.TransactionReqNo,
                        GstRate = GstResponse.Response,
                        AnchorCompanyId = LoanAccount.Response.AnchorCompanyId,
                        InterestRate = companyConfig.AnnualInterestRate,
                        BounceCharge = companyConfig.BounceCharge,
                        CreditDay = order.CreditDay,
                        DelayPenaltyRate = companyConfig.DelayPenaltyRate,
                        InterestPayableBy = companyConfig.AnnualInterestPayableBy,
                        Amount = order.Amount,
                        InterestType = "Percentage",
                        ProductId = LoanAccount.Response.ProductId,
                        LoanAccountId = order.LoanAccountId,
                        Token = NBFCToken
                    };

                    res.Request = obj;
                }
                else
                {
                    gRPCReply.Response = "";
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Company product not found";
                    return gRPCReply;
                }
            }
            var data = await _iloanAccountService.PostOrderPlacement(res);
            if (data != null && data.Status && !string.IsNullOrEmpty(data.Response))
            {
                gRPCReply.Response = order.TransactionReqNo;
                gRPCReply.Status = data.Status;
                gRPCReply.Message = data.Message;
            }
            else
            {
                gRPCReply.Response = "";
                gRPCReply.Status = data.Status;
                gRPCReply.Message = data.Message;
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<LoanCreditLimit>> GetAvailableCreditLimitByLeadId(GRPCRequest<long> LeadMasterId)
        {
            var result = await _iloanAccountService.GetAvailableCreditLimitByLeadId(LeadMasterId);
            return result;
        }
        public async Task<GRPCReply<OrderInitiateResponse>> OrderInitiate(PaymentRequestdc request)
        {
            GRPCReply<OrderInitiateResponse> result = new GRPCReply<OrderInitiateResponse>();
            var anchoreCompanyDetail = await _iCompanyService.GetCompanyDataByCode(new GRPCRequest<string> { Request = request.AnchorCompanyCode });
            if (anchoreCompanyDetail != null && anchoreCompanyDetail.Response.CompanyId > 0)
            {

                GRPCReply<string> tokenres = await _ayeFinanceSCFManger.GetNbfcToken();
                var AyeNBFCToken = tokenres.Status ? tokenres.Response : "";
                OrderPlacementWithOtp OrderInitiateReq = new OrderPlacementWithOtp
                {
                    AnchorCompanyId = anchoreCompanyDetail.Response.CompanyId,
                    TransactionAmount = request.TransactionAmount,
                    LoanAccountId = request.LoanAccountId,
                    OrderNo = request.OrderNo,
                    OrderAmount = request.OrderAmount,
                    Token = AyeNBFCToken
                };

                result = await _iloanAccountService.OrderInitiate(OrderInitiateReq);
                if (result.Status)
                {
                    //var smsResult = await GenerateOrderOtp(result.Response.MobileNo, request.TransactionAmount, result.Response.AnchorName, result.Response.CustomerName);
                    var smsResult = await GenerateOrderOtpForLogin(result.Response.MobileNo);
                    result.Response.AnchorCompanyId = anchoreCompanyDetail.Response.CompanyId;
                }
            }
            else
            {
                result.Status = false;
                result.Message = "Company not available.";
            }
            return result;
        }

        public async Task<bool> ResentOrderOTP(string MobileNo, string TransactionNo)
        {
            GRPCReply<string> tokenres = await _ayeFinanceSCFManger.GetNbfcToken();
            var NBFCToken = tokenres.Status ? tokenres.Response : "";

            //var LoanAccount = await _iloanAccountService.GetLoanAccountDetailByTxnId(new GRPCRequest<string> { Request = TransactionNo });
            var LoanAccount = await _iloanAccountService.GetNBFCLoanAccountDetailByTxnId(new GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO>
            {
                Request = new GetAyeNBFCLoanAccountDetailByTxnIdDTO
                {
                    TransactionReqNo = TransactionNo,
                    NBFCToken = NBFCToken
                }
            });

            if (LoanAccount.Response.AnchorCompanyId > 0)
            {
                var companyResponse = await _iCompanyService.GetCompanyShortName(new GRPCRequest<long> { Request = LoanAccount.Response.AnchorCompanyId });
                if (companyResponse.Status)
                    LoanAccount.Response.AnchorName = companyResponse.Response;
            }
            var smsResult = await GenerateOrderOtp(MobileNo, LoanAccount.Response.InvoiceAmount, LoanAccount.Response.AnchorName, LoanAccount.Response.CustomerName);
            return smsResult.Status;
        }
        private async Task<GenerateOTPResponse> GenerateOrderOtp(string MobileNo, double Amount, string AnchorName, string CustomerName)
        {
            GenerateOTPResponse generateOTPResponse = new GenerateOTPResponse();
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string otp = GenerateRandomNumber.GenerateRandomOTP(6, saAllowedCharacters);
            //TemplateMasters
            var TemMasterResponse = await _iloanAccountService.GetLoanAccountNotificationTemplate(new TemplateMasterRequestDc
            {
                TemplateCode = TemplateEnum.OrderPlaceOTPSMS.ToString()//"OrderPlaceOTPSMS"
            });
            if (TemMasterResponse.Status)
            {
                string DLTId = TemMasterResponse.Response.DLTID;
                string SMS = TemMasterResponse.Response.Template;
                var custName = CustomerName.ToString().Split(" ");
                var anchName = AnchorName.ToString().Split(" ");
                SMS = SMS.Replace("{#var1#}", (custName != null && custName.Any()) ? custName[0] : "Customer");
                SMS = SMS.Replace("{#var2#}", otp);
                SMS = SMS.Replace("{#var3#}", Amount.ToString());
                SMS = SMS.Replace("{#var4#}", (anchName != null && anchName.Any()) ? anchName[0] : "");

                var Sendsmsreply = await _iCommunicationService.SendSMS(new SendSMSRequest { ExpiredInMin = 10, MobileNo = MobileNo, OTP = otp, routeId = ((Int32)SMSRouteEnum.OTP).ToString(), SMS = SMS, DLTId = DLTId });
                if (Sendsmsreply.Status)
                {
                    generateOTPResponse.OTP = otp;
                    generateOTPResponse.Status = true;
                    generateOTPResponse.Message = "OTP send successfully.";
                }
                else
                {
                    generateOTPResponse.Status = false;
                    generateOTPResponse.Message = "OTP not send.";
                }
            }
            else
            {
                generateOTPResponse.Status = false;
                generateOTPResponse.Message = "Template Not Saved yet";
            }
            return generateOTPResponse;
        }

        public async Task<GRPCReply<OrderToken>> ValidateOrderOTPGetToken(string TransactionReqNo, string otp)
        {
            GRPCReply<OrderToken> gRPCReply = new GRPCReply<OrderToken>();
            gRPCReply.Status = false;
            gRPCReply.Message = "Invalid OTP. Please enter correct OTP.";

            GRPCReply<OrderOTPScreenResponse> res = await GetByTransactionReqNoForOTP(new GRPCRequest<string>() { Request = TransactionReqNo });
            if (res.Status)
            {
                if (res.Response != null && !string.IsNullOrEmpty(res.Response.MobileNo))
                {
                    var reply = await _iCommunicationService.ValidateOTP(new ValidateOTPRequest { MobileNo = res.Response.MobileNo, OTP = otp });
                    if (reply.Status)
                    {
                        var result = await _iIdentityService.GetCustomerToken(res.Response.MobileNo);
                        if (result.Status)
                        {
                            var anchorReply = await _iCompanyService.GetCustomerDetails();
                            gRPCReply.Status = true;
                            gRPCReply.Message = "OTP Validate Succussfully.";
                            gRPCReply.Response = new OrderToken
                            {
                                Token = result.UserToken,
                                TransactionReqNo = TransactionReqNo,
                                CustomerCareEmail = anchorReply.Status == true && anchorReply.Response != null ? anchorReply.Response.CustomerCareEmail : "",
                                CustomerCareMoblie = anchorReply.Status == true && anchorReply.Response != null ? anchorReply.Response.CustomerCareMobile : "",
                                CustomerName = res.Response.CustomerName,
                                ImageUrl = res.Response.ImageUrl
                            };
                        }
                    }
                }

            }
            return gRPCReply;

        }


        public async Task<string> getToten(string mobileNo)
        {
            var result = await _iIdentityService.GetCustomerToken(mobileNo);
            return result.UserToken;
        }

        public async Task<GRPCReply<OrderOTPScreenResponse>> GetByTransactionReqNoForOTP(GRPCRequest<string> request)
        {
            GRPCReply<OrderOTPScreenResponse> gRPCReply = new GRPCReply<OrderOTPScreenResponse>();
            gRPCReply.Status = false; gRPCReply.Message = "Transaction data not found";

            var tokenRes = await _ayeFinanceSCFManger.GetNbfcToken();
            var NBFCToken = tokenRes.Status ? tokenRes.Response : "";
            var LoanAccount = await _iloanAccountService.GetNBFCLoanAccountDetailByTxnId(new GRPCRequest<GetAyeNBFCLoanAccountDetailByTxnIdDTO>
            {
                Request = new GetAyeNBFCLoanAccountDetailByTxnIdDTO
                {
                    NBFCToken = NBFCToken,
                    TransactionReqNo = request.Request
                }
            });
            if (LoanAccount != null && LoanAccount.Response.AnchorCompanyId > 0)
            {
                var anchorReply = await _iCompanyService.GetCustomerDetails();
                gRPCReply.Response = new OrderOTPScreenResponse
                {
                    MobileNo = LoanAccount.Response.MobileNo,
                    CustomerName = LoanAccount.Response.CustomerName,
                    ImageUrl = LoanAccount.Response.ImageUrl,
                    CustomerCareEmail = anchorReply.Status == true ? anchorReply.Response.CustomerCareEmail : "",
                    CustomerCareMoblie = anchorReply.Status == true ? anchorReply.Response.CustomerCareMobile : ""
                };
                gRPCReply.Status = true; gRPCReply.Message = "success";

            }
            return gRPCReply;
        }

        public async Task<GRPCReply<LoanAccountReplyDC>> GetLoanAccountById(string userId)
        {
            GRPCRequest<string> gRPCRequest = new GRPCRequest<string>();
            gRPCRequest.Request = userId;

            GRPCReply<LoanAccountReplyDC> gRPCReply = new GRPCReply<LoanAccountReplyDC>();
            gRPCReply.Status = false; gRPCReply.Message = "data not found";

            var LoanAccountData = await _iloanAccountService.GetLoanAccountById(gRPCRequest);
            if (LoanAccountData != null && LoanAccountData.Response != null)
            {
                gRPCReply.Response = LoanAccountData.Response;
                gRPCReply.Status = true; gRPCReply.Message = "success";

            }
            return gRPCReply;
        }


        public async Task<SendSMSReply> SendInvoiceDisbursmentSMS(long BlackSoilAccountTransactionId)
        {
            SendSMSReply sendSMSReply = new SendSMSReply();

            var TemMasterResponse = await _iloanAccountService.GetLoanAccountNotificationTemplate(new TemplateMasterRequestDc
            {
                TemplateCode = TemplateEnum.SendInvoiceDisbursment.ToString()//"SendInvoiceDisbursment"
            });
            // by BlackSoilAccountTransactionId
            var disbursmentDetails = await _iloanAccountService.GetDisbursmentSMSDetail(
            new GRPCRequest<long>
            {
                Request = BlackSoilAccountTransactionId
            });
            if (disbursmentDetails != null && TemMasterResponse.Status && disbursmentDetails.Response.amount > 0 && disbursmentDetails.Response.DueDate.HasValue && !string.IsNullOrEmpty(disbursmentDetails.Response.OrderNo))
            {
                var custName = disbursmentDetails.Response.UserName.ToString().Split(" ");
                string SMS = TemMasterResponse.Response.Template;
                SMS = SMS.Replace("{#var1#}", String.Format("{0:0.##}", disbursmentDetails.Response.amount));
                SMS = SMS.Replace("{#var2#}", disbursmentDetails.Response.OrderNo.ToString());
                SMS = SMS.Replace("{#var3#}", disbursmentDetails.Response.DueDate.Value.ToString("dd/MM/yyyy"));
                SMS = SMS.Replace("{#var4#}", disbursmentDetails.Response.BankAccountNo.ToString());
                SMS = SMS.Replace("{#var5#}", (custName != null && custName.Any()) ? custName[0] : "Customer");
                var Sendsmsreply = await _iCommunicationService.SendSMS(new SendSMSRequest { ExpiredInMin = 10, MobileNo = disbursmentDetails.Response.MobileNo, OTP = "", routeId = ((Int32)SMSRouteEnum.OTP).ToString(), SMS = SMS, DLTId = TemMasterResponse.Response.DLTID });
                sendSMSReply.Status = Sendsmsreply.Status;
                sendSMSReply.Message = Sendsmsreply.Status ? "Send Invoice Disbursment SMS send Successfully." : " Send Invoice Disbursment SMS Failed";
            }
            else
            {
                sendSMSReply.Status = false;
                sendSMSReply.Message = "Template Not found";
            }
            return sendSMSReply;
        }

        public async Task<GRPCReply<long>> LoanAccountCompLead(long LeadId, long CompanyID)
        {
            var response = new GRPCReply<long>();
            GRPCRequest<SaveLoanAccountCompLeadReqDC> request = new GRPCRequest<SaveLoanAccountCompLeadReqDC>
            {
                Request = new SaveLoanAccountCompLeadReqDC
                {
                    LeadId = LeadId,
                    CompanyId = CompanyID,
                    SaveLoanAccountCompanyLeadListDC = new List<SaveLoanAccountCompanyLeadRequestDC>()
                }
            };

            var leadrequest = new GRPCRequest<long>();
            leadrequest.Request = LeadId;
            var lead = await _iLeadService.GetLeadInfoById(leadrequest);

            var anchorcompany = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = CompanyID });

            foreach (var company in lead.Response.LeadCompanies)
            {
                SaveLoanAccountCompanyLeadRequestDC RequestSaveLoanAccountCompLead = new SaveLoanAccountCompanyLeadRequestDC
                {
                    LoanAccountId = 0,
                    CompanyId = company.CompanyId,
                    LeadProcessStatus = company.LeadProcessStatus,
                    UserUniqueCode = company.UserUniqueCode != null ? company.UserUniqueCode : "",
                    AnchorName = anchorcompany.Response.CompanyName,
                    LogoURL = anchorcompany.Response.LogoURL,
                };
                request.Request.SaveLoanAccountCompanyLeadListDC.Add(RequestSaveLoanAccountCompLead);
            }
            var loanAccountReply = await _iloanAccountService.SaveLoanAccountCompLead(request);

            response.Status = lead.Status;
            response.Message = lead.Message;

            return response;
        }


        public async Task<bool> SendOverdueMessageJob()
        {
            var loanAccountResponse = await _iloanAccountService.GetOverdueAccounts();
            if (loanAccountResponse.Status)
            {
                var TemMasterResponse = await _iloanAccountService.GetLoanAccountNotificationTemplate(new TemplateMasterRequestDc
                {
                    TemplateCode = TemplateEnum.OverdueSMS.ToString() //"OverdueSMS"
                });
                if (TemMasterResponse.Status)
                {
                    foreach (var item in loanAccountResponse.Response)
                    {
                        string SMS = TemMasterResponse.Response.Template;
                        SMS = SMS.Replace("{#var1#}", item.CustomerName.ToString());
                        SMS = SMS.Replace("{#var2#}", String.Format("{0:0.##}", item.Amount));//    item.Amount.ToString());
                        SMS = SMS.Replace("{#var3#}", item.OrderNo.ToString());
                        SMS = SMS.Replace("{#var4#}", item.AnchorName.ToString());
                        var Sendsmsreply = await _iCommunicationService.SendSMS(new SendSMSRequest { ExpiredInMin = 10, MobileNo = item.MobileNo, OTP = "", routeId = ((Int32)SMSRouteEnum.OTP).ToString(), SMS = SMS, DLTId = TemMasterResponse.Response.DLTID });
                    }
                }
            }
            return true;
        }

        public async Task<bool> SendDueDisbursmentMessageJob()
        {
            var loanAccountResponse = await _iloanAccountService.GetDueDisbursmentDetails();

            if (loanAccountResponse.Status)
            {
                var TemMasterResponse = await _iloanAccountService.GetLoanAccountNotificationTemplate(new TemplateMasterRequestDc
                {
                    TemplateCode = TemplateEnum.SendInvoiceDisbursment.ToString() //"SendInvoiceDisbursment"
                });
                if (TemMasterResponse.Status)
                {
                    foreach (var item in loanAccountResponse.Response)
                    {
                        string SMS = TemMasterResponse.Response.Template;

                        SMS = SMS.Replace("{#var1#}", String.Format("{0:0.##}", item.amount));
                        SMS = SMS.Replace("{#var2#}", item.OrderNo.ToString());
                        SMS = SMS.Replace("{#var3#}", item.DueDate?.ToString("dd MMM yyyy"));
                        SMS = SMS.Replace("{#var4#}", item.BankAccountNo.ToString());
                        SMS = SMS.Replace("{#var5#}", item.UserName.ToString());


                        var Sendsmsreply = await _iCommunicationService.SendSMS(new SendSMSRequest { ExpiredInMin = 10, MobileNo = item.MobileNo, OTP = "", routeId = ((Int32)SMSRouteEnum.OTP).ToString(), SMS = SMS, DLTId = TemMasterResponse.Response.DLTID });
                    }
                }
            }
            return true;
        }

        public async Task<GRPCReply<string>> AnchorOrderCompleted(OrderCompletedRequest orderCompletedRequest)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            TranMobileRequest tranMobileRequest = new TranMobileRequest { Condition = orderCompletedRequest.transactionNo, Type = "TransactionNo" };
            var LoanAccount = await _iloanAccountService.GetTransactionMobileNo(tranMobileRequest);
            var result = await _iIdentityService.GetCustomerToken(LoanAccount.Response);
            if (result.Status)
            {
                reply = await _iloanAccountService.AnchorOrderCompleted(orderCompletedRequest, result.UserToken);
            }
            return reply;
        }

        public async Task<LoanInvoiceReply> UpdateInvoiceInformation(InvoiceRequestDC request)
        {
            LoanInvoiceReply ret = new LoanInvoiceReply();
            TranMobileRequest tranMobileRequest = new TranMobileRequest { Condition = request.OrderNo, Type = "OrderNo" };
            var LoanAccount = await _iloanAccountService.GetTransactionMobileNo(tranMobileRequest);
            var result = await _iIdentityService.GetCustomerToken(LoanAccount.Response);
            if (result.Status)
            {
                var nbfctoken = await _ayeFinanceSCFManger.GetNbfcToken();
                request.ayeFinNBFCToken = nbfctoken.Response;

                var reply = await _iloanAccountService.UpdateInvoiceInformation(request, result.UserToken);
                ret.Message = reply.Message;
                ret.status = reply.status;
                ret.Result = reply.Result;
                if (reply.status && reply.Result != "Invoice already posted.")
                {
                    if (reply.AnchorCompanyId.HasValue)
                    {
                        var companyResponse = await _iCompanyService.GetCompanyShortName(new GRPCRequest<long> { Request = reply.AnchorCompanyId.Value });
                        if (companyResponse.Status)
                            reply.AnchorName = companyResponse.Response;
                    }

                    var TemMasterResponse = await _iloanAccountService.GetLoanAccountNotificationTemplate(new TemplateMasterRequestDc
                    {
                        TemplateCode = TemplateEnum.OrderDeliveryDisbursementSMS.ToString()//"OrderDeliveryDisbursementSMS"
                    });
                    if (TemMasterResponse.Status)
                    {
                        string SMS = TemMasterResponse.Response.Template;
                        var custName = reply.CustomerName.ToString().Split(" ");
                        if (!string.IsNullOrEmpty(SMS))
                        {
                            SMS = SMS.Replace("{#var1#}", (custName != null && custName.Any()) ? custName[0] : "Customer");
                            SMS = SMS.Replace("{#var2#}", request.OrderNo.ToString());
                            SMS = SMS.Replace("{#var3#}", reply.AnchorName);
                            var Sendsmsreply = await _iCommunicationService.SendSMS(new SendSMSRequest { ExpiredInMin = 10, MobileNo = reply.MobileNo, OTP = "", routeId = ((Int32)SMSRouteEnum.OTP).ToString(), SMS = SMS, DLTId = TemMasterResponse.Response.DLTID });
                        }
                    }
                }
            }
            return ret;
        }

        public async Task<bool> UpdateTransactionStatusJob()
        {
            await _iloanAccountService.UpdateTransactionStatusJob();
            return await SendOverdueMessageJob();
        }

        public async Task<GRPCReply<List<AnchorMISListResponseDC>>> GetAnchorMISList(AnchorMISRequestDC obj)
        {
            GRPCReply<List<AnchorMISListResponseDC>> gRPCReply = new GRPCReply<List<AnchorMISListResponseDC>>();
            var anchorMISListResponse = await _iloanAccountService.GetAnchorMISList(obj);

            if (anchorMISListResponse != null && anchorMISListResponse.Response != null && anchorMISListResponse.Response.Any() && anchorMISListResponse.Status)
            {
                GRPCRequest<List<long>> gRPCRequest = new GRPCRequest<List<long>> { Request = anchorMISListResponse.Response.Select(y => y.AnchorCompanyId).ToList() };
                var GetCompanyBankDetails = await _iCompanyService.GetCompanyBankDetailsById(gRPCRequest);

                var GetNBFCComapny = await _iCompanyService.GetNBFCCompanyName();

                if (GetCompanyBankDetails.Response != null && GetCompanyBankDetails.Response.Any() && GetCompanyBankDetails.Status)
                {
                    foreach (var response in anchorMISListResponse.Response)
                    {
                        var bankInfo = GetCompanyBankDetails.Response.Where(y => y.CompanyId == response.AnchorCompanyId).FirstOrDefault();

                        if (bankInfo != null)
                        {
                            response.BeneficiaryAccountNumber = bankInfo.bankAccountNumber;
                            response.BeneficiaryName = bankInfo.bankName;
                        }

                        var NBFCComapny = GetNBFCComapny.Response.Where(y => y.NBFCCompanyId == response.NBFCCompanyId).FirstOrDefault();

                        if (NBFCComapny != null)
                        {
                            response.NBFCName = NBFCComapny.NBFCCompanyName;
                        }
                    }
                }

                gRPCReply.Response = anchorMISListResponse.Response;

                //string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_AnchorMISListExport.zip";
                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_AnchorMISListExport.csv";
                //DataTable dt = ListtoDataTableConverter.ToDataTable(anchorMISListResponse.Response);

                //// rearrange DataTable columns
                //dt.Columns["Reference No."].SetOrdinal(0);
                //dt.Columns["Loan ID"].SetOrdinal(1);
                //dt.Columns["Anchor Name"].SetOrdinal(2);
                //dt.Columns["Order ID"].SetOrdinal(3);
                //dt.Columns["Invoice Status"].SetOrdinal(4);
                //dt.Columns["Invoice No."].SetOrdinal(5);
                //dt.Columns["Invoice Date"].SetOrdinal(6);
                //dt.Columns["Invoice Amt."].SetOrdinal(7);
                //dt.Columns["Business Name"].SetOrdinal(8);
                //dt.Columns["Anchor Code"].SetOrdinal(9);
                //dt.Columns["NBFC Name"].SetOrdinal(10);
                //dt.Columns["Disbursal Date"].SetOrdinal(11);
                //dt.Columns["Disbursal Amount"].SetOrdinal(12);
                //dt.Columns["Service Fee"].SetOrdinal(13);
                //dt.Columns["GST"].SetOrdinal(14);
                //dt.Columns["Beneficiary Name"].SetOrdinal(15);
                //dt.Columns["Beneficiary Account Number"].SetOrdinal(16);
                //dt.Columns["UTR"].SetOrdinal(17);
                //dt.Columns["Status"].SetOrdinal(18);
                DataTable dt = new DataTable();
                dt.Columns.Add("Reference No.");
                dt.Columns.Add("Loan ID");
                dt.Columns.Add("Anchor Name");
                dt.Columns.Add("Order ID");
                dt.Columns.Add("Invoice Status");
                dt.Columns.Add("Invoice No.");
                dt.Columns.Add("Invoice Date");
                dt.Columns.Add("Invoice Amt.");
                dt.Columns.Add("Business Name");
                dt.Columns.Add("Anchor Code");
                dt.Columns.Add("NBFC Name");
                dt.Columns.Add("Disbursal Date");
                dt.Columns.Add("Disbursal Amount");
                dt.Columns.Add("Service Fee");
                dt.Columns.Add("GST");
                dt.Columns.Add("Beneficiary Name");
                dt.Columns.Add("Beneficiary Account Number");
                dt.Columns.Add("UTR");
                dt.Columns.Add("Status");
                foreach (var item in anchorMISListResponse.Response)
                {
                    DataRow row = dt.NewRow();
                    row[0] = item.ReferenceId;
                    row[1] = item.LoanID;
                    row[2] = item.AnchorName;
                    row[3] = item.OrderNo;
                    row[4] = item.InvoiceStatus;
                    row[5] = item.InvoiceNo;
                    row[6] = item.InvoiceDate;
                    row[7] = item.InvoiceAmount;
                    row[8] = item.BusinessName;
                    row[9] = item.AnchorCode;
                    row[10] = item.NBFCName;
                    row[11] = item.DisbursementDate;
                    row[12] = item.DisbursalAmount;
                    row[13] = item.ServiceFee;
                    row[14] = item.GST;
                    row[15] = item.BeneficiaryName;
                    row[16] = item.BeneficiaryAccountNumber;
                    row[17] = item.UTR;
                    row[18] = item.Status;
                    dt.Rows.Add(row);
                }

                //ExcelGeneratorHelper.DataTable_To_Excel(dt, "UploadedWhatsAppExcel", filePath);
                //string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "AnchorMIS_ExcelGeneratePath");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string path = Path.Combine(folderPath, fileName);


                dt.WriteToCsvFile(path);
                gRPCReply.Message = $"/AnchorMIS_ExcelGeneratePath/{fileName}";
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<List<NbfcMisListResponseDc>>> GetNbfcMISList(NbfcMisListRequestDc obj)
        {
            GRPCReply<List<NbfcMisListResponseDc>> gRPCReply = new GRPCReply<List<NbfcMisListResponseDc>>();
            var nbfcMISListResponse = await _iloanAccountService.GetNbfcMISList(obj);

            if (nbfcMISListResponse != null && nbfcMISListResponse.Response != null && nbfcMISListResponse.Response.Any() && nbfcMISListResponse.Status)
            {
                gRPCReply.Response = nbfcMISListResponse.Response;

                var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_NbfcMISListExport.csv";
                DataTable dt = new DataTable();
                dt.Columns.Add("InvoiceNo");
                dt.Columns.Add("TransactionNo");
                dt.Columns.Add("InvoiceTranType");
                dt.Columns.Add("DisbursementDate");
                dt.Columns.Add("DueDate");
                dt.Columns.Add("SettlementDate");
                dt.Columns.Add("PrincipleAmount");
                dt.Columns.Add("TransAmount");
                dt.Columns.Add("PaidAmount");
                dt.Columns.Add("NBFCRate");
                dt.Columns.Add("AnchorRate");
                dt.Columns.Add("ScaleupRate");
                dt.Columns.Add("IsSharable");
                foreach (var item in nbfcMISListResponse.Response)
                {
                    DataRow row = dt.NewRow();
                    row[0] = item.InvoiceNo;
                    row[1] = item.TransactionNo;
                    row[2] = item.InvoiceTranType;
                    row[3] = item.DisbursementDate;
                    row[4] = item.DueDate;
                    row[5] = item.SettlementDate;
                    row[6] = item.PrincipleAmount;
                    row[7] = item.TransAmount;
                    row[8] = item.PaidAmount;
                    row[9] = item.NBFCRate;
                    row[10] = item.AnchorRate;
                    row[11] = item.ScaleupRate;
                    row[12] = item.IsSharable;
                    dt.Rows.Add(row);
                }

                //ExcelGeneratorHelper.DataTable_To_Excel(dt, "UploadedWhatsAppExcel", filePath);
                //string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "NbfcMIS_ExcelGeneratePath");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string path = Path.Combine(folderPath, fileName);


                dt.WriteToCsvFile(path);
                gRPCReply.Message = $"/NbfcMIS_ExcelGeneratePath/{fileName}";
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<List<DSAMISListResponseDC>>> GetDSAMISList(DSAMISRequestDC request)
        {
            GRPCReply<List<DSAMISListResponseDC>> gRPCReply = new GRPCReply<List<DSAMISListResponseDC>> { Message = "Data Not Found!!!" };
            var loanResponse = await _iloanAccountService.GetDSAMISLoanData(request);
            if (loanResponse != null && loanResponse.Status)
            {
                gRPCReply.Message = "Data Found";
                gRPCReply.Status = true;
                gRPCReply.Response = new List<DSAMISListResponseDC>();
                var leadIds = loanResponse.Response.Select(x => x.SalesAgentLeadId).ToList();
                var customerLeadIds = loanResponse.Response.Select(x => x.CustomerLeadId).ToList();
                leadIds.AddRange(customerLeadIds);
                var leadResponse = await _iLeadService.GetDSAMISLeadData(new GRPCRequest<List<long>> { Request = leadIds });
                foreach (var item in loanResponse.Response)
                {
                    DSAMISListResponseDC response = new DSAMISListResponseDC
                    {
                        State = "",
                        Location = "",
                        UserType = item.SalesAgentType,
                        SalesAgentName = item.SalesAgentName,
                        SalesAgentPanNo = "",
                        SalesAgentCode = "",
                        ScaleUpCode = "",
                        CustomerName = item.CustomerName,
                        LoginDate = null,
                        LAN = item.CustomerLoanAccountNo,
                        Lender = item.NBFCName,
                        DisbursedDate = item.DisbursmentDate,
                        SanctionAmount = item.SanctionAmount,
                        DisbursedAmount = item.DisbursementAmount,
                        PayoutPecentage = item.PayoutPercentage,
                        Amount = item.PayoutAmount,
                        GSTAmount = item.GSTAmount,
                        TotalAmount = item.TotalAmount,
                        TDSAmount = item.TDSAmount,
                        NetPayoutAmount = item.NetPayoutAmount
                    };
                    if (leadResponse != null && leadResponse.Status)
                    {
                        var salesAgentLead = leadResponse.Response.FirstOrDefault(x => x.LeadId == item.SalesAgentLeadId);
                        if (salesAgentLead != null)
                        {
                            response.State = salesAgentLead.StateName;
                            response.Location = salesAgentLead.CityName;
                            response.SalesAgentPanNo = salesAgentLead.PANNo;
                        }
                        var customerLead = leadResponse.Response.FirstOrDefault(x => x.LeadId == item.CustomerLeadId);
                        if (customerLead != null)
                        {
                            response.ScaleUpCode = customerLead.LeadCode;
                            response.LoginDate = customerLead.CreatedDate;
                        }
                    }
                    var salesAgentUserIds = loanResponse.Response.Select(x => x.SalesAgentUserId).ToList();
                    var productResponse = await _iProductService.GetSalesAgentDetailsByUserIds(new GRPCRequest<List<string>> { Request = salesAgentUserIds });
                    if (productResponse != null && productResponse.Response != null && productResponse.Response.Any())
                    {
                        var salesAgentProduct = productResponse.Response.FirstOrDefault(x => x.UserId == item.SalesAgentUserId);
                        if (salesAgentProduct != null)
                        {
                            response.SalesAgentCode = salesAgentProduct.DSACode;
                            if (item.SalesAgentType == LoanAccountUserTypeConstants.DSAUser)
                            {
                                response.LoginDate = salesAgentProduct.CreatedDate;
                            }
                        }
                    }
                    gRPCReply.Response.Add(response);
                }
            }
            return gRPCReply;
        }
        public async Task<LoanAccountDashboardResponse> ScaleupLoanAccountDashboardDetails(DashboardLoanAccountDetailDc req)
        {
            return await _iloanAccountService.ScaleupLoanAccountDashboardDetails(req);
        }
        public async Task<GRPCReply<List<LoanAccountListResponseDc>>> GetBusinessLoanAccountList(LoanAccountListRequestDc request)
        {
            if (request != null && request.ProductType != null)
            {
                LeadListPageRequest req = new LeadListPageRequest();
                req.ProductId = new List<long>();
                GRPCRequest<string> gRPCRequest = new GRPCRequest<string>();
                gRPCRequest.Request = request.ProductType;
                var productList = await _iProductService.GetProductByProductType(gRPCRequest);
                if (productList != null && productList.Response != null && productList.Status)
                {
                    foreach (var product in productList.Response)
                    {
                        req.ProductId.Add(product.ProductId);
                    }
                }
                req.UserIds = request.UserIds;
                if (request?.AnchorId != null)
                {
                    if (req?.CompanyId == null)
                    {
                        req.CompanyId = new List<int>();
                    }

                    foreach (var i in request.AnchorId)
                    {
                        req.CompanyId.Add((int)i);
                    }
                }
                request.leadIds = new List<long>();
                var productReply = await _iProductService.GetUsersIDS(req);
                if (productReply != null && productReply.Status && productReply.Response != null) request.UserIds = productReply.Response;
                if (request.UserIds != null && request.UserIds.Any())
                {
                    var leadReply = await _iLeadService.GetLeadByIDs(new GRPCRequest<List<string>> { Request = request.UserIds });
                    if (leadReply != null && leadReply.Status) request.leadIds = leadReply.Response;
                }
            }

            return await _iloanAccountService.GetBusinessLoanAccountList(request);

        }

        #region Product Configuration
        public async Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request)
        {
            var reply = new GRPCReply<bool>();
            var productReply = await _iProductService.AddUpdateAnchorProductConfig(request);
            if (productReply != null && productReply.Status)
            {
                reply = await _iloanAccountService.AddUpdateAnchorProductConfig(request);
            }
            return reply;
        }

        public async Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request)
        {
            var reply = new GRPCReply<bool>();
            var productReply = await _iProductService.AddUpdateNBFCProductConfig(request);
            if (productReply != null && productReply.Status)
            {
                reply = await _iloanAccountService.AddUpdateNBFCProductConfig(request);
            }
            return reply;
        }
        #endregion

        public async Task<GRPCReply<List<CompanyInvoicesChargesResponseDc>>> GetCompanyInvoiceCharges(CompanyInvoiceChargesRequestDc request)
        {
            return await _iloanAccountService.GetCompanyInvoiceCharges(request);
        }
        public async Task<GRPCReply<List<CompanyInvoicesListResponseDc>>> GetCompanyInvoiceList(CompanyInvoiceRequestDc request)
        {
            var res = await _iloanAccountService.GetCompanyInvoiceList(request);
            var GetNBFCComapny = await _iCompanyService.GetNBFCCompanyName();

            if (res.Response != null && res.Status && GetNBFCComapny.Response != null && GetNBFCComapny.Status)
            {
                foreach (var response in res.Response)
                {
                    var NBFCComapny = GetNBFCComapny.Response.Where(y => y.NBFCCompanyId == response.NBFCCompanyId).FirstOrDefault();

                    if (NBFCComapny != null)
                    {
                        response.NBFCName = NBFCComapny.NBFCCompanyName;
                        response.CompanyEmail = NBFCComapny.BusinessContactEmail;
                    }
                }
            }
            return res;
        }
        public async Task<GRPCReply<List<CompanyInvoiceDetailsResponseDc>>> GetCompanyInvoiceDetails(CompanyInvoiceDetailsRequestDC request)
        {
            var res = await _iloanAccountService.GetCompanyInvoiceDetails(request);
            if (res != null && res.Response != null && res.Status)
            {
                var GetNBFCComapny = await _iCompanyService.GetNBFCCompanyName();
                var anchorCompanyReply = await _iCompanyService.GetCompanyList(new CompanyListRequest
                {
                    CompanyType = CompanyTypeEnum.Anchor.ToString(),
                    keyword = "",
                    Skip = 0,
                    Take = 1000,
                });
                if (GetNBFCComapny.Response != null && GetNBFCComapny.Status && anchorCompanyReply.Status)
                {
                    foreach (var response in res.Response)
                    {
                        var NBFCCompany = GetNBFCComapny.Response.FirstOrDefault(y => y.NBFCCompanyId == response.NBFCCompanyId);
                        if (NBFCCompany != null)
                        {
                            response.NBFCName = NBFCCompany.NBFCCompanyShortName ?? NBFCCompany.NBFCCompanyName;
                        }

                        var AnchorCompany = anchorCompanyReply.CompanyList.FirstOrDefault(y => y.Id == response.AnchorCompanyId);
                        if (AnchorCompany != null)
                        {
                            response.AnchorName = AnchorCompany.landingName ?? AnchorCompany.businessName;
                        }
                    }
                }
            }
            return res;
        }

        public async Task<GRPCReply<UpdateCompanyInvoiceReplyDC>> UpdateCompanyInvoiceDetails(GRPCRequest<UpdateCompanyInvoiceRequestDC> request)
        {
            var res = await _iloanAccountService.UpdateCompanyInvoiceDetails(request);

            if (res.Status && res.Response.InvoicePdfdetailDC != null && res.Response.IsPDFGenerate && request.Request.UserType == CompanyInvoiceUserTypeConstants.CheckerUser && request.Request.IsApproved)
            {
                var companyReply = await GetCompanyAddressAndDetails(res.Response.InvoicePdfdetailDC.NBFCCompanyId);
                var scaleupData = await _iCompanyService.GetFinTechCompany();
                var scaleupreply = await GetCompanyAddressAndDetails(scaleupData.Response);
                var NBFCInvoiceUrlresponse = await _iCompanyService.GetFintechNBFCInvoiceUrl();
                var gstReply = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
                if (NBFCInvoiceUrlresponse != null && !string.IsNullOrEmpty(NBFCInvoiceUrlresponse.Response) && companyReply.Status)
                {
                    byte[] file = null;
                    string htmldata = "";
                    using (var wc = new WebClient())
                    {
                        file = wc.DownloadData(NBFCInvoiceUrlresponse.Response);
                    }

                    if (file.Length > 0)
                    {
                        htmldata = System.Text.Encoding.UTF8.GetString(file);

                        if (!string.IsNullOrEmpty(htmldata))
                        {
                            var InvoiceData = res.Response.InvoicePdfdetailDC;
                            var companyDetail = companyReply.Response;
                            var BillAddress = companyDetail.CompanyAddress.FirstOrDefault();
                            var ScaleupBillAddress = scaleupreply.Response.CompanyAddress.FirstOrDefault();
                            var ShippingAddress = companyDetail.CompanyAddress.FirstOrDefault();

                            htmldata = htmldata.Replace("[GSTNO]", companyDetail.GSTNo);
                            htmldata = htmldata.Replace("[COMPANYCODE]", companyDetail.CompanyCode);
                            htmldata = htmldata.Replace("[COMPANYNAME]", companyDetail.BusinessName);
                            htmldata = htmldata.Replace("[COMPANYMOBILE]", companyDetail.BusinessContactNo);

                            htmldata = htmldata.Replace("[INVOICENO]", InvoiceData.InvoiceNo);
                            htmldata = htmldata.Replace("[INVOICEDATE]", InvoiceData.InvoiceDate);
                            htmldata = htmldata.Replace("[INVOICEPERIOD]", InvoiceData.InvoicePeriodFrom);
                            //htmldata = htmldata.Replace("[INVOICEPERIODFROM]", InvoiceData.InvoicePeriodFrom);
                            //htmldata = htmldata.Replace("[INVOICEPERIODTO]", InvoiceData.InvoicePeriodTo);
                            htmldata = htmldata.Replace("[DUEDATE]", InvoiceData.DueDate);


                            string _AddressComplete = "";
                            if (BillAddress != null)
                            {
                                _AddressComplete += !string.IsNullOrEmpty(BillAddress.AddressTypeName) ? (BillAddress.AddressTypeName + "- ") : "";
                                _AddressComplete += !string.IsNullOrEmpty(BillAddress.AddressLineOne) ? (BillAddress.AddressLineOne + ", ") : "";
                                _AddressComplete += !string.IsNullOrEmpty(BillAddress.AddressLineTwo) ? (BillAddress.AddressLineTwo + ", ") : "";
                                _AddressComplete += !string.IsNullOrEmpty(BillAddress.AddressLineThree) ? (BillAddress.AddressLineThree + ", ") : "";
                                _AddressComplete += !string.IsNullOrEmpty(BillAddress.CityName) ? (BillAddress.CityName + ", ") : "";
                                _AddressComplete += !string.IsNullOrEmpty(BillAddress.StateName) ? (BillAddress.StateName + ", ") : "";
                                _AddressComplete += !string.IsNullOrEmpty(BillAddress.CountryName) ? (BillAddress.CountryName) : "";
                                _AddressComplete += (BillAddress.ZipCode != null && BillAddress.ZipCode > 0) ? ("-" + BillAddress.ZipCode.ToString()) : "";

                                htmldata = htmldata.Replace("[BILLADDRESS]", _AddressComplete);

                            }

                            if (ShippingAddress != null)
                            {
                                htmldata = htmldata.Replace("[SHIPPINGADDRESS]", _AddressComplete);
                            }
                            //InvoiceData.InvoiceRate = InvoiceData.InvoiceRate * gstReply.Response / 100;
                            string InvoiceDetail = "<tr><td>1.</td><td>Credit Facilitation Service Fees</td><td>1</td><td>" + InvoiceData.InvoiceRate.ToString("N2") + "</td><td></td><td>997119</td><td>" + gstReply.Response.ToString() + "%</td><td>" + InvoiceData.InvoiceRate.ToString("N2") + "</td></tr>";
                            htmldata = htmldata.Replace("[INVOICEDETAIL]", InvoiceDetail);
                            double Tax = 0;
                            if (BillAddress != null && ScaleupBillAddress != null && BillAddress.StateId == ScaleupBillAddress.StateId)
                            {
                                Tax = InvoiceData.InvoiceRate * (gstReply.Response / 2) / 100;

                                htmldata = htmldata.Replace("[CGST]", Tax.ToString("N2"));
                                htmldata = htmldata.Replace("[SGST]", Tax.ToString("N2"));
                                htmldata = htmldata.Replace("[IGST]", "0.00");
                                Tax += Tax;

                                htmldata = htmldata.Replace("[SAMESTATE]", "block");
                                htmldata = htmldata.Replace("[DIFFSTATE]", "none");

                            }
                            else
                            {
                                Tax = InvoiceData.InvoiceRate * (gstReply.Response) / 100;

                                htmldata = htmldata.Replace("[IGST]", Tax.ToString("N2"));
                                htmldata = htmldata.Replace("[SGST]", "0.00");
                                htmldata = htmldata.Replace("[CGST]", "0.00");

                                htmldata = htmldata.Replace("[SAMESTATE]", "none");
                                htmldata = htmldata.Replace("[DIFFSTATE]", "block");

                            }

                            var TotalAmtBeforeRoundOff = InvoiceData.InvoiceRate + Tax;
                            var TotalAmt = Math.Round(TotalAmtBeforeRoundOff, 0);
                            htmldata = htmldata.Replace("[RoundOff]", (TotalAmtBeforeRoundOff - TotalAmt).ToString("N2"));

                            htmldata = htmldata.Replace("[AMOUNTINWORD]", StringHelper.ConvertToWords(TotalAmt.ToString()));
                            htmldata = htmldata.Replace("[ACCOUNTHOLDERNAME]", scaleupreply.Response.AccountHolderName ?? "Scaleupfincap Private Limited");
                            htmldata = htmldata.Replace("[ACCOUNTNO]", scaleupreply.Response.BankAccountNumber);
                            htmldata = htmldata.Replace("[BANKIFSC]", scaleupreply.Response.BankIFSC);
                            htmldata = htmldata.Replace("[BANKNAME]", scaleupreply.Response.BankName);
                            htmldata = htmldata.Replace("[ACCOUNTTYPE]", scaleupreply.Response.AccountType);
                            htmldata = htmldata.Replace("[TOTALINVOICEAMT]", TotalAmt.ToString("N2"));

                            GRPCRequest<GRPCHtmlConvertRequest> convertRequest = new GRPCRequest<GRPCHtmlConvertRequest>
                            {
                                Request = new GRPCHtmlConvertRequest
                                {
                                    HtmlContent = htmldata
                                }
                            };
                            var data = await _iMediaService.HtmlToPdf(convertRequest);
                            if (!string.IsNullOrEmpty(data.Response.PdfUrl))
                            {
                                GRPCRequest<UpdateCompanyInvoicePDFDC> pdfRequest = new GRPCRequest<UpdateCompanyInvoicePDFDC>();
                                pdfRequest.Request = new UpdateCompanyInvoicePDFDC { CompanyInvoiceId = request.Request.CompanyInvoiceId, PDFPath = data.Response.PdfUrl };
                                var response = _iloanAccountService.UpdateCompnayInvoicePdfPath(pdfRequest);
                            }
                        }
                    }
                }
            }
            return res;
        }

        private async Task<CompanyAddressDetailsDTO> GetCompanyAddressAndDetails(long companyId)
        {
            CompanyAddressDetailsDTO reply = new CompanyAddressDetailsDTO();
            var companyReply = await _iCompanyService.GetCompanyAddressAndDetails(new CompanyAddressAndDetailsReq { CompanyId = companyId });
            var list = new List<GetAddressdc>();
            var partnerlist = new List<CompanyPartnersDc>();
            if (companyReply.Status)
            {
                if (companyReply.Response.companyLocationIds != null && companyReply.Response.companyLocationIds.Any())
                {
                    var locationAddressReply = await _iLocationService.GetCompanyAddress(companyReply.Response.companyLocationIds);
                    if (locationAddressReply.Status)
                    {
                        foreach (var _locationAdd in locationAddressReply.GetAddressDTO)
                        {
                            var obj = new GetAddressdc
                            {
                                AddressLineOne = _locationAdd.AddressLineOne,
                                AddressLineThree = _locationAdd.AddressLineThree,
                                AddressLineTwo = _locationAdd.AddressLineTwo,
                                AddressTypeId = _locationAdd.AddressTypeId,
                                AddressTypeName = _locationAdd.AddressTypeName,
                                CityId = _locationAdd.CityId,
                                CityName = _locationAdd.CityName,
                                CountryId = _locationAdd.CountryId,
                                CountryName = _locationAdd.CountryName,
                                Id = _locationAdd.Id,
                                StateId = _locationAdd.StateId,
                                StateName = _locationAdd.StateName,
                                ZipCode = _locationAdd.ZipCode
                            };
                            list.Add(obj);
                        }
                    }
                    else
                    {
                        reply.Status = locationAddressReply.Status;
                        reply.Message = locationAddressReply.Message;
                    }
                }
                if (companyReply.Response.CompanyPartnerDc != null)
                {
                    foreach (var _partner in companyReply.Response.CompanyPartnerDc)
                    {
                        var obj = new CompanyPartnersDc
                        {
                            PartnerId = _partner.PartnerId,
                            MobileNo = _partner.MobileNo,
                            PartnerName = _partner.PartnerName
                        };
                        partnerlist.Add(obj);
                    }
                }
                CompanyAddressDetails comp = new CompanyAddressDetails();
                comp.GSTDocId = companyReply.Response.GSTDocId;
                comp.PanNo = companyReply.Response.PanNo;
                comp.CancelChequeURL = companyReply.Response.CancelChequeURL != null ? companyReply.Response.CancelChequeURL : "";
                comp.CancelChequeDocId = companyReply.Response.CancelChequeDocId;
                comp.GSTNo = companyReply.Response.GSTNo;
                comp.CompanyCode = companyReply.Response.CompanyCode;
                comp.WhitelistURL = companyReply.Response.WhitelistIRL;
                comp.BankIFSC = companyReply.Response.BankIFSC;
                comp.BusinessHelpline = companyReply.Response.BusinessHelpline;
                comp.BusinessContactEmail = companyReply.Response.BusinessContactEmail;
                comp.BusinessPanURL = companyReply.Response.BusinessPanURL != null ? companyReply.Response.BusinessPanURL : "";
                comp.BankAccountNumber = companyReply.Response.BankAccountNumber;
                comp.BankName = companyReply.Response.BankName;
                comp.BusinessContactNo = companyReply.Response.BusinessContactNo;
                comp.BusinessTypeId = companyReply.Response.BusinessTypeId;
                comp.BusinessPanDocId = companyReply.Response.BusinessPanDocId;
                comp.AgreementDocId = companyReply.Response.AgreementDocId;
                comp.AgreementEndDate = companyReply.Response.AgreementEndDate;
                comp.AgreementStartDate = companyReply.Response.AgreementStartDate;
                comp.AgreementURL = companyReply.Response.AgreementURL != null ? companyReply.Response.AgreementURL : "";
                comp.GSTDocumentURL = companyReply.Response.GSTDocumentURL != null ? companyReply.Response.GSTDocumentURL : "";
                comp.InterestRate = companyReply.Response.InterestRate;
                comp.APIKey = companyReply.Response.APIKey;
                comp.APISecretKey = companyReply.Response.APISecretKey;
                comp.BusinessName = companyReply.Response.BusinessName;
                comp.CompanyType = companyReply.Response.CompanyType;
                comp.ContactPersonName = companyReply.Response.ContactPersonName;
                comp.CustomerAgreementDocId = companyReply.Response.CustomerAgreementDocId;
                comp.CustomerAgreementURL = companyReply.Response.CustomerAgreementURL != null ? companyReply.Response.CustomerAgreementURL : "";
                comp.MSMEDocId = companyReply.Response.MSMEDocId;
                comp.IsDefault = companyReply.Response.IsDefault;
                comp.IsSelfConfiguration = companyReply.Response.IsSelfConfiguration;
                comp.LandingName = companyReply.Response.LandingName;
                comp.LogoURL = companyReply.Response.LogoURL != null ? companyReply.Response.LogoURL : "";
                comp.MSMEDocId = companyReply.Response.MSMEDocId;
                comp.MSMEDocumentURL = companyReply.Response.MSMEDocumentURL != null ? companyReply.Response.MSMEDocumentURL : "";
                comp.OfferMaxRate = companyReply.Response.OfferMaxRate;
                comp.CompanyStatus = companyReply.Response.CompanyStatus;
                comp.CompanyAddress = list;
                comp.PartnerList = partnerlist;
                comp.AccountType = companyReply.Response.AccountType;
                comp.PanDocId = companyReply.Response.PanDocId;
                comp.PanURL = companyReply.Response.PanURL;
                reply.Status = true;
                reply.Message = "Data found";
                reply.Response = comp;
            }
            else
            {
                reply.Status = companyReply.Status;
                reply.Message = companyReply.Message;
            }

            return reply;

        }

        public async Task<GRPCReply<string>> SendInvoiceEmail(SendInvoiceEmailDC request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            if (request != null && request.To != null && request.To.Any() && !string.IsNullOrEmpty(request.InvoiceUrl))
            {
                var mailReply = await _iCommunicationService.SendEmail(new SendEmailRequest
                {
                    BCC = "",
                    File = request.InvoiceUrl,
                    From = "",
                    Message = request.Body,
                    OTP = "",
                    Subject = request.Subject,
                    To = string.Join(';', request.To)
                });
                if (mailReply != null && mailReply.Status)
                {
                    reply.Status = true;
                    reply.Message = "Mail Sent Successfully";
                }
            }
            return reply;
        }
        public async Task<GRPCReply<List<GetInvoiceRegisterDataResponse>>> GetInvoiceRegisterData(GetInvoiceRegisterDataRequest request)
        {
            GRPCReply<List<GetInvoiceRegisterDataResponse>> reply = new GRPCReply<List<GetInvoiceRegisterDataResponse>> { Message = "Data not Found" };
            reply = await _iloanAccountService.GetInvoiceRegisterData(new GRPCRequest<GetInvoiceRegisterDataRequest>
            {
                Request = new GetInvoiceRegisterDataRequest
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                }
            });
            if (reply != null && reply.Status)
            {
                var NBFCReply = await GetCompanyAddressAndDetails(reply.Response.First().NBFCCompanyId);
                var scaleupId = await _iCompanyService.GetFinTechCompany();
                var scaleupreply = await GetCompanyAddressAndDetails(scaleupId.Response);
                var gstReply = await _iCompanyService.GetLatestGSTRateList(new GRPCRequest<List<string>> { Request = new List<string> { GstConstant.Gst18, GstConstant.Tds10 } });
                var nbfcAddress = NBFCReply.Response.CompanyAddress.FirstOrDefault();
                var scaleUpAddress = scaleupreply.Response.CompanyAddress.FirstOrDefault();
                double cgst = 0, sgst = 0, igst = 0;
                double tds10 = gstReply.Response.First(x => x.Code == GstConstant.Tds10).Value;
                if (nbfcAddress != null && scaleUpAddress != null && nbfcAddress.StateId == scaleUpAddress.StateId)
                {
                    cgst = gstReply.Response.First(x => x.Code == GstConstant.Gst18).Value / 2;
                    sgst = gstReply.Response.First(x => x.Code == GstConstant.Gst18).Value / 2;
                }
                else
                {
                    igst = gstReply.Response.First(x => x.Code == GstConstant.Gst18).Value;
                }
                foreach (var invoice in reply.Response)
                {
                    invoice.CustomerName = NBFCReply.Response.BusinessName;
                    invoice.CustomerCityState = nbfcAddress != null ? nbfcAddress.CityName + "-" + nbfcAddress.StateName : "";
                    invoice.CustomerGSTIN = NBFCReply.Response.GSTNo;
                    invoice.CGSTPercentage = cgst;
                    invoice.SGSTPercentage = sgst;
                    invoice.IGSTPercentage = igst;
                    invoice.CGSTAmount = cgst > 0 ? invoice.TaxableAmount * cgst / 100 : 0;
                    invoice.SGSTAmount = sgst > 0 ? invoice.TaxableAmount * sgst / 100 : 0;
                    invoice.IGSTAmount = igst > 0 ? invoice.TaxableAmount * igst / 100 : 0;
                    invoice.InvoiceAmount = invoice.TaxableAmount + invoice.CGSTAmount + invoice.SGSTAmount + invoice.IGSTAmount;
                    invoice.TDSAmount = tds10 > 0 ? invoice.InvoiceAmount * tds10 / 100 : 0;
                    invoice.NetReceivable = invoice.InvoiceAmount - invoice.TDSAmount;
                }
                reply.Response.Add(new GetInvoiceRegisterDataResponse
                {
                    InvoiceDate = "Total",
                    InvoiceNumber = "",
                    Month = "",
                    CustomerName = "",
                    CustomerCityState = "",
                    CustomerGSTIN = "",
                    Description = "",
                    SAC = "",
                    TaxableAmount = reply.Response.Sum(x => x.TaxableAmount),
                    CGSTPercentage = reply.Response.Max(x => x.CGSTPercentage),
                    SGSTPercentage = reply.Response.Max(x => x.SGSTPercentage),
                    IGSTPercentage = reply.Response.Max(x => x.IGSTPercentage),
                    CGSTAmount = reply.Response.Sum(x => x.CGSTAmount),
                    SGSTAmount = reply.Response.Sum(x => x.SGSTAmount),
                    IGSTAmount = reply.Response.Sum(x => x.IGSTAmount),
                    InvoiceAmount = reply.Response.Sum(x => x.InvoiceAmount),
                    TDSAmount = reply.Response.Sum(x => x.TDSAmount),
                    NetReceivable = reply.Response.Sum(x => x.NetReceivable),
                    Status = "",
                    NBFCCompanyId = 0
                });
            }
            return reply;
        }


        public async Task<bool> SalesAgentLoanDisbursmentJob()
        {
            var _getSalesAgentLoanDisbursment = await _iloanAccountService.GetSalesAgentLoanDisbursment();
            if (_getSalesAgentLoanDisbursment.Status)
            {
                GRPCRequest<List<string>> request = new GRPCRequest<List<string>>();
                request.Request = _getSalesAgentLoanDisbursment.Response.Select(x => x.LeadCreatedUserId).Distinct().ToList();
                var SalesAgentDetailList = await _iProductService.GetSalesAgentByUserId(request);
                if (SalesAgentDetailList.Status)
                {
                    var gstTdsReply = await _iCompanyService.GetLatestGSTRateList(new GRPCRequest<List<string>> { Request = new List<string> { GstConstant.Gst18, GstConstant.Tds5 } });
                    if (gstTdsReply != null && gstTdsReply.Response != null && gstTdsReply.Response.Any())
                    {
                        var loanResult = await _iloanAccountService.SaveSalesAgentLoanDisbursmentData(new GRPCRequest<SalesAgentDisbursmentDataDc>
                        {
                            Request = new SalesAgentDisbursmentDataDc
                            {
                                GetSalesAgentLoanDisbursments = _getSalesAgentLoanDisbursment.Response,
                                SalesAgentDetails = SalesAgentDetailList.Response,
                                GstRate = gstTdsReply.Response.First(x => x.Code == GstConstant.Gst18).Value,
                                TDSRate = gstTdsReply.Response.First(x => x.Code == GstConstant.Tds5).Value
                            }
                        });
                    }
                }

            }
            return true;
        }


        public async Task<bool> EmailAnchorMISDataJob()
        {
            var misData = await _iloanAccountService.GetEmailAnchorMISDataJob();
            if (misData != null && misData.Response != null && misData.Response.Any())
            {
                var companyIdList = misData.Response.Select(x => x.AnchorCompanyId).ToList();
                companyIdList.AddRange(misData.Response.Select(x => x.NBFCCompanyId).ToList());
                var companyDataList = await _iCompanyService.GetCompanyDetailList(new GRPCRequest<List<long>> { Request = companyIdList });
                if (companyDataList != null && companyDataList.Response != null && companyDataList.Response.Any())
                {
                    foreach (var anchorMis in misData.Response)
                    {
                        var anchorCompanyDetail = companyDataList.Response.FirstOrDefault(x => x.CompanyId == anchorMis.AnchorCompanyId && !string.IsNullOrEmpty(x.EmailAddress));
                        var nbfcCompanyDetail = companyDataList.Response.FirstOrDefault(x => x.CompanyId == anchorMis.NBFCCompanyId);
                        if (anchorCompanyDetail != null && nbfcCompanyDetail != null)
                        {
                            anchorMis.AnchorMISExcelDatas.ForEach(x =>
                            {
                                x.AnchorName = anchorCompanyDetail.LendingName;
                                x.BeneficiaryName = anchorCompanyDetail.BeneficiaryName;
                                x.BeneficiaryAccountNumber = anchorCompanyDetail.BeneficiaryAccountNumber;
                                x.NBFCName = nbfcCompanyDetail.BusinessName;
                            });
                            var misDatatable = ClassToDataTable.CreateDataTable(anchorMis.AnchorMISExcelDatas);
                            if (misDatatable != null)
                            {
                                var fileName = "AnchorMISData_" + DateTime.Now.ToString("yyyyddMHHmmss") + ".csv";
                                string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "EmailAnchorMIS_ExcelGeneratePath");
                                if (!Directory.Exists(folderPath))
                                {
                                    Directory.CreateDirectory(folderPath);
                                }
                                string path = Path.Combine(folderPath, fileName);

                                misDatatable.WriteToCsvFile(path);

                                byte[] fileBytes = File.ReadAllBytes(path);

                                FileUploadRequest f = new FileUploadRequest();
                                f.FileStream = fileBytes;
                                f.SubFolderName = "EmailAnchorMIS_ExcelGeneratePath";
                                f.FileExtensionWithoutDot = "csv";
                                GRPCRequest<FileUploadRequest> filerequest = new GRPCRequest<FileUploadRequest> { Request = f };

                                var res = await _iMediaService.SaveFile(filerequest);
                                if (res != null && res.Response != null && !string.IsNullOrEmpty(res.Response.ImagePath))
                                {
                                    await _iCommunicationService.SendEmail(new SendEmailRequest
                                    {
                                        BCC = "",
                                        File = res.Response.ImagePath,
                                        From = "",
                                        Message = "",
                                        OTP = "",
                                        Subject = "AnchorMISData",
                                        To = anchorCompanyDetail.EmailAddress
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        public async Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            var res = await _iloanAccountService.BLoanEMIPdf(request);
            if (res != null && res.Response != null && res.Status)
            {
                {
                    GRPCRequest<GRPCHtmlConvertRequest> convertRequest = new GRPCRequest<GRPCHtmlConvertRequest> { Request = new GRPCHtmlConvertRequest { HtmlContent = res.Response } };
                    var mediaResponse = await _iMediaService.HtmlToPdf(convertRequest);
                    if (mediaResponse != null && mediaResponse.Response != null && !string.IsNullOrEmpty(mediaResponse.Response.PdfUrl))
                    {
                        reply.Response = mediaResponse.Response.PdfUrl;
                        reply.Status = true;
                        reply.Message = "success";
                    }
                }
            }
            return reply;
        }


        public async Task<GRPCReply<string>> ValidateOrderOtp(PostOrderPlacementRequestDTO order)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            gRPCReply.Status = false;
            gRPCReply.Message = "Invalid OTP. Please enter correct OTP.";

            var reply = await _iCommunicationService.ValidateOTP(new ValidateOTPRequest { MobileNo = order.MobileNo, OTP = order.otp });
            if (reply.Status)
            {
                OrderPlacementRequestDTO orderplace = new OrderPlacementRequestDTO()
                {
                    Amount = order.Amount,
                    CreditDay = order.CreditDay,
                    LoanAccountId = order.LoanAccountId,
                    TransactionReqNo = order.TransactionReqNo
                };

                var orderReply = await PostOrderPlacement(orderplace);
                if (orderReply != null && orderReply.Status)
                {
                    gRPCReply.Status = true;
                    gRPCReply.Message = orderReply.Message;
                    gRPCReply.Response = "";
                }
                else
                {
                    gRPCReply.Status = true;
                    gRPCReply.Message = orderReply.Message;
                    gRPCReply.Response = "";
                }
            }
            return gRPCReply;

        }
        public async Task<GRPCReply<OrderToken>> ValidateLoginPinGetToken(string mobileNo, string pinNumber)
        {
            GRPCReply<OrderToken> gRPCReply = new GRPCReply<OrderToken>();
            gRPCReply.Status = false;
            gRPCReply.Message = "Invalid PIN. Please enter correct PIN.";
            //validate Pin

            if (true)
            {
                var result = await _iIdentityService.GetCustomerToken(mobileNo);
                if (result.Status)
                {
                    gRPCReply.Status = true;
                    gRPCReply.Message = "PIN Validate Successfully.";
                    gRPCReply.Response = new OrderToken
                    {
                        Token = result.UserToken
                    };
                }
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "";
            }
            return gRPCReply;

        }

        public async Task<bool> ResentOrderLoginOTP(string TransactionReqNo)
        {
            GRPCReply<OrderOTPScreenResponse> res = await GetByTransactionReqNoForOTP(new GRPCRequest<string>() { Request = TransactionReqNo });
            if (res.Status)
            {
                if (res.Response != null && !string.IsNullOrEmpty(res.Response.MobileNo))
                {
                    var smsResult = await GenerateOrderOtpForLogin(res.Response.MobileNo);
                    return smsResult.Status;
                }
                else { return false; }
            }
            else { return false; }
        }

        private async Task<GenerateOTPResponse> GenerateOrderOtpForLogin(string MobileNo)
        {
            GenerateOTPResponse generateOTPResponse = new GenerateOTPResponse();
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string otp = GenerateRandomNumber.GenerateRandomOTP(6, saAllowedCharacters);
            //TemplateMasters
            var TemMasterResponse = await _iloanAccountService.GetLoanAccountNotificationTemplate(new TemplateMasterRequestDc
            {
                TemplateCode = TemplateEnum.OrderPlaceLoginOTPSMS.ToString()//"OrderPlaceLoginOTPSMS"
            });
            if (TemMasterResponse.Status)
            {
                var SMS = TemMasterResponse.Response.Template;
                var DLTId = TemMasterResponse.Response.DLTID;
                SMS = SMS.Replace("{#var1#}", otp);

                var Sendsmsreply = await _iCommunicationService.SendSMS(new SendSMSRequest { ExpiredInMin = 10, MobileNo = MobileNo, OTP = otp, routeId = ((Int32)SMSRouteEnum.OTP).ToString(), SMS = SMS, DLTId = DLTId });
                if (Sendsmsreply.Status)
                {
                    generateOTPResponse.OTP = otp;
                    generateOTPResponse.Status = true;
                    generateOTPResponse.Message = "OTP send successfully.";
                }
                else
                {
                    generateOTPResponse.Status = false;
                    generateOTPResponse.Message = "OTP not send.";
                }
            }
            else
            {
                generateOTPResponse.Status = false;
                generateOTPResponse.Message = "Template Not Saved yet";
            }
            return generateOTPResponse;
        }

        public async Task<GRPCReply<bool>> SettleCompanyInvoiceTransactions(SettleCompanyInvoiceTransactionsRequest request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            reply = await _iloanAccountService.AddInvoiceSettlementData(new GRPCRequest<SettleCompanyInvoiceTransactionsRequest>
            {
                Request = request
            });
            return reply;
        }

        public async Task<bool> SettlePaymentLaterJob()
        {
            var loanReply = await _iloanAccountService.GetSettlePaymentJobData();
            List<CompanyConfigReply> CompanyConfigList = new List<CompanyConfigReply>();
            if (loanReply != null && loanReply.Status)
            {
                foreach (var item in loanReply.Response)
                {
                    var productReply = await _iProductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                    {
                        AnchorCompanyId = item.AnchorCompanyId,
                        NBFCCompanyId = item.NBFCCompanyId,
                        ProductId = item.ProductId,
                    });
                    CompanyConfigList.Add(productReply);
                }
            }
            var fintechCompany = await _iCompanyService.GetFinTechCompany();
            var gstRate = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
            var request = new GRPCRequest<SettlePaymentJobRequest>();
            if (CompanyConfigList != null && CompanyConfigList.Any())
            {
                request.Request = new SettlePaymentJobRequest
                {
                    AnchorCompanyConfigs = CompanyConfigList.Where(x => x.AnchorCompanyConfig != null).Select(x => new AnchorCompanyConfigsDc
                    {
                        AnchorCompanyId = x.AnchorCompanyConfig.CompanyId,
                        ProductId = x.AnchorCompanyConfig.ProductId,
                        AnnualInterestRate = x.AnchorCompanyConfig.AnnualInterestRate ?? 0,
                        BounceCharge = x.AnchorCompanyConfig.BounceCharge,
                        DelayPenaltyRate = x.AnchorCompanyConfig.DelayPenaltyRate
                    }).Distinct().ToList(),
                    NBFCCompanyConfigs = CompanyConfigList.Where(x => x.NBFCCompanyConfig != null).Select(x => new NBFCCompanyConfigsDc
                    {
                        NBFCCompanyId = x.NBFCCompanyConfig.CompanyId,
                        ProductId = x.NBFCCompanyConfig.ProductId,
                        AnnualInterestRate = x.NBFCCompanyConfig.InterestRate,
                        BounceCharge = x.NBFCCompanyConfig.BounceCharges,
                        PenaltyCharges = x.NBFCCompanyConfig.PenaltyCharges,
                        IsInterestRateCoSharing = x.NBFCCompanyConfig.IsInterestRateCoSharing,
                        IsBounceChargeCoSharing = x.NBFCCompanyConfig.IsBounceChargeCoSharing,
                        IsPenaltyChargeCoSharing = x.NBFCCompanyConfig.IsPenaltyChargeCoSharing
                    }).Distinct().ToList(),
                    FintechCompanyId = fintechCompany.Response,
                    GSTRate = gstRate.Response
                };

            }
            await _iloanAccountService.SettlePaymentLaterJob(request);
            return true;
        }


        public async Task<bool> InsertScaleUpShareTransactions()
        {
            var loanReply = await _iloanAccountService.GetScaleUpShareCompanyData();
            List<CompanyConfigReply> CompanyConfigList = new List<CompanyConfigReply>();
            if (loanReply != null && loanReply.Status)
            {
                foreach (var item in loanReply.Response)
                {
                    var productReply = await _iProductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                    {
                        AnchorCompanyId = item.AnchorCompanyId,
                        NBFCCompanyId = item.NBFCCompanyId,
                        ProductId = item.ProductId,
                    });
                    CompanyConfigList.Add(productReply);
                }
            }
            var fintechCompany = await _iCompanyService.GetFinTechCompany();
            var gstRate = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
            var request = new GRPCRequest<SettlePaymentJobRequest>();
            if (CompanyConfigList != null && CompanyConfigList.Any())
            {
                request.Request = new SettlePaymentJobRequest
                {
                    AnchorCompanyConfigs = CompanyConfigList.Where(x => x.AnchorCompanyConfig != null).Select(x => new AnchorCompanyConfigsDc
                    {
                        AnchorCompanyId = x.AnchorCompanyConfig.CompanyId,
                        ProductId = x.AnchorCompanyConfig.ProductId,
                        AnnualInterestRate = x.AnchorCompanyConfig.AnnualInterestRate ?? 0,
                        BounceCharge = x.AnchorCompanyConfig.BounceCharge,
                        DelayPenaltyRate = x.AnchorCompanyConfig.DelayPenaltyRate
                    }).Distinct().ToList(),
                    NBFCCompanyConfigs = CompanyConfigList.Where(x => x.NBFCCompanyConfig != null).Select(x => new NBFCCompanyConfigsDc
                    {
                        NBFCCompanyId = x.NBFCCompanyConfig.CompanyId,
                        ProductId = x.NBFCCompanyConfig.ProductId,
                        AnnualInterestRate = x.NBFCCompanyConfig.InterestRate,
                        BounceCharge = x.NBFCCompanyConfig.BounceCharges,
                        PenaltyCharges = x.NBFCCompanyConfig.PenaltyCharges,
                        IsInterestRateCoSharing = x.NBFCCompanyConfig.IsInterestRateCoSharing,
                        IsBounceChargeCoSharing = x.NBFCCompanyConfig.IsBounceChargeCoSharing,
                        IsPenaltyChargeCoSharing = x.NBFCCompanyConfig.IsPenaltyChargeCoSharing
                    }).Distinct().ToList(),
                    FintechCompanyId = fintechCompany.Response,
                    GSTRate = gstRate.Response
                };

            }
            await _iloanAccountService.InsertScaleUpShareTransactions(request);
            return true;
        }

        public async Task<GRPCReply<List<LoanAccountListResponseDc>>> GetNBFCBusinessLoanAccountList(LoanAccountListRequestDc request, long NbfcCompanyId)
        {
            GRPCReply<List<LoanAccountListResponseDc>> reply = new GRPCReply<List<LoanAccountListResponseDc>>();
            //string role = "";
            //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.AYEOperationExecutive.ToLower())))
            //    role = UserRoleConstants.AYEOperationExecutive;
            //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.MASOperationExecutive.ToLower())))
            //    role = UserRoleConstants.MASOperationExecutive;
            //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.MASOperationExecutive.ToLower())) &&
            //    UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.AYEOperationExecutive.ToLower())))
            //    role = "BothRole";
            request.NbfcCompanyId = NbfcCompanyId;

            if (request != null && request.ProductType != null)
            {
                reply = await _iloanAccountService.GetNBFCBusinessLoanAccountList(request);
            }

            return reply;
        }

        public async Task<DisbursementDashboardDataResponseDc> GetHopTrendDashboardData(HopDashboardRequestDc req)
        {
            var DashBoardData = await _iloanAccountService.GetHopTrendDashboardData(req);
            return DashBoardData;
        }

        public async Task<HopDisbursementDashboardResponseDc> GetHopDisbursementDashboard(HopDashboardRequestDc req)
        {
            var DashBoardData = await _iloanAccountService.GetHopDisbursementDashboard(req);
            var LoanData = await _iloanAccountService.GetLoanAccountData();
            if (LoanData.Status && LoanData.Response.Any())
            {
                var res = await _iLeadService.GetLeadCibilData(new GRPCRequest<List<long>> { Request = LoanData.Response });
                if (res.Status && res.Response != null)
                {
                    CibilData cibilData = new CibilData();
                    cibilData.CibilLessPercentage = res.Response.CibilLessPercentage;
                    cibilData.CibilGreaterPercentage = res.Response.CibilGreaterPercentage;
                    cibilData.CibilZeroPercentage = res.Response.CibilZeroPercentage;
                    DashBoardData.cibilData = cibilData;
                }
            }
            var GetTATData = await _iLeadService.GetDashBoardTATData(new GRPCRequest<DashboardTATLeadtDetailRequestDc>
            {
                //anchorid removed
                Request = new DashboardTATLeadtDetailRequestDc
                {
                    ProductType = req.ProductType,
                    ProductId = req.ProductId,
                    FromDate = req.FromDate,
                    ToDate = req.ToDate,
                    CityId = req.CityId,
                    CityName = req.CityName
                }
            });
            if (GetTATData.Status && GetTATData.Response != null)
            {
                CohortData cohortData = new CohortData();
                cohortData.SubmittedToAllApprovedTime = GetTATData.Response.SubmittedToAllApprovedTime;
                cohortData.AllApprovedToSendToLosTime = GetTATData.Response.AllApprovedToSendToLosTime;
                cohortData.InitiateSubmittedTime = GetTATData.Response.InitiateSubmittedTime;
                cohortData.SendToLosToOfferAcceptTime = GetTATData.Response.SendToLosToOfferAcceptTime;
                cohortData.OfferAcceptToAgreementTime = GetTATData.Response.OfferAcceptToAgreementTime;
                cohortData.AgreementToAgreementAcceptTime = GetTATData.Response.AgreementToAgreementAcceptTime;

                cohortData.SubmittedToAllApprovedTimeInHours = GetTATData.Response.SubmittedToAllApprovedTimeInHours;
                cohortData.AllApprovedToSendToLosTimeInHours = GetTATData.Response.AllApprovedToSendToLosTimeInHours;
                cohortData.InitiateSubmittedTimeInHours = GetTATData.Response.InitiateSubmittedTimeInHours;
                cohortData.SendToLosToOfferAcceptTimeInHours = GetTATData.Response.SendToLosToOfferAcceptTimeInHours;
                cohortData.OfferAcceptToAgreementTimeInHours = GetTATData.Response.OfferAcceptToAgreementTimeInHours;
                cohortData.AgreementToAgreementAcceptTimeInHours = GetTATData.Response.AgreementToAgreementAcceptTimeInHours;
                if (cohortData != null)
                {
                    DashBoardData.cohortData = cohortData;
                }
            }
            return DashBoardData;
        }

        public async Task<GRPCReply<LoanPaymentsResultDC>> GetLedger_RetailerStatement(long loanAccountId, DateTime FromDate, DateTime ToDate)
        {
            GRPCReply<LoanPaymentsResultDC> loanreply = new GRPCReply<LoanPaymentsResultDC>();
            loanreply = await _iloanAccountService.GetLedger_RetailerStatement(new GetLedger_RetailerStatementDC { loanAccountId = loanAccountId, FromDate = FromDate, ToDate = ToDate });
            if (loanreply != null && loanreply.Status)
            {
                var mediaReply = await _iMediaService.HtmlToPdf(new GRPCRequest<GRPCHtmlConvertRequest>
                {
                    Request = new GRPCHtmlConvertRequest { HtmlContent = loanreply.Response.HtmlContent, }
                });
                if (mediaReply != null)
                {
                    loanreply.Response.InvoicePdfPath = mediaReply.Response.PdfUrl;
                }
            }

            return loanreply;

        }

        public async Task<GRPCReply<bool>> InsertBLEmiTransactionsJob()
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool> { Response = true, Status = true };
            var companyReply = await _iCompanyService.GetAllNBFCCompany();
            if (companyReply != null && companyReply.Response != null && companyReply.Response.Any())
            {
                var nbfcIds = companyReply.Response.Select(x => x.NbfcId).ToList();
                var productReply = await _iProductService.GetBLNBFCConfigByCompanyIds(new GRPCRequest<List<long>> { Request = nbfcIds });
                if (productReply != null && productReply.Response != null && productReply.Response.Any())
                {
                    var loanReply = await _iloanAccountService.InsertBLEmiTransactionsJob(productReply);
                }
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> UploadBLInvoiceExcel(string fileUrl)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            var companyReply = await _iCompanyService.GetFinTechCompany();
            gRPCReply = await _iloanAccountService.UploadBLInvoiceExcel(new UploadBLInvoiceExcelReq
            {
                FileUrl = fileUrl,
                FinTechCompanyId = companyReply.Response
            });
            return gRPCReply;
        }

        public async Task<GRPCReply<BLDetailsDc>> GetBLDetails(long loanAccountId)
        {
            var loanReply = await _iloanAccountService.GetBLDetails(new GRPCRequest<long>
            {
                Request = loanAccountId
            });
            return loanReply;
        }

        public async Task<string> HtmlToPdfConvert(string fileUrl)
        {
            byte[] file = null;
            using (var wc = new WebClient())
            {
                file = wc.DownloadData(fileUrl);
            }
            string htmldata = System.Text.Encoding.UTF8.GetString(file);
            GRPCRequest<GRPCHtmlConvertRequest> convertRequest = new GRPCRequest<GRPCHtmlConvertRequest>
            {
                Request = new GRPCHtmlConvertRequest
                {
                    HtmlContent = htmldata
                }
            };
            var data = await _iMediaService.HtmlToPdf(convertRequest);
            return data.Response.PdfUrl;
        }




    }
}
