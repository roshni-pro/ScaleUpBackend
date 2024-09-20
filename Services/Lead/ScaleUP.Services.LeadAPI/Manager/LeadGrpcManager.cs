using Azure;
using Google.Protobuf.WellKnownTypes;
using IdentityServer4.Extensions;
using MassTransit;
using MassTransit.Initializers;
using MassTransit.Internals;
using MassTransit.SagaStateMachine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using Nito.AsyncEx;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.ArthMate;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.NBFC;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Helper;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using ScaleUP.Services.LeadAPI.Migrations;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.eSign;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadDTO.NBFC.BlackSoil;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Services.LeadModels.BusinessLoan;
using ScaleUP.Services.LeadModels.DSA;
using ScaleUP.Services.LeadModels.LeadNBFC;
using System;
using System.Collections.Generic;
using System.Data;

using System.Reflection.Emit;
using System.Xml;
using static IdentityServer4.Models.IdentityResources;
using static iTextSharp.text.pdf.AcroFields;
using static MassTransit.ValidationResultExtensions;



namespace ScaleUP.Services.LeadAPI.Manager
{
    public class LeadGrpcManager
    {

        private readonly LeadApplicationDbContext _context;
        private readonly IHostEnvironment _environment;
        private readonly NBFCSchedular _nBFCSchedular;
        private readonly LeadNBFCFactory _leadNBFCFactory;


        private readonly LeadHistoryManager _leadHistoryManager;
        private readonly IMassTransitService _massTransitService;

        public LeadGrpcManager(LeadApplicationDbContext context, IHostEnvironment environment, NBFCSchedular nBFCSchedular, LeadNBFCFactory leadNBFCFactory
            , LeadHistoryManager leadHistoryManager, IMassTransitService massTransitService)
        {
            _context = context;
            _environment = environment;
            _nBFCSchedular = nBFCSchedular;
            _leadNBFCFactory = leadNBFCFactory;
            _leadHistoryManager = leadHistoryManager;
            _massTransitService = massTransitService;
        }

        public LeadActivitySequenceResponse GetLeadCurrentActivity(LeadActivitySequenceRequest request)
        {
            LeadActivitySequenceResponse leadActivitySequenceResponse = new LeadActivitySequenceResponse();
            long? LeadId = (_context.Leads.FirstOrDefault(x => x.MobileNo == request.MobileNo && x.ProductId == request.ProductId && x.IsActive && !x.IsDeleted))?.Id;
            int? SequenceNo = 0;
            if (LeadId.HasValue)
            {
                var leadActivities = _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == LeadId.Value && x.IsActive && !x.IsDeleted)
                     .Select(x => new { x.ActivityMasterId, x.SubActivityMasterId, x.Sequence, x.IsCompleted, x.IsApproved, x.RejectMessage }).ToList();

                if (leadActivities.Any())
                {
                    leadActivitySequenceResponse.LeadActivityReply = leadActivities.Select(x => new LeadActivityReply
                    {
                        ActivityId = x.ActivityMasterId,
                        Sequence = x.Sequence,
                        SubActivityId = x.SubActivityMasterId,
                        IsApprove = x.IsApproved,
                        IsCompleted = x.IsCompleted,
                        RejectedReason = x.RejectMessage
                    }).ToList();
                    SequenceNo = (leadActivities.Where(x => (!x.IsCompleted || x.IsApproved == 2)).OrderBy(x => x.Sequence).FirstOrDefault())?.Sequence;
                }
                if (!SequenceNo.HasValue || SequenceNo.Value == 0)
                    SequenceNo = 1;
            }
            leadActivitySequenceResponse.CurrentSequence = SequenceNo ?? 0;
            leadActivitySequenceResponse.Status = true;
            leadActivitySequenceResponse.Message = "";
            return leadActivitySequenceResponse;

        }

        public async Task<LeadListPageReply> GetLeadForListPage(LeadListPageRequest request)
        {
            var predicate = PredicateBuilder.True<Leads>();
            LeadListPageReply leadListPageReply = new LeadListPageReply();

            List<LeadListDetail> leads = new List<LeadListDetail>();

            List<string> statusList = new List<string>();
            statusList.Add(LeadBusinessLoanStatusConstants.Pending);
            statusList.Add(LeadBusinessLoanStatusConstants.Pending.ToLower());
            statusList.Add(LeadBusinessLoanStatusConstants.LoanInitiated);
            statusList.Add(LeadBusinessLoanStatusConstants.LoanActivated);
            statusList.Add(LeadBusinessLoanStatusConstants.LoanApproved);
            statusList.Add(LeadBusinessLoanStatusConstants.LoanRejected);
            statusList.Add(LeadBusinessLoanStatusConstants.LoanAvailed);
            statusList.Add(LeadStatusEnum.LineInitiated.ToString());
            statusList.Add(LeadStatusEnum.LineActivated.ToString());
            statusList.Add(LeadStatusEnum.LineApproved.ToString());
            statusList.Add(LeadStatusEnum.LineRejected.ToString());

            predicate = predicate.And(x => !statusList.Contains(x.Status) && !x.IsDeleted && x.CompanyLeads.Any(y => !y.IsDeleted));

            if (request.ToDate != null)
                request.ToDate = request.ToDate?.Date.AddDays(1).AddMilliseconds(-1);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                predicate = predicate.And(x => x.MobileNo.Contains(request.Keyword.Trim()) || x.LeadCode.Contains(request.Keyword.Trim())
                                                || x.ApplicantName.Contains(request.Keyword.Trim()));
            }
            //if (!string.IsNullOrEmpty(request.Role) && request.Role.ToLower() == UserRoleConstants.MASOperationExecutive.ToLower())
            //{
            //    predicate = predicate.And(x => x.LeadOffers.Any(y => y.CompanyIdentificationCode != null && y.CompanyIdentificationCode.Contains(CompanyIdentificationCodeConstants.MASFIN) && !y.IsDeleted && y.IsActive));
            //}
            //else if (!string.IsNullOrEmpty(request.Role) && request.Role.ToLower() == UserRoleConstants.AYEOperationExecutive.ToLower())
            //{
            //    predicate = predicate.And(x => x.LeadOffers.Any(y => y.CompanyIdentificationCode != null && y.CompanyIdentificationCode.Contains(CompanyIdentificationCodeConstants.AYEFIN) && !y.IsDeleted && y.IsActive));
            //}
            if (request.UserType.ToLower() != UserTypeConstants.AdminUser.ToLower() && request.UserType.ToLower() != UserTypeConstants.SuperAdmin.ToLower())
            {
                predicate = predicate.And(x => x.LeadOffers.Any(y => y.NBFCCompanyId == request.NbfcCompanyId && !y.IsDeleted && y.IsActive));
            }
            //else if (!string.IsNullOrEmpty(request.Role) && request.Role == "BothRole")
            //{
            //    predicate = predicate.And(x => x.LeadOffers.Any(y => y.CompanyIdentificationCode != null && !y.IsDeleted && y.IsActive &&
            //    (y.CompanyIdentificationCode == CompanyIdentificationCodeConstants.MASFIN || y.CompanyIdentificationCode == CompanyIdentificationCodeConstants.AYEFIN)));
            //}
            if (request.FromDate != null && request.ToDate != null)
            {
                predicate = predicate.And(x => x.Created >= request.FromDate && x.Created <= request.ToDate);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                predicate = predicate.And(x => x.Status == request.Status);
            }

            if (request.CityId > 0)
            {
                predicate = predicate.And(x => x.CityId == request.CityId);
            }

            if (request.CompanyId != null && request.CompanyId.Count() > 0)
            {
                predicate = predicate.And(x => x.CompanyLeads.Any(y => request.CompanyId.Contains((int)y.CompanyId) && !y.IsDeleted));
            }

            if (request.ProductId != null && request.ProductId.Any() && request.ProductId.Count > 0)
            {
                predicate = predicate.And(x => request.ProductId.Contains(x.ProductId));
            }
            if (request.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower() || request.UserType.ToLower() == UserTypeConstants.SuperAdmin.ToLower())
            {
                if (request.IsDSA)
                {
                    predicate = predicate.And(x => request.UserIds != null && x.CreatedBy != null && request.UserIds.Contains(x.CreatedBy));
                }
                else
                {
                    predicate = predicate.And(x => x.CreatedBy == null || x.UserName == x.CreatedBy);
                }
            }
            //predicate = predicate.And(x => !statusList.Contains(x.Status));
            var leadData = await _context.Leads.Where(predicate).Include(y => y.CompanyLeads).OrderByDescending(z => z.Id).ToListAsync();

            leads = leadData.Skip((request.Skip - 1) * request.Take).Take(request.Take).Select(x => new LeadListDetail
            {
                CreatedDate = x.Created,
                Id = x.Id,
                MobileNo = x.MobileNo,
                UserId = x.UserName,
                LastModified = x.LastModified,
                LeadCode = x.LeadCode,
                CreditScore = x.CreditScore,
                Status = x.Status,
                CityId = x.CityId,
                LeadGenerator = x.LeadGenerator,
                LeadConvertor = x.LeadConverter,
                CreditLimit = x.CreditLimit,
                ProductCode = x.ProductCode,
                IsActive = x.IsActive,
                //PayoutPercentage = 0

            }).ToList();
            leadListPageReply.TotalCount = leadData.Count();

            List<long> LeadIds = leads.Select(x => x.Id).ToList();
            if (LeadIds != null && LeadIds.Any())
            {
                var companyName = await _context.CompanyLead.Where(x => LeadIds.Contains(x.LeadId)).Select(y => new
                {
                    leadId = y.LeadId,
                    anchorName = y.AnchorName,
                    uniquecode = y.UserUniqueCode,
                    companyId = y.CompanyId
                }).ToListAsync();

                if (leads != null && leads.Count > 0)
                {
                    foreach (var lead in leads)
                    {

                        if (companyName != null)
                        {
                            var companyData = companyName.FirstOrDefault(x => x.leadId == lead.Id);
                            lead.AnchorName = companyData.anchorName;
                            lead.UniqueCode = string.Join(",", companyName.Where(x => x.leadId == lead.Id).Select(y => y.uniquecode).ToList());
                            lead.AnchorCompanyId = companyData.companyId;
                        }
                    }
                }
            }

            if (LeadIds != null && LeadIds.Any())
            {
                var leadloandata = await _context.LeadLoan.Where(x => x.LeadMasterId.HasValue && LeadIds.Contains(x.LeadMasterId.Value)).Select(y => new
                {
                    leadId = y.LeadMasterId,
                    loan_app_id = y.loan_app_id,
                    partner_loan_app_id = y.partner_loan_app_id
                }).ToListAsync();

                if (leads != null && leads.Count > 0)
                {
                    foreach (var lead in leads)
                    {
                        if (leadloandata != null)
                        {
                            var leadloan = leadloandata.FirstOrDefault(x => x.leadId == lead.Id);
                            if (leadloan != null)
                            {
                                lead.Loan_app_id = leadloan.loan_app_id;
                                lead.Partner_Loan_app_id = leadloan.partner_loan_app_id;
                            }
                        }
                    }
                }
            }


            if (LeadIds != null && LeadIds.Any())
            {
                var data = await _context.LeadActivityMasterProgresses.Where(x => LeadIds.Contains(x.LeadMasterId) && x.IsCompleted == true).OrderBy(x => x.Sequence).GroupBy(x => x.LeadMasterId)
                    .Select(x => new
                    {
                        LeadId = x.Key,
                        Sequence = x.Max(y => y.Sequence)
                    ,
                        ActivityId = x.OrderByDescending(y => y.Sequence).FirstOrDefault().ActivityMasterId,
                        SubActivityId = x.OrderByDescending(y => y.Sequence).FirstOrDefault().SubActivityMasterId,
                        ActivityMasterName = x.OrderByDescending(y => y.Sequence).FirstOrDefault().ActivityMasterName,
                        SubactivityMasterName = x.OrderByDescending(y => y.Sequence).FirstOrDefault().SubActivityMasterName
                    }).ToListAsync();

                if (data != null)
                {
                    foreach (var item in leads)
                    {
                        if (data.Any(x => x.LeadId == item.Id))
                        {
                            item.SequenceNo = data.FirstOrDefault(x => x.LeadId == item.Id).Sequence;
                            item.SubActivityId = data.FirstOrDefault(x => x.LeadId == item.Id).SubActivityId;
                            item.ActivityId = data.FirstOrDefault(x => x.LeadId == item.Id).ActivityId;
                            item.ScreenName = string.IsNullOrEmpty(data.FirstOrDefault(x => x.LeadId == item.Id).SubactivityMasterName) ? (data.FirstOrDefault(x => x.LeadId == item.Id).SubactivityMasterName + " " + data.FirstOrDefault(x => x.LeadId == item.Id).ActivityMasterName) : data.FirstOrDefault(x => x.LeadId == item.Id).ActivityMasterName;
                        }
                    }
                }

            }
            leadListPageReply.LeadListDetails = leads;

