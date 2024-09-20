using MassTransit.Futures.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenHtmlToPdf;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.DTOs.DSA;
using ScaleUP.ApiGateways.Aggregator.Managers.NBFC;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Helper;
using System.Collections.Generic;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class DSAManager
    {
        private ILeadService _iLeadService;
        private IProductService _iProductService;
        private ICommunicationService _iCommunicationService;
        private IIdentityService _iIdentityService;
        private ICompanyService _iCompanyService;
        private INBFCService _iNBFCService;
        private IKycGrpcService _ikYCGrpcService;
        private ILocationService _iLocationService;
        private ILoanAccountService _iLoanAccountService;
        private KYCUserDetailManager _kYCUserDetailManager;
        private MediaManager _MediaManager;
        private IMediaService _iMediaService;


        private ArthMateManager _ArthMateManager;

        public DSAManager(ILeadService iLeadService, KYCUserDetailManager kYCUserDetailManager, ArthMateManager ArthMateManager
            , MediaManager mediaManager
, IProductService iProductService
            , ICommunicationService iCommunicationService, IIdentityService iIdentityService
            , ICompanyService iCompanyService, INBFCService iNBFCService
            , IKycGrpcService kycGrpcService
            , ILocationService locService
            , ILoanAccountService loanAccountService, IMediaService iMediaService)
        {
            _iLeadService = iLeadService;
            _iProductService = iProductService;
            _iCommunicationService = iCommunicationService;
            _iIdentityService = iIdentityService;
            _iCompanyService = iCompanyService;
            _iNBFCService = iNBFCService;
            _kYCUserDetailManager = kYCUserDetailManager;
            _ikYCGrpcService = kycGrpcService;
            _iLocationService = locService;
            _iLoanAccountService = loanAccountService;
            _ArthMateManager = ArthMateManager;
            _MediaManager = mediaManager;
            _iMediaService = iMediaService;
        }



        public async Task<GenerateOTPResponse> GenerateDSAOtp(string MobileNo)
        {
            var existOtpDetail = await _iCommunicationService.ExistValidOTP(MobileNo);
            GenerateOTPResponse generateOTPResponse = new GenerateOTPResponse();
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string otp = GenerateRandomNumber.GenerateRandomOTP(6, saAllowedCharacters);
            //TemplateMasters
            var TemMasterResponse = await _iLeadService.GetLeadNotificationTemplate(new TemplateMasterRequestDc
            {
                TemplateCode = TemplateEnum.DSALoginOTPSMS.ToString()//"LoginOTPSMS"
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
        public async Task<DSAMobileOTPValidateResponse> DSALeadMobileValidate(DSAMobileValidateOTPRequest request)
        {
            DSAMobileOTPValidateResponse dSAMobileOTPValidateResponse = new DSAMobileOTPValidateResponse();
            var reply = await _iCommunicationService.ValidateOTP(new ValidateOTPRequest { MobileNo = request.MobileNo, OTP = request.OTP });
            if (reply.Status)
            {
                List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>>
                {
                    //            new KeyValuePair<string, string>("companyid",existSalesAgentData.Response.AnchorCompanyId.ToString()),
                    //             new KeyValuePair<string, string>("productid",existSalesAgentData.Response.ProductId.ToString())
                };
                var user = await _iIdentityService.CreateUserWithToken(new CreateUserRequest
                {
                    EmailId = "",
                    MobileNo = request.MobileNo,
                    Password = "Sc@" + request.MobileNo,
                    UserName = request.MobileNo,
                    UserType = UserTypeConstants.Customer,
                    Claims = Claims
                });

                if (!string.IsNullOrEmpty(user.UserId))
                {
                    dSAMobileOTPValidateResponse.Token = user.UserToken;
                    dSAMobileOTPValidateResponse.UserId = user.UserId;
                    dSAMobileOTPValidateResponse.Status = true;
                    dSAMobileOTPValidateResponse.Message = "Success";
                }
                else
                {
                    dSAMobileOTPValidateResponse.Status = false;
                    dSAMobileOTPValidateResponse.Message = "something went wrong.";
                }
            }
            else
            {
                dSAMobileOTPValidateResponse.Status |= false;
                dSAMobileOTPValidateResponse.Message = reply.Message;
            }
            return dSAMobileOTPValidateResponse;
        }
        public async Task<DSAUserProfileResponse> GetDSAUserProfile(string UserId, string UserMobile)
        {
            DSAUserProfileResponse _DSAUserProfileResponse = new DSAUserProfileResponse();
            var companyDetail = await _iCompanyService.GetFinTechCompany();
            var anchoreCompanyCode = await _iCompanyService.GetCompanyCodeById(new GRPCRequest<long> { Request = companyDetail.Response });
            var DSAproduct = await _iProductService.GetProductIdByCode(new GRPCRequest<string> { Request = ProductCodeConstants.DSA });

            if (anchoreCompanyCode != null && anchoreCompanyCode.Status && DSAproduct != null && DSAproduct.Status)
            {
                GRPCRequest<LeadDetailRequest> LeadRequest = new GRPCRequest<LeadDetailRequest>
                {
                    Request = new LeadDetailRequest
                    {
                        UserId = UserId,
                        ProductId = DSAproduct.Response
                    }
                };
                var existSalesAgentData = await _iProductService.GetExistDSACompanyDetail(new SaleAgentRequest
                {
                    CompanyId = companyDetail.Response,
                    ProductIds = new List<long> { DSAproduct.Response },
                    UserId = UserId,
                    CompanyCode = anchoreCompanyCode.Response
                });
                if (existSalesAgentData.Status && existSalesAgentData.Response != null)
                {
                    List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("companyid",existSalesAgentData.Response.AnchorCompanyId.ToString()),
                         new KeyValuePair<string, string>("productid",existSalesAgentData.Response.ProductId.ToString())
                        };
                    var anchoreCompanyDetail = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = existSalesAgentData.Response.AnchorCompanyId });
                    if (anchoreCompanyDetail != null && anchoreCompanyDetail.Status)
                    {
                        var user = await _iIdentityService.CreateUserWithToken(new CreateUserRequest
                        {
                            EmailId = "",
                            MobileNo = UserMobile,
                            Password = "Sc@" + UserMobile,
                            UserName = UserMobile,
                            UserType = UserTypeConstants.Customer,
                            Claims = Claims,
                            UserRoles = new List<string> { existSalesAgentData.Response.Role }
                        });

                        if (!string.IsNullOrEmpty(user.UserId))
                        {
                            var AnCompanyCode = await _iCompanyService.GetCompanyCodeById(new GRPCRequest<long> { Request = existSalesAgentData.Response.AnchorCompanyId });

                            _DSAUserProfileResponse.Status = true;
                            _DSAUserProfileResponse.Message = "Success";
                            _DSAUserProfileResponse.UserId = user.UserId;
                            _DSAUserProfileResponse.UserToken = user.UserToken;
                            _DSAUserProfileResponse.CompanyCode = AnCompanyCode.Response;
                            _DSAUserProfileResponse.CompanyId = existSalesAgentData.Response.AnchorCompanyId;
                            _DSAUserProfileResponse.ProductCode = existSalesAgentData.Response.ProductCode;
                            _DSAUserProfileResponse.ProductId = existSalesAgentData.Response.ProductId;
                            _DSAUserProfileResponse.IsActivated = true;
                            _DSAUserProfileResponse.Role = existSalesAgentData.Response.Role;
                            _DSAUserProfileResponse.Type = existSalesAgentData.Response.Type;
                            _DSAUserProfileResponse.DSALeadCode = !string.IsNullOrEmpty(existSalesAgentData.Response.DSACode) ? existSalesAgentData.Response.DSACode : "";
                        }
                        else
                        {
                            _DSAUserProfileResponse.Status = false;
                            _DSAUserProfileResponse.Message = "Something went wrong";
                        }
                    }
                    else
                    {
                        _DSAUserProfileResponse.Status = false;
                        _DSAUserProfileResponse.Message = "Anchor company not active.";
                    }
                    userData userData = new userData();
                    userData.AadharNumber = existSalesAgentData.Response.AadharNumber;
                    userData.Address = existSalesAgentData.Response.Address;
                    userData.PanNumber = existSalesAgentData.Response.PanNumber;
                    userData.Name = existSalesAgentData.Response.Name;
                    userData.Mobile = existSalesAgentData.Response.Mobile;
                    userData.WorkingLocation = existSalesAgentData.Response.WorkingLocation;
                    userData.selfie = existSalesAgentData.Response.selfie;
                    userData.StartedOn = existSalesAgentData.Response.StartedOn;
                    userData.ExpiredOn = existSalesAgentData.Response.ExpiredOn;
                    userData.DocSignedUrl = existSalesAgentData.Response.DocSignedUrl;
                    userData.SalesAgentCommissions = existSalesAgentData.Response.SalesAgentCommissions;
                    _DSAUserProfileResponse.userData = userData;
                }
                else if (existSalesAgentData.Status && existSalesAgentData.Response == null)
                {
                    _DSAUserProfileResponse.Status = false;
                    _DSAUserProfileResponse.Message = existSalesAgentData.Message;
                }
                else
                {
                    List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>>
                    {
                        //new KeyValuePair<string, string>("companyid",companyDetail.Response.ToString()),
                        // new KeyValuePair<string, string>("productid",DSAproduct.Response.ToString())
                    };
                    var anchoreCompanyDetail = await _iCompanyService.GetCompanyDataById(new GRPCRequest<long> { Request = companyDetail.Response });
                    if (anchoreCompanyDetail != null && anchoreCompanyDetail.Status)
                    {
                        var user = await _iIdentityService.CreateUserWithToken(new CreateUserRequest
                        {
                            EmailId = "",
                            MobileNo = UserMobile,
                            Password = "Sc@" + UserMobile,
                            UserName = UserMobile,
                            UserType = UserTypeConstants.Customer,
                            Claims = Claims
                        });

                        if (!string.IsNullOrEmpty(user.UserId))
                        {
                            _DSAUserProfileResponse.Status = true;
                            _DSAUserProfileResponse.Message = "Success";
                            _DSAUserProfileResponse.UserId = user.UserId;
                            _DSAUserProfileResponse.UserToken = user.UserToken;
                            _DSAUserProfileResponse.CompanyCode = anchoreCompanyCode.Response;
                            _DSAUserProfileResponse.CompanyId = companyDetail.Response;
                            _DSAUserProfileResponse.ProductCode = ProductCodeConstants.DSA;
                            _DSAUserProfileResponse.ProductId = DSAproduct.Response;
                            _DSAUserProfileResponse.IsActivated = false;
                        }
                        else
                        {
                            _DSAUserProfileResponse.Status = false;
                            _DSAUserProfileResponse.Message = "Something went wrong";
                        }
                    }
                    else
                    {
                        _DSAUserProfileResponse.Status = false;
                        _DSAUserProfileResponse.Message = "Anchor company not active.";
                    }
                }
            }
            return _DSAUserProfileResponse;

        }

        public async Task<DSALeadResponse> GetLeadByMobileNo(string UserId, string MobileNo, string token)
        {
            DSALeadResponse dSAMobileOTPValidateResponse = new DSALeadResponse();

            dSAMobileOTPValidateResponse.LeadId = 0;

            if (!string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(MobileNo))
            {
                var companyreply = await _iCompanyService.GetFinTechCompanyDetail();//ProductCodeConstant
                if (companyreply.Status)
                {
                    var productdetailRequest = new ProductdetailRequest
                    {
                        CompanyId = companyreply.Response.CompanyId,
                        ProductCode = ProductCodeConstants.DSA,
                        CompanyType = CompanyTypeEnum.FinTech.ToString()
                    };

                    var prodActivities = await _iProductService.GetProductIdByCodeAndProductActivities(new GRPCRequest<ProductdetailRequest>
                    {
                        Request = productdetailRequest
                    });

                    if (!prodActivities.Status)
                    {
                        dSAMobileOTPValidateResponse.Status = false;
                        dSAMobileOTPValidateResponse.Message = "something went wrong in get prodActivities!!";

                        return dSAMobileOTPValidateResponse;
                    }
                    var leadresponse = await _iLeadService.GetLeadForMobile(new LeadMobileRequest
                    {
                        CompanyId = companyreply.Response.CompanyId,
                        MobileNo = MobileNo,
                        ProductId = prodActivities.Response.ProductId,
                        ActivityId = prodActivities.Response.LeadProductActivities.OrderBy(x => x.Sequence).FirstOrDefault().ActivityMasterId,
                        SubActivityId = prodActivities.Response.LeadProductActivities.OrderBy(x => x.Sequence).FirstOrDefault().SubActivityMasterId,
                        UserId = UserId,
                        ProductActivities = prodActivities.Status ? prodActivities.Response.LeadProductActivities : new List<GRPCLeadProductActivity>(),
                        VintageDays = 0,// leadMobileValidate.VintageDays,
                        MonthlyAvgBuying = 0,// leadMobileValidate.MonthlyAvgBuying,
                        CompanyCode = companyreply.Response.CompanyCode,
                        ProductCode = ProductCodeConstants.DSA //leadMobileValidate.ProductCode
                    }, token);

                    if (leadresponse.Status)
                    {
                        dSAMobileOTPValidateResponse.LeadId = leadresponse.LeadId;
                        dSAMobileOTPValidateResponse.Status = true;
                        dSAMobileOTPValidateResponse.Message = "Sucess!!";
                    }
                    else
                    {
                        dSAMobileOTPValidateResponse.Status = false;
                        dSAMobileOTPValidateResponse.Message = "Something went wrong !!";
                    }
                }
                else
                {
                    dSAMobileOTPValidateResponse.Message = "fintech company not found.";

                }
            }
            else
            {
                dSAMobileOTPValidateResponse.Status = false;
                dSAMobileOTPValidateResponse.Message = "Something went wrong.";
            }
            return dSAMobileOTPValidateResponse;
        }

        public async Task<GRPCReply<bool>> ApproveDSALead(ApproveDSALeadRequest request)
        {
            var reply = new GRPCReply<bool> { Message = "Something went wrong." };
            if (request != null && request.LeadId > 0 && request.ProductIds != null && request.ProductIds.Any())
            {
                List<string> MasterCodeList = new List<string>
                {
                         KYCMasterConstants.PAN,
                         KYCMasterConstants.Aadhar,
                         KYCMasterConstants.Selfie,
                         KYCMasterConstants.DSAProfileType,
                         KYCMasterConstants.DSAPersonalDetail,
                         KYCMasterConstants.ConnectorPersonalDetail,
                };
                var kYCUserDetail = await _kYCUserDetailManager.GetLeadDetailAll(request.UserId, ProductCodeConstants.DSA, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
                if (kYCUserDetail != null && kYCUserDetail.panDetail != null && kYCUserDetail.SelfieDetail != null && kYCUserDetail.aadharDetail != null && kYCUserDetail.DSAProfileInfo != null && (kYCUserDetail.DSAPersonalDetail != null || kYCUserDetail.ConnectorPersonalDetail != null))
                {
                    var existSalesAgentData = await _iProductService.GetExistDSACompanyDetail(new SaleAgentRequest
                    {
                        CompanyId = 0,
                        ProductIds = request.ProductIds,
                        UserId = request.UserId,
                        CompanyCode = ""
                    });
                    if (existSalesAgentData != null && existSalesAgentData.Status)
                    {
                        reply.Status = false;
                        reply.Message = "Lead Already Approved";
                        return reply;
                    }

                    var companyList = await _iCompanyService.GetCompanyIdentificationCode(new GRPCRequest<List<long>>());
                    string UserRole = "";
                    string ShopName = "";
                    string MobileNo = "";
                    string Type = "";
                    string AnchorName = "";
                    long companyId = 0;
                    //var leadPayoutData = await _iLeadService.GetDSALeadPayoutDetails(new GRPCRequest<long> { Request = request.LeadId });
                    var leadAgreementData = await _iLeadService.GetDSAAggreementDetailByLeadId(new GRPCRequest<long>
                    {
                        Request = request.LeadId
                    });
                    if (leadAgreementData != null && leadAgreementData.Status)
                    {
                        string fullAddress = "";
                        if (kYCUserDetail.aadharDetail.LocationAddress != null)
                            fullAddress = kYCUserDetail.aadharDetail.LocationAddress.AddressLineOne + kYCUserDetail.aadharDetail.LocationAddress.AddressLineTwo + kYCUserDetail.aadharDetail.LocationAddress.AddressLineThree;
                        //DSA Admin
                        if (kYCUserDetail.DSAProfileInfo.DSAType == DSAProfileTypeConstants.DSA && kYCUserDetail.DSAPersonalDetail != null)
                        {
                            ShopName = kYCUserDetail.DSAPersonalDetail.CompanyName;
                            MobileNo = kYCUserDetail.DSAPersonalDetail.MobileNo;
                            UserRole = UserRoleConstants.DSAAdmin;
                            Type = DSAProfileTypeConstants.DSA;
                            AnchorName = kYCUserDetail.DSAPersonalDetail.CompanyName;
                            var leadReply = await _iLeadService.GetLeadBankDetailByLeadId(new GRPCRequest<long> { Request = request.LeadId });
                            var leadBankDetail = leadReply.Response != null && leadReply.Response.Any() ? leadReply.Response.First() : null;
                            //Create new Company for DSA
                            var companyReply = await CreateDSACompany(new SaveCompanyAndLocationDTO
                            {
                                AccountType = leadBankDetail != null ? leadBankDetail.AccountType : "",
                                BankAccountNumber = leadBankDetail != null ? leadBankDetail.AccountNumber : "",
                                BankIFSC = leadBankDetail != null ? leadBankDetail.IFSCCode : "",
                                BankName = leadBankDetail != null ? leadBankDetail.BankName : "",
                                BusinessContactEmail = kYCUserDetail.DSAPersonalDetail.EmailId,
                                BusinessContactNo = kYCUserDetail.DSAPersonalDetail.MobileNo,
                                BusinessHelpline = kYCUserDetail.DSAPersonalDetail.MobileNo,
                                BusinessName = kYCUserDetail.DSAPersonalDetail.CompanyName,
                                LandingName = kYCUserDetail.DSAPersonalDetail.CompanyName,
                                BusinessPanURL = kYCUserDetail.panDetail.FrontImageUrl,
                                BusinessPanDocId = kYCUserDetail.panDetail.DocumentId,
                                BusinessTypeId = 0,
                                GSTNo = kYCUserDetail.DSAPersonalDetail.GSTNumber ?? "",
                                GSTDocId = 0,
                                GSTDocumentURL = "",
                                CompanyType = CompanyTypeEnum.Anchor.ToString(),
                                ContactPersonName = kYCUserDetail.panDetail.NameOnCard,
                                PanNo = kYCUserDetail.panDetail.UniqueId,
                                PanURL = kYCUserDetail.panDetail.FrontImageUrl,
                                PanDocId = kYCUserDetail.panDetail.DocumentId,
                                AgreementURL = "",
                                AgreementDocId = 0,
                                AgreementStartDate = null,
                                AgreementEndDate = null,
                                CancelChequeURL = "",
                                CancelChequeDocId = 0,
                                CompanyStatus = true,
                                CustomerAgreementURL = "",
                                CustomerAgreementDocId = 0,
                                IsSelfConfiguration = false,
                                LogoURL = "",
                                MSMEDocumentURL = "",
                                MSMEDocId = 0,
                                WhitelistURL = "",
                                CompanyAddress = new CompanyAddressDTO
                                {
                                    AddressLineOne = kYCUserDetail.aadharDetail.LocationAddress.AddressLineOne,
                                    AddressLineTwo = kYCUserDetail.aadharDetail.LocationAddress.AddressLineTwo,
                                    AddressLineThree = kYCUserDetail.aadharDetail.LocationAddress.AddressLineThree,
                                    AddressTypeId = kYCUserDetail.aadharDetail.LocationAddress.AddressTypeId,
                                    CityId = kYCUserDetail.aadharDetail.LocationAddress.CityId,
                                    ZipCode = kYCUserDetail.aadharDetail.LocationAddress.ZipCode
                                },
                                PartnerList = new List<DTOs.PartnerListDc> { new DTOs.PartnerListDc
                            {
                                MobileNo = kYCUserDetail.DSAPersonalDetail.MobileNo,
                                PartnerId = 0,
                                PartnerName = kYCUserDetail.panDetail.NameOnCard
                            }}
                            });
                            if (companyReply != null && companyReply.CompanyId > 0)
                            {
                                //Map Product
                                var fintechCompany = await _iCompanyService.GetFinTechCompany();
                                var productConfigReply = await _iProductService.GetLeadCompanyConfig(new CompanyConfigProdRequest
                                {
                                    AnchorCompanyId = fintechCompany.Response,
                                    NBFCCompanyId = 0,
                                    ProductId = request.ProductIds.First()
                                });
                                if (productConfigReply != null && productConfigReply.AnchorCompanyConfig != null)
                                {
                                    var addCompanyProductReply = await _iProductService.AddUpdateAnchorProductConfig(new AddUpdateAnchorProductConfigRequest
                                    {
                                        CompanyId = companyReply.CompanyId,
                                        ProductId = request.ProductIds.First(),
                                        AgreementDocId = productConfigReply.AnchorCompanyConfig.AgreementDocId,
                                        AgreementEndDate = productConfigReply.AnchorCompanyConfig.AgreementEndDate,
                                        AgreementStartDate = productConfigReply.AnchorCompanyConfig.AgreementStartDate,
                                        AgreementURL = productConfigReply.AnchorCompanyConfig.AgreementURL,
                                        AnnualInterestPayableBy = productConfigReply.AnchorCompanyConfig.AnnualInterestPayableBy,
                                        AnnualInterestRate = productConfigReply.AnchorCompanyConfig.AnnualInterestRate,
                                        BounceCharge = productConfigReply.AnchorCompanyConfig.BounceCharge,
                                        CommissionPayout = productConfigReply.AnchorCompanyConfig.CommissionPayout,
                                        ConsiderationFee = productConfigReply.AnchorCompanyConfig.ConsiderationFee,
                                        CreditDays = productConfigReply.AnchorCompanyConfig.CreditDays,
                                        DelayPenaltyRate = productConfigReply.AnchorCompanyConfig.DelayPenaltyRate,
                                        DisbursementSharingCommission = productConfigReply.AnchorCompanyConfig.DisbursementSharingCommission,
                                        DisbursementTAT = productConfigReply.AnchorCompanyConfig.DisbursementTAT,
                                        EMIBounceCharge = productConfigReply.AnchorCompanyConfig.EMIBounceCharge,
                                        EMIPenaltyRate = productConfigReply.AnchorCompanyConfig.EMIPenaltyRate,
                                        EMIProcessingFeeRate = productConfigReply.AnchorCompanyConfig.EMIProcessingFeeRate,
                                        EMIRate = productConfigReply.AnchorCompanyConfig.EMIRate,
                                        MinLoanAmount = productConfigReply.AnchorCompanyConfig.MinLoanAmount,
                                        MaxLoanAmount = productConfigReply.AnchorCompanyConfig.MaxLoanAmount,
                                        MinTenureInMonth = productConfigReply.AnchorCompanyConfig.MinTenureInMonth,
                                        MaxTenureInMonth = productConfigReply.AnchorCompanyConfig.MaxTenureInMonth,
                                        ProcessingFeePayableBy = productConfigReply.AnchorCompanyConfig.ProcessingFeePayableBy,
                                        ProcessingFeeRate = productConfigReply.AnchorCompanyConfig.ProcessingFeeRate,
                                        ProcessingFeeType = productConfigReply.AnchorCompanyConfig.ProcessingFeeType
                                    });
                                    if (addCompanyProductReply != null && addCompanyProductReply.Status)
                                    {
                                        //Add Sales Agent
                                        companyId = companyReply.CompanyId;
                                        var productReply = await _iProductService.AddUpdateSalesAgent(new GRPCRequest<SalesAgentRequest>
                                        {
                                            Request = new SalesAgentRequest
                                            {
                                                AnchorCompanyId = companyId,
                                                AadharBackUrl = kYCUserDetail.aadharDetail.BackImageUrl,
                                                AadharFrontUrl = kYCUserDetail.aadharDetail.FrontImageUrl,
                                                AadharNo = kYCUserDetail.aadharDetail.MaskedAadhaarNumber,
                                                AgreementEndDate = leadAgreementData.Response.ExpiredOn,
                                                AgreementStartDate = leadAgreementData.Response.StartedOn,
                                                AgreementUrl = leadAgreementData.Response.DocSignedUrl,
                                                CityName = kYCUserDetail.aadharDetail.LocationAddress.CityName,
                                                FullName = kYCUserDetail.panDetail.NameOnCard,
                                                GstnNo = kYCUserDetail.DSAPersonalDetail.GSTNumber,
                                                GstnUrl = "",
                                                MobileNo = kYCUserDetail.DSAPersonalDetail.MobileNo,
                                                PanNo = kYCUserDetail.panDetail.UniqueId,
                                                PanUrl = kYCUserDetail.panDetail.FrontImageUrl,
                                                ProductIds = request.ProductIds,
                                                SalesAgentId = 0,
                                                StateName = kYCUserDetail.aadharDetail.LocationAddress.StateName,
                                                Status = LeadStatusEnum.Activated.ToString(),
                                                Type = DSAProfileTypeConstants.DSA,
                                                UserId = request.UserId,
                                                //PayoutPercentage = leadAgreementData.Response.PayoutPercenatge,
                                                Role = UserRoleConstants.DSAAdmin,
                                                AadharAddress = fullAddress,
                                                WorkingLocation = kYCUserDetail.DSAPersonalDetail.WorkingLocation,
                                                SelfieUrl = kYCUserDetail.SelfieDetail.FrontImageUrl,
                                                EmailId = kYCUserDetail.DSAPersonalDetail.EmailId,
                                                SalesAgentCommissions = leadAgreementData.Response.SalesAgentCommissions
                                            }
                                        });
                                    }
                                }
                            }
                            else
                            {
                                reply.Message = companyReply != null ? companyReply.Message : "Failed to Create Company";
                                return reply;
                            }
                        }
                        //Connector
                        else if (kYCUserDetail.ConnectorPersonalDetail != null && companyList != null && companyList.Response != null && companyList.Response.Any(x => x.IdentificationCode == CompanyIdentificationCodeConstants.ScaleUpConnectorAnchor))
                        {
                            UserRole = UserRoleConstants.Connector;
                            MobileNo = kYCUserDetail.ConnectorPersonalDetail.MobileNo;
                            Type = DSAProfileTypeConstants.Connector;
                            companyId = companyList.Response.First(x => x.IdentificationCode == CompanyIdentificationCodeConstants.ScaleUpConnectorAnchor).CompanyId;
                            AnchorName = CompanyIdentificationCodeConstants.ScaleUpConnectorAnchor;
                            //Add Sales Agent
                            var productReply = await _iProductService.AddUpdateSalesAgent(new GRPCRequest<SalesAgentRequest>
                            {
                                Request = new SalesAgentRequest
                                {
                                    AnchorCompanyId = companyId,
                                    AadharBackUrl = kYCUserDetail.aadharDetail.BackImageUrl,
                                    AadharFrontUrl = kYCUserDetail.aadharDetail.FrontImageUrl,
                                    AadharNo = kYCUserDetail.aadharDetail.MaskedAadhaarNumber,
                                    AgreementEndDate = leadAgreementData.Response.ExpiredOn,
                                    AgreementStartDate = leadAgreementData.Response.StartedOn,
                                    AgreementUrl = leadAgreementData.Response.eSignedUrl,
                                    CityName = kYCUserDetail.aadharDetail.LocationAddress != null ? kYCUserDetail.aadharDetail.LocationAddress.CityName : "",
                                    FullName = kYCUserDetail.panDetail.NameOnCard,
                                    GstnNo = "",
                                    GstnUrl = "",
                                    MobileNo = kYCUserDetail.ConnectorPersonalDetail.MobileNo,
                                    PanNo = kYCUserDetail.panDetail.UniqueId,
                                    PanUrl = kYCUserDetail.panDetail.FrontImageUrl,
                                    ProductIds = request.ProductIds,
                                    SalesAgentId = 0,
                                    StateName = kYCUserDetail.aadharDetail.LocationAddress != null ? kYCUserDetail.aadharDetail.LocationAddress.StateName : "",
                                    Status = LeadStatusEnum.Activated.ToString(),
                                    Type = DSAProfileTypeConstants.Connector,
                                    UserId = request.UserId,
                                    Role = UserRoleConstants.Connector,
                                    //PayoutPercentage = leadAgreementData.Response.PayoutPercenatge,
                                    AadharAddress = fullAddress,
                                    WorkingLocation = kYCUserDetail.ConnectorPersonalDetail.WorkingLocation,
                                    SelfieUrl = kYCUserDetail.SelfieDetail.FrontImageUrl,
                                    EmailId = kYCUserDetail.ConnectorPersonalDetail.EmailId,
                                    SalesAgentCommissions = leadAgreementData.Response.SalesAgentCommissions
                                }
                            });
                        }
                        else
                        {
                            reply.Message = "DSA Type is Invalid / Scaleup Anchor Not Found!!!";
                            return reply;
                        }
                        if (!string.IsNullOrEmpty(UserRole) && !string.IsNullOrEmpty(request.UserId) && companyId > 0)
                        {
                            await _iLeadService.UpdateLeadStatus(new GRPCRequest<UpdateLeadStatusRequest>
                            {
                                Request = new UpdateLeadStatusRequest
                                {
                                    LeadId = request.LeadId,
                                    Status = LeadStatusEnum.Activated.ToString()
                                }
                            });
                            await _iIdentityService.UpdateUserRole(new GRPCRequest<UpdateUserRoleRequest>
                            {
                                Request = new UpdateUserRoleRequest
                                {
                                    RoleNames = new List<string> { UserRole },
                                    UserId = request.UserId
                                }
                            });

                            await _iCompanyService.AddUpdateCompanyUserMapping(new AddCompanyUserMappingRequest
                            {
                                CompanyId = companyId,
                                UserId = request.UserId
                            });
                            await _iLoanAccountService.AddDSALoanAccount(new GRPCRequest<AddDSALoanAccountRequest>
                            {
                                Request = new AddDSALoanAccountRequest
                                {
                                    LeadId = request.LeadId,
                                    UserId = request.UserId ?? "",
                                    AnchorCompanyId = companyId,
                                    AnchorName = AnchorName,
                                    LeadCode = leadAgreementData.Response.LeadCode ?? "",
                                    ProductId = request.ProductIds.First(),
                                    ProductType = ProductTypeConstants.BusinessLoan,
                                    AgreementRenewalDate = leadAgreementData.Response.StartedOn ?? DateTime.Now,
                                    ApplicationDate = DateTime.Now,
                                    DisbursalDate = DateTime.Now,
                                    CityName = kYCUserDetail.aadharDetail.LocationAddress != null ? kYCUserDetail.aadharDetail.LocationAddress.CityName : "",
                                    CustomerImage = kYCUserDetail.SelfieDetail.FrontImageUrl ?? "",
                                    CustomerName = kYCUserDetail.panDetail.NameOnCard ?? "",
                                    ShopName = ShopName ?? "",
                                    MobileNo = MobileNo,
                                    Type = Type,
                                    IsAccountActive = true,
                                    IsBlock = false,
                                    IsDefaultNBFC = false,
                                    NBFCCompanyCode = "",
                                    NBFCCompanyId = 0,
                                    ThirdPartyLoanCode = ""
                                }
                            });

                            reply.Message = "Approved Successfully";
                            reply.Status = true;
                            reply.Response = true;
                        }
                    }
                    else
                    {
                        reply.Message = "Lead Agreement not found!!!";
                    }
                }
            }
            return reply;
        }
        public async Task<CreateCompanyResponse> CreateDSACompany(SaveCompanyAndLocationDTO createCompanyDTO)
        {
            CreateCompanyResponse createCompanyResponse = new CreateCompanyResponse();
            var identityResponse = await _iIdentityService.CreateClient();

            GRPCRequest<GetEntityCodeRequest> companyCodeReq = new GRPCRequest<GetEntityCodeRequest>
            {
                Request = new GetEntityCodeRequest
                {
                    EntityName = "Company",
                    CompanyType = createCompanyDTO.CompanyType
                }
            };
            var companyCodeRes = await _iCompanyService.GetCurrentNumber(companyCodeReq);
            var request = new AddCompanyDTO
            {
                GSTNo = createCompanyDTO.GSTNo,
                PanNo = createCompanyDTO.PanNo,
                BusinessName = createCompanyDTO.BusinessName,
                LandingName = string.IsNullOrEmpty(createCompanyDTO.LandingName) ? createCompanyDTO.BusinessName : createCompanyDTO.LandingName,
                BusinessContactEmail = createCompanyDTO.BusinessContactEmail,
                BusinessContactNo = createCompanyDTO.BusinessContactNo,
                APIKey = identityResponse.Response.ApiKey,
                APISecretKey = identityResponse.Response.ApiSecret,
                LogoURL = createCompanyDTO.LogoURL,
                BusinessHelpline = createCompanyDTO.BusinessHelpline,
                BusinessTypeId = createCompanyDTO.BusinessTypeId,
                AgreementEndDate = createCompanyDTO.AgreementEndDate,
                AgreementStartDate = createCompanyDTO.AgreementStartDate,
                AgreementURL = createCompanyDTO.AgreementURL,
                AgreementDocId = createCompanyDTO.AgreementDocId,
                BankAccountNumber = createCompanyDTO.BankAccountNumber,
                BankIFSC = createCompanyDTO.BankIFSC,
                BankName = createCompanyDTO.BankName,
                BusinessPanURL = createCompanyDTO.BusinessPanURL,
                CancelChequeURL = createCompanyDTO.CancelChequeURL,
                CancelChequeDocId = createCompanyDTO.CancelChequeDocId,
                BusinessPanDocId = createCompanyDTO.BusinessPanDocId,
                CustomerAgreementDocId = createCompanyDTO.CustomerAgreementDocId,
                CustomerAgreementURL = createCompanyDTO.CustomerAgreementURL,
                ContactPersonName = createCompanyDTO.ContactPersonName,
                WhitelistURL = createCompanyDTO.WhitelistURL,
                CompanyType = createCompanyDTO.CompanyType,
                IsSelfConfiguration = createCompanyDTO.IsSelfConfiguration,
                GSTDocId = createCompanyDTO.GSTDocId,
                GSTDocumentURL = createCompanyDTO.GSTDocumentURL,
                MSMEDocId = createCompanyDTO.MSMEDocId,
                MSMEDocumentURL = createCompanyDTO.MSMEDocumentURL,
                CompanyStatus = createCompanyDTO.CompanyStatus,
                AccountType = createCompanyDTO.AccountType,
                PanURL = createCompanyDTO.PanURL,
                PanDocId = createCompanyDTO.PanDocId,
                IsDSA = true,
                CompanyCode = companyCodeRes.Status ? companyCodeRes.Response : createCompanyDTO.GSTNo.Substring(3) + createCompanyDTO.BusinessContactNo.ToString().Substring(3),
                PartnerList = createCompanyDTO.PartnerList.Select(x => new BuildingBlocks.GRPC.Contracts.Company.DataContracts.PartnerListDc
                {
                    MobileNo = x.MobileNo,
                    PartnerId = x.PartnerId,
                    PartnerName = x.PartnerName
                }
                ).ToList()
            };

            var companyReply = await _iCompanyService.AddCompany(request);
            if (companyReply.Status)
            {
                GRPCRequest<long> CompanyIdRequest = new GRPCRequest<long> { Request = companyReply.CompanyId };
                var companyLocations = await _iCompanyService.GetCompanyLocationById(CompanyIdRequest);
                var locationReply = await _iLocationService.CreateLocation(new CreateLocationRequest
                {
                    AddressLineOne = createCompanyDTO.CompanyAddress.AddressLineOne,
                    AddressLineThree = createCompanyDTO.CompanyAddress.AddressLineThree,
                    AddressLineTwo = createCompanyDTO.CompanyAddress.AddressLineTwo,
                    AddressTypeId = createCompanyDTO.CompanyAddress.AddressTypeId,
                    CityId = createCompanyDTO.CompanyAddress.CityId,
                    ZipCode = createCompanyDTO.CompanyAddress.ZipCode,
                    ExistingLocationIds = companyLocations.Response != null && companyLocations.Response.Any() ? new List<long>(companyLocations.Response) : new List<long>()
                }); ;
                if (locationReply.Status)
                {
                    var companyLocReply = await _iCompanyService.CreateCompanyLocation(new CompanyLocationDTO
                    {
                        CompanyId = companyReply.CompanyId,
                        LocationId = locationReply.LocationId
                    });
                    if (companyLocReply.Status)
                    {
                        createCompanyResponse.Status = true;
                        createCompanyResponse.Message = "Company Added Successfully";
                        createCompanyResponse.CompanyId = companyReply.CompanyId;
                    }
                    else
                    {
                        createCompanyResponse.Status = true;
                        createCompanyResponse.Message = "Failed to add Address";
                        createCompanyResponse.CompanyId = companyReply.CompanyId;
                    }
                }
                else
                {
                    createCompanyResponse.Status = false;
                    createCompanyResponse.Message = locationReply.Message;
                }
            }
            else
            {
                createCompanyResponse.Status = false;
                createCompanyResponse.Message = companyReply.Message;
            }


            return createCompanyResponse;
        }
        public async Task<GRPCReply<bool>> CreateDSAUser(CreateDSAUserRequest request, string UserId, long ProductId, long CompanyId)
        {
            var reply = new GRPCReply<bool>();
            reply.Message = "Something went wrong.";
            reply.Response = false;
            if (request != null && !string.IsNullOrEmpty(request.MobileNumber) && !string.IsNullOrEmpty(UserId))
            {
                List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>>
                {
                    //new KeyValuePair<string, string>("companyid",existSalesAgentData.Response.AnchorCompanyId.ToString()),
                    // new KeyValuePair<string, string>("productid",existSalesAgentData.Response.ProductId.ToString())
                };
                var user = await _iIdentityService.CreateUserWithToken(new CreateUserRequest
                {
                    EmailId = request.EmailId,
                    MobileNo = request.MobileNumber,
                    Password = "Sc@" + request.MobileNumber,
                    UserName = request.MobileNumber,
                    UserRoles = new List<string> { UserRoleConstants.SalesAgent },
                    UserType = UserTypeConstants.Customer,
                    Claims = Claims
                });

                if (!string.IsNullOrEmpty(user.UserId))
                {
                    GRPCRequest<CreateDSAUser> _request = new GRPCRequest<CreateDSAUser>
                    {
                        Request = new CreateDSAUser
                        {
                            MobileNumber = request.MobileNumber,
                            CreateBy = UserId, //DSA UserId
                            UserId = user.UserId,
                            EmailId = request.EmailId,
                            FullName = request.FullName,
                            PayoutPercenatge = request.PayoutPercenatge,
                            CompanyId = CompanyId,
                            ProductId = ProductId
                        }
                    };
                    var productReply = await _iProductService.CreateDSAUser(_request);
                    if (productReply.Status)
                    {
                        await _iCompanyService.AddUpdateCompanyUserMapping(new AddCompanyUserMappingRequest
                        {
                            CompanyId = CompanyId,
                            UserId = user.UserId
                        });
                        string anchorName = "";
                        var companyDetail = await _iCompanyService.GetCompany(new GRPCRequest<long> { Request = CompanyId });
                        if (companyDetail != null && companyDetail.Response != null)
                            anchorName = companyDetail.Response.CompanyName;

                        //DateConvertHelper _DateConvertHelper = new DateConvertHelper();
                        //var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
                        await _iLoanAccountService.AddDSALoanAccount(new GRPCRequest<AddDSALoanAccountRequest>
                        {
                            Request = new AddDSALoanAccountRequest
                            {
                                LeadId = 0,
                                UserId = user.UserId,
                                AnchorCompanyId = CompanyId,
                                AnchorName = anchorName,
                                LeadCode = "",
                                ProductId = ProductId,
                                ProductType = ProductTypeConstants.BusinessLoan,
                                AgreementRenewalDate = DateTime.Now,
                                ApplicationDate = DateTime.Now,
                                DisbursalDate = DateTime.Now,
                                CityName = "",
                                CustomerImage = "",
                                CustomerName = request.FullName,
                                ShopName = "",
                                MobileNo = request.MobileNumber,
                                Type = DSAProfileTypeConstants.DSAUser,
                                IsAccountActive = true,
                                IsBlock = false,
                                IsDefaultNBFC = false,
                                NBFCCompanyCode = "",
                                NBFCCompanyId = 0,
                                ThirdPartyLoanCode = ""
                            }
                        });
                    }

                    reply.Status = productReply.Status;
                    reply.Message = productReply.Message;
                    reply.Response = productReply.Response;
                }
            }
            return reply;
        }
        public async Task<GRPCReply<GetDSADashboardDetailResponse>> GetDSADashboardDetails(DSADashboardRequest request, string UserId, string UserRole, long ProductId)
        {
            GRPCReply<GetDSADashboardDetailResponse> reply = new GRPCReply<GetDSADashboardDetailResponse>
            {
                Message = "Data Not Found!!!",
                Response = new GetDSADashboardDetailResponse
                {
                    LeadOverviewData = new LeadOverviewDataDc(),
                    LoanOverviewData = new LoanOverviewDataDc(),
                    PayoutOverviewData = new PayoutOverviewDataDc()
                }
            };
            List<string> agentList = new List<string>();
            if (UserRole == UserRoleConstants.DSAAdmin)
            {
                if (string.IsNullOrEmpty(request.AgentUserId))
                {
                    var productReply = await _iProductService.GetDSASalesAgentList(new GRPCRequest<string> { Request = UserId });
                    if (productReply != null && productReply.Status && productReply.Response != null && productReply.Response.Any())
                    {
                        agentList = productReply.Response.Select(x => x.UserId).Distinct().ToList();
                    }
                    agentList.Add(UserId);
                }
                else
                {
                    agentList.Add(request.AgentUserId);
                }
            }
            else if (UserRole == UserRoleConstants.Connector || UserRole == UserRoleConstants.SalesAgent)
            {
                agentList.Add(UserId);
            }
            else
            {
                return new GRPCReply<GetDSADashboardDetailResponse> { Message = "You are not authorized." };
            }
            //LeadOverviewData
            var leadReply = await _iLeadService.GetDSALeadDashboardData(new GRPCRequest<DSADashboardLeadRequest>
            {
                Request = new DSADashboardLeadRequest
                {
                    AgentUserIds = agentList,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Skip = 0,
                    Take = 0,
                    IsPagination = false,
                    ProductId = ProductId
                }
            });
            if (leadReply != null && leadReply.Status)
            {
                List<string> leadStatusList = new List<string> { LeadStatusEnum.Initiate.ToString(), LeadStatusEnum.KYCInProcess.ToString(), LeadStatusEnum.KYCSuccess.ToString(), LeadStatusEnum.KYCReject.ToString(), LeadStatusEnum.Submitted.ToString(), LeadStatusEnum.Rejected.ToString() };
                reply.Response.LeadOverviewData = new LeadOverviewDataDc
                {
                    TotalLeads = leadReply.Response.Count(x => leadStatusList.Any(y => y == x.Status)),
                    Pending = leadReply.Response.Count(x => x.Status == LeadStatusEnum.Initiate.ToString() || x.Status == LeadStatusEnum.KYCInProcess.ToString() || x.Status == LeadStatusEnum.KYCSuccess.ToString() || x.Status == LeadStatusEnum.KYCReject.ToString()),
                    Rejected = leadReply.Response.Count(x => x.Status == LeadStatusEnum.Rejected.ToString()),
                    Submitted = leadReply.Response.Count(x => x.Status == LeadStatusEnum.Submitted.ToString()),
                    SuccessRate = 0
                };
                if (reply.Response.LeadOverviewData.TotalLeads > 0)
                    reply.Response.LeadOverviewData.SuccessRate = Math.Round((Convert.ToDouble(reply.Response.LeadOverviewData.Submitted) / reply.Response.LeadOverviewData.TotalLeads) * 100, 2);

                //LoanOverviewData
                List<string> loanStatusList = new List<string> { LeadBusinessLoanStatusConstants.Pending, LeadBusinessLoanStatusConstants.LoanApproved, LeadBusinessLoanStatusConstants.LoanInitiated, LeadBusinessLoanStatusConstants.LoanActivated, LeadBusinessLoanStatusConstants.LoanRejected, LeadBusinessLoanStatusConstants.LoanAvailed };
                reply.Response.LoanOverviewData = new LoanOverviewDataDc
                {
                    TotalLoans = leadReply.Response.Count(x => loanStatusList.Any(y => y == x.Status)),
                    Pending = leadReply.Response.Count(x => x.Status == LeadBusinessLoanStatusConstants.Pending || x.Status == LeadBusinessLoanStatusConstants.LoanApproved || x.Status == LeadBusinessLoanStatusConstants.LoanInitiated || x.Status == LeadBusinessLoanStatusConstants.LoanActivated),
                    Rejected = leadReply.Response.Count(x => x.Status == LeadBusinessLoanStatusConstants.LoanRejected),
                    Approved = leadReply.Response.Count(x => x.Status == LeadBusinessLoanStatusConstants.LoanAvailed),
                    SuccessRate = 0
                };
                if (reply.Response.LoanOverviewData.TotalLoans > 0)
                    reply.Response.LoanOverviewData.SuccessRate = Math.Round((Convert.ToDouble(reply.Response.LoanOverviewData.Approved) / reply.Response.LoanOverviewData.TotalLoans) * 100, 2);


                //PayoutOverviewData
                var loanReply = await _iLoanAccountService.GetDSALoanDashboardData(new GRPCRequest<DSALoanDashboardDataRequest>
                {
                    Request = new DSALoanDashboardDataRequest
                    {
                        LeadIds = leadReply.Response.Select(x => x.LeadId).ToList(),
                        AgentUserId = string.IsNullOrEmpty(request.AgentUserId) ? UserId : request.AgentUserId,
                        ProductId = ProductId,
                        Skip = 0,
                        Take = 0
                    }
                });
                if (loanReply != null && loanReply.Response != null)
                {
                    reply.Response.PayoutOverviewData.TotalDisbursedAmount = loanReply.Response.TotalDisbursedAmount;
                    reply.Response.PayoutOverviewData.PayoutAmount = loanReply.Response.TotalPayoutAmount;
                }
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }

        public async Task<GRPCReply<List<DSADashboardLeadResponse>>> GetDSADashboardLeadList(DSADashboardLeadListRequest request, string UserId, string UserRole, long ProductId)
        {
            GRPCReply<List<DSADashboardLeadResponse>> reply = new GRPCReply<List<DSADashboardLeadResponse>> { Message = "Data Not Found!!!" };
            List<string> agentList = new List<string>();
            if (UserRole == UserRoleConstants.DSAAdmin)
            {
                if (string.IsNullOrEmpty(request.AgentUserId))
                {
                    var productReply = await _iProductService.GetDSASalesAgentList(new GRPCRequest<string> { Request = UserId });
                    if (productReply != null && productReply.Status && productReply.Response != null && productReply.Response.Any())
                    {
                        agentList = productReply.Response.Select(x => x.UserId).Distinct().ToList();
                    }
                    agentList.Add(UserId);
                }
                else
                {
                    agentList.Add(request.AgentUserId);
                }
            }
            else if (UserRole == UserRoleConstants.Connector || UserRole == UserRoleConstants.SalesAgent)
            {
                agentList.Add(UserId);
            }
            else
            {
                return new GRPCReply<List<DSADashboardLeadResponse>> { Message = "You are not authorized." };
            }

            reply = await _iLeadService.GetDSALeadDashboardData(new GRPCRequest<DSADashboardLeadRequest>
            {
                Request = new DSADashboardLeadRequest
                {
                    AgentUserIds = agentList,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Skip = request.Skip,
                    Take = request.Take,
                    IsPagination = true,
                    ProductId = ProductId
                }
            });
            if (reply.Response != null && reply.Status)
            {
                var salesAgentData = await _iProductService.GetSalesAgentDetailsByUserIds(new GRPCRequest<List<string>> { Request = agentList });
                if (salesAgentData != null && salesAgentData.Response != null && salesAgentData.Status)
                {
                    foreach (var lead in reply.Response)
                    {
                        var agent = salesAgentData.Response.FirstOrDefault(y => y.UserId == lead.AgentUserId);
                        lead.AgentFullName = agent != null ? agent.FullName : "";
                    }
                }
            }
            return reply;
        }
        public async Task<GRPCReply<DSADashboardPayoutResponse>> GetDSADashboardPayoutList(DSADashboardLeadListRequest request, string UserId, string UserRole, long ProductId)
        {
            GRPCReply<DSADashboardPayoutResponse> reply = new GRPCReply<DSADashboardPayoutResponse>
            {
                Message = "Data Not Found!!!",
                Response = new DSADashboardPayoutResponse()
            };
            List<string> agentList = new List<string>();
            if (UserRole == UserRoleConstants.DSAAdmin)
            {
                if (string.IsNullOrEmpty(request.AgentUserId))
                {
                    var productReply = await _iProductService.GetDSASalesAgentList(new GRPCRequest<string> { Request = UserId });
                    if (productReply != null && productReply.Status && productReply.Response != null && productReply.Response.Any())
                    {
                        agentList = productReply.Response.Select(x => x.UserId).Distinct().ToList();
                    }
                    agentList.Add(UserId);
                }
                else
                {
                    agentList.Add(request.AgentUserId);
                }
            }
            else if (UserRole == UserRoleConstants.Connector || UserRole == UserRoleConstants.SalesAgent)
            {
                agentList.Add(UserId);
            }
            else
            {
                return new GRPCReply<DSADashboardPayoutResponse> { Message = "You are not authorized." };
            }
            var leadReply = await _iLeadService.GetDSALeadDashboardData(new GRPCRequest<DSADashboardLeadRequest>
            {
                Request = new DSADashboardLeadRequest
                {
                    AgentUserIds = agentList,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Skip = 0,
                    Take = 0,
                    IsPagination = false,
                    ProductId = ProductId
                }
            });
            if (leadReply != null && leadReply.Response != null && leadReply.Response.Any())
            {
                reply = await _iLoanAccountService.GetDSALoanPayoutList(new GRPCRequest<DSALoanDashboardDataRequest>
                {
                    Request = new DSALoanDashboardDataRequest
                    {
                        LeadIds = leadReply.Response.Select(x => x.LeadId).ToList(),
                        AgentUserId = string.IsNullOrEmpty(request.AgentUserId) ? UserId : request.AgentUserId,
                        ProductId = ProductId,
                        Skip = request.Skip,
                        Take = request.Take
                    }
                });
                if (reply.Status && reply.Response.LoanPayoutDetailList != null && reply.Response.LoanPayoutDetailList.Any())
                {
                    reply.Response.LoanPayoutDetailList.ForEach(x => x.Status = leadReply.Response.First(y => y.LeadId == x.LeadId).Status);
                }
            }
            return reply;
        }
        public async Task<DSALeadListPageListDTO> GetDSALeadForListPage(DSALeadListPageRequest LeadListPageRequest)
        {
            List<long> ProductIds = new List<long>();
            if (LeadListPageRequest != null && LeadListPageRequest.ProductType != null)
            {
                GRPCRequest<string> gRPCRequest = new GRPCRequest<string>();

                gRPCRequest.Request = LeadListPageRequest.ProductType;
                var productList = await _iProductService.GetDSAProductByProductType(gRPCRequest);
                if (productList != null && productList.Response != null && productList.Status)
                {

                    foreach (var product in productList.Response)
                    {
                        ProductIds.Add(product.ProductId);
                    }
                }
            }
            var leadslist = new LeadListPageReply();
            LeadListPageRequest newLeadListPageRequest = new LeadListPageRequest();

            newLeadListPageRequest.ProductId = ProductIds;
            newLeadListPageRequest.Status = LeadListPageRequest.Status;
            newLeadListPageRequest.FromDate = LeadListPageRequest.FromDate;
            newLeadListPageRequest.ToDate = LeadListPageRequest.ToDate;
            newLeadListPageRequest.CityId = LeadListPageRequest.CityId;
            newLeadListPageRequest.Keyword = LeadListPageRequest.Keyword;
            newLeadListPageRequest.ProductType = LeadListPageRequest.ProductType;
            newLeadListPageRequest.Skip = LeadListPageRequest.Skip;
            newLeadListPageRequest.Take = LeadListPageRequest.Take;
            newLeadListPageRequest.isDelete = LeadListPageRequest.isDelete;


            leadslist = await _iLeadService.GetDSALeadForListPage(newLeadListPageRequest);


            DSALeadListPageListDTO LeadListPagelistdata = new DSALeadListPageListDTO();

            List<DSALeadListPageDTO> list = new List<DSALeadListPageDTO>();

            if (leadslist.LeadListDetails != null)
            {
                GRPCRequest<KYCSpecificDetailRequest> request = new GRPCRequest<KYCSpecificDetailRequest>();
                request.Request = new KYCSpecificDetailRequest();
                request.Request.KYCReqiredFieldList = new Dictionary<string, List<string>>();
                request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.PAN, new List<string>
                {
                    KYCDetailConstants.NameOnCard
                });
                request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.Aadhar, new List<string>
                {
                    KYCAadharConstants.LocationId
                });
                request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.ConnectorPersonalDetail, new List<string>
                {
                    ConnectorPersonalDetailConstants.EmailId, ConnectorPersonalDetailConstants.AlternateContactNumber,ConnectorPersonalDetailConstants.WorkingLocation//ConnectorPersonalDetailConstants
                });
                request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.DSAPersonalDetail, new List<string>
                {
                    DSAPersonalDetailConstants.FirmType,DSAPersonalDetailConstants.CompanyName,DSAPersonalDetailConstants.WorkingLocation
                    ,DSAPersonalDetailConstants.CurrentAddressId
                });
                request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.DSAProfileType, new List<string>
                {
                    DSAProfileTypeConstants.DSA
                });

                request.Request.UserIdList = leadslist.LeadListDetails.Select(x => new KYCSpecificDetailUserRequest { ProductCode = x.ProductCode, UserId = x.UserId }).ToList();


                var response = await _ikYCGrpcService.GetKYCSpecificDetail(request);
                var AddressIds = new List<long>();

                var Userdetails = leadslist.LeadListDetails.Select(x =>
                new
                {
                    x.Id,
                    x.UserId,
                    x.MobileNo,
                    x.CreatedDate,
                    x.ScreenName,
                    x.SequenceNo,
                    x.LastModified,
                    x.LeadCode,
                    x.ActivityId,
                    x.SubActivityId,
                    x.CreditScore,
                    x.Status,
                    x.AnchorName,
                    x.UniqueCode,
                    x.CityId,
                    x.LeadConvertor,
                    x.LeadGenerator,
                    x.CreditLimit,
                    x.Loan_app_id,
                    x.Partner_Loan_app_id,
                    x.ProductCode,
                    x.ArthmateResponse,
                    x.IsActive,
                    x.AnchorCompanyId,
                    x.SalesAgentCommissions,
                    x.AgreementStartDate,
                    x.AgreementEndDate
                }).ToList();

                if (Userdetails != null)
                {
                    foreach (var i in Userdetails)
                    {
                        DSALeadListPageDTO leadListPageDTO = new DSALeadListPageDTO();
                        leadListPageDTO.LeadId = i.Id;
                        leadListPageDTO.UserId = i.UserId;
                        leadListPageDTO.MobileNo = i.MobileNo;
                        leadListPageDTO.CreatedDate = i.CreatedDate;
                        leadListPageDTO.SequenceNo = i.SequenceNo;
                        leadListPageDTO.LastModified = i.LastModified;
                        leadListPageDTO.LeadCode = i.LeadCode;
                        leadListPageDTO.Status = i.Status;
                        leadListPageDTO.ScreenName = i.ScreenName;
                        leadListPageDTO.AnchorName = i.AnchorName;
                        leadListPageDTO.UniqueCode = i.UniqueCode;
                        leadListPageDTO.CityId = i.CityId;
                        leadListPageDTO.LeadGenerator = i.LeadGenerator;
                        leadListPageDTO.LeadConvertor = i.LeadConvertor;
                        leadListPageDTO.ProductCode = i.ProductCode;
                        leadListPageDTO.RejectionReasons = i.ArthmateResponse;
                        leadListPageDTO.IsActive = i.IsActive;
                        leadListPageDTO.AnchorCompanyId = i.AnchorCompanyId;
                        leadListPageDTO.SalesAgentCommissions = i.SalesAgentCommissions;

                        leadListPageDTO.AgreementStartDate = i.AgreementStartDate != null ? i.AgreementStartDate : null;
                        leadListPageDTO.AgreementEndDate = i.AgreementEndDate != null ? i.AgreementEndDate : null;


                        if (response != null && response.Response != null)
                        {
                            var user = response.Response.GetValueOrDefault(i.UserId);

                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.PAN && x.FieldName == KYCDetailConstants.NameOnCard))
                            {
                                leadListPageDTO.CustomerName = user.First(x => x.MasterCode == KYCMasterConstants.PAN && x.FieldName == KYCDetailConstants.NameOnCard).FieldValue;
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.ConnectorPersonalDetail && x.FieldName == ConnectorPersonalDetailConstants.EmailId))
                            {
                                leadListPageDTO.EmailId = user.First(x => x.MasterCode == KYCMasterConstants.ConnectorPersonalDetail && x.FieldName == ConnectorPersonalDetailConstants.EmailId).FieldValue;
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.ConnectorPersonalDetail && x.FieldName == ConnectorPersonalDetailConstants.AlternateContactNumber))
                            {
                                leadListPageDTO.AlternatePhoneNo = user.First(x => x.MasterCode == KYCMasterConstants.ConnectorPersonalDetail && x.FieldName == ConnectorPersonalDetailConstants.AlternateContactNumber).FieldValue;
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.DSAPersonalDetail && x.FieldName == DSAPersonalDetailConstants.CompanyName))
                            {
                                leadListPageDTO.CompanyName = user.First(x => x.MasterCode == KYCMasterConstants.DSAPersonalDetail && x.FieldName == DSAPersonalDetailConstants.CompanyName).FieldValue;

                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.DSAPersonalDetail && x.FieldName == DSAPersonalDetailConstants.FirmType))
                            {
                                leadListPageDTO.FirmType = user.First(x => x.MasterCode == KYCMasterConstants.DSAPersonalDetail && x.FieldName == DSAPersonalDetailConstants.FirmType).FieldValue;

                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.Aadhar && x.FieldName == KYCAadharConstants.LocationId))
                            {
                                leadListPageDTO.AddressId = long.Parse(user.First(x => x.MasterCode == KYCMasterConstants.Aadhar && x.FieldName == KYCAadharConstants.LocationId).FieldValue);
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.DSAProfileType && x.FieldName == DSAProfileTypeConstants.DSA))
                            {
                                leadListPageDTO.profileType = user.First(x => x.MasterCode == KYCMasterConstants.DSAProfileType && x.FieldName == DSAProfileTypeConstants.DSA).FieldValue;

                                if (leadListPageDTO.profileType == DSAProfileTypeConstants.Connector && user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.ConnectorPersonalDetail && x.FieldName == ConnectorPersonalDetailConstants.WorkingLocation))
                                {
                                    leadListPageDTO.WorkingLocation = user.First(x => x.MasterCode == KYCMasterConstants.ConnectorPersonalDetail && x.FieldName == ConnectorPersonalDetailConstants.WorkingLocation).FieldValue;
                                }
                                else if (leadListPageDTO.profileType == DSAProfileTypeConstants.DSA && user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.DSAPersonalDetail && x.FieldName == DSAPersonalDetailConstants.WorkingLocation))
                                {
                                    leadListPageDTO.WorkingLocation = user.First(x => x.MasterCode == KYCMasterConstants.DSAPersonalDetail && x.FieldName == DSAPersonalDetailConstants.WorkingLocation).FieldValue;
                                }
                                if (leadListPageDTO.profileType == DSAProfileTypeConstants.Connector && user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.Aadhar && x.FieldName == KYCAadharConstants.LocationId))
                                {
                                    leadListPageDTO.AddressId = long.Parse(user.First(x => x.MasterCode == KYCMasterConstants.Aadhar && x.FieldName == KYCAadharConstants.LocationId).FieldValue);
                                }
                                else if (leadListPageDTO.profileType == DSAProfileTypeConstants.DSA && user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.DSAPersonalDetail && x.FieldName == DSAPersonalDetailConstants.CurrentAddressId))
                                {
                                    leadListPageDTO.AddressId = long.Parse(user.First(x => x.MasterCode == KYCMasterConstants.DSAPersonalDetail && x.FieldName == DSAPersonalDetailConstants.CurrentAddressId).FieldValue);
                                }
                            }
                        }

                        list.Add(leadListPageDTO);
                    }
                }
                int convertedId;
                AddressIds = list.Select(x => x.AddressId ?? 0).Distinct().ToList();
                var cityids = list.Where(x => x.WorkingLocation != null && int.TryParse(x.WorkingLocation, out convertedId)).Select(x => Convert.ToInt64(x.WorkingLocation)).Distinct().ToList();

                //if (cityids != null)
                //{
                //    AddressIds.AddRange(cityids);
                //}
                if (AddressIds != null && AddressIds.Any())
                {
                    var citylist = await _iLocationService.GetCityListByIds(new GRPCRequest<List<long>> { Request = cityids }); //workingLocation
                    var AddressCitylist = await _iLocationService.GetCompanyAddress(AddressIds);
                    if (list.Any())
                    {
                        if (citylist != null && citylist.Status)
                        {
                            var cityData = citylist.Response.ToList();
                            var userlist = list.Where(x => x.AddressId > 0).ToList();

                            if (userlist != null)
                            {
                                foreach (var userData in list)
                                {
                                    //if (userData.WorkingLocationId > 0)
                                    //{
                                    // userData.CityName = addressData.Where(x => x.cityId == userData.AddressId).Select(y => y.cityName ?? "").FirstOrDefault();
                                    if (int.TryParse(userData.WorkingLocation, out convertedId))
                                    {
                                        var cid = Convert.ToInt64(userData.WorkingLocation);
                                        userData.WorkingLocationId = cid;
                                        userData.WorkingLocation = cityData.Where(x => x.cityId == cid).Select(y => y.cityName ?? "").FirstOrDefault();
                                    }
                                    //}
                                }
                            }
                        }
                        if (AddressCitylist != null && AddressCitylist.Status)
                        {
                            var addressData = AddressCitylist.GetAddressDTO.ToList();
                            var userlist = list.Where(x => x.AddressId > 0).ToList();

                            if (userlist != null)
                            {
                                foreach (var userData in list)
                                {
                                    if (userData.AddressId > 0 && userData.AddressId != null)
                                    {
                                        userData.CityId = addressData.Where(x => x.Id == userData.AddressId).Select(y => y.CityId).FirstOrDefault();
                                        userData.CityName = addressData.Where(x => x.Id == userData.AddressId).Select(y => y.CityName ?? "").FirstOrDefault();

                                    }
                                }
                            }
                        }
                    }
                }

            }
            //City Filter
            list = list.Where(x => LeadListPageRequest.CityId == 0 || x.WorkingLocationId == LeadListPageRequest.CityId || x.CityId == LeadListPageRequest.CityId).ToList();

            if (LeadListPageRequest != null)
            {
                if (LeadListPageRequest != null && LeadListPageRequest.ProductType != "All")
                {
                    list = list.Where(x => x.profileType == LeadListPageRequest.ProductType).ToList();
                }
                if (LeadListPageRequest != null && !string.IsNullOrEmpty(LeadListPageRequest.Keyword))
                {
                    list = list.Where(x => (x.CustomerName != null && x.CustomerName.ToLower().Contains(LeadListPageRequest.Keyword.Trim().ToLower())) ||
                   (x.MobileNo != null && x.MobileNo.Contains(LeadListPageRequest.Keyword.Trim()))).ToList();
                }
                if (LeadListPageRequest != null && !string.IsNullOrEmpty(LeadListPageRequest.CityName))
                {
                    list = list.Where(x => x.CityName != null && x.CityName.ToLower().Contains(LeadListPageRequest.CityName.ToLower())).ToList();
                }
                LeadListPagelistdata.TotalCount = list.Count();
                if (LeadListPageRequest.Take != 0)
                {
                    list = list.Skip((LeadListPageRequest.Skip - 1) * LeadListPageRequest.Take).Take(LeadListPageRequest.Take).ToList();
                }
            }

            LeadListPagelistdata.leadListPageDTO = list;
            return LeadListPagelistdata;
        }


        public async Task<DSAProfileTypeResponse> GetDSAProfileType(string UserId, long leadId, string productCode)
        {
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.DSAProfileType,
                KYCMasterConstants.ConnectorPersonalDetail,
                KYCMasterConstants.DSAPersonalDetail
            };

            DSAProfileTypeResponse dSAProfileTypeResponse = new DSAProfileTypeResponse();
            dSAProfileTypeResponse.Status = false;
            dSAProfileTypeResponse.Message = "DSA Profile not found.";

            var reply = await _kYCUserDetailManager.GetLeadDetailAll(UserId, productCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            //reply.DSAPersonalDetail
            //reply.ConnectorPersonalDetail

            if (reply != null && reply.DSAProfileInfo != null)
            {
                //var commissionConfig = await _iProductService.GetCommissionConfigByProductId(new GRPCRequest<List<long>>
                //{
                //    Request = new List<long>()
                //});

                //var response = await leadListDetailsManager.GetCreditLimitByLeadId(leadid);
                GRPCReply<LeadCreditLimit> crLimitRes = null;
                if (leadId != 0)
                {
                    crLimitRes = await _iLeadService.GetCreditLimitByLeadId(new GRPCRequest<long>
                    {
                        Request = leadId
                    });
                }

                dSAProfileTypeResponse = new DSAProfileTypeResponse
                {
                    DSAType = reply.DSAProfileInfo.DSAType,
                    Status = true,
                    Message = "DSA Profile found.",
                    ConnectorPersonalDetail = reply.ConnectorPersonalDetail,
                    DSAPersonalDetail = reply.DSAPersonalDetail,
                    PayoutPercentage = 0,
                    SalesAgentCommissions = crLimitRes != null && crLimitRes.Response != null && crLimitRes.Response.SalesAgentCommissions != null && crLimitRes.Response.SalesAgentCommissions.Any()
                            ? crLimitRes.Response.SalesAgentCommissions : new List<SalesAgentCommissionList>()
                    //response.Response.SalesAgentCommissions
                };

            }
            return dSAProfileTypeResponse;
        }

        public async Task<GRPCReply<string>> DSAGenerateAgreement(string UserId, long leadId, long ProductId, long CompanyId, bool IsSubmit) //type=connector or dsa
        {
            var DSAProfileTyperes = await GetDSAProfileType(UserId, leadId, ProductCodeConstants.DSA);
            GRPCReply<string> reply = new GRPCReply<string>();
            if (DSAProfileTyperes.ConnectorPersonalDetail != null || DSAProfileTyperes.DSAPersonalDetail != null)
            {
                var response = await _iLeadService.GetDSALeadDataById(new GRPCRequest<LeadRequestDataDC>
                {
                    Request = new LeadRequestDataDC { LeadId = leadId, Status = "" }
                });

                if (response?.Response?.SalesAgentCommissions?.Any() == true)
                {
                    DSAProfileTyperes.SalesAgentCommissions = response.Response.SalesAgentCommissions;
                }
                reply = await _ArthMateManager.eSignGenerateAgreement(leadId, ProductId, DSAProfileTyperes, IsSubmit);
                if (reply.Status && IsSubmit)
                {
                    GRPCRequest<GRPCHtmlConvertRequest> convertRequest = new GRPCRequest<GRPCHtmlConvertRequest> { Request = new GRPCHtmlConvertRequest { HtmlContent = reply.Response } };
                    #region HtmltoPdf

                    //byte[] pdf = null;
                    //pdf = Pdf
                    //      .From(reply.Response)
                    //      .OfSize(OpenHtmlToPdf.PaperSize.A4)
                    //      //.WithHeader("<div style='text-align: center; font-size: 12px;'>Header</div>")
                    //      //.WithFooter("<div style='text-align: center; font-size: 12px;'>Page {page} of {pages}</div>")
                    //      .WithTitle("Agreement")
                    //      .WithoutOutline()
                    //      .WithMargins(PaperMargins.All(0.0.Millimeters()))
                    //      .WithObjectSetting("footer.fontSize", "8")
                    //      .WithObjectSetting("footer.fontName", "times")
                    //      .WithObjectSetting("footer.center", "[page] of [topage]")
                    //      .WithGlobalSetting("margin.bottom", "3cm")
                    //      .WithGlobalSetting("margin.top", "2cm")
                    //      .Portrait()
                    //      .Comressed()
                    //      .Content();

                    //FileStream file = File.Create("C:\\Users\\SK\\Desktop\\Scaleup\\ApiGateways\\ScaleUP.ApiGateways.Aggregator\\Managers\\pdf.pdf");
                    //file.Write(pdf, 0, pdf.Length);
                    //file.Close();
                    #endregion


                    var mediaResponse = await _MediaManager.HtmlToPdf(convertRequest);
                    if (mediaResponse != null && mediaResponse.Response != null && !string.IsNullOrEmpty(mediaResponse.Response.PdfUrl))
                    {
                        reply = new GRPCReply<string> { Response = mediaResponse.Response.PdfUrl, Status = true, Message = "Agreement Submitted Successfully" };
                        GRPCRequest<EsignLeadAgreementDc> request = new GRPCRequest<EsignLeadAgreementDc>();
                        EsignLeadAgreementDc obj = new EsignLeadAgreementDc();
                        obj.LeadId = leadId;
                        obj.AgreementPdfUrl = reply.Response;
                        request.Request = obj;
                        var res1 = await _iLeadService.SaveDsaEsignAgreement(request);
                        if (res1.Status)
                        {
                            reply.Response = res1.Response;
                        }

                        reply.Message = res1.Message;
                        reply.Status = res1.Status;

                    }
                }
            }
            return reply;
        }

        public async Task<ConnectorPersonalDetailDTO> GetConnectorPersonalDetail(string UserId, string productCode)
        {
            return await _kYCUserDetailManager.GetConnectorPersonalDetail(UserId, productCode);
        }
        public async Task<DSAPersonalDetailDTO> GetDSAPersonalDetail(string UserId, string productCode)
        {
            return await _kYCUserDetailManager.GetDSAPersonalDetail(UserId, productCode);
        }
        public async Task<GRPCReply<string>> PrepareAgreement(string UserId, long LeadId)
        {
            GRPCReply<string> grpCReply = new GRPCReply<string>();
            GRPCRequest<long> request = new GRPCRequest<long>();
            request.Request = LeadId;
            var MasterCodeList = new List<string>{
                             KYCMasterConstants.ConnectorPersonalDetail,
                             KYCMasterConstants.DSAPersonalDetail,
                             KYCMasterConstants.PAN,
                             KYCMasterConstants.Aadhar,
                             KYCMasterConstants.Selfie
                        };

            var kYCUserDetail = await _kYCUserDetailManager.GetLeadDetailAll(UserId, ProductCodeConstants.DSA, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);

            if (kYCUserDetail != null && kYCUserDetail.aadharDetail != null && kYCUserDetail.panDetail != null && kYCUserDetail.SelfieDetail != null && (kYCUserDetail.DSAPersonalDetail != null || kYCUserDetail.ConnectorPersonalDetail != null))
            {
                kYCUserDetail.LeadId = LeadId;

                var updateRes = await _iLeadService.UpdateLeadPersonalDetail(new GRPCRequest<UserDetailsReply> { Request = kYCUserDetail });
                if (updateRes.Status)
                {
                    grpCReply = await _iLeadService.PrepareAgreement(request);
                }
                else
                {
                    grpCReply.Status = false;
                    grpCReply.Message = updateRes.Message;
                }
            }
            return grpCReply;
        }
        public async Task<GRPCReply<string>> eSignDocumentsAsync(GRPCRequest<eSignDocumentStatusDc> request)
        {
            GRPCReply<string> resp = new GRPCReply<string>();
            var gRPCReply = await _iLeadService.eSignDocumentsAsync(request);
            if (gRPCReply.Status)
            {
                byte[] file = Convert.FromBase64String(gRPCReply.Response);
                if (file.Length > 0)
                {
                    FileUploadRequest f = new FileUploadRequest();
                    f.FileStream = file;
                    f.SubFolderName = "eSignAgreement";
                    f.FileExtensionWithoutDot = "pdf";
                    GRPCRequest<FileUploadRequest> filerequest = new GRPCRequest<FileUploadRequest> { Request = f };

                    var res = await _iMediaService.SaveFile(filerequest);
                    DSALeadAgreementDc post = new DSALeadAgreementDc();
                    post.SignedFile = res.Response.ImagePath;
                    post.LeadId = request.Request.LeadId;
                    GRPCRequest<DSALeadAgreementDc> req = new GRPCRequest<DSALeadAgreementDc> { Request = post };
                    var response = await _iLeadService.updateLeadAgreement(req);
                    if (response.Status)
                    {
                        var isAcceptTerms = await _iLeadService.AddLeadConsent(request.Request.LeadId);
                    }
                    resp.Message = response.Message;
                    resp.Status = response.Status;
                }
            }
            else
            {
                resp.Message = gRPCReply.Message;
                resp.Status = gRPCReply.Status;
            }
            return resp;
        }


        public async Task<GRPCReply<LeadAggrementDetailReponse>> GetDSAAgreement(long leadId)
        {
            var aggrementDetail = await _iLeadService.GetDSAAggreementDetailByLeadId(new GRPCRequest<long> { Request = leadId });
            if (aggrementDetail.Response != null)
            {
                var activationLeadDetail = await _iProductService.GetSalesLeadActivationSatus(aggrementDetail.Response.UserName);
                aggrementDetail.Response.isActivation = activationLeadDetail.Response;
            }
            return aggrementDetail;
        }


        public async Task<GRPCReply<DSALeadListPageDTO>> GetDSALeadDataById(long LeadId, string Status)
        {
            GRPCReply<DSALeadListPageDTO> gRPCReply = new GRPCReply<DSALeadListPageDTO> { Message = "Data not found!!!" };

            var response = await _iLeadService.GetDSALeadDataById(new GRPCRequest<LeadRequestDataDC>
            {
                Request = new LeadRequestDataDC { LeadId = LeadId, Status = Status }
            });
            if (response != null && response.Response != null && !string.IsNullOrEmpty(response.Response.UserId))
            {
                var type = "";
                var selfieImage = "";
                List<string> MasterCodeList = new List<string> { KYCMasterConstants.DSAProfileType, KYCMasterConstants.Selfie };
                var kYCUserDetail = await _kYCUserDetailManager.GetLeadDetailAll(response.Response.UserId, ProductCodeConstants.DSA, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
                if (kYCUserDetail != null && kYCUserDetail.DSAProfileInfo != null)
                {
                    type = kYCUserDetail.DSAProfileInfo.DSAType;
                    if (kYCUserDetail.SelfieDetail != null)
                    {
                        selfieImage = kYCUserDetail.SelfieDetail.FrontImageUrl;
                    }
                }
                gRPCReply.Response = new DSALeadListPageDTO
                {
                    UserId = response.Response.UserId,
                    LeadId = response.Response.LeadId,
                    LeadCode = response.Response.LeadCode,
                    Status = response.Response.Status,
                    MobileNo = response.Response.MobileNo,
                    ProductCode = response.Response.ProductCode,
                    IsActive = response.Response.IsActive,
                    //PayoutPercentage = response.Response.PayoutPercentage,
                    SalesAgentCommissions = response.Response.SalesAgentCommissions,
                    profileType = type != null ? type : "",
                    SelfieImage = selfieImage,
                    IsDeleted = response.Response.IsDeleted
                };
                gRPCReply.Status = true;
                gRPCReply.Message = "Data found";
            }
            return gRPCReply;
        }

        public async Task<bool> eSginCallback(string requestcontent)
        {
            if (requestcontent != null)
            {
                //var req = JsonConvert.DeserializeObject<eSignWebhookResponseDc>(requestcontent);
                //if (req != null)
                {
                    eSignDocumentStatusDc obj = new eSignDocumentStatusDc();
                    obj.DocumentId = "";
                    obj.LeadId = 0;
                    obj.RequestJson = requestcontent;
                    GRPCRequest<eSignDocumentStatusDc> request = new GRPCRequest<eSignDocumentStatusDc> { Request = obj };
                    await eSignDocumentsAsync(request);
                }
            }

            return true;
        }


        public async Task<GRPCReply<bool>> CheckLeadCreatePermission(string MobileNo, string UserId, string UserRole, long ProductId)
        {
            GRPCReply<bool> reply = new GRPCReply<bool> { Message = "You are not authorized" };
            List<string> agentList = new List<string>();
            List<string> UserIdList = new List<string> { UserId };
            UserListDetailsRequest userReq = new UserListDetailsRequest
            {
                userIds = UserIdList,
                keyword = null,
                Skip = 0,
                Take = 10
            };
            var userReply = await _iIdentityService.GetUserById(userReq);
            var username = userReply.UserListDetails != null ? userReply.UserListDetails.Select(y => y.UserName).FirstOrDefault() : "";

            var productReply = await _iProductService.GetALLDSASalesAgentList(new GRPCRequest<string> { Request = UserId });

            if (productReply != null && productReply.Status && productReply.Response != null && productReply.Response.Any(x => x.UserId == UserId))
            {
                if (productReply.Response.Any(x => x.UserId == UserId && x.IsActive))
                {
                }
                else if (productReply.Response.Any(x => x.UserId == UserId && !x.IsActive))
                {
                    reply.Message = "You are DeActivated.Please contact to scaleup person.";
                    return reply;
                }
                else if (productReply.Response.Any(x => x.UserId == UserId && x.IsDeleted))
                {
                    reply.Message = "You are Rejected by ScaleUp.";
                    return reply;
                }
            }

            if (UserRole == UserRoleConstants.DSAAdmin)
            {
                if (productReply != null && productReply.Status && productReply.Response != null && productReply.Response.Any())
                {
                    agentList = productReply.Response.Where(x => !x.IsDeleted).Select(x => x.UserId).Distinct().ToList();
                }
                agentList.Add(UserId);
            }
            else if (UserRole == UserRoleConstants.Connector || UserRole == UserRoleConstants.SalesAgent)
            {
                agentList.Add(UserId);
            }
            else
            {
                return reply;
            }

            reply = await _iLeadService.CheckLeadCreatePermission(new GRPCRequest<CheckLeadCreatePermissionRequest>
            {
                Request = new CheckLeadCreatePermissionRequest
                {
                    AgentUserIds = agentList,
                    MobileNo = MobileNo,
                    ProductId = ProductId,
                    UserId = UserId,
                    UserName = username
                }
            });
            return reply;
        }

        public async Task<GRPCReply<bool>> GetDSAGSTExist(string UserId, string gst, string productCode)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool> { Message = "Data not found!!!" };

            var response = await _iCompanyService.GetGSTCompany(new GRPCRequest<string> { Request = gst });
            DSAGSTExistRequest dSAGSTExistRequest = new DSAGSTExistRequest();
            dSAGSTExistRequest.UserId = UserId;
            dSAGSTExistRequest.productCode = productCode;
            dSAGSTExistRequest.gst = gst;

            var res = await _kYCUserDetailManager.GetGSTExistinKYCForDSA(dSAGSTExistRequest);
            if (res.Status || response.Status)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Already Exist";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> DSADeactivate(long leadId, bool isActive, bool isReject)
        {
            GRPCReply<bool> res = new GRPCReply<bool>() { Message = "Record not exists" };
            if (leadId > 0)
            {
                var response = await _iLeadService.ActiviateDeActivateLeadInfoByLeadId(new GRPCRequest<ActivatDeActivateDSALeadRequest>
                {
                    Request = new ActivatDeActivateDSALeadRequest
                    {
                        LeadId = leadId,
                        isActive = isActive,
                        isReject = isReject
                    }
                });
                if (response.Status)
                {
                    var salesAgentActivate = await _iProductService.DSASalesAgentActivationReject(new GRPCRequest<ActivatDeActivateDSALeadRequest>
                    {
                        Request = new ActivatDeActivateDSALeadRequest
                        {
                            LeadId = leadId,
                            isActive = isActive,
                            isReject = isReject,
                            UserId = response.Response
                        }
                    });
                    var kycInfo = await _kYCUserDetailManager.RemoveKYCMasterInfo(new GRPCRequest<string> { Request = response.Response });
                    return salesAgentActivate;
                }
            }
            return res;
        }

        public async Task<GRPCReply<List<DSACityListDc>>> GetDSACityList(string ProfileType)
        {
            GRPCReply<List<DSACityListDc>> res = new GRPCReply<List<DSACityListDc>>() { Message = "Record not exists" };

            if (!string.IsNullOrEmpty(ProfileType))
            {
                var kycInfo = await _kYCUserDetailManager.GetDSACityList(new GRPCRequest<string> { Request = ProfileType });
                if (kycInfo != null && kycInfo.Status && kycInfo.Response.Any())
                {
                    var AddressIds = kycInfo.Response.Where(x => x.CityId > 0).Select(x => x.CityId).ToList();
                    var AddressList = await _iLocationService.GetCompanyAddress(AddressIds);

                    if (AddressList != null && AddressList.Status)
                    {
                        var addressData = AddressList.GetAddressDTO.ToList();

                        if (addressData.Any())
                        {
                            res.Response = new List<DSACityListDc>();
                            foreach (var address in addressData)
                            {
                                res.Response.Add(new DSACityListDc
                                {
                                    CityId = address.CityId,
                                    CityName = address.CityName
                                });
                            }
                        }
                    }
                }
            }
            else { res.Status = false; res.Message = "Product type can not be empty"; }
            return res;
        }

        public async Task<GRPCReply<string>> GetDSACurrentCode(string EntityName, string DSAType)
        {
            return await _iProductService.GetDSACurrentCode(new GRPCRequest<DSAEntityDc> { Request = new DSAEntityDc { EntityName = EntityName, DSAType = DSAType } });
        }


    }
}
