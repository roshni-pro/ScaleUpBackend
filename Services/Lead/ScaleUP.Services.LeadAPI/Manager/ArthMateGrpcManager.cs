using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.ArthMate;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadModels;
using ScaleUP.Global.Infrastructure.Constants;
using Grpc.Core;
using ScaleUP.Services.LeadAPI.Controllers.NBFC;
using ScaleUP.Global.Infrastructure.Constants.Product;
using Org.BouncyCastle.Ocsp;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using Microsoft.AspNetCore.Mvc;
using ScaleUP.Services.LeadAPI.Migrations;
using static MassTransit.Monitoring.Performance.BuiltInCounters;
using ScaleUP.Services.LeadDTO.eSign;
using System;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Enum;
using Azure.Core;
using Microsoft.AspNetCore;
using Microsoft.Data.SqlClient;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class ArthMateGrpcManager
    {
        private readonly LeadApplicationDbContext _context;
        private readonly LeadNBFCFactory _leadNBFCFactory;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        //private readonly ArthMateNBFCHelper _ArthMateNBFCHelper;
        private readonly LeadGrpcManager _leadGrpcManager;
        private readonly ILogger<ArthMateGrpcManager> _logger;

        private readonly LeadHistoryManager _leadHistoryManager;
        private readonly IMassTransitService _massTransitService;

        public ArthMateGrpcManager(LeadApplicationDbContext context, LeadNBFCFactory leadNBFCFactory, LeadNBFCSubActivityManager leadNBFCSubActivityManager,
        ILogger<ArthMateGrpcManager> logger, LeadGrpcManager leadGrpcManager /*ArthMateNBFCHelper arthMateNBFCHelper*/
                      , LeadHistoryManager leadHistoryManager, IMassTransitService massTransitService)

        {
            _context = context;
            _leadNBFCFactory = leadNBFCFactory;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _logger = logger;
            //_ArthMateNBFCHelper = arthMateNBFCHelper;
            _leadGrpcManager = leadGrpcManager;
            _leadHistoryManager = leadHistoryManager;
            _massTransitService = massTransitService;
        }
        public async Task<GRPCReply<long>> ArthMateAScoreCallback(GRPCRequest<AScoreWebhookDc> request)
        {
            var result = new GRPCReply<long>();

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
            ArthMateNBFCHelper _ArthMateNBFCHelper = new ArthMateNBFCHelper();


            ArthMateWebhookResponse arthMateWebhookResponse = new ArthMateWebhookResponse
            {
                Response = request.Request.request_id,
                IsActive = true,
                CreatedBy = "",
                IsDeleted = false,
                Created = currentDateTime,
            };
            _context.ArthMateWebhookResponse.Add(arthMateWebhookResponse);
            _context.SaveChanges();

            _logger.LogInformation("Athmate AScore Callback (ArthMateAScoreCallback()) - request added to ArthMateWebhookResponse");
            if (request.Request.status == "success")
            {
                #region to stop duplicate req
                if (request.Request.request_id != null)
                {
                }
                #endregion

                var arthmateupdate = _context.ArthMateUpdates.Where(x => x.AScoreRequestId == request.Request.request_id && x.IsActive == true && !x.IsDeleted).FirstOrDefault();
                if (arthmateupdate != null && arthmateupdate.NBFCCompanyId > 0)
                {
                    arthmateupdate.IsAScoreWebhookHit = true;
                    arthmateupdate.LastModified = currentDateTime;
                    _context.Entry(arthmateupdate).State = EntityState.Modified;
                    _context.SaveChanges();

                    arthMateWebhookResponse.LeadId = arthmateupdate.LeadId;
                    _context.Entry(arthMateWebhookResponse).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Athmate AScore Callback (ArthMateAScoreCallback()) - IsAScoreWebhookHit = true saved");
                    var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
                    if (nbfcService != null)
                    {
                        _logger.LogInformation("Athmate AScore Callback (ArthMateAScoreCallback()) - Calling GenerateOffer method");
                        await nbfcService.GenerateOffer(arthmateupdate.LeadId, arthmateupdate.NBFCCompanyId ?? 0);
                    }
                }
                else
                {
                    _logger.LogInformation("Athmate AScore Callback (ArthMateAScoreCallback()) - arthmateupdate is null/ arthmateupdate.NBFCCompanyId==0");
                }
            }
            else
            {
                _logger.LogInformation("Athmate AScore Callback (ArthMateAScoreCallback()) - request.Request.status is not success ");
            }
            return result;
        }
        public async Task<GRPCReply<string>> SaveArthMateNBFCAgreement(GRPCRequest<ArthMateLeadAgreementDc> req)
        {
            GRPCReply<string> res = new GRPCReply<string>();


            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            var ArthMateUpdates = await _context.ArthMateUpdates.Where(x => x.LeadId == req.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            var lead = await _context.Leads.Where(x => x.Id == req.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();

            if (lead != null && ArthMateUpdates != null && req.Request.ProductCompanyConfig != null)
            {
                string eSignUrl = "";

                if (req.Request.ProductCompanyConfig.IseSignEnable)
                {
                    var leadagreement = await _context.leadAgreements.Where(x => x.LeadId == req.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    if (leadagreement != null)
                    {
                        var eSignRequests = await _context.eSignResponseDocumentIds.Where(x => x.LeadId == req.Request.LeadId && x.DocumentId == leadagreement.DocumentId && x.eSignRemark == null && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                        if (eSignRequests != null)
                        {
                            if (eSignRequests.ExpiryTime.Date >= currentDateTime.Date)
                            {
                                res.Response = leadagreement.eSignUrl;
                                res.Message = "success";
                                res.Status = true;
                                //return res;
                            }
                            else
                            {
                                var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
                                if (nbfcService != null)
                                {
                                    var eSignSessionres = await nbfcService.eSignSessionAsync(req.Request.AgreementPdfUrl, req.Request.LeadId);
                                    if (eSignSessionres.Status)
                                    {
                                        var eSignResponse = JsonConvert.DeserializeObject<eSignSessionResponse>(eSignSessionres.Data.ToString());
                                        if (eSignResponse != null)
                                        {
                                            eSignUrl = eSignResponse.result.signingDetails[0].signUrl;

                                            leadagreement.DocumentId = eSignResponse.result.documentId;
                                            leadagreement.eSignUrl = eSignResponse.result.signingDetails[0].signUrl;
                                            leadagreement.DocUnSignUrl = req.Request.AgreementPdfUrl;
                                            _context.Entry(leadagreement).State = EntityState.Modified;

                                            int expiryDate = eSignResponse.result.signingDetails[0].expiryTime;
                                            var result = await addeSignResponseDocumentId(req.Request.LeadId, eSignResponse.result.documentId, expiryDate);
                                            await _context.SaveChangesAsync();
                                            res.Message = "success";
                                            res.Response = eSignUrl;
                                            res.Status = true;
                                        }
                                    }
                                    else
                                    {
                                        res.Message = eSignSessionres.Data.ToString();
                                    }
                                }
                            }
                        }
                    }
                    if (leadagreement == null)
                    {
                        var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
                        if (nbfcService != null)
                        {
                            var eSignSessionres = await nbfcService.eSignSessionAsync(req.Request.AgreementPdfUrl, req.Request.LeadId);
                            if (eSignSessionres.Status)
                            {
                                var eSignResponse = JsonConvert.DeserializeObject<eSignSessionResponse>(eSignSessionres.Data.ToString());
                                if (eSignResponse != null)
                                {
                                    eSignUrl = eSignResponse.result.signingDetails[0].signUrl;
                                    await _context.leadAgreements.AddAsync(new LeadAgreement
                                    {
                                        DocUnSignUrl = req.Request.AgreementPdfUrl,
                                        LeadId = req.Request.LeadId,
                                        DocumentId = eSignResponse.result.documentId,
                                        eSignUrl = eSignUrl,
                                        Status = "Pending",
                                        IsActive = true,
                                        IsDeleted = false
                                    });
                                    int expiryDate = eSignResponse.result.signingDetails[0].expiryTime;
                                    var result = await addeSignResponseDocumentId(req.Request.LeadId, eSignResponse.result.documentId, expiryDate);
                                    await _context.SaveChangesAsync();
                                    res.Message = "success";
                                    res.Response = eSignUrl;
                                    res.Status = true;
                                }
                            }
                            else
                            {
                                res.Message = eSignSessionres.Data.ToString();
                            }
                        }
                    }
                }
                else
                {
                    //ArthMateUpdates.AgreementPdfURL = req.Request.AgreementPdfUrl;
                    //_context.Entry(ArthMateUpdates).State = EntityState.Modified;
                    //_context.SaveChanges();
                    //res.Message = "Successfully saved";

                    var leadagreement = await _context.leadAgreements.Where(x => x.LeadId == req.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    if (leadagreement != null)
                    {
                        leadagreement.DocSignedUrl = req.Request.AgreementPdfUrl;
                        leadagreement.LastModified = currentDateTime;
                        _context.Entry(leadagreement).State = EntityState.Modified;
                    }
                    else
                    {
                        await _context.leadAgreements.AddAsync(new LeadAgreement
                        {
                            DocUnSignUrl = req.Request.AgreementPdfUrl,
                            DocSignedUrl = "",
                            LeadId = req.Request.LeadId,
                            DocumentId = "",
                            eSignUrl = "",
                            Status = "",
                            IsActive = true,
                            IsDeleted = false,
                            Created = currentDateTime
                        });
                    }
                    lead.IsAgreementAccept = true;
                    lead.AgreementDate = currentDateTime;
                    _context.Entry(lead).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    _leadNBFCSubActivityManager.UpdateLeadProgressStatus(req.Request.LeadId, ActivityConstants.ArthmateAgreement);

                    //------------------S : Make log---------------------
                    #region Make History
                    var resultHistory = await _leadHistoryManager.GetLeadHistroy(req.Request.LeadId, "SaveArthMateNBFCAgreement_Prepare");
                    LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                    {
                        LeadId = req.Request.LeadId,
                        UserID = resultHistory.UserId,
                        UserName = "",
                        EventName = "Agreement Prepare-Arthmate",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                        Narretion = resultHistory.Narretion,
                        NarretionHTML = resultHistory.NarretionHTML,
                        CreatedTimeStamp = resultHistory.CreatedTimeStamp
                    };
                    await _massTransitService.Publish(histroyEvent);
                    #endregion
                    //------------------E : Make log---------------------

                    res.Message = "Successfully saved";
                }
            }
            return res;
        }

        // new for Esign
        public async Task<GRPCReply<string>> SaveDsaEsignAgreement(GRPCRequest<EsignLeadAgreementDc> req)
        {
            GRPCReply<string> res = new GRPCReply<string>();

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            if (req != null)
            {
                string eSignUrl = "";
                var leadagreement = await _context.leadAgreements.Where(x => x.LeadId == req.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                if (leadagreement != null)
                {
                    var eSignRequests = await _context.eSignResponseDocumentIds.Where(x => x.LeadId == req.Request.LeadId && x.DocumentId == leadagreement.DocumentId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    if (eSignRequests != null)
                    {
                        if (eSignRequests.ExpiryTime.Date >= currentDateTime.Date)
                        {
                            res.Response = leadagreement.eSignUrl;
                            res.Message = "success";
                            res.Status = true;
                            //return res;
                        }
                        else
                        {
                            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
                            if (nbfcService != null)
                            {
                                var eSignSessionres = await nbfcService.eSignSessionAsync(req.Request.AgreementPdfUrl, req.Request.LeadId);
                                if (eSignSessionres.Status)
                                {
                                    var eSignResponse = JsonConvert.DeserializeObject<eSignSessionResponse>(eSignSessionres.Data.ToString());
                                    if (eSignResponse != null)
                                    {
                                        eSignUrl = eSignResponse.result.signingDetails[0].signUrl;

                                        leadagreement.DocumentId = eSignResponse.result.documentId;
                                        leadagreement.eSignUrl = eSignResponse.result.signingDetails[0].signUrl;
                                        leadagreement.DocUnSignUrl = req.Request.AgreementPdfUrl;
                                        _context.Entry(leadagreement).State = EntityState.Modified;

                                        int expiryDate = eSignResponse.result.signingDetails[0].expiryTime;
                                        var result = await addeSignResponseDocumentId(req.Request.LeadId, eSignResponse.result.documentId, expiryDate);
                                        await _context.SaveChangesAsync();
                                        res.Message = "success";
                                        res.Response = eSignUrl;
                                        res.Status = true;
                                    }
                                }
                                else
                                {
                                    res.Message = eSignSessionres.Data.ToString();
                                }
                            }
                        }
                    }
                }
                if (leadagreement == null)
                {
                    var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
                    if (nbfcService != null)
                    {
                        var eSignSessionres = await nbfcService.eSignSessionAsync(req.Request.AgreementPdfUrl, req.Request.LeadId);
                        if (eSignSessionres.Status)
                        {
                            var eSignResponse = JsonConvert.DeserializeObject<eSignSessionResponse>(eSignSessionres.Data.ToString());
                            if (eSignResponse != null)
                            {
                                eSignUrl = eSignResponse.result.signingDetails[0].signUrl;
                                await _context.leadAgreements.AddAsync(new LeadAgreement
                                {
                                    DocUnSignUrl = req.Request.AgreementPdfUrl,
                                    LeadId = req.Request.LeadId,
                                    DocumentId = eSignResponse.result.documentId,
                                    eSignUrl = eSignUrl,
                                    Status = "Pending",
                                    IsActive = true,
                                    IsDeleted = false
                                });
                                int expiryDate = eSignResponse.result.signingDetails[0].expiryTime;
                                var result = await addeSignResponseDocumentId(req.Request.LeadId, eSignResponse.result.documentId, expiryDate);
                                await _context.SaveChangesAsync();
                                res.Message = "success";
                                res.Response = eSignUrl;
                                res.Status = true;
                            }
                        }
                        else
                        {
                            res.Message = eSignSessionres.Data.ToString();
                        }
                    }
                }
            }
            return res;
        }

        public async Task<GRPCReply<string>> SaveOfferEmiDetailsPdf(GRPCRequest<ArthMateLeadAgreementDc> req)
        {
            GRPCReply<string> res = new GRPCReply<string>();
            var ArthMateUpdates = await _context.ArthMateUpdates.Where(x => x.LeadId == req.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (ArthMateUpdates != null)
            {
                ArthMateUpdates.OfferEmiDetailPdfURL = req.Request.AgreementPdfUrl;
                _context.Entry(ArthMateUpdates).State = EntityState.Modified;
                _context.SaveChanges();

                res.Message = "Successfully saved";
                res.Response = "Successfully saved";
                res.Status = true;
            }
            return res;
        }
        public async Task<GRPCReply<ArthMateLoanDataResDc>> CompositeDisbursement(GRPCRequest<ArthmatDisbursementWebhookRequest> request)
        {

            ArthMateLoanDataResDc arthMateLoanDataResDc = new ArthMateLoanDataResDc();
            arthMateLoanDataResDc.Loan = new LoanDc();
            arthMateLoanDataResDc.leaddc = new Leaddc();
            GRPCReply<ArthMateLoanDataResDc> gRPCReply = new GRPCReply<ArthMateLoanDataResDc> { Response = arthMateLoanDataResDc };
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            ArthMateWebhookResponse arthMateWebhookResponse = new ArthMateWebhookResponse
            {
                Response = request.Request.Data,
                IsActive = true,
                CreatedBy = "",
                IsDeleted = false,
                Created = currentDateTime,
            };
            _context.ArthMateWebhookResponse.Add(arthMateWebhookResponse);
            await _context.SaveChangesAsync();
            var compositeDisbursementWebhook = JsonConvert.DeserializeObject<CompositeDisbursementWebhookDc>(request.Request.Data);



            var leadloan = await _context.LeadLoan.Where(x => x.loan_id == compositeDisbursementWebhook.data.loan_id && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();


            LoanDc LeadLoanData = new LoanDc();
            if (leadloan != null)
            {
                var leadDetail = await _context.Leads.Where(x => x.Id == leadloan.LeadMasterId).Select(x => new { x.Id, x.CreatedBy }).FirstOrDefaultAsync();
                if (leadDetail != null)
                {
                    gRPCReply.Response.leaddc.LeadCreatedUserId = leadDetail.CreatedBy;
                }

                var Disbursementresponse = JsonConvert.SerializeObject(request.Request);
                arthMateWebhookResponse.LeadId = leadloan.LeadMasterId ?? 0;
                _context.Entry(arthMateWebhookResponse).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _leadNBFCSubActivityManager.UpdateLeadMasterStatus(leadloan.LeadMasterId ?? 0, LeadBusinessLoanStatusConstants.LoanAvailed);
                CompositeDisbursementWebhookResponse obj = new CompositeDisbursementWebhookResponse();
                obj.Response = Disbursementresponse;
                obj.status_code = compositeDisbursementWebhook.data.status_code;
                obj.loan_id = compositeDisbursementWebhook.data.loan_id;
                obj.partner_loan_id = compositeDisbursementWebhook.data.partner_loan_id;
                obj.net_disbur_amt = compositeDisbursementWebhook.data.net_disbur_amt;
                obj.utr_number = compositeDisbursementWebhook.data.utr_number;
                obj.utr_date_time = compositeDisbursementWebhook.data.utr_date_time;
                obj.txn_id = compositeDisbursementWebhook.data.txn_id;
                obj.LeadMasterId = leadloan.LeadMasterId ?? 0;
                obj.IsActive = true;
                obj.IsDeleted = false;
                _context.CompositeDisbursementWebhookResponse.Add(obj);
                _context.SaveChanges();
                _logger.LogInformation($"Athmate CompositeDisbursement Callback (CompositeDisbursement()) - request saved in CompositeDisbursementWebhookResponse table");

                if (!_context.ArthmateDisbursements.Any(x => x.loan_id == obj.loan_id))
                {
                    _context.ArthmateDisbursements.Add(new ArthmateDisbursement
                    {
                        loan_id = obj.loan_id,
                        partner_loan_id = obj.partner_loan_id,
                        net_disbur_amt = obj.net_disbur_amt,
                        utr_date_time = obj.utr_date_time,
                        utr_number = obj.utr_number,
                        status_code = obj.status_code,
                        CreatedDate = currentDateTime,
                        LeadId = leadloan.LeadMasterId ?? 0
                    });
                    _logger.LogInformation($"Athmate CompositeDisbursement Callback (CompositeDisbursement()) - data saved in ArthmateDisbursements table");
                }
                else
                {
                    _logger.LogInformation($"Athmate CompositeDisbursement Callback (CompositeDisbursement()) - data already exists for loanid ({obj.loan_id}) in ArthmateDisbursements table");
                }
                //#region Loan detail
                LeadLoanData.LeadMasterId = leadloan.LeadMasterId;
                arthMateLoanDataResDc.Loan = LeadLoanData;
                gRPCReply.Response = arthMateLoanDataResDc;
                gRPCReply.Status = true;
                gRPCReply.Message = "";
                _leadNBFCSubActivityManager.UpdateLeadProgressStatus(leadloan.LeadMasterId ?? 0, ActivityConstants.Congratulations);
                _context.SaveChanges();
            }
            else
            {
                _logger.LogInformation($"Athmate CompositeDisbursement Callback (CompositeDisbursement()) - leadloan is null for loanid- ");
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<bool>> AddSlaLbaStamp(SlaLbaStampDc request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            if (request != null)
            {
                if (request.Id != null && request.Id > 0)
                {
                    var isStampExist = _context.ArthmateSlaLbaStampDetail.FirstOrDefault(x => x.Id != request.Id && (x.StampUrl == request.StampUrl || x.StampPaperNo == request.StampPaperNo) && !x.IsDeleted && x.IsActive);
                    if (isStampExist != null)
                    {
                        gRPCReply.Status = false;
                        gRPCReply.Message = "Stamp Already Exists.";
                        gRPCReply.Response = false;
                        return gRPCReply;
                    }
                }
                else
                {
                    var isStampExist = _context.ArthmateSlaLbaStampDetail.FirstOrDefault(x => (x.StampUrl == request.StampUrl || x.StampPaperNo == request.StampPaperNo) && !x.IsDeleted && x.IsActive);
                    if (isStampExist != null)
                    {
                        gRPCReply.Status = false;
                        gRPCReply.Message = "Stamp Already Exists.";
                        gRPCReply.Response = false;
                        return gRPCReply;
                    }
                }

                var existingStamp = await _context.ArthmateSlaLbaStampDetail.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive && !x.IsDeleted);
                if (existingStamp != null)
                {
                    existingStamp.PartnerName = request.PartnerName;
                    existingStamp.Purpose = request.Purpose;
                    existingStamp.StampAmount = request.StampAmount;
                    existingStamp.StampPaperNo = request.StampPaperNo;
                    existingStamp.StampUrl = request.StampUrl;
                    existingStamp.UsedFor = request.UsedFor;
                    _context.Entry(existingStamp).State = EntityState.Modified;
                }
                else
                {
                    _context.ArthmateSlaLbaStampDetail.Add(new ArthmateSlaLbaStampDetail
                    {
                        PartnerName = request.PartnerName,
                        Purpose = request.Purpose,
                        StampAmount = request.StampAmount,
                        StampPaperNo = request.StampPaperNo,
                        StampUrl = request.StampUrl,
                        UsedFor = request.UsedFor,
                        LeadmasterId = 0,
                        LoanId = "",
                        DateofUtilisation = null,
                        IsStampUsed = false,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
                if (_context.SaveChanges() > 0)
                {
                    gRPCReply.Response = true;
                    gRPCReply.Message = "Data Saved";
                    gRPCReply.Status = true;
                }
                else
                {
                    gRPCReply.Response = false;
                    gRPCReply.Message = "Data Not Saved";
                    gRPCReply.Status = false;
                }
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<ArthMateLoanDataResDc>> GetDisbursementAPI(GRPCRequest<long> request)
        {
            GRPCReply<ArthMateLoanDataResDc> gRPCReply = new GRPCReply<ArthMateLoanDataResDc>();
            ArthMateLoanDataResDc arthMateLoanDataResDc = new ArthMateLoanDataResDc();
            long Leadmasterid = request.Request;

            arthMateLoanDataResDc.arthmateDisbursementdc = new ArthmateDisbursementdc();

            if (Leadmasterid > 0)
            {
                ArthMateNBFCHelper _ArthMateNBFCHelper = new ArthMateNBFCHelper();
                var leadloan = _context.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (leadloan != null)
                {
                    DateConvertHelper _DateConvertHelper = new DateConvertHelper();
                    var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

                    var ArthmateDisbursement = _context.ArthmateDisbursements.Where(x => x.loan_id == leadloan.loan_id).FirstOrDefault();
                    if (ArthmateDisbursement != null)
                    {
                        var arthmateDisbursementdc = new ArthmateDisbursementdc
                        {
                            loan_id = ArthmateDisbursement.loan_id,
                            partner_loan_id = ArthmateDisbursement.partner_loan_id,
                            net_disbur_amt = ArthmateDisbursement.net_disbur_amt,
                            utr_date_time = ArthmateDisbursement.utr_date_time,
                            utr_number = ArthmateDisbursement.utr_number,
                            status_code = ArthmateDisbursement.status_code,
                            CreatedDate = currentDateTime
                        };
                        arthMateLoanDataResDc.arthmateDisbursementdc = arthmateDisbursementdc;
                        gRPCReply.Status = true;
                    }
                    else
                    {
                        var GetDisbursementApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateGetDisbursement && x.IsActive && !x.IsDeleted);
                        var Result = await _ArthMateNBFCHelper.GetDisbursementAPI(leadloan.loan_id, GetDisbursementApi.APIUrl, GetDisbursementApi.TAPIKey, GetDisbursementApi.TAPISecretKey, GetDisbursementApi.Id, Leadmasterid);
                        _context.ArthMateCommonAPIRequestResponses.Add(Result);
                        var resdata = JsonConvert.DeserializeObject<DisbursementDataDc>(Result.Response);
                        if (resdata != null && resdata.success)
                        {
                            var arthmateDisbursement = new ArthmateDisbursement
                            {
                                loan_id = resdata.data.loan_id,
                                partner_loan_id = resdata.data.partner_loan_id,
                                net_disbur_amt = resdata.data.net_disbur_amt,
                                utr_date_time = resdata.data.utr_date_time,
                                utr_number = resdata.data.utr_number,
                                status_code = resdata.data.status_code,
                                CreatedDate = currentDateTime
                            };
                            _context.Add(arthmateDisbursement);
                            _context.SaveChanges();

                            var arthmateDisbursementdc = new ArthmateDisbursementdc
                            {
                                loan_id = arthmateDisbursement.loan_id,
                                partner_loan_id = arthmateDisbursement.partner_loan_id,
                                net_disbur_amt = arthmateDisbursement.net_disbur_amt,
                                utr_date_time = arthmateDisbursement.utr_date_time,
                                utr_number = arthmateDisbursement.utr_number,
                                status_code = arthmateDisbursement.status_code,
                                CreatedDate = arthmateDisbursement.CreatedDate
                            };
                            arthMateLoanDataResDc.arthmateDisbursementdc = arthmateDisbursementdc;
                            gRPCReply.Status = true;
                        }
                        else
                        {
                            gRPCReply.Message = resdata.message;
                        }
                    }


                    arthMateLoanDataResDc.Loan = new LoanDc();
                    arthMateLoanDataResDc.leaddc = new Leaddc();
                    GRPCRequest<long> req = new GRPCRequest<long> { Request = Leadmasterid };
                    //var leadinfo = _leadGrpcManager.GetLeadInfoById(req);

                    var PersonalDetail = _context.PersonalDetails.Where(x => x.LeadId == Leadmasterid && x.IsActive && !x.IsDeleted).FirstOrDefault();
                    string ShopName = _context.BusinessDetails.Where(x => x.LeadId == Leadmasterid && x.IsActive && !x.IsDeleted).Select(x => x.BusinessName).FirstOrDefault();

                    var leadinfo = await _context.Leads.Where(x => x.Id == request.Request).Include(x => x.CompanyLeads).Select(x => new LeadResponse
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
                        LeadCompanies = x.CompanyLeads.Any(y => y.IsActive) ? x.CompanyLeads.Where(y => y.IsActive).Select(y => new LeadCompany
                        {
                            CompanyId = y.CompanyId,
                            LeadProcessStatus = y.LeadProcessStatus,
                            UserUniqueCode = y.UserUniqueCode

                        }).ToList() : null,
                    }).FirstOrDefaultAsync();

                    if (leadinfo != null)
                    {
                        Leaddc leaddata = new Leaddc();

                        leaddata.LeadId = leadinfo.LeadId;
                        leaddata.MobileNo = leadinfo.MobileNo;
                        leaddata.LeadCode = leadinfo.LeadCode;
                        leaddata.UserName = leadinfo.UserName;
                        leaddata.ProductId = leadinfo.ProductId;
                        leaddata.OfferCompanyId = leadinfo.OfferCompanyId;
                        //leaddata.NBFCCompanyId = CompanyDetail.AnchorCompanyConfig.CompanyId;
                        leaddata.CreditLimit = leadinfo.CreditLimit;
                        leaddata.AgreementDate = leadinfo.AgreementDate;
                        leaddata.ApplicationDate = leadinfo.ApplicationDate;
                        leaddata.LeadCompanies = leadinfo.LeadCompanies;
                        leaddata.CustomerImage = leadinfo.CustomerImage;
                        leaddata.ShopName = leadinfo.ShopName;
                        leaddata.CustomerCurrentCityName = leadinfo.CustomerCurrentCityName;
                        leaddata.CustomerName = leadinfo.CustomerName;
                        leaddata.CityName = leadinfo.CustomerCurrentCityName;
                        arthMateLoanDataResDc.leaddc = leaddata;
                    }
                    LoanDc LeadLoanData = new LoanDc();

                    #region Loan detail
                    LeadLoanData.LeadMasterId = leadloan.LeadMasterId;
                    LeadLoanData.ReponseId = 1;
                    LeadLoanData.RequestId = 1;
                    LeadLoanData.IsSuccess = true;
                    LeadLoanData.Message = leadloan.Message ?? "";
                    LeadLoanData.IsActive = true;
                    LeadLoanData.IsDeleted = false;
                    LeadLoanData.loan_app_id = leadloan.loan_app_id ?? "";
                    LeadLoanData.loan_id = leadloan.loan_id ?? "";
                    LeadLoanData.borrower_id = leadloan.borrower_id ?? "";
                    LeadLoanData.partner_loan_app_id = leadloan.partner_loan_app_id ?? "";
                    LeadLoanData.partner_loan_id = leadloan.partner_loan_id ?? "";
                    LeadLoanData.partner_borrower_id = leadloan.partner_borrower_id ?? "";
                    LeadLoanData.company_id = leadloan.company_id ?? 0;
                    LeadLoanData.product_id = leadloan.product_id.ToString() ?? "";
                    LeadLoanData.loan_app_date = leadloan.loan_app_date ?? "";
                    LeadLoanData.sanction_amount = leadloan.sanction_amount ?? 0;
                    LeadLoanData.gst_on_pf_amt = Convert.ToDouble(leadloan.gst_on_pf_amt);
                    LeadLoanData.gst_on_pf_perc = leadloan.gst_on_pf_perc ?? "";
                    LeadLoanData.net_disbur_amt = Convert.ToDouble(leadloan.net_disbur_amt);
                    LeadLoanData.status = leadloan.status ?? "";
                    LeadLoanData.stage = leadloan.stage ?? 0;
                    LeadLoanData.exclude_interest_till_grace_period = leadloan.exclude_interest_till_grace_period ?? "";
                    LeadLoanData.borro_bank_account_type = leadloan.borro_bank_account_type ?? "";
                    LeadLoanData.borro_bank_account_holder_name = leadloan.borro_bank_account_holder_name ?? "";
                    LeadLoanData.loan_int_rate = "24";//leadloan.loan_int_rate?? "";
                    LeadLoanData.processing_fees_amt = Convert.ToDouble(leadloan.processing_fees_amt);
                    LeadLoanData.processing_fees_perc = leadloan.processing_fees_perc ?? 0;
                    LeadLoanData.tenure = leadloan.tenure ?? "";
                    LeadLoanData.tenure_type = leadloan.tenure_type ?? "";
                    LeadLoanData.int_type = leadloan.int_type ?? "";
                    LeadLoanData.borro_bank_ifsc = leadloan.borro_bank_ifsc ?? "";
                    LeadLoanData.borro_bank_acc_num = leadloan.borro_bank_acc_num ?? "";
                    LeadLoanData.borro_bank_name = leadloan.borro_bank_name ?? "";
                    LeadLoanData.first_name = leadloan.first_name ?? "";
                    LeadLoanData.last_name = leadloan.last_name ?? "";
                    LeadLoanData.current_overdue_value = leadloan.current_overdue_value;
                    LeadLoanData.bureau_score = leadloan.bureau_score ?? "";
                    LeadLoanData.loan_amount_requested = Convert.ToDouble(leadloan.loan_amount_requested);
                    LeadLoanData.bene_bank_name = leadloan.bene_bank_name ?? "";
                    LeadLoanData.bene_bank_acc_num = leadloan.bene_bank_acc_num ?? "";
                    LeadLoanData.bene_bank_ifsc = leadloan.bene_bank_ifsc ?? "";
                    LeadLoanData.bene_bank_account_holder_name = leadloan.bene_bank_account_holder_name ?? "";
                    LeadLoanData.created_at = leadloan.created_at;
                    LeadLoanData.updated_at = leadloan.updated_at;
                    LeadLoanData.v = leadloan.v ?? 0;
                    LeadLoanData.co_lender_assignment_id = leadloan.co_lender_assignment_id ?? 0;
                    LeadLoanData.co_lender_id = leadloan.co_lender_id ?? 0;
                    LeadLoanData.co_lend_flag = leadloan.co_lend_flag ?? "";
                    LeadLoanData.itr_ack_no = leadloan.itr_ack_no ?? "";
                    LeadLoanData.penal_interest = leadloan.penal_interest ?? 0;
                    LeadLoanData.bounce_charges = leadloan.bounce_charges ?? 0;
                    LeadLoanData.repayment_type = leadloan.repayment_type ?? "";
                    LeadLoanData.first_inst_date = leadloan.first_inst_date;
                    LeadLoanData.final_approve_date = leadloan.final_approve_date;
                    LeadLoanData.final_remarks = leadloan.final_remarks ?? "";
                    LeadLoanData.foir = leadloan.foir ?? "";
                    LeadLoanData.upfront_interest = leadloan.upfront_interest ?? "";
                    LeadLoanData.business_vintage_overall = leadloan.business_vintage_overall ?? "";
                    LeadLoanData.loan_int_amt = Convert.ToDouble(leadloan.loan_int_amt);
                    LeadLoanData.conv_fees = leadloan.conv_fees ?? 0;
                    LeadLoanData.ninety_plus_dpd_in_last_24_months = leadloan.ninety_plus_dpd_in_last_24_months ?? "";
                    LeadLoanData.dpd_in_last_9_months = leadloan.dpd_in_last_9_months ?? "";
                    LeadLoanData.dpd_in_last_3_months = leadloan.dpd_in_last_3_months ?? "";
                    LeadLoanData.dpd_in_last_6_months = leadloan.dpd_in_last_6_months ?? "";
                    LeadLoanData.insurance_company = leadloan.insurance_company ?? "";
                    LeadLoanData.credit_card_settlement_amount = leadloan.credit_card_settlement_amount ?? 0;
                    LeadLoanData.emi_amount = leadloan.emi_amount ?? 0;
                    LeadLoanData.emi_allowed = leadloan.emi_allowed ?? "";
                    LeadLoanData.igst_amount = leadloan.igst_amount ?? 0;
                    LeadLoanData.cgst_amount = leadloan.cgst_amount ?? 0;
                    LeadLoanData.sgst_amount = leadloan.sgst_amount ?? 0;
                    LeadLoanData.emi_count = leadloan.emi_count ?? 0;
                    LeadLoanData.broken_interest = leadloan.broken_interest ?? 0;
                    LeadLoanData.dpd_in_last_12_months = leadloan.dpd_in_last_12_months ?? 0;
                    LeadLoanData.dpd_in_last_3_months_credit_card = leadloan.dpd_in_last_3_months_credit_card ?? 0;
                    LeadLoanData.dpd_in_last_3_months_unsecured = leadloan.dpd_in_last_3_months_unsecured ?? 0;
                    LeadLoanData.broken_period_int_amt = leadloan.broken_period_int_amt ?? 0;
                    LeadLoanData.dpd_in_last_24_months = leadloan.dpd_in_last_24_months ?? 0;
                    LeadLoanData.avg_banking_turnover_6_months = 0;// leadloan.avg_banking_turnover_6_months?? "";
                    LeadLoanData.enquiries_bureau_30_days = leadloan.enquiries_bureau_30_days ?? 0;
                    LeadLoanData.cnt_active_unsecured_loans = leadloan.cnt_active_unsecured_loans ?? 0;
                    LeadLoanData.total_overdues_in_cc = leadloan.total_overdues_in_cc ?? 0;
                    LeadLoanData.insurance_amount = leadloan.insurance_amount ?? 0;
                    LeadLoanData.bureau_outstanding_loan_amt = leadloan.bureau_outstanding_loan_amt ?? 0;
                    LeadLoanData.purpose_of_loan = leadloan.purpose_of_loan ?? "";
                    LeadLoanData.business_name = leadloan.business_name ?? "";
                    LeadLoanData.co_app_or_guar_name = leadloan.co_app_or_guar_name ?? "";
                    LeadLoanData.co_app_or_guar_address = leadloan.co_app_or_guar_address ?? "";
                    LeadLoanData.co_app_or_guar_mobile_no = leadloan.co_app_or_guar_mobile_no ?? "";
                    LeadLoanData.co_app_or_guar_pan = leadloan.co_app_or_guar_pan ?? "";
                    LeadLoanData.business_address_ownership = leadloan.business_address_ownership ?? "";
                    LeadLoanData.business_pan = leadloan.business_pan ?? "";
                    LeadLoanData.bureau_fetch_date = leadloan.bureau_fetch_date;// leadloan.bureau_fetch_date?? "";
                    LeadLoanData.enquiries_in_last_3_months = leadloan.enquiries_in_last_3_months ?? 0;
                    LeadLoanData.gst_on_conv_fees = leadloan.gst_on_conv_fees ?? 0;
                    LeadLoanData.cgst_on_conv_fees = leadloan.cgst_on_conv_fees ?? 0;
                    LeadLoanData.sgst_on_conv_fees = leadloan.sgst_on_conv_fees ?? 0;
                    LeadLoanData.igst_on_conv_fees = leadloan.igst_on_conv_fees ?? 0;
                    LeadLoanData.interest_type = leadloan.interest_type ?? "";
                    LeadLoanData.conv_fees_excluding_gst = leadloan.conv_fees_excluding_gst ?? 0;
                    LeadLoanData.a_score_request_id = leadloan.a_score_request_id ?? "";
                    LeadLoanData.a_score = leadloan.a_score ?? 0;
                    LeadLoanData.b_score = leadloan.b_score ?? 0;
                    LeadLoanData.offered_amount = leadloan.offered_amount ?? 0;
                    LeadLoanData.offered_int_rate = leadloan.offered_int_rate;
                    LeadLoanData.monthly_average_balance = leadloan.monthly_average_balance;
                    LeadLoanData.monthly_imputed_income = leadloan.monthly_imputed_income;
                    LeadLoanData.party_type = leadloan.party_type ?? "";
                    LeadLoanData.co_app_or_guar_dob = leadloan.co_app_or_guar_dob; //leadloan.co_app_or_guar_dob?? "";
                    LeadLoanData.co_app_or_guar_gender = leadloan.co_app_or_guar_gender ?? "";
                    LeadLoanData.co_app_or_guar_ntc = leadloan.co_app_or_guar_ntc ?? "";
                    LeadLoanData.udyam_reg_no = leadloan.udyam_reg_no ?? "";
                    LeadLoanData.program_type = leadloan.program_type ?? "";
                    LeadLoanData.written_off_settled = leadloan.written_off_settled ?? 0;
                    LeadLoanData.upi_handle = leadloan.upi_handle ?? "";
                    LeadLoanData.upi_reference = leadloan.upi_reference ?? "";
                    LeadLoanData.fc_offer_days = leadloan.fc_offer_days ?? 0;
                    LeadLoanData.foreclosure_charge = leadloan.foreclosure_charge ?? "";
                    LeadLoanData.eligible_loan_amount = 0;
                    LeadLoanData.UrlSlaDocument = "";
                    LeadLoanData.UrlSlaUploadSignedDocument = "";
                    LeadLoanData.IsUpload = false;
                    LeadLoanData.UrlSlaUploadDocument_id = "";
                    LeadLoanData.UMRN = "";
                    LeadLoanData.abb = "";
                    LeadLoanData.application_fees_excluding_gst = "";
                    LeadLoanData.bounces_in_three_month = "";
                    LeadLoanData.business_address = "";
                    LeadLoanData.business_city = "";
                    LeadLoanData.business_pin_code = "";
                    LeadLoanData.business_state = "";
                    LeadLoanData.cgst_on_application_fees = "";
                    LeadLoanData.cgst_on_subvention_fees = "";
                    LeadLoanData.co_app_or_guar_bureau_score = "";
                    LeadLoanData.customer_type_ntc = "";
                    LeadLoanData.emi_obligation = "";
                    LeadLoanData.gst_number = "";
                    LeadLoanData.gst_on_application_fees = "";
                    LeadLoanData.gst_on_subvention_fees = "";
                    LeadLoanData.igst_on_application_fees = "";
                    LeadLoanData.igst_on_subvention_fees = "";
                    LeadLoanData.monthly_income = "";
                    LeadLoanData.sgst_on_application_fees = "";
                    LeadLoanData.sgst_on_subvention_fees = "";
                    LeadLoanData.subvention_fees_amount = 0;
                    LeadLoanData.PlatFormFee = leadloan.PlatFormFee;

                    arthMateLoanDataResDc.Loan = LeadLoanData;
                    #endregion

                    gRPCReply.Response = arthMateLoanDataResDc;
                    gRPCReply.Status = true;
                    gRPCReply.Message = "";
                    _leadNBFCSubActivityManager.UpdateLeadProgressStatus(leadloan.LeadMasterId ?? 0, ActivityConstants.Congratulations);
                    _context.SaveChanges();
                }
                else
                {
                    gRPCReply.Message = "Loan Not Generated";
                    gRPCReply.Status = false;
                }
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<string>> eSignDocumentsAsync(GRPCRequest<eSignDocumentStatusDc> request)
        {
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            string DocumentId = string.Empty;
            long LeadId = request.Request.LeadId;

            eSignWebhookResponse webhook = new eSignWebhookResponse();
            if (request.Request.RequestJson != null)
            {
                webhook.Response = request.Request.RequestJson;
                webhook.IsActive = true;
                webhook.CreatedBy = "";
                webhook.IsDeleted = false;
                webhook.Created = currentDateTime;

                _context.eSignWebhookResponses.Add(webhook);
                await _context.SaveChangesAsync();

                var req = JsonConvert.DeserializeObject<eSignWebhookResponseDc>(request.Request.RequestJson);
                if (req != null)
                {
                    DocumentId = req.documentId;
                }
            }
            GRPCReply<string> res = new GRPCReply<string>() { Response = "", Message = "Not Found", Status = false };

            LeadAgreement leadAgreement = new LeadAgreement() { DocUnSignUrl = "", LeadId = 0 };

            if (LeadId == 0 && DocumentId != null)
            {
                leadAgreement = await _context.leadAgreements.Where(x => x.DocumentId == DocumentId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            }
            else
            {
                leadAgreement = await _context.leadAgreements.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            }
            if (leadAgreement != null)
            {
                LeadId = leadAgreement.LeadId;
                if (webhook != null && webhook.Response != null)
                {
                    webhook.LeadId = LeadId;
                    _context.Entry(webhook).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                var lead = _context.Leads.FirstOrDefault(x => x.Id == LeadId && x.IsActive && !x.IsDeleted);

                //var leadAgreement = await _context.leadAgreements.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                if (lead != null && leadAgreement != null)
                {
                    var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
                    if (nbfcService != null)
                    {
                        var eSignDocument = await nbfcService.eSignDocumentsAsync(LeadId, leadAgreement.DocumentId);
                        if (eSignDocument.Status)
                        {
                            var eSignResponse = JsonConvert.DeserializeObject<eSignDocumentResponseDc>(eSignDocument.Data.ToString());
                            if (eSignResponse != null)
                            {
                                DateTime signDate;
                                if (DateTime.TryParse(eSignResponse.result.verificationDetails[0].invitationStatus.signDate, out signDate))
                                {
                                    leadAgreement.StartedOn = signDate;
                                }
                                leadAgreement.ExpiredOn = signDate.AddYears(1);//As Discussed
                                leadAgreement.Status = eSignResponse.result.verificationDetails[0].invitationStatus.signed ? "Signed" : "UnSigned";

                                _context.Entry(leadAgreement).State = EntityState.Modified;
                                _context.SaveChanges();

                                ///Activity Complete signed wali
                                if (!eSignResponse.result.verificationDetails[0].invitationStatus.rejected && eSignResponse.result.verificationDetails[0].invitationStatus.signed)
                                {
                                    if (lead.Status != "Activated")
                                    {
                                        lead.Status = "AgreementSigned";
                                    }
                                    lead.IsAgreementAccept = true;
                                    lead.AgreementDate = currentDateTime;

                                    _context.Entry(lead).State = EntityState.Modified;
                                    _context.SaveChanges();
                                    _leadNBFCSubActivityManager.UpdateLeadProgressStatus(LeadId, ActivityConstants.ArthmateAgreement);
                                    res.Response = eSignResponse.result.file;
                                    res.Status = true;

                                    //------------------S : Make log---------------------
                                    #region Make History
                                    var resultHistory = await _leadHistoryManager.GetLeadHistroy(LeadId, "eSignDocumentsAsync_EsignArthmate");
                                    LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                                    {
                                        LeadId = LeadId,
                                        UserID = resultHistory.UserId,
                                        UserName = "",
                                        EventName = "Agreement Esign-Arthmate",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                                        Narretion = resultHistory.Narretion,
                                        NarretionHTML = resultHistory.NarretionHTML,
                                        CreatedTimeStamp = resultHistory.CreatedTimeStamp
                                    };
                                    await _massTransitService.Publish(histroyEvent);
                                    #endregion
                                    //------------------E : Make log---------------------

                                }
                                else if (eSignResponse.result.verificationDetails[0].invitationStatus.rejected)
                                {
                                    var eSignDocids = _context.eSignResponseDocumentIds.FirstOrDefault(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted);
                                    if (eSignDocids != null)
                                    {
                                        //eSignDocids.IsActive = false;
                                        //eSignDocids.IsDeleted = true;
                                        eSignDocids.eSignRemark = eSignResponse.result.verificationDetails[0].invitationStatus.rejectionReason;
                                        _context.Entry(eSignDocids).State = EntityState.Modified;
                                    }
                                    leadAgreement.IsActive = false;
                                    leadAgreement.IsDeleted = true;
                                    _context.Entry(leadAgreement).State = EntityState.Modified;
                                    _context.SaveChanges();

                                    res.Response = "Rejected";
                                    res.Status = false;
                                    res.Message = "Rejected";
                                }
                                else
                                {
                                    res.Response = "";
                                    res.Status = false;
                                    res.Message = "UnSigned";
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public async Task<GRPCReply<string>> updateLeadAgreement(GRPCRequest<DSALeadAgreementDc> request)
        {
            GRPCReply<string> res = new GRPCReply<string>();

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            var lead = _context.Leads.FirstOrDefault(x => x.Id == request.Request.LeadId && x.IsActive && !x.IsDeleted);

            var leadAgreement = await _context.leadAgreements.Where(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (lead != null && leadAgreement != null)
            {
                leadAgreement.DocSignedUrl = request.Request.SignedFile;
                _context.Entry(leadAgreement).State = EntityState.Modified;

                lead.IsAgreementAccept = true;
                lead.AgreementDate = currentDateTime;
                lead.Status = LeadStatusEnum.AgreementSigned.ToString();
                _context.Entry(lead).State = EntityState.Modified;
                _context.SaveChanges();
                _leadNBFCSubActivityManager.UpdateLeadProgressStatus(request.Request.LeadId, ActivityConstants.DSAAgreement);

                res.Message = "success";
                res.Status = true;
            }
            return res;
        }
        public async Task<GRPCReply<bool>> eSignCallBack(GRPCRequest<eSignWebhookResponseDc> request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
            ArthMateNBFCHelper _ArthMateNBFCHelper = new ArthMateNBFCHelper();

            var eSignWebhookRes = JsonConvert.SerializeObject(request);
            ArthMateWebhookResponse arthMateWebhookResponse = new ArthMateWebhookResponse
            {
                Response = eSignWebhookRes,
                IsActive = true,
                CreatedBy = "",
                IsDeleted = false,
                Created = currentDateTime,
            };
            await _context.ArthMateWebhookResponse.AddAsync(arthMateWebhookResponse);
            _context.SaveChanges();

            if (request.Request != null && request.Request.files.Count > 0 && request.Request.fileDc.filePath != null)
            {
                var leadAgreements = _context.leadAgreements.FirstOrDefault(x => x.DocumentId == request.Request.documentId && x.IsActive && !x.IsDeleted);
                if (leadAgreements != null)
                {
                    //GRPCRequest<long> leadid = new GRPCRequest<long> { Request = leadAgreements.LeadId };
                    //var res = await eSignDocumentsAsync(leadid);

                    //gRPCReply.Status = res.Status;
                    //gRPCReply.Message = res.Message;

                    var lead = _context.Leads.FirstOrDefault(x => x.Id == leadAgreements.LeadId && x.IsActive && !x.IsDeleted);

                    arthMateWebhookResponse.LeadId = leadAgreements.LeadId;
                    _context.Entry(arthMateWebhookResponse).State = EntityState.Modified;

                    var file = request.Request.files.FirstOrDefault();
                    _context.eSignDocumentResponse.Add(new eSignDocumentResponse
                    {
                        LeadId = leadAgreements.LeadId,
                        documentId = request.Request.documentId,
                        File = file != null ? file : "",
                        auditTrail = request.Request.auditTrail != null ? JsonConvert.SerializeObject(request.Request.auditTrail) : "",
                        users = request.Request.users != null ? JsonConvert.SerializeObject(request.Request.users) : "",
                        messages = request.Request.messages != null ? JsonConvert.SerializeObject(request.Request.messages) : "",
                        clientData = request.Request.clientData != null ? JsonConvert.SerializeObject(request.Request.clientData) : "",
                        irn = request.Request.irn,
                        Created = currentDateTime,
                        IsActive = true,
                        IsDeleted = false,
                    });
                    if (request.Request.users[0].request.signed)
                    {
                        leadAgreements.DocSignedUrl = request.Request.fileDc.filePath;
                        leadAgreements.Status = request.Request.users[0].request.signed ? "Signed" : "UnSigned";
                        _context.Entry(leadAgreements).State = EntityState.Modified;

                        lead.IsAgreementAccept = true;
                        lead.AgreementDate = currentDateTime;
                        lead.Status = "AgreementSigned";
                        _context.Entry(lead).State = EntityState.Modified;
                        _context.SaveChanges();
                        _leadNBFCSubActivityManager.UpdateLeadProgressStatus(leadAgreements.LeadId, ActivityConstants.ArthmateAgreement);
                    }

                    _context.SaveChanges();
                    gRPCReply.Status = true;
                    gRPCReply.Response = true;
                    gRPCReply.Message = "eSign Complete";
                }
            }

            return gRPCReply;
        }

        public async Task<bool> addeSignResponseDocumentId(long leadid, string documentid, int unixTimeStamp)
        {
            bool result = false;
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var startUtc2 = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();

            var eSignRequests = await _context.eSignResponseDocumentIds.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (eSignRequests != null)
            {
                eSignRequests.IsActive = false;
                eSignRequests.IsDeleted = true;
                eSignRequests.LastModified = currentDateTime;
                _context.Entry(eSignRequests).State = EntityState.Modified;

                _context.eSignResponseDocumentIds.Add(new eSignResponseDocumentId
                {
                    LeadId = leadid,
                    DocumentId = documentid,
                    ExpiryTime = startUtc2,
                    IsActive = true,
                    IsDeleted = false,
                    Created = currentDateTime,
                });
            }
            else
            {
                _context.eSignResponseDocumentIds.Add(new eSignResponseDocumentId
                {
                    LeadId = leadid,
                    DocumentId = documentid,
                    ExpiryTime = startUtc2,
                    IsActive = true,
                    IsDeleted = false,
                    Created = currentDateTime,
                });
            }
            if (_context.SaveChanges() > 0)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public async Task<GRPCReply<string>> AadhaarOtpVerify(GRPCRequest<SecondAadharXMLDc> request)
        {
            GRPCReply<string> response = new GRPCReply<string>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.AcceptOfferWithXMLAadharOTP(request);
                var leadInfo = _context.Leads.FirstOrDefault(x => x.Id == request.Request.LeadMasterId && x.IsActive && !x.IsDeleted);
                if (leadInfo != null)
                {
                    await AddLeadConsentLog(leadInfo.Id, LeadTypeConsentConstants.ArthmateOffer, leadInfo.UserName);
                }
            }
            return response;
        }
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
                return true;
            }
            return false;
        }

        public async Task<GRPCReply<LeadDetailForDisbursement>> GetLeadDetailForDisbursement(NBFCDisbursementPostdc obj)
        {
            GRPCReply<LeadDetailForDisbursement> gRPCReply = new GRPCReply<LeadDetailForDisbursement>();
            LeadDetailForDisbursement Disbursement = new LeadDetailForDisbursement();
            var acceptoffer = _context.BusinessLoanNBFCUpdate.FirstOrDefault(x => x.LeadId == obj.LeadId && x.NBFCCompanyId == obj.NBFCCompanyId && x.IsActive && !x.IsDeleted);
            if (acceptoffer != null)
            {
                var PersonalDetail = _context.PersonalDetails.Where(x => x.LeadId == obj.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefault();
                string ShopName = _context.BusinessDetails.Where(x => x.LeadId == obj.LeadId && x.IsActive && !x.IsDeleted).Select(x => x.BusinessName).FirstOrDefault() ?? "";
                var leadinfo = await _context.Leads.Where(x => x.Id == obj.LeadId).Include(x => x.CompanyLeads).Select(x => new LeadResponse
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
                    CreatedBy = x.CreatedBy,
                    LeadCompanies = x.CompanyLeads.Any(y => y.IsActive) ? x.CompanyLeads.Where(y => y.IsActive).Select(y => new LeadCompany
                    {
                        CompanyId = y.CompanyId,
                        LeadProcessStatus = y.LeadProcessStatus,
                        UserUniqueCode = y.UserUniqueCode

                    }).ToList() : null,

                }).FirstOrDefaultAsync();
                if (leadinfo != null)
                {
                    var leadOffers = await _context.LeadOffers.FirstOrDefaultAsync(x => x.LeadId == obj.LeadId && x.NBFCCompanyId == leadinfo.OfferCompanyId && x.IsActive && !x.IsDeleted);
                    if (leadOffers != null)
                    {
                        Leaddc leaddata = new Leaddc();

                        leaddata.LeadId = leadinfo.LeadId;
                        leaddata.MobileNo = leadinfo.MobileNo;
                        leaddata.LeadCode = leadinfo.LeadCode;
                        leaddata.UserName = leadinfo.UserName;
                        leaddata.ProductId = leadinfo.ProductId;
                        leaddata.OfferCompanyId = leadinfo.OfferCompanyId;
                        //leaddata.NBFCCompanyId = CompanyDetail.AnchorCompanyConfig.CompanyId;
                        leaddata.CreditLimit = leadinfo.CreditLimit;
                        leaddata.AgreementDate = leadinfo.AgreementDate;
                        leaddata.ApplicationDate = leadinfo.ApplicationDate;
                        leaddata.LeadCompanies = leadinfo.LeadCompanies;
                        leaddata.CustomerImage = leadinfo.CustomerImage;
                        leaddata.ShopName = leadinfo.ShopName;
                        leaddata.CustomerCurrentCityName = leadinfo.CustomerCurrentCityName;
                        leaddata.CustomerName = leadinfo.CustomerName;
                        leaddata.CityName = leadinfo.CustomerCurrentCityName;
                        leaddata.ProductType = leadinfo.ProductCode;
                        leaddata.NBFCCompanyCode = leadOffers.CompanyIdentificationCode;
                        leaddata.AnchorCompanyId = leadinfo.LeadCompanies.FirstOrDefault(x => x.LeadProcessStatus == 2).CompanyId;
                        leaddata.CreatedBy = leadinfo.CreatedBy;
                        Disbursement.leadinfo = leaddata;


                        var BusinessLoanNBFCUpdate = await _context.BusinessLoanNBFCUpdate.FirstOrDefaultAsync(x => x.LeadId == obj.LeadId && x.NBFCCompanyId == leadOffers.NBFCCompanyId && x.IsActive && !x.IsDeleted);
                        if (BusinessLoanNBFCUpdate != null)
                        {
                            Disbursement.DisbursementDetail = new DisbursementDetail
                            {
                                InterestRate = BusinessLoanNBFCUpdate.InterestRate,
                                LoanAmount = BusinessLoanNBFCUpdate.LoanAmount,
                                LoanInterestAmount = BusinessLoanNBFCUpdate.LoanInterestAmount,
                                MonthlyEMI = BusinessLoanNBFCUpdate.MonthlyEMI,
                                PFDiscount = BusinessLoanNBFCUpdate.PFDiscount,
                                ProcessingFeeAmount = BusinessLoanNBFCUpdate.ProcessingFeeAmount,
                                ProcessingFeeRate = BusinessLoanNBFCUpdate.ProcessingFeeRate,
                                Tenure = BusinessLoanNBFCUpdate.Tenure,
                                LoanId = BusinessLoanNBFCUpdate.LoanId,
                                ProcessingFeeTax = BusinessLoanNBFCUpdate.ProcessingFeeTax,
                                GST = BusinessLoanNBFCUpdate.GST,
                                CompanyIdentificationCode = BusinessLoanNBFCUpdate.CompanyIdentificationCode,
                                Commission = BusinessLoanNBFCUpdate.Commission,
                                CommissionType = BusinessLoanNBFCUpdate.CommissionType,
                                PFType = BusinessLoanNBFCUpdate.PFType,
                                NBFCCompanyId = BusinessLoanNBFCUpdate.NBFCCompanyId,

                                NBFCInterest = BusinessLoanNBFCUpdate.NBFCInterest,
                                NBFCProcessingFee = BusinessLoanNBFCUpdate.NBFCProcessingFee,
                                NBFCProcessingFeeType = BusinessLoanNBFCUpdate.NBFCProcessingFeeType,
                                ArrangementType = BusinessLoanNBFCUpdate.ArrangementType,
                                Bounce = BusinessLoanNBFCUpdate.Bounce!=null ? BusinessLoanNBFCUpdate.Bounce :0,
                                NBFCBounce = BusinessLoanNBFCUpdate.NBFCBounce != null ? BusinessLoanNBFCUpdate.NBFCBounce : 0, 
                                NBFCPenal = BusinessLoanNBFCUpdate.NBFCPenal != null ? BusinessLoanNBFCUpdate.NBFCPenal : 0,
                                Penal = BusinessLoanNBFCUpdate.Penal != null ? BusinessLoanNBFCUpdate.Penal : 0,
                            };
                        };

                        double FinalAmount = leadinfo.CreditLimit ?? 0;
                        DateTime firstEmiDate = obj.FirstEMIDate;

                        //if (obj.FirstEMIDate.Day >= 1 && obj.FirstEMIDate.Day <= 20)
                        //{ firstEmiDate = new DateTime(obj.FirstEMIDate.AddMonths(1).Year, obj.FirstEMIDate.AddMonths(1).Month, obj.FirstEMIDate.Day); }
                        //else
                        //{ firstEmiDate = new DateTime(obj.FirstEMIDate.AddMonths(2).Year, obj.FirstEMIDate.AddMonths(2).Month, obj.FirstEMIDate.Day); }
                        var emiAmount = pmt(BusinessLoanNBFCUpdate.InterestRate ?? 0, BusinessLoanNBFCUpdate.Tenure ?? 0, FinalAmount);

                        var Leadid = new SqlParameter("@leadId", obj.LeadId);
                        var SanctionAmount = new SqlParameter("@SanctionAmount", FinalAmount);
                        var EmiAmount = new SqlParameter("@EmiAmount", emiAmount);
                        var RPIValue = new SqlParameter("@RPIValue", BusinessLoanNBFCUpdate.InterestRate ?? 0);
                        var Tenure = new SqlParameter("@Tenure", BusinessLoanNBFCUpdate.Tenure);
                        var FirstEmiDate = new SqlParameter("@FirstEmiDate", firstEmiDate);

                        var Result = _context.Database.SqlQueryRaw<OfferEmiDetailDC>("exec GetOfferEmiDetails @leadId,@SanctionAmount,@EmiAmount,@RPIValue,@Tenure,@FirstEmiDate", Leadid, SanctionAmount, EmiAmount, RPIValue, Tenure, FirstEmiDate).AsEnumerable().ToList();


                        //var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
                        //if (nbfcService != null)
                        {
                            Disbursement.EMISchedule = new List<EMISchedule>();
                            //var res = await nbfcService.GetOfferEmiDetails(obj.LeadId);
                            foreach (var item in Result)
                            {
                                EMISchedule emi = new EMISchedule
                                {
                                    DueDate = item.DueDate,
                                    EMIAmount = item.EMIAmount,
                                    InterestAmount = item.InterestAmount,
                                    OutStandingAmount = item.OutStandingAmount,
                                    Prin = item.Prin,
                                    PrincipalAmount = item.PrincipalAmount,

                                };
                                Disbursement.EMISchedule.Add(emi);
                            }
                        }

                        //var leadOfferConfig = await _context.OfferConfigurationByLead.Where(x => x.LeadId == Leadid && x.IsActive && !x.IsDeleted).ToListAsync();
                        //if (leadOfferConfig != null && leadOfferConfig.Any())
                        //{
                        //    Disbursement.productSlabConfig = new List<SlabConfigDC>();
                        //    foreach (var offer in leadOfferConfig)
                        //    {
                        //        SlabConfigDC slabConfig = new SlabConfigDC
                        //        {
                        //            CompanyId = offer.CompanyId,
                        //            IsFixed = offer.IsFixed,
                        //            MaxLoanAmount = offer.MaxLoanAmount,
                        //            MaxValue = offer.MaxValue,
                        //            MinLoanAmount = offer.MinLoanAmount,
                        //            MinValue = offer.MinValue,
                        //            ProductId = offer.ProductId,
                        //            SharePercentage = offer.SharePercentage,
                        //            SlabType = offer.SlabType,
                        //            ValueType = offer.ValueType
                        //        };
                        //        Disbursement.productSlabConfig.Add(slabConfig);
                        //    }
                        //};
                    }
                    gRPCReply.Status = true;
                    gRPCReply.Message = "success";
                }
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Offer Not Accept By Customer";
            }
            gRPCReply.Response = Disbursement;
            return gRPCReply;
        }
        private decimal pmt(double yearlyinterestrate, int totalnumberofmonths, double loanamount)
        {
            if (yearlyinterestrate > 0)
            {
                var rate = (double)yearlyinterestrate / 100 / 12;
                var denominator = Math.Pow((1 + rate), totalnumberofmonths) - 1;
                return new decimal((rate + (rate / denominator)) * loanamount);
            }
            return totalnumberofmonths > 0 ? new decimal(loanamount / totalnumberofmonths) : 0;
        }
    }
}