            return leadListPageReply;
        }

        public GRPCReply<List<LeadActivityProgressListReply>> GetLeadActivityProgressList(LeadActivityProgressListRequest req)
        {
            GRPCReply<List<LeadActivityProgressListReply>> reply = new GRPCReply<List<LeadActivityProgressListReply>>();
            var leaduserid = _context.Leads.Where(x => x.IsActive && !x.IsDeleted && x.Id == req.LeadId).Select(x => x.UserName).FirstOrDefault();
            var leadActivities = _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == req.LeadId && x.IsActive && !x.IsDeleted)
                .Select(x => new LeadActivityProgressListReply
                {
                    ActivityId = x.ActivityMasterId,
                    SubActivityId = x.SubActivityMasterId,
                    Sequence = x.Sequence,
                    IsCompleted = x.IsCompleted,
                    IsApproved = x.IsApproved,
                    leadUserId = leaduserid,
                    ActivityName = x.ActivityMasterName,
                    SubActivityName = x.SubActivityMasterName

                }).ToList();

            if (leadActivities != null && leadActivities.Any())
            {
                reply.Response = leadActivities;
                reply.Status = true;
            }
            else
            {
                reply.Message = "Lead Activity Not Found";
                reply.Status = false;
            }
            return reply;
        }

        public async Task<GRPCReply<long>> LeadInitiate(InitiateLeadDetail initiateLeadDetail)
        {
            GRPCReply<long> Reply = new GRPCReply<long>();
            Leads? leads = _context.Leads.FirstOrDefault(x => x.ProductId == initiateLeadDetail.ProductId && x.MobileNo == initiateLeadDetail.MobileNumber && x.IsActive && !x.IsDeleted);

            if (leads == null)
            {
                List<LeadActivityMasterProgresses> leadActivityMasterProgresses = new List<LeadActivityMasterProgresses>();

                if (initiateLeadDetail.ProductActivities != null && initiateLeadDetail.ProductActivities.Any())
                {
                    foreach (var item in initiateLeadDetail.ProductActivities)
                    {
                        leadActivityMasterProgresses.Add(new LeadActivityMasterProgresses
                        {
                            ActivityMasterId = item.ActivityMasterId,
                            IsActive = (item.ActivityName == ActivityEnum.Rejected.ToString()) ? false : true,
                            IsApproved = 0,
                            IsCompleted = false,
                            IsDeleted = false,
                            LeadMasterId = 0,
                            Sequence = item.Sequence,
                            SubActivityMasterId = item.SubActivityMasterId ?? 0,
                            ActivityMasterName = item.ActivityName,
                            SubActivityMasterName = item.SubActivityName
                        });

                    }

                }

                string LeadNo = "LeadNo";
                GRPCRequest<string> req = new GRPCRequest<string>();
                req.Request = LeadNo;
                var res = await GetCurrentNumber(req);

                leads = new Leads
                {
                    MobileNo = initiateLeadDetail.MobileNumber,
                    ProductId = initiateLeadDetail.ProductId,
                    UserName = initiateLeadDetail.UserId,
                    LeadCode = res.Response,
                    IsActive = true,
                    CibilReport = "",
                    ProductCode = initiateLeadDetail.ProductCode,
                    LeadActivityMasterProgresses = leadActivityMasterProgresses,
                    CityId = initiateLeadDetail.CityId
                };
                _context.Leads.Add(leads);
                var result = await _context.SaveChangesAsync();
            }
            if (!_context.CompanyLead.Any(x => x.CompanyId == initiateLeadDetail.AnchorCompanyId && x.LeadId == leads.Id && x.IsActive && !x.IsDeleted))
            {
                var status = _context.CompanyLead.Any(x => x.LeadId == leads.Id && x.LeadProcessStatus == 0);

                var LeadCompanyBuyingHistories = (initiateLeadDetail.CustomerBuyingHistories != null && initiateLeadDetail.CustomerBuyingHistories.Any()) ? initiateLeadDetail.CustomerBuyingHistories.Select(x => new LeadCompanyBuyingHistory
                {
                    CompanyLeadId = 0,
                    MonthFirstBuyingDate = x.MonthFirstBuyingDate,
                    MonthTotalAmount = x.MonthTotalAmount,
                    TotalMonthInvoice = x.TotalMonthInvoice,
                    IsActive = true,
                    IsDeleted = false
                }).ToList()
                : new List<LeadCompanyBuyingHistory>();
                List<CompanyLead> companyLeads = new List<CompanyLead>();
                var monthbuying = Convert.ToInt32(LeadCompanyBuyingHistories.Any() ? LeadCompanyBuyingHistories.Average(x => x.MonthTotalAmount) : 0);
                CompanyLead companyLead = new CompanyLead
                {
                    CompanyId = initiateLeadDetail.AnchorCompanyId,
                    CompanyCode = initiateLeadDetail.AnchorCompanyCode,
                    LeadId = leads.Id,
                    IsActive = true,
                    IsDeleted = false,
                    LeadProcessStatus = status ? 1 : 0,
                    UserUniqueCode = initiateLeadDetail.CustomerReferenceNo,
                    Email = initiateLeadDetail.Email,
                    MonthlyAvgBuying = monthbuying,
                    VintageDays = initiateLeadDetail.VintageDays,
                    LeadCompanyBuyingHistories = LeadCompanyBuyingHistories,
                    AnchorName = initiateLeadDetail.AnchorCompanyName
                };
                companyLeads.Add(companyLead);
                await _context.CompanyLead.AddRangeAsync(companyLeads);
                if (await _context.SaveChangesAsync() > 0)
                {
                    Reply.Status = true;
                    Reply.Message = "Lead generated successfully.";
                    Reply.Response = leads.Id;
                }
                else
                {
                    Reply.Status = false;
                    Reply.Message = "Error during generate lead.";
                    Reply.Response = 0;
                }
            }
            else
            {
                #region buying history update 11jul2024
                var leadCompanybuyinghistory = _context.LeadCompanyBuyingHistorys.Where(x => x.CompanyLeadId == leads.Id && x.IsActive && !x.IsDeleted).ToList();
                if (leadCompanybuyinghistory.Any())
                {
                    foreach (var item in leadCompanybuyinghistory)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        _context.Entry(item).State = EntityState.Modified;
                    }
                }

                var LeadCompanyBuyingHistories = (initiateLeadDetail.CustomerBuyingHistories != null && initiateLeadDetail.CustomerBuyingHistories.Any()) ? initiateLeadDetail.CustomerBuyingHistories.Select(x => new LeadCompanyBuyingHistory
                {
                    CompanyLeadId = 0,
                    MonthFirstBuyingDate = x.MonthFirstBuyingDate,
                    MonthTotalAmount = x.MonthTotalAmount,
                    TotalMonthInvoice = x.TotalMonthInvoice,
                    IsActive = true,
                    IsDeleted = false
                }).ToList() : new List<LeadCompanyBuyingHistory>();

                await _context.LeadCompanyBuyingHistorys.AddRangeAsync(LeadCompanyBuyingHistories);
                await _context.SaveChangesAsync();

                #endregion
                Reply.Status = false;
                Reply.Message = "Buying history update"; //"Customer record already exists";
                Reply.Response = 0;
            }
            return Reply;
        }

        public async Task<LeadMobileReply> GetLeadForMobile(LeadMobileRequest request)
        {
            LeadMobileReply leadMobileReply = new LeadMobileReply();


            Leads? leads = _context.Leads.FirstOrDefault(x => x.ProductId == request.ProductId && x.MobileNo == request.MobileNo && !x.IsDeleted);//&& x.IsActive 

            if (leads == null)
            {
                List<CompanyLead> companyLeads = new List<CompanyLead>();
                CompanyLead companyLead = new CompanyLead
                {
                    CompanyId = request.CompanyId,
                    LeadId = 0,
                    IsActive = true,
                    IsDeleted = false,
                    LeadProcessStatus = 0, //0-Initiated
                    MonthlyAvgBuying = request.MonthlyAvgBuying,
                    VintageDays = request.VintageDays,
                    CompanyCode = request.CompanyCode
                };
                companyLeads.Add(companyLead);

                List<LeadActivityMasterProgresses> leadActivityMasterProgresses = new List<LeadActivityMasterProgresses>();

                if (request.ProductActivities != null && request.ProductActivities.Any())
                {
                    foreach (var item in request.ProductActivities)
                    {
                        leadActivityMasterProgresses.Add(new LeadActivityMasterProgresses
                        {
                            ActivityMasterId = item.ActivityMasterId,
                            IsActive = (item.ActivityName == ActivityEnum.Rejected.ToString()) ? false : true,
                            IsApproved = 0,
                            IsCompleted = request.ActivityId == item.ActivityMasterId,
                            IsDeleted = false,
                            LeadMasterId = 0,
                            Sequence = item.Sequence,
                            SubActivityMasterId = item.SubActivityMasterId ?? 0,
                            ActivityMasterName = item.ActivityName,
                            SubActivityMasterName = item.SubActivityName
                        });

                    }


                }
                string LeadNo = "LeadNo";
                GRPCRequest<string> req = new GRPCRequest<string>();
                req.Request = LeadNo;
                var res = await GetCurrentNumber(req);

                leads = new Leads
                {
                    Status = LeadStatusEnum.Initiate.ToString(),
                    MobileNo = request.MobileNo,
                    ProductId = request.ProductId,
                    UserName = request.UserId,
                    LeadCode = res.Response,
                    CompanyLeads = companyLeads,
                    LeadActivityMasterProgresses = leadActivityMasterProgresses,
                    IsActive = true,
                    CibilReport = "",
                    ProductCode = request.ProductCode,
                };
                _context.Leads.Add(leads);
            }
            else
            {
                //if (string.IsNullOrEmpty(leads.Status))
                //{
                leads.IsActive = true;
                leads.Status = string.IsNullOrEmpty(leads.Status) ? LeadStatusEnum.Initiate.ToString() : leads.Status;
                _context.Entry(leads).State = EntityState.Modified;
                //}

                var LeadActivity = _context.LeadActivityMasterProgresses.FirstOrDefault(x => x.LeadMasterId == leads.Id && x.IsActive && !x.IsDeleted && x.ActivityMasterId == request.ActivityId && x.SubActivityMasterId == request.SubActivityId);
                if (LeadActivity != null)
                {
                    LeadActivity.IsCompleted = true;
                    _context.Entry(LeadActivity).State = EntityState.Modified;
                }
                if (!_context.CompanyLead.Any(x => x.LeadId == leads.Id && x.CompanyId == request.CompanyId))
                {
                    var status = _context.CompanyLead.Any(x => x.LeadId == leads.Id && x.LeadProcessStatus == 0);

                    CompanyLead companyLead = new CompanyLead
                    {
                        CompanyId = request.CompanyId,
                        LeadId = leads.Id,
                        IsActive = true,
                        IsDeleted = false,
                        LeadProcessStatus = status ? 1 : 0,
                        VintageDays = request.VintageDays,
                        MonthlyAvgBuying = request.MonthlyAvgBuying,
                        CompanyCode = request.CompanyCode
                    };
                    _context.CompanyLead.Add(companyLead);
                }




            }
            if (_context.SaveChanges() > 0)
            {
                leadMobileReply.LeadId = leads.Id;
                leadMobileReply.Status = true;
                leadMobileReply.Message = "Lead generated successfully";
                if (leadMobileReply.Status)
                {
                    await AddLeadConsentLog(leadMobileReply.LeadId, LeadTypeConsentConstants.MobileOTP, leads.UserName);
                }
            }
            else
            {
                leadMobileReply.Status = false;
                leadMobileReply.Message = "Lead not generated";
            }

            return leadMobileReply;
        }

        public async Task<GRPCReply<bool>> InitiateLeadOffer(InitiateLeadOfferRequest initiateLeadOfferRequest)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();

            GRPCRequest<UserDetailsReply> kYCUserrequest = new GRPCRequest<UserDetailsReply>();
            kYCUserrequest.Request = initiateLeadOfferRequest.kycdetail;
            kYCUserrequest.Request.LeadId = initiateLeadOfferRequest.LeadId;
            var kycAddupdateResponse = await AddUpdateLeadDetail(kYCUserrequest);
            if (!kycAddupdateResponse.Status)
            {
                reply.Status = false;
                reply.Message = "Something went wrong in kyc Add update :" + kycAddupdateResponse.Message;
                return reply;
            }


            //if (initiateLeadOfferRequest.loanConfiguration != null)
            //{
            //    GRPCRequest<AddLoanConfigurationDc> _loanConfiguration = new GRPCRequest<AddLoanConfigurationDc>();
            //    _loanConfiguration.Request = initiateLeadOfferRequest.loanConfiguration;
            //    var AddLoanConfigurationResponse = await AddLoanConfiguration(_loanConfiguration);
            //    if (!AddLoanConfigurationResponse.Status)
            //    {
            //        reply.Status = false;
            //        reply.Message = "Something went wrong in Loan Configuration :" + AddLoanConfigurationResponse.Message;
            //        return reply;
            //    }
            //}
            if (initiateLeadOfferRequest.LeadNBFCSubActivityRequest != null && initiateLeadOfferRequest.LeadNBFCSubActivityRequest.Any())
            {
                GRPCRequest<List<LeadNBFCSubActivityRequestDc>> _leadNBFCSubActivityRequest = new GRPCRequest<List<LeadNBFCSubActivityRequestDc>>();
                _leadNBFCSubActivityRequest.Request = initiateLeadOfferRequest.LeadNBFCSubActivityRequest;
                var leadNBFCSubActivityRequest = await InsertLeadNBFCApi(_leadNBFCSubActivityRequest);
                if (!leadNBFCSubActivityRequest.Status)
                {
                    reply.Status = false;
                    reply.Message = "Something went wrong in Lead NBFC Sub Activity Insert :" + leadNBFCSubActivityRequest.Message;
                    return reply;
                }
            }
            bool IsGenerateOfferForLead = false;
            List<long> ids = new List<long>();
            var leadoffers = await _context.LeadOffers.Where(x => x.LeadId == initiateLeadOfferRequest.LeadId && x.IsActive && !x.IsDeleted).ToListAsync();
            if (leadoffers != null && leadoffers.Any())
            {

                foreach (var i in initiateLeadOfferRequest.Companys)
                {
                    bool ispresent = leadoffers.Any(x => x.NBFCCompanyId == i.NbfcId);
                    if (!ispresent)
                    {
                        ids.Add(i.NbfcId);
                    }
                    IsGenerateOfferForLead = true;
                }
            }
            else
            {
                foreach (var i in initiateLeadOfferRequest.Companys)
                {
                    ids.Add(i.NbfcId);
                    IsGenerateOfferForLead = true;

                }
            }

            if (ids.Count > 0)
            {
                var leadOffers = ids.Select(x => new LeadOffers
                {
                    LeadId = initiateLeadOfferRequest.LeadId,
                    NBFCCompanyId = x,
                    CompanyIdentificationCode = initiateLeadOfferRequest.Companys.FirstOrDefault(z => z.NbfcId == x).CompanyIdentificationCode,
                    IsActive = true,
                    IsDeleted = false,
                    Status = "Initiated"
                });
                await _context.AddRangeAsync(leadOffers);
                _context.SaveChanges();

                //------------------S : Make log---------------------
                #region Make History
                string doctypeSendToLos = "SendToLos";
                
                var result = await _leadHistoryManager.GetLeadHistroy(initiateLeadOfferRequest.LeadId, doctypeSendToLos);
                LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                {
                    LeadId = initiateLeadOfferRequest.LeadId,
                    UserID = result.UserId,
                    UserName = "",
                    EventName = doctypeSendToLos,//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                    Narretion = result.Narretion,
                    NarretionHTML = result.NarretionHTML,
                    CreatedTimeStamp = result.CreatedTimeStamp
                };
                await _massTransitService.Publish(histroyEvent);
                #endregion
                //------------------E : Make log---------------------

            }

            if (IsGenerateOfferForLead)
            {
                var res = await _nBFCSchedular.GenerateOfferForLead(initiateLeadOfferRequest.LeadId);
                reply.Status = res.Status;//true;
                reply.Message = res.Message;// "Lead Offer initiated successfully.";
            }
            else
            {
                reply.Status = false;
                reply.Message = "Lead Offer not initiated.";
            }
            return reply;
        }
        public async Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductId(GRPCRequest<long> LeadId)
        {
            GRPCReply<LeadAnchorProductRequest> reply = new GRPCReply<LeadAnchorProductRequest>();
            var lead = (await _context.Leads.Where(x => x.Id == LeadId.Request).Include(x => x.CompanyLeads).FirstOrDefaultAsync());
            if (lead != null)
            {
                LeadAnchorProductRequest leadAnchorProductRequest = new LeadAnchorProductRequest();
                leadAnchorProductRequest.ProductId = lead.ProductId;
                leadAnchorProductRequest.UserId = lead.UserName;
                leadAnchorProductRequest.AnchorCompanyId = lead.CompanyLeads.Any() ? lead.CompanyLeads.FirstOrDefault(x => x.LeadProcessStatus == 2)?.CompanyId : null;
                leadAnchorProductRequest.ProductCode = lead.ProductCode;
                reply.Response = leadAnchorProductRequest;
                reply.Status = true;
            }
            return reply;
        }

        public async Task<GRPCReply<LeadAnchorProductRequest>> GetLeadProductIdByRole(GRPCRequest<GetGenerateOfferByFinanceRequestDc> req)
        {
            GRPCReply<LeadAnchorProductRequest> reply = new GRPCReply<LeadAnchorProductRequest>();
            var lead = (await _context.Leads.Where(x => x.Id == req.Request.LeadId).Include(x => x.CompanyLeads).FirstOrDefaultAsync());
            if (lead != null)
            {
                var leadOffer = await _context.LeadOffers.Where(x => x.IsActive && !x.IsDeleted && x.LeadId == lead.Id && x.CompanyIdentificationCode != null && x.NBFCCompanyId == req.Request.NbfcCompanyId).FirstOrDefaultAsync();
                var RejectionMsg = await _context.BusinessLoanNBFCUpdate.Where(x => x.IsActive && !x.IsDeleted && x.LeadId == lead.Id && x.CompanyIdentificationCode != null && x.NBFCCompanyId == req.Request.NbfcCompanyId).Select(x => x.NBFCRemark).FirstOrDefaultAsync();
                LeadAnchorProductRequest leadAnchorProductRequest = new LeadAnchorProductRequest();
                leadAnchorProductRequest.ProductId = lead.ProductId;
                leadAnchorProductRequest.UserId = lead.UserName;
                leadAnchorProductRequest.AnchorCompanyId = lead.CompanyLeads.Any() ? lead.CompanyLeads.FirstOrDefault(x => x.LeadProcessStatus == 2)?.CompanyId : null;
                leadAnchorProductRequest.ProductCode = lead.ProductCode;
                if (leadOffer != null)
                {
                    leadAnchorProductRequest.NBFCCompanyId = leadOffer.NBFCCompanyId;
                    leadAnchorProductRequest.LastModifyBy = leadOffer.LastModifiedBy ?? "";
                    leadAnchorProductRequest.Created = leadOffer.Created;
                }
                if (RejectionMsg != null)
                {
                    leadAnchorProductRequest.RejectionReason = RejectionMsg;
                }
                reply.Response = leadAnchorProductRequest;
                reply.Status = true;
            }
            return reply;
        }

        public async Task<GRPCReply<LeadDetailReponse>> GetLeadByProductIdAndUserId(GRPCRequest<LeadDetailRequest> request)
        {
            GRPCReply<LeadDetailReponse> reply = new GRPCReply<LeadDetailReponse>();
            var lead = (await _context.Leads.Where(x => x.ProductId == request.Request.ProductId && x.UserName == request.Request.UserId).Include(x => x.CompanyLeads).FirstOrDefaultAsync());
            if (lead != null)
            {
                reply.Response = new LeadDetailReponse
                {
                    LeadId = lead.Id,
                    MobileNo = lead.MobileNo
                };
                reply.Status = true;
            }
            return reply;
        }



        public async Task<GRPCReply<List<LeadOfferReply>>> GetLeadNBFCId()
        {
            GRPCReply<List<LeadOfferReply>> reply = new GRPCReply<List<LeadOfferReply>>();
            var leadoffer = await _context.LeadOffers.Where(x => x.IsActive && x.Status == "Initiated" && !x.IsDeleted).ToListAsync();
            if (leadoffer != null && leadoffer.Any())
            {
                var LeadIds = leadoffer.Select(x => x.LeadId).Distinct().ToList();
                var Leadmasters = await _context.Leads.Where(x => LeadIds.Contains(x.Id) && x.IsActive && !x.IsDeleted).ToListAsync();
                var leadCompleteDetails = await _context.CompanyLead.Where(x => LeadIds.Contains(x.LeadId) && x.IsActive && x.LeadProcessStatus == 2).ToListAsync();
                var res = (leadoffer.Select(x => new LeadOfferReply
                {
                    LeadId = x.LeadId,
                    Id = x.Id,
                    NBFCCompanyId = x.NBFCCompanyId,
                    CibilScore = Convert.ToInt32(Leadmasters.FirstOrDefault(y => y.Id == x.LeadId).CreditScore ?? 0),
                    AvgMonthlyBuying = leadCompleteDetails.Any(y => y.LeadId == x.LeadId) ? (leadCompleteDetails.FirstOrDefault(y => y.LeadId == x.LeadId).MonthlyAvgBuying ?? 0) : 0,
                    CustomerType = "Retailer",
                    VintageDays = leadCompleteDetails.Any(y => y.LeadId == x.LeadId) ? (leadCompleteDetails.FirstOrDefault(y => y.LeadId == x.LeadId).VintageDays ?? 0) : 0
                }).ToList());
                if (res != null)
                {
                    reply.Response = res;
                    reply.Status = true;
                }
            }
            return reply;
        }
        public async Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<string> entityname)
        {
            GRPCReply<string> res = new GRPCReply<string>();

            var entity_name = new SqlParameter("entityname", entityname.Request);
            var result = _context.Database.SqlQueryRaw<string>("exec spGetCurrentNumber @entityname", entity_name).AsEnumerable().FirstOrDefault();
            res.Response = result;
            return res;

        }

        public async Task<GRPCReply<bool>> UpdateleadOffer(List<NBFCSelfOfferReply> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();

            List<long> LeadIds = request.Select(x => x.LeadId).ToList();
            List<long> CompanyIds = request.Select(x => x.CompanyId).ToList();
            var Leads = await _context.Leads.Where(x => LeadIds.Contains(x.Id)).ToListAsync();
            var leadAllOffers = await _context.LeadOffers.Where(x => LeadIds.Contains(x.LeadId) && x.IsActive && !x.IsDeleted).ToListAsync();
            var leadOffers = leadAllOffers.Where(x => CompanyIds.Contains(x.NBFCCompanyId)).ToList();
            foreach (var Leaddetaits in request.GroupBy(x => x.LeadId))
            {
                foreach (var offer in Leaddetaits)
                {
                    var leadoffer = leadOffers.FirstOrDefault(x => x.LeadId == offer.LeadId && x.NBFCCompanyId == offer.CompanyId);
                    if (leadoffer != null)
                    {
                        leadoffer.Status = "OfferGenerated";
                        leadoffer.CreditLimit = offer.OfferAmount;
                        _context.Entry(leadoffer).State = EntityState.Modified;
                    }
                }
                var lead = Leads.FirstOrDefault(x => x.Id == Leaddetaits.Key);

                if (lead != null && leadAllOffers.All(x => x.LeadId == Leaddetaits.Key && x.Status == "OfferGenerated"))
                {
                    var offerCompanyId = Leaddetaits.FirstOrDefault(x => x.LeadId == Leaddetaits.Key && x.OfferAmount == request.Max(x => x.OfferAmount)).CompanyId;

                    lead.CreditLimit = request.Max(x => x.OfferAmount);
                    lead.OfferCompanyId = offerCompanyId;
                    _context.Entry(lead).State = EntityState.Modified;

                    var leadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == lead.Id
                                        && !x.IsCompleted && x.IsActive && !x.IsDeleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();

                    if (leadActivity != null)
                    {
                        leadActivity.IsCompleted = true;
                        leadActivity.IsApproved = 1;
                        _context.Entry(leadActivity).State = EntityState.Modified;
                    }
                }
            }
            reply.Status = _context.SaveChanges() > 0;

            return reply;
        }

        public async Task<GRPCReply<LeadCompanyConfigProdReply>> GetLeadAllCompanyAsync(GRPCRequest<LeadCompanyConfigProdRequest> request)
        {
            GRPCReply<LeadCompanyConfigProdReply> reply = new GRPCReply<LeadCompanyConfigProdReply>();
            var lead = await _context.Leads.Where(x => x.Id == request.Request.LeadId && x.OfferCompanyId.HasValue).Include(x => x.CompanyLeads).FirstOrDefaultAsync();
            if (lead != null)
            {

                var data = new LeadCompanyConfigProdReply
                {
                    NBFCCompanyId = lead.OfferCompanyId.Value,
                    AnchorCompanyId = (request.Request.AnchorCompanyId.HasValue && request.Request.AnchorCompanyId.Value > 0) ? request.Request.AnchorCompanyId.Value : (lead.CompanyLeads.Any(x => x.LeadProcessStatus == 2) ? lead.CompanyLeads.FirstOrDefault(x => x.LeadProcessStatus == 2).CompanyId : 0),
                    ProductId = lead.ProductId
                };
                reply.Response = data;
                reply.Status = true;
            }
            return reply;
        }

        public async Task<GRPCReply<LeadAcceptOffer>> GetLeadOfferCompany(GRPCRequest<long> request)
        {
            GRPCReply<LeadAcceptOffer> reply = new GRPCReply<LeadAcceptOffer>();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request && x.OfferCompanyId.HasValue);
            if (lead != null)
            {
                var companyLead = await _context.CompanyLead.FirstOrDefaultAsync(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted);
                reply.Status = true;
                reply.Response = new LeadAcceptOffer
                {
                    OfferCompanyId = lead.OfferCompanyId.Value,
                    ProductId = lead.ProductId,
                    AnchorCompanyId = companyLead != null ? companyLead.CompanyId : 0
                };
            }

            return reply;
        }

        public async Task<GRPCReply<bool>> AcceptOfferAndAddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities)
        {
            GRPCReply<bool> rely = new GRPCReply<bool>();
            var leadid = gRPCLeadProductActivities.FirstOrDefault().LeadId;
            var lead = await _context.Leads.Where(x => x.Id == leadid).FirstOrDefaultAsync();
            var leadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadid
                                    && (x.ActivityMasterName == ActivityConstants.ShowOffer || x.ActivityMasterName == ActivityConstants.ArthmateShowOffer) && !x.IsCompleted && x.IsActive && !x.IsDeleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();


            var maxSequence = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadid && x.IsActive && !x.IsDeleted).MaxAsync(x => x.Sequence);
            if (leadActivity != null)
            {
                leadActivity.IsCompleted = true;
                leadActivity.IsApproved = 1;
                _context.Entry(leadActivity).State = EntityState.Modified;
            }

            var subactivityList = _context.LeadNBFCSubActivitys.Where(x => x.LeadId == leadid && x.ActivityName == ActivityConstants.Agreement && x.IsActive && !x.IsDeleted).Include(y => y.LeadNBFCApis).ToList();
            if (subactivityList != null && subactivityList.Any())
            {
                subactivityList = subactivityList.Where(x => x.NBFCCompanyId != lead.OfferCompanyId).ToList();

                if (subactivityList != null && subactivityList.Any())
                {
                    foreach (var item in subactivityList)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        _context.Entry(item).State = EntityState.Modified;

                        if (item.LeadNBFCApis != null && item.LeadNBFCApis.Any())
                        {
                            foreach (var api in item.LeadNBFCApis)
                            {
                                api.IsActive = false;
                                api.IsDeleted = true;
                                _context.Entry(api).State = EntityState.Modified;

                            }
                        }
                    }
                }
            }

            List<LeadActivityMasterProgresses> leadActivityMasterProgresses = new List<LeadActivityMasterProgresses>();



            // to avoid duplicacy
            List<long> activityIds = gRPCLeadProductActivities.Select(x => x.ActivityMasterId).Distinct().ToList();
            List<long> ExistsleadActivityId = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadid
                                      && activityIds.Contains(x.ActivityMasterId) && x.IsActive && !x.IsDeleted).Select(x => x.ActivityMasterId).Distinct().ToListAsync();




            if (gRPCLeadProductActivities != null && gRPCLeadProductActivities.Any())
            {
                foreach (var item in gRPCLeadProductActivities.Where(x => !ExistsleadActivityId.Contains(x.ActivityMasterId)))
                {
                    maxSequence++;
                    leadActivityMasterProgresses.Add(new LeadActivityMasterProgresses
                    {
                        ActivityMasterId = item.ActivityMasterId,
                        IsActive = true,
                        IsApproved = 0,
                        IsCompleted = false,
                        IsDeleted = false,
                        LeadMasterId = leadid,
                        Sequence = maxSequence,
                        SubActivityMasterId = item.SubActivityMasterId.HasValue ? item.SubActivityMasterId : 0,
                        ActivityMasterName = item.ActivityName,
                        SubActivityMasterName = item.SubActivityName
                    });
                }
                if (leadActivityMasterProgresses != null && leadActivityMasterProgresses.Any())
                {
                    await _context.LeadActivityMasterProgresses.AddRangeAsync(leadActivityMasterProgresses);
                }
            }

            if (await _context.SaveChangesAsync() > 0)
            {
                //Code For BlackSoil

                try
                {
                    await _nBFCSchedular.GenerateAgreement(leadid);

                }
                catch (Exception es)
                {
                }
                rely.Status = true;
                rely.Message = "Lead next NBFC company Activity added successfully";
            }
            return rely;
        }

        public async Task<GRPCReply<bool>> AddNBFCActivity(List<GRPCLeadProductActivity> gRPCLeadProductActivities)
        {
            GRPCReply<bool> rely = new GRPCReply<bool>();
            var leadid = gRPCLeadProductActivities.FirstOrDefault().LeadId;

            var maxSequence = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadid && x.IsActive && !x.IsDeleted).MaxAsync(x => x.Sequence);

            List<LeadActivityMasterProgresses> leadActivityMasterProgresses = new List<LeadActivityMasterProgresses>();

            if (gRPCLeadProductActivities != null && gRPCLeadProductActivities.Any())
            {
                maxSequence += 1;//Gap For Rejected
                foreach (var item in gRPCLeadProductActivities)
                {
                    maxSequence++;
                    leadActivityMasterProgresses.Add(new LeadActivityMasterProgresses
                    {
                        ActivityMasterId = item.ActivityMasterId,
                        IsActive = true,
                        IsApproved = 0,
                        IsCompleted = false,
                        IsDeleted = false,
                        LeadMasterId = leadid,
                        Sequence = maxSequence,
                        SubActivityMasterId = item.SubActivityMasterId.HasValue ? item.SubActivityMasterId : 0,
                        ActivityMasterName = item.ActivityName,
                        SubActivityMasterName = item.SubActivityName
                    });
                }
                await _context.LeadActivityMasterProgresses.AddRangeAsync(leadActivityMasterProgresses);
            }

            if (await _context.SaveChangesAsync() > 0)
            {
                rely.Status = true;
                rely.Message = "Lead next NBFC company Activity added successfully";
            }
            return rely;
        }
        public async Task<GRPCReply<FileUploadRequest>> CustomerNBFCAgreement(GRPCRequest<NBFCAgreement> request)
        {
            GenerateNBFCAgreementHelper helper = new GenerateNBFCAgreementHelper(_context);
            var res = await helper.GenerateAgreement(request);
            return res;
        }
        public async Task<GRPCReply<bool>> AddLeadAgreement(GRPCRequest<LeadAgreementDc> request)
        {
            GRPCReply<bool> res = new GRPCReply<bool>();

            var leads = await _context.Leads.Where(x => x.Id == request.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
            if (leads != null)
            {
                LeadAgreement _LeadAgreement = new LeadAgreement
                {
                    LeadId = request.Request.LeadId,
                    DocumentId = request.Request.DocumentId,
                    ExpiredOn = request.Request.ExpiredOn,
                    DocUnSignUrl = request.Request.DocUrl,
                    Created = currentDateTime,
                    IsActive = true,
                    IsDeleted = false,
                    DocSignedUrl = "",
                    StartedOn = currentDateTime,
                };
                await _context.leadAgreements.AddAsync(_LeadAgreement);

                var leadActivities = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leads.Id && x.IsActive && !x.IsDeleted && !x.IsCompleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
                if (leadActivities != null)
                {
                    leadActivities.IsCompleted = true;
                    leadActivities.IsApproved = 1;
                    _context.Entry(leadActivities).State = EntityState.Modified;
                }
                leads.AgreementDate = currentDateTime;
                leads.IsAgreementAccept = true;
                _context.Entry(leads).State = EntityState.Modified;
            }

            _context.SaveChanges();

            res.Status = true;
            return res;

        }
        public async Task<GRPCReply<LeadCreditLimit>> GetCreditLimitByLeadId(GRPCRequest<long> request)
        {
            GRPCReply<LeadCreditLimit> res = new GRPCReply<LeadCreditLimit>();
            LeadCreditLimit lead = new LeadCreditLimit();
            double? CreditLimit = 0;
            var data = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request);
            lead.CreditLimit = Convert.ToDouble(data.CreditLimit);
            lead.UserId = data.UserName;
            lead.LeadCode = data.LeadCode;
            lead.OfferCompanyId = data.OfferCompanyId;
            lead.ProductCode = data.ProductCode;
            lead.InterestRate = data.InterestRate;
            lead.AnchorCompanyId = (await _context.CompanyLead.FirstOrDefaultAsync(x => x.LeadId == request.Request && x.IsActive && x.LeadProcessStatus == 2))?.CompanyId;
            var _companyLead = await _context.CompanyLead.Where(x => x.LeadId == request.Request).FirstOrDefaultAsync();
            if (_companyLead != null)
            {
                lead.VintageDays = _companyLead.VintageDays;
                lead.AnchorCompanyName = _companyLead.AnchorName;
                lead.BusinessVintageDays = _companyLead.BusinessVintageDays ?? 0;
            }
            var payoutPercentages = await _context.DSAPayouts.Where(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted).Select(x =>
                new SalesAgentCommissionList { PayoutPercentage = x.PayoutPercenatge, MaxAmount = x.MaxAmount, MinAmount = x.MinAmount, ProductId = x.ProductId }).ToListAsync();
            lead.SalesAgentCommissions = payoutPercentages != null && payoutPercentages.Any() ? payoutPercentages : new List<SalesAgentCommissionList>();

            res.Response = lead;
            res.Status = true;
            res.Message = "Success";
            return res;
        }

        public async Task<GRPCReply<List<BankStatementDetailDc>>> GetBankStatementDetailByLeadId(GRPCRequest<BankStatementRequestDc> request)
        {
            GRPCReply<List<BankStatementDetailDc>> res = new GRPCReply<List<BankStatementDetailDc>>();
            List<BankStatementDetailDc> bankStatementDetails = new List<BankStatementDetailDc>();
            if (request.Request.UserId != null && request.Request.ProductCode != null)
            {
                var LeadId = await _context.Leads.Where(x => x.UserName == request.Request.UserId && x.ProductCode == request.Request.ProductCode && x.IsActive == true && x.IsDeleted == false).Select(y => y.Id).FirstOrDefaultAsync();
                var bankData = await _context.LeadBankDetails.Where(x => x.LeadId == LeadId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(y => y.Id).ToListAsync();

                if (bankData != null && bankData.Count > 0)
                {
                    foreach (var bank in bankData)
                    {

                        BankStatementDetailDc bankStatementDetailDc = new BankStatementDetailDc();
                        if (!string.IsNullOrEmpty(bank.BankName))
                        {
                            bankStatementDetailDc.Bankname = bank.BankName;
                        }
                        if (!string.IsNullOrEmpty(bank.IFSCCode))
                        {
                            bankStatementDetailDc.IFSCCode = bank.IFSCCode;
                        }
                        if (!string.IsNullOrEmpty(bank.AccountNumber))
                        {
                            bankStatementDetailDc.AccountNumber = bank.AccountNumber;
                        }
                        if (!string.IsNullOrEmpty(bank.AccountType))
                        {
                            bankStatementDetailDc.AccountType = bank.AccountType;
                        }
                        if (!string.IsNullOrEmpty(bank.AccountHolderName))
                        {
                            bankStatementDetailDc.Accountholdername = bank.AccountHolderName;
                        }
                        if (!string.IsNullOrEmpty(bank.Type))
                        {
                            bankStatementDetailDc.Type = bank.Type;
                        }
                        bankStatementDetailDc.BankDetailId = bank.Id;
                        bankStatementDetails.Add(bankStatementDetailDc);
                    }
                }
                if (bankStatementDetails != null)
                {
                    res.Status = true;
                    res.Response = bankStatementDetails;
                }
            }
            return res;
        }

        public async Task<GRPCReply<CreditBureauListDc>> GetCreditBureauDetails(GRPCRequest<string> request)
        {
            GRPCReply<CreditBureauListDc> res = new GRPCReply<CreditBureauListDc>();
            CreditBureauListDc creditBureauListDc = new CreditBureauListDc();
            if (request.Request != null)
            {
                var cibilReportPath = await _context.Leads.Where(x => x.UserName == request.Request && x.IsActive == true && x.IsDeleted == false).Select(y => new { y.CibilReport }).FirstOrDefaultAsync();

                string path = cibilReportPath.CibilReport;
                if (path != null)
                {
                    try
                    {
                        if (path != null && !string.IsNullOrEmpty(path))
                        {
                            XmlDocument doc = new XmlDocument();
                            var xmldata = ScaleUP.Global.Infrastructure.Helper.FileSaverHelper.GetStreamFromUrl(path);
                            doc.LoadXml(xmldata);

                            if (creditBureauListDc != null)
                            {
                                res.Response = creditBureauListDc;
                                res.Status = true;
                                res.Message = "";
                                string jsonText = JsonConvert.SerializeXmlNode(doc);
                                CreditBureauResponseDc creditBureauReportDc = new CreditBureauResponseDc();
                                creditBureauReportDc = JsonConvert.DeserializeObject<CreditBureauResponseDc>(jsonText);
                                creditBureauListDc.CreditScoreReponse = new CreditBureauResponseDc();
                                creditBureauListDc.CreditScoreReponse = creditBureauReportDc;
                                creditBureauListDc.cibiljson = jsonText;
                                res.Response = creditBureauListDc;
                                res.Status = true;
                                res.Message = "";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        res.Status = false;
                        res.Message = "";
                        return res;
                    }

                }
            }
            return res;
        }

        public async Task<GRPCReply<long>> GetLeadIdByMobile(GRPCRequest<LeadActivitySequenceRequest> request)
        {
            GRPCReply<long> res = new GRPCReply<long>();

            long? leadId = 0;
            leadId = (await _context.Leads.FirstOrDefaultAsync(x => x.MobileNo == request.Request.MobileNo && x.ProductId == request.Request.ProductId))?.Id;
            res.Status = true;
            res.Response = leadId.HasValue ? leadId.Value : 0;

            return res;
        }

        public async Task<GRPCReply<LeadResponse>> GetLeadInfoById(GRPCRequest<long> request)
        {
            GRPCReply<LeadResponse> res = new GRPCReply<LeadResponse>();
            var PersonalDetail = _context.PersonalDetails.Where(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted).FirstOrDefault();
            string ShopName = _context.BusinessDetails.Where(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted).Select(x => x.BusinessName).FirstOrDefault();
            var lead = await _context.Leads.Where(x => x.Id == request.Request).Include(x => x.CompanyLeads).Select(x => new LeadResponse
            {
                LeadId = x.Id,
                CreditLimit = x.CreditLimit,
                LeadCode = x.LeadCode,
                MobileNo = x.MobileNo,
                OfferCompanyId = x.OfferCompanyId,
                ProductId = x.ProductId,
                UserName = x.UserName,
                AgreementDate = x.AgreementDate,
                ApplicationDate = x.Created,
                CustomerImage = PersonalDetail != null ? PersonalDetail.SelfieImageUrl : "",
                ShopName = ShopName,
                CustomerCurrentCityName = PersonalDetail != null ? PersonalDetail.PermanentCityName : "", //CurrentCityName
                CustomerName = PersonalDetail != null ? PersonalDetail.PanNameOnCard : "",
                ProductCode = x.ProductCode,
                Status = x.Status,
                LeadCompanies = x.CompanyLeads.Any(y => y.IsActive) ? x.CompanyLeads.Where(y => y.IsActive).Select(y => new LeadCompany
                {
                    CompanyId = y.CompanyId,
                    LeadProcessStatus = y.LeadProcessStatus,
                    UserUniqueCode = y.UserUniqueCode

                }).ToList() : null,
            }).FirstOrDefaultAsync();

            var pfcollection = await _context.BlackSoilPFCollections.Where(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted).Select(x => new BlackSoilPfCollectionDc
            {
                processing_fee = x.processing_fee,
                processing_fee_status = x.status,
                processing_fee_tax = x.processing_fee_tax,
                total_processing_fee = x.total_processing_fee

            }).FirstOrDefaultAsync();
            if (pfcollection != null)
            {
                lead.BlackSoilPfCollection = pfcollection;
            }
            res.Status = true;
            res.Response = lead;
            return res;

        }

        public async Task<GRPCReply<BlackSoilUpdateResponse>> GetBlackSoilApplicationDetails(GRPCRequest<long> request)
        {
            var result = new BlackSoilUpdateResponse();

            GRPCReply<BlackSoilUpdateResponse> res = new GRPCReply<BlackSoilUpdateResponse>();
            var BlackSoilUpdates = _context.BlackSoilUpdates.Where(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (BlackSoilUpdates != null)
            {
                var nbfcresult = await _nBFCSchedular.BlackSoilCommonApplicationDetail(BlackSoilUpdates.LeadId);
                if (nbfcresult != null && !nbfcresult.IsSuccess)
                {
                    res.Message = "Currently Lead is not line_activated";
                    res.Status = false;
                    return res;

                }
                result.ApplicationCode = BlackSoilUpdates.ApplicationCode;
                result.BusinessCode = BlackSoilUpdates.BussinessCode;
                result.ApplicationId = BlackSoilUpdates.ApplicationId.ToString();
                result.BusinessId = BlackSoilUpdates.BusinessId.ToString();
                res.Status = true;

            }

            res.Response = result;
            return res;

        }




        public async Task<GRPCReply<long>> UpdateCurrentActivity(GRPCRequest<long> request)
        {
            GRPCReply<long> res = new GRPCReply<long>();

            res.Status = false;
            res.Message = "Failed";
            res.Response = 0;

            //var leads = _context.Leads.Where(x => x.Id == request.Request && x.IsActive && !x.IsDeleted).FirstOrDefault();
            //if (leads != null)
            {
                var leadActivities = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.Request && x.IsActive && !x.IsDeleted && !x.IsCompleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
                if (leadActivities != null)
                {
                    leadActivities.IsCompleted = true;
                    if (leadActivities.ActivityMasterName == ActivityConstants.Disbursement.ToString())
                    {
                        leadActivities.IsApproved = 1;
                    }
                    _context.Entry(leadActivities).State = EntityState.Modified;
                    _context.SaveChanges();

                    res.Status = true;
                    res.Message = "Success";
                    res.Response = leadActivities.Id;
                }
            }
            return res;
        }

        public async Task<GRPCReply<AgreementDetailDc>> GetAgreementDetail(GRPCRequest<string> request)
        {
            GRPCReply<AgreementDetailDc> res = new GRPCReply<AgreementDetailDc>();
            AgreementDetailDc agreementDetailDc = new AgreementDetailDc();
            if (request.Request != null)
            {
                var LeadId = await _context.Leads.Where(x => x.UserName == request.Request && x.IsActive == true && x.IsDeleted == false).Select(y => y.Id).FirstOrDefaultAsync();

                if (LeadId != null)
                {
                    var result = await (from l in _context.Leads
                                        join la in _context.leadAgreements on l.Id equals la.LeadId
                                        where l.Id == LeadId &&
                                              l.IsAgreementAccept == true &&
                                              l.IsActive &&
                                              !l.IsDeleted &&
                                              la.IsActive &&
                                              !la.IsDeleted
                                        orderby la.Created descending
                                        select la.DocSignedUrl).Take(1).FirstOrDefaultAsync();

                    if (result != null)
                    {
                        agreementDetailDc.AgreementUrl = result;
                        res.Response = agreementDetailDc;
                        res.Status = true;
                        res.Message = "";
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "";
                    }
                }
            }
            return res;
        }


        public async Task<GRPCReply<bool>> AddUpdateLeadDetail(GRPCRequest<UserDetailsReply> request)
        {
            GRPCReply<bool> result = new GRPCReply<bool>();
            //long LeadId = await _context.Leads.FirstOrDefaultAsync(x => x.UserName == request.Request.UserId).Select(x => x.Id);
            long LeadId = request.Request.LeadId ?? 0;
            if (LeadId > 0)
            {
                try
                {
                    string[] name = request.Request.panDetail.NameOnCard.ToString().Trim().Split(new char[] { ' ' }, 3);
                    string firstname = "", lastname = "", middlename = "";

                    if (name[0] != null)

                    {
                        firstname = name[0];
                    }
                    if (name.Length > 1 && name[1] != null)
                    {
                        if (name.Length == 2)
                        {
                            lastname = name[1];
                        }
                        else
                        {
                            middlename = name[1];
                        }
                    }
                    if (name.Length > 2 && name[2] != null)
                    {
                        lastname = name[2];
                    }

                    string fatherLastName = "";
                    if (!string.IsNullOrEmpty(request.Request.panDetail.FatherName) && request.Request.panDetail.FatherName.Trim().Split(" ").ToList().Count > 1)
                    {
                        fatherLastName = request.Request.panDetail.FatherName.Trim().Split(" ").ToList()[request.Request.panDetail.FatherName.Trim().Split(" ").ToList().Count - 1];
                    }


                    var PersonalDetail = await _context.PersonalDetails.Where(x => x.LeadId == LeadId && x.IsDeleted == false && x.IsActive == true).FirstOrDefaultAsync();
                    if (PersonalDetail != null)
                    {
                        PersonalDetail.FirstName = firstname; //request.Request.panDetail != null ? request.Request.panDetail.NameOnCard : "";
                        PersonalDetail.MiddleName = middlename;//request.Request.PersonalDetail.MiddleName;
                        PersonalDetail.LastName = lastname; //request.Request.PersonalDetail.LastName != null ? request.Request.PersonalDetail.LastName : "";
                        PersonalDetail.AadhaarMaskNO = request.Request.aadharDetail.UniqueId;
                        PersonalDetail.PanMaskNO = request.Request.panDetail.UniqueId;
                        PersonalDetail.AlternatePhoneNo = request.Request.PersonalDetail.AlternatePhoneNo;
                        PersonalDetail.DOB = request.Request.panDetail.DOB;
                        PersonalDetail.FatherName = request.Request.panDetail.FatherName;
                        PersonalDetail.FatherLastName = fatherLastName;
                        PersonalDetail.CurrentAddressLineOne = request.Request.PersonalDetail.CurrentAddress.AddressLineOne;
                        PersonalDetail.CurrentAddressLineTwo = request.Request.PersonalDetail.CurrentAddress.AddressLineTwo;
                        PersonalDetail.CurrentAddressLineThree = request.Request.PersonalDetail.CurrentAddress.AddressLineThree;
                        PersonalDetail.CurrentCityName = request.Request.PersonalDetail.CurrentAddress.CityName;
                        PersonalDetail.CurrentStateName = request.Request.PersonalDetail.CurrentAddress.StateName;
                        PersonalDetail.CurrentZipCode = request.Request.PersonalDetail.CurrentAddress.ZipCode;
                        PersonalDetail.CurrentCountryName = request.Request.PersonalDetail.CurrentAddress.CountryName;
                        PersonalDetail.PermanentAddressLineOne = request.Request.PersonalDetail.PermanentAddress.AddressLineOne;
                        PersonalDetail.PermanentAddressLineTwo = request.Request.PersonalDetail.PermanentAddress.AddressLineTwo;
                        PersonalDetail.PermanentAddressLineThree = request.Request.PersonalDetail.PermanentAddress.AddressLineThree;
                        PersonalDetail.PermanentCityName = request.Request.PersonalDetail.PermanentAddress.CityName;
                        PersonalDetail.PermanentStateName = request.Request.PersonalDetail.PermanentAddress.StateName;
                        PersonalDetail.PermanentZipCode = request.Request.PersonalDetail.PermanentAddress.ZipCode;
                        PersonalDetail.PermanentCountryName = request.Request.PersonalDetail.PermanentAddress.CountryName;
                        PersonalDetail.EmailId = request.Request.PersonalDetail.EmailId;
                        PersonalDetail.Gender = request.Request.PersonalDetail.Gender;
                        PersonalDetail.MobileNo = request.Request.PersonalDetail.MobileNo;
                        PersonalDetail.AadharBackImage = request.Request.aadharDetail.BackImageUrl;
                        PersonalDetail.AadharFrontImage = request.Request.aadharDetail.FrontImageUrl;
                        PersonalDetail.PanFrontImage = request.Request.panDetail.FrontImageUrl;
                        PersonalDetail.PanNameOnCard = request.Request.panDetail.NameOnCard;
                        PersonalDetail.SelfieImageUrl = request.Request.SelfieDetail?.FrontImageUrl;
                        PersonalDetail.Marital = request.Request.PersonalDetail.Marital;
                        _context.Entry(PersonalDetail).State = EntityState.Modified;
                    }
                    else
                    {


                        var AddPersonalDetail = new PersonalDetail
                        {
                            LeadId = LeadId,
                            FirstName = firstname,//request.Request.panDetail.NameOnCard,
                            MiddleName = middlename, //request.Request.PersonalDetail.MiddleName,
                            LastName = lastname,//request.Request.PersonalDetail.LastName,
                            AadhaarMaskNO = request.Request.aadharDetail.UniqueId,
                            PanMaskNO = request.Request.panDetail.UniqueId,
                            AlternatePhoneNo = request.Request.PersonalDetail.AlternatePhoneNo,
                            DOB = request.Request.panDetail.DOB,
                            FatherName = request.Request.panDetail.FatherName,
                            FatherLastName = fatherLastName,
                            CurrentAddressLineOne = request.Request.PersonalDetail.CurrentAddress.AddressLineOne,
                            CurrentAddressLineTwo = request.Request.PersonalDetail.CurrentAddress.AddressLineTwo,
                            CurrentAddressLineThree = request.Request.PersonalDetail.CurrentAddress.AddressLineThree,
                            CurrentCityName = request.Request.PersonalDetail.CurrentAddress.CityName,
                            CurrentStateName = request.Request.PersonalDetail.CurrentAddress.StateName,
                            CurrentZipCode = request.Request.PersonalDetail.CurrentAddress.ZipCode,
                            CurrentCountryName = request.Request.PersonalDetail.CurrentAddress.CountryName,
                            PermanentAddressLineOne = request.Request.PersonalDetail.PermanentAddress.AddressLineOne,
                            PermanentAddressLineTwo = request.Request.PersonalDetail.PermanentAddress.AddressLineTwo,
                            PermanentAddressLineThree = request.Request.PersonalDetail.PermanentAddress.AddressLineThree,
                            PermanentCityName = request.Request.PersonalDetail.PermanentAddress.CityName,
                            PermanentStateName = request.Request.PersonalDetail.PermanentAddress.StateName,
                            PermanentZipCode = request.Request.PersonalDetail.PermanentAddress.ZipCode,
                            PermanentCountryName = request.Request.PersonalDetail.PermanentAddress.CountryName,
                            EmailId = request.Request.PersonalDetail.EmailId,
                            Gender = request.Request.PersonalDetail.Gender,
                            MobileNo = request.Request.PersonalDetail.MobileNo,
                            IsActive = true,
                            IsDeleted = false,
                            AadharBackImage = request.Request.aadharDetail.BackImageUrl,
                            AadharFrontImage = request.Request.aadharDetail.FrontImageUrl,
                            PanFrontImage = request.Request.panDetail.FrontImageUrl,
                            PanNameOnCard = request.Request.panDetail.NameOnCard,
                            SelfieImageUrl = request.Request.SelfieDetail?.FrontImageUrl,
                            Marital = request.Request.PersonalDetail.Marital
                        };
                        await _context.PersonalDetails.AddAsync(AddPersonalDetail);
                    }
                    var BuisnessDetail = await _context.BusinessDetails.Where(x => x.LeadId == LeadId && x.IsDeleted == false && x.IsActive == true).FirstOrDefaultAsync();
                    if (BuisnessDetail != null)
                    {
                        BuisnessDetail.DOI = request.Request.BuisnessDetail.DOI;
                        BuisnessDetail.BusGSTNO = request.Request.BuisnessDetail.BusGSTNO;
                        BuisnessDetail.ElectricityNumber = request.Request.PersonalDetail.IVRSNumber;
                        BuisnessDetail.ElectricityOwnerName = "";
                        BuisnessDetail.ElectricityOwnerAddress = "";
                        BuisnessDetail.AddressLineOne = request.Request.BuisnessDetail.CurrentAddress.AddressLineOne;
                        BuisnessDetail.AddressLineTwo = request.Request.BuisnessDetail.CurrentAddress.AddressLineTwo;
                        BuisnessDetail.AddressLineThree = request.Request.BuisnessDetail.CurrentAddress.AddressLineThree;
                        BuisnessDetail.BuisnessMonthlySalary = request.Request.BuisnessDetail.BuisnessMonthlySalary;
                        BuisnessDetail.BusEntityType = request.Request.BuisnessDetail.BusEntityType.ToLower();
                        BuisnessDetail.BusinessName = request.Request.BuisnessDetail.BusinessName;
                        BuisnessDetail.BusMaskPan = request.Request.BuisnessDetail.BusPan;
                        BuisnessDetail.CityName = request.Request.BuisnessDetail.CurrentAddress.CityName;
                        BuisnessDetail.StateName = request.Request.BuisnessDetail.CurrentAddress.StateName;
                        BuisnessDetail.CountryName = request.Request.BuisnessDetail.CurrentAddress.CountryName;
                        BuisnessDetail.IncomeSlab = request.Request.BuisnessDetail.IncomeSlab;
                        BuisnessDetail.OwnershipType = request.Request.PersonalDetail.OwnershipType;
                        BuisnessDetail.ZipCode = request.Request.BuisnessDetail.CurrentAddress.ZipCode;
                        BuisnessDetail.InquiryAmount = request.Request.BuisnessDetail.InquiryAmount;
                        _context.Entry(BuisnessDetail).State = EntityState.Modified;

                    }
                    else
                    {
                        var BusinessDetail = new BusinessDetail
                        {
                            LeadId = LeadId,
                            DOI = request.Request.BuisnessDetail.DOI,
                            BusGSTNO = request.Request.BuisnessDetail.BusGSTNO,
                            ElectricityNumber = request.Request.PersonalDetail.IVRSNumber,
                            ElectricityOwnerName = "",
                            ElectricityOwnerAddress = "",
                            AddressLineOne = request.Request.BuisnessDetail.CurrentAddress.AddressLineOne,
                            AddressLineTwo = request.Request.BuisnessDetail.CurrentAddress.AddressLineTwo,
                            AddressLineThree = request.Request.BuisnessDetail.CurrentAddress.AddressLineThree,
                            BuisnessMonthlySalary = request.Request.BuisnessDetail.BuisnessMonthlySalary,
                            BusEntityType = request.Request.BuisnessDetail.BusEntityType.ToLower(),
                            BusinessName = request.Request.BuisnessDetail.BusinessName,
                            BusMaskPan = request.Request.BuisnessDetail.BusPan,
                            CityName = request.Request.BuisnessDetail.CurrentAddress.CityName,
                            StateName = request.Request.BuisnessDetail.CurrentAddress.StateName,
                            CountryName = request.Request.BuisnessDetail.CurrentAddress.CountryName,
                            IncomeSlab = request.Request.BuisnessDetail.IncomeSlab,
                            OwnershipType = request.Request.PersonalDetail.OwnershipType,
                            ZipCode = request.Request.BuisnessDetail.CurrentAddress.ZipCode,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = "",
                            InquiryAmount = request.Request.BuisnessDetail.InquiryAmount

                        };
                        await _context.BusinessDetails.AddAsync(BusinessDetail);
                    }


                    #region insert Lead Document Details
                    var existingleadDocDetails = await _context.LeadDocumentDetails.Where(x => x.LeadId == LeadId && x.DocumentName != BlackSoilBusinessDocNameConstants.Statement
                                                    && x.DocumentType != BlackSoilBusinessDocTypeConstants.SurrogateProof && x.IsActive && !x.IsDeleted).ToListAsync();
                    if (existingleadDocDetails != null && existingleadDocDetails.Any())
                    {
                        foreach (var item in existingleadDocDetails)
                        {
                            item.IsActive = false;
                            item.IsDeleted = true;
                            _context.Entry(item).State = EntityState.Modified;
                        }
                    }

                    List<LeadDocumentDetail> leadDocumentDetails = new List<LeadDocumentDetail>();
                    if (request.Request.BuisnessDetail != null && !string.IsNullOrEmpty(request.Request.BuisnessDetail.BusGSTNO) && !string.IsNullOrEmpty(request.Request.BuisnessDetail.BuisnessDocumentNo) && request.Request.BuisnessDetail.BuisnessProof == "GST Certificate" && !string.IsNullOrEmpty(request.Request.BuisnessDetail.BuisnessProofUrl))
                    {

                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.GstCertificate,
                            DocumentNumber = request.Request.BuisnessDetail.BuisnessDocumentNo,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = request.Request.BuisnessDetail.BuisnessProofUrl,
                            LeadId = LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }

                    //if (!string.IsNullOrEmpty(request.Request.BuisnessDetail.BuisnessDocumentNo) && request.Request.BuisnessDetail.BuisnessProof == "Udyog Aadhar Certificate" && !string.IsNullOrEmpty(request.Request.BuisnessDetail.BuisnessProofUrl))
                    //{
                    //    leadDocumentDetails.Add(new LeadDocumentDetail
                    //    {
                    //        DocumentName = BlackSoilBusinessDocNameConstants.UdyogAadhaar,
                    //        DocumentNumber = request.Request.BuisnessDetail.BuisnessDocumentNo,
                    //        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                    //        FileUrl = request.Request.BuisnessDetail.BuisnessProofUrl,
                    //        LeadId = LeadId,
                    //        IsActive = true,
                    //        IsDeleted = false
                    //    });
                    //}
                    if (request.Request.PersonalDetail != null && !string.IsNullOrEmpty(request.Request.PersonalDetail.ManualElectricityBillImage))
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.Other,
                            DocumentNumber = string.IsNullOrEmpty(request.Request.PersonalDetail.IVRSNumber) ? "" : request.Request.PersonalDetail.IVRSNumber,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = request.Request.PersonalDetail.ManualElectricityBillImage,
                            LeadId = LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                    if (request.Request.MSMEDetail != null && !string.IsNullOrEmpty(request.Request.MSMEDetail.MSMECertificateUrl))
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.UdyogAadhaar,
                            DocumentNumber = request.Request.MSMEDetail.MSMERegNum ?? "",
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = request.Request.MSMEDetail.MSMECertificateUrl,
                            LeadId = LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }

                    if (request.Request.BankStatementCreditLendingDeail != null && request.Request.BankStatementCreditLendingDeail.StatementList != null && request.Request.BankStatementCreditLendingDeail.StatementList.Count > 0)
                    {
                        var bankDetail = _context.LeadBankDetails.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefault();
                        foreach (var url in request.Request.BankStatementCreditLendingDeail.StatementList)
                        {
                            leadDocumentDetails.Add(new LeadDocumentDetail
                            {
                                DocumentName = BlackSoilBusinessDocNameConstants.Statement,
                                DocumentNumber = bankDetail.AccountNumber,
                                DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                                FileUrl = url.ImageUrl,
                                LeadId = LeadId,
                                IsActive = true,
                                IsDeleted = false,
                                PdfPassword = bankDetail.PdfPassword
                            });
                        }
                    }

                    if (!string.IsNullOrEmpty(request.Request.panDetail.FrontImageUrl) && !string.IsNullOrEmpty(request.Request.panDetail.UniqueId))
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.PANImage,
                            DocumentNumber = request.Request.panDetail.UniqueId,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = request.Request.panDetail.FrontImageUrl,
                            LeadId = LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });

                    }

                    if (!string.IsNullOrEmpty(request.Request.aadharDetail.FrontImageUrl) && !string.IsNullOrEmpty(request.Request.aadharDetail.UniqueId))
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.AadhaarFrontImage,
                            DocumentNumber = request.Request.aadharDetail.UniqueId,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = request.Request.aadharDetail.FrontImageUrl,
                            LeadId = LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }

                    if (!string.IsNullOrEmpty(request.Request.aadharDetail.BackImageUrl) && !string.IsNullOrEmpty(request.Request.aadharDetail.UniqueId))
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.AadhaarBackImage,
                            DocumentNumber = request.Request.aadharDetail.UniqueId,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = request.Request.aadharDetail.BackImageUrl,
                            LeadId = LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }

                    if (leadDocumentDetails != null && leadDocumentDetails.Any())
                    {
                        _context.LeadDocumentDetails.AddRange(leadDocumentDetails);
                    }
                    #endregion
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        result.Status = true;
                    }
                }
                catch (Exception e)
                {
                    result.Message = e.InnerException.ToString();
                }
            }
            return result;
        }

        public async Task<GRPCReply<bool>> AddUpdatePersonalBussDetail(GRPCRequest<UpdateAddressRequest> request)
        {
            GRPCReply<bool> result = new GRPCReply<bool>();
            if (request.Request.AddressType == "PersonalDetail")
            {
                var PersonalDetail = await _context.PersonalDetails.Where(x => x.LeadId == request.Request.LeadId && x.IsDeleted == false && x.IsActive == true).FirstOrDefaultAsync();
                if (PersonalDetail != null)
                {
                    PersonalDetail.CurrentAddressLineOne = request.Request.AddCorrLine1;
                    PersonalDetail.CurrentAddressLineTwo = request.Request.AddCorrLine2;
                    PersonalDetail.CurrentAddressLineThree = request.Request.AddCorrLine3;
                    PersonalDetail.CurrentCityName = request.Request.AddCorrCityName;
                    PersonalDetail.CurrentStateName = request.Request.AddCorrStateName;
                    PersonalDetail.CurrentZipCode = int.Parse(request.Request.AddCorrPincode);
                    _context.Entry(PersonalDetail).State = EntityState.Modified;
                }
            }
            else
            {
                var BuisnessDetail = await _context.BusinessDetails.Where(x => x.LeadId == request.Request.LeadId && x.IsDeleted == false && x.IsActive == true).FirstOrDefaultAsync();
                if (BuisnessDetail != null)
                {
                    BuisnessDetail.AddressLineOne = request.Request.AddCorrLine1;
                    BuisnessDetail.AddressLineTwo = request.Request.AddCorrLine2;
                    BuisnessDetail.AddressLineThree = request.Request.AddCorrLine3;
                    BuisnessDetail.CityName = request.Request.AddCorrCityName;
                    BuisnessDetail.StateName = request.Request.AddCorrStateName;
                    BuisnessDetail.ZipCode = int.Parse(request.Request.AddCorrPincode);
                    _context.Entry(BuisnessDetail).State = EntityState.Modified;
                }
            }
            _context.SaveChanges();
            return result;
        }

        public async Task<GRPCReply<bool>> IsKycCompleted(GRPCRequest<string> request)
        {
            GRPCReply<bool> result = new GRPCReply<bool>();
            long LeadId = await _context.Leads.FirstOrDefaultAsync(x => x.UserName == request.Request).Select(x => x.Id);
            if (LeadId > 0)
            {

                var LeadActivityList = (await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == LeadId &&
                                                                                            x.ActivityMasterName == ActivityEnum.KYC.ToString() &&
                                                                           x.IsActive && !x.IsDeleted).ToListAsync());

                result.Status = (LeadActivityList).All(x => x.IsCompleted);
            }

            return result;
        }

        public async Task<GRPCReply<bool>> InsertLeadNBFCApi(GRPCRequest<List<LeadNBFCSubActivityRequestDc>> request)
        {
            GRPCReply<bool> result = new GRPCReply<bool>();
            if (request.Request != null && request.Request.Any())
            {
                foreach (var master in request.Request)
                {
                    var existingParent = await _context.LeadNBFCSubActivitys.Where(x => x.LeadId == master.LeadId && x.NBFCCompanyId == master.NBFCCompanyId && x.Code == master.Code && x.IsActive && !x.IsDeleted).Include(x => x.LeadNBFCApis).FirstOrDefaultAsync();
                    if (existingParent == null)
                    {
                        List<LeadNBFCApi> apiList = new List<LeadNBFCApi>();
                        if (master.NBFCCompanyApiList != null && master.NBFCCompanyApiList.Any())
                        {
                            foreach (var child in master.NBFCCompanyApiList)
                            {
                                LeadNBFCApi leadNBFCApi = new LeadNBFCApi
                                {
                                    APIUrl = child.APIUrl ?? "",
                                    Code = child.Code ?? "",
                                    Status = child.Status ?? "",
                                    TAPIKey = child.TAPIKey ?? "",
                                    TAPISecretKey = child.TAPISecretKey ?? "",
                                    Sequence = child.Sequence,
                                    RequestId = null,
                                    ResponseId = null,
                                    IsActive = true,
                                    IsDeleted = false,
                                    TReferralCode = child.TReferralCode ?? ""
                                };
                                apiList.Add(leadNBFCApi);
                            }
                        }
                        LeadNBFCSubActivity leadNBFCSubActivity = new LeadNBFCSubActivity
                        {
                            NBFCCompanyId = master.NBFCCompanyId,
                            ActivityMasterId = master.ActivityMasterId,
                            SubActivityMasterId = master.SubActivityMasterId,
                            Code = master.Code,//SubActivityName
                            Status = master.Status,
                            LeadId = master.LeadId,
                            IdentificationCode = master.IdentificationCode, //company IdentificationCode
                            IsActive = true,
                            IsDeleted = false,
                            LeadNBFCApis = apiList,
                            SubActivitySequence = master.SubActivitySequence,
                            ActivityName = master.ActivityName,

                        };
                        await _context.AddAsync(leadNBFCSubActivity);

                    }
                    else
                    {
                        if (master.NBFCCompanyApiList != null && master.NBFCCompanyApiList.Any())
                        {
                            foreach (var child in master.NBFCCompanyApiList)
                            {
                                var existingChild = await _context.LeadNBFCApis.Where(x => x.LeadNBFCSubActivityId == existingParent.Id && x.Code == child.Code && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                                if (existingChild == null)
                                {
                                    LeadNBFCApi leadNBFCApi = new LeadNBFCApi
                                    {
                                        LeadNBFCSubActivityId = existingParent.Id == null ? 0 : existingParent.Id,
                                        APIUrl = child.APIUrl ?? "",
                                        Code = child.Code ?? "",
                                        Status = child.Status ?? "",
                                        TAPIKey = child.TAPIKey ?? "",
                                        TAPISecretKey = child.TAPISecretKey ?? "",
                                        Sequence = child.Sequence,
                                        RequestId = null,
                                        ResponseId = null,
                                        IsActive = true,
                                        IsDeleted = false,
                                        TReferralCode = child.TReferralCode
                                    };
                                    await _context.AddAsync(leadNBFCApi);
                                }
                                else
                                {
                                    existingChild.Sequence = child.Sequence;
                                    _context.Entry(existingChild).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                }
                #region Old
                //var existingApis = await _context.LeadNBFCSubActivitys.Where(x => x.LeadId == request.Request.FirstOrDefault().LeadId && x.IsActive && !x.IsDeleted).Include(x => x.LeadNBFCApis).ToListAsync();
                //if (existingApis != null && existingApis.Any())
                //{
                //    foreach (var item in existingApis)
                //    {
                //        item.IsActive = false;
                //        item.IsDeleted = true;
                //        _context.Entry(item).State = EntityState.Modified;
                //        if (item.LeadNBFCApis != null && item.LeadNBFCApis.Any())
                //        {
                //            foreach (var child in existingApis)
                //            {
                //                child.IsActive = false;
                //                child.IsDeleted = true;
                //                _context.Entry(child).State = EntityState.Modified;
                //            }
                //        }
                //    }
                //}
                //foreach (var master in request.Request)
                //{
                //    List<LeadNBFCApi> apiList = new List<LeadNBFCApi>();
                //    if (master.NBFCCompanyApiList != null && master.NBFCCompanyApiList.Any())
                //    {
                //        foreach (var child in master.NBFCCompanyApiList)
                //        {
                //            LeadNBFCApi leadNBFCApi = new LeadNBFCApi
                //            {
                //                APIUrl = child.APIUrl ?? "",
                //                Code = child.Code ?? "",
                //                Status = child.Status ?? "",
                //                TAPIKey = child.TAPIKey ?? "",
                //                TAPISecretKey = child.TAPISecretKey ?? "",
                //                Sequence = child.Sequence,
                //                RequestId = null,
                //                ResponseId = null,
                //                IsActive = true,
                //                IsDeleted = false,
                //                TReferralCode = child.TReferralCode
                //            };
                //            apiList.Add(leadNBFCApi);
                //        }
                //    }
                //    LeadNBFCSubActivity leadNBFCSubActivity = new LeadNBFCSubActivity
                //    {
                //        NBFCCompanyId = master.NBFCCompanyId,
                //        ActivityMasterId = master.ActivityMasterId,
                //        SubActivityMasterId = master.SubActivityMasterId,
                //        Code = master.Code,//SubActivityName
                //        Status = master.Status,
                //        LeadId = master.LeadId,
                //        IdentificationCode = master.IdentificationCode, //company IdentificationCode
                //        IsActive = true,
                //        IsDeleted = false,
                //        LeadNBFCApis = apiList,
                //        SubActivitySequence = master.SubActivitySequence,
                //        ActivityName = master.ActivityName,

                //    };
                //    await _context.AddAsync(leadNBFCSubActivity);
                //}
                #endregion
                var rowChanged = await _context.SaveChangesAsync();
                if (rowChanged > 0)
                {
                    result.Status = true;
                    result.Message = "Data Saved";
                    result.Response = true;
                }
                else
                {
                    result.Status = false;
                    result.Message = "Failed To Save";
                    result.Response = false;

                }
            }
            return result;
        }
        public async Task<GRPCReply<List<DefaultOfferSelfConfigurationDc>>> AddUpdateSelfConfiguration(GRPCRequest<List<DefaultOfferSelfConfigurationDc>> selfConfigList)
        {
            GRPCReply<List<DefaultOfferSelfConfigurationDc>> response = new GRPCReply<List<DefaultOfferSelfConfigurationDc>>();
            foreach (var config in selfConfigList.Request)
            {
                if (config.Id > 0)
                {
                    var existing = await _context.DefaultOfferSelfConfigurations.FirstOrDefaultAsync(x => x.Id == config.Id);
                    if (existing != null)
                    {
                        existing.CompanyId = config.CompanyId;
                        existing.CustomerType = config.CustomerType;
                        existing.MaxCibilScore = config.MaxCibilScore;
                        existing.MaxCreditLimit = config.MaxCreditLimit;
                        existing.MaxVintageDays = config.MaxVintageDays;
                        existing.MinCibilScore = config.MinCibilScore;
                        existing.MinCreditLimit = config.MinCreditLimit;
                        existing.MinVintageDays = config.MinVintageDays;
                        existing.MultiPlier = config.MultiPlier;
                        existing.IsActive = config.IsActive;

                        _context.Entry(existing).State = EntityState.Modified;
                    }
                }
                else
                {
                    DefaultOfferSelfConfiguration offerSelfConfiguration = new DefaultOfferSelfConfiguration
                    {
                        CompanyId = config.CompanyId,
                        CustomerType = config.CustomerType,
                        MaxCibilScore = config.MaxCibilScore,
                        MaxCreditLimit = config.MaxCreditLimit,
                        MaxVintageDays = config.MaxVintageDays,
                        MinCibilScore = config.MinCibilScore,
                        MinCreditLimit = config.MinCreditLimit,
                        MinVintageDays = config.MinVintageDays,
                        MultiPlier = config.MultiPlier,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.DefaultOfferSelfConfigurations.Add(offerSelfConfiguration);
                }
            }
            int rowChanged = await _context.SaveChangesAsync();
            if (rowChanged > 0)
            {
                response.Status = true;
                response.Message = "Configuration Updated Successfully";
                response.Response = selfConfigList.Request;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed To Update Configs";
            }
            return response;
        }
        public async Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request)
        {
            var result = new GRPCReply<long>();
            if (request != null)
            {
                string EventName = request.Request.EventName;
                long LeadId = 0;
                BlackSoilWebhookResponse blackSoilWebhookResponse = new BlackSoilWebhookResponse
                {
                    response = request.Request.Data,
                    eventName = request.Request.EventName,
                    LeadId = null,
                    IsActive = true,
                    CreatedBy = "",
                    IsDeleted = false
                };
                _context.BlackSoilWebhookResponses.Add(blackSoilWebhookResponse);
                await _context.SaveChangesAsync();
                try
                {
                    switch (EventName)
                    {
                        case BlackSoilWebhookConstant.ApplicationCreated:
                            var applicationcreated = JsonConvert.DeserializeObject<BlackSoilApplicationCreated>(request.Request.Data);

                            var blacksoilupdates = await _context.BlackSoilUpdates.Where(x => x.BusinessId == applicationcreated.data.business).FirstOrDefaultAsync();
                            blacksoilupdates.ApplicationId = applicationcreated.data.id;
                            blacksoilupdates.ApplicationCode = applicationcreated.data.application_id;
                            LeadId = blacksoilupdates.LeadId;
                            _context.Entry(blacksoilupdates).State = EntityState.Modified;

                            break;
                        case BlackSoilWebhookConstant.LineApproved:
                            var lineapproved = JsonConvert.DeserializeObject<BlackSoilLineApproved>(request.Request.Data);
                            var blacksoilupdatesLineApproved = await _context.BlackSoilUpdates.Where(x => x.BusinessId == lineapproved.data.business).FirstOrDefaultAsync();


                            if (!blacksoilupdatesLineApproved.ApplicationId.HasValue || blacksoilupdatesLineApproved.ApplicationId.Value == 0)
                            {
                                blacksoilupdatesLineApproved.ApplicationId = lineapproved.data.id;
                                blacksoilupdatesLineApproved.ApplicationCode = lineapproved.data.application_id;
                                _context.Entry(blacksoilupdatesLineApproved).State = EntityState.Modified;
                            }
                            var leadonLineApproved = await _context.Leads.FirstOrDefaultAsync(x => x.Id == blacksoilupdatesLineApproved.LeadId);
                            LeadId = blacksoilupdatesLineApproved.LeadId;

                            leadonLineApproved.Status = LeadStatusEnum.LineApproved.ToString();
                            _context.Entry(leadonLineApproved).State = EntityState.Modified;
                            blackSoilWebhookResponse.LeadId = LeadId;

                            _context.Entry(blackSoilWebhookResponse).State = EntityState.Modified;

                            await _context.SaveChangesAsync();


                            try
                            {
                                await _nBFCSchedular.GenerateBlackSoilOfferForLead(LeadId);
                            }
                            catch (Exception ex)
                            {
                            }


                            break;
                        case BlackSoilWebhookConstant.LineInitiate:
                            var LineInitiate = JsonConvert.DeserializeObject<BlackSoilLineInitiated>(request.Request.Data);
                            LeadId = await _context.BlackSoilUpdates.Where(x => x.BusinessId == LineInitiate.data.business).Select(x => x.LeadId).FirstOrDefaultAsync();
                            try
                            {
                                var leadonLineInitiate = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId);
                                leadonLineInitiate.Status = LeadStatusEnum.LineInitiated.ToString();
                                _context.Entry(leadonLineInitiate).State = EntityState.Modified;


                                await _nBFCSchedular.LineInitiateOrRejectBlackSoil(LeadId, LeadOfferConstant.OfferGenerated, "");
                            }
                            catch (Exception ex)
                            {
                            }

                            break;
                        case BlackSoilWebhookConstant.Linerejected:
                            var Linerejected = JsonConvert.DeserializeObject<BlackSoilLineRejected>(request.Request.Data);
                            LeadId = await _context.BlackSoilUpdates.Where(x => x.BusinessId == Linerejected.data.business).Select(x => x.LeadId).FirstOrDefaultAsync();
                            try
                            {
                                var leadonLinerejected = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId);
                                leadonLinerejected.Status = LeadStatusEnum.LineRejected.ToString();
                                _context.Entry(leadonLinerejected).State = EntityState.Modified;
                                await _nBFCSchedular.LineInitiateOrRejectBlackSoil(LeadId, LeadOfferConstant.OfferRejected, Linerejected.data.close_application_reasons);
                            }
                            catch (Exception ex)
                            {
                            }
                            break;
                        case BlackSoilWebhookConstant.LineActivate:
                            var lineactivate = JsonConvert.DeserializeObject<BlackSoilLineActivated>(request.Request.Data);
                            LeadId = await _context.BlackSoilUpdates.Where(x => x.BusinessId == lineactivate.data.business.id).Select(x => x.LeadId).FirstOrDefaultAsync();
                            var leadonLineActivate = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId);
                            leadonLineActivate.Status = LeadStatusEnum.LineActivated.ToString();
                            _context.Entry(leadonLineActivate).State = EntityState.Modified;
                            break;
                    }
                    if (LeadId > 0)
                    {
                        blackSoilWebhookResponse.LeadId = LeadId;
                        blackSoilWebhookResponse.Status = EventName;
                        _context.Entry(blackSoilWebhookResponse).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {

                }
                result.Response = LeadId;
                result.Status = true;
            }
            return result;
        }

        public async Task<GRPCReply<List<OfferListReply>>> GetOfferList(GRPCRequest<long> request)
        {
            GRPCReply<List<OfferListReply>> reply = new GRPCReply<List<OfferListReply>>();
            var lead = (await _context.Leads.Where(x => x.Id == request.Request).Include(x => x.CompanyLeads).FirstOrDefaultAsync());
            if (lead != null)
            {
                var leadoffer = await _context.LeadOffers.Where(x => x.LeadId == request.Request && !x.IsDeleted && x.IsActive).Select(x => new OfferListReply
                {
                    LeadOfferId = x.Id,
                    NBFCCompanyId = x.NBFCCompanyId,
                    Status = x.Status,
                    CreditLimit = x.CreditLimit,
                    LeadId = x.LeadId
                }).ToListAsync();
                if (leadoffer != null && leadoffer.Any())
                {
                    reply.Response = leadoffer;
                    reply.Status = true;
                }
            }
            return reply;
        }


        public async Task<TemplateMasterResponseDc> GetLeadNotificationTemplate(TemplateMasterRequestDc request)
        {
            TemplateMasterResponseDc Reply = new TemplateMasterResponseDc();
            if (request != null)
            {
                var data = await _context.LeadTemplateMasters.Where(x => x.TemplateCode == request.TemplateCode).Select(x =>
                new GetTemplateMasterListResponseDc
                {
                    DLTID = x.DLTID,
                    TemplateCode = request.TemplateCode,
                    IsActive = x.IsActive,
                    TemplateType = x.TemplateType,
                    CreatedDate = x.Created,
                    Template = x.Template,
                    TemplateId = x.Id
                }).FirstOrDefaultAsync();
                if (data != null)
                {
                    Reply.Response = data;
                    Reply.Status = true;
                }
                else
                {
                    Reply.Status = false;
                    Reply.Message = "data not found";
                }
            }
            else
            {
                Reply.Status = true;
                Reply.Message = "empty request";
            }
            return Reply;
        }

        public async Task<GRPCReply<string>> GetLeadPreAgreement(GRPCRequest<long> LeadId)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            var BlackSoilUpdates = (await _context.BlackSoilUpdates.Where(x => x.LeadId == LeadId.Request && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync());
            if (BlackSoilUpdates != null && BlackSoilUpdates.SingingUrl != null)
            {
                reply.Response = BlackSoilUpdates.SingingUrl;
                reply.Status = true;
            }
            return reply;
        }

        public async Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync()
        {
            GRPCReply<List<GetTemplateMasterListResponseDc>> reply = new GRPCReply<List<GetTemplateMasterListResponseDc>>();
            var leadTemplateList = (await _context.LeadTemplateMasters.Where(x => !x.IsDeleted).Select(x => new GetTemplateMasterListResponseDc
            {
                DLTID = x.DLTID,
                Template = x.Template,
                TemplateCode = x.TemplateCode,
                TemplateType = x.TemplateType,
                TemplateId = x.Id,
                IsActive = x.IsActive,
                CreatedDate = x.Created
            }).ToListAsync());
            if (leadTemplateList != null && leadTemplateList.Any())
            {
                reply.Response = leadTemplateList;
                reply.Status = true;
                reply.Message = "data found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "data not found";
            }
            return reply;
        }

        public async Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request)
        {
            GRPCReply<GetTemplateMasterListResponseDc> reply = new GRPCReply<GetTemplateMasterListResponseDc>();
            var leadTemplate = (await _context.LeadTemplateMasters.Where(x => x.Id == request.Request && !x.IsDeleted).Select(x => new GetTemplateMasterListResponseDc
            {
                DLTID = x.DLTID,
                Template = x.Template,
                TemplateCode = x.TemplateCode,
                TemplateType = x.TemplateType,
                IsActive = x.IsActive,
                TemplateId = x.Id
            }).FirstOrDefaultAsync());
            if (leadTemplate != null)
            {
                reply.Response = leadTemplate;
                reply.Status = true;
                reply.Message = "data found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "data not found";
            }
            return reply;
        }

        public async Task<LeadListPageReply> GetLeadForListPageExport(LeadListPageRequest request)
        {

            LeadListPageReply leadListPageReply = new LeadListPageReply();
            var predicate = PredicateBuilder.True<Leads>();
            List<LeadListDetail> leads = new List<LeadListDetail>();

            List<string> statusList = new List<string>();
            statusList.Add(LeadBusinessLoanStatusConstants.Pending);
            statusList.Add(LeadBusinessLoanStatusConstants.Pending.ToLower());
            statusList.Add(LeadBusinessLoanStatusConstants.LoanInitiated);
            statusList.Add(LeadBusinessLoanStatusConstants.LoanActivated);
            statusList.Add(LeadBusinessLoanStatusConstants.LoanApproved);
            statusList.Add(LeadBusinessLoanStatusConstants.LoanRejected);
            statusList.Add(LeadBusinessLoanStatusConstants.LoanAvailed);
            statusList.Add(LeadStatusEnum.LineInitiated.ToString());
            statusList.Add(LeadStatusEnum.LineActivated.ToString());
            statusList.Add(LeadStatusEnum.LineApproved.ToString());
            statusList.Add(LeadStatusEnum.LineRejected.ToString());
            predicate = predicate.And(x => !statusList.Contains(x.Status) && !x.IsDeleted && x.CompanyLeads.Any(y => !y.IsDeleted));

            if (request.ToDate != null)
                request.ToDate = request.ToDate?.Date.AddDays(1).AddMilliseconds(-1);

            //predicate = predicate.And(x => x.IsActive && !x.IsDeleted && x.CompanyLeads.Any(y => y.IsActive && !y.IsDeleted));

            if (request.ToDate != null)
                request.ToDate = request.ToDate?.Date.AddDays(1).AddMilliseconds(-1);

            if ((request.ProductType != DSAProfileTypeConstants.DSA && request.ProductType != DSAProfileTypeConstants.Connector && request.ProductType != "All")
                && !string.IsNullOrEmpty(request.Keyword))
            {
                predicate = predicate.And(x => x.MobileNo.Contains(request.Keyword.Trim()) || x.LeadCode.Contains(request.Keyword.Trim()) || x.ApplicantName.Contains(request.Keyword.Trim()));
            }

            if (request.FromDate != null && request.ToDate != null)
            {
                predicate = predicate.And(x => x.Created >= request.FromDate && x.Created <= request.ToDate);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                predicate = predicate.And(x => x.Status == request.Status);
            }

            if ((request.ProductType != DSAProfileTypeConstants.DSA && request.ProductType != DSAProfileTypeConstants.Connector && request.ProductType != "All") && request.CityId > 0)
            {
                predicate = predicate.And(x => x.CityId == request.CityId);
            }

            if (request.CompanyId != null && request.CompanyId.Count() > 0)
            {
                predicate = predicate.And(x => x.CompanyLeads.Any(y => request.CompanyId.Contains((int)y.CompanyId) && y.IsActive && !y.IsDeleted));
            }

            if (request.ProductId.Any() && request.ProductId.Count > 0)
            {
                predicate = predicate.And(x => request.ProductId.Contains(x.ProductId));
            }
            if (request.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower() || request.UserType.ToLower() == UserTypeConstants.SuperAdmin.ToLower())
            {
                if (request.IsDSA)
                {
                    predicate = predicate.And(x => request.UserIds != null && x.CreatedBy != null && request.UserIds.Contains(x.CreatedBy));
                }
                else
                {
                    predicate = predicate.And(x => x.CreatedBy == null || x.UserName == x.CreatedBy);
                }
            }
            var leadData = await _context.Leads.Where(predicate).OrderByDescending(z => z.Id).ToListAsync();
            var payOutPercentageList = await _context.DSAPayouts.Where(x => x.IsActive && !x.IsDeleted).Select(x => new { x.LeadId, x.MinAmount, x.MaxAmount, x.PayoutPercenatge, x.ProductId }).ToListAsync();
            var leadAgreement = await _context.leadAgreements.Where(x => x.IsActive && !x.IsDeleted).Select(x => new { x.LeadId, x.StartedOn, x.ExpiredOn }).ToListAsync();
            leads = leadData.Select(x => new LeadListDetail
            {
                CreatedDate = x.Created,
                Id = x.Id,
                MobileNo = x.MobileNo,
                UserId = x.UserName,
                LastModified = x.LastModified,
                LeadCode = x.LeadCode,
                CreditScore = x.CreditScore,
                Status = x.Status,
                CityId = x.CityId,
                LeadGenerator = x.LeadGenerator,
                LeadConvertor = x.LeadConverter,
                CreditLimit = x.CreditLimit,
                ProductCode = x.ProductCode,
                AgreementStartDate = (leadAgreement != null && leadAgreement.Count > 0)
                                        ? leadAgreement.Where(y => y.LeadId == x.Id).Select(y => y.StartedOn).FirstOrDefault() : null,
                AgreementEndDate = (leadAgreement != null && leadAgreement.Count > 0)
                                        ? leadAgreement.Where(y => y.LeadId == x.Id).Select(y => y.ExpiredOn).FirstOrDefault() : null,
                SalesAgentCommissions = (payOutPercentageList != null && payOutPercentageList.Count > 0)
                                        ? payOutPercentageList.Where(y => y.LeadId == x.Id).Select(y => new SalesAgentCommissionList { PayoutPercentage = y.PayoutPercenatge, MinAmount = y.MinAmount, MaxAmount = y.MaxAmount, ProductId = y.ProductId }).ToList()
                                        : new List<SalesAgentCommissionList>()
            }).ToList();

            List<long> LeadIds = leads.Select(x => x.Id).ToList();
            if (LeadIds != null && LeadIds.Any())
            {
                var companyName = await _context.CompanyLead.Where(x => LeadIds.Contains(x.LeadId)).Select(y => new
                {
                    leadId = y.LeadId,
                    anchorName = y.AnchorName,
                    uniquecode = y.UserUniqueCode,
                    vintageDays = y.VintageDays
                }).ToListAsync();

                if (leads != null && leads.Count > 0)
                {
                    foreach (var lead in leads)
                    {
                        if (companyName != null)
                        {
                            var companyData = companyName.FirstOrDefault(x => x.leadId == lead.Id);
                            lead.AnchorName = companyData.anchorName;
                            lead.UniqueCode = string.Join(",", companyName.Where(x => x.leadId == lead.Id).Select(y => y.uniquecode).ToList());
                            lead.VintageDays = companyData.vintageDays;
                        }
                    }
                }

                if (LeadIds != null && LeadIds.Any())
                {
                    var leadloandata = await _context.LeadLoan.Where(x => x.LeadMasterId.HasValue && LeadIds.Contains(x.LeadMasterId.Value)).Select(y => new
                    {
                        leadId = y.LeadMasterId,
                        loan_app_id = y.loan_app_id,
                        partner_loan_app_id = y.partner_loan_app_id
                    }).ToListAsync();

                    if (leads != null && leads.Count > 0)
                    {
                        foreach (var lead in leads)
                        {
                            if (leadloandata != null)
                            {
                                var leadloan = leadloandata.FirstOrDefault(x => x.leadId == lead.Id);
                                if (leadloan != null)
                                {
                                    lead.Loan_app_id = leadloan.loan_app_id;
                                    lead.Partner_Loan_app_id = leadloan.partner_loan_app_id;
                                }
                            }
                        }
                    }
                }
            }


            if (LeadIds != null && LeadIds.Any())
            {
                var data = await _context.LeadActivityMasterProgresses.Where(x => LeadIds.Contains(x.LeadMasterId) && x.IsCompleted == true).OrderBy(x => x.Sequence).GroupBy(x => x.LeadMasterId)
                    .Select(x => new
                    {
                        LeadId = x.Key,
                        Sequence = x.Max(y => y.Sequence),
                        ActivityId = x.OrderByDescending(y => y.Sequence).FirstOrDefault().ActivityMasterId,
                        SubActivityId = x.OrderByDescending(y => y.Sequence).FirstOrDefault().SubActivityMasterId,
                        ActivityMasterName = x.OrderByDescending(y => y.Sequence).FirstOrDefault().ActivityMasterName,
                        SubactivityMasterName = x.OrderByDescending(y => y.Sequence).FirstOrDefault().SubActivityMasterName
                    }).ToListAsync();

                var rejectedData = await _context.LeadActivityMasterProgresses.Where(x => LeadIds.Contains(x.LeadMasterId) && x.ActivityMasterName == ActivityEnum.Rejected.ToString() && !string.IsNullOrEmpty(x.RejectMessage)).GroupBy(x => x.LeadMasterId)
                   .Select(x => new
                   {
                       LeadId = x.Key,
                       RejectMessage = x.FirstOrDefault().RejectMessage
                   }).ToListAsync();

                if (data != null)
                {
                    foreach (var item in leads)
                    {
                        if (data.Any(x => x.LeadId == item.Id))
                        {
                            item.SequenceNo = data.FirstOrDefault(x => x.LeadId == item.Id).Sequence;
                            item.SubActivityId = data.FirstOrDefault(x => x.LeadId == item.Id).SubActivityId;
                            item.ActivityId = data.FirstOrDefault(x => x.LeadId == item.Id).ActivityId;
                            item.ScreenName = string.IsNullOrEmpty(data.FirstOrDefault(x => x.LeadId == item.Id).SubactivityMasterName) ? (data.FirstOrDefault(x => x.LeadId == item.Id).SubactivityMasterName + " " + data.FirstOrDefault(x => x.LeadId == item.Id).ActivityMasterName) : data.FirstOrDefault(x => x.LeadId == item.Id).ActivityMasterName;
                        }

                        if (rejectedData != null)
                        {
                            if (rejectedData.Any(x => x.LeadId == item.Id))
                            {
                                item.RejectionMessage = rejectedData.FirstOrDefault(x => x.LeadId == item.Id).RejectMessage;
                            }
                        }
                    }
                }

            }

            leadListPageReply.LeadListDetails = leads;

            return leadListPageReply;
        }
        public async Task<GRPCReply<VerifyLeadDocumentReply>> VerifyLeadDocument(GRPCRequest<VerifyLeadDocumentRequest> request)
        {
            GRPCReply<VerifyLeadDocumentReply> verifyLeadDocumentReply = new GRPCReply<VerifyLeadDocumentReply>();
            if (request != null)
            {
                var AllleadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.Request.LeadId && x.IsDeleted == false).ToListAsync();

                if (AllleadActivity != null)
                {
                    LeadActivityMasterProgresses _lead = AllleadActivity.Where(x => x.LeadMasterId == request.Request.LeadId && x.ActivityMasterId == request.Request.ActivityMasterId && x.IsActive && !x.IsDeleted && x.SubActivityMasterId == request.Request.SubActivityMasterId).FirstOrDefault();
                    if (_lead != null)
                    {
                        LeadActivityMasterProgresses _personalDetail = AllleadActivity.Where(x => x.LeadMasterId == request.Request.LeadId && x.IsActive && !x.IsDeleted && x.ActivityMasterName == ActivityConstants.PersonalInfo).FirstOrDefault();
                        LeadActivityMasterProgresses _aadharDetail = AllleadActivity.Where(x => x.LeadMasterId == request.Request.LeadId && x.IsActive && !x.IsDeleted && x.SubActivityMasterName == SubActivityConstants.Aadhar).FirstOrDefault();
                        LeadActivityMasterProgresses _businessInfo = AllleadActivity.Where(x => x.LeadMasterId == request.Request.LeadId && x.IsActive && !x.IsDeleted && x.ActivityMasterName == ActivityConstants.BusinessInfo).FirstOrDefault();

                        //2 Reject , 1 Approve
                        if (request.Request.IsApprove == 2)
                        {
                            if (_lead.SubActivityMasterName == SubActivityConstants.Pan)
                            {
                                if (_aadharDetail != null)
                                {
                                    _aadharDetail.IsApproved = request.Request.IsApprove;
                                    _aadharDetail.Comment = request.Request.Comment;
                                    _context.Entry(_aadharDetail).State = EntityState.Modified;
                                }

                                if (_personalDetail != null)
                                {

                                    _personalDetail.IsApproved = request.Request.IsApprove;
                                    _personalDetail.Comment = request.Request.Comment;

                                    _context.Entry(_personalDetail).State = EntityState.Modified;

                                }

                                if (_businessInfo != null)
                                {

                                    _businessInfo.IsApproved = request.Request.IsApprove;
                                    _businessInfo.Comment = request.Request.Comment;

                                    _context.Entry(_businessInfo).State = EntityState.Modified;

                                }
                            }
                            else if (_lead.SubActivityMasterName == SubActivityConstants.Aadhar)
                            {
                                if (_personalDetail != null)
                                {

                                    _personalDetail.IsApproved = request.Request.IsApprove;
                                    _personalDetail.Comment = request.Request.Comment;
                                    _context.Entry(_personalDetail).State = EntityState.Modified;


                                }


                                if (_businessInfo != null)
                                {

                                    _businessInfo.IsApproved = request.Request.IsApprove;
                                    _businessInfo.Comment = request.Request.Comment;

                                    _context.Entry(_businessInfo).State = EntityState.Modified;

                                }
                            }
                            else if (_lead.ActivityMasterName == ActivityConstants.DSATypeSelection)
                            {
                                var _dsaInfoList = AllleadActivity.Where(x => x.LeadMasterId == request.Request.LeadId && x.IsActive && !x.IsDeleted && (x.ActivityMasterName == ActivityConstants.DSAPersonalInfo || x.ActivityMasterName == ActivityConstants.DSATypeSelection)).ToList();
                                if (_dsaInfoList != null && _dsaInfoList.Any())
                                {
                                    foreach (var data in _dsaInfoList)
                                    {
                                        data.IsApproved = request.Request.IsApprove;
                                        data.Comment = request.Request.Comment;
                                        data.IsCompleted = false;
                                        _context.Entry(data).State = EntityState.Modified;
                                    };
                                }
                            }
                        }

                        _lead.IsApproved = request.Request.IsApprove;
                        _lead.Comment = request.Request.Comment;

                        //Insert Rejected Activity 
                        //if (request.IsApprove == 2)
                        //{
                        //    if (AllleadActivity != null)
                        //    {
                        //        var leadActivityMasterProgresses = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId &&
                        //                                                                  x.ActivityMasterId == request.ActivityMasterId &&
                        //                                                                  x.SubActivityMasterId == request.SubActivityMasterId &&
                        //                                                                  x.IsActive && !x.IsDeleted);
                        //        if (leadActivityMasterProgresses != null)
                        //        {
                        //            var RejectleadActivity = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId && x.ActivityMasterName == ActivityEnum.Rejected.ToString());
                        //            if (RejectleadActivity != null)
                        //            {
                        //                int i = 2;
                        //                foreach (var item in AllleadActivity.Where(x => x.Sequence > leadActivityMasterProgresses.Sequence))
                        //                {
                        //                    if (item.ActivityMasterName != ActivityEnum.Inprogress.ToString())
                        //                    {
                        //                        item.IsActive = true;
                        //                        item.Sequence = _lead.Sequence + 1;
                        //                        _context.Entry(item).State = EntityState.Modified;
                        //                    }
                        //                    i = i + 1;
                        //                }
                        //                RejectleadActivity.IsActive = true;
                        //                RejectleadActivity.Sequence = _lead.Sequence + 1;
                        //                RejectleadActivity.IsCompleted = false;
                        //                _context.Entry(RejectleadActivity).State = EntityState.Modified;
                        //            }                           
                        //        }
                        //    }

                        //}
                        _context.Entry(_lead).State = EntityState.Modified;
                        int rowchanged = await _context.SaveChangesAsync();
                        if (rowchanged > 0)
                        {
                            verifyLeadDocumentReply.Status = true;
                            verifyLeadDocumentReply.Message = "Document " + (request.Request.IsApprove == 1 ? "Approved " : "Rejected ") + "Successfully.";
                        }
                        else
                        {
                            verifyLeadDocumentReply.Status = false;
                            verifyLeadDocumentReply.Message = "issue During Update.";
                        }

                        //------------------S : Make log---------------------
                        #region Make History
                        var result = await _leadHistoryManager.GetLeadHistroy(request.Request.LeadId, "VerifyLeadDocument");
                        LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                        {
                            LeadId = request.Request.LeadId,
                            UserID = result.UserId,
                            UserName = "",
                            EventName = "KYC Approved/Reject",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                            Narretion = result.Narretion,
                            NarretionHTML = result.NarretionHTML,
                            CreatedTimeStamp = result.CreatedTimeStamp
                        };
                        await _massTransitService.Publish(histroyEvent);
                        #endregion
                        //------------------E : Make log---------------------

                    }
                    else
                    {
                        verifyLeadDocumentReply.Status = false;
                        verifyLeadDocumentReply.Message = "issue During Update.";
                    }
                    return verifyLeadDocumentReply;
                }
                else
                {
                    verifyLeadDocumentReply.Status = false;
                    verifyLeadDocumentReply.Message = "issue During Update.";
                }
            }
            else
            {
                verifyLeadDocumentReply.Status = false;
                verifyLeadDocumentReply.Message = "record not found";
            }
            return verifyLeadDocumentReply;

        }

        public async Task<LeadListDetail> GetLeadCommonDetail(long leadId)
        {
            var lead = await _context.Leads.Where(x => x.Id == leadId).ToListAsync();
            var leadData = lead.Select(y => new LeadListDetail
            {
                LeadCode = y.LeadCode,
                CustomerName = y.ApplicantName,
                MobileNo = y.MobileNo,
            }).FirstOrDefault();



            var data = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadId && x.IsCompleted == true).OrderBy(x => x.Sequence).GroupBy(x => x.LeadMasterId)
                .Select(x => new
                {
                    LeadId = x.Key,
                    ActivityMasterName = x.OrderByDescending(y => y.Sequence).FirstOrDefault().ActivityMasterName,
                    SubactivityMasterName = x.OrderByDescending(y => y.Sequence).FirstOrDefault().SubActivityMasterName
                }).ToListAsync();

            if (data != null)
            {
                if (data.Any(x => x.LeadId == leadId))
                {
                    leadData.ScreenName = string.IsNullOrEmpty(data.FirstOrDefault(x => x.LeadId == leadId).SubactivityMasterName) ? (data.FirstOrDefault(x => x.LeadId == leadId).SubactivityMasterName + " " + data.FirstOrDefault(x => x.LeadId == leadId).ActivityMasterName) : data.FirstOrDefault(x => x.LeadId == leadId).ActivityMasterName;
                }
            }

            return leadData;
        }

        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            GRPCReply<List<AuditLogReply>> gRPCReply = new GRPCReply<List<AuditLogReply>>();
            AuditLogHelper auditLogHelper = new AuditLogHelper(_context);
            var auditLogs = await auditLogHelper.GetAuditLogs(request.Request.EntityId, request.Request.EntityName, request.Request.Skip, request.Request.Take);
            if (auditLogs != null && auditLogs.Any())
            {
                gRPCReply.Status = true;
                gRPCReply.Response = auditLogs.Select(x => new AuditLogReply
                {
                    ModifiedDate = x.Timestamp,
                    Changes = x.Changes,
                    ModifiedBy = x.UserId,
                    TotalRecords = x.TotalRecords,
                    ActionType = x.Action
                }).ToList();
                gRPCReply.Message = "Data Found";
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Data Not Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<LeadBankDetailResponse>>> GetLeadBankDetailByLeadId(GRPCRequest<long> request)
        {
            GRPCReply<List<LeadBankDetailResponse>> gRPCReply = new GRPCReply<List<LeadBankDetailResponse>>();
            gRPCReply.Response = await _context.LeadBankDetails.Where(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted).Select(x => new LeadBankDetailResponse
            {
                LeadId = x.LeadId,
                AccountHolderName = x.AccountHolderName,
                AccountNumber = x.AccountNumber,
                AccountType = x.AccountType,
                BankName = x.BankName,
                Type = x.Type,
                IFSCCode = x.IFSCCode,
                LoanAccountId = 0
            }).ToListAsync();

            if (gRPCReply.Response != null)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Data Not Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<UpdateLeadOfferResponse>> UpdateLeadOffers(GRPCRequest<UpdateLeadOfferRequest> request)
        {
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            GRPCReply<UpdateLeadOfferResponse> gRPCReply = new GRPCReply<UpdateLeadOfferResponse>();
            gRPCReply.Status = false;
            gRPCReply.Message = "Something went wrong.";
            var leadoffer = await _context.LeadOffers.FirstOrDefaultAsync(x => x.Id == request.Request.LeadOfferId);
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == leadoffer.LeadId);

            if (lead != null && (lead.OfferCompanyId == null || lead.OfferCompanyId == 0) && leadoffer != null && leadoffer.Status == LeadOfferConstant.OfferGenerated && leadoffer.CreditLimit > 0)
            {

                lead.CreditLimit = leadoffer.CreditLimit;
                lead.OfferCompanyId = leadoffer.NBFCCompanyId;
                lead.InterestRate = request.Request.InterestRate;
                lead.LastModified = currentDateTime;

                _context.Entry(lead).State = EntityState.Modified;

                var colender = _context.CoLenderResponse.FirstOrDefault(x => x.LeadMasterId == lead.Id && x.IsActive && !x.IsDeleted);
                if (colender != null && request.Request.newOfferAmout > 0)
                {
                    colender.SanctionAmount = request.Request.newOfferAmout ?? 0;
                    _context.Entry(colender).State = EntityState.Modified;
                }
                var leadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == lead.Id
                                    && !x.IsCompleted && x.IsActive && !x.IsDeleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();

                if (leadActivity != null)
                {
                    leadActivity.IsCompleted = true;
                    leadActivity.IsApproved = 1;
                    _context.Entry(leadActivity).State = EntityState.Modified;
                }
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    gRPCReply.Status = true;
                    gRPCReply.Response = new UpdateLeadOfferResponse { LeadId = lead.Id, UserId = lead.UserName };
                    gRPCReply.Message = "Lead Offer updated successfully on Lead. ";


                    //------------------S : Make log---------------------
                    #region Make History
                    var result = await _leadHistoryManager.GetLeadHistroy(leadoffer.LeadId, "UpdateLeadOffers");
                    LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                    {
                        LeadId = leadoffer.LeadId,
                        UserID = result.UserId,
                        UserName = "",
                        EventName = "LeadOffers Approved/Reject",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                        Narretion = result.Narretion,
                        NarretionHTML = result.NarretionHTML,
                        CreatedTimeStamp = result.CreatedTimeStamp
                    };
                    await _massTransitService.Publish(histroyEvent);
                    #endregion
                    //------------------E : Make log---------------------

                }

            }
            return gRPCReply;

        }

        public async Task<List<LeadActivityHistoryDc>> LeadActivityHistory(GRPCRequest<long> request)
        {
            List<LeadActivityHistoryDc> leadActivityHistoryDc = new List<LeadActivityHistoryDc>();
            var Lead_Id = new SqlParameter("LeadId", request.Request);
            leadActivityHistoryDc = await _context.Database.SqlQueryRaw<LeadActivityHistoryDc>("exec GetLeadActivityHistory  @LeadId", Lead_Id).ToListAsync();
            return leadActivityHistoryDc;

        }

        public async Task<GRPCReply<string>> ResetLeadActivityMasterProgresse(GRPCRequest<long> LeadId)
        {
            var LeadActivityMaster = _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == LeadId.Request && x.IsActive && !x.IsDeleted).ToList();
            var lead = _context.Leads.FirstOrDefault(x => x.Id == LeadId.Request);
            if (LeadActivityMaster != null && lead != null)
            {
                foreach (var item in LeadActivityMaster)
                {
                    item.IsApproved = 0;
                    item.IsCompleted = false;
                    _context.Entry(item).State = EntityState.Modified;
                }
                _context.SaveChanges();
                return new GRPCReply<string> { Response = lead.UserName, Status = true, Message = "" };

            }
            else
            {
                return new GRPCReply<string> { Response = "", Status = false, Message = "" };
            }
        }

        public async Task<GRPCReply<AgreementResponseDc>> ArthmateGenerateAgreement(GRPCRequest<ArthmateAgreementRequest> request)
        {
            GRPCReply<AgreementResponseDc> result = new GRPCReply<AgreementResponseDc>();
            AgreementResponseDc agreementResponseDc = new AgreementResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
            if (nbfcService != null)
            {
                var res = await nbfcService.LBABusinessLoan(request.Request.LeadId, request.Request.AgreementURL, request.Request.IsSubmit, request.Request.ProductCompanyConfig);
                if (!string.IsNullOrEmpty(res.Htmldata))
                {
                    //var loanconfig = _context.LoanConfiguration.FirstOrDefault(x => x.IsActive && !x.IsDeleted);
                    agreementResponseDc.HtmlContent = res.Htmldata;
                    agreementResponseDc.IseSignEnable = res.IseSignEnable;
                    result.Response = agreementResponseDc;
                    result.Status = true;
                    result.Message = "Data Found";
                }
                else
                {
                    result.Status = false;
                    result.Message = "Data Not Found";
                }
            }
            return result;
        }


        //new for Esign
        //EsignGenerateAgreement

        public async Task<GRPCReply<string>> EsignGenerateAgreement(GRPCRequest<EsignAgreementRequest> request)
        {
            GRPCReply<string> result = new GRPCReply<string>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
            if (nbfcService != null)
            {
                // result.Response = await nbfcService.AgreementEsign(request.Request.LeadId, request.Request.AgreementURL, request.Request.IsSubmit);
                result.Response = await nbfcService.AgreementEsign(request);
                if (!string.IsNullOrEmpty(result.Response))
                {
                    result.Status = true;
                    result.Message = "Data Found";
                }
                else
                {
                    result.Status = false;
                    result.Message = "Data Not Found";
                }
            }
            return result;

        }

        public async Task<GRPCReply<GetAnchoreProductConfigResponseDc>> GetProductCompanyIdByLeadId(GRPCRequest<long> request)
        {
            GRPCReply<GetAnchoreProductConfigResponseDc> reply = new GRPCReply<GetAnchoreProductConfigResponseDc>();
            List<GetAnchoreProductConfigResponseDc> list = new List<GetAnchoreProductConfigResponseDc>();
            reply.Status = false;
            reply.Message = "";

            var leads = await _context.Leads.Where(x => x.Id == request.Request && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();

            if (leads != null && leads != null)
            {
                GetAnchoreProductConfigResponseDc getAnchoreProductConfigResponseDc = new GetAnchoreProductConfigResponseDc();
                getAnchoreProductConfigResponseDc.ProductId = leads.ProductId;
                reply.Response = getAnchoreProductConfigResponseDc;
                reply.Status = true;
                reply.Message = "";
            }
            return reply;
        }

        public async Task<GRPCReply<leadDashboardResponseDc>> LeadDashboardDetails(leadDashboardDetailDc req)
        {
            GRPCReply<leadDashboardResponseDc> reply = new GRPCReply<leadDashboardResponseDc>();
            var CityId = new DataTable();
            CityId.Columns.Add("IntValue");
            foreach (var id in req.CityId)
            {
                var dr = CityId.NewRow();
                dr["IntValue"] = id;
                CityId.Rows.Add(dr);
            }
            var allCityIds = new SqlParameter
            {
                ParameterName = "CityId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = CityId
            };
            var AnchorId = new DataTable();
            AnchorId.Columns.Add("IntValue");
            foreach (var id in req.AnchorId)
            {
                var dr = AnchorId.NewRow();
                dr["IntValue"] = id;
                AnchorId.Rows.Add(dr);
            }
            var allAnchIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = AnchorId
            };
            var FromDate = new SqlParameter("FromDate", req.FromDate);
            var ToDate = new SqlParameter("ToDate", req.ToDate);
            var ProductId = new SqlParameter("ProductId", req.ProductId);
            var ProductType = new SqlParameter("ProductType", req.ProductType);
            var LeadCount = _context.Database.SqlQueryRaw<leadDashboardData>("exec ScaleupleadDashboardDetails @CityId, @AnchorId, @FromDate, @ToDate,@ProductId", allCityIds, allAnchIds, FromDate, ToDate, ProductId).AsEnumerable().FirstOrDefault();
            if (LeadCount != null)
            {
                var AccountRawData = _context.Database.SqlQueryRaw<AccountResponseDc>("exec AccountDashboard @CityId, @AnchorId, @FromDate, @ToDate,@ProductId,@ProductType", allCityIds, allAnchIds, FromDate, ToDate, ProductId, ProductType).AsEnumerable().ToList();
                var totalDisbursementByUMRN = _context.BusinessLoanNBFCUpdate.Where(l => l.IsActive && !l.IsDeleted).ToList();
                var DisbursementApprove = _context.LeadActivityMasterProgresses.Where(l => l.ActivityMasterName.Equals(ActivityConstants.Congratulations) && l.IsCompleted && l.IsActive && !l.IsDeleted).ToList();
                reply.Response = new leadDashboardResponseDc
                {
                    Approved = LeadCount.Approved,
                    ApprovalPercentage = LeadCount.ApprovalPercentage,
                    NotContactable = LeadCount.NotContactable,
                    NotIntrested = LeadCount.NotIntrested,
                    Pending = LeadCount.Pending,
                    Rejected = LeadCount.Rejected,
                    TotalLeads = LeadCount.TotalLeads,
                    WholeDays = LeadCount.WholeDays,
                    RemainingHours = LeadCount.RemainingHours,
                    AccountRowDataDC = AccountRawData != null ? AccountRawData : new List<AccountResponseDc>(),
                    DisbursementApprove = DisbursementApprove.Count(),
                    totalDisbursementByUMRN = totalDisbursementByUMRN.Count(),
                    DisbursementRejected = totalDisbursementByUMRN.Where(x => !string.IsNullOrEmpty(x.NBFCRemark)).ToList().Count()
                };
                reply.Status = true;
                reply.Message = "Data Found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "Data Not Found";
            }
            return reply;
        }

        public async Task<GRPCReply<List<long>>> GetLeadCityList()
        {
            GRPCReply<List<long>> reply = new GRPCReply<List<long>>();
            reply.Status = false;
            reply.Message = "Data not Found";

            var distinctCityIds = await _context.Leads.Where(lead => lead.CityId != null).Select(lead => lead.CityId.Value).Distinct().ToListAsync();

            if (distinctCityIds != null && distinctCityIds.Any())
            {
                reply.Response = distinctCityIds;
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }

        public async Task<GRPCReply<List<leadExportDc>>> LeadExport(leadDashboardDetailDc req)
        {
            GRPCReply<List<leadExportDc>> reply = new GRPCReply<List<leadExportDc>>();

            var CityId = new DataTable();
            CityId.Columns.Add("IntValue");
            foreach (var id in req.CityId)
            {
                var dr = CityId.NewRow();
                dr["IntValue"] = id;
                CityId.Rows.Add(dr);
            }
            var allCityIds = new SqlParameter
            {
                ParameterName = "CityId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = CityId
            };
            var AnchorId = new DataTable();
            AnchorId.Columns.Add("IntValue");
            foreach (var id in req.AnchorId)
            {
                var dr = AnchorId.NewRow();
                dr["IntValue"] = id;
                AnchorId.Rows.Add(dr);
            }
            var allAnchIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = AnchorId
            };
            var FromDate = new SqlParameter("FromDate", req.FromDate);
            var ToDate = new SqlParameter("ToDate", req.ToDate);
            var ProductId = new SqlParameter("ProductId", req.ProductId);
            var LeadDetails = _context.Database.SqlQueryRaw<leadExportDc>("exec LeadDetailExport @CityId, @AnchorId, @FromDate, @ToDate,@ProductId", allCityIds, allAnchIds, FromDate, ToDate, ProductId).AsEnumerable().ToList();

            if (LeadDetails != null)
            {
                reply.Status = true;
                reply.Message = "Data Found";
                reply.Response = LeadDetails;
            }

            reply.Status = false;
            reply.Message = "Data not found";
            return reply;
        }

        //public async Task<GRPCReply<List<LeadLoanDetailsResponseDc>>> GetLeadLoanDetails(GRPCRequest<List<long>> request)
        //{
        //    GRPCReply<List<LeadLoanDetailsResponseDc>> reply = new GRPCReply<List<LeadLoanDetailsResponseDc>>();
        //    reply.Status = false;
        //    reply.Message = "Data not Found";

        //    var leadLoanData = await _context.LeadLoan.Where(x => request.Request.Contains(x.LeadMasterId.Value)).Select(y => new LeadLoanDetailsResponseDc
        //    {
        //        LeadId = y.LeadMasterId,
        //        Loan_app_id = y.loan_app_id,
        //        Partner_loan_app_id = y.partner_loan_app_id
        //    }).ToListAsync();

        //    if (leadLoanData != null && leadLoanData.Any())
        //    {
        //        reply.Response = leadLoanData;
        //        reply.Status = true;
        //        reply.Message = "Data Found";
        //    }
        //    return reply;
        //}

        //public async Task<GRPCReply<bool>> AddLoanConfiguration(GRPCRequest<AddLoanConfigurationDc> request)
        //{
        //    GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
        //    if (request.Request != null)
        //    {
        //        var existing = await _context.LoanConfiguration.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted);
        //        if (existing != null)
        //        {
        //            existing.BounceCharge = request.Request.BounceCharge ?? existing.BounceCharge;
        //            existing.GST = request.Request.GST ?? existing.GST;
        //            // existing.InterestRate = request.Request.InterestRate ?? existing.InterestRate;
        //            existing.PenalPercent = request.Request.PenalPercent ?? existing.PenalPercent;
        //            existing.PF = request.Request.PF ?? existing.PF;
        //            existing.ODCharges = request.Request.ODCharges ?? existing.ODCharges;
        //            existing.ODdays = request.Request.ODdays ?? existing.ODdays;
        //            _context.Entry(existing).State = EntityState.Modified;
        //        }
        //        else
        //        {
        //            _context.LoanConfiguration.Add(new LoanConfiguration
        //            {
        //                BounceCharge = request.Request.BounceCharge ?? 0,
        //                GST = request.Request.GST ?? 0,
        //                InterestRate = request.Request.InterestRate ?? 0,
        //                PenalPercent = request.Request.PenalPercent ?? 0,
        //                PF = request.Request.PF ?? 0,
        //                ODCharges = request.Request.ODCharges ?? 0,
        //                ODdays = request.Request.ODdays ?? 0,
        //                MaxInterestRate = request.Request.InterestRate ?? 0
        //            });
        //        }
        //        _context.SaveChanges();
        //    }
        //    gRPCReply.Status = true;
        //    return gRPCReply;
        //}

        public async Task<GRPCReply<StampRemainderEmailReply>> GetStampRemainderEmailData()
        {
            GRPCReply<StampRemainderEmailReply> gRPCReply = new GRPCReply<StampRemainderEmailReply>();
            var stamp = await _context.ArthmateSlaLbaStampDetail.Where(x => x.LeadmasterId == 0 && (x.IsStampUsed == false) && x.IsActive && !x.IsDeleted).CountAsync();
            var emailData = await _context.EmailRecipients.FirstOrDefaultAsync(x => x.EmailType == EmailTypeConstants.StampRemainder && x.IsActive && !x.IsDeleted);
            if (emailData != null && stamp <= emailData.LimitValue && !string.IsNullOrEmpty(emailData.To))
            {
                gRPCReply.Response = new StampRemainderEmailReply
                {
                    To = emailData.To,
                    Bcc = emailData.Bcc,
                    From = emailData.From,
                    StampCount = stamp
                };
                gRPCReply.Status = true;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<SCAccountResponseDc>>> GetSCAccountList(SCAccountRequestDC obj)
        {
            GRPCReply<List<SCAccountResponseDc>> gRPCReply = new GRPCReply<List<SCAccountResponseDc>>();
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var cityname = new SqlParameter("CityName", obj.CityName ?? (object)DBNull.Value);
            var keyword = new SqlParameter("Keyword", obj.Keyword.Trim() ?? (object)DBNull.Value);
            //var min = new SqlParameter("Min", obj.Min);
            //var max = new SqlParameter("Max", obj.Max);
            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);
            var anchorId = new SqlParameter("AnchorId", obj.AnchorId);
            var status = new SqlParameter("Status", obj.Status);
            var cityId = new SqlParameter("CityId", obj.CityId);

            var list = _context.Database.SqlQueryRaw<SCAccountResponseDc>(" exec GetSCAccountList @Fromdate,@ToDate,@CityName,@Keyword,@Skip,@Take,@AnchorId,@Status,@CityId", fromdate, toDate, cityname, keyword, skip, take, anchorId, status, cityId).AsEnumerable().ToList();
            if (list.Any())
            {
                gRPCReply.Response = list;
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
            }
            else
            {
                gRPCReply.Response = new List<SCAccountResponseDc>();
                gRPCReply.Status = false;
                gRPCReply.Message = "Not Found";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<List<BLAccountResponseDC>>> GetBLAccountList(BLAccountRequestDc obj)
        {
            GRPCReply<List<BLAccountResponseDC>> gRPCReply = new GRPCReply<List<BLAccountResponseDC>>();
            var leadis = new List<long>();

            if (obj.IsDSA)
            {
                if (obj.UserIds != null && obj.UserIds.Any())
                {
                    leadis = await _context.Leads.Where(x => x.CreatedBy != null && obj.UserIds.Contains(x.CreatedBy)).Select(x => x.Id).ToListAsync();
                }
            }
            else
            {
                leadis = await _context.Leads.Where(x => x.CreatedBy == null || x.UserName == x.CreatedBy).Select(x => x.Id).ToListAsync();
            }
            if (leadis != null && leadis.Any())
            {
                //if (!string.IsNullOrEmpty(obj.Role) && obj.Role.ToLower() == UserRoleConstants.MASOperationExecutive.ToLower())
                //{
                //    leadis = await _context.LeadOffers.Where(x => leadis.Any(y => y == x.LeadId) && x.CompanyIdentificationCode == CompanyIdentificationCodeConstants.MASFIN && x.IsActive && !x.IsDeleted).Select(x => x.LeadId).ToListAsync();
                //}
                //else if (!string.IsNullOrEmpty(obj.Role) && obj.Role.ToLower() == UserRoleConstants.AYEOperationExecutive.ToLower())
                //{
                //    leadis = await _context.LeadOffers.Where(x => leadis.Any(y => y == x.LeadId) && x.CompanyIdentificationCode == CompanyIdentificationCodeConstants.AYEFIN && x.IsActive && !x.IsDeleted).Select(x => x.LeadId).ToListAsync();
                //}
                //else if (!string.IsNullOrEmpty(obj.Role) && obj.Role == "BothRole")
                //{
                //    leadis = await _context.LeadOffers.Where(x => leadis.Any(y => y == x.LeadId) && (x.CompanyIdentificationCode == CompanyIdentificationCodeConstants.MASFIN || x.CompanyIdentificationCode == CompanyIdentificationCodeConstants.AYEFIN) && x.IsActive && !x.IsDeleted).Select(x => x.LeadId).ToListAsync();
                //}
                if (obj.UserType.ToLower() != UserTypeConstants.AdminUser.ToLower() && obj.UserType.ToLower() != UserTypeConstants.SuperAdmin.ToLower())
                {
                    leadis = await _context.LeadOffers.Where(x => leadis.Any(y => y == x.LeadId) && x.NBFCCompanyId == obj.NbfcCompanyId && x.IsActive && !x.IsDeleted).Select(x => x.LeadId).ToListAsync();
                }
                else
                {
                    leadis = await _context.LeadOffers.Where(x => leadis.Any(y => y == x.LeadId) && x.IsActive && !x.IsDeleted).Select(x => x.LeadId).ToListAsync();
                }
            }



            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var cityname = new SqlParameter("CityName", obj.CityName);
            var keyword = new SqlParameter("Keyword", obj.Keyword);
            //var min = new SqlParameter("Min", obj.Min);
            //var max = new SqlParameter("Max", obj.Max);
            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);
            //var anchorId = new SqlParameter("AnchorId", obj.AnchorId);
            var status = new SqlParameter("Status", obj.Status);
            var cityId = new SqlParameter("CityId", obj.CityId);

            var isdsa = new SqlParameter("isDSA", obj.IsDSA);
            var leadIds = new DataTable();
            leadIds.Columns.Add("IntValue");
            foreach (var lead in leadis)
            {
                var dr = leadIds.NewRow();
                dr["IntValue"] = lead;
                leadIds.Rows.Add(dr);
            }
            var LeadIds = new SqlParameter
            {
                ParameterName = "leadIds",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = leadIds
            };

            var anchorIds = new DataTable();
            anchorIds.Columns.Add("IntValue");
            foreach (var anchor in obj.AnchorId)
            {
                var dr = anchorIds.NewRow();
                dr["IntValue"] = anchor;
                anchorIds.Rows.Add(dr);
            }
            var AnchorIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = anchorIds
            };

            var list = _context.Database.SqlQueryRaw<BLAccountResponseDC>(" exec BLAccountList @Fromdate,@ToDate,@CityName,@Keyword,@Skip,@Take,@AnchorId,@Status,@CityId,@leadIds,@isDSA", fromdate, toDate, cityname, keyword, skip, take, AnchorIds, status, cityId, LeadIds, isdsa).AsEnumerable().ToList();
            if (list.Any())
            {
                if (obj.NbfcCompanyId > 0)
                {
                    list = list.Where(x => x.NbfcCompanyId == obj.NbfcCompanyId).ToList();
                }

                //if (!string.IsNullOrEmpty(obj.Role) && obj.Role.ToLower() == UserRoleConstants.MASOperationExecutive.ToLower())
                //{
                //    list = list.Where(x => x.NBFCname.ToLower() == CompanyIdentificationCodeConstants.MASFIN.ToLower()).ToList();
                //}
                //else if (!string.IsNullOrEmpty(obj.Role) && obj.Role.ToLower() == UserRoleConstants.AYEOperationExecutive.ToLower())
                //{
                //    list = list.Where(x => x.NBFCname.ToLower() == CompanyIdentificationCodeConstants.AYEFIN.ToLower()).ToList();
                //}
                //else if (!string.IsNullOrEmpty(obj.Role) && obj.Role == "BothRole")
                //{
                //    list = list.Where(x => x.NBFCname.ToLower() == CompanyIdentificationCodeConstants.MASFIN.ToLower() || x.NBFCname.ToLower() == CompanyIdentificationCodeConstants.AYEFIN.ToLower()).ToList();
                //}
                gRPCReply.Response = list;
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
            }
            else
            {
                gRPCReply.Response = new List<BLAccountResponseDC>();
                gRPCReply.Status = false;
                gRPCReply.Message = "Not Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> request)
        {
            GRPCReply<string> result = new GRPCReply<string>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
            if (nbfcService != null)
            {
                result = await nbfcService.GetOfferEmiDetailsDownloadPdf(request);
                if (!string.IsNullOrEmpty(result.Response))
                {
                    result.Status = true;
                    result.Message = "Data Found";
                }
                else
                {
                    result.Status = false;
                    result.Message = "Data Not Found";
                }
            }
            return result;
        }

        public async Task<GRPCReply<bool>> AddUpdateBuisnessDetail(GRPCRequest<UpdateBuisnessDetailRequest> request)
        {
            GRPCReply<bool> result = new GRPCReply<bool>();
            var BuisnessDetail = await _context.BusinessDetails.Where(x => x.LeadId == request.Request.LeadMasterId && x.IsDeleted == false && x.IsActive == true).FirstOrDefaultAsync();
            if (BuisnessDetail != null)
            {
                BuisnessDetail.BusEntityType = request.Request.BusEntityType;
                BuisnessDetail.BuisnessMonthlySalary = request.Request.BuisnessMonthlySalary;
                BuisnessDetail.BusGSTNO = request.Request.BusGSTNO;
                BuisnessDetail.BusinessName = request.Request.BusName;
                BuisnessDetail.DOI = DateFormatReturn(request.Request.DOI);
                BuisnessDetail.IncomeSlab = request.Request.IncomeSlab;
                BuisnessDetail.InquiryAmount = request.Request.InquiryAmount;
                if (request.Request.AddressId != request.Request.CurrentAddressId)
                {
                    BuisnessDetail.AddressLineOne = request.Request.BusAddCorrLine1;
                    BuisnessDetail.AddressLineTwo = request.Request.BusAddCorrLine2;
                    BuisnessDetail.ZipCode = int.Parse(request.Request.BusAddCorrPincode);
                    BuisnessDetail.CityName = request.Request.BusAddCorrCity;
                }
                BuisnessDetail.IsActive = true;
                BuisnessDetail.IsDeleted = false;
                BuisnessDetail.LastModified = DateTime.Now;
                BuisnessDetail.LastModifiedBy = request.Request.UserId;
                _context.Entry(BuisnessDetail).State = EntityState.Modified;
            }
            _context.SaveChanges();
            return result;
        }

        public async Task<GRPCReply<bool>> UploadLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailRequest> leadDocumentDetailDTO)
        {
            GRPCReply<bool> result = new GRPCReply<bool>();
            #region insert Lead Document Details
            var existingleadDocDetails = await _context.LeadDocumentDetails.Where(x => x.LeadId == leadDocumentDetailDTO.Request.LeadId && x.DocumentName == leadDocumentDetailDTO.Request.DocumentName && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            var leadBankDetail = await _context.LeadBankDetails.Where(x => x.LeadId == leadDocumentDetailDTO.Request.LeadId && x.IsActive).ToListAsync();
            if (leadBankDetail != null && leadBankDetail.Any())
            {
                foreach (var item in leadBankDetail)
                {
                    item.PdfPassword = leadDocumentDetailDTO.Request.PdfPassword;
                    _context.Entry(item).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
            }
            if (existingleadDocDetails != null)
            {
                existingleadDocDetails.IsActive = false;
                existingleadDocDetails.IsDeleted = true;
                _context.Entry(existingleadDocDetails).State = EntityState.Modified;


                List<LeadDocumentDetail> leadDocumentDetails = new List<LeadDocumentDetail>();
                if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == "gst_certificate")
                {

                    leadDocumentDetails.Add(new LeadDocumentDetail
                    {
                        DocumentName = BlackSoilBusinessDocNameConstants.GstCertificate,
                        DocumentNumber = existingleadDocDetails.DocumentNumber,
                        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                        FileUrl = leadDocumentDetailDTO.Request.FileUrl,
                        LeadId = leadDocumentDetailDTO.Request.LeadId,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
                if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == "other")
                {
                    leadDocumentDetails.Add(new LeadDocumentDetail
                    {
                        DocumentName = BlackSoilBusinessDocNameConstants.Other,
                        DocumentNumber = string.IsNullOrEmpty(existingleadDocDetails.DocumentNumber) ? "" : existingleadDocDetails.DocumentNumber,
                        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                        FileUrl = leadDocumentDetailDTO.Request.FileUrl,
                        LeadId = leadDocumentDetailDTO.Request.LeadId,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
                if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == "udyog_aadhaar")
                {
                    leadDocumentDetails.Add(new LeadDocumentDetail
                    {
                        DocumentName = BlackSoilBusinessDocNameConstants.UdyogAadhaar,
                        DocumentNumber = existingleadDocDetails.DocumentNumber ?? "",
                        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                        FileUrl = leadDocumentDetailDTO.Request.FileUrl,
                        LeadId = leadDocumentDetailDTO.Request.LeadId,
                        IsActive = true,
                        IsDeleted = false
                    });
                }

                if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == "bank_statement")
                {
                    leadDocumentDetails.Add(new LeadDocumentDetail
                    {
                        DocumentName = BlackSoilBusinessDocNameConstants.Statement,
                        DocumentNumber = existingleadDocDetails.DocumentNumber,
                        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                        FileUrl = leadDocumentDetailDTO.Request.FileUrl,
                        LeadId = leadDocumentDetailDTO.Request.LeadId,
                        IsActive = true,
                        IsDeleted = false,
                        PdfPassword = leadDocumentDetailDTO.Request.PdfPassword
                    });
                }

                if (leadDocumentDetails != null && leadDocumentDetails.Any())
                {
                    _context.LeadDocumentDetails.AddRange(leadDocumentDetails);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                result.Status = false;
                result.Message = "Data not found in LeadDocuments!!";
                result.Response = false;
            }
            #endregion

            return result;
        }
        public async Task<GRPCReply<bool>> UploadMultiLeadDocuments(GRPCRequest<UpdateLeadDocumentDetailListRequest> leadDocumentDetailDTO)
        {
            GRPCReply<bool> result = new GRPCReply<bool>();
            #region insert Lead Document Details
            if (leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate || leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.Other || leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar)
            {
                if (leadDocumentDetailDTO.Request.DocList.Count > 1)
                {
                    result.Status = false;
                    result.Message = "Multiple Document Can not be upload for " + leadDocumentDetailDTO.Request.DocumentName;
                    result.Response = false;
                    return result;
                }
            }
            var existingleadDocDetails = await _context.LeadDocumentDetails.Where(x => x.LeadId == leadDocumentDetailDTO.Request.LeadId && x.DocumentName == leadDocumentDetailDTO.Request.DocumentName && x.IsActive && !x.IsDeleted).ToListAsync();
            var leadBankDetail = await _context.LeadBankDetails.Where(x => x.LeadId == leadDocumentDetailDTO.Request.LeadId && x.IsActive).ToListAsync();
            if (leadBankDetail != null && leadBankDetail.Any())
            {
                foreach (var item in leadBankDetail)
                {
                    item.PdfPassword = leadDocumentDetailDTO.Request.PdfPassword;
                    _context.Entry(item).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
            }
            if (existingleadDocDetails != null && existingleadDocDetails.Count > 0)
            {
                foreach (var existingLeadDocs in existingleadDocDetails)
                {
                    existingLeadDocs.IsActive = false;
                    existingLeadDocs.IsDeleted = true;
                    _context.Entry(existingLeadDocs).State = EntityState.Modified;
                }
                List<LeadDocumentDetail> leadDocumentDetails = new List<LeadDocumentDetail>();
                foreach (var doc in leadDocumentDetailDTO.Request.DocList)
                {
                    string documentNumber = "";

                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName))
                    {
                        documentNumber = existingleadDocDetails.FirstOrDefault(doc => doc.DocumentName == leadDocumentDetailDTO.Request.DocumentName)?.DocumentNumber;
                    }

                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.GstCertificate,
                            //DocumentNumber = existingLeadDocs.DocumentNumber,
                            DocumentNumber = documentNumber,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.Other)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.Other,
                            DocumentNumber = documentNumber,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.UdyogAadhaar,
                            DocumentNumber = documentNumber ?? "",
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.Statement)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.Statement,
                            DocumentNumber = documentNumber,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false,
                            PdfPassword = leadDocumentDetailDTO.Request.PdfPassword
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateGstCertificate)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.SurrogateGstCertificate,
                            DocumentNumber = documentNumber,
                            DocumentType = BlackSoilBusinessDocTypeConstants.SurrogateProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false,
                            PdfPassword = leadDocumentDetailDTO.Request.PdfPassword
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.SurrogateITRCertificate)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.SurrogateITRCertificate,
                            DocumentNumber = documentNumber,
                            DocumentType = BlackSoilBusinessDocTypeConstants.SurrogateProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false,
                            PdfPassword = leadDocumentDetailDTO.Request.PdfPassword
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.BusinessPhotos)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = leadDocumentDetailDTO.Request.DocumentName,
                            DocumentNumber = documentNumber ?? "",
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                }
                if (leadDocumentDetails != null && leadDocumentDetails.Any())
                {
                    _context.LeadDocumentDetails.AddRange(leadDocumentDetails);
                }
                await _context.SaveChangesAsync();

            }
            else if (leadDocumentDetailDTO.Request.DocumentName != BlackSoilBusinessDocNameConstants.SurrogateITRCertificate || leadDocumentDetailDTO.Request.DocumentName != BlackSoilBusinessDocNameConstants.SurrogateGstCertificate || leadDocumentDetailDTO.Request.DocumentName != BlackSoilBusinessDocNameConstants.Statement)
            {
                List<LeadDocumentDetail> leadDocumentDetails = new List<LeadDocumentDetail>();
                foreach (var doc in leadDocumentDetailDTO.Request.DocList)
                {
                    string documentNumber = "";

                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.GstCertificate,
                            //DocumentNumber = existingLeadDocs.DocumentNumber,
                            DocumentNumber = documentNumber,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.Other)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.Other,
                            DocumentNumber = documentNumber,
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = BlackSoilBusinessDocNameConstants.UdyogAadhaar,
                            DocumentNumber = documentNumber ?? "",
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == "Address Proof")
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = "Address Proof",
                            DocumentNumber = documentNumber ?? "",
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }

                    if (!string.IsNullOrEmpty(leadDocumentDetailDTO.Request.DocumentName) && leadDocumentDetailDTO.Request.DocumentName == BlackSoilBusinessDocNameConstants.BusinessPhotos)
                    {
                        leadDocumentDetails.Add(new LeadDocumentDetail
                        {
                            DocumentName = leadDocumentDetailDTO.Request.DocumentName,
                            DocumentNumber = documentNumber ?? "",
                            DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                            FileUrl = doc.FilePath,
                            LeadId = leadDocumentDetailDTO.Request.LeadId,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                }
                if (leadDocumentDetails != null && leadDocumentDetails.Any())
                {
                    _context.LeadDocumentDetails.AddRange(leadDocumentDetails);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                result.Status = false;
                result.Message = "Data not found in LeadDocuments!!";
                result.Response = false;
            }
            #endregion

            return result;
        }

        public DateTime DateFormatReturn(string sdate)
        {
            DateTime date;
            string[] formats = { "dd/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "d/MM/yyyy",
                                "dd/MM/yy", "dd/M/yy", "d/M/yy", "d/MM/yy", "MM/dd/yyyy",

            "dd-MM-yyyy", "dd-M-yyyy", "d-M-yyyy", "d-MM-yyyy",
                                "dd-MM-yy", "dd-M-yy", "d-M-yy", "d-MM-yy", "MM-dd-yyyy"
                                , "yyyy-MM-dd", "yyyy/MM/dd"
            };

            DateTime.TryParseExact(sdate, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);
            return date;
        }

        public async Task<GRPCReply<List<LeadDocumentDetailReply>>> GetLeadDocumentsByLeadId(GRPCRequest<long> request)
        {
            GRPCReply<List<LeadDocumentDetailReply>> reply = new GRPCReply<List<LeadDocumentDetailReply>>();
            #region get Lead Document Details
            //var leadActivityOfferStatus = await GetLeadActivityOfferStatus(request);
            //if (leadActivityOfferStatus != null && leadActivityOfferStatus.Status)
            //{
            reply.Response = await _context.LeadDocumentDetails.Where(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted).Select(x => new LeadDocumentDetailReply
            {
                DocumentName = x.DocumentName,
                DocumentNumber = x.DocumentNumber,
                LeadId = x.LeadId,
                FileUrl = new List<string> { x.FileUrl },
                PdfPassword = x.PdfPassword,
                LeadDocDetailId = x.Id,
                Sequence = x.Sequence
            }).OrderByDescending(x => x.LeadDocDetailId).ToListAsync();
            //}
            //else
            //{
            //    reply.Response = await _context.LeadDocumentDetails.Where(x => x.LeadId == request.Request && x.DocumentName == "bank_statement" && x.IsActive && !x.IsDeleted).Select(x => new LeadDocumentDetailReply
            //    {
            //        DocumentName = x.DocumentName,
            //        DocumentNumber = x.DocumentNumber,
            //        LeadId = x.LeadId,
            //        FileUrl = x.FileUrl,
            //        PdfPassword = x.PdfPassword,
            //        LeadDocDetailId = x.Id,
            //        Sequence = x.Sequence
            //    }).OrderByDescending(x => x.LeadDocDetailId).ToListAsync();

            //}
            if (reply.Response != null && reply.Response.Count > 0)
            {
                reply.Status = true;
                reply.Message = "Data Found!";
            }
            else
            {
                reply.Status = false;
                reply.Message = "No Data Found!";
            }
            #endregion
            return reply;
        }

        public async Task<GRPCReply<bool>> GetLeadActivityOfferStatus(GRPCRequest<long> request)
        {
            GRPCReply<bool> result = new GRPCReply<bool>();
            var leadIdparam = new SqlParameter("leadId", request.Request);
            var response = await _context.Database.SqlQueryRaw<LeadActivityOfferCurrentStatus>("exec GetLeadActivityOfferStatus   @leadId", leadIdparam).ToListAsync();

            if (response.Count > 0)
            {
                result.Status = true;
            }
            else
            {
                result.Status = false;
            }
            return result;

        }
        public async Task<GRPCReply<List<CompanyBuyingHistoryTotalAmountResponseDc>>> GetCompanyBuyingHistory(GRPCRequest<CompanyBuyingHistoryRequestDc> request)
        {
            GRPCReply<List<CompanyBuyingHistoryTotalAmountResponseDc>> result = new GRPCReply<List<CompanyBuyingHistoryTotalAmountResponseDc>>();
            //long CompanyLeadId = 0;
            //var CompanyLeadData = await _context.CompanyLead.Where(x => x.CompanyId == request.Request.CompanyId && request.Request.LeadId.Contains(x.LeadId) && x.IsActive && !x.IsDeleted).Select(x => new CompanyBuyingHistoryDc { CompanyLeadId = x.Id ,LeadId = x.LeadId}).ToListAsync();
            //var companyBuyingHistoryListDTO = await _context.LeadCompanyBuyingHistorys.Where(y => y.CompanyLeadId == CompanyLeadData).Select(y => new CompanyBuyingHistoryDTO { MonthTotalAmount = y.MonthTotalAmount }).ToListAsync();

            var data = (from _companyLead in _context.CompanyLead
                        join req in request.Request.LeadId on _companyLead.LeadId equals req
                        join _leadCompanyBuyingHistorys in _context.LeadCompanyBuyingHistorys on _companyLead.Id equals _leadCompanyBuyingHistorys.CompanyLeadId
                        group _leadCompanyBuyingHistorys by new { _leadCompanyBuyingHistorys.CompanyLeadId, _companyLead.LeadId } into TotalSumAmountTable
                        select new CompanyBuyingHistoryTotalAmountResponseDc
                        {
                            LeadId = TotalSumAmountTable.Key.LeadId,
                            CompanyLeadId = TotalSumAmountTable.Key.CompanyLeadId,
                            MonthTotalAmount = TotalSumAmountTable.Sum(o => o.MonthTotalAmount)
                        }).ToList();

            return new GRPCReply<List<CompanyBuyingHistoryTotalAmountResponseDc>> { Status = true, Message = "", Response = data };


            //result.Result = companyBuyingHistoryListDTO;
            //if (companyBuyingHistoryListDTO.Count > 0)
            //{
            //    result.IsSuccess = true;
            //    result.Message = "";
            //}
            //else
            //{
            //    result.IsSuccess = false;
            //    result.Message = "Data Not Found!!";
            //}
            //return result;
        }

        #region DSA
        public async Task<GRPCReply<List<DSADashboardLeadResponse>>> GetDSALeadDashboardData(GRPCRequest<DSADashboardLeadRequest> request)
        {
            GRPCReply<List<DSADashboardLeadResponse>> gRPCReply = new GRPCReply<List<DSADashboardLeadResponse>> { Message = "Data not found" };
            if (request.Request.IsPagination)
            {
                var query = _context.Leads.Where(x => request.Request.AgentUserIds.Contains(x.CreatedBy)
                && x.ProductId == request.Request.ProductId &&
                (x.Created.Date >= request.Request.StartDate.Date && x.Created.Date <= request.Request.EndDate.Date) &&
                 x.IsActive && !x.IsDeleted);
                int totalRecords = query.Count();
                gRPCReply.Response = query.Select(x => new DSADashboardLeadResponse
                {
                    LeadId = x.Id,
                    Status = x.Status,
                    CreatedDate = x.Created,
                    FullName = x.ApplicantName,
                    LeadCode = x.LeadCode,
                    MobileNo = x.MobileNo,
                    ProfileImage = "",
                    AgentUserId = x.CreatedBy,
                    AgentFullName = "",
                    TotalRecords = totalRecords
                }).Skip(request.Request.Skip).Take(request.Request.Take).ToList();
            }
            else
            {
                gRPCReply.Response = await _context.Leads.Where(x => request.Request.AgentUserIds.Contains(x.CreatedBy) && x.ProductId == request.Request.ProductId &&
                                                               (x.Created.Date >= request.Request.StartDate.Date && x.Created.Date <= request.Request.EndDate.Date) &&
                                                                x.IsActive && !x.IsDeleted).Select(x => new DSADashboardLeadResponse
                                                                {
                                                                    LeadId = x.Id,
                                                                    Status = x.Status,
                                                                    CreatedDate = x.Created,
                                                                    FullName = x.ApplicantName,
                                                                    LeadCode = x.LeadCode,
                                                                    MobileNo = x.MobileNo,
                                                                    ProfileImage = "",
                                                                    AgentUserId = x.CreatedBy,
                                                                    AgentFullName = "",
                                                                    TotalRecords = 0
                                                                }).ToListAsync();
            }
            if (gRPCReply.Response != null && gRPCReply.Response.Any())
            {
                if (request.Request.IsPagination)
                {
                    gRPCReply.Response = (from res in gRPCReply.Response
                                          join personal in _context.PersonalDetails on res.LeadId equals personal.LeadId into p
                                          from personal in p.DefaultIfEmpty()
                                          select new DSADashboardLeadResponse
                                          {
                                              LeadId = res.LeadId,
                                              Status = res.Status,
                                              CreatedDate = res.CreatedDate,
                                              FullName = res.FullName,
                                              LeadCode = res.LeadCode,
                                              MobileNo = res.MobileNo,
                                              AgentUserId = res.AgentUserId,
                                              AgentFullName = res.AgentFullName,
                                              ProfileImage = personal != null ? personal.SelfieImageUrl ?? "" : "",
                                              TotalRecords = res.TotalRecords
                                          }).ToList();
                }

                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<GetDSALeadPayoutDetailsResponse>> GetDSALeadPayoutDetails(GRPCRequest<long> request)
        {
            GRPCReply<GetDSALeadPayoutDetailsResponse> reply = new GRPCReply<GetDSALeadPayoutDetailsResponse> { Message = "Data Not Found!!!" };
            var leadPayout = await _context.DSAPayouts.FirstOrDefaultAsync(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted);
            if (leadPayout != null)
            {
                reply.Response = new GetDSALeadPayoutDetailsResponse
                {
                    LeadId = leadPayout.LeadId,
                    PayoutPercentage = leadPayout.PayoutPercenatge
                };
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }

        public async Task<GRPCReply<bool>> UpdateLeadPersonalDetail(GRPCRequest<UserDetailsReply> request)
        {
            GRPCReply<bool> result = new GRPCReply<bool> { Message = "Failed" };
            long? LeadId = request.Request.LeadId;
            var allCompletedApproved = false;
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId && x.IsActive && !x.IsDeleted);

            var leadActivityProgresses = await _context.LeadActivityMasterProgresses
                    .Where(x => x.IsActive && !x.IsDeleted && x.LeadMasterId == LeadId).ToListAsync();

            var IsPayoutPercentageExist = await _context.DSAPayouts.FirstOrDefaultAsync(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted);
            if (IsPayoutPercentageExist == null)
            {
                result.Message = "Please Save Payout Percentage First";
                result.Status = false;
                return result;
            }

            if (leadActivityProgresses != null)
            {
                var requiredActivities = new[] { "KYC", "DSATypeSelection", "DSAPersonalInfo", "Bank Detail" };

                allCompletedApproved = requiredActivities.All(activity =>
                    leadActivityProgresses.Any(row =>
                        row.ActivityMasterName == activity && row.IsCompleted && row.IsApproved == 1));
                if (!allCompletedApproved)
                {
                    result.Message = "failed to update Details please approve all Activity";
                    result.Status = false;
                    return result;
                }
            }

            if (lead != null && lead.Status == LeadStatusEnum.Submitted.ToString() && allCompletedApproved && IsPayoutPercentageExist != null)
            {
                string[] name = request.Request.DSAPersonalDetail != null
                    ? request.Request.DSAPersonalDetail.FullName.ToString().Trim().Split(new char[] { ' ' }, 3)
                    : request.Request.ConnectorPersonalDetail.FullName.ToString().Trim().Split(new char[] { ' ' }, 3);

                string firstname = "", lastname = "", middlename = "";

                if (name[0] != null)
                {
                    firstname = name[0];
                }
                if (name.Length > 1 && name[1] != null)
                {
                    if (name.Length == 2)
                    {
                        lastname = name[1];
                    }
                    else
                    {
                        middlename = name[1];
                    }
                }
                if (name.Length > 2 && name[2] != null)
                {
                    lastname = name[2];
                }

                var PersonalDetail = await _context.PersonalDetails.Where(x => x.LeadId == LeadId && x.IsDeleted == false && x.IsActive == true).FirstOrDefaultAsync();
                if (PersonalDetail != null)
                {
                    if (request.Request.DSAPersonalDetail != null)
                    {
                        PersonalDetail.FirstName = firstname; //request.Request.panDetail != null ? request.Request.panDetail.NameOnCard : "";
                        PersonalDetail.MiddleName = middlename;//request.Request.PersonalDetail.MiddleName;
                        PersonalDetail.LastName = lastname; //request.Request.PersonalDetail.LastName != null ? request.Request.PersonalDetail.LastName : "";
                        PersonalDetail.AadhaarMaskNO = request.Request.aadharDetail.UniqueId;
                        PersonalDetail.PanMaskNO = request.Request.panDetail.UniqueId;
                        PersonalDetail.AlternatePhoneNo = request.Request.DSAPersonalDetail.AlternatePhoneNo;
                        PersonalDetail.DOB = request.Request.panDetail.DOB;
                        PersonalDetail.FatherName = request.Request.panDetail.FatherName;
                        PersonalDetail.FatherLastName = request.Request.DSAPersonalDetail.FatherOrHusbandName;
                        PersonalDetail.CurrentAddressLineOne = request.Request.DSAPersonalDetail.CurrentAddress.AddressLineOne;
                        PersonalDetail.CurrentAddressLineTwo = request.Request.DSAPersonalDetail.CurrentAddress.AddressLineTwo;
                        PersonalDetail.CurrentAddressLineThree = request.Request.DSAPersonalDetail.CurrentAddress.AddressLineThree;
                        PersonalDetail.CurrentCityName = request.Request.DSAPersonalDetail.CurrentAddress.CityName;
                        PersonalDetail.CurrentStateName = request.Request.DSAPersonalDetail.CurrentAddress.StateName;
                        PersonalDetail.CurrentZipCode = request.Request.DSAPersonalDetail.CurrentAddress.ZipCode;
                        PersonalDetail.CurrentCountryName = request.Request.DSAPersonalDetail.CurrentAddress.CountryName;
                        PersonalDetail.PermanentAddressLineOne = request.Request.DSAPersonalDetail.PermanentAddress.AddressLineOne;
                        PersonalDetail.PermanentAddressLineTwo = request.Request.DSAPersonalDetail.PermanentAddress.AddressLineTwo;
                        PersonalDetail.PermanentAddressLineThree = request.Request.DSAPersonalDetail.PermanentAddress.AddressLineThree;
                        PersonalDetail.PermanentCityName = request.Request.DSAPersonalDetail.PermanentAddress.CityName;
                        PersonalDetail.PermanentStateName = request.Request.DSAPersonalDetail.PermanentAddress.StateName;
                        PersonalDetail.PermanentZipCode = request.Request.DSAPersonalDetail.PermanentAddress.ZipCode;
                        PersonalDetail.PermanentCountryName = request.Request.DSAPersonalDetail.PermanentAddress.CountryName;
                        PersonalDetail.EmailId = request.Request.DSAPersonalDetail.EmailId;
                        PersonalDetail.Gender = request.Request.aadharDetail.Gender;
                        PersonalDetail.MobileNo = request.Request.DSAPersonalDetail.MobileNo;
                        PersonalDetail.AadharBackImage = request.Request.aadharDetail.BackImageUrl;
                        PersonalDetail.AadharFrontImage = request.Request.aadharDetail.FrontImageUrl;
                        PersonalDetail.PanFrontImage = request.Request.panDetail.FrontImageUrl;
                        PersonalDetail.PanNameOnCard = request.Request.panDetail.NameOnCard;
                        PersonalDetail.SelfieImageUrl = request.Request.SelfieDetail.FrontImageUrl;
                        PersonalDetail.Marital = "";
                        _context.Entry(PersonalDetail).State = EntityState.Modified;
                    }
                    else
                    {
                        PersonalDetail.FirstName = firstname; //request.Request.panDetail != null ? request.Request.panDetail.NameOnCard : "";
                        PersonalDetail.MiddleName = middlename;//request.Request.PersonalDetail.MiddleName;
                        PersonalDetail.LastName = lastname; //request.Request.PersonalDetail.LastName != null ? request.Request.PersonalDetail.LastName : "";
                        PersonalDetail.AadhaarMaskNO = request.Request.aadharDetail.UniqueId;
                        PersonalDetail.PanMaskNO = request.Request.panDetail.UniqueId;
                        PersonalDetail.AlternatePhoneNo = request.Request.ConnectorPersonalDetail.AlternateContactNo;
                        PersonalDetail.DOB = request.Request.ConnectorPersonalDetail.DOB;
                        PersonalDetail.FatherName = request.Request.ConnectorPersonalDetail.FatherName;
                        PersonalDetail.FatherLastName = request.Request.ConnectorPersonalDetail.FatherName;
                        PersonalDetail.CurrentAddressLineOne = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineOne;
                        PersonalDetail.CurrentAddressLineTwo = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineTwo;
                        PersonalDetail.CurrentAddressLineThree = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineThree;
                        PersonalDetail.CurrentCityName = request.Request.ConnectorPersonalDetail.CurrentAddress.CityName;
                        PersonalDetail.CurrentStateName = request.Request.ConnectorPersonalDetail.CurrentAddress.StateName;
                        PersonalDetail.CurrentZipCode = request.Request.ConnectorPersonalDetail.CurrentAddress.ZipCode;
                        PersonalDetail.CurrentCountryName = request.Request.ConnectorPersonalDetail.CurrentAddress.CountryName;
                        PersonalDetail.PermanentAddressLineOne = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineOne;
                        PersonalDetail.PermanentAddressLineTwo = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineTwo;
                        PersonalDetail.PermanentAddressLineThree = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineThree;
                        PersonalDetail.PermanentCityName = request.Request.ConnectorPersonalDetail.CurrentAddress.CityName;
                        PersonalDetail.PermanentStateName = request.Request.ConnectorPersonalDetail.CurrentAddress.StateName;
                        PersonalDetail.PermanentZipCode = request.Request.ConnectorPersonalDetail.CurrentAddress.ZipCode;
                        PersonalDetail.PermanentCountryName = request.Request.ConnectorPersonalDetail.CurrentAddress.CountryName;
                        PersonalDetail.EmailId = request.Request.ConnectorPersonalDetail.EmailId;
                        PersonalDetail.Gender = request.Request.aadharDetail.Gender;
                        PersonalDetail.MobileNo = request.Request.ConnectorPersonalDetail.MobileNo;
                        PersonalDetail.AadharBackImage = request.Request.aadharDetail.BackImageUrl;
                        PersonalDetail.AadharFrontImage = request.Request.aadharDetail.FrontImageUrl;
                        PersonalDetail.PanFrontImage = request.Request.panDetail.FrontImageUrl;
                        PersonalDetail.PanNameOnCard = request.Request.panDetail.NameOnCard;
                        PersonalDetail.SelfieImageUrl = request.Request.SelfieDetail.FrontImageUrl;
                        PersonalDetail.Marital = "";
                        _context.Entry(PersonalDetail).State = EntityState.Modified;
                    }
                }
                else
                {
                    if (request.Request.DSAPersonalDetail != null)
                    {
                        var AddPersonalDetail = new PersonalDetail
                        {
                            FirstName = firstname, //request.Request.panDetail != null ? request.Request.panDetail.NameOnCard : "",
                            MiddleName = middlename,//request.Request.PersonalDetail.MiddleName,
                            LastName = lastname, //request.Request.PersonalDetail.LastName != null ? request.Request.PersonalDetail.LastName : "",
                            AadhaarMaskNO = request.Request.aadharDetail.UniqueId,
                            PanMaskNO = request.Request.panDetail.UniqueId,
                            AlternatePhoneNo = request.Request.DSAPersonalDetail.AlternatePhoneNo,
                            DOB = request.Request.panDetail.DOB,
                            FatherName = request.Request.panDetail.FatherName,
                            FatherLastName = request.Request.DSAPersonalDetail.FatherOrHusbandName,
                            CurrentAddressLineOne = request.Request.DSAPersonalDetail.CurrentAddress.AddressLineOne,
                            CurrentAddressLineTwo = request.Request.DSAPersonalDetail.CurrentAddress.AddressLineTwo,
                            CurrentAddressLineThree = request.Request.DSAPersonalDetail.CurrentAddress.AddressLineThree,
                            CurrentCityName = request.Request.DSAPersonalDetail.CurrentAddress.CityName,
                            CurrentStateName = request.Request.DSAPersonalDetail.CurrentAddress.StateName,
                            CurrentZipCode = request.Request.DSAPersonalDetail.CurrentAddress.ZipCode,
                            CurrentCountryName = request.Request.DSAPersonalDetail.CurrentAddress.CountryName,
                            PermanentAddressLineOne = request.Request.DSAPersonalDetail.PermanentAddress.AddressLineOne,
                            PermanentAddressLineTwo = request.Request.DSAPersonalDetail.PermanentAddress.AddressLineTwo,
                            PermanentAddressLineThree = request.Request.DSAPersonalDetail.PermanentAddress.AddressLineThree,
                            PermanentCityName = request.Request.DSAPersonalDetail.PermanentAddress.CityName,
                            PermanentStateName = request.Request.DSAPersonalDetail.PermanentAddress.StateName,
                            PermanentZipCode = request.Request.DSAPersonalDetail.PermanentAddress.ZipCode,
                            PermanentCountryName = request.Request.DSAPersonalDetail.PermanentAddress.CountryName,
                            EmailId = request.Request.DSAPersonalDetail.EmailId,
                            Gender = request.Request.aadharDetail.Gender,
                            MobileNo = request.Request.DSAPersonalDetail.MobileNo,
                            AadharBackImage = request.Request.aadharDetail.BackImageUrl,
                            AadharFrontImage = request.Request.aadharDetail.FrontImageUrl,
                            PanFrontImage = request.Request.panDetail.FrontImageUrl,
                            PanNameOnCard = request.Request.panDetail.NameOnCard,
                            SelfieImageUrl = request.Request.SelfieDetail.FrontImageUrl,
                            Marital = "",
                            LeadId = request.Request.LeadId.Value,
                            IsActive = true,
                            IsDeleted = false,

                        };
                        await _context.PersonalDetails.AddAsync(AddPersonalDetail);
                    }
                    else
                    {
                        var AddPersonalDetail = new PersonalDetail
                        {
                            FirstName = firstname, //request.Request.panDetail != null ? request.Request.panDetail.NameOnCard : "",
                            MiddleName = middlename,//request.Request.PersonalDetail.MiddleName,
                            LastName = lastname, //request.Request.PersonalDetail.LastName != null ? request.Request.PersonalDetail.LastName : "",
                            AadhaarMaskNO = request.Request.aadharDetail.UniqueId,
                            PanMaskNO = request.Request.panDetail.UniqueId,
                            AlternatePhoneNo = request.Request.ConnectorPersonalDetail.AlternateContactNo,
                            DOB = request.Request.ConnectorPersonalDetail.DOB,
                            FatherName = request.Request.ConnectorPersonalDetail.FatherName,
                            FatherLastName = request.Request.ConnectorPersonalDetail.FatherName,
                            CurrentAddressLineOne = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineOne,
                            CurrentAddressLineTwo = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineTwo,
                            CurrentAddressLineThree = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineThree,
                            CurrentCityName = request.Request.ConnectorPersonalDetail.CurrentAddress.CityName,
                            CurrentStateName = request.Request.ConnectorPersonalDetail.CurrentAddress.StateName,
                            CurrentZipCode = request.Request.ConnectorPersonalDetail.CurrentAddress.ZipCode,
                            CurrentCountryName = request.Request.ConnectorPersonalDetail.CurrentAddress.CountryName,
                            PermanentAddressLineOne = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineOne,
                            PermanentAddressLineTwo = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineTwo,
                            PermanentAddressLineThree = request.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineThree,
                            PermanentCityName = request.Request.ConnectorPersonalDetail.CurrentAddress.CityName,
                            PermanentStateName = request.Request.ConnectorPersonalDetail.CurrentAddress.StateName,
                            PermanentZipCode = request.Request.ConnectorPersonalDetail.CurrentAddress.ZipCode,
                            PermanentCountryName = request.Request.ConnectorPersonalDetail.CurrentAddress.CountryName,
                            EmailId = request.Request.ConnectorPersonalDetail.EmailId,
                            Gender = request.Request.aadharDetail.Gender,
                            MobileNo = request.Request.ConnectorPersonalDetail.MobileNo,
                            AadharBackImage = request.Request.aadharDetail.BackImageUrl,
                            AadharFrontImage = request.Request.aadharDetail.FrontImageUrl,
                            PanFrontImage = request.Request.panDetail.FrontImageUrl,
                            PanNameOnCard = request.Request.panDetail.NameOnCard,
                            SelfieImageUrl = request.Request.SelfieDetail.FrontImageUrl,
                            Marital = "",
                            LeadId = request.Request.LeadId.Value,
                            IsActive = true,
                            IsDeleted = false
                        };
                        await _context.PersonalDetails.AddAsync(AddPersonalDetail);
                    }
                }
                if (await _context.SaveChangesAsync() > 0)
                {
                    result.Status = true;
                }
                else
                {
                    result.Status = false;
                }
            }
            else
            {
                result.Message = "Lead is not Submitted Yet";
                result.Status = false;
                return result;
            }

            return result;
        }

        public async Task<GRPCReply<string>> PrepareAgreement(GRPCRequest<long> request)
        {
            GRPCReply<string> reply = new GRPCReply<string> { Message = "Data Not Found!!!" };
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request && x.IsActive && !x.IsDeleted);

            if (lead != null)
            {
                lead.Status = LeadStatusEnum.AgreementInProgress.ToString();
                _context.Entry(lead).State = EntityState.Modified;

                var InprogressleadActivity = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == request.Request &&
                x.ActivityMasterName == ActivityEnum.Inprogress.ToString() && !x.IsDeleted && x.IsActive);

                if (InprogressleadActivity != null)
                {
                    InprogressleadActivity.IsCompleted = true;
                    _context.Entry(InprogressleadActivity).State = EntityState.Modified;
                }

                if (_context.SaveChanges() > 0)
                {
                    reply.Status = true;
                    reply.Message = "updated Successfully";
                    reply.Response = "";
                }
                else
                {
                    reply.Status = false;
                    reply.Message = "failed to update";
                    reply.Response = "";
                }
            }
            else
            {
                reply.Status = false;
                reply.Message = "failed to update";
                reply.Response = "";
            }
            return reply;
        }

        public async Task<GRPCReply<bool>> UpdateLeadStatus(GRPCRequest<UpdateLeadStatusRequest> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool> { Message = "Lead Not Found!!!" };
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request.LeadId && x.IsActive && !x.IsDeleted);
            if (lead != null)
            {
                lead.Status = request.Request.Status;
                _context.Entry(lead).State = EntityState.Modified;

                var leadprogress = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == request.Request.LeadId && x.ActivityMasterName == ActivityConstants.DSACongratulations && x.IsActive && !x.IsDeleted);
                if (leadprogress != null)
                {
                    leadprogress.IsCompleted = true;
                    _context.Entry(leadprogress).State = EntityState.Modified;
                }
                if (_context.SaveChanges() > 0)
                {
                    reply.Status = true;
                    reply.Message = "updated Successfully";
                    reply.Response = true;
                }
            }
            return reply;
        }
        #endregion

        //public async Task<GRPCReply<long>> GetLeadCompany(DSALeadInitiate initiateLeadDetail)
        //{
        //    GRPCReply<long> Reply = new GRPCReply<long>();
        //    Leads? leads = _context.Leads.FirstOrDefault(x => x.MobileNo == initiateLeadDetail.MobileNumber && x.IsActive && !x.IsDeleted);//x.ProductId == initiateLeadDetail.ProductId

        //    if (leads == null)
        //    {
        //        List<LeadActivityMasterProgresses> leadActivityMasterProgresses = new List<LeadActivityMasterProgresses>();

        //        //if (initiateLeadDetail.ProductActivities != null && initiateLeadDetail.ProductActivities.Any())
        //        //{
        //        //    foreach (var item in initiateLeadDetail.ProductActivities)
        //        //    {
        //        //        leadActivityMasterProgresses.Add(new LeadActivityMasterProgresses
        //        //        {
        //        //            ActivityMasterId = item.ActivityMasterId,
        //        //            IsActive = (item.ActivityName == ActivityEnum.Rejected.ToString()) ? false : true,
        //        //            IsApproved = 0,
        //        //            IsCompleted = false,
        //        //            IsDeleted = false,
        //        //            LeadMasterId = 0,
        //        //            Sequence = item.Sequence,
        //        //            SubActivityMasterId = item.SubActivityMasterId ?? 0,
        //        //            ActivityMasterName = item.ActivityName,
        //        //            SubActivityMasterName = item.SubActivityName
        //        //        });

        //        //    }

        //        //}

        //        string LeadNo = "LeadNo";
        //        GRPCRequest<string> req = new GRPCRequest<string>();
        //        req.Request = LeadNo;
        //        var res = await GetCurrentNumber(req);

        //        //leads = new Leads
        //        //{
        //        //    MobileNo = initiateLeadDetail.MobileNumber,
        //        //    ProductId = initiateLeadDetail.ProductId,
        //        //    UserName = initiateLeadDetail.UserId,
        //        //    LeadCode = res.Response,
        //        //    IsActive = true,
        //        //    CibilReport = "",
        //        //    ProductCode = initiateLeadDetail.ProductCode,
        //        //    LeadActivityMasterProgresses = leadActivityMasterProgresses,
        //        //    CityId = initiateLeadDetail.CityId
        //        //};
        //        //_context.Leads.Add(leads);
        //        //var result = await _context.SaveChangesAsync();
        //    }
        //    if (!_context.CompanyLead.Any(x => x.LeadId == leads.Id && x.CompanyCode == "ScaleupFintech" && x.IsActive && !x.IsDeleted))
        //    {
        //        var status = _context.CompanyLead.Any(x => x.LeadId == leads.Id && x.LeadProcessStatus == 0);

        //        //var LeadCompanyBuyingHistories = (initiateLeadDetail.CustomerBuyingHistories != null && initiateLeadDetail.CustomerBuyingHistories.Any()) ? initiateLeadDetail.CustomerBuyingHistories.Select(x => new LeadCompanyBuyingHistory
        //        //{
        //        //    CompanyLeadId = 0,
        //        //    MonthFirstBuyingDate = x.MonthFirstBuyingDate,
        //        //    MonthTotalAmount = x.MonthTotalAmount,
        //        //    TotalMonthInvoice = x.TotalMonthInvoice,
        //        //    IsActive = true,
        //        //    IsDeleted = false
        //        //}).ToList()
        //        //: new List<LeadCompanyBuyingHistory>();
        //        List<CompanyLead> companyLeads = new List<CompanyLead>();
        //        var monthbuying = Convert.ToInt32(LeadCompanyBuyingHistories.Any() ? LeadCompanyBuyingHistories.Average(x => x.MonthTotalAmount) : 0);
        //        CompanyLead companyLead = new CompanyLead
        //        {
        //            CompanyId = initiateLeadDetail.AnchorCompanyId,
        //            CompanyCode = initiateLeadDetail.AnchorCompanyCode,
        //            LeadId = leads.Id,
        //            IsActive = true,
        //            IsDeleted = false,
        //            LeadProcessStatus = status ? 1 : 0,
        //            UserUniqueCode = initiateLeadDetail.CustomerReferenceNo,
        //            Email = initiateLeadDetail.Email,
        //            MonthlyAvgBuying = monthbuying,
        //            VintageDays = initiateLeadDetail.VintageDays,
        //            LeadCompanyBuyingHistories = LeadCompanyBuyingHistories,
        //            AnchorName = initiateLeadDetail.AnchorCompanyName
        //        };
        //        companyLeads.Add(companyLead);
        //        await _context.CompanyLead.AddRangeAsync(companyLeads);
        //        if (await _context.SaveChangesAsync() > 0)
        //        {
        //            Reply.Status = true;
        //            Reply.Message = "Lead generated successfully.";
        //            Reply.Response = leads.Id;
        //        }
        //        else
        //        {
        //            Reply.Status = false;
        //            Reply.Message = "Error during generate lead.";
        //            Reply.Response = 0;
        //        }
        //    }
        //    if (!_context.CompanyLead.Any(x => x.LeadId == leads.Id && x.CompanyCode == "SCDSA" && x.IsActive && !x.IsDeleted))
        //    {
        //        var status = _context.CompanyLead.Any(x => x.LeadId == leads.Id && x.LeadProcessStatus == 0);

        //        //var LeadCompanyBuyingHistories = (initiateLeadDetail.CustomerBuyingHistories != null && initiateLeadDetail.CustomerBuyingHistories.Any()) ? initiateLeadDetail.CustomerBuyingHistories.Select(x => new LeadCompanyBuyingHistory
        //        //{
        //        //    CompanyLeadId = 0,
        //        //    MonthFirstBuyingDate = x.MonthFirstBuyingDate,
        //        //    MonthTotalAmount = x.MonthTotalAmount,
        //        //    TotalMonthInvoice = x.TotalMonthInvoice,
        //        //    IsActive = true,
        //        //    IsDeleted = false
        //        //}).ToList()
        //        //: new List<LeadCompanyBuyingHistory>();
        //        List<CompanyLead> companyLeads = new List<CompanyLead>();
        //        var monthbuying = Convert.ToInt32(LeadCompanyBuyingHistories.Any() ? LeadCompanyBuyingHistories.Average(x => x.MonthTotalAmount) : 0);
        //        CompanyLead companyLead = new CompanyLead
        //        {
        //            CompanyId = initiateLeadDetail.AnchorCompanyId,
        //            CompanyCode = initiateLeadDetail.AnchorCompanyCode,
        //            LeadId = leads.Id,
        //            IsActive = true,
        //            IsDeleted = false,
        //            LeadProcessStatus = status ? 1 : 0,
        //            UserUniqueCode = initiateLeadDetail.CustomerReferenceNo,
        //            Email = initiateLeadDetail.Email,
        //            MonthlyAvgBuying = monthbuying,
        //            VintageDays = initiateLeadDetail.VintageDays,
        //            LeadCompanyBuyingHistories = LeadCompanyBuyingHistories,
        //            AnchorName = initiateLeadDetail.AnchorCompanyName
        //        };
        //        companyLeads.Add(companyLead);
        //        await _context.CompanyLead.AddRangeAsync(companyLeads);
        //        if (await _context.SaveChangesAsync() > 0)
        //        {
        //            Reply.Status = true;
        //            Reply.Message = "Lead generated successfully.";
        //            Reply.Response = leads.Id;
        //        }
        //        else
        //        {
        //            Reply.Status = false;
        //            Reply.Message = "Error during generate lead.";
        //            Reply.Response = 0;
        //        }
        //    }

        //    else
        //    {
        //        Reply.Status = false;
        //        Reply.Message = "Customer record already exists";
        //        Reply.Response = 0;
        //    }
        //    return Reply;
        //}

        public async Task<GRPCReply<LeadAggrementDetailReponse>> GetDSAAggreementDetailByLeadId(GRPCRequest<long> request)
        {
            GRPCReply<LeadAggrementDetailReponse> reply = new GRPCReply<LeadAggrementDetailReponse> { Message = "Data Not Found!!" };
            var lead = await _context.Leads.Where(x => x.Id == request.Request && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (lead != null)
            {
                var leadAggrement = await _context.leadAgreements.Where(x => x.LeadId == lead.Id && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                if (leadAggrement != null)
                {
                    var payouts = await _context.DSAPayouts.Where(x => x.LeadId == lead.Id && x.IsActive && !x.IsDeleted).ToListAsync();
                    reply.Response = new LeadAggrementDetailReponse
                    {
                        LeadId = lead.Id,
                        LeadCode = lead.LeadCode,
                        DocSignedUrl = leadAggrement.DocSignedUrl,
                        StartedOn = leadAggrement.StartedOn,
                        ExpiredOn = leadAggrement.ExpiredOn,
                        Status = leadAggrement.Status,
                        DocUnSignedUrl = leadAggrement.DocUnSignUrl,
                        eSignedUrl = leadAggrement.eSignUrl,
                        //PayoutPercenatge = payout != null ? payout.PayoutPercenatge : 0,
                        UserName = lead.UserName,
                        SalesAgentCommissions = payouts != null && payouts.Any() ? payouts.Select(x => new SalesAgentCommissionList
                        {
                            MaxAmount = x.MaxAmount,
                            MinAmount = x.MinAmount,
                            PayoutPercentage = x.PayoutPercenatge,
                            ProductId = x.ProductId
                        }).ToList() : new List<SalesAgentCommissionList>()
                    };
                    reply.Status = true;
                    reply.Message = "Data Found";
                }

            }
            return reply;
        }


        public async Task<GRPCReply<LeadDataDC>> GetDSALeadDataById(GRPCRequest<LeadRequestDataDC> request)
        {
            GRPCReply<LeadDataDC> gRPCReply = new GRPCReply<LeadDataDC>();
            if (request.Request.LeadId > 0)
            {
                var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request.LeadId && ((request.Request.Status == LeadStatusEnum.Deleted.ToString() && x.IsDeleted) || !x.IsDeleted));
                var Payouts = await _context.DSAPayouts.Where(x => x.LeadId == request.Request.LeadId && !x.IsDeleted && x.IsActive).ToListAsync();
                if (lead != null)
                {
                    LeadDataDC data = new LeadDataDC
                    {
                        ApplicantName = lead.ApplicantName,
                        IsActive = lead.IsActive,
                        Created = lead.Created,
                        LeadCode = lead.LeadCode,
                        MobileNo = lead.MobileNo,
                        ProductCode = lead.ProductCode,
                        IsDeleted = lead.IsDeleted,
                        LeadId = lead.Id,
                        ProductId = lead.ProductId,
                        Status = lead.Status,
                        UserId = lead.UserName,
                        CreatedBy = lead.CreatedBy,
                        SalesAgentCommissions = Payouts.Select(x => new SalesAgentCommissionList
                        {
                            MaxAmount = x.MaxAmount,
                            MinAmount = x.MinAmount,
                            PayoutPercentage = x.PayoutPercenatge,
                            ProductId = x.ProductId
                        }).ToList()
                        //PayoutPercentage = Payout != null ? Payout.PayoutPercenatge : null
                    };
                    //var dsaDetail = await _context.Leads.FirstOrDefaultAsync(x => x.UserName == data.CreatedBy && x.ProductCode == ProductCodeConstants.DSA);
                    //if (dsaDetail != null)
                    //{
                    //    data.DSALeadCode = dsaDetail.LeadCode;
                    //}
                    gRPCReply.Response = data;
                    gRPCReply.Status = true;
                }
                else
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Data not found";
                }
            }
            return gRPCReply;
        }

        public async Task<LeadListPageReply> GetDSALeadForListPage(LeadListPageRequest request)
        {
            var predicate = PredicateBuilder.True<Leads>();
            LeadListPageReply leadListPageReply = new LeadListPageReply();

            List<LeadListDetail> leads = new List<LeadListDetail>();

            List<string> statusList = new List<string>();
            statusList.Add("pending");
            statusList.Add("LoanInitiated");
            statusList.Add("LoanActivated");
            statusList.Add("LoanApproved");
            statusList.Add("LoanRejected");
            statusList.Add("LoanAvailed");

            statusList.Add("LineInitiated");
            statusList.Add("LineActivated");
            statusList.Add("LineApproved");
            statusList.Add("LineRejected");

            if (request.isDelete)
            {
                predicate = predicate.And(x => !statusList.Contains(x.Status) && x.CompanyLeads.Any(y => !y.IsDeleted));
            }
            else
            {
                predicate = predicate.And(x => !statusList.Contains(x.Status) && !x.IsDeleted && x.CompanyLeads.Any(y => !y.IsDeleted));
            }


            if (request.ToDate != null)
                request.ToDate = request.ToDate?.Date.AddDays(1).AddMilliseconds(-1);

            //if (!string.IsNullOrEmpty(request.Keyword))
            //{
            //    predicate = predicate.And(x => x.MobileNo.Contains(request.Keyword.Trim()) || x.LeadCode.Contains(request.Keyword.Trim())
            //                                    || x.ApplicantName.Contains(request.Keyword.Trim()));
            //}

            if (request.FromDate != null && request.ToDate != null)
            {
                predicate = predicate.And(x => x.Created >= request.FromDate && x.Created <= request.ToDate);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                predicate = predicate.And(x => x.Status == request.Status);
            }

            //if (request.CityId > 0)
            //{
            //    predicate = predicate.And(x => x.CityId == request.CityId);
            //}

            if (request.CompanyId != null && request.CompanyId.Count() > 0)
            {
                predicate = predicate.And(x => x.CompanyLeads.Any(y => request.CompanyId.Contains((int)y.CompanyId) && !y.IsDeleted));
            }

            if (request.ProductId != null && request.ProductId.Any() && request.ProductId.Count > 0)
            {
                predicate = predicate.And(x => request.ProductId.Contains(x.ProductId));
            }
            //predicate = predicate.And(x => !statusList.Contains(x.Status));
            var leadData = await _context.Leads.Where(predicate).Include(y => y.CompanyLeads).OrderByDescending(z => z.Id).ToListAsync();
            var payOutPercentageList = await _context.DSAPayouts.Where(x => x.IsActive && !x.IsDeleted).Select(x => new { x.LeadId, x.MinAmount, x.MaxAmount, x.PayoutPercenatge, x.ProductId }).ToListAsync();
            var leadAgreement = await _context.leadAgreements.Where(x => x.IsActive && !x.IsDeleted).Select(x => new { x.LeadId, x.StartedOn, x.ExpiredOn }).ToListAsync();

            leads = leadData.Select(x => new LeadListDetail
            {
                CreatedDate = x.Created,
                Id = x.Id,
                MobileNo = x.MobileNo,
                UserId = x.UserName,
                LastModified = x.LastModified,
                LeadCode = x.LeadCode,
                CreditScore = x.CreditScore,
                Status = x.Status,
                CityId = x.CityId,
                LeadGenerator = x.LeadGenerator,
                LeadConvertor = x.LeadConverter,
                CreditLimit = x.CreditLimit,
                ProductCode = x.ProductCode,
                IsActive = x.IsActive,
                AgreementStartDate = (leadAgreement != null && leadAgreement.Count > 0)
                                        ? leadAgreement.Where(y => y.LeadId == x.Id).Select(y => y.StartedOn).FirstOrDefault() : null,
                AgreementEndDate = (leadAgreement != null && leadAgreement.Count > 0)
                                        ? leadAgreement.Where(y => y.LeadId == x.Id).Select(y => y.ExpiredOn).FirstOrDefault() : null,
                //PayoutPercentage = (payOutPercentageList != null && payOutPercentageList.Count > 0)
                //                        ? payOutPercentageList.Where(y => y.LeadId == x.Id).Select(y => y.PayoutPercenatge).FirstOrDefault() : null
                SalesAgentCommissions = (payOutPercentageList != null && payOutPercentageList.Count > 0)
                                        ? payOutPercentageList.Where(y => y.LeadId == x.Id).Select(y => new SalesAgentCommissionList { MaxAmount = y.MaxAmount, MinAmount = y.MinAmount, PayoutPercentage = y.PayoutPercenatge, ProductId = y.ProductId }).ToList()
                                        : new List<SalesAgentCommissionList>()

            }).ToList();
            leadListPageReply.TotalCount = leadData.Count();

            List<long> LeadIds = leads.Select(x => x.Id).ToList();
            if (LeadIds != null && LeadIds.Any())
            {
                var companyName = await _context.CompanyLead.Where(x => LeadIds.Contains(x.LeadId)).Select(y => new
                {
                    leadId = y.LeadId,
                    anchorName = y.AnchorName,
                    uniquecode = y.UserUniqueCode,
                    companyId = y.CompanyId
                }).ToListAsync();

                if (leads != null && leads.Count > 0)
                {
                    foreach (var lead in leads)
                    {

                        if (companyName != null)
                        {
                            var companyData = companyName.FirstOrDefault(x => x.leadId == lead.Id);
                            lead.AnchorName = companyData.anchorName;
                            lead.UniqueCode = string.Join(",", companyName.Where(x => x.leadId == lead.Id).Select(y => y.uniquecode).ToList());
                            lead.AnchorCompanyId = companyData.companyId;
                        }
                    }
                }
            }

            if (LeadIds != null && LeadIds.Any())
            {
                var leadloandata = await _context.LeadLoan.Where(x => x.LeadMasterId.HasValue && LeadIds.Contains(x.LeadMasterId.Value)).Select(y => new
                {
                    leadId = y.LeadMasterId,
                    loan_app_id = y.loan_app_id,
                    partner_loan_app_id = y.partner_loan_app_id
                }).ToListAsync();

                if (leads != null && leads.Count > 0)
                {
                    foreach (var lead in leads)
                    {
                        if (leadloandata != null)
                        {
                            var leadloan = leadloandata.FirstOrDefault(x => x.leadId == lead.Id);
                            if (leadloan != null)
                            {
                                lead.Loan_app_id = leadloan.loan_app_id;
                                lead.Partner_Loan_app_id = leadloan.partner_loan_app_id;
                            }
                        }
                    }
                }
            }


            if (LeadIds != null && LeadIds.Any())
            {
                var data = await _context.LeadActivityMasterProgresses.Where(x => LeadIds.Contains(x.LeadMasterId) && x.IsCompleted == true).OrderBy(x => x.Sequence).GroupBy(x => x.LeadMasterId)
                    .Select(x => new
                    {
                        LeadId = x.Key,
                        Sequence = x.Max(y => y.Sequence)
                    ,
                        ActivityId = x.OrderByDescending(y => y.Sequence).FirstOrDefault().ActivityMasterId,
                        SubActivityId = x.OrderByDescending(y => y.Sequence).FirstOrDefault().SubActivityMasterId,
                        ActivityMasterName = x.OrderByDescending(y => y.Sequence).FirstOrDefault().ActivityMasterName,
                        SubactivityMasterName = x.OrderByDescending(y => y.Sequence).FirstOrDefault().SubActivityMasterName
                    }).ToListAsync();

                if (data != null)
                {
                    foreach (var item in leads)
                    {
                        if (data.Any(x => x.LeadId == item.Id))
                        {
                            item.SequenceNo = data.FirstOrDefault(x => x.LeadId == item.Id).Sequence;
                            item.SubActivityId = data.FirstOrDefault(x => x.LeadId == item.Id).SubActivityId;
                            item.ActivityId = data.FirstOrDefault(x => x.LeadId == item.Id).ActivityId;
                            item.ScreenName = string.IsNullOrEmpty(data.FirstOrDefault(x => x.LeadId == item.Id).SubactivityMasterName) ? (data.FirstOrDefault(x => x.LeadId == item.Id).SubactivityMasterName + " " + data.FirstOrDefault(x => x.LeadId == item.Id).ActivityMasterName) : data.FirstOrDefault(x => x.LeadId == item.Id).ActivityMasterName;
                        }
                    }
                }

            }
            leadListPageReply.LeadListDetails = leads;

            return leadListPageReply;
        }



        public async Task<GRPCReply<bool>> CheckLeadCreatePermission(GRPCRequest<CheckLeadCreatePermissionRequest> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool> { Message = "You are not authorized" };
            var leadInfo = await _context.Leads.Where(x => x.UserName == request.Request.UserId && x.ProductCode == ProductCodeConstants.DSA).ToListAsync();
            if (leadInfo != null && leadInfo.Count > 0)
            {
                if (leadInfo.Any(x=>x.IsActive))
                {
                    var lead = await _context.Leads.FirstOrDefaultAsync(x => x.MobileNo == request.Request.MobileNo && x.ProductId == request.Request.ProductId);
                    if (lead == null)
                    {
                        reply.Response = true;
                        reply.Status = true;
                        reply.Message = "You are Permitted";
                    }
                    else if (lead.IsActive)
                    {
                        var isPermitted = request.Request.AgentUserIds.Any(x => x == lead.CreatedBy);
                        if (isPermitted)
                        {
                            lead.LeadGenerator = request.Request.UserName;
                            _context.Entry(lead).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                            reply.Response = true;
                            reply.Status = true;
                            reply.Message = "You are Permitted";
                        }
                    }
                    else if (!lead.IsActive)
                    {
                        reply.Response = false;
                        reply.Status = false;
                        reply.Message = "You are DeActivated.Please contact to scaleup person.";
                    }
                    else if (lead.IsDeleted)
                    {
                        reply.Response = false;
                        reply.Status = false;
                        reply.Message = "You are Rejected by ScaleUp.";
                    }
                }
                else if (!leadInfo.Any(x=>x.IsActive))
                {
                    reply.Response = false;
                    reply.Status = false;
                    reply.Message = "You are DeActivated.Please contact to scaleup person.";
                }
                else if (leadInfo.Any(x=>x.IsDeleted))
                {
                    reply.Response = false;
                    reply.Status = false;
                    reply.Message = "You are Rejected by ScaleUp.";
                }


            }
            else
            {
                var lead = await _context.Leads.FirstOrDefaultAsync(x => x.MobileNo == request.Request.MobileNo && x.ProductId == request.Request.ProductId);
                if (lead == null)
                {
                    reply.Response = true;
                    reply.Status = true;
                    reply.Message = "You are Permitted";
                }
                else if (lead.IsActive)
                {
                    var isPermitted = request.Request.AgentUserIds.Any(x => x == lead.CreatedBy);
                    if (isPermitted)
                    {
                        lead.LeadGenerator = request.Request.UserName;
                        _context.Entry(lead).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        reply.Response = true;
                        reply.Status = true;
                        reply.Message = "You are Permitted";
                    }
                }
                else if (!lead.IsActive)
                {
                    reply.Response = false;
                    reply.Status = false;
                    reply.Message = "You are DeActivated.Please contact to scaleup person.";
                }
                else if (lead.IsDeleted)
                {
                    reply.Response = false;
                    reply.Status = false;
                    reply.Message = "You are Rejected by ScaleUp.";
                }
            }
            return reply;
        }

        public async Task<GRPCReply<List<long>>> GetLeadByIDs(GRPCRequest<List<string>> request)
        {
            GRPCReply<List<long>> gRPCReply = new GRPCReply<List<long>>();
            if (request.Request != null && request.Request.Any())
            {
                var leads = await _context.Leads.Where(x => x.CreatedBy != null && request.Request.Contains(x.CreatedBy) && !x.IsDeleted && x.IsActive).ToListAsync();
                if (leads != null && leads.Any())
                {
                    gRPCReply.Response = leads.Select(x => x.Id).ToList();
                    gRPCReply.Status = true;
                }
                else
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Data not found";
                }
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<LeadResponse>> GetLeadInfoByUserId(GRPCRequest<string> request)
        {
            GRPCReply<LeadResponse> res = new GRPCReply<LeadResponse>();
            var lead = await _context.Leads.Where(x => x.UserName == request.Request).FirstOrDefaultAsync();
            if (lead != null)
            {
                lead.LeadGenerator = request.Request;
                _context.Entry(lead).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            res.Status = true;
            //res.Response = lead;
            return res;

        }

        public async Task<GRPCReply<string>> DSADeactivate(GRPCRequest<ActivatDeActivateDSALeadRequest> request)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            if (request.Request.LeadId > 0)
            {
                var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request.LeadId && !x.IsDeleted);
                if (lead != null)
                {
                    if (request.Request.isReject)
                    {
                        lead.IsActive = request.Request.isActive;
                        lead.IsDeleted = request.Request.isReject;
                        lead.Status = LeadStatusEnum.Deleted.ToString();
                        lead.LastModifiedBy = request.Request.UserId;
                    }
                    else
                    {
                        lead.IsActive = request.Request.isActive;
                        lead.LastModifiedBy = request.Request.UserId;
                        lead.Status = request.Request.isActive ? LeadStatusEnum.Activated.ToString() : LeadStatusEnum.DeActivated.ToString();
                    }
                    _context.Entry(lead).State = EntityState.Modified;
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        gRPCReply.Response = lead.UserName;
                        gRPCReply.Status = true;
                    }
                }
                else
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Data not found";
                }
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> GenerateLeadOfferByFinance(InitiateLeadOfferRequest initiateLeadOfferRequest)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();

            GRPCRequest<UserDetailsReply> kYCUserrequest = new GRPCRequest<UserDetailsReply>();
            kYCUserrequest.Request = initiateLeadOfferRequest.kycdetail;
            kYCUserrequest.Request.LeadId = initiateLeadOfferRequest.LeadId;
            var kycAddupdateResponse = await AddUpdateLeadDetail(kYCUserrequest);
            if (!kycAddupdateResponse.Status)
            {
                reply.Status = false;
                reply.Message = "Something went wrong in kyc Add update :" + kycAddupdateResponse.Message;
                return reply;
            }

            GRPCRequest<List<LeadNBFCSubActivityRequestDc>> _leadNBFCSubActivityRequest = new GRPCRequest<List<LeadNBFCSubActivityRequestDc>>();
            _leadNBFCSubActivityRequest.Request = initiateLeadOfferRequest.LeadNBFCSubActivityRequest;
            var leadNBFCSubActivityRequest = await InsertLeadNBFCApi(_leadNBFCSubActivityRequest);
            if (!leadNBFCSubActivityRequest.Status)
            {
                reply.Status = false;
                reply.Message = "Something went wrong in Lead NBFC Sub Activity Insert :" + leadNBFCSubActivityRequest.Message;
                return reply;
            }
            bool IsGenerateOfferForLead = false;
            List<long> ids = new List<long>();
            var leadoffers = await _context.LeadOffers.Where(x => x.LeadId == initiateLeadOfferRequest.LeadId && x.IsActive && !x.IsDeleted).ToListAsync();
            if (leadoffers != null && leadoffers.Any())
            {

                foreach (var i in initiateLeadOfferRequest.Companys)
                {
                    bool ispresent = leadoffers.Any(x => x.NBFCCompanyId == i.NbfcId);
                    if (!ispresent)
                    {
                        ids.Add(i.NbfcId);
                    }
                    IsGenerateOfferForLead = true;
                }
            }
            else
            {
                foreach (var i in initiateLeadOfferRequest.Companys)
                {
                    ids.Add(i.NbfcId);
                    IsGenerateOfferForLead = true;

                }
            }

            if (ids.Count > 0)
            {
                var leadOffers = ids.Select(x => new LeadOffers
                {
                    LeadId = initiateLeadOfferRequest.LeadId,
                    NBFCCompanyId = x,
                    CompanyIdentificationCode = initiateLeadOfferRequest.Companys.FirstOrDefault(z => z.NbfcId == x).CompanyIdentificationCode,
                    IsActive = true,
                    IsDeleted = false,
                    CreditLimit = initiateLeadOfferRequest.CreditLimit,
                    Status = "OfferGenerated"
                });
                await _context.AddRangeAsync(leadOffers);
                //var leadInfo =  _context.Leads.FirstOrDefault(x => x.Id == initiateLeadOfferRequest.LeadId && x.IsActive && !x.IsDeleted);
                //if(leadInfo != null)
                //{
                //    leadInfo.CreditLimit = initiateLeadOfferRequest.CreditLimit;
                //    _context.Entry(leadInfo).State = EntityState.Modified;
                //}
                if (_context.SaveChanges() > 0)
                {
                    reply.Status = true;
                    reply.Message = "Lead Offer generated successfully.";
                }
                else
                {
                    reply.Status = false;
                    reply.Message = "Lead Offer not generated.";
                }
            }

            if (IsGenerateOfferForLead)
            {
                reply.Status = true;
                reply.Message = "Lead Offer generated successfully.";
            }
            else
            {
                reply.Status = false;
                reply.Message = "Lead Offer not generated.";
            }
            return reply;
        }

        public async Task<GRPCReply<LeadCityIds>> GetAllLeadCities()
        {
            GRPCReply<LeadCityIds> gRPCReply = new GRPCReply<LeadCityIds> { Response = new LeadCityIds() };
            var CityIds = await _context.Leads.Where(x => !x.IsDeleted && x.CityId != null).Select(x => x.CityId).Distinct().ToListAsync();
            if (CityIds.Count > 0 && CityIds.Any())
            {
                gRPCReply.Response.CityIds = CityIds;
                gRPCReply.Status = true;
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Data not found";
            }
            return gRPCReply;
        }

        #region MAS finance Agreement

        public async Task<GRPCReply<string>> UploadMASfinanceAgreement(GRPCRequest<MASFinanceAgreementDc> req)
        {
            GRPCReply<string> reply = new GRPCReply<string> { Message = "Failed to upload" };
            var leadagreement = await _context.leadAgreements.Where(x => x.LeadId == req.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (leadagreement != null)
            {
                //if (string.IsNullOrEmpty(leadagreement.DocSignedUrl))
                //{
                leadagreement.DocSignedUrl = req.Request.DocUrl;
                leadagreement.DocumentId = req.Request.DocId;
                _context.Entry(leadagreement).State = EntityState.Modified;
                if (await _context.SaveChangesAsync() > 0)
                {
                    reply.Status = true;
                    reply.Message = "Upload Successfully";
                }
                //}
                //else
                //{
                //    reply.Status = false;
                //    reply.Message = "Agreement Already Exist";
                //}
            }
            else
            {
                await _context.leadAgreements.AddAsync(new LeadAgreement
                {
                    DocUnSignUrl = "",
                    DocSignedUrl = req.Request.DocUrl,
                    LeadId = req.Request.LeadId,
                    DocumentId = req.Request.DocId,
                    Status = "Signed",
                    IsActive = true,
                    IsDeleted = false
                });
                if (await _context.SaveChangesAsync() > 0)
                {
                    reply.Status = true;
                    reply.Message = "Upload Successfully";
                }
            }
            return reply;
        }

        #endregion
        public async Task<bool> AddLeadConsentLog(long LeadId, string DocType, string UserId)
        {
            LeadConsentLog leadConsentLog = new LeadConsentLog
            {
                LeadId = LeadId,
                IsChecked = true,
                Type = DocType,
                CreatedBy = UserId,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Today
            };
            _context.LeadConsentLogs.Add(leadConsentLog);

            var rowChanges = await _context.SaveChangesAsync();
            if (rowChanges > 0)
            {
                //------------------S : Make log---------------------
                #region Make History - MobileOTP
                string doctypeotp = "LeadConsentLog_" + DocType;
                var result = await _leadHistoryManager.GetLeadHistroy(LeadId, doctypeotp);
                LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                {
                    LeadId = LeadId,
                    UserID = result.UserId,
                    UserName = "",
                    EventName = doctypeotp,//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                    Narretion = result.Narretion,
                    NarretionHTML = result.NarretionHTML,
                    CreatedTimeStamp = result.CreatedTimeStamp
                };
                await _massTransitService.Publish(histroyEvent);
                #endregion
                //------------------E : Make log---------------------


                return true;
            }
            return false;
        }

        public async Task<GRPCReply<bool>> UpdateBuyingHistory(GRPCRequest<UpdateBuyingHistoryRequest> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.MobileNo == request.Request.MobileNumber && x.ProductCode == request.Request.ProductCode && x.IsActive && !x.IsDeleted);
            if (lead != null)
            {
                var companyLead = await _context.CompanyLead.FirstOrDefaultAsync(x => x.LeadId == lead.Id && x.CompanyCode == request.Request.AnchorCompanyCode && x.IsActive && !x.IsDeleted);
                if (companyLead != null)
                {
                    if (request.Request.BuyingHistories != null && request.Request.BuyingHistories.Any())
                    {
                        var existingHistories = await _context.LeadCompanyBuyingHistorys.Where(x => x.CompanyLeadId == companyLead.Id && x.IsActive && !x.IsDeleted).ToListAsync();
                        if (existingHistories != null && existingHistories.Any())
                        {
                            foreach (var item in existingHistories)
                            {
                                item.IsActive = false;
                                item.IsDeleted = true;
                                _context.Entry(item).State = EntityState.Modified;
                            }
                        }
                        List<LeadCompanyBuyingHistory> buyingList = new List<LeadCompanyBuyingHistory>();
                        foreach (var item in request.Request.BuyingHistories)
                        {
                            buyingList.Add(new LeadCompanyBuyingHistory
                            {
                                CompanyLeadId = companyLead.Id,
                                MonthFirstBuyingDate = item.MonthFirstBuyingDate,
                                TotalMonthInvoice = item.TotalMonthInvoice,
                                MonthTotalAmount = item.MonthTotalAmount,
                                IsActive = true,
                                IsDeleted = false
                            });
                        }
                        _context.AddRange(buyingList);
                        if (_context.SaveChanges() > 0)
                        {
                            reply.Status = true;
                            reply.Message = "Data Updated";
                        }
                    }
                }
                else
                {
                    reply.Message = "Company Lead not found";
                }
            }
            else
            {
                reply.Message = "Mobile Number not found";
            }
            return reply;
        }
        public async Task<GRPCReply<double>> GetOfferAmountByNbfc(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request)
        {
            GRPCReply<double> res = new GRPCReply<double>();

            var leadOffer = await _context.LeadOffers.Where(x => x.LeadId == request.Request.LeadId && x.NBFCCompanyId == request.Request.NbfcCompanyId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (leadOffer != null)
            {
                res.Response = leadOffer.CreditLimit ?? 0;
                res.Status = true;
                res.Message = "Data Found";
            }
            else
            {
                res.Status = false;
                res.Message = "Data Not Found";
            }
            return res;
        }


        public async Task<bool> AddLeadConsentLog(long LeadId)
        {
            var leadInfo = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId && x.IsActive && !x.IsDeleted);
            LeadConsentLog leadConsentLog = new LeadConsentLog
            {
                LeadId = LeadId,
                IsChecked = true,
                Type = LeadTypeConsentConstants.Agrement,
                CreatedBy = leadInfo.UserName,
                IsActive = true,
                IsDeleted = false,
                Created = DateTime.Today
            };
            _context.LeadConsentLogs.Add(leadConsentLog);

            var rowChanges = await _context.SaveChangesAsync();
            if (rowChanges > 0)
            {
                //------------------S : Make log---------------------
                #region Make History - MobileOTP
                string doctypeotp = "LeadConsentLog_" + LeadTypeConsentConstants.Agrement;
                var result = await _leadHistoryManager.GetLeadHistroy(LeadId, doctypeotp);
                LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                {
                    LeadId = LeadId,
                    UserID = result.UserId,
                    UserName = "",
                    EventName = doctypeotp,//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                    Narretion = result.Narretion,
                    NarretionHTML = result.NarretionHTML,
                    CreatedTimeStamp = result.CreatedTimeStamp
                };
                await _massTransitService.Publish(histroyEvent);
                #endregion
                //------------------E : Make log---------------------


                return true;
            }
            return false;
        }


        public async Task<GRPCReply<bool>> AddLeadOfferConfig(GRPCRequest<AddLeadOfferConfigRequestDc> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            if (request != null && request.Request != null && request.Request.ProductSlabConfigs != null && request.Request.ProductSlabConfigs.Any())
            {
                var existingConfigs = await _context.OfferConfigurationByLead.Where(x => x.LeadId == request.Request.LeadId && x.CompanyId == request.Request.ProductSlabConfigs.First().CompanyId && x.ProductId == request.Request.ProductSlabConfigs.First().ProductId && x.IsActive && !x.IsDeleted).ToListAsync();
                if (existingConfigs != null && existingConfigs.Any())
                {
                    foreach (var config in existingConfigs)
                    {
                        config.IsActive = false;
                        config.IsDeleted = true;
                        _context.Entry(config).State = EntityState.Modified;
                    }
                }
                List<OfferConfigurationByLead> configList = new List<OfferConfigurationByLead>();
                foreach (var slab in request.Request.ProductSlabConfigs)
                {
                    configList.Add(new OfferConfigurationByLead
                    {
                        LeadId = request.Request.LeadId,
                        CompanyId = slab.CompanyId,
                        ProductId = slab.ProductId,
                        SlabType = slab.SlabType,
                        MinLoanAmount = slab.MinLoanAmount,
                        MaxLoanAmount = slab.MaxLoanAmount,
                        ValueType = slab.ValueType,
                        MinValue = slab.MinValue,
                        MaxValue = slab.MaxValue,
                        IsFixed = slab.IsFixed,
                        SharePercentage = slab.SharePercentage,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
                _context.AddRange(configList);
                if (_context.SaveChanges() > 0)
                {
                    reply.Status = true;
                    reply.Response = true;
                    reply.Message = "Data Saved";
                }
            }
            return reply;
        }

        public async Task<GRPCReply<long>> CurrentActivityCompleteForNBFC(GRPCRequest<long> request)
        {
            GRPCReply<long> res = new GRPCReply<long>();

            res.Status = false;
            res.Message = "Failed";
            res.Response = 0;

            var leadinfo = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request && x.IsActive && !x.IsDeleted);

            var leadActivities = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.Request && x.ActivityMasterName == ActivityConstants.Congratulations && x.IsActive && !x.IsDeleted && !x.IsCompleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
            if (leadinfo != null && leadActivities != null)
            {
                leadActivities.IsCompleted = true;
                if (leadActivities.ActivityMasterName == ActivityConstants.Congratulations.ToString())
                {
                    leadActivities.IsApproved = 1;
                }
                _context.Entry(leadActivities).State = EntityState.Modified;

                leadinfo.Status = LeadBusinessLoanStatusConstants.LoanAvailed;
                _context.Entry(leadinfo).State = EntityState.Modified;
                _context.SaveChanges();

                res.Status = true;
                res.Message = "Success";
                res.Response = leadActivities.Id;
            }
            return res;
        }

        public async Task<GRPCReply<List<LeadAnchorProductRequest>>> GetLeadOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> req)
        {
            GRPCReply<List<LeadAnchorProductRequest>> reply = new GRPCReply<List<LeadAnchorProductRequest>>();
            var lead = (await _context.Leads.Where(x => x.Id == req.Request.LeadId).Include(x => x.CompanyLeads).FirstOrDefaultAsync());
            if (lead != null)
            {
                var leadOffer = await _context.LeadOffers.Where(x => x.IsActive && !x.IsDeleted && x.LeadId == lead.Id && x.CompanyIdentificationCode != null).Select(y => new LeadAnchorProductRequest
                {
                    NBFCCompanyId = y.NBFCCompanyId,
                    LastModifyBy = y.LastModifiedBy ?? "",
                    Created = y.Created,
                    ProductCode = lead.ProductCode,
                    UserId = lead.UserName,
                    ProductId = lead.ProductId
                }).ToListAsync();
                //var RejectionMsg = await _context.BusinessLoanNBFCUpdate.Where(x => x.IsActive && !x.IsDeleted && x.LeadId == lead.Id && x.CompanyIdentificationCode != null && x.CompanyIdentificationCode == req.Request.Role).Select(x => x.NBFCRemark).FirstOrDefaultAsync();
                //LeadAnchorProductRequest leadAnchorProductRequest = new LeadAnchorProductRequest();
                //leadAnchorProductRequest.ProductId = lead.ProductId;
                //leadAnchorProductRequest.UserId = lead.UserName;
                //leadAnchorProductRequest.AnchorCompanyId = lead.CompanyLeads.Any() ? lead.CompanyLeads.FirstOrDefault(x => x.LeadProcessStatus == 2)?.CompanyId : null;
                //leadAnchorProductRequest.ProductCode = lead.ProductCode;
                //if (leadOffer != null && leadOffer.Any())
                //{
                //    leadAnchorProductRequest.NBFCCompanyId = leadOffer.NBFCCompanyId;
                //    leadAnchorProductRequest.LastModifyBy = leadOffer.LastModifiedBy ?? "";
                //    leadAnchorProductRequest.Created = leadOffer.Created;
                //}
                if (leadOffer != null && leadOffer.Any())
                {
                    reply.Response = leadOffer;
                    reply.Status = true;
                }
            }
            return reply;
        }

        public async Task<GRPCReply<Loandetaildc>> GetNBFCOfferByLeadId(GRPCRequest<GetGenerateOfferByFinanceRequestDc> request)
        {
            GRPCReply<Loandetaildc> res = new GRPCReply<Loandetaildc>();

            var leadOffer = await _context.LeadOffers.Where(x => x.LeadId == request.Request.LeadId && x.NBFCCompanyId == request.Request.NbfcCompanyId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            var nbfcOffer = await _context.nbfcOfferUpdate.Where(x => x.LeadId == request.Request.LeadId && x.NBFCCompanyId == request.Request.NbfcCompanyId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (leadOffer != null && nbfcOffer != null)
            {
                Loandetaildc obj = new Loandetaildc
                {
                    LoanAmount = nbfcOffer.LoanAmount ?? 0,
                    GST = nbfcOffer.GST,
                    loanIntAmt = nbfcOffer.LoanInterestAmount ?? 0,
                    monthlyPayment = nbfcOffer.MonthlyEMI ?? 0,
                    ProcessingFeeRate = nbfcOffer.ProcessingFeeRate,
                    ProcessingfeeTax = nbfcOffer.ProcessingFeeTax ?? 0,
                    processing_fee = nbfcOffer.ProcessingFeeAmount ?? 0,
                    RateOfInterest = nbfcOffer.InterestRate ?? 0,
                    Tenure = nbfcOffer.Tenure ?? 0,
                    UserName = nbfcOffer.CreatedBy,
                    CreatedDate = nbfcOffer.Created,
                    PFType = nbfcOffer.PFType,
                    RejectionReason = nbfcOffer.NBFCRemark,
                    offerInitiateDate = leadOffer.Created
                };

                res.Response = obj;
                res.Status = true;
                res.Message = "Data Found";
            }
            else
            {
                res.Status = false;
                res.Message = "Data Not Found";
            }
            return res;
        }

        public async Task<GRPCReply<List<BLAccountResponseDC>>> GetNBFCBLAccountList(BLAccountRequestDc obj)
        {
            GRPCReply<List<BLAccountResponseDC>> gRPCReply = new GRPCReply<List<BLAccountResponseDC>>();

            //if (!string.IsNullOrEmpty(obj.Role) && obj.Role.ToLower() == UserRoleConstants.MASOperationExecutive.ToLower())
            //{
            //    obj.Role = CompanyIdentificationCodeConstants.MASFIN;
            //}
            //else if (!string.IsNullOrEmpty(obj.Role) && obj.Role.ToLower() == UserRoleConstants.AYEOperationExecutive.ToLower())
            //{
            //    obj.Role = CompanyIdentificationCodeConstants.AYEFIN;
            //}

            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var cityname = new SqlParameter("CityName", obj.CityName);
            var keyword = new SqlParameter("Keyword", obj.Keyword);
            //var min = new SqlParameter("Min", obj.Min);
            //var max = new SqlParameter("Max", obj.Max);
            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);
            //var anchorId = new SqlParameter("AnchorId", obj.AnchorId);
            var status = new SqlParameter("Status", obj.Status);
            var cityId = new SqlParameter("CityId", obj.CityId);

            //var role = new SqlParameter("role", obj.Role);
            var nbfcCompanyId = new SqlParameter("NbfcCompanyId", obj.NbfcCompanyId);

            var anchorIds = new DataTable();
            anchorIds.Columns.Add("IntValue");
            foreach (var anchor in obj.AnchorId)
            {
                var dr = anchorIds.NewRow();
                dr["IntValue"] = anchor;
                anchorIds.Rows.Add(dr);
            }
            var AnchorIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = anchorIds
            };

            var list = await _context.Database.SqlQueryRaw<BLAccountResponseDC>(" exec NBFCBLAccountList @Fromdate,@ToDate,@CityName,@Keyword,@Skip,@Take,@AnchorId,@Status,@CityId,@NbfcCompanyId", fromdate, toDate, cityname, keyword, skip, take, AnchorIds, status, cityId, nbfcCompanyId).AsAsyncEnumerable().ToListAsync();
            if (list.Any())
            {
                gRPCReply.Response = list.ToList();
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
            }
            else
            {
                gRPCReply.Response = new List<BLAccountResponseDC>();
                gRPCReply.Status = false;
                gRPCReply.Message = "Not Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<LeadCibilDataResponseDc>> GetLeadCibilData(GRPCRequest<List<long>> request)
        {
            GRPCReply<LeadCibilDataResponseDc> gRPCReply = new GRPCReply<LeadCibilDataResponseDc>();
            if (request.Request.Any())
            {
                var joinedList = from l in _context.Leads
                                 join req in request.Request
                                 on l.Id equals req
                                 where l.IsActive == true && l.IsDeleted == false && l.CompanyLeads.Any()
                                 select l;
                int cibilCountGreater = 0;
                int cibilCountLess = 0;
                int cibilCountZero = 0;
                int TotalCount = await joinedList.CountAsync();
                if (joinedList.Any())
                {
                    foreach (var l in joinedList)
                    {
                        if (l.CreditScore == 0 || l.CreditScore == null)
                        {
                            cibilCountZero = cibilCountZero + 1;
                        }
                        else if (l.CreditScore > 700)
                        {
                            cibilCountGreater = cibilCountGreater + 1;
                        }
                        else
                        {
                            cibilCountLess = cibilCountLess + 1;
                        }
                    }
                    if (TotalCount > 0)
                    {
                        LeadCibilDataResponseDc leadCibilDataResponseDc = new LeadCibilDataResponseDc();
                        leadCibilDataResponseDc.CibilGreaterPercentage = ((cibilCountGreater / TotalCount) * 100);
                        leadCibilDataResponseDc.CibilLessPercentage = ((cibilCountLess / TotalCount) * 100);
                        leadCibilDataResponseDc.CibilZeroPercentage = ((cibilCountZero / TotalCount) * 100);
                        gRPCReply.Response = leadCibilDataResponseDc;
                        gRPCReply.Status = true;
                    }
                }
                else
                {
                    gRPCReply.Response = new LeadCibilDataResponseDc();
                    gRPCReply.Status = false;
                }
            }
            else
            {
                gRPCReply.Response = new LeadCibilDataResponseDc();
                gRPCReply.Status = false;
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<DashBoardCohortData>> GetDashBoardTATData(GRPCRequest<DashboardTATLeadtDetailRequestDc> req)
        {
            GRPCReply<DashBoardCohortData> gRPCReply = new GRPCReply<DashBoardCohortData>();
            var cityIds = new DataTable();
            cityIds.Columns.Add("IntValue");
            if (req.Request.CityId != null)
            {
                foreach (var id in req.Request.CityId)
                {
                    var dr = cityIds.NewRow();
                    dr["IntValue"] = id;
                    cityIds.Rows.Add(dr);
                }
            }
            else
            {
                var dr = cityIds.NewRow();
                dr["IntValue"] = 0;
                cityIds.Rows.Add(dr);
            }
            //var allCityIds = new SqlParameter
            //{
            //    ParameterName = "CityId",
            //    SqlDbType = SqlDbType.Structured,
            //    TypeName = "dbo.IntValues",
            //    Value = cityIds
            //};
            var allCityIds = new SqlParameter
            {
                ParameterName = "CityId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = cityIds.Rows.Count > 0 ? cityIds : (object)DBNull.Value
            };

            var ProductType = new SqlParameter("ProductType", "");
            var FromDate = new SqlParameter("FromDate", req.Request.FromDate);
            var ToDate = new SqlParameter("ToDate", req.Request.ToDate);

            var Cohort = _context.Database.SqlQueryRaw<DashBoardCohortData>
                    ("exec GetTATData @ProductType,@CityId, @FromDate, @ToDate"
                    , ProductType, allCityIds, FromDate, ToDate).AsEnumerable().FirstOrDefault();
            if (Cohort != null)
            {
                gRPCReply.Response = Cohort;
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<UpdateKYCStatusResponseDc>> UpdateKYCStatus(UpdateKYCStatusDc request)
        {
            GRPCReply<UpdateKYCStatusResponseDc> reply = new GRPCReply<UpdateKYCStatusResponseDc> { Message = "Something went wrong!!" };
            LeadActivityMasterProgresses leadActivityMasterProgresses = new LeadActivityMasterProgresses
            {
                ActivityMasterId = 0,
                ActivityMasterName = "",
                IsApproved = 0,
                IsCompleted = false,
                LeadMasterId = 0,
                Sequence = 0
            };

            //bool result = true;
            if (request != null && request.LeadId > 0)
            {
                var Lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.LeadId);
                var AllleadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.LeadId).ToListAsync();
                if (AllleadActivity != null)
                {
                    //if (request.ActivityMasterName.ToUpper() == KYCMasterConstants.BuisnessDetail.ToUpper() || request.ActivityMasterName.ToUpper() == KYCMasterConstants.PersonalDetail.ToUpper())
                    //{
                    //    leadActivityMasterProgresses = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId &&
                    //                                                          x.ActivityMasterName.ToUpper() == request.ActivityMasterName.ToUpper() &&
                    //                                                          x.IsActive && !x.IsDeleted);
                    //    Lead.Status = request.ActivityMasterName.ToUpper() == KYCMasterConstants.PersonalDetail.ToUpper() ? (LeadStatusEnum.KYCSuccess.ToString()) : (LeadStatusEnum.Submitted.ToString());
                    //    //Lead.LastModified = DateTime.Now;
                    //    //_context.Entry(Lead).State = EntityState.Modified;
                    //}
                    //else if (request.ActivityMasterName.ToUpper() == KYCMasterConstants.PAN.ToUpper() || request.ActivityMasterName.ToUpper() == KYCMasterConstants.Aadhar.ToUpper())
                    //{
                        string? SubActivityName = request.ActivityMasterName ?? "";
                    leadActivityMasterProgresses = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId &&
                                                                         x.ActivityMasterName.ToUpper() == "KYC"
                                                                         && x.IsDeleted);
                                                                             //x.SubActivityMasterName == SubActivityName &&
                                                                             //x.IsActive && !x.IsDeleted);
                       
                        //Lead.LastModified = DateTime.Now;
                        //_context.Entry(Lead).State = EntityState.Modified;
                    //}

                    //var leadActivityMasterProgresses = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId &&
                    //                                                          x.ActivityMasterId == request.ActivityMasterId &&
                    //                                                          x.SubActivityMasterId == request.SubActivityMasterId &&
                    //                                                          x.IsActive && !x.IsDeleted);
                    if (leadActivityMasterProgresses != null)
                    {
                        var RejectleadActivity = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId
                                                 && x.ActivityMasterName == ActivityEnum.Rejected.ToString());


                        foreach (var item in AllleadActivity.Where(x => x.Sequence >= leadActivityMasterProgresses.Sequence))
                        {
                            if(leadActivityMasterProgresses.Sequence == item.Sequence)
                            {
                                item.IsCompleted = true;
                                item.IsApproved = 1;
                            }
                            item.IsActive = true;
                            item.IsDeleted = false;
                            _context.Entry(item).State = EntityState.Modified;
                        }
                        RejectleadActivity.IsActive = false;
                        RejectleadActivity.Sequence = leadActivityMasterProgresses.Sequence + 1;
                        RejectleadActivity.RejectMessage = "Status Changed By " + request.UserName;//Username Add
                        _context.Entry(RejectleadActivity).State = EntityState.Modified;
                        Lead.Status = LeadStatusEnum.KYCSuccess.ToString();
                        Lead.LastModified = DateTime.Now;
                        _context.Entry(Lead).State = EntityState.Modified;
                        var res = await _context.SaveChangesAsync();
                        if (res > 0)
                        {
                            UpdateKYCStatusResponseDc updateKYCStatusResponseDc = new UpdateKYCStatusResponseDc();
                            updateKYCStatusResponseDc.UserName = Lead.UserName;
                            updateKYCStatusResponseDc.ProductCode = Lead.ProductCode;
                            reply.Status = true;
                            reply.Response = updateKYCStatusResponseDc;
                            reply.Message = "Status Changed Successfully.";
                        }
                    }
                }
            }
            return reply;
        }

        public async Task<GRPCReply<string>> GetDSALeadCodeByCreatedBy(GRPCRequest<string> req)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            var response = await _context.Leads.Where(x => x.UserName == req.Request && x.IsActive && !x.IsDeleted && x.ProductCode == ProductCodeConstants.DSA).Select(y => y.LeadCode).FirstOrDefaultAsync();
            if (response != null && !string.IsNullOrEmpty(response))
            {
                gRPCReply.Response = response;
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<DSAMISLeadResponseDC>>> GetDSAMISLeadData(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<DSAMISLeadResponseDC>> response = new GRPCReply<List<DSAMISLeadResponseDC>> { Message = "Data Not Found!!!" };
            response.Response = await (from l in _context.Leads
                                       join req in request.Request on l.Id equals req
                                       join p in _context.PersonalDetails on l.Id equals p.LeadId
                                       join cl in _context.CompanyLead on l.Id equals cl.LeadId into a
                                       from cl in a.DefaultIfEmpty()
                                       where l.IsActive && !l.IsDeleted && p.IsActive && !p.IsDeleted
                                       select new DSAMISLeadResponseDC
                                       {
                                           LeadId = l.Id,
                                           StateName = p.PermanentStateName,
                                           CityName = p.PermanentCityName,
                                           CreatedDate = l.Created,
                                           LeadCode = l.LeadCode,
                                           PANNo = p.PanMaskNO,
                                           SKCode = cl != null && cl.UserUniqueCode != null ? cl.UserUniqueCode : "",
                                       }).ToListAsync();
            if (response.Response != null && response.Response.Any())
            {
                response.Status = true;
                response.Message = "Data Found";
            }
            return response;
        }

        public async Task<GRPCReply<string>> GenerateAyeToken()
        {
            GRPCReply<string> response = new GRPCReply<string> { Message = "Data Not Found!!!" };
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.GenerateToken();
            }

            return response;
        }
        public async Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> request)
        {
            GRPCReply<string> response = new GRPCReply<string> { Message = "Data Not Found!!!" };
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.AddLead(request);
            }

            return response;
        }

        public async Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> request)
        {
            GRPCReply<string> response = new GRPCReply<string> { Message = "Data Not Found!!!" };
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.GetWebUrl(request);
            }

            return response;
        }

        public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> request)
        {
            GRPCReply<CheckCreditLineData> response = new GRPCReply<CheckCreditLineData> { Message = "Data Not Found!!!" };
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.CheckCreditLine(request);
            }

            return response;
        }

        public async Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> request)
        {
            GRPCReply<string> response = new GRPCReply<string> { Message = "Data Not Found!!!" };
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.TransactionSendOtp(request);
            }

            return response;
        }
        public async Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> request)
        {
            GRPCReply<string> response = new GRPCReply<string> { Message = "Data Not Found!!!" };
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.TransactionVerifyOtp(request);
            }

            return response;
        }

        public async Task<GRPCReply<long>> UpdateActivityForAyeFin(GRPCRequest<long> request)
        {
            GRPCReply<long> res = new GRPCReply<long>() { Message = "Failed" };
            var leadActivities = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.Request && x.IsActive && !x.IsDeleted && !x.IsCompleted).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
            if (leadActivities != null)
            {
                leadActivities.IsCompleted = true;
                if (leadActivities.ActivityMasterName == ActivityConstants.DisbursementCompleted.ToString())
                {
                    leadActivities.IsApproved = 1;
                }
                _context.Entry(leadActivities).State = EntityState.Modified;
                _context.SaveChanges();

                res.Status = true;
                res.Message = "Success";
                res.Response = leadActivities.Id;
            }
            return res;
        }
    }
}
