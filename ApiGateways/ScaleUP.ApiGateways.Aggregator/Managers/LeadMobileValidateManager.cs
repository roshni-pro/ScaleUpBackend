
using Azure.Core;
using MassTransit;
using MassTransit.Futures.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Azure;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.DTOs.DSA;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Security.Cryptography;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class LeadMobileValidateManager
    {
        private ILeadService _iLeadService;
        private IProductService _iProductService;
        private ICommunicationService _iCommunicationService;
        private IIdentityService _iIdentityService;
        private ICompanyService _iCompanyService;
        private INBFCService _iNBFCService;
        private KYCUserDetailManager _kYCUserDetailManager;
        private IKycGrpcService _kYCGrpcService;
        private ILocationService _iLocationService;
        private ILoanAccountService _iLoanAccountService;

        public LeadMobileValidateManager(ILeadService iLeadService, KYCUserDetailManager kYCUserDetailManager
, IProductService iProductService
            , ICommunicationService iCommunicationService, IIdentityService iIdentityService
            , ICompanyService iCompanyService, INBFCService iNBFCService
            , IKycGrpcService kycGrpcService
            , ILocationService locService
            , ILoanAccountService loanAccountService)
        {
            _iLeadService = iLeadService;
            _iProductService = iProductService;
            _iCommunicationService = iCommunicationService;
            _iIdentityService = iIdentityService;
            _iCompanyService = iCompanyService;
            _iNBFCService = iNBFCService;
            _kYCUserDetailManager = kYCUserDetailManager;
            _kYCGrpcService = kycGrpcService;
            _iLocationService = locService;
            _iLoanAccountService = loanAccountService;
        }

        public async Task<GRPCReply<InitiateLeadReply>> LeadInitiate(AnchorLeadInitiate initiateLeadDetail, string token)
        {
            GRPCReply<InitiateLeadReply> reply = new GRPCReply<InitiateLeadReply>();
            List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>>();

            var anchoreCompanyDetail = await _iCompanyService.GetCompanyDataByCode(new GRPCRequest<string> { Request = initiateLeadDetail.AnchorCompanyCode });
            var productId = await _iProductService.GetProductIdByCode(new GRPCRequest<string> { Request = initiateLeadDetail.ProductCode });
            if (anchoreCompanyDetail != null && anchoreCompanyDetail.Status && productId != null && productId.Status)
            {
                Claims.Add(new KeyValuePair<string, string>("companyid", anchoreCompanyDetail.Response.CompanyId.ToString()));
                Claims.Add(new KeyValuePair<string, string>("productid", productId.Response.ToString()));

                var user = await _iIdentityService.CreateUserWithToken(new CreateUserRequest
                {
                    EmailId = initiateLeadDetail.Email,
                    MobileNo = initiateLeadDetail.MobileNumber,
                    Password = "Sc@" + initiateLeadDetail.MobileNumber,
                    UserName = initiateLeadDetail.MobileNumber,
                    UserType = UserTypeConstants.Customer,
                    Claims = Claims
                });
                var companyreply = await _iCompanyService.GetFinTechCompany();
                long fintechCompanyId = 0;
                if (companyreply.Status)
                    fintechCompanyId = companyreply.Response;

                var prodActivities = await _iProductService.GetProductActivity(new LeadProductRequest
                {
                    CompanyId = fintechCompanyId,
                    ProductId = productId.Response,
                    AnchorCompanyId = anchoreCompanyDetail.Response.CompanyId,
                    CompanyType = CompanyTypeEnum.FinTech.ToString()
                });

                long? cityId = null;
                if (!string.IsNullOrEmpty(initiateLeadDetail.State) && !string.IsNullOrEmpty(initiateLeadDetail.City))
                {
                    var stateReply = await _iLocationService.GetStateByName(new BuildingBlocks.GRPC.Contracts.Location.DataContracts.GSTverifiedRequest
                    {
                        State = initiateLeadDetail.State
                    });
                    if (stateReply != null && stateReply.stateId > 0)
                    {
                        var cityReply = await _iLocationService.GetCityByName(new BuildingBlocks.GRPC.Contracts.Location.DataContracts.GSTverifiedRequest
                        {
                            stateId = stateReply.stateId,
                            City = initiateLeadDetail.City,
                            State = stateReply.stateName
                        });
                        if (cityReply != null && cityReply.Status && cityReply.Response.cityId > 0)
                        {
                            cityId = cityReply.Response.cityId;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(user.UserId))
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        token = user.UserToken;
                    }

                    InitiateLeadDetail leadDetail = new InitiateLeadDetail
                    {
                        AnchorCompanyId = anchoreCompanyDetail.Response.CompanyId,
                        AnchorCompanyCode = initiateLeadDetail.AnchorCompanyCode,
                        ProductCode = initiateLeadDetail.ProductCode,
                        AnchorCompanyName = anchoreCompanyDetail != null && anchoreCompanyDetail.Status ? anchoreCompanyDetail.Response.CompanyName : "",
                        ProductId = productId.Response,
                        UserId = user.UserId,
                        Email = initiateLeadDetail.Email,
                        CustomerReferenceNo = initiateLeadDetail.CustomerReferenceNo,
                        MobileNumber = initiateLeadDetail.MobileNumber,
                        VintageDays = initiateLeadDetail.VintageDays,
                        CityId = cityId,
                        ProductActivities = prodActivities.Status ? prodActivities.LeadProductActivity : new List<GRPCLeadProductActivity>(),
                        CustomerBuyingHistories = initiateLeadDetail.BuyingHistories != null ?
                                                   initiateLeadDetail.BuyingHistories.Select(x => new CustomerBuyingHistory
                                                   {
                                                       MonthFirstBuyingDate = x.MonthFirstBuyingDate,
                                                       MonthTotalAmount = x.MonthTotalAmount,
                                                       TotalMonthInvoice = x.TotalMonthInvoice
                                                   }).ToList() : new List<CustomerBuyingHistory>()
                    };

                    var leadresponse = await _iLeadService.LeadInitiate(leadDetail, token);
                    reply.Status = leadresponse.Status;
                    reply.Message = leadresponse.Message;
                    reply.Response = new InitiateLeadReply { LeadId = leadresponse.Response, CompanyId = anchoreCompanyDetail.Response.CompanyId, ProductId = productId.Response };

                }
                else
                {
                    reply.Status = false;
                    reply.Message = "Lead not generated.";
                }
            }
            else
            {
                reply.Status = false;
                reply.Message = "Lead not generated.";
            }

            return reply;
        }

        public async Task<GRPCReply<CompanyProductDetail>> GetCompanyProductDetail(string product, string company)
        {
            GRPCReply<CompanyProductDetail> reply = new GRPCReply<CompanyProductDetail>();
            CompanyProductDetail companyProductDetail = new CompanyProductDetail();
            // long productId = 0, companyId=0;
            if (long.TryParse(product, out long productId))
            {
                var productres = await _iProductService.GetProductCodeById(new GRPCRequest<long> { Request = productId });
                if (productres.Status)
                {
                    companyProductDetail.ProductId = productId;
                    companyProductDetail.ProductCode = productres.Response;
                }
            }
            else
            {
                var productres = await _iProductService.GetProductIdByCode(new GRPCRequest<string> { Request = product });
                if (productres.Status)
                {
                    companyProductDetail.ProductId = productres.Response;
                    companyProductDetail.ProductCode = product;
                }
            }
            if (long.TryParse(company, out long companyId))
            {
                var companyres = await _iCompanyService.GetCompanyCodeById(new GRPCRequest<long> { Request = companyId });
                if (companyres.Status)
                {
                    companyProductDetail.CompanyId = companyId;
                    companyProductDetail.CompanyCode = companyres.Response;
                }
            }
            else
            {
                var companyres = await _iCompanyService.GetCompanyDataByCode(new GRPCRequest<string> { Request = company });
                if (companyres.Status)
                {
                    companyProductDetail.CompanyId = companyres.Response.CompanyId;
                    companyProductDetail.CompanyCode = company;
                }
            }

            if (companyProductDetail.ProductId > 0 && !string.IsNullOrEmpty(companyProductDetail.ProductCode)
               && companyProductDetail.CompanyId > 0 && !string.IsNullOrEmpty(companyProductDetail.CompanyCode))
            {
                reply.Status = true;
                reply.Response = companyProductDetail;
            }
            else
            {
                reply.Message = "You are not authorize to access application.";
            }
            return reply;
        }

        public async Task<GenerateOTPResponse> GenerateOtp(string MobileNo, long companyId)
        {
            var existOtpDetail = await _iCommunicationService.ExistValidOTP(MobileNo);
            GenerateOTPResponse generateOTPResponse = new GenerateOTPResponse();
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string otp = GenerateRandomNumber.GenerateRandomOTP(6, saAllowedCharacters);
            //TemplateMasters
            var TemMasterResponse = await _iLeadService.GetLeadNotificationTemplate(new TemplateMasterRequestDc
            {
                TemplateCode = TemplateEnum.LoginOTPSMS.ToString()//"LoginOTPSMS"
            }); ;
            if (TemMasterResponse.Status)
            {
                var SMS = TemMasterResponse.Response.Template;
                var DLTId = TemMasterResponse.Response.DLTID;
                SMS = SMS.Replace("{#var1#}", otp);
                //SMS = SMS.Replace("{#var2#}", MobileNo);
                var Sendsmsreply = await _iCommunicationService.SendSMS(new SendSMSRequest { ExpiredInMin = 15, MobileNo = MobileNo, OTP = otp, routeId = ((Int32)SMSRouteEnum.OTP).ToString(), SMS = SMS, DLTId = DLTId });
                if (Sendsmsreply.Status)
                {
                    generateOTPResponse.OTP = (EnvironmentConstants.EnvironmentName.ToLower() != "production" ? otp : "");
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

        public async Task<LeadMobileResponse> LeadMobileValidate(LeadMobileValidate leadMobileValidate, string Token)
        {
            LeadMobileResponse leadMobileResponse = new LeadMobileResponse();
            var reply = await _iCommunicationService.ValidateOTP(new ValidateOTPRequest { MobileNo = leadMobileValidate.MobileNo, OTP = leadMobileValidate.OTP });
            if (reply.Status)
            {
                List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("companyid",leadMobileValidate.CompanyId.ToString()),
                 new KeyValuePair<string, string>("productid",leadMobileValidate.ProductId.ToString())
                };

                var anchoreCompanyDetail = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = leadMobileValidate.CompanyId });
                if (anchoreCompanyDetail != null && anchoreCompanyDetail.Status)
                {
                    var user = await _iIdentityService.CreateUserWithToken(new CreateUserRequest
                    {
                        EmailId = "",
                        MobileNo = leadMobileValidate.MobileNo,
                        Password = "Sc@" + leadMobileValidate.MobileNo,
                        UserName = leadMobileValidate.MobileNo,
                        UserType = UserTypeConstants.Customer,
                        Claims = Claims
                    });

                    if (!string.IsNullOrEmpty(user.UserId))
                    {
                        var companyreply = await _iCompanyService.GetFinTechCompany();
                        long fintechCompanyId = 0;
                        if (companyreply.Status)
                            fintechCompanyId = companyreply.Response;

                        var prodActivities = await _iProductService.GetProductActivity(new LeadProductRequest
                        {
                            CompanyId = fintechCompanyId,
                            ProductId = leadMobileValidate.ProductId,
                            AnchorCompanyId = leadMobileValidate.CompanyId,
                            CompanyType = CompanyTypeEnum.FinTech.ToString()
                        });



                        var leadresponse = await _iLeadService.GetLeadForMobile(new LeadMobileRequest
                        {
                            CompanyId = leadMobileValidate.CompanyId,
                            MobileNo = leadMobileValidate.MobileNo,
                            ProductId = leadMobileValidate.ProductId,
                            ActivityId = leadMobileValidate.ActivityId,
                            SubActivityId = leadMobileValidate.SubActivityId,
                            UserId = user.UserId,
                            ProductActivities = prodActivities.Status ? prodActivities.LeadProductActivity : new List<GRPCLeadProductActivity>(),
                            VintageDays = leadMobileValidate.VintageDays,
                            MonthlyAvgBuying = leadMobileValidate.MonthlyAvgBuying,
                            CompanyCode = leadMobileValidate.CompanyCode,
                            ProductCode = leadMobileValidate.ProductCode
                        }, (string.IsNullOrEmpty(Token) ? user.UserToken : Token));

                        if (leadresponse.Status)
                        {
                            leadMobileResponse.Status = true;
                            leadMobileResponse.Message = "OTP Validate Successfully.";
                            leadMobileResponse.LeadId = leadresponse.LeadId;
                            leadMobileResponse.UserId = user.UserId;
                            leadMobileResponse.ProductType = prodActivities.ProductType;
                            leadMobileResponse.UserTokan = (string.IsNullOrEmpty(Token) ? user.UserToken : Token);
                        }
                        else
                        {
                            leadMobileResponse.Status = false;
                            leadMobileResponse.Message = "Incorrect OTP.";
                        }
                    }
                    else
                    {
                        leadMobileResponse.Status = false;
                        leadMobileResponse.Message = "Incorrect OTP.";
                    }
                }
                else
                {
                    leadMobileResponse.Status = false;
                    leadMobileResponse.Message = "Anchor company not active.";
                }
            }
            else
            {
                leadMobileResponse.Status = false;
                if (reply.Message == "OTP expired. Please resend again")
                {
                    leadMobileResponse.Message = reply.Message;
                }
                else
                {
                    leadMobileResponse.Message = "Incorrect OTP.";
                }

            }

            return leadMobileResponse;

        }

        public async Task<GRPCReply<bool>> InitiateLeadOffer(InitiateLeadOfferRequestDC request)
        {
            var reply = new GRPCReply<bool>();
            reply.Message = "Something went wrong.";
            reply.Response = false;
            var LeadProdReply = await _iLeadService.GetLeadProductId(request.LeadId);
            if (LeadProdReply.Status)
            {
                var nbfcCompanies = await _iCompanyService.GetAllNBFCCompany();
                nbfcCompanies.Response = nbfcCompanies.Response.Where(x => request.CompanyIds.Contains(x.NbfcId)).ToList();
                if (nbfcCompanies != null && nbfcCompanies.Status)
                {
                    List<string> MasterCodeList = new List<string>
                    {
                         KYCMasterConstants.PAN,
                         KYCMasterConstants.PersonalDetail,
                         KYCMasterConstants.BuisnessDetail,
                         KYCMasterConstants.Aadhar,
                         KYCMasterConstants.Selfie,
                         KYCMasterConstants.BankStatementCreditLending,
                         KYCMasterConstants.MSME
                    };

                    var kYCUserDetail = await _kYCUserDetailManager.GetLeadDetailAll(LeadProdReply.Response.UserId, LeadProdReply.Response.ProductCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
                    if (kYCUserDetail != null && kYCUserDetail.panDetail != null && kYCUserDetail.PersonalDetail != null && kYCUserDetail.BuisnessDetail != null && kYCUserDetail.aadharDetail != null)
                    {
                        kYCUserDetail.UserId = LeadProdReply.Response.UserId;
                        GRPCRequest<UserDetailsReply> kYCUserrequest = new GRPCRequest<UserDetailsReply>();
                        kYCUserrequest.Request = kYCUserDetail;
                        kYCUserrequest.Request.LeadId = request.LeadId;
                        var nbfcRequestids = nbfcCompanies.Response.Select(x => x.NbfcId).ToList();
                        var offerCompanies = await _iProductService.GetOfferCompany(LeadProdReply.Response.ProductId, nbfcRequestids, LeadProdReply.Response.AnchorCompanyId);
                        if (offerCompanies.Status)
                        {
                            List<LeadNbfcResponse> leadNbfcResponses = new List<LeadNbfcResponse>();
                            foreach (var item in offerCompanies.Response.CompanyIds)
                            {

                                leadNbfcResponses.Add(new LeadNbfcResponse
                                {
                                    NbfcId = item,
                                    CompanyIdentificationCode = nbfcCompanies.Response.First(x => x.NbfcId == item).CompanyIdentificationCode
                                });
                            }
                            if (offerCompanies.Response.LeadNBFCSubActivity != null && offerCompanies.Response.LeadNBFCSubActivity.Any())
                            {
                                offerCompanies.Response.LeadNBFCSubActivity.ForEach(x =>
                                {
                                    x.LeadId = request.LeadId;
                                    x.IdentificationCode = nbfcCompanies.Response.First(z => z.NbfcId == x.NBFCCompanyId).CompanyIdentificationCode;
                                });
                            }

                            var grpcreply = await _iLeadService.InitiateLeadOffer(request.LeadId, leadNbfcResponses, kYCUserDetail, offerCompanies.Response.LeadNBFCSubActivity);

                            reply.Status = grpcreply.Status;
                            reply.Message = grpcreply.Status ? "Offer Initiated successfully" : grpcreply.Message;
                        }

                    }
                    else
                    {
                        reply.Message = "Kyc details are inappropriate";
                    }

                }
            }
            return reply;
        }
        public async Task<GRPCReply<bool>> GeneratedOffer(long LeadId = 0)
        {
            var reply = new GRPCReply<bool>();
            reply.Message = "Something went wrong.";
            reply.Response = false;
            var leadOffer = await _iLeadService.GetLeadNBFCId();
            if (leadOffer.Status)
            {
                var allNbfcCompanyIds = leadOffer.Response.Select(x => x.NBFCCompanyId).ToList();
                GRPCRequest<List<long>> nbfcRequest = new GRPCRequest<List<long>>();
                nbfcRequest.Request = allNbfcCompanyIds;
                var nbfcReply = await _iCompanyService.GetDefaultConfigNBFCCompany(nbfcRequest);

                if (nbfcReply.Status && nbfcReply.Response.Any())
                {
                    List<GenerateOfferRequest> GenerateOfferRequest = new List<GenerateOfferRequest>();
                    if (LeadId > 0)
                    {

                        GenerateOfferRequest = leadOffer.Response.Where(x => x.LeadId == LeadId && nbfcReply.Response.Contains(x.NBFCCompanyId)).Select(x => new GenerateOfferRequest
                        {
                            AvgMonthlyBuying = x.AvgMonthlyBuying,
                            CibilScore = x.CibilScore,
                            CustomerType = x.CustomerType,
                            LeadId = x.LeadId,
                            NBFCCompanyId = x.NBFCCompanyId,
                            VintageDays = x.VintageDays
                        }).ToList();
                    }
                    else
                    {
                        GenerateOfferRequest = leadOffer.Response.Where(x => nbfcReply.Response.Contains(x.NBFCCompanyId)).Select(x => new GenerateOfferRequest
                        {
                            AvgMonthlyBuying = x.AvgMonthlyBuying,
                            CibilScore = x.CibilScore,
                            CustomerType = x.CustomerType,
                            LeadId = x.LeadId,
                            NBFCCompanyId = x.NBFCCompanyId,
                            VintageDays = x.VintageDays
                        }).ToList();
                    }
                    if (GenerateOfferRequest.Any())
                    {
                        var companyselfofferlist = await _iNBFCService.GetCompanyselfOfferList(GenerateOfferRequest);
                        if (companyselfofferlist != null && companyselfofferlist.Status && companyselfofferlist.Response != null)
                        {
                            var updateleadoffer = await _iLeadService.UpdateleadOffer(companyselfofferlist.Response);
                            if (updateleadoffer.Status)
                            {
                                reply.Status = updateleadoffer.Status;
                                reply.Message = "Offer generated successfully";
                            }

                        }
                    }

                    var otherNbfcOffer = leadOffer.Response.Where(x => !nbfcReply.Response.Contains(x.NBFCCompanyId)).Select(x => new GenerateOfferRequest
                    {
                        AvgMonthlyBuying = x.AvgMonthlyBuying,
                        CibilScore = x.CibilScore,
                        CustomerType = x.CustomerType,
                        LeadId = x.LeadId,
                        NBFCCompanyId = x.NBFCCompanyId,
                        VintageDays = x.VintageDays
                    }).ToList();

                    if (otherNbfcOffer.Any())
                    {





                        //other nbfc offer code
                    }

                }
                else
                {

                    //other nbfc offer code
                }
            }
            return reply;
        }

        public async Task<GRPCReply<string>> GetCurrentNumber(string EntityName)
        {
            GRPCRequest<string> request = new GRPCRequest<string>();
            request.Request = EntityName;
            return await _iLeadService.GetCurrentNumber(request);
        }

        public async Task<GRPCReply<bool>> AcceptOffer(long leadId)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            GRPCRequest<long> leadrequest = new GRPCRequest<long>();
            leadrequest.Request = leadId;
            var leadreply = await _iLeadService.GetLeadOfferCompany(leadrequest);
            if (leadreply != null && leadreply.Status)
            {
                LeadProductRequest leadProductRequest = new LeadProductRequest
                {
                    CompanyId = leadreply.Response.OfferCompanyId,
                    CompanyType = CompanyTypeEnum.NBFC.ToString(),
                    ProductId = leadreply.Response.ProductId,
                };
                var prodReply = await _iProductService.GetNBFCCompanyProductActivity(leadProductRequest);

                if (prodReply != null && prodReply.Status && prodReply.LeadProductActivity.Any())
                {
                    var NBFCActivitylist = prodReply.LeadProductActivity.Select(x => new GRPCLeadProductActivity
                    {
                        ActivityMasterId = x.ActivityMasterId,
                        ProductId = leadreply.Response.ProductId,
                        LeadId = leadId,
                        Sequence = x.Sequence,
                        SubActivityMasterId = x.SubActivityMasterId,
                        ActivityName = x.ActivityName,
                        SubActivityName = x.SubActivityName,
                    }).ToList();
                    var offeracceptreply = await _iLeadService.AcceptOfferAndAddNBFCActivity(NBFCActivitylist);
                    if (offeracceptreply.Status)
                    {
                        reply.Status = true;
                        reply.Message = "Lead offer accepted.";
                    }
                    else
                    {
                        reply.Status = false;
                        reply.Message = "Lead offer not accepted.";
                    }
                }

            }

            return reply;
        }


        public async Task<GRPCReply<bool>> IsKycCompleted(string UserId)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            GRPCRequest<string> request = new GRPCRequest<string>();
            request.Request = UserId;
            reply = await _iLeadService.IsKycCompleted(request);
            return reply;

        }

        public async Task<GRPCReply<List<LeadNBFCSubActivityRequestDc>>> InsertLeadNBFCApiConfig(List<NBFCIdentificationCodes> NBFCIdentificationCode, long ProductId, long LeadId, long AnchorCompanyId)
        {
            GRPCReply<List<LeadNBFCSubActivityRequestDc>> gRPCReply = new GRPCReply<List<LeadNBFCSubActivityRequestDc>>();
            GRPCRequest<CompanyProductRequest> request = new GRPCRequest<CompanyProductRequest> { Request = new CompanyProductRequest { CompanyIds = NBFCIdentificationCode.Select(x => x.NBFCCompanyId).ToList(), ProductId = ProductId, AnchorCompanyId = AnchorCompanyId } };
            var Companyapiconfig = await _iProductService.GetCompanyApiConfig(request);

            if (Companyapiconfig.Status && Companyapiconfig.Response != null && Companyapiconfig.Response.Any())
            {
                Companyapiconfig.Response.ForEach(x =>
                {
                    x.LeadId = LeadId;
                    x.IdentificationCode = NBFCIdentificationCode.Where(y => y.NBFCCompanyId == x.NBFCCompanyId).Select(y => y.IdentificationCodes).FirstOrDefault();
                });
                gRPCReply.Response = Companyapiconfig.Response;
            }
            return gRPCReply;
        }

        public async Task<GenerateOTPResponse> GenerateOtpForEmail(string email)
        {
            GenerateOTPResponse generateOTPResponse = new GenerateOTPResponse();
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string otp = GenerateRandomNumber.GenerateRandomOTP(6, saAllowedCharacters);
            //TemplateMasters
            //var TemMasterResponse = await _iLeadService.GetLeadNotificationTemplate(new TemplateMasterRequestDc
            //{
            //    TemplateCode = "Lead Email otp"
            //});
            //if (TemMasterResponse.Status)
            //{
            //    var SMS = TemMasterResponse.Response.Template;
            //    SMS = SMS.Replace("{#var1#}", otp);

            //    var emailReply = await _iCommunicationService.SendOTPOnEmail(new SendEmailRequest
            //    {
            //        From = "",
            //        To = email,
            //        Message = $@"'{SMS}'  <br>
            //                                Please Verify Your OTP <br>
            //                                Regards:- ScaleUp",
            //        Subject = "Email OTP Verification",
            //        File = "",
            //        BCC = "",
            //        OTP = otp
            //    });

            //    generateOTPResponse.Status = emailReply.Status;
            //    generateOTPResponse.Message = emailReply.Message;
            //}
            //else
            //{
            //    generateOTPResponse.Status = false;
            //    generateOTPResponse.Message = "Template Not Found Yet";
            //}
            string SMS = "Thank you for choosing Scaleup! To complete your email verification process, please enter the OTP (One-Time Password) provided below:Verification OTP:";
            SMS = SMS + " " + otp;
            var emailReply = await _iCommunicationService.SendOTPOnEmail(new SendEmailRequest
            {
                From = "",
                To = email,
                Message = $@"{SMS}  <br>
                                            If you haven't initiated this process, please contact our support team immediately at +91-9981531563 <br>
                                            Best regards:- 
                                    <br>
                                            ScaleUp",
                Subject = "Email OTP Verification",
                File = "",
                BCC = "",
                OTP = otp
            });

            generateOTPResponse.Status = emailReply.Status;
            generateOTPResponse.Message = emailReply.Message;

            return generateOTPResponse;
        }

        public async Task<LeadEmailResponse> OTPValidateForEmail(LeadEmailValidate leadEmailValidate)
        {
            LeadEmailResponse leadEmailResponse = new LeadEmailResponse();
            var reply = await _iCommunicationService.EmailValidateOTP(new ValidateEmailRequest { Email = leadEmailValidate.Email, OTP = leadEmailValidate.OTP });
            if (reply.Status)
            {
                leadEmailResponse.Status = reply.Status;
                leadEmailResponse.Message = reply.Message;
            }
            else
            {
                leadEmailResponse.Status = false;
                leadEmailResponse.Message = "Incorrect OTP.";
            }

            return leadEmailResponse;

        }

        public async Task<GRPCReply<bool>> AddUpdateSelfConfiguration(List<DefaultOfferSelfConfigurationDTO> selfConfigList)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            GRPCRequest<List<DefaultOfferSelfConfigurationDc>> gRPCRequest = new GRPCRequest<List<DefaultOfferSelfConfigurationDc>>
            {
                Request = selfConfigList.Select(x => new DefaultOfferSelfConfigurationDc
                {
                    Id = x.Id,
                    CompanyId = x.CompanyId,
                    CustomerType = x.CustomerType,
                    IsActive = x.IsActive,
                    MaxCibilScore = x.MaxCibilScore,
                    MaxCreditLimit = x.MaxCreditLimit,
                    MaxVintageDays = x.MaxVintageDays,
                    MinCibilScore = x.MaxCibilScore,
                    MinCreditLimit = x.MinCreditLimit,
                    MinVintageDays = x.MinVintageDays,
                    MultiPlier = x.MultiPlier
                }).ToList()
            };
            var NBFCconfigReply = await _iNBFCService.AddUpdateSelfConfiguration(gRPCRequest);
            if (NBFCconfigReply.Status)
            {
                var leadResponse = await _iLeadService.AddUpdateSelfConfiguration(gRPCRequest);
                gRPCReply.Status = leadResponse.Status;
                gRPCReply.Message = leadResponse.Message;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<OfferListReply>>> GetOfferList(long LeadId)
        {
            var reply = new GRPCReply<List<OfferListReply>>();
            reply.Message = "Something went wrong.";
            reply.Status = false;
            var OfferListReply = await _iLeadService.GetOfferList(LeadId);
            if (OfferListReply.Status)
            {

                GRPCRequest<List<long>> request = new GRPCRequest<List<long>> { Request = OfferListReply.Response.Select(x => x.NBFCCompanyId).ToList() };
                var nbfcCompanies = await _iCompanyService.GetNBFCCompanyById(request);
                if (nbfcCompanies.Status)
                {
                    OfferListReply.Response.ForEach(x =>
                    {
                        x.NBFCName = nbfcCompanies.Response.Where(y => y.CompanyId == x.NBFCCompanyId).Select(x => x.BusinessName).FirstOrDefault();
                    });

                    reply.Response = OfferListReply.Response;
                    reply.Status = true;
                }
            }
            return reply;
        }

        public async Task<GRPCReply<string>> GetLeadPreAgreement(long LeadId)
        {
            var reply = new GRPCReply<string>();
            reply.Status = false;
            var LeadPreAgreement = await _iLeadService.GetLeadPreAgreement(LeadId);
            if (LeadPreAgreement.Status)
            {
                reply.Response = LeadPreAgreement.Response;
                reply.Status = true;

            }
            else
            {
                reply.Message = "Something went wrong.";
            }
            return reply;
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
        public async Task<GRPCReply<LoanAccount_DisplayDisbursalAggDTO>> GetLeadOffer(long leadId, double DisbursalAmount, string LeadCode, long companyId, long anchoreCompanyId)
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

        public async Task<GRPCReply<UpdateLeadOfferResponse>> UpdateLeadOffer(UpdateLeadOfferPostDc leadoffer)
        {
            GRPCRequest<UpdateLeadOfferRequest> leadofferrequest = new GRPCRequest<UpdateLeadOfferRequest>();
            leadofferrequest.Request = new UpdateLeadOfferRequest
            {
                LeadOfferId = leadoffer.LeadOfferId,
                InterestRate = leadoffer.interestRate,
                newOfferAmout = leadoffer.newOfferAmout
            };
            var leadofferreply = await _iLeadService.UpdateLeadOffer(leadofferrequest);
            if (leadofferreply != null && leadofferreply.Status && !string.IsNullOrEmpty(leadofferreply.Response.UserId))
            {
                await _kYCGrpcService.RemoveSecretInfo(new GRPCRequest<string> { Request = leadofferreply.Response.UserId });

                #region insert PFCollection Activity
                var leadreply = await _iLeadService.GetLeadOfferCompany(new GRPCRequest<long>
                {
                    Request = leadofferreply.Response.LeadId
                });
                if (leadreply != null && leadreply.Status)
                {
                    LeadProductRequest leadProductRequest = new LeadProductRequest
                    {
                        AnchorCompanyId = leadreply.Response.AnchorCompanyId,
                        CompanyId = leadreply.Response.OfferCompanyId,
                        CompanyType = CompanyTypeEnum.NBFC.ToString(),
                        ProductId = leadreply.Response.ProductId,
                        ActivityName = ActivityConstants.PFCollection.ToString()
                    };
                    var prodReply = await _iProductService.GetNBFCCompanyProductActivityByName(leadProductRequest);

                    if (prodReply != null && prodReply.Status && prodReply.LeadProductActivity.Any())
                    {
                        var NBFCActivitylist = prodReply.LeadProductActivity.Select(x => new GRPCLeadProductActivity
                        {
                            ActivityMasterId = x.ActivityMasterId,
                            ProductId = leadreply.Response.ProductId,
                            LeadId = leadofferreply.Response.LeadId,
                            Sequence = x.Sequence,
                            SubActivityMasterId = x.SubActivityMasterId,
                            ActivityName = x.ActivityName,
                            SubActivityName = x.SubActivityName,
                        }).ToList();
                        var offeracceptreply = await _iLeadService.AddNBFCActivity(NBFCActivitylist);
                    }
                }
                #endregion
            }
            return leadofferreply;
        }

        public async Task<List<LeadCompanyGenerateOfferNewDTO>> GetLeadActivityOfferStatusNew(GenerateOfferStatusPostDc obj)
        {
            if (obj.UserType.ToLower() != UserTypeConstants.AdminUser.ToLower() && obj.UserType.ToLower() != UserTypeConstants.SuperAdmin.ToLower())
            {
                return new List<LeadCompanyGenerateOfferNewDTO>();
            }
                GRPCRequest<GenerateOfferStatusPost> gRPCRequest = new GRPCRequest<GenerateOfferStatusPost>();
            var leadReply = await _iLeadService.GetLeadAllCompanyAsync(new GRPCRequest<LeadCompanyConfigProdRequest> { Request = new LeadCompanyConfigProdRequest { AnchorCompanyId = 0, LeadId = obj.LeadId } });
            if (leadReply != null)
            {
                var latestGST = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);

                var nbfcList = await _iCompanyService.GetAllNBFCCompany();
                var LeadProdReply = await _iLeadService.GetLeadProductId(obj.LeadId);
                //var companyConfigReply = await _iProductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                //{
                //    AnchorCompanyId = LeadProdReply.Response.AnchorCompanyId ?? 0,
                //    NBFCCompanyId = 0,
                //    ProductId = LeadProdReply.Response.ProductId
                //});

                var nbfcids = nbfcList.Response.Select(x => x.NbfcId).ToList();
                var productIds = LeadProdReply.Response.ProductId;

                var nbfcConfig = await _iProductService.GetProductNBFCConfigs(new GRPCRequest<GetProductNBFCConfigRequestDc>
                {
                    Request = new GetProductNBFCConfigRequestDc { NBFCCompanyIds = nbfcids, ProductId = productIds }
                });
                GenerateOfferStatusPost offer = new GenerateOfferStatusPost();
                offer.LeadId = obj.LeadId;
                offer.ProductType = obj.ProductType;
                offer.ProductCompanyConfigDcs = null;// companyConfigDc;
                offer.ProductSlabConfigs = null; //companyConfigReply.AnchorSlabConfigs;
                offer.GetProductNBFCConfigResponseDcs = nbfcConfig.Response;
                offer.GST = latestGST.Response;


                gRPCRequest.Request = offer;
            }
            var res = await _iLeadService.GetLeadActivityOfferStatusNew(gRPCRequest);
            if (res != null && res.Any())
            {
                var LeadProdReply = await _iLeadService.GetLeadOfferByLeadId(new GRPCRequest<GetGenerateOfferByFinanceRequestDc>
                {
                    Request = new GetGenerateOfferByFinanceRequestDc
                    {
                        LeadId = obj.LeadId,
                        //Role = "",
                        NbfcCompanyId = 0
                    }
                });

                foreach (var user in res)
                {
                    UserListDetailsRequest userReq = new UserListDetailsRequest
                    {
                        userIds = LeadProdReply.Response.Where(x => x.NBFCCompanyId == user.NbfcCompanyId).Select(y => y.LastModifyBy).ToList(),
                        keyword = null,
                        Skip = 0,
                        Take = 10
                    };
                    var userReply = await _iIdentityService.GetUserById(userReq);
                    var username = userReply.UserListDetails != null ? userReply.UserListDetails.Select(y => y.UserName).FirstOrDefault() : "";
                    user.UserName = username;
                }
            }
            return res;
        }

        public async Task<GRPCReply<bool>> GenerateLeadOfferByFinance(long LeadId, double CreditLimit)
        {
            var reply = new GRPCReply<bool>();
            reply.Message = "Something went wrong.";
            reply.Response = false;
            var LeadProdReply = await _iLeadService.GetLeadProductId(LeadId);
            if (LeadProdReply.Status)
            {
                var nbfcCompanies = await _iCompanyService.GetAllNBFCCompany();
                if (nbfcCompanies != null && nbfcCompanies.Status)
                {
                    List<string> MasterCodeList = new List<string>
                    {
                         KYCMasterConstants.PAN,
                         KYCMasterConstants.PersonalDetail,
                         KYCMasterConstants.BuisnessDetail,
                         KYCMasterConstants.Aadhar,
                         KYCMasterConstants.Selfie,
                         KYCMasterConstants.BankStatementCreditLending,
                         KYCMasterConstants.MSME
                    };

                    var kYCUserDetail = await _kYCUserDetailManager.GetLeadDetailAll(LeadProdReply.Response.UserId, LeadProdReply.Response.ProductCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
                    if (kYCUserDetail != null && kYCUserDetail.panDetail != null && kYCUserDetail.PersonalDetail != null && kYCUserDetail.BuisnessDetail != null && kYCUserDetail.aadharDetail != null)
                    {
                        kYCUserDetail.UserId = LeadProdReply.Response.UserId;
                        GRPCRequest<UserDetailsReply> kYCUserrequest = new GRPCRequest<UserDetailsReply>();
                        kYCUserrequest.Request = kYCUserDetail;
                        kYCUserrequest.Request.LeadId = LeadId;
                        var nbfcRequestids = nbfcCompanies.Response.Select(x => x.NbfcId).ToList();
                        var offerCompanies = await _iProductService.GetOfferCompany(LeadProdReply.Response.ProductId, nbfcRequestids, LeadProdReply.Response.AnchorCompanyId);
                        if (offerCompanies.Status)
                        {
                            List<LeadNbfcResponse> leadNbfcResponses = new List<LeadNbfcResponse>();
                            foreach (var item in offerCompanies.Response.CompanyIds)
                            {

                                leadNbfcResponses.Add(new LeadNbfcResponse
                                {
                                    NbfcId = item,
                                    CompanyIdentificationCode = nbfcCompanies.Response.FirstOrDefault(x => x.NbfcId == item).CompanyIdentificationCode
                                });
                            }

                            offerCompanies.Response.LeadNBFCSubActivity.ForEach(x =>
                            {
                                x.LeadId = LeadId;
                                x.IdentificationCode = nbfcCompanies.Response.FirstOrDefault(z => z.NbfcId == x.NBFCCompanyId).CompanyIdentificationCode;
                            });

                            var grpcreply = await _iLeadService.GenerateLeadOfferByFinance(LeadId, CreditLimit, leadNbfcResponses, kYCUserDetail, offerCompanies.Response.LeadNBFCSubActivity);
                            reply.Status = grpcreply.Status;
                            reply.Message = grpcreply.Status ? "Offer Generated successfully" : grpcreply.Message;
                        }
                    }
                    else
                    {
                        reply.Message = "Kyc details are inappropriate";
                    }
                }
            }
            return reply;
        }

        public async Task<GRPCReply<Loandetaildc>> GenerateOfferByFinance(GenerateOfferByFinanceRequestDc generateOfferByFinanceRequestDc)
        {
            Loandetaildc loandetaildc = new Loandetaildc();
            GRPCReply<Loandetaildc> reply = new GRPCReply<Loandetaildc>() { Message = "Data not Found!" };

            var latestGST = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
            var LeadProdReply = await _iLeadService.GetLeadProductId(generateOfferByFinanceRequestDc.LeadId);

            List<long> nbfcids = new List<long>();
            nbfcids.Add(generateOfferByFinanceRequestDc.NbfcCompanyId);

            var productIds = LeadProdReply.Response.ProductId;

            var nbfcConfig = await _iProductService.GetProductNBFCConfigs(new GRPCRequest<GetProductNBFCConfigRequestDc>
            {
                Request = new GetProductNBFCConfigRequestDc { NBFCCompanyIds = nbfcids, ProductId = productIds,OfferAmount = generateOfferByFinanceRequestDc.OfferAmount }
            });
            var companyConfigReply = nbfcConfig.Response[0];

            GRPCRequest<long> gRPCRequest = new GRPCRequest<long>();
            gRPCRequest.Request = generateOfferByFinanceRequestDc.LeadId;
            var leadData = await _iLeadService.GetCreditLimitByLeadId(gRPCRequest);

            if (companyConfigReply != null)
            {
                double? processing_fee = 0;
                double? ProcessingfeeTax = 0;
                double LoanAmount = 0;
                double RateOfInterest = 0;
                int Tenure = 0;
                double monthlyPayment = 0;
                double loanIntAmt = 0;
                double PFPer = 0;
                double GST = 0;

                var ProductSlabConfig = companyConfigReply.ProductSlabConfigs;
                if (ProductSlabConfig != null)
                {

                    var Anchorconfig = ProductSlabConfig.Where(x => x.MinLoanAmount <= generateOfferByFinanceRequestDc.OfferAmount && x.MaxLoanAmount >= generateOfferByFinanceRequestDc.OfferAmount).ToList();
                    if (Anchorconfig != null && Anchorconfig.Any())
                    {
                        LoanAmount = generateOfferByFinanceRequestDc.OfferAmount;
                        var commission = Anchorconfig.FirstOrDefault(x => x.SlabType == SlabTypeConstants.Commission);
                        var ROI = Anchorconfig.FirstOrDefault(x => x.SlabType == SlabTypeConstants.ROI);
                        var PF = Anchorconfig.FirstOrDefault(x => x.SlabType == SlabTypeConstants.PF);
                        RateOfInterest = ROI != null ? ROI.MaxValue : 0;
                        Tenure = Convert.ToInt32(companyConfigReply.Tenure);
                        monthlyPayment = Math.Round(Convert.ToDouble(CalculateMonthlyPayment(RateOfInterest, Tenure, LoanAmount)), 2);
                        loanIntAmt = Math.Round(((Convert.ToDouble(monthlyPayment) * Tenure) - LoanAmount), 2);
                        PFPer = Anchorconfig.First(x => x.SlabType == SlabTypeConstants.PF).MaxValue;
                        GST = latestGST.Response;
                        processing_fee = PF.ValueType == "Percentage" ? Math.Round((LoanAmount * PFPer) / 100, 2) : PFPer;
                        ProcessingfeeTax = Math.Round(Convert.ToDouble(processing_fee * GST) / 100, 2);

                        reply.Response = new Loandetaildc
                        {
                            LoanAmount = LoanAmount,
                            RateOfInterest = RateOfInterest,
                            Tenure = Tenure,
                            monthlyPayment = monthlyPayment,
                            loanIntAmt = loanIntAmt,
                            ProcessingFeeRate = PFPer,
                            GST = GST,
                            processing_fee = processing_fee.Value,
                            ProcessingfeeTax = ProcessingfeeTax.Value,
                            PFType = PF.ValueType,
                            Commission = commission != null ? commission.MaxValue : 0,
                            CommissionType = commission != null ? commission.ValueType : "",

                            ArrangementType = companyConfigReply.ArrangementType ?? "",
                            Bounce = companyConfigReply.MaxBounceCharge ,
                            Penal = companyConfigReply.MaxPenalPercent,
                            NBFCBounce = companyConfigReply.BounceCharge,
                            NBFCInterest = companyConfigReply.NBFCInterest,
                            NBFCPenal = companyConfigReply.PenalPercent,
                            NBFCProcessingFee = companyConfigReply.NBFCProcessingFee,
                            NBFCProcessingFeeType = companyConfigReply.NBFCProcessingFeeType
                        };
                        if (reply.Response != null)
                        {
                            reply.Status = true;
                            reply.Message = "Data Found";
                        }
                    }
                    else
                    {
                        reply.Status = false;
                        reply.Message = "Product slab not found";
                    }
                }
                else
                {
                    reply.Status = true;
                    reply.Message = "Configuration Not Found";
                }
            }
            return reply;
        }
        public async Task<GRPCReply<Loandetaildc>> GetGenerateOfferByFinance(GetGenerateOfferByFinanceRequestDc getGenerateOfferByFinanceRequestDc)
        {
            Loandetaildc loandetaildc = new Loandetaildc();
            GRPCReply<Loandetaildc> reply = new GRPCReply<Loandetaildc>() { Message = "Data not Found!" };

            var username = "";
            {
                var resp = await _iLeadService.GetNBFCOfferByLeadId(new GRPCRequest<GetGenerateOfferByFinanceRequestDc>
                {
                    Request = new GetGenerateOfferByFinanceRequestDc
                    {
                        LeadId = getGenerateOfferByFinanceRequestDc.LeadId,
                        //Role = getGenerateOfferByFinanceRequestDc.Role,
                        NbfcCompanyId = getGenerateOfferByFinanceRequestDc.NbfcCompanyId
                    }
                });

                if(resp.Status && resp.Response != null)
                {
                    UserListDetailsRequest userReq = new UserListDetailsRequest
                    {
                        userIds = new List<string>() { resp.Response.UserName },
                        keyword = null,
                        Skip = 0,
                        Take = 10
                    };

                    var userReply = await _iIdentityService.GetUserById(userReq);
                    username = userReply.UserListDetails != null ? userReply.UserListDetails.Select(y => y.UserName).FirstOrDefault() : "";

                    reply.Response = new Loandetaildc
                    {
                        LoanAmount = resp.Response.LoanAmount,
                        RateOfInterest = resp.Response.RateOfInterest,
                        Tenure = resp.Response.Tenure,
                        monthlyPayment = resp.Response.monthlyPayment,
                        loanIntAmt = resp.Response.loanIntAmt,
                        processing_fee = resp.Response.processing_fee,
                        ProcessingfeeTax = resp.Response.ProcessingfeeTax,
                        UserName = username,
                        CreatedDate = resp.Response.CreatedDate,
                        RejectionReason = resp.Response.RejectionReason,
                        offerInitiateDate=resp.Response.offerInitiateDate,
                    };
                }
                if (reply.Response != null)
                {
                    reply.Status = true;
                    reply.Message = "Data Found";
                }
                //}
            }
            return reply;
        }
        public static decimal CalculateMonthlyPayment(double yearlyinterestrate, int totalnumberofmonths, double loanamount)
        {
            if (yearlyinterestrate > 0)
            {
                var rate = (double)yearlyinterestrate / 100 / 12;
                var denominator = Math.Pow((1 + rate), totalnumberofmonths) - 1;
                return new decimal((rate + (rate / denominator)) * loanamount);
            }
            return totalnumberofmonths > 0 ? new decimal(loanamount / totalnumberofmonths) : 0;
        }

        public async Task<GRPCReply<bool>> UpdateBuyingHistory(UpdateBuyingHistoryRequest request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            reply = await _iLeadService.UpdateBuyingHistory(new GRPCRequest<UpdateBuyingHistoryRequest> { Request = request });
            return reply;
        }


        public async Task<GRPCReply<bool>> AddLeadOfferConfig(AddLeadOfferConfigRequest request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var productConfig = await _iProductService.GetProductSlabConfigs(new GRPCRequest<ProductSlabConfigRequest>
            {
                Request = new ProductSlabConfigRequest
                {
                    CompanyId = request.CompanyId,
                    ProductId = request.ProductId,
                    SlabTypes = null
                }
            });
            if (productConfig != null && productConfig.Status)
            {
                reply = await _iLeadService.AddLeadOfferConfig(new GRPCRequest<AddLeadOfferConfigRequestDc>
                {
                    Request = new AddLeadOfferConfigRequestDc
                    {
                        LeadId = request.LeadId,
                        ProductSlabConfigs = productConfig.Response
                    }
                });
            }
            return reply;
        }

        public async Task<GRPCReply<string>> GenerateKarzaAadhaarOtpForNBFC(GRPCRequest<AcceptOfferByLeadDc> request)
        {
            //var latestGST = await _iCompanyService.GetLatestGSTRate(GstConstant.Gst18);
            //var LeadProdReply = await _iLeadService.GetLeadProductId(request.Request.LeadId);

            //List<long> nbfcids = new List<long>();
            //nbfcids.Add(request.Request.NBFCCompanyId);

            //var productIds = LeadProdReply.Response.ProductId;

            //var nbfcConfig = await _iProductService.GetProductNBFCConfigs(new GRPCRequest<GetProductNBFCConfigRequestDc>
            //{
            //    Request = new GetProductNBFCConfigRequestDc { NBFCCompanyIds = nbfcids, ProductId = productIds }
            //});
            //var SlabConfig = nbfcConfig.Response[0];

            //var Anchorconfig = SlabConfig.ProductSlabConfigs.Where(x => x.MinLoanAmount <= request.Request.Amount && x.MaxLoanAmount >= request.Request.Amount).ToList();
            //request.Request.ProductSlabConfigResponse = Anchorconfig;
            //request.Request.GST = latestGST.Response;
            var result = await _iLeadService.GenerateKarzaAadhaarOtpForNBFC(request);
            return result;
        }

        public async Task<GRPCReply<string>> GetUserNameByUserId(string UserId)
        {
            GRPCReply<string> reply = new GRPCReply<string> { Message = "User Not Found!!" };
            List<string> UserIdList = new List<string> { UserId };
            UserListDetailsRequest userReq = new UserListDetailsRequest
            {
                userIds = UserIdList,
                keyword = null,
                Skip = 0,
                Take = 10
            };
            var userReply = await _iIdentityService.GetUserById(userReq);
            reply.Response = userReply.UserListDetails != null ? userReply.UserListDetails.Select(y => y.UserName).FirstOrDefault() : "";
            if(reply.Response != "")
            {
                reply.Status = true;
                reply.Message = "User Found";
            }
            return reply;
        }

        public async Task<CustomerMobileResponse> CustomerMobileValidate(CustomerMobileValidate customerMobileValidate, string Token)
        {
            CustomerMobileResponse customerMobileResponse = new CustomerMobileResponse();
            var reply = await _iCommunicationService.ValidateOTP(new ValidateOTPRequest { MobileNo = customerMobileValidate.MobileNo, OTP = customerMobileValidate.OTP });
            if (reply.Status)
            {
                List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>> {
                };
                    var user = await _iIdentityService.CreateUserWithToken(new CreateUserRequest
                    {
                        EmailId = "",
                        MobileNo = customerMobileValidate.MobileNo,
                        Password = "Sc@" + customerMobileValidate.MobileNo,
                        UserName = customerMobileValidate.MobileNo,
                        UserType = UserTypeConstants.Customer,
                        Claims = Claims
                    });

                    if (!string.IsNullOrEmpty(user.UserId))
                    {
                    customerMobileResponse.Status = true;
                    customerMobileResponse.Message = "OTP Validate Successfully.";
                    customerMobileResponse.UserId = user.UserId;
                    customerMobileResponse.UserTokan = (string.IsNullOrEmpty(Token) ? user.UserToken : Token);
                       
                    }
                    else
                    {
                    customerMobileResponse.Status = false;
                    customerMobileResponse.Message = "Incorrect OTP.";
                    }
            }
            else
            {
                customerMobileResponse.Status = false;
                if (reply.Message == "OTP expired. Please resend again")
                {
                    customerMobileResponse.Message = reply.Message;
                }
                else
                {
                    customerMobileResponse.Message = "Incorrect OTP.";
                }

            }

            return customerMobileResponse;

        }

    }
}

