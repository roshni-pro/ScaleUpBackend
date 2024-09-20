using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadModels.LeadNBFC;
using System.Net;
using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using System.Data;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using ScaleUP.Services.LeadDTO.Constant;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Global.Infrastructure.Constants.NBFC;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadDTO.NBFC.BlackSoil;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Global.Infrastructure.Constants;
using iTextSharp.text.pdf.parser.clipper;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;


namespace ScaleUP.Services.LeadAPI.Manager
{
    public class LeadNBFCSubActivityManager
    {
        private readonly LeadApplicationDbContext _context;

        public LeadNBFCSubActivityManager(LeadApplicationDbContext context
            )
        {
            _context = context;
        }

        public async Task<List<LeadNBFCSubActivityDTO>> GetLeadNBFCSubActivity(LeadNBFCSubActivityRequest leadNBFCSubActivityRequest)
        {
            List<LeadNBFCSubActivityDTO> leadNBFCSubActivityDTOs = new List<LeadNBFCSubActivityDTO>();

            var query = from a in _context.LeadNBFCSubActivitys
                        join b in _context.LeadNBFCApis on a.Id equals b.LeadNBFCSubActivityId
                        where b.IsActive == true && b.IsDeleted == false
                        && a.LeadId == leadNBFCSubActivityRequest.LeadId
                        && a.Code == leadNBFCSubActivityRequest.Code
                        //&& a.NBFCCompanyId == leadNBFCSubActivityRequest.CompanyId
                        && a.IdentificationCode == leadNBFCSubActivityRequest.CompanyIdentificationCode
                        && (a.Status != LeadNBFCApiConstants.Completed && a.Status != LeadNBFCApiConstants.CompletedWithError)
                        && a.IsActive && !a.IsDeleted
                        select new LeadNBFCSubActivityDTO
                        {
                            Sequence = b.Sequence,
                            Code = b.Code,
                            APIUrl = b.APIUrl,
                            LeadNBFCApiId = b.Id,
                            RequestId = b.RequestId,
                            ResponseId = b.ResponseId,
                            Status = b.Status,
                            TAPIKey = b.TAPIKey,
                            TAPISecretKey = b.TAPISecretKey,
                            ReferrelCode = b.TReferralCode,
                            LeadNBFCSubActivityId = a.Id
                        };

            var LeadNBFCApisList = query.OrderBy(x => x.Sequence).ToList();


            return LeadNBFCApisList;
        }

        public async Task<List<LeadNBFCSubActivityDTO>> GetLeadNBFCSubActivity(long leadid, long companyId)
        {
            List<LeadNBFCSubActivityDTO> leadNBFCSubActivityDTOs = new List<LeadNBFCSubActivityDTO>();

            var query = from a in _context.LeadNBFCSubActivitys
                        join b in _context.LeadNBFCApis on a.SubActivityMasterId equals b.Id
                        where b.IsActive == true && b.IsDeleted == false
                        && a.LeadId == leadid
                        && a.NBFCCompanyId == companyId
                        //&& a.IdentificationCode == leadNBFCSubActivityRequest.CompanyIdentificationCode
                        && (a.Status != LeadNBFCApiConstants.Completed && a.Status != LeadNBFCApiConstants.CompletedWithError)
                        && a.IsActive && !a.IsDeleted
                        select b;

            var LeadNBFCApisList = query.ToList();
            leadNBFCSubActivityDTOs = LeadNBFCApisList.Select(x => new LeadNBFCSubActivityDTO
            {
                Sequence = x.Sequence,
                Code = x.Code,
                APIUrl = x.APIUrl,
                LeadNBFCApiId = x.Id,
                RequestId = x.RequestId,
                ResponseId = x.ResponseId,
                Status = x.Status,
                TAPIKey = x.TAPIKey,
                TAPISecretKey = x.TAPISecretKey,
                ReferrelCode = x.TReferralCode

            }).ToList();

            return leadNBFCSubActivityDTOs;
        }



        public bool UpdateStatus(long Id, string Status, long RequestId)
        {
            var UpdateStatus = _context.LeadNBFCApis.Where(x => x.Id == Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateStatus != null)
            {
                UpdateStatus.Status = Status;
                UpdateStatus.RequestId = RequestId;
                UpdateStatus.ResponseId = RequestId;
                _context.Entry(UpdateStatus).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                _context.SaveChanges();
            }
            return true;
        }

        public bool UpdateStatus(long Id, string Status)
        {
            var UpdateStatus = _context.LeadNBFCApis.Where(x => x.Id == Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateStatus != null)
            {
                UpdateStatus.Status = Status;
                _context.Entry(UpdateStatus).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.SaveChanges();
            }
            return true;
        }

