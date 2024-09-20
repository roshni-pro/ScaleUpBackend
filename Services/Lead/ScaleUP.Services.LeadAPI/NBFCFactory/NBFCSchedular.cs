using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadModels;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using Newtonsoft.Json;
using ScaleUP.Services.LeadDTO.NBFC.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using System.Diagnostics;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Response;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;

namespace ScaleUP.Services.LeadAPI.NBFCFactory
{
    public class NBFCSchedular
    {
        private readonly LeadApplicationDbContext _context;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly LeadNBFCFactory _leadNBFCFactory;
        public NBFCSchedular(LeadApplicationDbContext context, LeadNBFCSubActivityManager leadNBFCSubActivityManager, LeadNBFCFactory leadNBFCFactory)
        {
            _context = context;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _leadNBFCFactory = leadNBFCFactory;
        }

        public async Task<GRPCReply<bool>> GenerateOfferForLead(long leadId)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool> { Status = true, Message = "No Activity left for Generate Offer" };
            //1: get all pending subactivity
            var activityList = _leadNBFCSubActivityManager.GetActivityData(leadId, ActivityConstants.GenerateOffer);

            //2: 
            if (activityList != null && activityList.Any())
            {

                foreach (var activity in activityList)
                {
                    var nbfcService = _leadNBFCFactory.GetService(activity.CompanyIdentificationCode);
                    if (nbfcService != null)
                    {
                        var res = await nbfcService.GenerateOffer(leadId, activity.NBFCCompanyId);
                        gRPCReply.Status = res.IsSuccess;
                        gRPCReply.Message = res.Message;
                    }
                }
            }
            return gRPCReply;
        }

        public async Task GenerateBlackSoilOfferForLead(long leadId)
        {
            //1: get all pending subactivity
            var activityList = _leadNBFCSubActivityManager.GetActivityData(leadId, ActivityConstants.GenerateOffer, LeadNBFCConstants.BlackSoil);

            //2: 
            if (activityList != null && activityList.Any())
            {

                foreach (var activity in activityList)
                {
                    var nbfcService = _leadNBFCFactory.GetService(activity.CompanyIdentificationCode);
                    if (nbfcService != null)
                    {
                        await nbfcService.GenerateOffer(leadId, activity.NBFCCompanyId);
                    }

                }
            }
        }

        public async Task GenerateAgreement(long leadId)
        {
            //1: get all pending subactivity
            var activityList = _leadNBFCSubActivityManager.GetActivityData(leadId, ActivityConstants.Agreement);

            //2: 
            if (activityList != null && activityList.Any())
            {

                foreach (var activity in activityList)
                {
                    var nbfcService = _leadNBFCFactory.GetService(activity.CompanyIdentificationCode);
                    if (nbfcService != null)
                    {
                        await nbfcService.PrpareAgreement(leadId, activity.NBFCCompanyId);
                    }

                }
            }
        }

