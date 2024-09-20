using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadAPI.Helper.Cashfree;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Cashfree;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.Lead.DSA;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.BusinessLoan;
using ScaleUP.Services.LeadModels.Cashfree;
using ScaleUP.Services.LeadModels.DSA;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class LeadManager
    {
        private readonly LeadApplicationDbContext _context;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private IHostEnvironment _hostingEnvironment;

        private readonly LeadHistoryManager _leadHistoryManager;
        private readonly IMassTransitService _massTransitService;

        public LeadManager(LeadApplicationDbContext context, IHostEnvironment hostingEnvironment, LeadHistoryManager leadHistoryManager, IMassTransitService massTransitService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _leadHistoryManager = leadHistoryManager;
            _massTransitService = massTransitService;
        }
        public async Task<LeadCurrentActivity> GetLeadCurrentActivity(string MobileNo, long ProductId, long CompanyId, long? LeadId)
        {
            LeadCurrentActivity leadCurrentActivity = new LeadCurrentActivity();
            Leads leadData = null;
            if (LeadId.HasValue && LeadId.Value == 0)
            {
                leadData = await _context.Leads.FirstOrDefaultAsync(x => x.MobileNo == MobileNo && x.ProductId == ProductId && x.IsActive && !x.IsDeleted);
            }
            else
            {
                leadData = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId && x.IsActive && !x.IsDeleted);
            }
            LeadActivitySequenceResponse leadActivitySequenceResponse = new LeadActivitySequenceResponse();
            int? SequenceNo = 0;
            if (leadData != null)
            {
                LeadId = leadData.Id;
                leadCurrentActivity.UserId = leadData.UserName;
            }
            if (LeadId.HasValue)
            {
                SequenceNo = (await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == LeadId.Value && (!x.IsCompleted || x.IsApproved == 2)).OrderBy(x => x.Sequence).FirstOrDefaultAsync())?.Sequence;
                if (!SequenceNo.HasValue || SequenceNo.Value == 0)
                    SequenceNo = 1;                    //SequenceNo -= 1;
            }
            leadCurrentActivity.LeadId = LeadId;
            leadCurrentActivity.SequenceNo = SequenceNo ?? 0;
            return leadCurrentActivity;
        }

        public async Task<VerifyLeadDocumentReply> VerifyLeadDocument(VerifyLeadDocumentRequest request)
        {
            VerifyLeadDocumentReply verifyLeadDocumentReply = new VerifyLeadDocumentReply();
            var AllleadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.LeadId && x.IsDeleted == false).ToListAsync();
            LeadActivityMasterProgresses _lead = AllleadActivity.Where(x => x.LeadMasterId == request.LeadId && x.ActivityMasterId == request.ActivityMasterId && x.IsActive && !x.IsDeleted && x.SubActivityMasterId == request.SubActivityMasterId).FirstOrDefault();
            if (_lead != null)
            {
                //LeadActivityMasterProgresses _personalDetail = AllleadActivity.Where(x => x.LeadMasterId == request.LeadId && x.IsActive && !x.IsDeleted && x.SubActivityMasterName == SubActivityConstants.PersonalInfo).FirstOrDefault();
                //LeadActivityMasterProgresses _aadharDetail = AllleadActivity.Where(x => x.LeadMasterId == request.LeadId && x.IsActive && !x.IsDeleted && x.SubActivityMasterName == SubActivityConstants.Aadhar).FirstOrDefault();

                //2 Reject , 1 Approve
                //if (request.IsApprove == 2)
                //{
                //    if (_lead.SubActivityMasterName == SubActivityConstants.Pan)
                //    {
                //        if (_aadharDetail != null)
                //        {
                //            _aadharDetail.IsApproved = request.IsApprove;
                //            _aadharDetail.Comment = request.Comment;
                //            _context.Entry(_aadharDetail).State = EntityState.Modified;
                //        }

                //        if (_personalDetail != null)
                //        {

                //            _personalDetail.IsApproved = request.IsApprove;
                //            _personalDetail.Comment = request.Comment;

                //            _context.Entry(_personalDetail).State = EntityState.Modified;

                //        }
                //    }
                //    else if (_lead.SubActivityMasterName == SubActivityConstants.Aadhar)
                //    {
                //        if (_personalDetail != null)
                //        {

                //            _personalDetail.IsApproved = request.IsApprove;
                //            _personalDetail.Comment = request.Comment;
                //            _context.Entry(_personalDetail).State = EntityState.Modified;


                //        }
                //    }
                //}

                _lead.IsApproved = request.IsApprove;
                _lead.Comment = request.Comment;

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
                    verifyLeadDocumentReply.Message = "Document " + (request.IsApprove == 1 ? "Approved " : "Rejected ") + "Successfully.";
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
                verifyLeadDocumentReply.Message = "issue During Update.";
            }
            return verifyLeadDocumentReply;
        }

        public async Task<VerifyLeadDocumentReply> GetVerifiedLeadDocumentStatus(VerifyLeadDocumentRequest request)
        {
            VerifyLeadDocumentReply verifyLeadDocumentReply = new VerifyLeadDocumentReply();
            LeadActivityMasterProgresses _lead = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.LeadId && x.ActivityMasterId == request.ActivityMasterId && x.IsActive && !x.IsDeleted && x.SubActivityMasterId == request.SubActivityMasterId).FirstOrDefaultAsync();
            if (_lead != null)
            {
                if (_lead.IsApproved == 1)
                {
                    verifyLeadDocumentReply.Status = true;
                    verifyLeadDocumentReply.Message = "Document Approved Successfully";
                }
                else if (_lead.IsApproved == 2)
                {
                    verifyLeadDocumentReply.Status = false;
                    verifyLeadDocumentReply.Message = "Document Rejected Successfully";
                }
                else
                {
                    verifyLeadDocumentReply.Status = false;
                    verifyLeadDocumentReply.Message = "Document Status not Updated";
                }
            }
            return verifyLeadDocumentReply;
        }

        public async Task<LeadOfferDTO> GetLeadOffer(long LeadId)
        {
            LeadOfferDTO leadOfferDTO = new LeadOfferDTO();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId);
            leadOfferDTO.CreditLimit = lead.CreditLimit != null ? lead.CreditLimit.Value : 0;
            return leadOfferDTO;
        }
        public async Task<LeadOfferAcceptedDTO> GetOfferAccepted(long LeadId)
        {
            LeadOfferAcceptedDTO leadOfferAcceptedDTO = new LeadOfferAcceptedDTO();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId);
            leadOfferAcceptedDTO.IsOfferAccepted = lead.OfferCompanyId != null && lead.OfferCompanyId > 0 ? true : false;
            return leadOfferAcceptedDTO;
        }

        public async Task<long> LeadExistsForCustomer(string companyCode, string productCode, string Mobile, string customerReferenceNo)
        {
            var lead = await _context.Leads.Where(x => x.MobileNo == Mobile && x.ProductCode == productCode && x.IsActive && !x.IsDeleted && x.CompanyLeads.Any(x => x.CompanyCode == companyCode && !x.IsDeleted && x.IsActive)).Include(x => x.CompanyLeads).FirstOrDefaultAsync();
            var leadid = lead != null ? lead.Id : 0;
            return leadid;
        }

        public async Task<bool> DisbursementReject(DisbursementRejectDTO disbursementRejectDTO)
        {
            bool result = false;
            var AllleadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == disbursementRejectDTO.LeadId && x.IsDeleted == false).ToListAsync();
            var RejectleadActivity = AllleadActivity.FirstOrDefault(x => x.ActivityMasterName == ActivityEnum.Rejected.ToString());
            var DisburseleadActivity = AllleadActivity.FirstOrDefault(x => x.ActivityMasterName == ActivityEnum.Disbursement.ToString());
            if (DisburseleadActivity != null && RejectleadActivity != null)
            {
                DisburseleadActivity.IsCompleted = true;
                _context.Entry(DisburseleadActivity).State = EntityState.Modified;
                var nextSeq = DisburseleadActivity.Sequence + 2;
                foreach (var item in AllleadActivity.Where(x => x.Sequence > DisburseleadActivity.Sequence))
                {
                    item.Sequence = nextSeq;
                    _context.Entry(item).State = EntityState.Modified;
                    nextSeq++;
                }
                RejectleadActivity.IsActive = true;
                RejectleadActivity.Sequence = DisburseleadActivity.Sequence + 1;
                RejectleadActivity.RejectMessage = disbursementRejectDTO.RejectReason;
                _context.Entry(RejectleadActivity).State = EntityState.Modified;
                result = await _context.SaveChangesAsync() > 0;
            }
            return result;
        }

        public async Task<bool> ManageRejectActivity(ManageRejectActivityDc request)
        {
            bool result = true;
            if (request != null && request.LeadId > 0)
            {
                var Lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.LeadId);
                var AllleadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.LeadId && x.IsDeleted == false).ToListAsync();
                if (AllleadActivity != null)
                {
                    var leadActivityMasterProgresses = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId &&
                                                                              x.ActivityMasterId == request.ActivityMasterId &&
                                                                              x.SubActivityMasterId == request.SubActivityMasterId &&
                                                                              x.IsActive && !x.IsDeleted);
                    if (leadActivityMasterProgresses != null)
                    {

                        var RejectleadActivity = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId
                                                 && x.ActivityMasterName == ActivityEnum.Rejected.ToString());
                        if (RejectleadActivity != null && RejectleadActivity.IsActive && request.IsRejected)
                        {
                            //if (request.IsKYCCompleted)
                            //{
                            //    Lead.Status = LeadStatusEnum.KYCSuccess.ToString();
                            //    _context.Entry(Lead).State = EntityState.Modified;
                            //}
                            //else if (leadActivityMasterProgresses.ActivityMasterName == ActivityEnum.CreditBureau.ToString())
                            //{
                            //    Lead.Status = LeadStatusEnum.CibilSuccess.ToString();
                            //    Lead.LastModified = DateTime.Now;
                            //    _context.Entry(Lead).State = EntityState.Modified;
                            //}
                            //else if (leadActivityMasterProgresses.ActivityMasterName == ActivityEnum.Pan.ToString())
                            //{
                            //    Lead.Status = LeadStatusEnum.KYCInProcess.ToString();
                            //    Lead.LastModified = DateTime.Now;
                            //}
                            foreach (var item in AllleadActivity.Where(x => x.Sequence > leadActivityMasterProgresses.Sequence))
                            {
                                //if (item.ActivityMasterName != ActivityEnum.Inprogress.ToString())
                                //{
                                item.IsActive = true;
                                if (RejectleadActivity.Sequence - 1 == item.Sequence)
                                {
                                    item.IsApproved = 2;
                                }
                                _context.Entry(item).State = EntityState.Modified;
                                //}
                                //if (item.ActivityMasterName == ActivityEnum.Inprogress.ToString() && leadActivityMasterProgresses.ActivityMasterName == "BusinessInfo")
                                //{
                                //    item.IsActive = true;
                                //    _context.Entry(item).State = EntityState.Modified;
                                //}
                            }

                            RejectleadActivity.IsActive = false;
                            _context.Entry(RejectleadActivity).State = EntityState.Modified;
                        }
                        else if (RejectleadActivity != null && !request.IsRejected)
                        {
                            Lead.Status = LeadStatusEnum.KYCReject.ToString();
                            Lead.LastModified = DateTime.Now;
                            _context.Entry(Lead).State = EntityState.Modified;
                            foreach (var item in AllleadActivity.Where(x => x.Sequence > leadActivityMasterProgresses.Sequence))
                            {
                                item.IsActive = false;
                                _context.Entry(item).State = EntityState.Modified;
                            }
                            RejectleadActivity.IsActive = true;
                            RejectleadActivity.Sequence = leadActivityMasterProgresses.Sequence + 1;
                            RejectleadActivity.RejectMessage = request.Message;
                            _context.Entry(RejectleadActivity).State = EntityState.Modified;

                        }
                        var res = await _context.SaveChangesAsync();
                    }
                }
            }
            return result;
        }

        public async Task<Leads> GetLeadById(long id)
        {
            var lead = _context.Leads.Where(x => x.Id == id).FirstOrDefault();
            return lead;
        }

        public async Task<bool> CheckVintage(long LeadId, long? CompanyId, int VintageDays)
        {
            var companyLead = _context.CompanyLead.Where(x => x.CompanyId == CompanyId && x.LeadId == LeadId).FirstOrDefault();
            if (companyLead != null)
            {
                //if(companyLead.VintageDays > 0)
                //{
                //    return false;
                //}
                //else
                //{
                companyLead.BusinessVintageDays = VintageDays;
                _context.Entry(companyLead).State = EntityState.Modified;
                var rowChanges = await _context.SaveChangesAsync();
                if (rowChanges > 0)
                {
                    return true;
                }
                //}
            }
            return false;
        }

        //public async Task<bool> WebhookResponse(string response)
        //{
        //    bool result = true;
        //    if (response != null)
        //    {
        //        _context.WebhookResponses.Add(new WebhookResponse
        //        {
        //            response = response,
        //            Created = DateTime.Now,
        //            IsActive = true,
        //            CreatedBy = "",
        //            IsDeleted = false

        //        });
        //        await _context.SaveChangesAsync();
        //    }
        //    return result;
        //}


        public async Task<bool> UpdateLeadOffer(long LeadOfferId)
        {
            bool reply = false;
            var leadoffer = await _context.LeadOffers.FirstOrDefaultAsync(x => x.Id == LeadOfferId);
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == leadoffer.LeadId);

            if (lead != null && (lead.OfferCompanyId == null || lead.OfferCompanyId == 0) && leadoffer != null && leadoffer.Status == LeadOfferConstant.OfferGenerated)
            {

                lead.CreditLimit = leadoffer.CreditLimit;
                lead.OfferCompanyId = leadoffer.NBFCCompanyId;
                _context.Entry(lead).State = EntityState.Modified;

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
                    reply = true;
                }

            }
            return reply;
        }
        public async Task<TemplateResponseDc> SaveModifyTemplateMaster(TemplateDc templatedc)
        {
            TemplateResponseDc temp = new TemplateResponseDc();

            if (templatedc != null && templatedc.TemplateCode != null && templatedc.TemplateType != null && templatedc.TemplateID == null)
            {
                var exist = await _context.LeadTemplateMasters.Where(x => x.TemplateType == templatedc.TemplateType && x.TemplateCode == templatedc.TemplateCode && x.IsDeleted == false).FirstOrDefaultAsync();
                if (exist == null)
                {
                    var smstemp = new LeadTemplateMaster
                    {
                        DLTID = templatedc.DLTID,
                        TemplateType = templatedc.TemplateType,
                        TemplateCode = templatedc.TemplateCode,
                        Template = templatedc.Template,
                        IsActive = templatedc.Status,
                        IsDeleted = false,
                        Created = DateTime.Now
                    };
                    _context.LeadTemplateMasters.Add(smstemp);
                    await _context.SaveChangesAsync();
                    temp.Status = true;
                    temp.Message = "successfully added!";
                }
                else
                {
                    temp.Message = "Template code already exist!!";
                }
            }
            else
            {

                var data = await _context.LeadTemplateMasters.Where(x => x.Id == templatedc.TemplateID && x.IsDeleted == false).Select(x => x).FirstOrDefaultAsync();
                if (data != null)
                {
                    if (data.TemplateType == "SMS")
                    {
                        data.DLTID = templatedc.DLTID;
                    }
                    data.TemplateCode = templatedc.TemplateCode;
                    data.Template = templatedc.Template;
                    data.IsActive = templatedc.Status;
                    data.LastModified = DateTime.Now;
                    _context.Entry(data).State = EntityState.Modified;
                    int isSaved = await _context.SaveChangesAsync();
                    if (isSaved > 0)
                    {
                        temp.Status = true;
                        temp.Message = "successfully Modified!";
                    }
                    else
                    {
                        temp.Status = false;
                        temp.Message = "failed to Save";
                    }
                }
                else
                {
                    temp.Status = false;
                    temp.Message = "data not found!";
                }
            }
            return temp;
        }

        public async Task<ResultViewModel<bool>> IsOfferGenerated(long LeadId)
        {
            ResultViewModel<bool> reply = new ResultViewModel<bool>();
            reply.IsSuccess = false;
            reply.Result = false;
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId);
            if (lead != null)
            {
                if (await _context.LeadOffers.AnyAsync(x => x.LeadId == lead.Id && x.IsActive && !x.IsDeleted && x.Status == LeadOfferConstant.OfferGenerated))
                {
                    reply.IsSuccess = true;
                    reply.Result = true;
                }
            }
            return reply;
        }


        public async Task<ResultViewModel<List<LeadOfferStatusDTO>>> GetAllLeadOfferStatus(long LeadId)
        {
            ResultViewModel<List<LeadOfferStatusDTO>> reply = new ResultViewModel<List<LeadOfferStatusDTO>>();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId);
            if (lead != null)
            {
                var result = await _context.LeadOffers.Where(x => x.LeadId == lead.Id && x.IsActive && !x.IsDeleted).Select(x => new LeadOfferStatusDTO
                {
                    CompanyIdentificationCode = x.CompanyIdentificationCode,
                    IsOfferGenerated = (x.Status == LeadOfferConstant.OfferGenerated || lead.OfferCompanyId > 0) ? true : false,
                    LeadOfferId = x.Id,
                    NbfcId = x.NBFCCompanyId,
                    LeadId = lead.Id,
                }).ToListAsync();
                reply.IsSuccess = true;
                reply.Result = result;
            }
            return reply;
        }

        public async Task<ResultViewModel<bool>> PostPersonalDetailByLeadId(LeadPersonalDetailDTO command)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();

            var Lead = _context.Leads.Where(x => x.Id == command.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (Lead != null)
            {
                //Lead.CityId = Convert.ToInt64(command.City);
                //Lead.ApplicantName = command.FirstName + " " + command.MiddleName + " " + command.LastName;
                Lead.ApplicantName = command.FirstName + " " + (!string.IsNullOrEmpty(command.MiddleName) ? command.MiddleName + " " : command.MiddleName) + command.LastName;
                _context.Entry(Lead).State = EntityState.Modified;
                _context.SaveChanges();

                res = new ResultViewModel<bool>
                {
                    IsSuccess = true,
                    Message = "SuccesFully",
                    Result = true
                };
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    IsSuccess = false,
                    Message = "Failed",
                    Result = false
                };
            }
            return res;
        }
        public async Task<ResultViewModel<bool>> LeadReject(long LeadId, string Message)
        {
            ResultViewModel<bool> reply = new ResultViewModel<bool>();
            reply.Message = "Something went wrong.";
            if (LeadId > 0 && !string.IsNullOrEmpty(Message))
            {
                var Lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId);
                if (Lead != null && Lead.OfferCompanyId > 0)
                {
                    reply.IsSuccess = false;
                    reply.Message = "Lead can't rejected due to offer already accepted.";
                    return reply;
                }
                var AllleadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == LeadId && x.IsDeleted == false).ToListAsync();
                if (AllleadActivity != null)
                {
                    #region For Arthmate
                    var arthmateUpdate = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted);
                    if (arthmateUpdate != null)
                    {
                        arthmateUpdate.IsOfferRejected = true;
                        _context.Entry(arthmateUpdate).State = EntityState.Modified;
                    }
                    #endregion
                    var RejectleadActivity = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == LeadId && x.ActivityMasterName == ActivityEnum.Rejected.ToString());
                    if (RejectleadActivity != null)
                    {
                        RejectleadActivity.IsActive = true;
                        RejectleadActivity.Sequence = AllleadActivity.Where(y => y.Id != RejectleadActivity.Id && y.IsActive && y.IsCompleted && !y.IsDeleted).Max(y => y.Sequence) + 1;
                        RejectleadActivity.RejectMessage = Message;
                        _context.Entry(RejectleadActivity).State = EntityState.Modified;

                        Lead.Status = ActivityEnum.Rejected.ToString();
                        _context.Entry(Lead).State = EntityState.Modified;
                    }


                    var leadActivityInactive = AllleadActivity.Where(x => x.ActivityMasterName != ActivityEnum.Rejected.ToString() && !x.IsCompleted).ToList();
                    if (leadActivityInactive != null)
                    {
                        foreach (var leadActivity in leadActivityInactive)
                        {
                            leadActivity.IsActive = false;
                            leadActivity.IsDeleted = true;
                            _context.Entry(leadActivity).State = EntityState.Modified;
                        }
                    }

                    if (await _context.SaveChangesAsync() > 0)
                    {
                        #region Make History
                        string doctypeLeadReject = "LeadReject";

                        var result = await _leadHistoryManager.GetLeadHistroy(LeadId, doctypeLeadReject);
                        LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                        {
                            LeadId = LeadId,
                            UserID = result.UserId,
                            UserName = "",
                            EventName = doctypeLeadReject, //"AddLeadConsentLog_PostLeadPAN_PostLeadAadharVerifyOTP"
                            Narretion = result.Narretion,
                            NarretionHTML = result.NarretionHTML,
                            CreatedTimeStamp = result.CreatedTimeStamp
                        };
                        await _massTransitService.Publish(histroyEvent);
                        #endregion

                        reply.IsSuccess = true;
                        reply.Result = true;
                        reply.Message = "Rejected Successfully";
                    }

                }
            }
            return reply;
        }

        public async Task<LeadListDetail> GetLeadCommonDetail(long leadId)
        {
            var lead = await _context.Leads.Where(x => x.Id == leadId).ToListAsync();
            var leadData = new LeadListDetail();
            if (lead != null)
            {
                leadData = lead.Select(y => new LeadListDetail
                {
                    OfferCompanyId = y.OfferCompanyId,
                    LeadCode = y.LeadCode,
                    CustomerName = y.ApplicantName,
                    MobileNo = y.MobileNo,
                    CreditScore = y.CreditScore,
                    LeadGenerator = y.LeadGenerator,
                    LeadConvertor = y.LeadConverter,
                    CibilReport = y.CibilReport,
                    ProductId = y.ProductId
                }).FirstOrDefault();
            }

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

        public async Task<bool> ResetLeadActivityMasterProgresse(long LeadId)
        {
            var LeadActivityMaster = _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == LeadId && x.IsActive && !x.IsDeleted).ToList();
            if (LeadActivityMaster != null)
            {
                foreach (var item in LeadActivityMaster)
                {
                    item.IsApproved = 0;
                    item.IsCompleted = false;
                    _context.Entry(item).State = EntityState.Modified;
                }
                _context.SaveChanges();
            }
            return true;
        }

        public async Task<ResultViewModel<bool>> DisbursementNext(long leadId)
        {
            ResultViewModel<bool> reply = new ResultViewModel<bool>();
            if (leadId > 0)
            {
                var lead = await _context.Leads.Where(x => x.Id == leadId).FirstOrDefaultAsync();
                if (lead != null && lead.OfferCompanyId > 0)
                {
                    var leadActivityMasterProgresses = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == lead.Id && x.IsActive && !x.IsDeleted
                    && (x.ActivityMasterName == ActivityConstants.DisbursementCompleted.ToString() || x.ActivityMasterName == ActivityConstants.MyAccount.ToString())).ToListAsync();
                    if (leadActivityMasterProgresses != null && leadActivityMasterProgresses.Any())
                    {
                        var DisbursmentProgresses = leadActivityMasterProgresses.FirstOrDefault(x => x.ActivityMasterName == ActivityConstants.DisbursementCompleted);
                        var MyAccountProgresses = leadActivityMasterProgresses.FirstOrDefault(x => x.ActivityMasterName == ActivityConstants.MyAccount);
                        if (DisbursmentProgresses != null && MyAccountProgresses != null)
                        {
                            DisbursmentProgresses.IsApproved = 1;
                            DisbursmentProgresses.IsCompleted = true;
                            _context.Entry(DisbursmentProgresses).State = EntityState.Modified;
                            reply.IsSuccess = await _context.SaveChangesAsync() > 0;
                        }
                    }
                }
            }
            return reply;
        }

        public async Task<ResultViewModel<List<CompanyBuyingHistoryDTO>>> GetCompanyBuyingHistory(long CompanyId, long LeadId)
        {
            ResultViewModel<List<CompanyBuyingHistoryDTO>> result = new ResultViewModel<List<CompanyBuyingHistoryDTO>>();
            long CompanyLeadId = 0;
            CompanyLeadId = await _context.CompanyLead.Where(x => x.CompanyId == CompanyId && x.LeadId == LeadId && x.IsActive && !x.IsDeleted).Select(x => x.Id).FirstOrDefaultAsync();
            var companyBuyingHistoryListDTO = await _context.LeadCompanyBuyingHistorys.Where(y => y.CompanyLeadId == CompanyLeadId && y.IsActive && !y.IsDeleted).Select(y => new CompanyBuyingHistoryDTO { MonthFirstBuyingDate = y.MonthFirstBuyingDate, MonthTotalAmount = y.MonthTotalAmount, TotalMonthInvoice = y.TotalMonthInvoice }).ToListAsync();
            result.Result = companyBuyingHistoryListDTO;
            if (companyBuyingHistoryListDTO.Count > 0)
            {
                result.IsSuccess = true;
                result.Message = "";
            }
            else
            {
                result.IsSuccess = false;
                result.Message = "Data Not Found!!";
            }
            return result;
        }

        public async Task<bool> AddLeadGeneratorConvertor(AddLeadGeneratorConvertorDTO addLeadGeneratorConvertorDTO)
        {
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            var leads = await _context.Leads.Where(x => x.Id == addLeadGeneratorConvertorDTO.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (leads != null)
            {
                leads.LeadGenerator = addLeadGeneratorConvertorDTO.LeadGenerator;
                leads.LeadConverter = addLeadGeneratorConvertorDTO.LeadConvertor;
                leads.LastModified = indianTime;
                leads.LastModifiedBy = addLeadGeneratorConvertorDTO.UserName;
                _context.Entry(leads).State = EntityState.Modified;
                if (_context.SaveChanges() > 0)
                {
                    #region Make History
                    string doctypeLeadLgLc = "Lead_LG LC";

                    var result = await _leadHistoryManager.GetLeadHistroy(addLeadGeneratorConvertorDTO.LeadId, doctypeLeadLgLc);
                    LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                    {
                        LeadId = addLeadGeneratorConvertorDTO.LeadId,
                        UserID = result.UserId,
                        UserName = "",
                        EventName = doctypeLeadLgLc, //"AddLeadConsentLog_PostLeadPAN_PostLeadAadharVerifyOTP"
                        Narretion = result.Narretion,
                        NarretionHTML = result.NarretionHTML,
                        CreatedTimeStamp = result.CreatedTimeStamp
                    };
                    await _massTransitService.Publish(histroyEvent);
                    #endregion

                    return true;
                }
            }
            return false;
        }

        public async Task<ResultViewModel<Leads>> LeadDataOnInProgressScreen(long leadId)
        {
            ResultViewModel<Leads> reply = new ResultViewModel<Leads>();
            if (leadId > 0)
            {
                var lead = await _context.Leads.Where(x => x.Id == leadId).FirstOrDefaultAsync();
                if (lead != null)
                {
                    reply.IsSuccess = true;
                    reply.Result = lead;
                }
                else
                {
                    reply.IsSuccess = false;
                    reply.Message = "No data found!!!";
                }
            }
            return reply;
        }

        public async Task<ResultViewModel<bool>> GetPFCollectionActivityStatus(long leadId)
        {
            ResultViewModel<bool> reply = new ResultViewModel<bool>();
            if (leadId > 0)
            {
                var currentActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadId && x.ActivityMasterName == "PFCollection").FirstOrDefaultAsync();
                if (currentActivity != null && currentActivity.IsCompleted && currentActivity.IsApproved == 1)
                {
                    reply.IsSuccess = true;
                }
                else
                {
                    reply.IsSuccess = false;
                    reply.Message = "No data found!!!";
                }
            }
            return reply;
        }


        public async Task<ResultViewModel<List<SlaLbaStampDetailsData>>> getSlaLbaStampDetailsData(int isStampUsed, int skip, int take)
        {
            ResultViewModel<List<SlaLbaStampDetailsData>> reply = new ResultViewModel<List<SlaLbaStampDetailsData>>();
            var IsStampUsed = new SqlParameter("isStampUsed", isStampUsed);
            var Skip = new SqlParameter("skip", skip);
            var Take = new SqlParameter("take", take);
            try
            {
                var stampdetails = _context.Database.SqlQueryRaw<SlaLbaStampDetailsData>("exec getSlaLbaStampDetailsData @isStampUsed, @skip, @take", IsStampUsed, Skip, Take).AsEnumerable().ToList();
                if (stampdetails != null && stampdetails.Any())
                {
                    reply.IsSuccess = true;
                    reply.Result = stampdetails;
                    reply.Message = "data found";
                }
                else
                {
                    reply.IsSuccess = false;
                    reply.Message = "data not found";
                }
            }
            catch (Exception ex)
            {
                reply.IsSuccess = false;
                reply.Message = ex.Message;
            }
            return reply;
        }

        public async Task<ResultViewModel<bool>> StampPagerDelete(long ArthmateSlaId)
        {
            ResultViewModel<bool> reply = new ResultViewModel<bool>();
            var isExistRecord = await _context.ArthmateSlaLbaStampDetail.Where(x => x.Id == ArthmateSlaId && !x.IsDeleted && x.IsActive).FirstOrDefaultAsync();
            if (isExistRecord != null)
            {
                if (isExistRecord.DateofUtilisation != null)
                {
                    reply.IsSuccess = false;
                    reply.Message = "Stamp Is Already In Use Can't be delete";
                    return reply;
                }
                if (isExistRecord.LeadmasterId != 0 && isExistRecord.LeadmasterId != null)
                {
                    reply.IsSuccess = false;
                    reply.Message = "Stamp is already taken by lead Id " + isExistRecord.LeadmasterId + " . Can't be delete";
                    return reply;
                }
                isExistRecord.IsDeleted = true;
                isExistRecord.IsActive = false;
                _context.Entry(isExistRecord).State = EntityState.Modified;
                _context.SaveChanges();
                reply.IsSuccess = true;
                reply.Message = "Data deleted successfully";
            }
            else
            {
                reply.IsSuccess = false;
                reply.Message = "Record not Exist";
            }
            return reply;
        }

        public async Task<ResultViewModel<bool>> StampPaperNumberCheck(int stampPaperNo)
        {
            ResultViewModel<bool> reply = new ResultViewModel<bool>();
            var isExistPaperNo = await _context.ArthmateSlaLbaStampDetail.Where(x => x.IsActive && !x.IsDeleted && x.StampPaperNo == stampPaperNo).FirstOrDefaultAsync();
            if (isExistPaperNo != null)
            {
                reply.IsSuccess = false;
                reply.Message = "Stamp Number is already exist";
            }
            else { reply.IsSuccess = true; }
            return reply;
        }

        public async Task<ResultViewModel<bool>> UploadLeadDocuments(LeadDocumentDetailDTO leadDocumentDetailDTO)
        {
            ResultViewModel<bool> reply = new ResultViewModel<bool>();
            #region insert Lead Document Details
            var existingleadDocDetails = await _context.LeadDocumentDetails.Where(x => x.LeadId == leadDocumentDetailDTO.LeadId && x.DocumentName == leadDocumentDetailDTO.DocumentName && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (existingleadDocDetails != null)
            {
                existingleadDocDetails.IsActive = false;
                existingleadDocDetails.IsDeleted = true;
                _context.Entry(existingleadDocDetails).State = EntityState.Modified;


                List<LeadDocumentDetail> leadDocumentDetails = new List<LeadDocumentDetail>();
                if (!string.IsNullOrEmpty(leadDocumentDetailDTO.DocumentName) && leadDocumentDetailDTO.DocumentName == "GST Certificate")
                {

                    leadDocumentDetails.Add(new LeadDocumentDetail
                    {
                        DocumentName = BlackSoilBusinessDocNameConstants.GstCertificate,
                        DocumentNumber = existingleadDocDetails.DocumentNumber,
                        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                        FileUrl = leadDocumentDetailDTO.FileUrl,
                        LeadId = leadDocumentDetailDTO.LeadId,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
                if (!string.IsNullOrEmpty(leadDocumentDetailDTO.DocumentName) && leadDocumentDetailDTO.DocumentName == "Other")
                {
                    leadDocumentDetails.Add(new LeadDocumentDetail
                    {
                        DocumentName = BlackSoilBusinessDocNameConstants.Other,
                        DocumentNumber = string.IsNullOrEmpty(existingleadDocDetails.DocumentNumber) ? "" : existingleadDocDetails.DocumentNumber,
                        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                        FileUrl = leadDocumentDetailDTO.FileUrl,
                        LeadId = leadDocumentDetailDTO.LeadId,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
                if (!string.IsNullOrEmpty(leadDocumentDetailDTO.DocumentName) && leadDocumentDetailDTO.DocumentName == "UdyogAadhaar")
                {
                    leadDocumentDetails.Add(new LeadDocumentDetail
                    {
                        DocumentName = BlackSoilBusinessDocNameConstants.UdyogAadhaar,
                        DocumentNumber = existingleadDocDetails.DocumentNumber ?? "",
                        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                        FileUrl = leadDocumentDetailDTO.FileUrl,
                        LeadId = leadDocumentDetailDTO.LeadId,
                        IsActive = true,
                        IsDeleted = false
                    });
                }

                if (!string.IsNullOrEmpty(leadDocumentDetailDTO.DocumentName) && leadDocumentDetailDTO.DocumentName == "Statement")
                {
                    leadDocumentDetails.Add(new LeadDocumentDetail
                    {
                        DocumentName = BlackSoilBusinessDocNameConstants.Statement,
                        DocumentNumber = existingleadDocDetails.DocumentNumber,
                        DocumentType = BlackSoilBusinessDocTypeConstants.IdProof,
                        FileUrl = leadDocumentDetailDTO.FileUrl,
                        LeadId = leadDocumentDetailDTO.LeadId,
                        IsActive = true,
                        IsDeleted = false,
                        PdfPassword = leadDocumentDetailDTO.PdfPassword
                    });
                }

                if (leadDocumentDetails != null && leadDocumentDetails.Any())
                {
                    _context.LeadDocumentDetails.AddRange(leadDocumentDetails);
                }
            }
            else
            {

            }
            #endregion
            if (await _context.SaveChangesAsync() > 0)
            {
                reply.IsSuccess = true;
                reply.Message = "Uploaded Successfully";
            }
            else
            {
                reply.IsSuccess = false;
                reply.Message = "Something nwent wrong!!";
            }
            return reply;
        }

        public async Task<ResultViewModel<List<LeadDocumentDetailDTO>>> GetLeadDocumentsByLeadId(long LeadId)
        {
            ResultViewModel<List<LeadDocumentDetailDTO>> reply = new ResultViewModel<List<LeadDocumentDetailDTO>>();
            #region get Lead Document Details
            reply.Result = await _context.LeadDocumentDetails.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).Select(x => new LeadDocumentDetailDTO
            {
                DocumentName = x.DocumentName,
                DocumentNumber = x.DocumentNumber,
                LeadId = x.LeadId,
                FileUrl = x.FileUrl,
                PdfPassword = x.PdfPassword,
                LeadDocDetailId = x.Id,
                Sequence = x.Sequence
            }).OrderByDescending(x => x.LeadDocDetailId).ToListAsync();
            //var response = await _context.LeadDocumentDetails.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).ToListAsync();

            //foreach(var item in response)
            //{
            //    LeadDocumentDetailDTO leadDocumentDetailDTO = new LeadDocumentDetailDTO{
            //        DocumentName = item.DocumentName,
            //        DocumentNumber = item.DocumentNumber,
            //        LeadId = item.LeadId,
            //        FileUrl = item.FileUrl,
            //        PdfPassword = item.PdfPassword
            //    };
            //    reply.Result.Add(leadDocumentDetailDTO);
            //}

            if (reply.Result != null)
            {
                reply.IsSuccess = true;
                reply.Message = "Data Found!";
            }
            else
            {
                reply.IsSuccess = false;
                reply.Message = "No Data Found!";
            }
            #endregion           
            return reply;
        }

        public async Task<ResultViewModel<bool>> UpdateCibilDetails(AddCibilDetailsRequestDTO addCibilDetailsRequestDTO)
        {
            ResultViewModel<bool> result = new ResultViewModel<bool> { Message = "", Result = false, IsSuccess = false };
            var lead = _context.Leads.FirstOrDefault(x => x.Id == addCibilDetailsRequestDTO.LeadId && x.ProductCode == addCibilDetailsRequestDTO.ProductCode && x.IsActive && !x.IsDeleted);
            if (lead != null)
            {
                lead.CibilReport = addCibilDetailsRequestDTO.CibilReport;
                lead.CreditScore = addCibilDetailsRequestDTO.CibilScore;
                _context.Entry(lead).State = EntityState.Modified;
                if (_context.SaveChanges() > 0)
                {
                    result.Result = true;
                    result.IsSuccess = true;
                    result.Message = "Cibil Details Updated Successfully";
                }
            }
            return result;
        }

        #region Cashfree

        public async Task<ResultViewModel<CashfreeCreateSubscriptionDetailResponse>> GenerateSubscriptionwithPlanInfo(long leadId)
        {
            ResultViewModel<CashfreeCreateSubscriptionDetailResponse> reply = new ResultViewModel<CashfreeCreateSubscriptionDetailResponse>();
            CashfreeHelper cashfreeHelper = new CashfreeHelper();

            var cashfreeEnach = await _context.CashfreeEnachs.Where(x => x.IsActive && !x.IsDeleted && x.LeadId == leadId).FirstOrDefaultAsync();
            if (cashfreeEnach != null)
            {
                var leadloan = await _context.LeadLoan.FirstOrDefaultAsync(x => x.LeadMasterId == leadId && x.IsActive && !x.IsDeleted);
                DateConvertHelper _DateConvertHelper = new DateConvertHelper();
                var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

                var subscriptiondetail = await GetSubscriptionDetails(leadId, cashfreeEnach.subReferenceId);
                if (subscriptiondetail != null && subscriptiondetail.IsSuccess && subscriptiondetail.Result.subscription.status.Trim().ToUpper() == "ACTIVE")
                {
                    if (leadloan != null && string.IsNullOrEmpty(leadloan.UMRN))
                    {
                        //Patch api need to call
                    }
                    var leadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadId
                                         && !x.IsCompleted && x.IsActive && !x.IsDeleted && x.ActivityMasterName == ActivityConstants.ENach && x.SubActivityMasterName == SubActivityConstants.Emandate).FirstOrDefaultAsync();
                    if (leadActivity != null)
                    {
                        leadActivity.IsCompleted = true;
                        leadActivity.IsApproved = 1;
                        _context.Entry(leadActivity).State = EntityState.Modified;
                    }
                    cashfreeEnach.Umrn = subscriptiondetail.Result.subscription.umn;
                    _context.Entry(cashfreeEnach).State = EntityState.Modified;
                    leadloan.UMRN = subscriptiondetail.Result.subscription.umn;
                    _context.Entry(leadloan).State = EntityState.Modified;
                    reply.IsSuccess = await _context.SaveChangesAsync() > 0;
                    reply.Message = "Enach successully done.";
                    return reply;
                }
                else if (currentDateTime > cashfreeEnach.linkExpiryDate)
                {
                    cashfreeEnach.IsActive = false;
                    cashfreeEnach.IsDeleted = true;
                    _context.Entry(cashfreeEnach).State = EntityState.Modified;

                    var leadnbfcApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.CashfreeCancelSubscription && x.IsActive && !x.IsDeleted);
                    var cancelSubscription = await cashfreeHelper.CancelSubscription(leadnbfcApi.APIUrl.Replace("{{subReferenceId}}", cashfreeEnach.subReferenceId), leadnbfcApi.TAPIKey, leadnbfcApi.TAPISecretKey, leadnbfcApi.Id, leadId);//cashfreeEnach.subReferenceId, 
                    if (cancelSubscription.IsSuccess)
                    {
                        reply = await CreateSubscriptionwithPlanInfo(leadId);
                    }
                    reply.Message = subscriptiondetail.Message;
                    return reply;
                }
                else
                {
                    reply.Message = subscriptiondetail.Message != null ? subscriptiondetail.Message : "Status Of Subscription detail is in " + subscriptiondetail.Result.subscription.status.Trim().ToUpper() + " mode";
                    return reply;

                }

            }
            if (cashfreeEnach == null)
            {
                reply = await CreateSubscriptionwithPlanInfo(leadId);
            }
            else
            {
                reply.Message = "Something went wrong.";
                reply.IsSuccess = false;
            }
            return reply;
        }
        public async Task<ResultViewModel<CashfreeCreateSubscriptionDetailResponse>> CreateSubscriptionwithPlanInfo(long leadId)
        {
            ResultViewModel<CashfreeCreateSubscriptionDetailResponse> reply = new ResultViewModel<CashfreeCreateSubscriptionDetailResponse>();

            var leadLoanDetails = await _context.LeadLoan.FirstOrDefaultAsync(x => x.LeadMasterId == leadId && x.IsActive && !x.IsDeleted);
            var arthmateUpdateDetails = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadId && x.IsActive && !x.IsDeleted);
            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadId && x.IsActive && !x.IsDeleted);
            var cashfreeenachconfig = await _context.cashFreeEnachconfigurations.FirstOrDefaultAsync();
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            if (leadLoanDetails != null && arthmateUpdateDetails != null && leadPersonalDetails != null && cashfreeenachconfig != null)
            {
                int subscount = await _context.CashfreeEnachs.CountAsync(x => x.LeadId == leadId);


                CashfreeEnach cashfreeEnach = new CashfreeEnach
                {
                    LeadId = leadId,
                    expiresOn = currentDateTime.AddYears(cashfreeenachconfig.expiresOnYear),//currentDateTime.Date.AddYears(3)
                    linkExpiryDate = currentDateTime.AddMinutes(cashfreeenachconfig.linkExpiryDay > 0 ? cashfreeenachconfig.linkExpiryDay * 24 * 60 : 5), // 5 * 24 * 60, 5day minute
                    maxCycles = Convert.ToInt32(arthmateUpdateDetails.Tenure),
                    planName = leadLoanDetails.loan_id,
                    recurringAmount = leadLoanDetails.emi_amount ?? 0, //emi amount
                    subscriptionId = leadLoanDetails.loan_id + "" + (subscount == 0 ? 1 : subscount + 1), //sequence me add
                    status = "INITIATED",
                    IsActive = true,
                    IsDeleted = false
                };
                await _context.CashfreeEnachs.AddAsync(cashfreeEnach);
                await _context.SaveChangesAsync();
                CashfreeHelper cashfreeHelper = new CashfreeHelper();
                var notes = new Notes
                {
                    key1 = "value1",
                    key2 = "value2",
                    key3 = "value3",
                    key4 = "value4"
                };

                var CreateRequest = new CreateSubscriptionwithPlanInfoInput
                {
                    authAmount = 1,
                    customerEmail = leadPersonalDetails.EmailId,
                    customerName = leadPersonalDetails.PanNameOnCard ?? "",
                    customerPhone = leadPersonalDetails.MobileNo ?? "",
                    expiresOn = cashfreeEnach.expiresOn.ToString("yyyy-MM-dd HH:mm:ss"),
                    notificationChannels = new List<string> { "EMAIL", "SMS" },
                    returnUrl = "",
                    subscriptionId = cashfreeEnach.subscriptionId,
                    notes = notes,
                    firstChargeDate = leadLoanDetails.first_inst_date.HasValue ? leadLoanDetails.first_inst_date.Value.ToString("yyyy-MM-dd") : currentDateTime.Date.ToString("yyyy-MM-dd"),
                    planInfo = new PlanInfo
                    {
                        intervals = cashfreeenachconfig.intervals,//1
                        intervalType = cashfreeenachconfig.intervalType ?? "MONTH",//"MONTH",
                        linkExpiry = cashfreeenachconfig.linkExpiryDay > 0 ? cashfreeenachconfig.linkExpiryDay * 24 * 60 : 5,
                        maxAmount = leadLoanDetails.sanction_amount.HasValue ? leadLoanDetails.sanction_amount.Value * cashfreeenachconfig.maxAmountMultiplier : 0,//leadLoanDetails.sanction_amount.HasValue ? leadLoanDetails.sanction_amount.Value * 3.0 : 0,
                        maxCycles = cashfreeEnach.maxCycles,
                        planName = cashfreeEnach.planName,//"AMLAAYAIR100000005788"
                        recurringAmount = leadLoanDetails.emi_amount ?? 0,
                        type = cashfreeenachconfig.type ?? "PERIODIC"//"PERIODIC"
                    }
                };
                var leadnbfcApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.CashfreeCreateSubscription && x.IsActive && !x.IsDeleted);
                var response = await cashfreeHelper.CreateSubscriptionwithPlanInfo(CreateRequest, leadnbfcApi.APIUrl, leadnbfcApi.TAPIKey, leadnbfcApi.TAPISecretKey, leadnbfcApi.Id, leadId);
                response.LeadId = leadId;
                await _context.CashfreeCommonAPIRequestResponses.AddAsync(response);
                await _context.SaveChangesAsync();

                if (response.IsSuccess)
                {

                    var CashfreeCreateSubscriptionDetailResponse = JsonConvert.DeserializeObject<CashfreeCreateSubscriptionDetailResponse>(response.Response);

                    cashfreeEnach.authLink = CashfreeCreateSubscriptionDetailResponse.data.authLink;
                    cashfreeEnach.subReferenceId = CashfreeCreateSubscriptionDetailResponse.data.subReferenceId;

                    _context.Entry(cashfreeEnach).State = EntityState.Modified;
                    reply.IsSuccess = await _context.SaveChangesAsync() > 0;
                    reply.Result = CashfreeCreateSubscriptionDetailResponse;
                    var remainingDay = (int)(cashfreeEnach.linkExpiryDate - cashfreeEnach.Created).TotalDays;
                    var remainingHr = (int)(cashfreeEnach.linkExpiryDate - cashfreeEnach.Created).TotalHours;
                    var remainingMin = (int)(cashfreeEnach.linkExpiryDate - cashfreeEnach.Created).TotalMinutes;

                    reply.Message = remainingHr + ":" + remainingMin + " | " + remainingDay + " days left";

                }
            }
            return reply;
        }
        public async Task<ResultViewModel<CashfreeGetSubscriptionResponse>> GetSubscriptionDetails(long leadId, string subReferenceId)
        {
            ResultViewModel<CashfreeGetSubscriptionResponse> reply = new ResultViewModel<CashfreeGetSubscriptionResponse>();
            CashfreeHelper cashfreeHelper = new CashfreeHelper();

            var leadnbfcApi = _context.LeadNBFCApis.FirstOrDefault(x => x.Code == CompanyApiConstants.CashfreeGetSubscriptionDetails && x.IsActive && !x.IsDeleted);
            var response = await cashfreeHelper.GetSubscriptionDetails(leadnbfcApi.APIUrl.Replace("{{subReferenceId}}", subReferenceId), leadnbfcApi.TAPIKey, leadnbfcApi.TAPISecretKey, leadnbfcApi.Id, leadId);//subReferenceId, 
            response.LeadId = leadId;
            await _context.CashfreeCommonAPIRequestResponses.AddAsync(response);
            await _context.SaveChangesAsync();
            if (response.IsSuccess)
            {
                reply.Result = JsonConvert.DeserializeObject<CashfreeGetSubscriptionResponse>(response.Response);
                reply.IsSuccess = true;
            }
            else
            {
                reply.Message = response.Response;
                reply.IsSuccess = false;

            }
            return reply;
        }


        #endregion

        public async Task<ResultViewModel<bool>> CheckIsOfferRejected(long leadId)
        {
            ResultViewModel<bool> result = new ResultViewModel<bool> { Message = "Offer Not Rejected" };
            var leadUpdate = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadId && x.IsActive && !x.IsDeleted);
            if (leadUpdate != null && leadUpdate.IsOfferRejected != null && leadUpdate.IsOfferRejected.Value)
            {
                result.Result = true;
                result.IsSuccess = true;
                result.Message = "Offer Rejected";
            }
            return result;
        }

        #region DSA
        public async Task<CommonResponseDc> SaveDSAPayouts(List<AddLeadPayOutsDTO> req)
        {
            CommonResponseDc res = new CommonResponseDc { Msg = "Failed to save due to Status is not Submitted" };
            var leadStatus = await _context.Leads.FirstOrDefaultAsync(x => x.Id == req.First().leadId && x.IsActive && !x.IsDeleted);
            if (leadStatus != null && leadStatus.Status == LeadStatusEnum.Submitted.ToString())
            {
                var existDSAPayouts = await _context.DSAPayouts.Where(x => x.LeadId == req.First().leadId && x.IsActive && !x.IsDeleted).ToListAsync();
                if (existDSAPayouts != null)
                {
                    foreach (var item in existDSAPayouts)
                    {
                        item.IsActive = false;
                        item.IsDeleted = true;
                        _context.Entry(item).State = EntityState.Modified;
                    }
                }
                foreach (var item in req)
                {
                    DSAPayout payout = new DSAPayout
                    {
                        LeadId = item.leadId,
                        PayoutPercenatge = item.payoutPerc,
                        MaxAmount = item.MaxAmount,
                        MinAmount = item.MinAmount,
                        IsActive = true,
                        IsDeleted = false,
                        ProductId = item.ProductId
                    };
                    await _context.DSAPayouts.AddAsync(payout);
                }
                if (await _context.SaveChangesAsync() > 0)
                {
                    res.Msg = "Data saved Successfully";
                    res.Status = true;
                }
            }
            return res;
        }

        public async Task<CommonResponseDc> DSADeactivate(long leadId, bool isActive, bool isReject)
        {
            CommonResponseDc res = new CommonResponseDc() { Msg = "Record not exists" };
            if (leadId > 0)
            {
                var existRecord = await _context.Leads.FirstOrDefaultAsync(x => x.Id == leadId && !x.IsDeleted);
                if (existRecord != null)
                {
                    if (isReject)
                    {
                        existRecord.IsActive = isActive;
                        existRecord.IsDeleted = isReject;
                    }
                    else
                    {
                        existRecord.IsActive = isActive;
                    }
                    _context.Entry(existRecord).State = EntityState.Modified;
                }
                if (await _context.SaveChangesAsync() > 0)
                {
                    res.Status = true;
                    if (isActive) res.Msg = "Activated successfully!";
                    else res.Msg = "Deactivated successfully!";
                }
                else
                {
                    res.Status = false;
                    res.Msg = "failed to Save";
                }
            }
            return res;
        }


        public async Task<CommonResponseDc> GetDSAAgreement(long LeadId)
        {
            CommonResponseDc reply = new CommonResponseDc() { Msg = "Data Not Found!!!" };

            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadId && x.IsActive && !x.IsDeleted);
            if (lead != null)
            {
                var leadAggrement = _context.leadAgreements.FirstOrDefaultAsync(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted);
                if (leadAggrement != null)
                {
                    reply.Data = leadAggrement;
                    reply.Status = true;
                    reply.Msg = "Data Found!!";
                }
            }
            return reply;
        }

        public async Task<bool> UpdateLeadGenerator(long LeadId, string UserName)
        {
            var LeadInfo = _context.Leads.Where(x => x.Id == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (LeadInfo != null)
            {
                if (!string.IsNullOrEmpty(LeadInfo.LeadGenerator))
                {
                    return false;
                }
                else
                {
                    LeadInfo.LeadGenerator = UserName;
                    LeadInfo.LeadConverter = UserName;
                    _context.Entry(LeadInfo).State = EntityState.Modified;
                    var rowChanges = await _context.SaveChangesAsync();
                    if (rowChanges > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> AddLeadConsentLog(long LeadId, string DocType, string UserId)
        {
            if(string.IsNullOrEmpty(UserId))
            {
                UserId = await _context.Leads.AsNoTracking().Where(x => x.Id == LeadId && x.IsActive && !x.IsDeleted).Select(x=>x.UserName).FirstOrDefaultAsync();
            }
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

                #region Make History
                string doctypeFonrConsent = "LeadConsentLog_" + DocType;
                
                var result = await _leadHistoryManager.GetLeadHistroy(LeadId, doctypeFonrConsent);
                LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                {
                    LeadId = LeadId,
                    UserID = result.UserId,
                    UserName = "",
                    EventName = doctypeFonrConsent, //"AddLeadConsentLog_PostLeadPAN_PostLeadAadharVerifyOTP"
                    Narretion = result.Narretion,
                    NarretionHTML = result.NarretionHTML,
                    CreatedTimeStamp = result.CreatedTimeStamp
                };
                await _massTransitService.Publish(histroyEvent);
                #endregion


                return true;
            }
            return false;
        }


        #endregion

        #region MAS finance Agreement
        public async Task<ResultViewModel<LeadAgreement>> GetMASFinanceAgreement(long LeadId)
        {
            ResultViewModel<LeadAgreement> result = new ResultViewModel<LeadAgreement> { Message = "Data Not Found" };
            if (LeadId > 0)
            {
                LeadAgreement leadagreement = await _context.leadAgreements.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                if (leadagreement != null)
                {
                    result.Result = leadagreement;
                    result.IsSuccess = true;
                    result.Message = "Data Found!";
                    return result;
                }
            }
            return result;
        }

        #endregion

        #region MAS/AYE finance Update OfferAmount
        public async Task<ResultViewModel<bool>> UpdateLeadOfferByFinance(UpdateLeadOfferFinanceRequestDTO updateLeadOfferFinanceRequestDTO)
        {
            ResultViewModel<bool> result = new ResultViewModel<bool> { Message = "Data Not Found" };

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            var nbfcoffer = _context.nbfcOfferUpdate.FirstOrDefault(x => x.LeadId == updateLeadOfferFinanceRequestDTO.LeadId && x.NBFCCompanyId == updateLeadOfferFinanceRequestDTO.NBFCCompanyId && x.IsActive && !x.IsDeleted);
            if (nbfcoffer != null && (nbfcoffer.OfferStatus == LeadOfferConstant.OfferRejected))
            {
                result.IsSuccess = false;
                result.Message = "offer status Rejected";
                return result;
            }

            var leadoffer = await _context.LeadOffers.FirstOrDefaultAsync(x => x.NBFCCompanyId == updateLeadOfferFinanceRequestDTO.NBFCCompanyId && x.LeadId == updateLeadOfferFinanceRequestDTO.LeadId && x.IsActive && !x.IsDeleted);
            if (leadoffer != null)
            {
                var nbfcOffer = await _context.nbfcOfferUpdate.Where(x => x.LeadId == leadoffer.LeadId && x.NBFCCompanyId == updateLeadOfferFinanceRequestDTO.NBFCCompanyId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();

                if (nbfcOffer != null && (nbfcOffer.PFDiscount == null || nbfcOffer.PFDiscount == 0) && updateLeadOfferFinanceRequestDTO.PFDiscount > 0)
                {
                    double PFamount = updateLeadOfferFinanceRequestDTO.ProcessingFeeAmount ?? 0;
                    double PFdiscount = updateLeadOfferFinanceRequestDTO.PFDiscount ?? 0;
                    double? PFRate = updateLeadOfferFinanceRequestDTO.ProcessingFeeRate;
                    string? PFType = updateLeadOfferFinanceRequestDTO.PFType;
                    double gst = updateLeadOfferFinanceRequestDTO.GST ?? 0;
                    double finalPFAmount = (PFamount - PFdiscount);
                    //double processingfee = PFType == "Percentage" ? Math.Round(Convert.ToDouble(finalPFAmount * PFRate / 100), 2) : finalPFAmount;
                    double PFAmountwithTax = Math.Round((finalPFAmount * gst / 100), 2);

                    nbfcOffer.PFDiscount = updateLeadOfferFinanceRequestDTO.PFDiscount;
                    //nbfcOffer.ProcessingFeeAmount = finalPFAmount;
                    //nbfcOffer.ProcessingFeeTax = PFAmountwithTax;
                    nbfcOffer.ReviseProcessingFeeTax = PFAmountwithTax;
                    nbfcOffer.LastModified = currentDateTime;
                    _context.Entry(nbfcOffer).State = EntityState.Modified;
                }
                else
                {
                    if (updateLeadOfferFinanceRequestDTO.LoanId == null || updateLeadOfferFinanceRequestDTO.LoanId == "")
                    {
                        result.IsSuccess = false;
                        result.Message = "LoanId Required";
                        result.Result = false;
                        return result;
                    }
                    var LoanIdExist = await _context.nbfcOfferUpdate.Where(x => x.NBFCCompanyId == updateLeadOfferFinanceRequestDTO.NBFCCompanyId && x.LoanId == updateLeadOfferFinanceRequestDTO.LoanId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                    if (LoanIdExist != null && !string.IsNullOrEmpty(LoanIdExist.LoanId))
                    {
                        result.IsSuccess = false;
                        result.Message = "Loan Id can not be duplicate ";
                        result.Result = false;
                    }
                    else
                    {
                        nbfcOfferUpdate nbfcOfferUpdate = new nbfcOfferUpdate();
                        nbfcOfferUpdate.LoanId = updateLeadOfferFinanceRequestDTO.LoanId ?? "";
                        nbfcOfferUpdate.LeadId = leadoffer.LeadId;
                        nbfcOfferUpdate.NBFCCompanyId = leadoffer.NBFCCompanyId;
                        nbfcOfferUpdate.CompanyIdentificationCode = leadoffer.CompanyIdentificationCode ?? "";
                        nbfcOfferUpdate.Tenure = updateLeadOfferFinanceRequestDTO.Tenure ?? 0;
                        nbfcOfferUpdate.InterestRate = updateLeadOfferFinanceRequestDTO.InterestRate;
                        nbfcOfferUpdate.LoanInterestAmount = updateLeadOfferFinanceRequestDTO.LoanInterestAmount;
                        nbfcOfferUpdate.LoanAmount = updateLeadOfferFinanceRequestDTO.CreditLimit;
                        nbfcOfferUpdate.MonthlyEMI = updateLeadOfferFinanceRequestDTO.MonthlyEMI;
                        nbfcOfferUpdate.ProcessingFeeAmount = updateLeadOfferFinanceRequestDTO.ProcessingFeeAmount;
                        nbfcOfferUpdate.ProcessingFeeRate = updateLeadOfferFinanceRequestDTO.ProcessingFeeRate;
                        nbfcOfferUpdate.ProcessingFeeTax = updateLeadOfferFinanceRequestDTO.ProcessingFeeTax;
                        nbfcOfferUpdate.GST = updateLeadOfferFinanceRequestDTO.GST;
                        nbfcOfferUpdate.AnchorCompanyId = updateLeadOfferFinanceRequestDTO.AnchorCompanyId ?? 0;
                        nbfcOfferUpdate.OfferStatus = LeadOfferConstant.OfferGenerated;
                        nbfcOfferUpdate.PFType = updateLeadOfferFinanceRequestDTO.PFType;
                        nbfcOfferUpdate.IsActive = true;
                        nbfcOfferUpdate.IsDeleted = false;
                        nbfcOfferUpdate.Created = currentDateTime;
                        nbfcOfferUpdate.Commission = updateLeadOfferFinanceRequestDTO.Commission;
                        nbfcOfferUpdate.CommissionType = updateLeadOfferFinanceRequestDTO.CommissionType;

                        nbfcOfferUpdate.NBFCProcessingFee = updateLeadOfferFinanceRequestDTO.NBFCProcessingFee;
                        nbfcOfferUpdate.NBFCProcessingFeeType = updateLeadOfferFinanceRequestDTO.NBFCProcessingFeeType;
                        nbfcOfferUpdate.NBFCInterest = updateLeadOfferFinanceRequestDTO.NBFCInterest;
                        nbfcOfferUpdate.NBFCPenal = updateLeadOfferFinanceRequestDTO.NBFCPenal;
                        nbfcOfferUpdate.NBFCBounce = updateLeadOfferFinanceRequestDTO.NBFCBounce;
                        nbfcOfferUpdate.Penal = updateLeadOfferFinanceRequestDTO.Penal;
                        nbfcOfferUpdate.Bounce = updateLeadOfferFinanceRequestDTO.Bounce;
                        nbfcOfferUpdate.ArrangementType = updateLeadOfferFinanceRequestDTO.ArrangementType;

                        _context.Add(nbfcOfferUpdate);

                        leadoffer.CreditLimit = updateLeadOfferFinanceRequestDTO.CreditLimit;
                        leadoffer.Status = LeadOfferConstant.OfferGenerated;
                        _context.Entry(leadoffer).State = EntityState.Modified;
                    }

                }
                if (_context.SaveChanges() > 0)
                {
                    result.IsSuccess = true;
                    result.Message = "Information Updated";
                    result.Result = true;
                }
            }

            return result;
        }

        public async Task<ResultViewModel<GetAcceptedLoanDetailDC>> GetAcceptedLoanDetail(long LeadId)
        {
            ResultViewModel<GetAcceptedLoanDetailDC> reply = new ResultViewModel<GetAcceptedLoanDetailDC> { Message = "Data Not Found" };

            var Leadmasterid = new SqlParameter("leadmasterid", LeadId);

            try
            {
                var acceptedLoanDetail = _context.Database.SqlQueryRaw<GetAcceptedLoanDetailDC>("exec GetAcceptedLoanDetail @leadmasterid", Leadmasterid).AsEnumerable().FirstOrDefault();
                if (acceptedLoanDetail != null)
                {
                    reply.IsSuccess = true;
                    reply.Result = acceptedLoanDetail;
                    reply.Message = "data found";
                }
                else
                {
                    reply.IsSuccess = false;
                    reply.Message = "data not found";
                }
            }
            catch (Exception ex)
            {
                reply.IsSuccess = false;
                reply.Message = ex.Message;
            }
            return reply;
        }

        public async Task<ResultViewModel<string>> RejectNBFCOffer(RejectNBFCOfferDC req)
        {
            ResultViewModel<string> reply = new ResultViewModel<string> { Message = "Failed to Update" };

            var nbfcoffer = _context.nbfcOfferUpdate.FirstOrDefault(x => x.LeadId == req.LeadId && x.NBFCCompanyId == req.nbfcCompanyId && x.IsActive && !x.IsDeleted);
            if (nbfcoffer != null && (nbfcoffer.OfferStatus == LeadOfferConstant.OfferGenerated || nbfcoffer.OfferStatus == "Generated"))
            {
                reply.IsSuccess = false;
                reply.Message = "Can not Reject";
                return reply;
            }
            else
            {
                try
                {
                    nbfcOfferUpdate businessLoanNBFCUpdate = new nbfcOfferUpdate();
                    businessLoanNBFCUpdate.LoanId = "";
                    businessLoanNBFCUpdate.LeadId = req.LeadId;
                    businessLoanNBFCUpdate.NBFCCompanyId = req.nbfcCompanyId;
                    businessLoanNBFCUpdate.CompanyIdentificationCode = "";
                    businessLoanNBFCUpdate.Tenure = 0;
                    businessLoanNBFCUpdate.InterestRate = 0;
                    businessLoanNBFCUpdate.LoanInterestAmount = 0;
                    businessLoanNBFCUpdate.LoanAmount = 0;
                    businessLoanNBFCUpdate.MonthlyEMI = 0;
                    businessLoanNBFCUpdate.ProcessingFeeAmount = 0;
                    businessLoanNBFCUpdate.ProcessingFeeRate = 0;
                    businessLoanNBFCUpdate.PFDiscount = 0;
                    businessLoanNBFCUpdate.ProcessingFeeTax = 0;
                    businessLoanNBFCUpdate.IsActive = true;
                    businessLoanNBFCUpdate.IsDeleted = false;
                    businessLoanNBFCUpdate.NBFCRemark = req.RejectMessage;
                    businessLoanNBFCUpdate.OfferStatus = LeadOfferConstant.OfferRejected;
                    businessLoanNBFCUpdate.PFType = "";

                    _context.Add(businessLoanNBFCUpdate);

                    if (await _context.SaveChangesAsync() > 0)
                    {
                        reply.IsSuccess = true;
                        reply.Message = "Updated Successfully";
                    }
                    else
                    {
                        reply.IsSuccess = false;
                        reply.Message = "Failed to Update";
                    }
                }
                catch (Exception ex)
                {
                    reply.IsSuccess = false;
                    reply.Message = ex.Message;
                }
            }
            return reply;
        }

        public async Task<GRPCReply<string>> GenerateKarzaAadhaarOtpForNBFC(GRPCRequest<AcceptOfferByLeadDc> request)
        {
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            GRPCReply<string> res = new GRPCReply<string>();

            var leadinfo = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request.LeadId && x.IsActive && !x.IsDeleted);
            var personalInfo = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted);
            //var personalInfo = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted);

            if (leadinfo != null)
            {
                var nbfcupdate = await _context.nbfcOfferUpdate.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.NBFCCompanyId == request.Request.NBFCCompanyId && x.IsActive && !x.IsDeleted);
                //var config = request.Request.ProductSlabConfigResponse;
                if (nbfcupdate != null)
                {
                    double? processing_fee = 0;
                    double? ProcessingfeeTax = 0;
                    double LoanAmount = 0;
                    double RateOfInterest = 0;
                    int Tenure = 0;
                    double monthlyPayment = 0;
                    double loanIntAmt = 0;
                    double PFPer = 0;
                    double? GST = 0;


                    LoanAmount = request.Request.Amount;
                    //var PF = config.FirstOrDefault(x => x.SlabType == SlabTypeConstants.PF);
                    RateOfInterest = (leadinfo.InterestRate > 0 ? leadinfo.InterestRate : nbfcupdate.InterestRate) ?? 0;
                    Tenure = Convert.ToInt32(request.Request.Tenure);
                    monthlyPayment = Math.Round(Convert.ToDouble(CalculateMonthlyPayment(RateOfInterest, Tenure, LoanAmount)), 2);
                    loanIntAmt = Math.Round(((Convert.ToDouble(monthlyPayment) * Tenure) - LoanAmount), 2);
                    PFPer = nbfcupdate.ProcessingFeeRate ?? 0; //config.First(x => x.SlabType == SlabTypeConstants.PF).MaxValue;
                    GST = nbfcupdate.GST;
                    processing_fee = nbfcupdate.PFType != null && nbfcupdate.PFType == "Percentage" ? Math.Round((LoanAmount * PFPer) / 100, 2) : PFPer;
                    ProcessingfeeTax = Math.Round(Convert.ToDouble(processing_fee * GST) / 100, 2);
                    var pftax = (nbfcupdate.ReviseProcessingFeeTax != null || nbfcupdate.ReviseProcessingFeeTax > 0 ? nbfcupdate.ReviseProcessingFeeTax : nbfcupdate.ProcessingFeeTax);

                    var BusinessLoan = await _context.BusinessLoanNBFCUpdate.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.NBFCCompanyId == request.Request.NBFCCompanyId && x.IsActive && !x.IsDeleted);
                    if (BusinessLoan != null)
                    {
                        BusinessLoan.LoanAmount = request.Request.Amount;
                        BusinessLoan.InterestRate = leadinfo.InterestRate;
                        BusinessLoan.LoanInterestAmount = loanIntAmt;
                        BusinessLoan.Tenure = request.Request.Tenure;
                        BusinessLoan.MonthlyEMI = monthlyPayment;
                        BusinessLoan.ProcessingFeeRate = PFPer;
                        BusinessLoan.ProcessingFeeAmount = processing_fee;
                        BusinessLoan.ProcessingFeeTax = pftax;
                        BusinessLoan.GST = GST;
                        BusinessLoan.OfferStatus = 1;
                        BusinessLoan.PFDiscount = nbfcupdate.PFDiscount;
                        BusinessLoan.Commission = nbfcupdate.Commission;
                        BusinessLoan.CommissionType = nbfcupdate.CommissionType;
                        BusinessLoan.PFType = nbfcupdate.PFType;
                        BusinessLoan.CompanyIdentificationCode = nbfcupdate.CompanyIdentificationCode;
                        BusinessLoan.LoanId = nbfcupdate.LoanId;
                        BusinessLoan.NBFCCompanyId = nbfcupdate.NBFCCompanyId;
                        BusinessLoan.LastModified = currentDateTime;

                        BusinessLoan.NBFCBounce = nbfcupdate.NBFCBounce !=null ? nbfcupdate.NBFCBounce : 0 ;
                        BusinessLoan.NBFCPenal = nbfcupdate.NBFCPenal != null ? nbfcupdate.NBFCPenal : 0;
                        BusinessLoan.Bounce = nbfcupdate.Bounce != null ? nbfcupdate.Bounce : 0;
                        BusinessLoan.Penal = nbfcupdate.Penal != null ? nbfcupdate.Penal : 0;
                        BusinessLoan.NBFCProcessingFee = nbfcupdate.NBFCProcessingFee;
                        BusinessLoan.NBFCProcessingFeeType = nbfcupdate.NBFCProcessingFeeType;
                        BusinessLoan.ArrangementType = nbfcupdate.ArrangementType;
                        BusinessLoan.NBFCInterest = nbfcupdate.NBFCInterest;

                        _context.Entry(BusinessLoan).State = EntityState.Modified;
                    }
                    else
                    {
                        BusinessLoanNBFCUpdate BLNBFCUpdate = new BusinessLoanNBFCUpdate
                        {
                            LoanId = nbfcupdate.LoanId,
                            NBFCCompanyId = nbfcupdate.NBFCCompanyId,
                            LeadId = request.Request.LeadId,
                            LoanAmount = request.Request.Amount,
                            InterestRate = leadinfo.InterestRate,
                            LoanInterestAmount = loanIntAmt,
                            Tenure = request.Request.Tenure,
                            MonthlyEMI = monthlyPayment,
                            ProcessingFeeRate = PFPer,
                            ProcessingFeeAmount = nbfcupdate.ProcessingFeeAmount,
                            ProcessingFeeTax = nbfcupdate.ReviseProcessingFeeTax,
                            GST = GST,
                            OfferStatus = 1,
                            PFDiscount = nbfcupdate.PFDiscount,
                            Commission = nbfcupdate.Commission,
                            CommissionType = nbfcupdate.CommissionType,
                            PFType = nbfcupdate.PFType,
                            CompanyIdentificationCode = nbfcupdate.CompanyIdentificationCode,
                            Created = currentDateTime,
                            IsActive = true,
                            IsDeleted = false,

                            NBFCBounce = nbfcupdate.NBFCBounce != null ? nbfcupdate.NBFCBounce : 0 ,
                            NBFCPenal = nbfcupdate.NBFCPenal != null ? nbfcupdate.NBFCPenal : 0,
                            Bounce = nbfcupdate.Bounce != null ? nbfcupdate.Bounce : 0,
                            Penal = nbfcupdate.Penal != null ? nbfcupdate.Penal : 0,
                            NBFCProcessingFee = nbfcupdate.NBFCProcessingFee,
                            NBFCProcessingFeeType = nbfcupdate.NBFCProcessingFeeType,
                            ArrangementType = nbfcupdate.ArrangementType,
                            NBFCInterest = nbfcupdate.NBFCInterest
                        };
                        _context.BusinessLoanNBFCUpdate.Add(BLNBFCUpdate);
                    }
                    //_context.SaveChanges();

                    leadinfo.Status = LeadBusinessLoanStatusConstants.LoanActivated;
                    _context.Entry(leadinfo).State = EntityState.Modified;

                    //_context.SaveChanges();

                    var leadActivityMasterProgresses = await _context.LeadActivityMasterProgresses.FirstOrDefaultAsync(x => x.LeadMasterId == request.Request.LeadId &&
                                                                                         x.ActivityMasterId == request.Request.ActivityId &&
                                                                                         x.SubActivityMasterId == request.Request.SubActivityId &&
                                                                                         x.IsActive && !x.IsDeleted);
                    if (leadActivityMasterProgresses != null)
                    {
                        leadActivityMasterProgresses.IsCompleted = true;
                        if (leadActivityMasterProgresses.IsApproved == 2)
                        {
                            leadActivityMasterProgresses.IsApproved = 0;
                        }
                        _context.Entry(leadActivityMasterProgresses).State = EntityState.Modified;
                    }
                    if (await _context.SaveChangesAsync() > 0)
                    {
                        res.Status = true;
                        res.Response = "offer accepted";
                        res.Message = "offer accepted";

                        //------------------S : Make log---------------------
                        #region Make History
                        var result = await _leadHistoryManager.GetLeadHistroy(request.Request.LeadId, "GenerateKarzaAadhaarOtpForNBFC");
                        LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                        {
                            LeadId = request.Request.LeadId,
                            UserID = result.UserId,
                            UserName = "",
                            EventName = "LeadOffers Approved/Reject By User",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                            Narretion = result.Narretion,
                            NarretionHTML = result.NarretionHTML,
                            CreatedTimeStamp = result.CreatedTimeStamp
                        };
                        await _massTransitService.Publish(histroyEvent);
                        #endregion
                        //------------------E : Make log---------------------

                    }
                }
                //}
            }
            else
            {
                res.Status = true;
                res.Response = "Personal detail not found";
                res.Message = "Personal detail not found";
            }
            return res;
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
        public async Task<ResultViewModel<List<LeadOfferInitiatedStatusResponseDTO>>> GetLeadOfferInitiatedStatus(long LeadId)
        {
            ResultViewModel<List<LeadOfferInitiatedStatusResponseDTO>> reply = new ResultViewModel<List<LeadOfferInitiatedStatusResponseDTO>> { Message = "Data Not Found" };
            List<LeadOfferInitiatedStatusResponseDTO> leadOfferInitiatedStatusResponseDTO = new List<LeadOfferInitiatedStatusResponseDTO>();
            leadOfferInitiatedStatusResponseDTO = await _context.LeadOffers.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).Select(y => new LeadOfferInitiatedStatusResponseDTO
            {
                LeadId = y.LeadId,
                NbfcCompanyId = y.NBFCCompanyId,
                Status = y.Status
            }).ToListAsync();
            if (leadOfferInitiatedStatusResponseDTO.Any() && leadOfferInitiatedStatusResponseDTO != null)
            {

                reply.IsSuccess = true;
                reply.Message = "data found";
                reply.Result = leadOfferInitiatedStatusResponseDTO;
            }
            return reply;
        }
        #endregion

        public async Task<ResultViewModel<string>> OfferReject(OfferRejectDc obj)
        {

            ResultViewModel<string> res = new ResultViewModel<string>();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == obj.LeadId && x.IsActive && !x.IsDeleted);
            if (lead != null)
            {
                var offer = await _context.LeadOffers.FirstOrDefaultAsync(x => x.LeadId == obj.LeadId && x.NBFCCompanyId == obj.NBFCCompanyId && x.IsActive && !x.IsDeleted);
                if (offer != null && (lead.OfferCompanyId == null || lead.OfferCompanyId == 0))
                {
                    offer.Status = LeadOfferConstant.OfferRejected;
                    offer.Comment = obj.Message;
                    _context.Entry(offer).State = EntityState.Modified;
                    if (_context.SaveChanges() > 0)
                    {
                        res.Message = "offer rejected succesfully";
                        res.IsSuccess = true;
                        res.Result = "offer rejected succesfully";
                    }
                    else
                    {
                        res.Message = "something went wrong";
                        res.IsSuccess = false;
                        res.Result = "";
                    }

                }
                else
                {
                    res.Message = "Offer already approved";
                    res.IsSuccess = false;
                    res.Result = "";
                }
            }
            return res;
        }


        public async Task<List<LeadUpdateHistorieDTO>> GetLeadUpdateHistorie(LeadUpdateHistorieRequestDTO obj)
        {

            List<LeadUpdateHistorieDTO> list = new List<LeadUpdateHistorieDTO>();
            var leadIdValue = new SqlParameter("LeadId", obj.LeadId);
            var eventNameValue = new SqlParameter("EventName", obj.EventName);
            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);

            try
            {
                list = _context.Database.SqlQueryRaw<LeadUpdateHistorieDTO>("exec GetLeadUpdateHistorie @LeadID, @EventName,@Skip,@Take", leadIdValue, eventNameValue, skip, take).AsEnumerable().ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return list;
        }

        public async Task<GRPCReply<string>> ProceedCreditApplication(long leadId)
        {

            GRPCReply<string> gRPCReply = new GRPCReply<string>();

            return gRPCReply;
        }

        public async Task<ResultViewModel<bool>> NbfcOfferAccepted(NbfcOfferRequestDc obj)
        {

            ResultViewModel<bool> res = new ResultViewModel<bool>();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == obj.LeadId && x.IsActive && !x.IsDeleted);
            if (lead != null)
            {
                BusinessLoanNBFCUpdate nbfcOffer = new BusinessLoanNBFCUpdate();

                if (obj.NbfcCompanyId > 0)
                {
                    nbfcOffer = await _context.BusinessLoanNBFCUpdate.FirstOrDefaultAsync(x => x.LeadId == obj.LeadId && x.NBFCCompanyId == obj.NbfcCompanyId && x.OfferStatus == 1 && x.IsActive && !x.IsDeleted);
                }
                else
                {
                    nbfcOffer = await _context.BusinessLoanNBFCUpdate.FirstOrDefaultAsync(x => x.LeadId == obj.LeadId && x.OfferStatus == 1 && x.IsActive && !x.IsDeleted);
                }
                if (nbfcOffer != null)
                {
                    res.Message = "Found";
                    res.IsSuccess = true;
                    res.Result = true;
                }
                else
                {
                    res.Message = "Not Found";
                    res.IsSuccess = false;
                    res.Result = false;
                }
            }
            return res;
        }

        //public async Task<bool> UpdateKYCStatus(ManageKYCStatusDc request, string UserId)
        //{
        //    bool result = true;
        //    if (request != null && request.LeadId > 0)
        //    {
        //        var Lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.LeadId);
        //        var AllleadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == request.LeadId && x.IsDeleted == false).ToListAsync();
        //        if (AllleadActivity != null)
        //        {
        //            var leadActivityMasterProgresses = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId &&
        //                                                                      x.ActivityMasterId == request.ActivityMasterId &&
        //                                                                      x.SubActivityMasterId == request.SubActivityMasterId &&
        //                                                                      x.IsActive && !x.IsDeleted);
        //            if (leadActivityMasterProgresses != null)
        //            {

        //                var RejectleadActivity = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == request.LeadId
        //                                         && x.ActivityMasterName == ActivityEnum.Rejected.ToString());

        //                //List<string> UserIdList = new List<string> { UserId };
        //                //UserListDetailsRequest userReq = new UserListDetailsRequest
        //                //{
        //                //    userIds = UserIdList,
        //                //    keyword = null,
        //                //    Skip = 0,
        //                //    Take = 10
        //                //};
        //                //var userReply = await _iIdentityService.GetUserById(userReq);
        //                //var username = userReply.UserListDetails != null ? userReply.UserListDetails.Select(y => y.UserName).FirstOrDefault() : "";

        //                //if (RejectleadActivity != null && RejectleadActivity.IsActive && request.IsRejected)
        //                //{
        //                //    foreach (var item in AllleadActivity.Where(x => x.Sequence > leadActivityMasterProgresses.Sequence))
        //                //    {
        //                //        item.IsActive = false;
        //                //        if (RejectleadActivity.Sequence - 1 == item.Sequence)
        //                //        {
        //                //            item.IsApproved = 1;
        //                //        }
        //                //        _context.Entry(item).State = EntityState.Modified;
        //                //    }

        //                //    RejectleadActivity.IsActive = true;
        //                //    _context.Entry(RejectleadActivity).State = EntityState.Modified;
        //                //}
        //                //else if (RejectleadActivity != null && !request.IsRejected)
        //                //{
        //                Lead.Status = LeadStatusEnum.KYCSuccess.ToString();
        //                    Lead.LastModified = DateTime.Now;
        //                    _context.Entry(Lead).State = EntityState.Modified;
        //                    foreach (var item in AllleadActivity.Where(x => x.Sequence > leadActivityMasterProgresses.Sequence))
        //                    {
        //                        item.IsActive = true;
        //                        _context.Entry(item).State = EntityState.Modified;
        //                    }
        //                    RejectleadActivity.IsActive = false;
        //                    RejectleadActivity.Sequence = leadActivityMasterProgresses.Sequence + 1;
        //                    RejectleadActivity.RejectMessage = "Status Changed By " + UserId;//Username Add
        //                    _context.Entry(RejectleadActivity).State = EntityState.Modified;

        //                //}
        //                var res = await _context.SaveChangesAsync();
        //            }
        //        }
        //    }
        //    return result;
        //}


        public async Task<bool> UpdateKYCStatusForMismatchAadhar(long LeadId, string UserId)
        {
            LeadActivityMasterProgresses leadActivityMasterProgresses = new LeadActivityMasterProgresses
            {
                ActivityMasterId = 0,
                ActivityMasterName = "",
                IsApproved = 0,
                IsCompleted = false,
                LeadMasterId = 0,
                Sequence = 0
            };
            if (!string.IsNullOrEmpty(UserId))
            {
                var leadData = await _context.Leads.Where(x => x.Id == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                leadData.Status = LeadStatusEnum.KYCReject.ToString();
                leadData.LastModified = DateTime.Now;
                leadData.LastModifiedBy = UserId;
                _context.Entry(leadData).State = EntityState.Modified;

                var AllleadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == LeadId && x.IsDeleted == false).ToListAsync();
                if (AllleadActivity != null)
                {
                        leadActivityMasterProgresses = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == LeadId &&
                                                                             x.ActivityMasterName.ToUpper() == "KYC" &&
                                                                             x.SubActivityMasterName.ToUpper() == KYCMasterConstants.Aadhar.ToUpper() &&
                                                                             x.IsActive && !x.IsDeleted);
                    if (leadActivityMasterProgresses != null)
                    {
                        var RejectleadActivity = AllleadActivity.FirstOrDefault(x => x.LeadMasterId == LeadId
                                                 && x.ActivityMasterName == ActivityEnum.Rejected.ToString());


                        foreach (var item in AllleadActivity.Where(x => x.Sequence >= leadActivityMasterProgresses.Sequence))
                        {
                            item.IsActive = false;
                            item.IsDeleted = true;
                            _context.Entry(item).State = EntityState.Modified;
                        }
                        RejectleadActivity.IsActive = true;
                        RejectleadActivity.IsDeleted = false;
                        RejectleadActivity.Sequence = leadActivityMasterProgresses.Sequence + 1;
                        _context.Entry(RejectleadActivity).State = EntityState.Modified;
                    }
                }
            }    
            var rowChanges = await _context.SaveChangesAsync();
            if (rowChanges > 0)
            {
                return true;
            }
            return false;
        }



    }
}