        public bool UpdateLeadProgressStatus(long LeadId, string ActivityName, bool IsCompleted = true)
        {
            var UpdateStatus = _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == LeadId && x.ActivityMasterName == ActivityName && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateStatus != null)
            {
                UpdateStatus.IsCompleted = IsCompleted;
                _context.Entry(UpdateStatus).State = EntityState.Modified;
                _context.SaveChanges();
            }
            return true;
        }

        public bool UpdateLeadMasterStatus(long LeadId, string Status)
        {
            var UpdateStatus = _context.Leads.Where(x => x.Id == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateStatus != null)
            {
                UpdateStatus.Status = Status;
                _context.Entry(UpdateStatus).State = EntityState.Modified;
                _context.SaveChanges();
            }
            return true;
        }

        public List<LeadNBFCSubActivity> GetSubactivityData(long leadid)
        {
            var query = from s in _context.LeadNBFCSubActivitys
                        where s.LeadId == leadid
                            && (s.Status != LeadNBFCSubActivityConstants.Completed && s.Status != LeadNBFCSubActivityConstants.CompletedWithError)
                            && s.IsActive == true && s.IsDeleted == false
                        select s;
            var result = query.ToList()
                .OrderBy(x => x.NBFCCompanyId)
                //.ThenBy(y => y.)
                .ToList();

            return result;
        }

        public List<NBFCLeadSubactivityDTO> GetActivityData(long leadid)
        {
            var query = from s in _context.LeadNBFCSubActivitys
                        where s.LeadId == leadid
                            && (s.Status != LeadNBFCSubActivityConstants.Completed && s.Status != LeadNBFCSubActivityConstants.CompletedWithError)
                            && s.IsActive == true && s.IsDeleted == false
                        group s by new { s.NBFCCompanyId, s.ActivityName } into grp
                        select new NBFCLeadSubactivityDTO
                        {
                            NBFCCompanyId = grp.Key.NBFCCompanyId,
                            ActivityName = grp.Key.ActivityName,
                            CompanyIdentificationCode = grp.First().IdentificationCode
                        };
            var result = query.ToList();

            return result;
        }

        public List<NBFCLeadSubactivityDTO> GetActivityData(long leadid, string activityName, string companyIdentificationCode = "", long nbfcId = 0)
        {
            var query = from s in _context.LeadNBFCSubActivitys
                        where s.LeadId == leadid
                            && (s.Status != LeadNBFCSubActivityConstants.Completed && s.Status != LeadNBFCSubActivityConstants.CompletedWithError)
                            && s.IsActive == true && s.IsDeleted == false
                            && s.ActivityName == activityName
                            && !string.IsNullOrEmpty(s.IdentificationCode)
                            && (nbfcId == 0 || s.NBFCCompanyId == nbfcId)
                            && (string.IsNullOrEmpty(companyIdentificationCode) || s.IdentificationCode == companyIdentificationCode)
                        group s by new { s.NBFCCompanyId, s.ActivityName } into grp
                        select new NBFCLeadSubactivityDTO
                        {
                            NBFCCompanyId = grp.Key.NBFCCompanyId,
                            ActivityName = grp.Key.ActivityName,
                            CompanyIdentificationCode = grp.First().IdentificationCode
                        };
            var result = query.ToList();

            return result;
        }

        public List<LeadNBFCSubActivity> GetSubactivityData(long leadid, long nbfcCompanyId, string activityName)
        {
            var query = from s in _context.LeadNBFCSubActivitys
                        where s.LeadId == leadid
                            && s.NBFCCompanyId == nbfcCompanyId
                            && (s.Status != LeadNBFCSubActivityConstants.Completed && s.Status != LeadNBFCSubActivityConstants.CompletedWithError)
                            && (s.ActivityName == activityName)
                            && s.IsActive == true && s.IsDeleted == false
                        select s;
            var result = query.ToList()
                .OrderBy(y => y.SubActivitySequence)
                .ToList();

            return result;
        }

