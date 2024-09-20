using IdentityServer4.Validation;
using MassTransit.Initializers;
using Microsoft.Data.SqlClient;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using System;
using System.Collections.Generic;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class LeadListDetailsManager
    {
        private IKycService _iKycService;
        private IProductService _iProductService;
        private ILeadService _iLeadService;
        private IMediaService _iMediaService;
        private ILocationService _iLocationService;
        private ICompanyService _iCompanyService;
        private ILoanAccountService _iLoanAccountService;
        private KYCUserDetailManager _kYCUserDetailManager;
        public LeadListDetailsManager(IKycService iKycService, IProductService iProductService
            , ILeadService iLeadService, IMediaService iMediaService, ILocationService iLocationService
            , ICompanyService companyService
            , ILoanAccountService iLoanAccountService
            , KYCUserDetailManager kYCUserDetailManager)
        {
            _iKycService = iKycService;
            _iProductService = iProductService;
            _iLeadService = iLeadService;
            _iMediaService = iMediaService;
            _iLocationService = iLocationService;
            _iCompanyService = companyService;
            _iLoanAccountService = iLoanAccountService;
            _kYCUserDetailManager = kYCUserDetailManager;
        }

        public async Task<LeadListPageListDTO> GetLeadForListPage(LeadListPageRequest LeadListPageRequest)
        {
            //string role = "";
            //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.AYEOperationExecutive.ToLower())))
            //    role = UserRoleConstants.AYEOperationExecutive;
            //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.MASOperationExecutive.ToLower())))
            //    role = UserRoleConstants.MASOperationExecutive;
            //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.MASOperationExecutive.ToLower())) &&
            //    UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.AYEOperationExecutive.ToLower())))
            //    role = "BothRole";
            //LeadListPageRequest.Role = role;

            if (LeadListPageRequest != null && LeadListPageRequest.ProductType != null)
            {
                GRPCRequest<string> gRPCRequest = new GRPCRequest<string>();
                gRPCRequest.Request = LeadListPageRequest.ProductType;
                var productList = await _iProductService.GetProductByProductType(gRPCRequest);
                if (productList != null && productList.Response != null && productList.Status)
                {
                    foreach (var product in productList.Response)
                    {
                        LeadListPageRequest.ProductId.Add(product.ProductId);
                    }
                }
            }

            var leadslist = new LeadListPageReply();
            List<long> loanaccountResponseleadids = new List<long>();
            GRPCReply<List<LoanAccountDisbursementResponse>> loanResult = new GRPCReply<List<LoanAccountDisbursementResponse>>();

            if (LeadListPageRequest != null && LeadListPageRequest.IsDSA && LeadListPageRequest.UserIds != null)
            {
                var productReply = await _iProductService.GetUsersIDS(LeadListPageRequest);
                if (productReply != null && productReply.Status && productReply.Response != null)
                    LeadListPageRequest.UserIds = productReply.Response;
            }
            if (LeadListPageRequest != null && LeadListPageRequest.Skip == 0 && LeadListPageRequest.Take == 0)
            {
                leadslist = await _iLeadService.GetLeadForListPageExport(LeadListPageRequest);
                if (leadslist.LeadListDetails != null && leadslist.LeadListDetails.Count > 0)
                {
                    GRPCRequest<List<long>> rq = new GRPCRequest<List<long>>();
                    rq.Request = leadslist.LeadListDetails.Select(x => x.Id).ToList();
                    loanResult = await _iLoanAccountService.GetLoanAccountDisbursement(rq);
                    if (loanResult.Response != null && loanResult.Status)
                    {
                        loanaccountResponseleadids = loanResult.Response.Where(x => rq.Request.Contains(x.LeadId)).Select(y => y.LeadId).ToList();
                    }
                }
            }
            else
            {
                leadslist = await _iLeadService.GetLeadForListPage(LeadListPageRequest);
            }

            LeadListPageListDTO LeadListPagelistdata = new LeadListPageListDTO();

            List<LeadListPageDTO> list = new List<LeadListPageDTO>();

            //var productData = await _iProductService.GetLeadStatus();

            //GRPCRequest<long> anchorReq = new GRPCRequest<long>();
            //anchorReq.Request = (long)LeadListPageRequest.CompanyId;
            //var company="";
            //if (LeadListPageRequest.CompanyId > 0)
            //{
            //   var com = await _iCompanyService.GetCompanyDataById(anchorReq);
            //    if(com.Response != null && com.Response.CompanyName != null)
            //    {
            //        company = com.Response.CompanyName;
            //    }
            //}

            var citylist = await _iLocationService.GetAllCities();


            if (leadslist.LeadListDetails != null)
            {
                GRPCRequest<KYCSpecificDetailRequest> request = new GRPCRequest<KYCSpecificDetailRequest>();
                request.Request = new KYCSpecificDetailRequest();
                request.Request.KYCReqiredFieldList = new Dictionary<string, List<string>>();
                request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.PAN, new List<string>
                {
                    KYCDetailConstants.NameOnCard
                });
                request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.PersonalDetail, new List<string>
                {
                    KYCPersonalDetailConstants.EmailId, KYCPersonalDetailConstants.AlternatePhoneNo,KYCPersonalDetailConstants.SurrogateType,KYCPersonalDetailConstants.OwnershipType
                });
                request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.BuisnessDetail, new List<string>
                {
                    KYCBuisnessDetailConstants.BusinessName,KYCBuisnessDetailConstants.BusEntityType
                });

                request.Request.UserIdList = leadslist.LeadListDetails.Select(x => new KYCSpecificDetailUserRequest { ProductCode = x.ProductCode, UserId = x.UserId }).ToList();


                var response = await _iKycService.GetKYCSpecificDetail(request);

                var Userdetails = leadslist.LeadListDetails.Select(x => new { x.Id, x.UserId, x.MobileNo, x.CreatedDate, x.ScreenName, x.SequenceNo, x.LastModified, x.LeadCode, x.ActivityId, x.SubActivityId, x.CreditScore, x.Status, x.AnchorName, x.UniqueCode, x.CityId, x.LeadConvertor, x.LeadGenerator, x.CreditLimit, x.Loan_app_id, x.Partner_Loan_app_id, x.ProductCode, x.ArthmateResponse, x.VintageDays, x.RejectionMessage }).ToList();

                List<long> leadidsList = new List<long>();
                leadidsList = leadslist.LeadListDetails.Select(x => x.Id).ToList();
                GRPCRequest<CompanyBuyingHistoryRequestDc> GetCompanyBuyingHistoryRequest = new GRPCRequest<CompanyBuyingHistoryRequestDc>();
                GetCompanyBuyingHistoryRequest.Request = new CompanyBuyingHistoryRequestDc
                {
                    CompanyId = new List<long>(),
                    LeadId = leadidsList
                };
                var GetCompanyBuyingData = await _iLeadService.GetCompanyBuyingHistory(GetCompanyBuyingHistoryRequest);

                if (Userdetails != null)
                {
                    foreach (var i in Userdetails)
                    {
                        LeadListPageDTO leadListPageDTO = new LeadListPageDTO();
                        leadListPageDTO.LeadId = i.Id;
                        leadListPageDTO.UserId = i.UserId;
                        leadListPageDTO.MobileNo = i.MobileNo;
                        leadListPageDTO.CreatedDate = i.CreatedDate;
                        leadListPageDTO.SequenceNo = i.SequenceNo;
                        leadListPageDTO.LastModified = i.LastModified;
                        leadListPageDTO.LeadCode = i.LeadCode;
                        leadListPageDTO.CreditScore = i.CreditScore;
                        leadListPageDTO.Status = i.Status;
                        leadListPageDTO.ScreenName = i.ScreenName;
                        leadListPageDTO.AnchorName = i.AnchorName;
                        leadListPageDTO.UniqueCode = i.UniqueCode;
                        leadListPageDTO.CityId = i.CityId;
                        leadListPageDTO.LeadGenerator = i.LeadGenerator;
                        leadListPageDTO.LeadConvertor = i.LeadConvertor;
                        leadListPageDTO.CreditLimit = i.CreditLimit;
                        leadListPageDTO.Loan_app_id = i.Loan_app_id;
                        leadListPageDTO.Partner_Loan_app_id = i.Partner_Loan_app_id;
                        leadListPageDTO.ProductCode = i.ProductCode;
                        leadListPageDTO.RejectionReasons = i.ArthmateResponse;
                        leadListPageDTO.VintageDays = i.VintageDays;
                        leadListPageDTO.RejectMessage = i.RejectionMessage;

                        if (GetCompanyBuyingData != null && GetCompanyBuyingData.Status && GetCompanyBuyingData.Response != null)
                        {
                            var companyBuying = GetCompanyBuyingData.Response.FirstOrDefault(x => x.LeadId == i.Id);
                            if (companyBuying != null)
                            {
                                leadListPageDTO.AvgMonthlyBuying = companyBuying.MonthTotalAmount;
                            }
                        }
                        if (loanaccountResponseleadids != null && loanaccountResponseleadids.Count > 0)
                        {
                            var id = loanaccountResponseleadids.Where(x => x == i.Id).Select(y => y).FirstOrDefault();
                            if (id > 0)
                            {
                                var loandata = loanResult.Response.Where(x => x.LeadId == id).Select(y => new LoanAccountDisbursementResponse
                                {
                                    OrderAmount = y.OrderAmount,
                                    DisbursementDate = y.DisbursementDate
                                }).FirstOrDefault();

                                if (loandata != null)
                                {
                                    leadListPageDTO.OrderAmount = loandata.OrderAmount;
                                    leadListPageDTO.DisbursementDate = loandata.DisbursementDate;
                                }
                            }

                        }
                        if (response != null && response.Response != null)
                        {
                            var user = response.Response.GetValueOrDefault(i.UserId);

                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.PAN && x.FieldName == KYCDetailConstants.NameOnCard))
                            {
                                leadListPageDTO.CustomerName = user.First(x => x.MasterCode == KYCMasterConstants.PAN && x.FieldName == KYCDetailConstants.NameOnCard).FieldValue;
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.PersonalDetail && x.FieldName == KYCPersonalDetailConstants.EmailId))
                            {
                                leadListPageDTO.EmailId = user.First(x => x.MasterCode == KYCMasterConstants.PersonalDetail && x.FieldName == KYCPersonalDetailConstants.EmailId).FieldValue;
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.PersonalDetail && x.FieldName == KYCPersonalDetailConstants.AlternatePhoneNo))
                            {
                                leadListPageDTO.AlternatePhoneNo = user.First(x => x.MasterCode == KYCMasterConstants.PersonalDetail && x.FieldName == KYCPersonalDetailConstants.AlternatePhoneNo).FieldValue;
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.BuisnessDetail && x.FieldName == KYCBuisnessDetailConstants.BusinessName))
                            {
                                leadListPageDTO.BusinessName = user.First(x => x.MasterCode == KYCMasterConstants.BuisnessDetail && x.FieldName == KYCBuisnessDetailConstants.BusinessName).FieldValue;

                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.PersonalDetail && x.FieldName == KYCPersonalDetailConstants.SurrogateType))
                            {
                                leadListPageDTO.SurrogateType = user.First(x => x.MasterCode == KYCMasterConstants.PersonalDetail && x.FieldName == KYCPersonalDetailConstants.SurrogateType).FieldValue;
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.PersonalDetail && x.FieldName == KYCPersonalDetailConstants.OwnershipType))
                            {
                                leadListPageDTO.OwnershipType = user.First(x => x.MasterCode == KYCMasterConstants.PersonalDetail && x.FieldName == KYCPersonalDetailConstants.OwnershipType).FieldValue;
                            }
                            if (user != null && user.Any() && user.Any(x => x.MasterCode == KYCMasterConstants.BuisnessDetail && x.FieldName == KYCBuisnessDetailConstants.BusEntityType))
                            {
                                leadListPageDTO.BusEntityType = user.First(x => x.MasterCode == KYCMasterConstants.BuisnessDetail && x.FieldName == KYCBuisnessDetailConstants.BusEntityType).FieldValue;

                            }

                        }

                        list.Add(leadListPageDTO);
                    }
                }


                if (list.Any() && citylist.Response != null && citylist.Response.Count > 0 && citylist.Status)
                {
                    var cityData = citylist.Response.ToList();
                    var userlist = list.Where(x => x.CityId > 0).ToList();

                    if (userlist != null)
                    {
                        foreach (var userData in list)
                        {
                            if (userData.CityId > 0)
                            {
                                userData.CityName = cityData.Where(x => x.Id == userData.CityId).Select(y => y.CityName).FirstOrDefault();
                            }
                        }
                    }
                }

            }

            LeadListPagelistdata.TotalCount = leadslist.TotalCount;

            LeadListPagelistdata.leadListPageDTO = list;
            return LeadListPagelistdata;
        }

        public async Task<InitialLeadResponse> LeadCurrentActivityAsync(InitialLeadDTO leadDTO)
        {
            InitialLeadResponse initialLeadResponse = new InitialLeadResponse();
            initialLeadResponse.LeadProductActivity = new List<LeadProductActivity>();
            initialLeadResponse.CurrentSequence = 0;
            var req = new GRPCRequest<LeadActivitySequenceRequest>();
            req.Request = new LeadActivitySequenceRequest
            {
                CompanyId = leadDTO.CompanyId,
                LeadId = leadDTO.LeadId,
                MobileNo = leadDTO.MobileNo,
                ProductId = leadDTO.ProductId
            };
            if (req.Request.LeadId == 0)
            {
                var leaddata = await _iLeadService.GetLeadIdByMobile(req);
                leadDTO.LeadId = leaddata.Response;
            }
            if (leadDTO.LeadId > 0)
            {
                var LeadCurrentActivityReply = await _iLeadService.GetLeadCurrentActivity(new BuildingBlocks.GRPC.Contracts.Lead.DataContracts.LeadActivitySequenceRequest
                {
                    LeadId = leadDTO.LeadId,
                    ProductId = leadDTO.ProductId,
                    CompanyId = leadDTO.CompanyId,
                    MobileNo = leadDTO.MobileNo
                });
                if (LeadCurrentActivityReply.Status)
                {
                    initialLeadResponse.CurrentSequence = LeadCurrentActivityReply.CurrentSequence;
                    if (LeadCurrentActivityReply.LeadActivityReply != null && LeadCurrentActivityReply.LeadActivityReply.Any())
                    {
                        List<ActivitySubActivityNameRequest> request = LeadCurrentActivityReply.LeadActivityReply
                                                                       .Select(x => new ActivitySubActivityNameRequest { ActivityId = x.ActivityId, SubActivityId = x.SubActivityId }).ToList();
                        var ActivityNames = await _iProductService.GetActivitySubActivityName(request);
                        initialLeadResponse.LeadProductActivity = LeadCurrentActivityReply.LeadActivityReply
                                                                       .Select(x => new LeadProductActivity
                                                                       {
                                                                           ActivityMasterId = x.ActivityId,
                                                                           SubActivityMasterId = x.SubActivityId,
                                                                           ActivityName = ActivityNames.Response != null
                                                                                         && ActivityNames.Response.Any(y => y.ActivityId == x.ActivityId)
                                                                                        ? ActivityNames.Response.FirstOrDefault(y => y.ActivityId == x.ActivityId).ActivityName
                                                                                        : "",
                                                                           SubActivityName = ActivityNames.Response != null && x.SubActivityId.HasValue
                                                                                         && ActivityNames.Response.Any(y => y.ActivityId == x.ActivityId && y.SubActivityId == x.SubActivityId)
                                                                                        ? ActivityNames.Response.FirstOrDefault(y => y.ActivityId == x.ActivityId && y.SubActivityId == x.SubActivityId).SubActivityName
                                                                                        : "",
                                                                           LeadId = leadDTO.LeadId,
                                                                           Sequence = x.Sequence,
                                                                           IsEditable = !x.IsCompleted || x.IsApprove == 2 ? true : false,
                                                                           RejectedReason = x.RejectedReason
                                                                       }).ToList();

                        if (initialLeadResponse.LeadProductActivity.Any(x => x.ActivityName == ActivityEnum.Rejected.ToString() && x.Sequence == initialLeadResponse.CurrentSequence))
                        {
                            var preActivityEditable = initialLeadResponse.LeadProductActivity.FirstOrDefault(x => x.Sequence == initialLeadResponse.CurrentSequence - 1);
                            if (preActivityEditable != null)
                            {
                                preActivityEditable.IsEditable = true;
                            }
                        }
                    }
                }

            }

            if (leadDTO.LeadId == 0 || initialLeadResponse.LeadProductActivity == null || !initialLeadResponse.LeadProductActivity.Any())
            {
                var companyreply = await _iCompanyService.GetFinTechCompany();
                long fintechCompanyId = 0;
                if (companyreply.Status)
                {
                    fintechCompanyId = companyreply.Response;

                    var prodActivityReply = await _iProductService.GetProductActivity(new BuildingBlocks.GRPC.Contracts.Product.DataContracts.LeadProductRequest
                    {
                        CompanyId = fintechCompanyId,
                        ProductId = leadDTO.ProductId,
                        AnchorCompanyId = leadDTO.CompanyId,
                        CompanyType = CompanyTypeEnum.FinTech.ToString()
                    });
                    if (prodActivityReply.Status)
                    {
                        initialLeadResponse.LeadProductActivity = prodActivityReply.LeadProductActivity.Any() ? prodActivityReply.LeadProductActivity.Select(x => new LeadProductActivity
                        {
                            ActivityMasterId = x.ActivityMasterId,
                            ActivityName = x.ActivityName,
                            KycMasterCode = x.KycMasterCode,
                            Sequence = x.Sequence,
                            SubActivityMasterId = x.SubActivityMasterId,
                            SubActivityName = x.SubActivityName,
                            LeadId = x.LeadId,
                            IsEditable = true
                        }).ToList() : new List<LeadProductActivity>();

                    }
                }
            }

            return initialLeadResponse;
        }

        public async Task<LeadActivityMasterProgressListReply> GetLeadActivityProgressList(long leadId)
        {
            LeadActivityMasterProgressListReply res = new LeadActivityMasterProgressListReply();

            if (leadId > 0)
            {
                var LeadCurrentActivityReply = await _iLeadService.GetLeadActivityProgressList(new LeadActivityProgressListRequest
                {
                    LeadId = leadId
                });
                if (LeadCurrentActivityReply.Status)
                {
                    if (LeadCurrentActivityReply.Response != null && LeadCurrentActivityReply.Response.Any())
                    {
                        var requiredList = LeadCurrentActivityReply.Response
                            .Select(x => new LeadActivityProgress
                            {
                                ActivityMasterId = x.ActivityId,
                                SubActivityMasterId = x.SubActivityId,
                                ActivityName = x.ActivityName,
                                //ActivityNames.Response != null
                                //               && ActivityNames.Response.Any(y => y.ActivityId == x.ActivityId)
                                //           ? ActivityNames.Response.FirstOrDefault(y => y.ActivityId == x.ActivityId).ActivityName
                                //           : "",
                                SubActivityName = x.SubActivityName,
                                //ActivityNames.Response != null && x.SubActivityId.HasValue
                                //                && ActivityNames.Response.Any(y => y.ActivityId == x.ActivityId && y.SubActivityId == x.SubActivityId)
                                //            ? ActivityNames.Response.FirstOrDefault(y => y.ActivityId == x.ActivityId && y.SubActivityId == x.SubActivityId).SubActivityName
                                //            : "",
                                Sequence = x.Sequence,
                                IsApproved = x.IsApproved,
                                IsCompleted = x.IsCompleted,
                                leadUserId = x.leadUserId,
                                RejectMessage = x.RejectMessage
                            }).ToList();


                        if (requiredList != null && requiredList.Any())
                        {
                            res.LeadActivityProgress = requiredList;
                            res.Status = true;
                        }

                    }
                }
                else
                {
                    res.Message = LeadCurrentActivityReply.Message;
                    res.Status = LeadCurrentActivityReply.Status;
                }
            }
            else
            {
                res.Message = "Lead Not Found";
                res.Status = false;
            }
            return res;
        }
        public async Task<GRPCReply<FileUploadRequest>> CustomerNBFCAgreement(GRPCRequest<NBFCAgreement> request)
        {
            var res = await _iLeadService.CustomerNBFCAgreement(request);
            return res;
        }
        public async Task<GRPCReply<DocReply>> UploadNBFCAgreement(GRPCRequest<FileUploadRequest> request)
        {
            var res = await _iMediaService.SaveFile(request);
            return res;
        }
        public async Task<GRPCReply<bool>> AddLeadAgreement(GRPCRequest<LeadAgreementDc> request)
        {
            var res = await _iLeadService.AddLeadAgreement(request);
            return res;
        }
        public async Task<GRPCReply<LeadCreditLimit>> GetCreditLimitByLeadId(GRPCRequest<long> LeadId)
        {
            var res = await _iLeadService.GetCreditLimitByLeadId(LeadId);
            return res;
        }
        public async Task<GRPCReply<double?>> GetCommissionConfig(List<long> ProductIds, string UserId)
        {
            GRPCReply<double?> gRPCReply = new GRPCReply<double?>();

            var commissionConfigByProductId = await _iProductService.GetCommissionConfigByProductId(new GRPCRequest<List<long>>
            {
                Request = ProductIds
            });

            if (commissionConfigByProductId.Status && commissionConfigByProductId.Response != null)
            {
                gRPCReply.Response = commissionConfigByProductId.Response.CommissionValue != null ? commissionConfigByProductId.Response.CommissionValue : null;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<VerifyLeadDocumentReply>> VerifyLeadDocument(VerifyLeadDocumentRequest _request)
        {
            GRPCRequest<VerifyLeadDocumentRequest> request = new GRPCRequest<VerifyLeadDocumentRequest>();

            LeadActivityMasterProgressListReply res = await GetLeadActivityProgressList(_request.LeadId);

            //Personal Or Bussinesss Update 
            if (_request != null && res != null && res.Status)
            {
                var LeadProdReply = await _iLeadService.GetLeadProductId(_request.LeadId);

                if (_request.IsApprove == 1 && res.LeadActivityProgress.Any(x => x.ActivityMasterId == _request.ActivityMasterId && x.SubActivityMasterId == _request.SubActivityMasterId
                && (x.ActivityName == "PersonalInfo" || x.ActivityName == "BusinessInfo")
                ))
                {
                    List<string> MasterCodeList = new List<string>
                    {
                         KYCMasterConstants.PAN,
                         KYCMasterConstants.PersonalDetail,
                         KYCMasterConstants.BuisnessDetail,
                         KYCMasterConstants.Aadhar,
                         KYCMasterConstants.Selfie,
                         KYCMasterConstants.BankStatementCreditLending,
                         KYCMasterConstants.MSME,
                         KYCMasterConstants.BankStatement
                    };

                    var kYCUserDetail = await _kYCUserDetailManager.GetLeadDetailAll(LeadProdReply.Response.UserId, LeadProdReply.Response.ProductCode, MasterCodeList, IsGetBankStatementDetail: false, IsGetCreditBureau: false, IsGetAgreement: false);
                    if (kYCUserDetail != null && kYCUserDetail.panDetail != null && kYCUserDetail.PersonalDetail != null && kYCUserDetail.BuisnessDetail != null && kYCUserDetail.aadharDetail != null)
                    {
                        kYCUserDetail.UserId = LeadProdReply.Response.UserId;
                        GRPCRequest<UserDetailsReply> kYCUserrequest = new GRPCRequest<UserDetailsReply>();
                        kYCUserrequest.Request = kYCUserDetail;
                        var kycupdateres = await _iLeadService.AddUpdateLeadDetail(kYCUserrequest);
                        if (kycupdateres != null && !kycupdateres.Status)
                        {
                            request.Request = _request;
                            return await _iLeadService.VerifyLeadDocument(request);

                        }
                    }
                }
                if (res != null && res.LeadActivityProgress.Any(x => x.ActivityMasterId == _request.ActivityMasterId
                && x.SubActivityMasterId == _request.SubActivityMasterId && (x.ActivityName == ActivityConstants.DSATypeSelection)
                && _request.IsApprove == 2))
                {
                    var reply = _kYCUserDetailManager.RemoveDSAPersonalDetails(LeadProdReply.Response.UserId);
                }
            }
            request.Request = _request;
            return await _iLeadService.VerifyLeadDocument(request);

        }

        public async Task<GRPCReply<List<SCAccountResponseDc>>> GetSCAccountList(SCAccountRequestDC request)
        {
            var reply = await _iLeadService.GetSCAccountList(request);
            var citylist = await _iLocationService.GetAllCities();

            if (reply.Response != null && reply.Response.Any() && citylist.Response != null && citylist.Response.Count > 0 && citylist.Status)
            {
                var cityData = citylist.Response.ToList();
                var userlist = reply.Response.Where(x => x.CityId > 0).ToList();

                if (userlist != null)
                {
                    foreach (var userData in reply.Response)
                    {
                        if (userData.CityId > 0)
                        {
                            userData.CityName = cityData.Where(x => x.Id == userData.CityId).Select(y => y.CityName).FirstOrDefault();
                        }
                    }
                }
            }
            return reply;
        }
        public async Task<GRPCReply<List<BLAccountResponseDC>>> GetBLAccountList(BLAccountRequestDc request)
        {
            if (request != null)
            {
                LeadListPageRequest req = new LeadListPageRequest();
                req.ProductId = new List<long>();
                var productList = await _iProductService.GetProductByProductType(new GRPCRequest<string> { Request = ProductTypeConstants.BusinessLoan });
                if (productList != null && productList.Response != null && productList.Status)
                {
                    req.ProductId = productList.Response.Select(x => x.ProductId).ToList();
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

              
                var productReply = await _iProductService.GetUsersIDS(req);
                if (productReply != null && productReply.Status && productReply.Response != null) request.UserIds = productReply.Response;
            }
            var reply = await _iLeadService.GetBLAccountList(request);
            var citylist = await _iLocationService.GetAllCities();

            if (reply.Response != null && reply.Response.Any() && citylist.Response != null && citylist.Response.Count > 0 && citylist.Status)
            {
                var cityData = citylist.Response.ToList();
                var userlist = reply.Response.Where(x => x.CityId > 0).ToList();

                if (userlist != null)
                {
                    foreach (var userData in reply.Response)
                    {
                        if (userData.CityId > 0)
                        {
                            userData.CityName = cityData.Where(x => x.Id == userData.CityId).Select(y => y.CityName).FirstOrDefault();
                        }
                    }
                }
            }

            return reply;
        }

        public async Task<GRPCReply<DSALeadListPageDTO>> GetDSALeadDataById(long LeadId)
        {
            GRPCReply<DSALeadListPageDTO> gRPCReply = new GRPCReply<DSALeadListPageDTO> { Message = "Data not found!!!" };

            var response = await _iLeadService.GetDSALeadDataById(new GRPCRequest<LeadRequestDataDC>
            {
                Request = new LeadRequestDataDC { LeadId = LeadId, Status = "" }
            });
            if (response != null && response.Response != null && !string.IsNullOrEmpty(response.Response.CreatedBy) && !string.IsNullOrEmpty(response.Response.UserId))
            {
                var AnchorName = "";
                var productReply = await _iProductService.GetSalesAgentDetails(new GRPCRequest<string> { Request = response.Response.CreatedBy });
                if (productReply != null && productReply.Response != null && productReply.Response.AnchorCompanyId > 0)
                {
                    GRPCReply<CompanyDetailDc> companyReply = await _iCompanyService.GetCompany(new GRPCRequest<long> { Request = productReply.Response.AnchorCompanyId });
                    if (companyReply.Status)
                    {
                        AnchorName = companyReply.Response.CompanyName;
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
                        SalesAgentCommissions = response.Response.SalesAgentCommissions != null && response.Response.SalesAgentCommissions.Any() ? response.Response.SalesAgentCommissions:new List<SalesAgentCommissionList>(),
                        profileType = productReply.Response.Type != null ? productReply.Response.Type : null,
                        CustomerName = productReply.Response.FullName != null ? productReply.Response.FullName : "",
                        AnchorName = AnchorName,
                        DSALeadCode = productReply.Response.DSACode != null ? productReply.Response.DSACode : ""
                    };

                }
                gRPCReply.Status = true;
                gRPCReply.Message = "Data found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> UploadMASfinanceAgreement(MASFinanceAgreementDc financeAgreementDc)
        {
            //var validRoles = new[] { UserRoleConstants.MASOperationExecutive, UserRoleConstants.AYEOperationExecutive };
            //if (validRoles.Contains(role))
            if (financeAgreementDc.NbfcCompanyId > 0)
            {
                return await _iLeadService.UploadMASfinanceAgreement(new GRPCRequest<MASFinanceAgreementDc>
                {
                    Request = financeAgreementDc
                });
            }
            else
            {
                return new GRPCReply<string> { Message = "Not Authorised" };
            }
        }

        public async Task<GRPCReply<List<CityMasterListReply>>> GetAllLeadCities()
        {
            GRPCReply<List<CityMasterListReply>> gRPCReply = new GRPCReply<List<CityMasterListReply>> { Message = "Data not found!!!" };

            var response = await _iLeadService.GetAllLeadCities();
            if (response != null && response.Status && response.Response.CityIds.Count > 0)
            {
                LeadCityIds leadCityIds = new LeadCityIds();
                leadCityIds.CityIds = response.Response.CityIds;
                var cityList = await _iLocationService.GetAllLeadCities(leadCityIds);
                gRPCReply.Response = cityList.Response;
                gRPCReply.Status = true;
                gRPCReply.Message = "Data found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<BLAccountResponseDC>>> GetNBFCBLAccountList(BLAccountRequestDc request)
        {
            if (request != null)
            {
                //string role = "";
                //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.AYEOperationExecutive.ToLower())))
                //    role = UserRoleConstants.AYEOperationExecutive;
                //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.MASOperationExecutive.ToLower())))
                //    role = UserRoleConstants.MASOperationExecutive;
                //if (UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.MASOperationExecutive.ToLower())) &&
                //    UserRoles.Split(",").Any(x => x.ToLower().Contains(UserRoleConstants.AYEOperationExecutive.ToLower())))
                //    role = "BothRole";
                //request.Role = role;
            }
            var reply = await _iLeadService.GetNBFCBLAccountList(request);
            var citylist = await _iLocationService.GetAllCities();

            if (reply.Response != null && reply.Response.Any() && citylist.Response != null && citylist.Response.Count > 0 && citylist.Status)
            {
                var cityData = citylist.Response.ToList();
                var userlist = reply.Response.Where(x => x.CityId > 0).ToList();

                if (userlist != null)
                {
                    foreach (var userData in reply.Response)
                    {
                        if (userData.CityId > 0)
                        {
                            userData.CityName = cityData.Where(x => x.Id == userData.CityId).Select(y => y.CityName).FirstOrDefault();
                        }
                    }
                }
            }
            return reply;
        }

    }
}