        public async Task<long> LineInitiateOrRejectBlackSoil(long leadId, string status, string errorMessage = "")
        {
            long leadOfferId = 0;
            var query = from lo in _context.LeadOffers
                        join lns in _context.LeadNBFCSubActivitys
                            on new { lo.LeadId, lo.NBFCCompanyId } equals new { lns.LeadId, lns.NBFCCompanyId }
                        where lns.IdentificationCode == LeadNBFCConstants.BlackSoil
                                && lns.Code == SubActivityConstants.SendToLos
                                && lo.IsActive && !lo.IsDeleted
                                && lns.IsActive && !lns.IsDeleted
                                && lo.LeadId == leadId
                        select lo;

            var leadOffer = query.FirstOrDefault();

            if (leadOffer != null)
            {
                leadOffer.Status = status;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    leadOffer.ErrorMessage = errorMessage;
                }
                //update entity code
                _context.Entry(leadOffer).State = EntityState.Modified;


                //_leadNBFCSubActivityManager.UpdateSubActivityStatus(api.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Completed);
                //_leadNBFCSubActivityManager.UpdateStatus(api.LeadNBFCApiId, status, response.Id);
                _context.SaveChanges();
                leadOfferId = leadOffer.Id;
            }
            return leadOfferId;

        }


        public async Task GenerateAgreementRetry(long leadId, long NbfcId)
        {


            //1: get all pending subactivity
            var activityList = _leadNBFCSubActivityManager.GetActivityData(leadId, ActivityConstants.Agreement, nbfcId: NbfcId);

            //2: 
            if (activityList != null && activityList.Any())
            {

                foreach (var activity in activityList)
                {
                    var nbfcService = _leadNBFCFactory.GetService(activity.CompanyIdentificationCode);
                    if (nbfcService != null)
                    {
                        await nbfcService.PrpareAgreement(leadId, activity.NBFCCompanyId);
                    }

                }
            }
        }
        public async Task<GRPCReply<string>> GenerateOfferForLeadRetry(long leadId, long NbfcId)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string> { Status = true, Message = "Offer Generated" };
            //1: get all pending subactivity
            var activityList = _leadNBFCSubActivityManager.GetActivityData(leadId, ActivityConstants.GenerateOffer, nbfcId: NbfcId);

            //2: 
            if (activityList != null && activityList.Any())
            {

                foreach (var activity in activityList)
                {
                    var nbfcService = _leadNBFCFactory.GetService(activity.CompanyIdentificationCode);
                    if (nbfcService != null)
                    {
                        var res = await nbfcService.GenerateOffer(leadId, activity.NBFCCompanyId);
                        gRPCReply.Status = res.IsSuccess;
                        gRPCReply.Message = res.Message;
                    }
                }
            }
            return gRPCReply;
        }


        public async Task UpdateLeadInfo(long leadId, long NbfcId, string companyIdentificationCode)
        {

            var nbfcService = _leadNBFCFactory.GetService(companyIdentificationCode);
            if (nbfcService != null)
            {
                try
                {
                    await nbfcService.PanUpdate(leadId, NbfcId);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    await nbfcService.AadhaarUpdate(leadId, NbfcId);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    await nbfcService.BusinessUpdate(leadId, NbfcId);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    await nbfcService.PersonUpdate(leadId, NbfcId);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    await nbfcService.PersonAddressUpdate(leadId, NbfcId);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    await nbfcService.BusinessAddressUpdate(leadId, NbfcId);
                }
                catch (Exception ex)
                {

                }
            }
        }


        public async Task<ICreateLeadNBFCResponse> BlackSoilCommonApplicationDetail(long leadId)
        {
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.BlackSoil);
            return await nbfcService.BlackSoilCommonApplicationDetail(leadId);

        }

        public async Task<ResultViewModel<bool>> BlackSoilInitiateApproved()
        {

            var result = new ResultViewModel<bool>
            {
                IsSuccess = false,
                Message = "something went wrong.",
                Result = false
            };
            var leadOffers = await _context.LeadOffers.Where(x => x.IsActive
            && !x.IsDeleted && (x.Status == LeadOfferConstant.Initiated || x.Status == LeadOfferConstant.AwaitingInitiated) && x.CompanyIdentificationCode == LeadNBFCConstants.BlackSoil.ToString()).ToListAsync();
            if (leadOffers != null && leadOffers.Any())
            {
                var blackSoilCommonApiForSecret = await _context.LeadNBFCApis.Where(x => x.Code == CompanyApiConstants.BlackSoilCommonApiForSecret && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();

                if (blackSoilCommonApiForSecret != null)
                {
                    foreach (var leadOffer in leadOffers)
                    {
                        var lead = await _context.Leads.Where(x => x.Id == leadOffer.Id).FirstOrDefaultAsync();
                        var BlackSoilUpdate = await _context.BlackSoilUpdates.Where(x => x.LeadId == lead.Id).FirstOrDefaultAsync();
                        if (lead != null && leadOffer != null)
                        {
                            string apiurl = BlackSoilUpdate.BusinessUpdateUrl + "?expand=applications,credit_line,loan_account";
                            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
                            var response = await blackSoilNBFCHelper.BlackSoilCommonBusinessDetail(apiurl, blackSoilCommonApiForSecret.TAPIKey, blackSoilCommonApiForSecret.TAPISecretKey);
                            _context.BlackSoilCommonAPIRequestResponses.Add(response);
                            _context.SaveChanges();
                            if (response != null && response.IsSuccess)
                            {

                                var BlackSoilInitiateApprovedManual = JsonConvert.DeserializeObject<BlackSoilInitiateApprovedManualDTO>(response.Response);
                                if (BlackSoilInitiateApprovedManual != null)
                                {
                                    if (BlackSoilInitiateApprovedManual.applications.Any(x => x.status == "line_approved")) //LineApproved
                                    {
                                        if (leadOffer.Status == LeadOfferConstant.Initiated || leadOffer.Status == LeadOfferConstant.AwaitingInitiated)  //Approved
                                        {
                                            if (!BlackSoilUpdate.ApplicationId.HasValue || BlackSoilUpdate.ApplicationId.Value == 0)
                                            {
                                                BlackSoilUpdate.ApplicationId = BlackSoilInitiateApprovedManual.applications.FirstOrDefault().id;
                                                BlackSoilUpdate.ApplicationCode = BlackSoilInitiateApprovedManual.applications.FirstOrDefault().application_id;
                                                _context.Entry(BlackSoilUpdate).State = EntityState.Modified;

                                                lead.Status = LeadStatusEnum.LineApproved.ToString();
                                                _context.Entry(lead).State = EntityState.Modified;
                                                _context.SaveChanges();
                                            }
                                            try
                                            {
                                                await GenerateBlackSoilOfferForLead(lead.Id);
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                    }
                                    else if (BlackSoilInitiateApprovedManual.applications.Any(x => x.status == "line_initiated"))
                                    {
                                        if (leadOffer.Status == LeadOfferConstant.AwaitingInitiated) //Line Initiated
                                        {
                                            try
                                            {
                                                BlackSoilUpdate.ApplicationId = BlackSoilInitiateApprovedManual.applications.FirstOrDefault().id;
                                                BlackSoilUpdate.ApplicationCode = BlackSoilInitiateApprovedManual.applications.FirstOrDefault().application_id;
                                                _context.Entry(BlackSoilUpdate).State = EntityState.Modified;
                                                lead.Status = LeadStatusEnum.LineInitiated.ToString();
                                                _context.Entry(lead).State = EntityState.Modified;
                                                _context.SaveChanges();

                                                await LineInitiateOrRejectBlackSoil(lead.Id, LeadOfferConstant.OfferGenerated, "");
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                    }
                                }
                                result.IsSuccess = true;
                                result.Message = "Success.";
                                result.Result = true;
                            }
                        }

                    }
                }
            }
            return result;
        }





        public async Task<ResultViewModel<string>> GetPFCollection(long leadid)
        {
            ResultViewModel<string> createLeadNBFCResponse = new ResultViewModel<string>();
            var lead = await _context.Leads.Where(x => x.Id == leadid).FirstOrDefaultAsync();

            if (lead != null && lead.OfferCompanyId > 0)
            {
                var activityList = _leadNBFCSubActivityManager.GetActivityData(leadid, ActivityConstants.PFCollection, nbfcId: lead.OfferCompanyId ?? 0);

                if (activityList != null && activityList.Any())
                {
                    var nbfcService = _leadNBFCFactory.GetService(activityList.FirstOrDefault().CompanyIdentificationCode);
                    if (nbfcService != null)
                    {
                        createLeadNBFCResponse = await nbfcService.GetPFCollection(leadid, lead.MobileNo);
                    }
                }
            }
            return createLeadNBFCResponse;
        }

        #region AyeFinanceSCF
        public async Task<GRPCReply<string>> GenerateToken()
        {
            var res = new GRPCReply<string>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                res = await nbfcService.GenerateToken();
            }
            return res;
        }
        public async Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            var res = new GRPCReply<string>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                res = await nbfcService.AddLead(ayeleadReq);
            }
            return res;
        }

        public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            var res = new GRPCReply<CheckCreditLineData>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                res = await nbfcService.CheckCreditLine(ayeleadReq);
            }
            return res;
        }

        public async Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            var res = new GRPCReply<string>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                res = await nbfcService.GetWebUrl(ayeleadReq);
            }
            return res;
        }

        public async Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> ayeleadReq)
        {   
            var res = new GRPCReply<string>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                res = await nbfcService.TransactionSendOtp(ayeleadReq);
            }
            return res;
        }public async Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> txnVerifyOtp)
        {   
            var res = new GRPCReply<string>();
            var nbfcService = _leadNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                res = await nbfcService.TransactionVerifyOtp(txnVerifyOtp);
            }
            return res;
        }

        #endregion
    }
}