        public async Task<ResultViewModel<bool>> IsAllCompleted(NBFCSubactivityCompletedInput nBFCSubactivityCompletedInput)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();
            var data = _context.LeadNBFCSubActivitys.Where(x => x.NBFCCompanyId == nBFCSubactivityCompletedInput.NBFCCompanyId && x.LeadId == nBFCSubactivityCompletedInput.LeadId
                        && x.IsActive && !x.IsDeleted
                        && x.Status != LeadNBFCSubActivityConstants.Completed && x.Status != LeadNBFCSubActivityConstants.CompletedWithError).FirstOrDefault();
            if (data != null)
            {
                res = new ResultViewModel<bool>
                {
                    Result = false,
                    Message = "Some activity not complete!",
                    IsSuccess = true
                };
            }
            else
            {
                res = new ResultViewModel<bool>
                {
                    Result = true,
                    Message = "Al; activities are completed!",
                    IsSuccess = true
                };
            }
            return res;
        }

        public bool UpdateSubActivityStatus(long Id, string Status)
        {
            var updateStatus = _context.LeadNBFCSubActivitys.Where(x => x.Id == Id && x.IsActive == true && !x.IsDeleted).FirstOrDefault();
            if (updateStatus != null)
            {
                updateStatus.Status = Status;
                _context.Entry(updateStatus).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                var Count = _context.SaveChanges() > 0;
            }
            return true;
        }


        public async Task<List<LeadCompanyGenerateOfferDTO>> GetLeadActivityOfferStatus(long LeadId, string ActivityName)
        {
            var result = new List<LeadCompanyGenerateOfferDTO>();
            var leadIdparam = new SqlParameter("leadId", LeadId);
            var activitynameparam = new SqlParameter("ActivityName", ActivityName);

            var leadcompanygenerateoffer = await _context.Database.SqlQueryRaw<LeadCompanyGenerateOfferDc>("exec GetGenerateOfferStatus   @leadId, @ActivityName", leadIdparam, activitynameparam).ToListAsync();

            if (leadcompanygenerateoffer != null)
            {
                var q = from l in leadcompanygenerateoffer
                        group l by new
                        {
                            l.CreditLimit,
                            l.ComapanyName,
                            l.ErrorMessage,
                            l.LeadOfferId,
                            l.LeadOfferStatus,
                            l.NBFCCompanyId
                        } into grp
                        select new LeadCompanyGenerateOfferDTO
                        {
                            CreditLimit = grp.Key.CreditLimit,
                            ComapanyName = grp.Key.ComapanyName,
                            LeadOfferErrorMessage = grp.Key.ErrorMessage,
                            LeadOfferId = grp.Key.LeadOfferId,
                            LeadOfferStatus = grp.Key.LeadOfferStatus,
                            NbfcCompanyId = grp.Key.NBFCCompanyId,
                            LeadId = LeadId,
                            SubactivityList = new List<LeadCompanyGenerateOfferSubactivityDTO>()
                        };

                result = q.ToList();

                if (result != null)
                {
                    foreach (var comp in result)
                    {
                        var r = from l in leadcompanygenerateoffer
                                where l.ComapanyName == comp.ComapanyName
                                group l by new
                                {
                                    l.LeadNBFCSubActivityId,
                                    l.ActivityMasterId,
                                    l.SubActivityMasterId,
                                    l.SubActivitySequence,
                                    l.Code,
                                    l.ActivityName,
                                    l.SubActivityStatus
                                } into grp
                                select new LeadCompanyGenerateOfferSubactivityDTO
                                {
                                    ActivityMasterId = grp.Key.ActivityMasterId,
                                    ActivityName = grp.Key.ActivityName,
                                    LeadNBFCSubActivityId = grp.Key.LeadNBFCSubActivityId,
                                    Sequence = grp.Key.SubActivitySequence,
                                    Status = grp.Key.SubActivityStatus,
                                    SubActivityMasterId = grp.Key.SubActivityMasterId,
                                    SubActivityName = grp.Key.Code,
                                    ApiList = grp.ToList().Select(x => new LeadCompanyGenerateOfferApiDTO
                                    {
                                        ApiId = x.APIId,
                                        ApiUrl = x.APIUrl,
                                        Code = x.ApiCode,
                                        Sequence = x.APISequence,
                                        ApiStatus = x.ApiStatus,
                                        Request = x.Request,
                                        Response = x.Response,
                                    }).ToList(),
                                };

                        comp.SubactivityList = r.ToList();
                    }
                }
            }


            return result;

        }

        public async Task<ResultViewModel<bool>> IsGenerateAgreementCompleted(long leadId)
        {
            bool isCompleted = false;
            var query = from s in _context.LeadNBFCSubActivitys
                        where s.LeadId == leadId
                         && s.IsActive && !s.IsDeleted
                         && s.ActivityName == ActivityConstants.Agreement
                        select s;
            var result = query.ToList();

            if (result != null)
            {
                isCompleted = !result.Any(x => x.Status != LeadNBFCSubActivityConstants.Completed && x.Status != LeadNBFCSubActivityConstants.CompletedWithError);
            }
            else
            {
                isCompleted = false;
            }
            return new ResultViewModel<bool>
            {
                IsSuccess = true,
                Message = "Success",
                Result = isCompleted
            };
        }

        public async Task<ResultViewModel<bool>> IsLeadCompleted(long leadId)
        {
            bool isCompleted = false;

            var leadActivity = await _context.LeadActivityMasterProgresses.Where(x => x.LeadMasterId == leadId
                                   && !x.IsCompleted && x.IsApproved == 0 && x.IsActive && !x.IsDeleted && x.SubActivityMasterName == SubActivityConstants.AgreementEsign).FirstOrDefaultAsync();
            if (leadActivity != null)
            {
                leadActivity.IsCompleted = true;
                leadActivity.IsApproved = 1;
                _context.Entry(leadActivity).State = EntityState.Modified;
                _context.SaveChanges();
                isCompleted = true;
            }
            return new ResultViewModel<bool>
            {
                IsSuccess = true,
                Message = "Success",
                Result = isCompleted
            };
        }


        public async Task<bool> TestUpdateApi(string updatetype)
        {
            BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();

            switch (updatetype)
            {
                case "pan":
                    await helper.PanUpdate(
                             new LeadDTO.NBFC.BlackSoil.BlackSoilPanUpdateInput
                             {
                                 doc_number = "ATBPY4993J",
                                 file = "https://internal.er15.xyz//ArthmateDocument/KYC/IMG-20240212-WA000012022024141041.jpg",
                                 update_url = "https://stag.saraloan.in/api/v1/core/persons/12560/documents/52614/",
                                 business = "12629",
                                 doc_name = "pan",
                                 doc_type = "id_proof",
                                 type = "person_document"
                             }, "shopkirana@saraloan.in", "shopkirana@saraloan.in");
                    break;

                case "aadhaar":
                    await helper.AadhaarUpdate(
                             new LeadDTO.NBFC.BlackSoil.BlackSoilAadhaarUpdateInput
                             {
                                 doc_number = "891746006439",
                                 file = "https://internal.er15.xyz//ArthmateDocument/KYC/IMG-20240212-WA000112022024141118.jpg",
                                 update_url = "https://stag.saraloan.in/api/v1/core/persons/12560/documents/52615/",
                                 business = "12629",
                                 doc_name = "aadhaar",
                                 doc_type = "id_proof",
                                 type = "person_document"
                             }, "shopkirana@saraloan.in", "shopkirana@saraloan.in");
                    break;
                case "person":
                    await helper.PersonUpdate(
                             new LeadDTO.NBFC.BlackSoil.BlackSoilPersonUpdateInput
                             {
                                 update_url = "https://stag.saraloan.in/api/v1/core/persons/12560/",
                                 first_name = "Test",
                                 full_name = "Test Deo",
                                 last_name = "Deo",
                                 dob = "2001-01-01",
                                 gender = "female",
                                 middle_name = ""
                             }, "shopkirana@saraloan.in", "shopkirana@saraloan.in");
                    break;
                case "business":
                    await helper.BusinessUpdate(
                             new LeadDTO.NBFC.BlackSoil.BlackSoilBusinessUpdateInput
                             {
                                 update_url = "https://stag.saraloan.in/api/v1/core/businesses/12629/",
                                 business_type = "partnership",
                                 name = "Test pvt"
                             }, "shopkirana@saraloan.in", "shopkirana@saraloan.in");
                    break;

                case "personaddress":
                    await helper.PersonAddressUpdate(
                             new LeadDTO.NBFC.BlackSoil.BlackSoilPersonAddressUpdateInput
                             {
                                 update_url = "https://stag.saraloan.in/api/v1/core/persons/12560/addresses/19641/",
                                 address_line = "Mondo D AddL",
                                 city = "Indore",
                                 state = "MP",
                                 country = "India",
                                 full_address = "Mondo D AddL,Indore,MP,India ",
                                 landmark = "",
                                 locality = "",
                                 pincode = "452001",
                                 address_name = "permanent_address",
                                 address_type = "person_address",
                                 business = "12629"
                             }, "shopkirana@saraloan.in", "shopkirana@saraloan.in");
                    break;
                case "businessaddress":
                    await helper.BusinessAddressUpdate(
                             new LeadDTO.NBFC.BlackSoil.BlackSoilBusinessAddressUpdateInput
                             {
                                 update_url = "https://stag.saraloan.in/api/v1/core/businesses/12629/addresses/19642/",
                                 address_line = "BMondo D AddL",
                                 city = "Indore",
                                 state = "MP",
                                 country = "India",
                                 full_address = "BMondo D AddL,Indore,MP,India ",
                                 landmark = "",
                                 locality = "",
                                 pincode = "452010",
                                 address_name = "mailing_address",
                                 address_type = "business_address",
                                 business = "12629"
                             }, "shopkirana@saraloan.in", "shopkirana@saraloan.in");
                    break;
            }
            return true;
        }


        public async Task<List<LeadCompanyGenerateOfferNewDTO>> GetLeadActivityOfferStatusNew(GRPCRequest<GenerateOfferStatusPost> request)
        {
            List<LeadCompanyGenerateOfferNewDTO> Loandetaillist = new List<LeadCompanyGenerateOfferNewDTO>();
            var result = new List<LeadCompanyGenerateOfferNewDTO>();
            var leadIdparam = new SqlParameter("leadId", request.Request.LeadId);
            var activitynameparam = new SqlParameter("ActivityName", ActivityConstants.GenerateOffer);

            var leadcompanygenerateoffer = await _context.Database.SqlQueryRaw<LeadCompanyGenerateOfferDc>("exec GetGenerateOfferStatus   @leadId, @ActivityName", leadIdparam, activitynameparam).ToListAsync();

            if (leadcompanygenerateoffer != null)
            {
                var q = from l in leadcompanygenerateoffer
                        group l by new
                        {
                            l.CreditLimit,
                            l.ComapanyName,
                            l.ErrorMessage,
                            l.LeadOfferId,
                            l.LeadOfferStatus,
                            l.NBFCCompanyId
                        } into grp
                        select new LeadCompanyGenerateOfferNewDTO
                        {
                            CreditLimit = grp.Key.CreditLimit,
                            ComapanyName = grp.Key.ComapanyName,
                            LeadOfferErrorMessage = grp.Key.ErrorMessage,
                            LeadOfferId = grp.Key.LeadOfferId,
                            LeadOfferStatus = grp.Key.LeadOfferStatus,
                            NbfcCompanyId = grp.Key.NBFCCompanyId,
                            LeadId = request.Request.LeadId,
                            SubactivityList = new List<LeadCompanyGenerateOfferSubactivityNewDTO>()
                        };

                result = q.ToList();

                if (result != null)
                {
                    var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request.LeadId && x.IsActive && !x.IsDeleted);
                    var leadoffer = _context.LeadOffers.Where(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted).ToList();

                    foreach (var offer in leadoffer)
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
                        string ComapanyName = "";
                        double PfDiscount = 0;
                        double? PFAfterDiscount = 0;
                        double? PFRevisedTax = 0;


                        LeadCompanyGenerateOfferNewDTO loan = new LeadCompanyGenerateOfferNewDTO();
                        if (request.Request.ProductType == ProductTypeConstants.BusinessLoan)
                        {

                            GetProductNBFCConfigResponseDc? loanConfig = null;
                            if (request.Request.GetProductNBFCConfigResponseDcs != null)
                                loanConfig = request.Request.GetProductNBFCConfigResponseDcs.FirstOrDefault(x => x.CompanyId == offer.NBFCCompanyId);
                            if (offer.CompanyIdentificationCode == CompanyIdentificationCodeConstants.ArthMate)
                            {
                                var arthMateUpdates = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted);
                                var colenderOffer = await _context.CoLenderResponse.FirstOrDefaultAsync(x => x.LeadMasterId == request.Request.LeadId && x.IsActive && !x.IsDeleted);

                                if (arthMateUpdates != null && loanConfig != null && lead != null && colenderOffer != null)
                                {
                                    LoanAmount = colenderOffer.SanctionAmount;
                                    RateOfInterest = lead.InterestRate > 0 ? lead.InterestRate.Value : loanConfig.InterestRate;
                                    Tenure = Convert.ToInt16(arthMateUpdates.Tenure);
                                    monthlyPayment = Math.Round(Convert.ToDouble(CalculateMonthlyPayment(RateOfInterest, Tenure, LoanAmount)), 2);
                                    loanIntAmt = Math.Round(((Convert.ToDouble(monthlyPayment) * Tenure) - LoanAmount), 2);
                                    PFPer = loanConfig.PF;
                                    GST = request.Request.GST;
                                    processing_fee = Math.Round((LoanAmount * PFPer) / 100, 2);
                                    ProcessingfeeTax = Math.Round(Convert.ToDouble(processing_fee * GST) / 100, 2);
                                    ComapanyName = offer.CompanyIdentificationCode;

                                    loan.CreditLimit = LoanAmount;
                                    loan.SanctonAmount = LoanAmount;
                                    loan.InterestRate = RateOfInterest;
                                    loan.Tenure = Tenure;
                                    loan.MonthlyEMI = monthlyPayment;
                                    loan.LoanInterestAmount = loanIntAmt;
                                    loan.Processingfee = processing_fee;
                                    loan.ProcessingfeeTax = ProcessingfeeTax;
                                    loan.ComapanyName = ComapanyName;
                                    loan.MinInterestRate = loanConfig.InterestRate;
                                    loan.MaxInterestRate = loanConfig.MaxInterestRate;
                                    loan.LeadOfferId = offer.Id;
                                    loan.NbfcCompanyId = offer.NBFCCompanyId;
                                    loan.LeadId = lead.Id;
                                    loan.OfferApprove = (lead.OfferCompanyId != null && offer.NBFCCompanyId == lead.OfferCompanyId) ? true : false;
                                    loan.ProcessingfeeRate = PFPer;
                                    loan.GSTRate = GST;
                                    Loandetaillist.Add(loan);
                                }
                                foreach (var comp in result)
                                {
                                    var r = from l in leadcompanygenerateoffer
                                            where l.ComapanyName == comp.ComapanyName
                                            group l by new
                                            {
                                                l.LeadNBFCSubActivityId,
                                                l.ActivityMasterId,
                                                l.SubActivityMasterId,
                                                l.SubActivitySequence,
                                                l.Code,
                                                l.ActivityName,
                                                l.SubActivityStatus
                                            } into grp
                                            select new LeadCompanyGenerateOfferSubactivityNewDTO
                                            {
                                                ActivityMasterId = grp.Key.ActivityMasterId,
                                                ActivityName = grp.Key.ActivityName,
                                                LeadNBFCSubActivityId = grp.Key.LeadNBFCSubActivityId,
                                                Sequence = grp.Key.SubActivitySequence,
                                                Status = grp.Key.SubActivityStatus,
                                                SubActivityMasterId = grp.Key.SubActivityMasterId,
                                                SubActivityName = grp.Key.Code,
                                                ApiList = grp.ToList().Select(x => new LeadCompanyGenerateOfferApiNewDTO
                                                {
                                                    ApiId = x.APIId,
                                                    ApiUrl = x.APIUrl,
                                                    Code = x.ApiCode,
                                                    Sequence = x.APISequence,
                                                    ApiStatus = x.ApiStatus,
                                                    Request = x.Request,
                                                    Response = x.Response,
                                                }).ToList(),
                                            };

                                    comp.SubactivityList = r.ToList();
                                    Loandetaillist.Add(comp);
                                }
                            }
                            else
                            {
                                if (offer.CreditLimit != null && offer.CreditLimit > 0)
                                {
                                    //var NBFCUpdate = _context.BusinessLoanNBFCUpdate.FirstOrDefault(x => x.LeadId == request.Request.LeadId && x.CompanyIdentificationCode == offer.CompanyIdentificationCode && x.IsActive && !x.IsDeleted);
                                    var NBFCUpdate = _context.nbfcOfferUpdate.FirstOrDefault(x => x.LeadId == request.Request.LeadId && x.CompanyIdentificationCode == offer.CompanyIdentificationCode && x.IsActive && !x.IsDeleted);

                                    var Anchorconfig = loanConfig != null && loanConfig.ProductSlabConfigs != null ? loanConfig.ProductSlabConfigs.Where(x => x.MinLoanAmount <= offer.CreditLimit && x.MaxLoanAmount >= offer.CreditLimit).ToList() : null;
                                    if (lead != null && Anchorconfig != null && Anchorconfig.Any())
                                    {
                                        var nbfcConfig = loanConfig; // request.Request.GetProductNBFCConfigResponseDcs.FirstOrDefault(x => x.CompanyId == offer.NBFCCompanyId);

                                        var roi = Anchorconfig.FirstOrDefault(x => x.SlabType == SlabTypeConstants.ROI);
                                        var pf = Anchorconfig.FirstOrDefault(x => x.SlabType == SlabTypeConstants.PF);

                                        LoanAmount = offer.CreditLimit ?? 0;
                                        //RateOfInterest = lead.InterestRate > 0 ? lead.InterestRate.Value : Anchorconfig.FirstOrDefault(x => x.SlabType == SlabTypeConstants.ROI).Value;
                                        RateOfInterest = NBFCUpdate != null ? NBFCUpdate.InterestRate ?? 0 : 0;
                                        Tenure = NBFCUpdate != null ? Convert.ToInt32(NBFCUpdate.Tenure) : 0;
                                        monthlyPayment = NBFCUpdate != null ? NBFCUpdate.MonthlyEMI ?? 0 : 0; //Math.Round(Convert.ToDouble(CalculateMonthlyPayment(RateOfInterest, Tenure, LoanAmount)), 2);
                                        loanIntAmt = Math.Round(((Convert.ToDouble(monthlyPayment) * Tenure) - LoanAmount), 2);
                                        PFPer = NBFCUpdate != null ? NBFCUpdate.ProcessingFeeRate ?? 0 : 0;
                                        GST = NBFCUpdate != null ? NBFCUpdate.GST ?? 0 : 0; //request.Request.GST;
                                        processing_fee = NBFCUpdate != null ? NBFCUpdate.ProcessingFeeAmount ?? 0 : 0; //pf.ValueType == "Percentage" ? Math.Round((LoanAmount * PFPer) / 100, 2) : pf.MaxValue;
                                        ProcessingfeeTax = NBFCUpdate != null ? NBFCUpdate.ProcessingFeeTax ?? 0 : 0; //Math.Round(Convert.ToDouble(processing_fee * GST) / 100, 2);
                                        ComapanyName = offer.CompanyIdentificationCode;
                                        PfDiscount = NBFCUpdate != null ? NBFCUpdate.PFDiscount ?? 0 : 0;
                                        PFAfterDiscount = processing_fee - PfDiscount;
                                        PFRevisedTax = NBFCUpdate.ReviseProcessingFeeTax;

                                        loan.CreditLimit = LoanAmount;
                                        loan.SanctonAmount = LoanAmount;
                                        loan.InterestRate = RateOfInterest;
                                        loan.Tenure = Tenure;
                                        loan.MonthlyEMI = monthlyPayment;
                                        loan.LoanInterestAmount = loanIntAmt;
                                        loan.Processingfee = processing_fee;
                                        loan.ProcessingfeeTax = ProcessingfeeTax;
                                        loan.ComapanyName = ComapanyName;
                                        loan.LeadOfferId = offer.Id;
                                        loan.NbfcCompanyId = offer.NBFCCompanyId;
                                        loan.LeadId = lead.Id;
                                        loan.LeadOfferStatus = offer.Status;
                                        loan.OfferApprove = (lead.OfferCompanyId != null && offer.NBFCCompanyId == lead.OfferCompanyId) ? true : false;
                                        loan.MinInterestRate = roi.MinValue;
                                        loan.MaxInterestRate = roi.MaxValue;
                                        loan.ProcessingfeeRate = PFPer;
                                        loan.GSTRate = GST;
                                        loan.PfDiscount = PfDiscount;
                                        loan.NbfcCreatedDate = NBFCUpdate != null ? NBFCUpdate.Created : null;
                                        loan.NbfcUserId = NBFCUpdate != null ? NBFCUpdate.CreatedBy : null;
                                        loan.PFType = pf.ValueType;
                                        loan.PFAfterDiscount = PFAfterDiscount;
                                        loan.PFRevisedTax = PFRevisedTax;
                                        loan.LoanId = NBFCUpdate != null ? NBFCUpdate.LoanId : null;
                                        loan.OfferInitiateDate = offer.Created;
                                        Loandetaillist.Add(loan);
                                    }
                                }
                                else
                                {
                                    loan.CreditLimit = 0;
                                    loan.SanctonAmount = 0;
                                    loan.InterestRate = 0;
                                    loan.Tenure = 0;
                                    loan.MonthlyEMI = 0;
                                    loan.LoanInterestAmount = 0;
                                    loan.Processingfee = 0;
                                    loan.ProcessingfeeTax = 0;
                                    loan.ComapanyName = offer.CompanyIdentificationCode;
                                    loan.MinInterestRate = 0;
                                    loan.MaxInterestRate = 0;
                                    loan.LeadOfferId = offer.Id;
                                    loan.NbfcCompanyId = offer.NBFCCompanyId;
                                    loan.LeadId = lead.Id;
                                    loan.LeadOfferStatus = offer.Status;
                                    loan.OfferApprove = false;
                                    loan.ProcessingfeeRate = 0;
                                    loan.GSTRate = 0;
                                    loan.PfDiscount = 0;
                                    loan.NbfcCreatedDate = null;
                                    loan.NbfcUserId = null;
                                    loan.PFType = "";
                                    loan.LoanId = "";
                                    loan.OfferInitiateDate = null;
                                    Loandetaillist.Add(loan);
                                }
                            }
                        }
                        if (request.Request.ProductType == ProductTypeConstants.CreditLine)
                        {
                            Loandetaillist = new List<LeadCompanyGenerateOfferNewDTO>();
                            if (offer.CompanyIdentificationCode == CompanyIdentificationCodeConstants.BlackSoil)
                            {
                                var BlackSoilPF = _context.BlackSoilPFCollections.FirstOrDefault(x => x.LeadId == request.Request.LeadId);
                                //if (BlackSoilPF != null)
                                //{
                                //    loan.Processingfee = BlackSoilPF.processing_fee;
                                //    loan.ProcessingfeeTax = BlackSoilPF.processing_fee_tax;
                                //    loan.OfferApprove = (lead.OfferCompanyId != null && offer.NBFCCompanyId == lead.OfferCompanyId) ? true : false;
                                //    ComapanyName = offer.CompanyIdentificationCode;
                                //    Loandetaillist.Add(loan);
                                //}

                                foreach (var comp in result)
                                {

                                    comp.Processingfee = BlackSoilPF != null ? BlackSoilPF.processing_fee : 0;
                                    comp.ProcessingfeeTax = BlackSoilPF != null ? BlackSoilPF.processing_fee_tax : 0;
                                    comp.OfferApprove = (lead.OfferCompanyId != null && offer.NBFCCompanyId == lead.OfferCompanyId) ? true : false;
                                    ComapanyName = offer.CompanyIdentificationCode;

                                    var r = from l in leadcompanygenerateoffer
                                            where l.ComapanyName == comp.ComapanyName
                                            group l by new
                                            {
                                                l.LeadNBFCSubActivityId,
                                                l.ActivityMasterId,
                                                l.SubActivityMasterId,
                                                l.SubActivitySequence,
                                                l.Code,
                                                l.ActivityName,
                                                l.SubActivityStatus
                                            } into grp
                                            select new LeadCompanyGenerateOfferSubactivityNewDTO
                                            {
                                                ActivityMasterId = grp.Key.ActivityMasterId,
                                                ActivityName = grp.Key.ActivityName,
                                                LeadNBFCSubActivityId = grp.Key.LeadNBFCSubActivityId,
                                                Sequence = grp.Key.SubActivitySequence,
                                                Status = grp.Key.SubActivityStatus,
                                                SubActivityMasterId = grp.Key.SubActivityMasterId,
                                                SubActivityName = grp.Key.Code,
                                                ApiList = grp.ToList().Select(x => new LeadCompanyGenerateOfferApiNewDTO
                                                {
                                                    ApiId = x.APIId,
                                                    ApiUrl = x.APIUrl,
                                                    Code = x.ApiCode,
                                                    Sequence = x.APISequence,
                                                    ApiStatus = x.ApiStatus,
                                                    Request = x.Request,
                                                    Response = x.Response,
                                                }).ToList(),
                                            };

                                    comp.SubactivityList = r.ToList();
                                    Loandetaillist.Add(comp);
                                }
                            }
                            if (offer.CompanyIdentificationCode == CompanyIdentificationCodeConstants.AyeFinanceSCF)
                            {
                                foreach (var comp in result)
                                {

                                    //comp.Processingfee = BlackSoilPF != null ? BlackSoilPF.processing_fee : 0;
                                    //comp.ProcessingfeeTax = BlackSoilPF != null ? BlackSoilPF.processing_fee_tax : 0;
                                    comp.OfferApprove = (lead.OfferCompanyId != null && offer.NBFCCompanyId == lead.OfferCompanyId) ? true : false;
                                    ComapanyName = offer.CompanyIdentificationCode;

                                    var r = from l in leadcompanygenerateoffer
                                            where l.ComapanyName == comp.ComapanyName
                                            group l by new
                                            {
                                                l.LeadNBFCSubActivityId,
                                                l.ActivityMasterId,
                                                l.SubActivityMasterId,
                                                l.SubActivitySequence,
                                                l.Code,
                                                l.ActivityName,
                                                l.SubActivityStatus
                                            } into grp
                                            select new LeadCompanyGenerateOfferSubactivityNewDTO
                                            {
                                                ActivityMasterId = grp.Key.ActivityMasterId,
                                                ActivityName = grp.Key.ActivityName,
                                                LeadNBFCSubActivityId = grp.Key.LeadNBFCSubActivityId,
                                                Sequence = grp.Key.SubActivitySequence,
                                                Status = grp.Key.SubActivityStatus,
                                                SubActivityMasterId = grp.Key.SubActivityMasterId,
                                                SubActivityName = grp.Key.Code,
                                                ApiList = grp.ToList().Select(x => new LeadCompanyGenerateOfferApiNewDTO
                                                {
                                                    ApiId = x.APIId,
                                                    ApiUrl = x.APIUrl,
                                                    Code = x.ApiCode,
                                                    Sequence = x.APISequence,
                                                    ApiStatus = x.ApiStatus,
                                                    Request = x.Request,
                                                    Response = x.Response,
                                                }).ToList(),
                                            };

                                    comp.SubactivityList = r.ToList();
                                    Loandetaillist.Add(comp);
                                }
                            }
                        }
                    }


                    foreach (var item in Loandetaillist)
                    {
                        //if (item.SubactivityList.Count == 0)
                        {
                            var subactivity = result.Where(x => x.ComapanyName == item.ComapanyName).ToList();
                            if (subactivity.Any())
                            {
                                var Activitylist = subactivity.SelectMany(x => x.SubactivityList).ToList();
                                item.SubactivityList = new List<LeadCompanyGenerateOfferSubactivityNewDTO>();
                                item.SubactivityList.AddRange(Activitylist);

                                item.LeadOfferErrorMessage = subactivity.FirstOrDefault().LeadOfferErrorMessage;
                                item.LeadOfferStatus = subactivity.FirstOrDefault().LeadOfferStatus;
                            }
                        }
                    }

                }
            }


            return Loandetaillist;

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
    }
}



