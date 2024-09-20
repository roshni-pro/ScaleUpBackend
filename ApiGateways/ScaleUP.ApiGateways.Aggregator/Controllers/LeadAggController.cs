
using AutoMapper.Execution;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.DTOs.DSA;
using ScaleUP.ApiGateways.Aggregator.Managers;
using ScaleUP.ApiGateways.Aggregator.Managers.NBFC;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;

namespace ScaleUP.ApiGateways.Aggregator.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LeadAggController : BaseController
    {
        private eNachAggManager nachAggManager;
        private LeadListDetailsManager leadListDetailsManager;
        private LeadMobileValidateManager leadMobileValidateManager;
        private KYCUserDetailManager kYCUserDetailManager;
        private LoanAccountManager _loanAccountManager;
        private MediaManager MediaManager;
        private CompanyManager companymanager;
        private ArthMateManager _ArthMateManager;
        private IProductService _productService;
        private IKycService _kycService;
        private ILeadService _leadService;
        private ILocationService _locationService;
        private IIdentityService _identityService;


        public LeadAggController(LeadListDetailsManager _leadListDetailsManager
            , LeadMobileValidateManager _leadMobileValidateManager
            , KYCUserDetailManager _kYCUserDetailManager
            , eNachAggManager _nachAggManager
            , LoanAccountManager loanAccountManager, MediaManager _mediaManager,
            CompanyManager _companymanager,
             ArthMateManager ArthMateManager,
            IProductService productService,
            IKycService kycService,
            ILeadService leadService,
            ILocationService locationService,
            IIdentityService identityService
            )
        {
            leadListDetailsManager = _leadListDetailsManager;
            leadMobileValidateManager = _leadMobileValidateManager;
            kYCUserDetailManager = _kYCUserDetailManager;
            nachAggManager = _nachAggManager;
            _loanAccountManager = loanAccountManager;
            MediaManager = _mediaManager;
            companymanager = _companymanager;
            _ArthMateManager = ArthMateManager;
            _productService = productService;
            _kycService = kycService;
            _leadService = leadService;
            _locationService = locationService;
            _identityService = identityService;
        }
        [HttpPost]
        [Route("LeadInitiate")]
        //[AllowAnonymous]
        public async Task<GRPCReply<InitiateLeadReply>> LeadInitiate(AnchorLeadInitiate initiateLeadDetail)
        {

            var result = await leadMobileValidateManager.LeadInitiate(initiateLeadDetail, Token);

            return result;
        }


        [HttpGet]
        [Route("ProductCompanyDetail")]
        [AllowAnonymous]
        public async Task<GRPCReply<CompanyProductDetail>> ProductCompanyDetail(string product, string company)
        {
            var result = await leadMobileValidateManager.GetCompanyProductDetail(product, company);
            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("LeadCurrentActivityAsync")]
        public async Task<InitialLeadResponse> LeadCurrentActivityAsync(InitialLeadDTO leadDTO)
        {
            var result = await leadListDetailsManager.LeadCurrentActivityAsync(leadDTO);
            if (result != null && result.LeadProductActivity != null && result.LeadProductActivity.Any())
            {
                foreach (var item in result.LeadProductActivity)
                {
                    item.SubActivityMasterId = item.SubActivityMasterId.HasValue ? item.SubActivityMasterId.Value : 0;
                }
            }
            return result;
        }

        [HttpGet]
        [Route("GenerateOtp")]
        [AllowAnonymous]
        public async Task<GenerateOTPResponse> GenerateOtp(string MobileNo, int companyId)
        {
            return await leadMobileValidateManager.GenerateOtp(MobileNo, companyId);
        }

        [HttpPost]
        [Route("LeadMobileValidate")]
        [AllowAnonymous]
        public async Task<LeadMobileResponse> LeadMobileValidate(LeadMobileValidate leadMobileValidate)
        {
            return await leadMobileValidateManager.LeadMobileValidate(leadMobileValidate, Token);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadPAN")]
        public async Task<PANDTO> GetLeadPAN(string UserId, string productCode)
        {
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.PAN
            };
            PANDTO leadPANDTO = new PANDTO();
            var reply = await kYCUserDetailManager.GetLeadDetailAll(UserId, productCode.ToString(), MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            if (reply != null && reply.panDetail != null)
            {
                leadPANDTO = new PANDTO
                {
                    documentId = reply.panDetail.DocumentId,
                    panCard = reply.panDetail.UniqueId,
                    panImagePath = reply.panDetail.FrontImageUrl,
                    fatherName = reply.panDetail.FatherName,
                    dob = reply.panDetail.DOB,
                    NameOnCard = reply.panDetail.NameOnCard,
                };
            }
            //var leadrequest = new GRPCRequest<string>();
            //leadrequest.Request = UserId;

            // var salesAgentNme = await _productService.GetSalesAgentNameByUserId(leadrequest);
            return leadPANDTO;
        }

        [HttpGet]
        [Route("GetLeadAadhar")]
        public async Task<LeadAadharDTO> GetLeadAadhar(string UserId, string productCode)
        {
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.Aadhar
            };

            LeadAadharDTO leadAadharDTO = new LeadAadharDTO();
            var reply = await kYCUserDetailManager.GetLeadDetailAll(UserId, productCode.ToString(), MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);

            if (reply != null && reply.aadharDetail != null)
            {
                leadAadharDTO = new LeadAadharDTO
                {
                    FrontImageUrl = reply.aadharDetail.FrontImageUrl,
                    FrontDocumentId = reply.aadharDetail.FrontDocumentId,
                    BackDocumentId = reply.aadharDetail.BackDocumentId,
                    BackImageUrl = reply.aadharDetail.BackImageUrl,
                    DocumentNumber = reply.aadharDetail.UniqueId
                };
            }
            return leadAadharDTO;
        }
        //Commented as discussed with pooja
        //[HttpGet]
        //[Route("GetLeadBankStatement")]
        //public async Task<BankStatementDTO> GetLeadBankStatement(string UserId, string productCode)
        //{
        //    //List<string> MasterCodeList = new List<string>
        //    //{
        //    //    KYCMasterConstants.BankStatement
        //    //};

        //    BankStatementDTO leadBankStatementDTO = new BankStatementDTO();
        //    var reply = await kYCUserDetailManager.GetLeadDetailAll(UserId, productCode);
        //    if (reply != null && reply.BankStatementDetail != null)
        //    {
        //        leadBankStatementDTO = new BankStatementDTO
        //        {
        //            accType = reply.BankStatementDetail.AccType,
        //            bankStatement = reply.BankStatementDetail.BankOrGSTImageUrl,
        //            borroBankAccNum = reply.BankStatementDetail.BorroBankAccNum,
        //            borroBankIFSC = reply.BankStatementDetail.BorroBankIFSC,
        //            borroBankName = reply.BankStatementDetail.BorroBankName,
        //            documentId = reply.BankStatementDetail.DocumentId,
        //            enquiryAmount = reply.BankStatementDetail.EnquiryAmount,
        //            pdfPassword = reply.BankStatementDetail.PdfPassword
        //        };
        //    }
        //    return leadBankStatementDTO;
        //}

        [HttpGet]
        [Route("GetLeadMSME")]
        public async Task<MSMEDTO> GetLeadMSME(string UserId, string productCode)
        {
            MSMEDTO leadMSMEDTO = new MSMEDTO();
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.BuisnessDetail,
                KYCMasterConstants.MSME
            };
            var reply = await kYCUserDetailManager.GetLeadDetailAll(UserId, productCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            if (reply != null && reply.MSMEDetail != null)
            {
                leadMSMEDTO = new MSMEDTO
                {
                    businessName = reply.MSMEDetail.BusinessName,
                    businessType = reply.MSMEDetail.BusinessType,
                    doi = reply.BuisnessDetail != null ? reply.BuisnessDetail.DOI : new DateTime(),
                    frontDocumentId = reply.MSMEDetail.FrontDocumentId,
                    msmeCertificateUrl = reply.MSMEDetail.MSMECertificateUrl,
                    msmeRegNum = reply.MSMEDetail.MSMERegNum,
                    vintage = reply.MSMEDetail.Vintage
                };
            }
            //else if (reply != null && reply.BuisnessDetail != null && reply.BuisnessDetail.BuisnessProof == "Udyog Aadhar Certificate")
            //{
            //    leadMSMEDTO = new MSMEDTO
            //    {
            //        businessName = reply.BuisnessDetail.BusinessName,
            //        businessType = reply.BuisnessDetail.BusEntityType,
            //        frontDocumentId = reply.BuisnessDetail.BuisnessProofDocId,
            //        msmeCertificateUrl = reply.BuisnessDetail.BuisnessProofUrl,
            //        msmeRegNum = reply.BuisnessDetail.BuisnessDocumentNo,
            //        vintage = 0,
            //        doi = reply.BuisnessDetail != null ? reply.BuisnessDetail.DOI : new DateTime(),
            //    };

            //}
            else
            {
                leadMSMEDTO = new MSMEDTO
                {
                    doi = reply.BuisnessDetail != null ? reply.BuisnessDetail.DOI : new DateTime(),
                };
            }
            return leadMSMEDTO;
        }
        [HttpGet]
        [Route("GetLeadPersonalDetail")]
        public async Task<LeadPersonalDetailDTO> GetLeadPersonalDetail(string UserId, string productCode)
        {
            return await kYCUserDetailManager.GetLeadPersonalDetail(UserId, productCode);
        }



        [HttpGet]
        [Route("GetLeadSelfie")]
        public async Task<LeadSelfieDTO> GetLeadSelfie(string UserId, string productCode)
        {
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.Selfie
            };

            LeadSelfieDTO leadSelfieDTO = new LeadSelfieDTO();
            leadSelfieDTO.Status = false;
            leadSelfieDTO.Message = "Lead Selfie not found.";

            var reply = await kYCUserDetailManager.GetLeadDetailAll(UserId, productCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            if (reply != null && reply.SelfieDetail != null)
            {
                leadSelfieDTO = new LeadSelfieDTO
                {
                    FrontDocumentId = reply.SelfieDetail.FrontDocumentId,
                    FrontImageUrl = reply.SelfieDetail.FrontImageUrl
                };
            }
            return leadSelfieDTO;
        }

        [HttpGet]
        [Route("GetLeadBusinessDetail")]
        public async Task<LeadBusinessDetailDTO> GetLeadBusinessDetail(string UserId, string productCode)
        {
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.BuisnessDetail
            };
            LeadBusinessDetailDTO leadBusinessDetailDTO = new LeadBusinessDetailDTO();
            var reply = await kYCUserDetailManager.GetLeadDetailAll(UserId, productCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            if (reply != null && reply.BuisnessDetail != null)
            {
                leadBusinessDetailDTO = new LeadBusinessDetailDTO
                {
                    doi = reply.BuisnessDetail.DOI,
                    addressLineOne = reply.BuisnessDetail.CurrentAddress.AddressLineOne,
                    addressLineThree = reply.BuisnessDetail.CurrentAddress.AddressLineThree,
                    addressLineTwo = reply.BuisnessDetail.CurrentAddress.AddressLineTwo,
                    busEntityType = reply.BuisnessDetail.BusEntityType,
                    busGSTNO = reply.BuisnessDetail.BusGSTNO,
                    businessName = reply.BuisnessDetail.BusinessName,
                    busPan = reply.BuisnessDetail.BusPan,
                    cityId = reply.BuisnessDetail.CurrentAddress.CityId,
                    stateId = reply.BuisnessDetail.CurrentAddress.StateId,
                    zipCode = reply.BuisnessDetail.CurrentAddress.ZipCode,
                    buisnessMonthlySalary = reply.BuisnessDetail.BuisnessMonthlySalary,
                    incomeSlab = reply.BuisnessDetail.IncomeSlab,
                    buisnessDocumentNo = reply.BuisnessDetail.BuisnessDocumentNo,
                    buisnessProof = reply.BuisnessDetail.BuisnessProof,
                    buisnessProofDocId = reply.BuisnessDetail.BuisnessProofDocId,
                    buisnessProofUrl = reply.BuisnessDetail.BuisnessProofUrl,
                    InquiryAmount = reply.BuisnessDetail.InquiryAmount,
                    SurrogateType = reply.BuisnessDetail.SurrogateType
                };
            }
            return leadBusinessDetailDTO;
        }

        [HttpGet]
        [Route("GetLeadBankStatementCreditLending")]
        public async Task<LeadStatementCreditLendingDTO> GetLeadBankStatementCreditLending(string UserId, string productCode)
        {
            LeadStatementCreditLendingDTO leadStatementCreditLendingDTO = new LeadStatementCreditLendingDTO();
            List<LeadBankStatementCreditLendingDTO> leadBankStatementCreditLendingDTO = new List<LeadBankStatementCreditLendingDTO>();
            List<LeadSarrogateStmtCLDTO> leadSarrogateStmtCLDTO = new List<LeadSarrogateStmtCLDTO>();
            //List<LeadITRStatementCreditLendingDTO> leadITRStatementCreditLendingDTO = new List<LeadITRStatementCreditLendingDTO>();
            List<string> MasterCodeList = new List<string>
            {
                KYCMasterConstants.BankStatementCreditLending
            };
            var reply = await kYCUserDetailManager.GetLeadDetailAll(UserId, productCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
            if (reply != null && reply.BankStatementCreditLendingDeail != null && reply.BankStatementCreditLendingDeail.StatementList != null)
            {
                //leadBankStatementCreditLendingDTO = reply.BankStatementCreditLendingDeail.StatementList.Select(x => new LeadBankStatementCreditLendingDTO
                //{
                //    ImageUrl = x.ImageUrl,
                //    DocumentId = x.DocumentId
                //}).ToList();

                leadBankStatementCreditLendingDTO = reply.BankStatementCreditLendingDeail.StatementList.Select(x => new LeadBankStatementCreditLendingDTO
                {
                    ImageUrl = x.ImageUrl,
                    DocumentId = x.DocumentId
                }).ToList();
                if (reply.BankStatementCreditLendingDeail.SurrogateType == "GST" || reply.BankStatementCreditLendingDeail.SurrogateType == "ITR")
                {
                    leadSarrogateStmtCLDTO = reply.BankStatementCreditLendingDeail.SarrogateStatementList.Select(x => new LeadSarrogateStmtCLDTO
                    {
                        ImageUrl = x.ImageUrl,
                        DocumentId = x.DocumentId
                    }).ToList();
                }
                //if(reply.BankStatementCreditLendingDeail.SurrogateType == "GST")
                //{
                //    leadGSTStatementCreditLendingDTO = reply.BankStatementCreditLendingDeail.GSTStatementList.Select(x => new LeadGSTStatementCreditLendingDTO
                //    {
                //        ImageUrl = x.ImageUrl,
                //        DocumentId = x.DocumentId
                //    }).ToList();
                //}
                //if (reply.BankStatementCreditLendingDeail.SurrogateType == "ITR")
                //{
                //    leadITRStatementCreditLendingDTO = reply.BankStatementCreditLendingDeail.ITRStatementList.Select(x => new LeadITRStatementCreditLendingDTO
                //    {
                //        ImageUrl = x.ImageUrl,
                //        DocumentId = x.DocumentId
                //    }).ToList();
                //}

                leadStatementCreditLendingDTO = new LeadStatementCreditLendingDTO
                {
                    LeadBankStatementCreditLendingDTO = leadBankStatementCreditLendingDTO,
                    LeadSarrogateDocDTO = leadSarrogateStmtCLDTO,
                    SurrogateType = reply.BankStatementCreditLendingDeail.SurrogateType
                };

            }
            return leadStatementCreditLendingDTO;
        }

        [HttpPost]
        [Route("GetLeadForListPage")]
        public async Task<LeadListPageListDTO> GetLeadForListPage(LeadListPageRequest LeadListPageRequest)
        {
            LeadListPageRequest.UserType = UserType;
            return await leadListDetailsManager.GetLeadForListPage(LeadListPageRequest);
        }

        [HttpGet]
        [Route("GetLeadDetails")]
        //LeadListPageDTO
        public async Task<dynamic> GetLeadDetails(string Usersid)
        {
            return await kYCUserDetailManager.GetLeadDetails(Usersid);
        }

        [HttpGet]
        [Route("GetLeadImageByDocumentId")]
        public async Task<LeadPANDTO> GetLeadPANImage(long DocumentId)
        {
            return await kYCUserDetailManager.GetLeadPANImage(DocumentId);
        }


        [HttpPost]
        [Route("InitiateLeadOffer")]
        public async Task<GRPCReply<bool>> InitiateLeadOffer(InitiateLeadOfferRequestDC request)
        {
            return await leadMobileValidateManager.InitiateLeadOffer(request);
        }
        [HttpGet]
        [Route("GetLeadDetailAll")]

        public async Task<UserDetailsReply> GetLeadDetailAll(string Usersid, long LeadId, long KycMasterCode)
        {
            GRPCRequest<long> leadid = new GRPCRequest<long>() { Request = LeadId };
            UserDetailsReply leadDetails = new UserDetailsReply();
            List<string> kycMasterCodes = new List<string>();
            kycMasterCodes = null;

            if (leadid.Request > 0)
            {
                var response = await leadListDetailsManager.GetCreditLimitByLeadId(leadid);

                if (response.Status && response.Response != null)
                {
                    leadDetails = await kYCUserDetailManager.GetLeadDetailAll(Usersid, response.Response.ProductCode, kycMasterCodes, IsGetBankStatementDetail: true, IsGetCreditBureau: false, IsGetAgreement: false);

                    if (leadDetails != null)
                    {
                        if (leadDetails.BuisnessDetail != null)
                        {
                            leadDetails.BuisnessDetail.VintageDays = response.Response.VintageDays;
                            leadDetails.BuisnessDetail.AnchorCompanyName = response.Response.AnchorCompanyName;
                            leadDetails.BuisnessDetail.BusinessVintageDays = response.Response.BusinessVintageDays;
                        }
                        if (leadDetails.ConnectorPersonalDetail != null && !string.IsNullOrEmpty(leadDetails.ConnectorPersonalDetail.WorkingLocation))
                        {
                            int convertedId;
                            if (int.TryParse(leadDetails.ConnectorPersonalDetail.WorkingLocation, out convertedId))
                            {
                                var cid = Convert.ToInt64(leadDetails.ConnectorPersonalDetail.WorkingLocation);

                                var cityData = await _locationService.GetCityById(new GRPCRequest<long> { Request = cid });

                                if (cityData != null && cityData.Response != null)
                                {
                                    //var workingLocation = cityData.GetAddressDTO.Where(x => x.Id == wid).Select(y => y.CityName ?? "").FirstOrDefault();
                                    //if (workingLocation != null)
                                    //{
                                    leadDetails.ConnectorPersonalDetail.WorkingLocation = cityData.Response.cityName;
                                    //}
                                }
                                else { leadDetails.ConnectorPersonalDetail.WorkingLocation = ""; }
                            }
                            else { leadDetails.ConnectorPersonalDetail.WorkingLocation = ""; }
                        }
                        else if (leadDetails.DSAPersonalDetail != null && !string.IsNullOrEmpty(leadDetails.DSAPersonalDetail.WorkingLocation))
                        {
                            int convertedId;
                            if (int.TryParse(leadDetails.DSAPersonalDetail.WorkingLocation, out convertedId))
                            {
                                var cid = Convert.ToInt64(leadDetails.DSAPersonalDetail.WorkingLocation);



                                var cityData = await _locationService.GetCityById(new GRPCRequest<long> { Request = cid });

                                if (cityData != null && cityData.Response != null)
                                {
                                    //var workingLocation = cityData.Response.Where(x => x.cityId == cid).Select(y => y.CityName ?? "").FirstOrDefault();
                                    //if (workingLocation != null)
                                    //{
                                    leadDetails.DSAPersonalDetail.WorkingLocation = cityData.Response.cityName;
                                    //}
                                }
                                else { leadDetails.DSAPersonalDetail.WorkingLocation = ""; }
                            }
                            else { leadDetails.DSAPersonalDetail.WorkingLocation = ""; }
                        }
                        leadDetails.SalesAgentCommissions = response.Response.SalesAgentCommissions;
                    }
                }
            }
            return leadDetails;
        }

        [HttpGet]
        [Route("GetLeadActivityProgressList")]
        public async Task<LeadActivityMasterProgressListReply> GetLeadActivityProgressList(long leadId)
        {
            return await leadListDetailsManager.GetLeadActivityProgressList(leadId);
        }

        [HttpPost]
        [Route("GetBureau")]
        public async Task<ExperianOTPRegistrationRequestDC> GetBureau(ExperianOTPRegistrationInput experianOTPRegistrationInput)
        {
            return await kYCUserDetailManager.GetBureau(experianOTPRegistrationInput);
        }

        //[HttpGet]
        //[Route("GeneratedOffer")]
        //public async Task<GRPCReply<bool>> GeneratedOffer()
        //{
        //    return await leadMobileValidateManager.GeneratedOffer();
        //}

        [HttpGet]
        [Route("GetCurrentNumber")]
        public async Task<GRPCReply<string>> GetCurrentNumber(string EntityName)
        {
            return await leadMobileValidateManager.GetCurrentNumber(EntityName);
        }

        [Route("eNachSendeMandateRequest")]
        [HttpPost]
        public async Task<eNachSendReqAggDTO> eNachSendeMandateRequest(eNachBankDetailAggDc EMandateBankDetail)
        {
            return await nachAggManager.eNachSendeMandateRequest(EMandateBankDetail);
        }

        [HttpGet]
        [Route("AcceptOffer")]
        public async Task<GRPCReply<bool>> AcceptOffer(long leadId)
        {
            return await leadMobileValidateManager.AcceptOffer(leadId);
        }

        [HttpGet]
        [Route("GetAgreemetDetail")]
        public async Task<PreAgreementResponse> GetAgreemetDetail(long leadId, bool IsAccept, long? companyId)
        {
            PreAgreementResponse reply = new PreAgreementResponse();
            string result = "";
            double creditlimit = 0;
            string LeadCode = "";
            NBFCAgreement agreement = new NBFCAgreement();
            GRPCRequest<long> leadid = new GRPCRequest<long>() { Request = leadId };


            var response = await leadListDetailsManager.GetCreditLimitByLeadId(leadid);

            if (!companyId.HasValue)
                companyId = response.Response.OfferCompanyId;
            var CompanyDetail = await _loanAccountManager.LeadCompanyConfig(leadId, response.Response.OfferCompanyId ?? 0);

            //For NBFC Only
            if (CompanyDetail.NBFCCompanyConfig.CustomerAgreementType == "URL")
            {
                var nbfccompany = await companymanager.GetCompany(response.Response.OfferCompanyId ?? 0);
                if (nbfccompany != null && nbfccompany.Response.IdentificationCode == CompanyIdentificationCodeConstants.BlackSoil)
                {
                    var leadagreement = await leadMobileValidateManager.GetLeadPreAgreement(leadId);
                    if (leadagreement != null && leadagreement.Status)
                    {
                        reply.Response = leadagreement.Response;
                        reply.Status = true;
                        reply.IsUrl = true;
                    }
                    else
                    {
                        reply.Message = "Something went wrong";
                    }
                }

            }
            else
            {
                //var LeadDetail = await kYCUserDetailManager.GetLeadDetailAll(response.Response.UserId);
                var LeadDetail = await kYCUserDetailManager.GetLeadPersonalDetail(response.Response.UserId, "");
                creditlimit = Convert.ToDouble(response.Response.CreditLimit);
                LeadCode = response.Response.LeadCode;

                if (LeadDetail != null && CompanyDetail != null && CompanyDetail.AnchorCompanyConfig != null)
                {
                    GRPCRequest<long> companyid = new GRPCRequest<long>() { Request = CompanyDetail.NBFCCompanyConfig.CompanyId };
                    var companyRes = await _loanAccountManager.GetCompanyLogo(companyid);

                    string LeadName = LeadDetail.FirstName + " " + LeadDetail.MiddleName + " " + LeadDetail.LastName;
                    agreement.NameOfBorrower = LeadName;
                    agreement.FatherName = LeadDetail.FatherName;
                    agreement.GstNumber = LeadDetail.BusGSTNO;
                    agreement.CreditLimit = creditlimit;
                    agreement.TransactionConvenienceFeeRate = Convert.ToDouble(CompanyDetail.AnchorCompanyConfig.AnnualInterestRate);
                    agreement.ProcessingFees = CompanyDetail.AnchorCompanyConfig.ProcessingFeeRate;
                    agreement.NetPF = CompanyDetail.AnchorCompanyConfig.ProcessingFeeRate;
                    agreement.DelayPenaltyFeeRate = CompanyDetail.AnchorCompanyConfig.DelayPenaltyRate;
                    agreement.BounceCharge = CompanyDetail.AnchorCompanyConfig.BounceCharge;
                    agreement.ApplicationDate = DateTime.Now;
                    agreement.address1 = LeadDetail.PermanentAddressLine1;
                    agreement.address2 = LeadDetail.PermanentAddressLine2;
                    agreement.CustomerAgreementDocId = Convert.ToInt64(CompanyDetail.AnchorCompanyConfig.AgreementDocId);
                    agreement.CustomerAgreementURL = CompanyDetail.AnchorCompanyConfig.AgreementURL; //$"{EnvironmentConstants.LeadUrl}/templates/agreement.html";//CompanyDetail.NBFCCompanyConfig.CustomerAgreementURL;
                    agreement.LeadCode = LeadCode;
                    agreement.IsAccept = IsAccept;
                    agreement.CompanyLogo = companyRes.Response.LogoURL;
                    agreement.GSTRate = companyRes.Response.GstRate;
                    agreement.ProcessingFeeType = CompanyDetail.AnchorCompanyConfig.ProcessingFeeType;
                    GRPCRequest<NBFCAgreement> Request = new GRPCRequest<NBFCAgreement> { Request = agreement };
                    var res = await leadListDetailsManager.CustomerNBFCAgreement(Request);

                    if (res != null && res.Response != null)
                    {
                        if (res.Response.html != null && IsAccept == false && res.Response.FileStream == null)
                        {
                            result = res.Response.html;
                            reply.Response = result;
                            return reply;
                        }

                        //GRPCRequest<FileUploadRequest> gRPCRequest = new GRPCRequest<FileUploadRequest>() { Request = res.Response };
                        //var data = await leadListDetailsManager.UploadNBFCAgreement(gRPCRequest);

                        GRPCHtmlConvertRequest htmlReq = new GRPCHtmlConvertRequest();
                        htmlReq.HtmlContent = res.Response.html;
                        GRPCRequest<GRPCHtmlConvertRequest> convertRequest = new GRPCRequest<GRPCHtmlConvertRequest>() { Request = htmlReq };

                        var pdfres = await MediaManager.HtmlToPdf(convertRequest);

                        var docid = "";//data.Response.Id;
                        var docurl = pdfres.Response;
                        LeadAgreementDc obj = new LeadAgreementDc
                        {
                            LeadId = leadId,
                            ExpiredOn = DateTime.Now,
                            DocumentId = docid.ToString(),
                            DocUrl = docurl.PdfUrl //data.Response.ImagePath,
                        };
                        GRPCRequest<LeadAgreementDc> request = new GRPCRequest<LeadAgreementDc>() { Request = obj };
                        var resutlt = await leadListDetailsManager.AddLeadAgreement(request);
                        result = "Success";
                        reply.Status = true;
                        reply.Message = result;
                    }
                }

            }
            return reply;
        }

        //[Route("eNachAddResponse")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<bool> eNachAddResponse(eNachRespDocAggDC eNachResponseDC)
        //{
        //    return await nachAggManager.eNachAddResponse(eNachResponseDC);
        //}


        [HttpGet]
        [Route("GetCustomerDetailUsingGST")]
        public async Task<LeadBusinessDetailDTO> GetCustomerDetailUsingGST(string GSTNO)
        {
            return await kYCUserDetailManager.GetCustomerDetailUsingGST(GSTNO);
        }

        [HttpGet]
        [Route("GetLeadName")]
        public async Task<GRPCReply<string>> GetLeadName(string UserId, string productCode)
        {
            return await kYCUserDetailManager.GetLeadName(UserId, productCode);
        }

        [HttpGet]
        [Route("GetDisbursementProposal")]
        public async Task<GRPCReply<LoanAccount_DisplayDisbursalAggDTO>> GetDisbursementProposal(long leadId)
        {
            double CreditLimit = 0;
            long? companyId = 0;
            string LeadCode = "";
            GRPCReply<LoanAccount_DisplayDisbursalAggDTO> res = new GRPCReply<LoanAccount_DisplayDisbursalAggDTO>();

            GRPCRequest<long> leadid = new GRPCRequest<long>() { Request = leadId };
            var response = await leadListDetailsManager.GetCreditLimitByLeadId(leadid);

            if (response != null)
            {
                companyId = response.Response.OfferCompanyId;
                CreditLimit = response.Response.CreditLimit ?? 0;
                LeadCode = response.Response.LeadCode;
                if (companyId.HasValue && response.Response.AnchorCompanyId.HasValue)
                {
                    res = await _loanAccountManager.GetAccountDisbursement(leadId, CreditLimit, LeadCode, companyId.Value, response.Response.AnchorCompanyId.Value);

                }
                else
                {
                    res.Status = false;
                    res.Message = "NBFC and Anchor company not found for this lead.";
                }

            }
            else
            {
                res.Status = false;
                res.Message = "Lead data not found .";
            }
            return res;
        }


        [Route("PostDisbursement")]
        [HttpPost]
        public async Task<GRPCReply<long>> PostDisbursement(PostDisbursementDTO request)
        {
            var res = await _loanAccountManager.PostDisbursement(request);
            return res;
        }

        [HttpGet]
        [Route("GetDisbursement")]
        public async Task<GRPCReply<DisbursementResponse>> GetDisbursement(long leadId)
        {
            GRPCRequest<long> leadid = new GRPCRequest<long>() { Request = leadId };
            return await _loanAccountManager.GetDisbursement(leadid);
        }

        [HttpGet]
        [Route("SendOtpOnEmail")]
        [AllowAnonymous]

        public async Task<GenerateOTPResponse> SendOtpOnEmail(string email)
        {
            return await leadMobileValidateManager.GenerateOtpForEmail(email);
        }

        [HttpPost]
        [Route("OTPValidateForEmail")]
        [AllowAnonymous]
        public async Task<LeadEmailResponse> OTPValidateForEmail(LeadEmailValidate leadEmailValidate)
        {
            return await leadMobileValidateManager.OTPValidateForEmail(leadEmailValidate);
        }
        [HttpPost]
        [Route("AddUpdateSelfConfiguration")]
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> AddUpdateSelfConfiguration(List<DefaultOfferSelfConfigurationDTO> selfConfigList)
        {
            return await leadMobileValidateManager.AddUpdateSelfConfiguration(selfConfigList);
        }

        [HttpGet]
        [Route("GetOfferList")]
        public async Task<GRPCReply<List<OfferListReply>>> GetOfferList(long LeadId)
        {
            return await leadMobileValidateManager.GetOfferList(LeadId);
        }


        [HttpGet]
        [Route("GetLeadOffer")]
        public async Task<GRPCReply<LoanAccount_DisplayDisbursalAggDTO>> GetLeadOffer(long leadId, long companyId)
        {
            double CreditLimit = 0;
            string LeadCode = "";
            GRPCReply<LoanAccount_DisplayDisbursalAggDTO> res = new GRPCReply<LoanAccount_DisplayDisbursalAggDTO>();

            GRPCRequest<long> leadid = new GRPCRequest<long>() { Request = leadId };
            var response = await leadListDetailsManager.GetCreditLimitByLeadId(leadid);

            if (response != null)
            {
                CreditLimit = response.Response.CreditLimit ?? 0;
                LeadCode = response.Response.LeadCode;
                if (companyId > 0 && response.Response.OfferCompanyId.HasValue)
                {
                    res = await leadMobileValidateManager.GetLeadOffer(leadId, CreditLimit, LeadCode, response.Response.OfferCompanyId.Value, companyId);

                }
                else
                {
                    res.Status = false;
                    res.Message = "NBFC and Anchor company not found for this lead.";
                }

            }
            else
            {
                res.Status = false;
                res.Message = "Lead data not found .";
            }
            return res;
        }

        [HttpPost]
        [Route("VerifyLeadDocument")]
        public async Task<GRPCReply<VerifyLeadDocumentReply>> VerifyLeadDocument(VerifyLeadDocumentRequest request)
        {
            return await leadListDetailsManager.VerifyLeadDocument(request);
        }

        [HttpPost]
        [Route("GetCompanySummary")]
        [AllowAnonymous]
        public async Task<GRPCReply<List<CompanySummaryReply>>> GetCompanySummary(CompanySummaryRequest request)
        {
            GRPCRequest<CompanySummaryRequest> summaryReq = new GRPCRequest<CompanySummaryRequest>();
            summaryReq.Request = request;
            var result = await companymanager.GetCompanySummary(summaryReq);
            if (result != null && result.Response.Any())
            {
                GRPCRequest<List<long>> req = new GRPCRequest<List<long>> { Request = result.Response.Select(x => x.CompanyId).ToList() };
                var AnchorCompanyList = await _productService.GetAnchorComapnyData(req);
                if (AnchorCompanyList != null)
                {
                    foreach (var item in AnchorCompanyList.Response)
                    {
                        var summary = result.Response.Where(x => x.CompanyId == item.CompanyId).FirstOrDefault();
                        if (summary != null)
                        {
                            summary.CompanyId = item.AnchorCompanyId;
                        }
                    }
                }
            }
            return result;
        }
        //Accept offer from backend
        [HttpPost]
        [Route("UpdateLeadOffer")]
        public async Task<GRPCReply<UpdateLeadOfferResponse>> UpdateLeadOffer(UpdateLeadOfferPostDc leadoffer)
        {
            return await leadMobileValidateManager.UpdateLeadOffer(leadoffer);
            //offer id and product ID
        }

        [Route("LoanAccountCompLead")]
        [HttpGet]
        public async Task<GRPCReply<long>> LoanAccountCompLead(long LeadId, long CompanyID)
        {
            var res = await _loanAccountManager.LoanAccountCompLead(LeadId, CompanyID);
            return res;
        }

        [Route("LeadActivityHistory")]
        [HttpGet]
        public async Task<List<LeadActivityHistoryDc>> LeadActivityHistory(long LeadId)
        {
            var res = await companymanager.LeadActivityHistory(LeadId);
            return res;
        }

        [Route("ResetLeadActivityMasterProgresse")]
        [HttpGet]
        public async Task<bool> ResetLeadActivityMasterProgresse(long LeadId)
        {
            var LeadActivityMaster = await _leadService.ResetLeadActivityMasterProgresse(new GRPCRequest<long> { Request = LeadId });
            if (LeadActivityMaster != null && !string.IsNullOrEmpty(LeadActivityMaster.Response))
            {
                await _kycService.RemoveKYCPersonalInfo(new GRPCRequest<string> { Request = LeadActivityMaster.Response });
            }
            return true;
        }

        [Route("ResetLead")]
        [HttpGet]
        public async Task<bool> ResetLead(long LeadId, string ProductCode)
        {
            var LeadActivityMaster = await _leadService.ResetLeadActivityMasterProgresse(new GRPCRequest<long> { Request = LeadId });
            if (LeadActivityMaster != null && !string.IsNullOrEmpty(LeadActivityMaster.Response))
            {
                //await _kycService.RemoveKYCInfoOnReset(new GRPCRequest<ResetLeadRequestDc> { Request = {
                //    userid = LeadActivityMaster.Response,
                //    ProductCode = ProductCode
                //    });
                GRPCRequest<ResetLeadRequestDc> resetLeadRequest = new GRPCRequest<ResetLeadRequestDc>();
                resetLeadRequest.Request = new ResetLeadRequestDc
                {
                    userid = LeadActivityMaster.Response,
                    ProductCode = ProductCode
                };
                await _kycService.RemoveKYCInfoOnReset(resetLeadRequest);
            }
            return true;
        }

        [Route("ArthmateGenerateAgreement")]
        [HttpGet]

        public async Task<GRPCReply<AgreementResponseDc>> ArthmateGenerateAgreement(long leadId, bool IsSubmit)
        {
            var reply = await _ArthMateManager.ArthmateGenerateAgreement(leadId, IsSubmit);
            return reply;
        }

        [HttpPost]
        [Route("GetOfferEmiDetailsDownloadPdf")]
        public async Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(EmiDetailReqDc req)
        {
            GRPCRequest<EmiDetailReqDc> request = new GRPCRequest<EmiDetailReqDc> { Request = req };
            var reply = await _ArthMateManager.GetOfferEmiDetailsDownloadPdf(request);
            if (reply.Response != null && reply.Status)
            {
                GRPCRequest<GRPCHtmlConvertRequest> convertRequest = new GRPCRequest<GRPCHtmlConvertRequest> { Request = new GRPCHtmlConvertRequest { HtmlContent = reply.Response } };
                var mediaResponse = await MediaManager.HtmlToPdf(convertRequest);
                if (mediaResponse != null && mediaResponse.Response != null && !string.IsNullOrEmpty(mediaResponse.Response.PdfUrl))
                {
                    GRPCRequest<ArthMateLeadAgreementDc> agrementreq = new GRPCRequest<ArthMateLeadAgreementDc>();
                    ArthMateLeadAgreementDc obj = new ArthMateLeadAgreementDc();
                    obj.LeadId = req.LeadId;
                    obj.AgreementPdfUrl = mediaResponse.Response.PdfUrl;

                    agrementreq.Request = obj;
                    var res = await _leadService.SaveOfferEmiDetailsPdf(agrementreq);
                    reply.Response = mediaResponse.Response.PdfUrl;
                    reply.Status = true;
                    reply.Message = "success";
                }
            }
            return reply;
        }

        [HttpPost]
        [Route("GetGenerateOfferStatusNew")]
        [AllowAnonymous]
        public async Task<List<LeadCompanyGenerateOfferNewDTO>> GetGenerateOfferStatusNew(GenerateOfferStatusPostDc obj)
        {
            obj.UserType = UserType;
            var data = await leadMobileValidateManager.GetLeadActivityOfferStatusNew(obj);
            return data;
        }

        [Route("ScaleupDashboardDetails")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<scaleupDashboardResponseDc> ScaleupDashboardDetails(leadDashboardDetailDc req)
        {
            scaleupDashboardResponseDc reply = new scaleupDashboardResponseDc();
            var LeadDetails = await _leadService.LeadDashboardDetails(req);
            if (LeadDetails.Status)
            {
                reply.leadResponse = new leadDashboardResponseDc
                {
                    Approved = LeadDetails.Response.Approved,
                    Pending = LeadDetails.Response.Pending,
                    Rejected = LeadDetails.Response.Rejected,
                    ApprovalPercentage = LeadDetails.Response.ApprovalPercentage,
                    NotContactable = LeadDetails.Response.NotContactable,
                    NotIntrested = LeadDetails.Response.NotIntrested,
                    TotalLeads = LeadDetails.Response.TotalLeads,
                    WholeDays = LeadDetails.Response.WholeDays,
                    RemainingHours = LeadDetails.Response.RemainingHours,
                    AccountRowDataDC = new List<AccountResponseDc>()
                };
            }

            var LoanAccDetails = await _loanAccountManager.ScaleupLoanAccountDashboardDetails(new DashboardLoanAccountDetailDc
            {
                AnchorId = req.AnchorId,
                CityId = req.CityId,
                CityName = req.CityName,
                FromDate = req.FromDate,
                ProductId = req.ProductId,
                ProductType = req.ProductType,
                ToDate = req.ToDate
            });

            if (LeadDetails.Response != null && LeadDetails.Response.AccountRowDataDC != null && LeadDetails.Response.AccountRowDataDC.Any())
            {
                var accountData = LeadDetails.Response.AccountRowDataDC;
                if (LoanAccDetails.loanAccountData != null && LoanAccDetails.loanAccountData.Any())
                {
                    var loanAccountLeadIds = LoanAccDetails.loanAccountData.Select(x => x.LeadId).ToList();
                    accountData = LeadDetails.Response.AccountRowDataDC.Where(x => !loanAccountLeadIds.Contains(x.LeadId)).ToList();
                }

                if (accountData != null && accountData.Any())
                {
                    reply.AccResponseDc = new AccountDashboardResponseDc
                    {
                        ApprovalPending = accountData.Where(x => x.Status.ToLower() == LeadStatusEnum.Pending.ToString().ToLower()).Count(),
                        CreditApproved = accountData.Where(x => (x.Status.Equals(LeadBusinessLoanStatusConstants.LoanActivated) || x.Status.Equals(LeadBusinessLoanStatusConstants.LoanApproved) || x.Status.Equals(LeadBusinessLoanStatusConstants.LoanInitiated))
                         || (x.Status.Equals(LeadStatusEnum.LineInitiated.ToString()) || x.Status.Equals(LeadStatusEnum.LineActivated.ToString()) || x.Status.Equals(LeadStatusEnum.LineApproved.ToString()))).Count(),
                        CreditRejected = accountData.Where(x => x.Status.Equals(LeadBusinessLoanStatusConstants.LoanRejected) || x.Status.Equals(LeadStatusEnum.LineRejected.ToString())).Count(),
                        OfferRejected = 0,
                        Rejected = 0,
                        TotalAccounts = accountData.Count(),
                        CreditApprovalPercentage = 0
                    };
                    reply.AccResponseDc.CreditApprovalPercentage = (reply.AccResponseDc.CreditApproved * 100) / reply.AccResponseDc.TotalAccounts;
                }
            }
            else
            {
                reply.AccResponseDc = new AccountDashboardResponseDc
                {
                    ApprovalPending = 0,
                    CreditApproved = 0,
                    CreditRejected = 0,
                    OfferRejected = 0,
                    Rejected = 0,
                    TotalAccounts = 0,
                    CreditApprovalPercentage = 0
                };
            }

            if (req.ProductType == ProductTypeConstants.CreditLine && LeadDetails.Response != null && LeadDetails.Response.AccountRowDataDC != null)
            {
                var TotalCreditLineLoan = LeadDetails.Response.AccountRowDataDC.Where(x => x.Status == LeadStatusEnum.LineActivated.ToString()).ToList().Count();
                var leadIds = LeadDetails.Response.AccountRowDataDC.Where(x => x.Status == LeadStatusEnum.LineActivated.ToString()).Select(x => x.LeadId).ToList();
                var CreditLineRejected = 0;
                var CreditLineApproved = 0;
                if (LoanAccDetails != null && LoanAccDetails.loanAccountData != null && LoanAccDetails.loanAccountData.Any())
                {
                    CreditLineRejected = LoanAccDetails.loanAccountData.Where(x => leadIds.Contains(x.LeadId) && x.IsBlock == true).ToList().Count();
                    CreditLineApproved = LoanAccDetails.loanAccountData.Where(x => leadIds.Contains(x.LeadId)).ToList().Count();
                }
                reply.LoanResponseDc = new LoanDashboardResponseDc
                {
                    CreditLinePending = TotalCreditLineLoan - (CreditLineApproved + CreditLineRejected),
                    CreditLineApproved = CreditLineApproved,
                    CreditLineRejected = CreditLineRejected,
                    CreditLineTotalLoan = TotalCreditLineLoan,
                    CreditLineApprovalPercentage = 0,
                    CreditLineOfferRejected = 0,
                    CLRejected = 0,
                };
                reply.LoanResponseDc.CreditLineApprovalPercentage = reply.LoanResponseDc.CreditLineTotalLoan > 0 ? ((reply.LoanResponseDc.CreditLineApproved * 100) / reply.LoanResponseDc.CreditLineTotalLoan) : 0;
            }
            else if (req.ProductType == ProductTypeConstants.BusinessLoan && LeadDetails.Response != null)
            {
                reply.LoanResponseDc = new LoanDashboardResponseDc
                {
                    DisbursementPending = LeadDetails.Response.totalDisbursementByUMRN - (LeadDetails.Response.DisbursementApprove + LeadDetails.Response.Rejected),
                    DisbursementApproved = LeadDetails.Response.DisbursementApprove,
                    DisbursementRejected = LeadDetails.Response.Rejected,
                    TotalLoan = LeadDetails.Response.totalDisbursementByUMRN,
                    DisbursementApprovalPercentage = 0,
                    OfferRejected = 0,
                    Rejected = 0
                };
                reply.LoanResponseDc.DisbursementApprovalPercentage = reply.LoanResponseDc.TotalLoan > 0 ? ((reply.LoanResponseDc.DisbursementApproved * 100) / reply.LoanResponseDc.TotalLoan) : 0;
            }
            else
            {
                reply.LoanResponseDc = new LoanDashboardResponseDc
                {
                    CLRejected = 0,
                    CreditLineApprovalPercentage = 0,
                    CreditLineApproved = 0,
                    CreditLineOfferRejected = 0,
                    CreditLineRejected = 0,
                    CreditLinePending = 0,
                    CreditLineTotalLoan = 0,
                    DisbursementApproved = 0,
                    DisbursementApprovalPercentage = 0,
                    DisbursementPending = 0,
                    DisbursementRejected = 0,
                    OfferRejected = 0,
                    Rejected = 0,
                    TotalLoan = 0
                };
            }

            reply.Staus = LoanAccDetails.Status;
            reply.Message = LoanAccDetails.Message;
            LoanAccDetails.loanAccountData = new List<loanAccountDc>();
            reply.DashboardResponse = LoanAccDetails;
            return reply;
        }

        [Route("GetLeadCityList")]
        [HttpGet]
        //[AllowAnonymous]
        public async Task<GRPCReply<List<CityReply>>> GetLeadCityList()
        {
            GRPCReply<List<CityReply>> reply = new GRPCReply<List<CityReply>>();
            var LeadCitylist = await _leadService.GetLeadCityList();
            if (LeadCitylist != null && LeadCitylist.Status)
            {
                GRPCRequest<List<long>> gRPCRequest = new GRPCRequest<List<long>>();
                gRPCRequest.Request = LeadCitylist.Response;
                var cities = await _locationService.GetCityListByIds(gRPCRequest);
                if (cities.Status)
                {
                    reply.Response = cities.Response;
                    reply.Status = cities.Status;
                    reply.Message = cities.Message;
                }
            }
            return reply;
        }


        [Route("LeadExport")]
        [HttpPost]
        //[AllowAnonymous]
        public async Task<GRPCReply<List<leadExportRes>>> LeadExport(leadDashboardDetailDc req)
        {
            GRPCReply<List<leadExportRes>> reply = new GRPCReply<List<leadExportRes>>();
            var citylist = await GetLeadCityList();
            var leads = await _leadService.LeadExport(req);
            if (leads != null && leads.Response != null)
            {
                var query = from l in leads.Response
                            join c in citylist.Response on l.CityId equals c.cityId
                            select new leadExportRes
                            {
                                Activity = l.Activity,
                                AnchorName = l.AnchorName,
                                ApplicantName = l.ApplicantName,
                                CityId = l.CityId,
                                Created = l.Created,
                                CreditLimit = l.CreditLimit,
                                DisbursedDate = l.DisbursedDate,
                                LeadCode = l.LeadCode,
                                LeadID = l.LeadID,
                                MobileNo = l.MobileNo,
                                Status = l.Status,
                                SubActivityMasterName = l.SubActivityMasterName,
                                UserUniqueCode = l.UserUniqueCode,
                                cityName = c.cityName
                            };
                reply.Status = true;
                reply.Message = "Data found";
                reply.Response = query.ToList();
            }
            else
            {
                reply.Status = true;
                reply.Message = "Data not found";
            }
            return reply;
        }

        [HttpGet]
        [Route("SendStampRemainderEmailJob")]
        [AllowAnonymous]
        public async Task<bool> SendStampRemainderEmailJob()
        {
            return await _ArthMateManager.SendStampRemainderEmailJob();
        }

        [HttpPost]
        [Route("GetDisbursementAPI")]
        [AllowAnonymous]
        //For Business loan
        public async Task<GRPCReply<bool>> GetDisbursementAPI(long LeadId)
        {
            var res = await _ArthMateManager.GetDisbursementAPI(LeadId);
            return res;
        }

        [HttpGet]
        [Route("GetStatusList")]
        [AllowAnonymous]
        public async Task<List<KeyValuePair<string, string>>> GetStatusList(string ProductType)
        {
            List<KeyValuePair<string, string>> status = new List<KeyValuePair<string, string>>();
            if (ProductType == ProductTypeConstants.BusinessLoan)
            {
                if (UserType.ToLower() == UserTypeConstants.AdminUser.ToLower() || UserType.ToLower() == UserTypeConstants.SuperAdmin.ToLower())
                {
                    status.Add(new KeyValuePair<String, String>("All", "All"));
                    status.Add(new KeyValuePair<String, String>("Pending", LeadBusinessLoanStatusConstants.Pending)); //"pending"
                    status.Add(new KeyValuePair<String, String>("Loan Approved", LeadBusinessLoanStatusConstants.LoanApproved));
                    status.Add(new KeyValuePair<String, String>("Loan Initiated", LeadBusinessLoanStatusConstants.LoanInitiated));
                    status.Add(new KeyValuePair<String, String>("Loan Activated", LeadBusinessLoanStatusConstants.LoanActivated));
                    status.Add(new KeyValuePair<String, String>("Loan Availed", LeadBusinessLoanStatusConstants.LoanAvailed));
                    status.Add(new KeyValuePair<String, String>("Loan Rejected", LeadBusinessLoanStatusConstants.LoanRejected));
                }
                else
                {
                    status.Add(new KeyValuePair<String, String>("All", "All"));
                }
            }
            if (ProductType == ProductTypeConstants.CreditLine)
            {
                status.Add(new KeyValuePair<String, String>("All", "All"));
                status.Add(new KeyValuePair<String, String>("Pending", LeadStatusEnum.Pending.ToString())); //"pending"
                status.Add(new KeyValuePair<String, String>("line Approved", LeadStatusEnum.LineApproved.ToString())); //"lineapproved"
                status.Add(new KeyValuePair<String, String>("line Initiated", LeadStatusEnum.LineInitiated.ToString())); //"lineinitiated"
                status.Add(new KeyValuePair<String, String>("line Availed", LeadStatusEnum.LoanAvailed.ToString())); //"LoanAvailed"
                status.Add(new KeyValuePair<String, String>("line Activated", LeadStatusEnum.LineActivated.ToString())); //"lineactivated"
                status.Add(new KeyValuePair<String, String>("line Rejected", LeadStatusEnum.LineRejected.ToString())); //"linerejected"
            }
            if (ProductType == ProductTypeConstants.DSA)
            {
                status.Add(new KeyValuePair<String, String>("All", "All"));
                status.Add(new KeyValuePair<String, String>("Initiate", LeadStatusEnum.Initiate.ToString()));
                status.Add(new KeyValuePair<String, String>("KYCInProcess", LeadStatusEnum.KYCInProcess.ToString()));
                status.Add(new KeyValuePair<String, String>("KYCSuccess", LeadStatusEnum.KYCSuccess.ToString()));
                status.Add(new KeyValuePair<String, String>("Submitted", LeadStatusEnum.Submitted.ToString()));
                status.Add(new KeyValuePair<String, String>("Agreement In-Progress", LeadStatusEnum.AgreementInProgress.ToString()));
                status.Add(new KeyValuePair<String, String>("Agreement Signed", LeadStatusEnum.AgreementSigned.ToString()));
                status.Add(new KeyValuePair<String, String>("Activated", LeadStatusEnum.Activated.ToString()));
                status.Add(new KeyValuePair<String, String>("DeActivated", LeadStatusEnum.DeActivated.ToString()));
                status.Add(new KeyValuePair<String, String>("Deleted", LeadStatusEnum.Deleted.ToString()));
                status.Add(new KeyValuePair<String, String>("Rejected", LeadStatusEnum.Rejected.ToString()));
            }
            return status;
        }


        [HttpPost]
        [Route("UpdateAddress")]
        [AllowAnonymous]
        public async Task<GRPCReply<long>> UpdateAddress(UpdateAddressRequest updateAddressRequest)
        {
            GRPCReply<long> reply = new GRPCReply<long>();
            var leadrequest = new GRPCRequest<long>();
            leadrequest.Request = updateAddressRequest.LeadId;
            var leadInfo = await _leadService.GetLeadInfoById(leadrequest);
            if (leadInfo != null)
            {
                reply = await _locationService.AddAddress(new UpdateCompanyAddressRequest
                {
                    AddressId = updateAddressRequest.AddressId,
                    AddressLineOne = updateAddressRequest.AddCorrLine1,
                    AddressLineThree = updateAddressRequest.AddCorrLine3,
                    AddressLineTwo = updateAddressRequest.AddCorrLine2,
                    CityId = int.Parse(updateAddressRequest.AddCorrCity),
                    ZipCode = int.Parse(updateAddressRequest.AddCorrPincode),
                    MasterName = (updateAddressRequest.AddressType == "PersonalDetail") ? "PersonalDetail" : ""
                });
                if (reply.Status && reply.Response > 0)
                {
                    updateAddressRequest.CurrentAddressId = reply.Response;
                    updateAddressRequest.ProductCode = leadInfo.Response.ProductCode;
                    await _kycService.UpdateAddress(new GRPCRequest<UpdateAddressRequest> { Request = updateAddressRequest });

                    var UpdateLeadAdd = await _leadService.AddUpdatePersonalBussDetail(new GRPCRequest<UpdateAddressRequest> { Request = updateAddressRequest });
                }
                //if (AddressReply.Status)
                //{
                //   reply.Response = AddressReply.Response;
                //    reply.Status = AddressReply.Status;
                //    reply.Message = "Updated Successfully!";
                //}
                //else
                //{
                //    reply.Status = AddressReply.Status;
                //    reply.Message = AddressReply.Message;
                //}
            }
            return reply;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetSCAccountList")]
        public async Task<GRPCReply<List<SCAccountResponseDc>>> GetSCAccountList(SCAccountRequestDC request)
        {
            var res = await leadListDetailsManager.GetSCAccountList(request);
            return res;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetBLAccountList")]
        public async Task<GRPCReply<List<BLAccountResponseDC>>> GetBLAccountList(BLAccountRequestDc request)
        {
            request.UserType = UserType;
            var res = await leadListDetailsManager.GetBLAccountList(request);
            return res;
        }

        [HttpPost]
        [Route("UpdateBuisnessDetail")]
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> UpdateBuisnessDetail(UpdateBuisnessDetailRequest updateBuisnessDetailRequest)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var leadrequest = new GRPCRequest<long>();
            leadrequest.Request = updateBuisnessDetailRequest.LeadMasterId; ;
            var leadInfo = await _leadService.GetLeadInfoById(leadrequest);
            if (leadInfo != null)
            {
                if (updateBuisnessDetailRequest.AddressId != updateBuisnessDetailRequest.CurrentAddressId)
                {
                    var addressResponse = await _locationService.AddAddress(new UpdateCompanyAddressRequest
                    {
                        AddressId = updateBuisnessDetailRequest.AddressId,
                        AddressLineOne = updateBuisnessDetailRequest.BusAddCorrLine1,
                        AddressLineThree = updateBuisnessDetailRequest.BusAddCorrLine2,
                        AddressLineTwo = updateBuisnessDetailRequest.BusAddCorrLine2,
                        CityId = int.Parse(updateBuisnessDetailRequest.BusAddCorrCity),
                        ZipCode = int.Parse(updateBuisnessDetailRequest.BusAddCorrPincode)
                    });
                    if (addressResponse.Status && addressResponse.Response > 0)
                    {
                        updateBuisnessDetailRequest.CurrentAddressId = addressResponse.Response;
                    }
                }
                if (updateBuisnessDetailRequest.CurrentAddressId > 0)
                {
                    updateBuisnessDetailRequest.ProductCode = leadInfo.Response.ProductCode;
                    reply = await _kycService.UpdateBuisnessDetail(new GRPCRequest<UpdateBuisnessDetailRequest> { Request = updateBuisnessDetailRequest });

                    var UpdateLeadBuisnessDetail = await _leadService.AddUpdateBuisnessDetail(new GRPCRequest<UpdateBuisnessDetailRequest> { Request = updateBuisnessDetailRequest });
                }
            }
            return reply;
        }


        [HttpPost]
        [Route("UploadLeadDocuments")]
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> UploadLeadDocuments(UpdateLeadDocumentDetailRequest updateLeadDocumentDetailRequest)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var leadrequest = new GRPCRequest<long>();
            leadrequest.Request = updateLeadDocumentDetailRequest.LeadId;
            var leadInfo = await _leadService.GetLeadInfoById(leadrequest);
            if (leadInfo != null)
            {
                updateLeadDocumentDetailRequest.ProductCode = leadInfo.Response.ProductCode;
                var leadDoc = await _leadService.UploadLeadDocuments(new GRPCRequest<UpdateLeadDocumentDetailRequest> { Request = updateLeadDocumentDetailRequest });
                reply = await _kycService.UploadLeadDocuments(new GRPCRequest<UpdateLeadDocumentDetailRequest> { Request = updateLeadDocumentDetailRequest });
            }
            return reply;
        }

        [HttpPost]
        [Route("UploadMultiLeadDocuments")]
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> UploadMultiLeadDocuments(UpdateLeadDocumentDetailListRequest updateLeadDocumentDetailRequest)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var leadInfo = await _leadService.GetLeadInfoById(new GRPCRequest<long> { Request = updateLeadDocumentDetailRequest.LeadId });
            if (leadInfo != null)
            {
                updateLeadDocumentDetailRequest.ProductCode = leadInfo.Response.ProductCode;
                var leadDoc = await _leadService.UploadMultiLeadDocuments(new GRPCRequest<UpdateLeadDocumentDetailListRequest> { Request = updateLeadDocumentDetailRequest });
                reply = await _kycService.UploadMultiLeadDocuments(new GRPCRequest<UpdateLeadDocumentDetailListRequest> { Request = updateLeadDocumentDetailRequest });
            }
            return reply;

        }

        [HttpPost]
        [Route("GetLeadDocumentsByLeadId")]
        [AllowAnonymous]
        public async Task<GRPCReply<List<LeadDocumentDetailReply>>> GetLeadDocumentsByLeadId(long LeadId)
        {
            GRPCReply<List<LeadDocumentDetailReply>> reply = new GRPCReply<List<LeadDocumentDetailReply>>();
            var leadrequest = new GRPCRequest<long>();
            leadrequest.Request = LeadId;
            var leadInfo = await _leadService.GetLeadInfoById(leadrequest);
            //var leadActivityOfferStatus = await _leadService.GetLeadActivityOfferStatus(leadrequest);
            var leadDocs = await _leadService.GetLeadDocumentsByLeadId(leadrequest);
            if (leadInfo != null)
            {
                //if (!leadActivityOfferStatus.Status || !leadDocs.Status)//LoanApproved
                //{
                List<LeadDocumentDetailReply> leadDocumentList = new List<LeadDocumentDetailReply>();
                List<string> MasterCodeList = new List<string>
                    {
                        KYCMasterConstants.BuisnessDetail,
                        KYCMasterConstants.PersonalDetail,
                        KYCMasterConstants.MSME,
                        KYCMasterConstants.DSAProfileType,
                        KYCMasterConstants.DSAPersonalDetail
                    };
                var leaddetail = await kYCUserDetailManager.GetLeadDetailAll(leadInfo.Response.UserName, leadInfo.Response.ProductCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
                if (leaddetail != null && leadInfo.Response.ProductCode == "DSA")
                {
                    if (leaddetail.DSAPersonalDetail != null && !string.IsNullOrEmpty(leaddetail.DSAPersonalDetail.BuisnessDocImg))
                    {
                        List<string> url = new List<string>();
                        url.Add(leaddetail.DSAPersonalDetail.BuisnessDocImg);
                        var leadDoc = new LeadDocumentDetailReply
                        {
                            DocumentName = leaddetail.DSAPersonalDetail.BuisnessDocument,
                            DocumentNumber = "",
                            FileUrl = url,
                            LeadDocDetailId = 0,
                            LeadId = LeadId,
                            PdfPassword = "",
                            Sequence = 0,
                            Label = leaddetail.DSAPersonalDetail.BuisnessDocument
                        };
                        leadDocumentList.Add(leadDoc);
                    }
                }
                if (leaddetail != null)
                {
                    if (leaddetail.PersonalDetail != null && !string.IsNullOrEmpty(leaddetail.PersonalDetail.ManualElectricityBillImage))
                    {
                        List<string> url = new List<string>();
                        url.Add(leaddetail.PersonalDetail.ManualElectricityBillImage);
                        var leadDoc = new LeadDocumentDetailReply
                        {
                            DocumentName = "Address Proof",
                            DocumentNumber = "",
                            FileUrl = url,
                            LeadDocDetailId = 0,
                            LeadId = LeadId,
                            PdfPassword = "",
                            Sequence = 0,
                            Label = "Address Proof"
                        };
                        leadDocumentList.Add(leadDoc);
                    }
                    if (leaddetail.BuisnessDetail != null && !string.IsNullOrEmpty(leaddetail.BuisnessDetail.BuisnessProofUrl))
                    {
                        List<string> url = new List<string>();
                        url.Add(leaddetail.BuisnessDetail.BuisnessProofUrl);
                        var leadDoc = new LeadDocumentDetailReply
                        {
                            DocumentName = leaddetail.BuisnessDetail.BuisnessProof == "GST Certificate" ? DTOs.BlackSoilBusinessDocNameConstants.GstCertificate : DTOs.BlackSoilBusinessDocNameConstants.Other,
                            DocumentNumber = leaddetail.BuisnessDetail.BuisnessDocumentNo,
                            FileUrl = url,
                            LeadDocDetailId = 0,
                            LeadId = LeadId,
                            PdfPassword = "",
                            Sequence = 0,
                            Label = leaddetail.BuisnessDetail.BuisnessProof == "GST Certificate" ? "GST Certificate" : "Others"
                        };
                        leadDocumentList.Add(leadDoc);
                    }
                    if (leaddetail.BuisnessDetail != null && leaddetail.BuisnessDetail.BuisnessPhotoUrl != null && leaddetail.BuisnessDetail.BuisnessPhotoUrl.Count > 0)
                    {
                        List<string> url = new List<string>();
                        url = leaddetail.BuisnessDetail.BuisnessPhotoUrl.Select(x => x).ToList();
                        var leadDoc = new LeadDocumentDetailReply
                        {
                            DocumentName = DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos,
                            DocumentNumber = "",
                            FileUrl = url,
                            LeadDocDetailId = 0,
                            LeadId = LeadId,
                            PdfPassword = "",
                            Sequence = 0,
                            Label = DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos
                        };
                        leadDocumentList.Add(leadDoc);
                    }
                    if (leaddetail.MSMEDetail != null && !string.IsNullOrEmpty(leaddetail.MSMEDetail.MSMECertificateUrl))
                    {
                        List<string> url = new List<string>();
                        url.Add(leaddetail.MSMEDetail.MSMECertificateUrl);
                        var leadDoc = new LeadDocumentDetailReply
                        {
                            DocumentName = DTOs.BlackSoilBusinessDocNameConstants.UdyogAadhaar,
                            DocumentNumber = leaddetail.MSMEDetail.MSMERegNum,
                            FileUrl = url,
                            LeadDocDetailId = 0,
                            LeadId = LeadId,
                            PdfPassword = "",
                            Sequence = 0,
                            Label = "Udyog Aadhaar"
                        };
                        leadDocumentList.Add(leadDoc);
                    }
                    if (leadDocs.Status)
                    {
                        if (leadDocs.Response != null && leadDocs.Response.Any())
                        {
                            foreach (var item in leadDocs.Response)
                            {
                                //|| item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos
                                if (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.SurrogateGstCertificate || item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.SurrogateITRCertificate || item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Statement || item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos)
                                {
                                    //if(item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos && leadDocumentList.Any(doc => doc.DocumentName == item.DocumentName))
                                    if (leadInfo.Response.ProductCode == "BusinessLoan")
                                    {
                                        if (leadDocumentList.Any(doc => doc.DocumentName == item.DocumentName))
                                        {
                                            var existingDocument = leadDocumentList.FirstOrDefault(doc => doc.DocumentName == item.DocumentName);

                                            if (existingDocument != null)
                                            {
                                                existingDocument.FileUrl.Add(item.FileUrl.FirstOrDefault());
                                            }
                                        }
                                        else
                                        {
                                            var label = (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.SurrogateGstCertificate) ? "Surrogate Gst" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.SurrogateITRCertificate) ? "Surrogate ITR" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Statement) ? "Bank Statement" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Other) ? "Others" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.UdyogAadhaar) ? "Udyog Aadhaar" :
                                                        (item.DocumentName == "Address Proof") ? "Address Proof" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.GstCertificate) ? "GstCertificate" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos) ? "BusinessPhotos" : item.DocumentName;

                                            var leadDoc = new LeadDocumentDetailReply
                                            {
                                                DocumentName = item.DocumentName,
                                                DocumentNumber = item.DocumentNumber,
                                                FileUrl = item.FileUrl,
                                                LeadDocDetailId = item.LeadDocDetailId,
                                                LeadId = LeadId,
                                                PdfPassword = item.PdfPassword,
                                                Sequence = item.Sequence,
                                                Label = label
                                            };
                                            leadDocumentList.Add(leadDoc);
                                        }
                                    }
                                    else if (leadInfo.Response.ProductCode == "DSA")
                                    {
                                        if (leadDocumentList.Any(doc => doc.DocumentName == item.DocumentName))
                                        {
                                            var existingDocument = leadDocumentList.FirstOrDefault(doc => doc.DocumentName == item.DocumentName);

                                            if (existingDocument != null)
                                            {
                                                existingDocument.FileUrl.Add(item.FileUrl.FirstOrDefault());
                                            }
                                        }
                                        else
                                        {
                                            var label = (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Statement) ? "Bank Statement" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Other) ? "Others" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.UdyogAadhaar) ? "Udyog Aadhaar" :
                                                        (item.DocumentName == "Address Proof") ? "Address Proof" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.GstCertificate) ? "GstCertificate" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos) ? "BusinessPhotos" : item.DocumentName;
                                            var leadDoc = new LeadDocumentDetailReply
                                            {
                                                DocumentName = item.DocumentName,
                                                DocumentNumber = item.DocumentNumber,
                                                FileUrl = item.FileUrl,
                                                LeadDocDetailId = item.LeadDocDetailId,
                                                LeadId = LeadId,
                                                PdfPassword = item.PdfPassword,
                                                Sequence = item.Sequence,
                                                Label = label
                                            };
                                            leadDocumentList.Add(leadDoc);
                                        }
                                    }
                                    else if (leadInfo.Response.ProductCode == "CreditLine")
                                    {
                                        if (leadDocumentList.Any(doc => doc.DocumentName == item.DocumentName))
                                        {
                                            var existingDocument = leadDocumentList.FirstOrDefault(doc => doc.DocumentName == item.DocumentName);

                                            if (existingDocument != null)
                                            {
                                                existingDocument.FileUrl.Add(item.FileUrl.FirstOrDefault());
                                            }
                                        }
                                        else
                                        {
                                            var label = (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Statement) ? "Bank Statement" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Other) ? "Others" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.UdyogAadhaar) ? "Udyog Aadhaar" :
                                                        (item.DocumentName == "Address Proof") ? "Address Proof" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.GstCertificate) ? "GstCertificate" :
                                                        (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos) ? "BusinessPhotos" : item.DocumentName;
                                            var leadDoc = new LeadDocumentDetailReply
                                            {
                                                DocumentName = item.DocumentName,
                                                DocumentNumber = item.DocumentNumber,
                                                FileUrl = item.FileUrl,
                                                LeadDocDetailId = item.LeadDocDetailId,
                                                LeadId = LeadId,
                                                PdfPassword = item.PdfPassword,
                                                Sequence = item.Sequence,
                                                Label = label
                                            };
                                            leadDocumentList.Add(leadDoc);
                                        }
                                    }
                                }
                                else if (!leadDocumentList.Any(doc => doc.DocumentName == item.DocumentName))
                                {
                                    var label = (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.SurrogateGstCertificate) ? "Surrogate Gst" :
                                                (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.SurrogateITRCertificate) ? "Surrogate ITR" :
                                                (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Statement) ? "Bank Statement" :
                                                (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.Other) ? "Others" :
                                                (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.UdyogAadhaar) ? "Udyog Aadhaar" :
                                                (item.DocumentName == "Address Proof") ? "Address Proof" :
                                                (item.DocumentName == DTOs.BlackSoilBusinessDocNameConstants.GstCertificate) ? "GstCertificate" : item.DocumentName;
                                    var leadDoc = new LeadDocumentDetailReply
                                    {
                                        DocumentName = item.DocumentName,
                                        DocumentNumber = item.DocumentNumber,
                                        FileUrl = item.FileUrl,
                                        LeadDocDetailId = item.LeadDocDetailId,
                                        LeadId = LeadId,
                                        PdfPassword = item.PdfPassword,
                                        Sequence = item.Sequence,
                                        Label = label
                                    };
                                    leadDocumentList.Add(leadDoc);
                                }
                            }
                        }
                    }
                    List<string> docNameList = new List<string> { DTOs.BlackSoilBusinessDocNameConstants.GstCertificate
                                                                        , DTOs.BlackSoilBusinessDocNameConstants.UdyogAadhaar
                                                                        , DTOs.BlackSoilBusinessDocNameConstants.Other
                                                                        //, DTOs.BlackSoilBusinessDocNameConstants.Statement
                                                                        //, DTOs.BlackSoilBusinessDocNameConstants.SurrogateGstCertificate
                                                                        //, DTOs.BlackSoilBusinessDocNameConstants.SurrogateITRCertificate
                                                                        , DTOs.BlackSoilBusinessDocNameConstants.AadhaarFrontImage
                                                                        , DTOs.BlackSoilBusinessDocNameConstants.AadhaarBackImage
                                                                        , DTOs.BlackSoilBusinessDocNameConstants.PANImage
                                                                        , DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos};

                    var notExisting = docNameList.Where(x => !leadDocumentList.Any(y => y.DocumentName == x)).ToList();
                    if (notExisting != null && notExisting.Any())
                    {
                        foreach (var item in notExisting)
                        {
                            var label = (item == DTOs.BlackSoilBusinessDocNameConstants.SurrogateGstCertificate) ? "Surrogate Gst" :
                                        (item == DTOs.BlackSoilBusinessDocNameConstants.SurrogateITRCertificate) ? "Surrogate ITR" :
                                        (item == DTOs.BlackSoilBusinessDocNameConstants.Statement) ? "Bank Statement" :
                                        (item == DTOs.BlackSoilBusinessDocNameConstants.Other) ? "Others" :
                                        (item == DTOs.BlackSoilBusinessDocNameConstants.UdyogAadhaar) ? "Udyog Aadhaar" :
                                        (item == "Address Proof") ? "Address Proof" :
                                        (item == DTOs.BlackSoilBusinessDocNameConstants.GstCertificate) ? "GstCertificate" :
                                        (item == DTOs.BlackSoilBusinessDocNameConstants.BusinessPhotos) ? "BusinessPhotos" : item;
                            var leadDoc = new LeadDocumentDetailReply
                            {
                                DocumentName = item,
                                DocumentNumber = "",
                                FileUrl = new List<string> { },
                                LeadDocDetailId = 0,
                                LeadId = LeadId,
                                PdfPassword = "",
                                Sequence = 0,
                                Label = label
                            };
                            leadDocumentList.Add(leadDoc);
                        }
                    }

                }

                if (leadDocumentList.Count > 0)
                {
                    reply.Response = leadDocumentList;
                    reply.Status = true;
                }
                else
                {
                    reply.Status = false;
                    reply.Message = "Data not found!!";
                }
            }
            //}
            //else
            //{
            //    if (leadDocs.Status)
            //    {
            //        reply.Response = leadDocs.Response;
            //        if (leadDocs.Response.Count > 0)
            //        {
            //            reply.Status = true;
            //        }
            //    }
            //    else
            //    {
            //        reply.Status = false;
            //        reply.Message = "Data not found!!";
            //    }

            //}
            return reply;
        }

        [Route("AadhaarOtpVerify")]
        [HttpPost]
        public async Task<GRPCReply<string>> AadhaarOtpVerify(AadharOTPVerifyDc obj)
        {
            GRPCReply<string> response = new GRPCReply<string>();
            if (obj.request_id == null || obj.request_id == "")
            {
                response.Response = "";
                response.Message = "RequestId Cannot be null";
                response.Status = false;
                return response;
            }
            SecondAadharXMLDc aadhar = new SecondAadharXMLDc();
            aadhar.LeadMasterId = obj.LeadMasterId;
            aadhar.loan_amt = obj.loan_amt;
            aadhar.request_id = obj.request_id;
            aadhar.insurance_applied = obj.insurance_applied;
            aadhar.otp = obj.otp;
            GRPCRequest<SecondAadharXMLDc> request = new GRPCRequest<SecondAadharXMLDc> { Request = aadhar };

            response = await _ArthMateManager.AadhaarOtpVerify(request);
            return response;
        }

        [HttpGet]
        [Route("GetLeadDataById")]
        public async Task<GRPCReply<DSALeadListPageDTO>> GetLeadDataById(long LeadId)
        {
            return await leadListDetailsManager.GetDSALeadDataById(LeadId);
        }

        [HttpGet]
        [Route("GenerateLeadOfferByFinance")]
        public async Task<GRPCReply<bool>> GenerateLeadOfferByFinance(long LeadId, double CreditLimit)
        {
            return await leadMobileValidateManager.GenerateLeadOfferByFinance(LeadId, CreditLimit);
        }

        #region MAS finance Agreement

        [HttpPost]
        [Route("UploadMASfinanceAgreement")]
        public async Task<GRPCReply<string>> UploadMASfinanceAgreement(MASFinanceAgreementDc financeAgreementDc)
        {
            //string role = "";
            //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.AYEOperationExecutive.ToLower())))
            //    role = UserRoleConstants.AYEOperationExecutive;
            //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.MASOperationExecutive.ToLower())))
            //    role = UserRoleConstants.MASOperationExecutive;

            return await leadListDetailsManager.UploadMASfinanceAgreement(financeAgreementDc);
        }

        [HttpPost]
        [Route("GenerateOfferByFinance")]
        public async Task<GRPCReply<Loandetaildc>> GenerateOfferByFinance(GenerateOfferByFinanceRequestDc generateOfferByFinanceRequestDc)
        {
            return await leadMobileValidateManager.GenerateOfferByFinance(generateOfferByFinanceRequestDc);
        }
        [HttpPost]
        [Route("GetGenerateOfferByFinance")]
        public async Task<GRPCReply<Loandetaildc>> GetGenerateOfferByFinance(GetGenerateOfferByFinanceRequestDc getGenerateOfferByFinanceRequestDc)
        {
            return await leadMobileValidateManager.GetGenerateOfferByFinance(getGenerateOfferByFinanceRequestDc);
        }
        #endregion
        #region GetAllleadCities

        [HttpGet]
        [Route("GetAllLeadCities")]
        public async Task<GRPCReply<List<CityMasterListReply>>> GetAllLeadCities()
        {
            return await leadListDetailsManager.GetAllLeadCities();
        }

        #endregion

        [HttpPost]
        [Route("UpdateBuyingHistory")]
        public async Task<GRPCReply<bool>> UpdateBuyingHistory(UpdateBuyingHistoryRequest request)
        {
            var result = await leadMobileValidateManager.UpdateBuyingHistory(request);
            return result;
        }

        [HttpPost]
        [Route("NBFCDisbursement")]
        public async Task<GRPCReply<string>> NBFCDisbursement(NBFCDisbursementPostdc obj)
        {
            GRPCReply<string> res = new GRPCReply<string>();
            //GRPCRequest<long> request = new GRPCRequest<long> { Request = leadid };
            var result = await _ArthMateManager.GetLeadDetailForDisbursement(obj);
            res.Response = "";
            res.Status = true;
            return res;
        }

        [HttpPost]
        [Route("AcceptOfferByLead")] // for nbfc
        public async Task<GRPCReply<string>> GenerateKarzaAadhaarOtpForNBFC(AcceptOfferByLeadDc offer)
        {
            GRPCRequest<AcceptOfferByLeadDc> request = new GRPCRequest<AcceptOfferByLeadDc> { Request = offer };
            var result = await leadMobileValidateManager.GenerateKarzaAadhaarOtpForNBFC(request);
            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetNBFCBLAccountList")]
        public async Task<GRPCReply<List<BLAccountResponseDC>>> GetNBFCBLAccountList(BLAccountRequestDc request)
        {
            var res = await leadListDetailsManager.GetNBFCBLAccountList(request);
            return res;
        }

        [HttpGet]
        [Route("GetUserNameByUserId")]
        public async Task<GRPCReply<string>> GetUserNameByUserId(string Usersid)
        {
            var result = await leadMobileValidateManager.GetUserNameByUserId(Usersid);
            return result;
        }

        [HttpPost]
        [Route("CustomerMobileValidate")]
        [AllowAnonymous]
        public async Task<CustomerMobileResponse> CustomerMobileValidate(CustomerMobileValidate customerMobileValidate)
        {
            return await leadMobileValidateManager.CustomerMobileValidate(customerMobileValidate, Token);
        }

        #region Change KYCReject to KYCSuccess
        [HttpPost]
        [Route("UpdateKYCStatus")]
        public async Task<GRPCReply<bool>> UpdateKYCStatus(UpdateKYCStatusDc request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool> { Message = "You are not authorize." };
            string role = "";
            if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.SuperAdmin.ToLower())))
                role = UserRoleConstants.SuperAdmin;
            if (role == UserRoleConstants.SuperAdmin)
            {
                List<string> UserIdList = new List<string> { UserId };
                UserListDetailsRequest userReq = new UserListDetailsRequest
                {
                    userIds = UserIdList,
                    keyword = null,
                    Skip = 0,
                    Take = 10
                };
                var userReply = await _identityService.GetUserById(userReq);
                request.UserName = userReply.UserListDetails != null ? userReply.UserListDetails.Select(y => y.UserName).FirstOrDefault() : "";

                var response = await _leadService.UpdateKYCStatus(request);
                List<string> UserNameList = new List<string> { response.Response.UserName };
                return await _kycService.UpdateAadharStatus(new KYCAllInfoByUserIdsRequest
                {
                    UserId = UserNameList,
                    KycMasterCode = 0,
                    ProductCode = response.Response.ProductCode,
                    CreatedBy = UserId
                });

            }
            else
            {
                gRPCReply.Status = false;
                return gRPCReply;
            }

        }

        #endregion

    }
}

