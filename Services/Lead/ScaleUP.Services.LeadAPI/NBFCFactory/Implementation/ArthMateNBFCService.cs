using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.ArthMate;
using System.Collections.Generic;
using static MassTransit.ValidationResultExtensions;
using System.Globalization;
using System.Drawing;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Services.LeadAPI.Migrations;
using System.IO;
using System.Net;
using System;
using ScaleUP.Services.LeadAPI.Helper;
using System.Linq;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Ocsp;
using iTextSharp.text;
using IdentityServer4.Models;
using System.Data;
using Microsoft.VisualBasic;
using OpenHtmlToPdf;
using Microsoft.Data.SqlClient;
using System.Transactions;
using OpenTelemetry.Trace;
using Org.BouncyCastle.Utilities.Collections;
using AutoMapper;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.LeadAPI.Constants;
using ScaleUP.Services.LeadAPI.Controllers.NBFC;
using Serilog;
using ILogger = Serilog.ILogger;
using iTextSharp.text.pdf.parser.clipper;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Services.LeadDTO.Constant;
using static IdentityServer4.Models.IdentityResources;
using System.Reflection.Metadata;
using System.Reflection;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using static iTextSharp.text.pdf.AcroFields;
using System.ServiceModel.Channels;
using ScaleUP.Services.LeadDTO.eSign;
using MediatR;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using MassTransit.Testing;
using MassTransit.RabbitMqTransport;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;


namespace ScaleUP.Services.LeadAPI.NBFCFactory.Implementation
{
    public class ArthMateNBFCService : ILeadNBFCService
    {
        private IHostEnvironment _hostingEnvironment;
        private readonly LeadApplicationDbContext _context;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly ArthMateNBFCHelper _ArthMateNBFCHelper;
        string LoanStatus = "kyc_data_approved";
        string sLoan_sanction_letter = "loan_sanction_letter";
        string sSigned_loan_sanction_letter = "agreement";//"signed_loan_sanction_letter";

        private readonly ILogger<ArthMateNBFCService> _logger;
        private readonly eSignKarzaHelper _eSignKarzaHelper;

        private readonly LeadHistoryManager _leadHistoryManager;
        private readonly IMassTransitService _massTransitService;

        public ArthMateNBFCService(LeadApplicationDbContext context, LeadNBFCSubActivityManager leadNBFCSubActivityManager
        , IHostEnvironment hostingEnvironment, ILogger<ArthMateNBFCService> logger, eSignKarzaHelper eSignKarzaHelper
        , LeadHistoryManager leadHistoryManager, IMassTransitService massTransitService)
        {
            _context = context;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _ArthMateNBFCHelper = new ArthMateNBFCHelper();
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _eSignKarzaHelper = eSignKarzaHelper;
            _leadHistoryManager = leadHistoryManager;
            _massTransitService = massTransitService;
        }

        public Task<ICreateLeadNBFCResponse> AadhaarUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BlackSoilCommonApplicationDetail(long leadid)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BusinessAddressUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BusinessUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public async Task<ICreateLeadNBFCResponse> GenerateOffer(long leadid, long nbfcCompanyId)
        {
            ICreateLeadNBFCResponse response = null;
            var subactivity = _leadNBFCSubActivityManager.GetSubactivityData(leadid, nbfcCompanyId, ActivityConstants.GenerateOffer);
            if (subactivity != null && subactivity.Any())
            {
                bool isSuccess = true;
                foreach (var item in subactivity)
                {
                    if (isSuccess)
                    {
                        switch (item.Code)
                        {
                            case SubActivityConstants.CreateLead:
                                response = await CreateLead(leadid, item.NBFCCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                            case SubActivityConstants.AScore:
                                response = await AScore(leadid, item.NBFCCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                            case SubActivityConstants.Ceplar:
                                response = await Ceplar(leadid, item.NBFCCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                            case SubActivityConstants.Offer:
                                response = await Offer(leadid, item.NBFCCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                        }

                    }

                }
            }

            return response;
        }

        private async Task<ICreateLeadNBFCResponse> CreateLead(long leadid, long nbfccompanyid, bool isTest = false)
        {
            ICreateLeadNBFCResponse response = null;
            string subactivityCode = SubActivityConstants.CreateLead;
            var apiList = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
            {
                Code = subactivityCode,
                LeadId = leadid,
                CompanyIdentificationCode = CompanyIdentificationCodeConstants.ArthMate
            });

            if (apiList != null && apiList.Any())
            {
                bool isSuccess = true;
                foreach (var api in apiList)
                {
                    if (isSuccess)
                    {
                        switch (api.Code)
                        {
                            case CompanyApiConstants.ArthmateCreateLead:
                                response = await ArthmateCreateLead(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;

                            case CompanyApiConstants.ArthmateLoanDocument:
                                response = await ArthmateLoanDocument(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;

                            case CompanyApiConstants.ArthmatePanValidate:
                                response = await ArthmatePanVarification(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                        }
                    }
                }
            }
            return response;
        }
        private async Task<ICreateLeadNBFCResponse> ArthmateCreateLead(LeadNBFCSubActivityDTO apis, long leadid, long NBFCCompanyId, bool isTest = false)
        {
            if (apis.Status == LeadNBFCApiConstants.Completed || apis.Status == LeadNBFCApiConstants.CompletedWithError)
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "api already completed/completed with error"
                };
            }
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse
            {
                Message = "Something Went Wrong"
            };
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == leadid && x.IsActive && !x.IsDeleted);
            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var leadBusinessDetails = await _context.BusinessDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            if (lead != null && leadPersonalDetails != null && leadBusinessDetails != null)
            {
                var partner_loan_app_id = lead.LeadCode + "-" + lead.MobileNo;
                var partner_borrower_id = lead.LeadCode + "/" + "00001";
                List<LeadPostdc> list = new List<LeadPostdc>();
                string address__ln1 = "";
                string address__ln2 = "";
                if (leadPersonalDetails.PermanentAddressLineOne.Length >= 10)
                {
                    address__ln1 = leadPersonalDetails.PermanentAddressLineOne;
                    address__ln2 = leadPersonalDetails.PermanentAddressLineTwo;
                }
                else
                {
                    address__ln1 = (string.IsNullOrEmpty(leadPersonalDetails.PermanentAddressLineOne) ? "" : leadPersonalDetails.PermanentAddressLineOne) + (string.IsNullOrEmpty(leadPersonalDetails.PermanentAddressLineTwo) ? "" : leadPersonalDetails.PermanentAddressLineTwo) + (string.IsNullOrEmpty(leadPersonalDetails.PermanentCityName) ? "" : leadPersonalDetails.PermanentCityName) + (string.IsNullOrEmpty(leadPersonalDetails.PermanentStateName) ? "" : leadPersonalDetails.PermanentStateName);
                    address__ln2 = ".";
                }
                LeadPostdc leadObj = new LeadPostdc
                {
                    partner_loan_app_id = partner_loan_app_id,
                    partner_borrower_id = partner_borrower_id,
                    first_name = leadPersonalDetails.FirstName + (string.IsNullOrEmpty(leadPersonalDetails.MiddleName) ? "" : " " + leadPersonalDetails.MiddleName),
                    middle_name = "",
                    last_name = string.IsNullOrEmpty(leadPersonalDetails.LastName) ? "." : leadPersonalDetails.LastName,
                    type_of_addr = "Current",
                    resi_addr_ln1 = leadPersonalDetails.CurrentAddressLineOne.Length >= 40 ? leadPersonalDetails.CurrentAddressLineOne.Substring(0, 39) : leadPersonalDetails.CurrentAddressLineOne,
                    resi_addr_ln2 = leadPersonalDetails.CurrentAddressLineTwo ?? "",
                    city = leadPersonalDetails.CurrentCityName,
                    state = leadPersonalDetails.CurrentStateName,
                    pincode = leadPersonalDetails.CurrentZipCode.ToString(),
                    per_addr_ln1 = address__ln1,// leadPersonalDetails.PermanentAddressLineOne,
                    per_addr_ln2 = address__ln2,//leadPersonalDetails.PermanentAddressLineTwo ?? "",
                    per_city = leadPersonalDetails.PermanentCityName,
                    per_state = leadPersonalDetails.PermanentStateName,
                    per_pincode = leadPersonalDetails.PermanentZipCode.ToString(),
                    appl_phone = leadPersonalDetails.MobileNo ?? "",
                    appl_pan = leadPersonalDetails.PanMaskNO,
                    email_id = leadPersonalDetails.EmailId,
                    aadhar_card_num = leadPersonalDetails.AadhaarMaskNO,
                    dob = leadPersonalDetails.DOB.ToString("yyyy-MM-dd"),
                    age = (currentDateTime.Year - leadPersonalDetails.DOB.Year).ToString(),
                    gender = (leadPersonalDetails.Gender.ToLower() == "m" || leadPersonalDetails.Gender.ToLower() == "male") ? "Male" : (leadPersonalDetails.Gender.ToLower() == "f" || leadPersonalDetails.Gender.ToLower() == "female") ? "Female" : "Others",
                    bus_pan = leadBusinessDetails.BusMaskPan ?? "",
                    bus_add_corr_line1 = leadBusinessDetails.AddressLineOne ?? "",
                    bus_add_corr_line2 = leadBusinessDetails.AddressLineTwo ?? "",
                    bus_add_corr_city = leadBusinessDetails.CityName,
                    bus_add_corr_state = leadBusinessDetails.StateName,
                    bus_add_corr_pincode = leadBusinessDetails.ZipCode.ToString(),
                    bus_add_per_line1 = leadBusinessDetails.AddressLineOne ?? "",
                    bus_add_per_line2 = leadBusinessDetails.AddressLineTwo ?? "",
                    bus_add_per_city = leadBusinessDetails.CityName,
                    bus_add_per_state = leadBusinessDetails.StateName,
                    bus_add_per_pincode = leadBusinessDetails.ZipCode > 0 ? leadBusinessDetails.ZipCode.ToString() : "",
                    residence_status = "Owned",//leadBusinessDetails.OwnershipType,
                    bureau_pull_consent = "Yes",
                    father_fname = leadPersonalDetails.FatherName,
                    father_lname = string.IsNullOrEmpty(leadPersonalDetails.FatherLastName) ? "." : leadPersonalDetails.FatherLastName,
                    bus_name = leadBusinessDetails.BusinessName,
                    doi = leadBusinessDetails.DOI.ToString("yyyy-MM-dd"),
                    bus_entity_type = leadBusinessDetails.BusEntityType
                };
                list.Add(leadObj);
                _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.GenerateLead(list, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.LeadNBFCApiId, leadid);
                _context.ArthMateCommonAPIRequestResponses.Add(Result);
                _context.SaveChanges();
                if (Result.IsSuccess)
                {
                    var res = JsonConvert.DeserializeObject<LeadResponseDc>(Result.Response);
                    if (res != null && res.data != null && res.data.preparedbiTmpl != null && res.data.preparedbiTmpl.Any())
                    {
                        _context.ArthMateUpdates.Add(new ArthMateUpdates
                        {
                            LeadId = leadid,
                            PartnerloanAppId = partner_loan_app_id,
                            PartnerborrowerId = partner_borrower_id,
                            LoanAppId = res.data.preparedbiTmpl.FirstOrDefault().loan_app_id,
                            borrowerId = res.data.preparedbiTmpl.FirstOrDefault().borrower_id,
                            Tenure = "36",
                            Created = currentDateTime,
                            LastModified = null,
                            IsActive = true,
                            IsDeleted = false,
                            NBFCCompanyId = NBFCCompanyId,
                            IsOfferRejected = false,
                        });
                        _context.SaveChanges();
                        response.Message = "Lead Generated Successfully";
                    }
                    _leadNBFCSubActivityManager.UpdateLeadMasterStatus(leadid, LeadBusinessLoanStatusConstants.Pending);
                }
                string status = Result.IsSuccess ? LeadNBFCApiConstants.Completed : LeadNBFCApiConstants.Error;

                _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, status, Result.Id);
                response.IsSuccess = Result.IsSuccess;
            }
            return response;
        }
        private async Task<ICreateLeadNBFCResponse> ArthmateLoanDocument(LeadNBFCSubActivityDTO apis, long leadid, long NBFCCompanyId, bool isTest = false)
        {
            if (apis.Status == LeadNBFCApiConstants.Completed || apis.Status == LeadNBFCApiConstants.CompletedWithError)
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "api already completed/completed with error"
                };
            }
            string baseBath = _hostingEnvironment.ContentRootPath;
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse { Message = "Something Went Wrong" };
            List<LoanDocumentPostDc> loanDocumentPostDcs = new List<LoanDocumentPostDc>();
            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var arthmateUpdateData = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var arthmateDocuments = await _context.ArthMateDocumentMaster.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
            if (leadPersonalDetails != null && arthmateUpdateData != null && arthmateDocuments != null && arthmateDocuments.Any())
            {
                if (!string.IsNullOrEmpty(leadPersonalDetails.AadharFrontImage))
                {
                    loanDocumentPostDcs.Add(new LoanDocumentPostDc
                    {
                        base64pdfencodedfile = await ConvertToBase64StringAsync(leadPersonalDetails.AadharFrontImage, "AadharFrontImage"),
                        file_url = "",
                        FrontUrl = "",
                        LeadMasterId = leadid,
                        loan_app_id = arthmateUpdateData.LoanAppId,
                        partner_borrower_id = arthmateUpdateData.PartnerborrowerId,
                        partner_loan_app_id = arthmateUpdateData.PartnerloanAppId,
                        borrower_id = arthmateUpdateData.borrowerId,
                        PdfPassword = "",
                        code = arthmateDocuments.FirstOrDefault(x => x.DocumentName == "aadhar_card").DocumentTypeCode//"099"
                    });
                }
                if (!string.IsNullOrEmpty(leadPersonalDetails.AadharBackImage))
                {
                    loanDocumentPostDcs.Add(new LoanDocumentPostDc
                    {
                        base64pdfencodedfile = await ConvertToBase64StringAsync(leadPersonalDetails.AadharBackImage, "AadharBackImage"),
                        file_url = "",
                        FrontUrl = "",
                        LeadMasterId = leadid,
                        loan_app_id = arthmateUpdateData.LoanAppId,
                        partner_borrower_id = arthmateUpdateData.PartnerborrowerId,
                        partner_loan_app_id = arthmateUpdateData.PartnerloanAppId,
                        borrower_id = arthmateUpdateData.borrowerId,
                        PdfPassword = "",
                        code = arthmateDocuments.FirstOrDefault(x => x.DocumentName == "aadhaar_card_back").DocumentTypeCode//"115"
                    });
                }
                if (!string.IsNullOrEmpty(leadPersonalDetails.PanFrontImage))
                {

                    loanDocumentPostDcs.Add(new LoanDocumentPostDc
                    {
                        base64pdfencodedfile = await ConvertToBase64StringAsync(leadPersonalDetails.PanFrontImage, "PanFrontImage"),
                        file_url = "",
                        FrontUrl = "",
                        LeadMasterId = leadid,
                        loan_app_id = arthmateUpdateData.LoanAppId,
                        partner_borrower_id = arthmateUpdateData.PartnerborrowerId,
                        partner_loan_app_id = arthmateUpdateData.PartnerloanAppId,
                        borrower_id = arthmateUpdateData.borrowerId,
                        PdfPassword = "",
                        code = arthmateDocuments.FirstOrDefault(x => x.DocumentName == "pan_card").DocumentTypeCode//"005"
                    });
                }
                if (!string.IsNullOrEmpty(leadPersonalDetails.SelfieImageUrl))
                {

                    loanDocumentPostDcs.Add(new LoanDocumentPostDc
                    {
                        base64pdfencodedfile = await ConvertToBase64StringAsync(leadPersonalDetails.SelfieImageUrl, "Selfie"),
                        file_url = "",
                        FrontUrl = "",
                        LeadMasterId = leadid,
                        loan_app_id = arthmateUpdateData.LoanAppId,
                        partner_borrower_id = arthmateUpdateData.PartnerborrowerId,
                        partner_loan_app_id = arthmateUpdateData.PartnerloanAppId,
                        borrower_id = arthmateUpdateData.borrowerId,
                        PdfPassword = "",
                        code = arthmateDocuments.FirstOrDefault(x => x.DocumentName == "selfie").DocumentTypeCode//"003"
                    });
                }
                var leadDocuments = await _context.LeadDocumentDetails.Where(x => x.LeadId == leadid && !string.IsNullOrEmpty(x.FileUrl)
                && (x.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate || x.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar
                || x.DocumentName == BlackSoilBusinessDocNameConstants.Statement)
                && x.IsActive && !x.IsDeleted).ToListAsync();
                if (leadDocuments != null && leadDocuments.Any())
                {
                    foreach (var doc in leadDocuments)
                    {
                        string code = "";
                        bool IsBankStatement = false;
                        if (doc.DocumentName == BlackSoilBusinessDocNameConstants.GstCertificate)
                            code = arthmateDocuments.FirstOrDefault(x => x.DocumentName == "buss_gst_certifi").DocumentTypeCode;
                        else if (doc.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar)
                            code = arthmateDocuments.FirstOrDefault(x => x.DocumentName == "udyam_reg_cert").DocumentTypeCode;
                        else if (doc.DocumentName == BlackSoilBusinessDocNameConstants.Statement)
                        {
                            IsBankStatement = true;
                            code = arthmateDocuments.FirstOrDefault(x => x.DocumentName == "bank_stmnts").DocumentTypeCode;
                        }

                        loanDocumentPostDcs.Add(new LoanDocumentPostDc
                        {
                            base64pdfencodedfile = await ConvertToBase64StringAsync(doc.FileUrl, "bank"),
                            file_url = "",
                            FrontUrl = "",
                            LeadMasterId = leadid,
                            loan_app_id = arthmateUpdateData.LoanAppId,
                            partner_borrower_id = arthmateUpdateData.PartnerborrowerId,
                            partner_loan_app_id = arthmateUpdateData.PartnerloanAppId,
                            borrower_id = arthmateUpdateData.borrowerId,
                            PdfPassword = "",
                            code = code,
                            IsBankStatement = IsBankStatement
                        });
                    }
                }
            }
            _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
            _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);
            if (loanDocumentPostDcs.Any())
            {
                foreach (var loanDocument in loanDocumentPostDcs)
                {
                    ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.LoanDocumentApi(loanDocument, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.LeadNBFCApiId, leadid, baseBath);
                    _context.ArthMateCommonAPIRequestResponses.Add(Result);
                    response.IsSuccess = Result.IsSuccess;
                    _context.SaveChanges();
                    if (!response.IsSuccess) break;
                }
            }

            string status = response.IsSuccess ? LeadNBFCApiConstants.Completed : LeadNBFCApiConstants.Error;
            _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, status);
            return response;
        }
        private async Task<ICreateLeadNBFCResponse> AScore(long leadid, long nbfccompanyid, bool isTest = false)
        {
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();

            string subactivityCode = SubActivityConstants.AScore;
            var apiList = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
            {
                Code = subactivityCode,
                LeadId = leadid,
                CompanyIdentificationCode = CompanyIdentificationCodeConstants.ArthMate
            });

            if (apiList != null && apiList.Any())
            {
                bool isSuccess = true;
                foreach (var api in apiList)
                {
                    if (isSuccess)
                    {
                        switch (api.Code)
                        {
                            case CompanyApiConstants.ArthmateRequestAScore:
                                response = await ArthmateRequestAScore(api, leadid, nbfccompanyid, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                        }
                        response.IsSuccess = isSuccess;
                    }
                }
            }

            return response;
        }
        public async Task<ICreateLeadNBFCResponse> ArthmateRequestAScore(LeadNBFCSubActivityDTO apis, long leadid, long nbfccompanyid, bool isTest = false)
        {
            if (apis.Status == LeadNBFCApiConstants.Completed || apis.Status == LeadNBFCApiConstants.CompletedWithError)
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "api already completed/completed with error"
                };
            }
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse
            {
                Message = "Something Went Wrong"
            };

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
            var leadBusinessDetails = await _context.BusinessDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            if (leadBusinessDetails != null && leadBusinessDetails.InquiryAmount == 0)
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "InquiryAmount amount should be greater than 0"
                };
            }
            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var arthmateUpdateData = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);

            if (leadPersonalDetails != null && arthmateUpdateData != null && leadBusinessDetails != null)
            {
                int stateCodeValue = _context.ArthMateStateCode.Where(x => x.State == leadPersonalDetails.PermanentStateName && x.IsActive && !x.IsDeleted).Select(x => x.StateCode).FirstOrDefault();
                if (stateCodeValue > 0)
                {
                    string stateCode = stateCodeValue.ToString("00");
                    AScoreAPIRequest ascore = new AScoreAPIRequest
                    {
                        first_name = leadPersonalDetails.FirstName,
                        last_name = leadPersonalDetails.LastName ?? "",
                        dob = leadPersonalDetails.DOB.ToString("yyyy-MM-dd"),
                        pan = leadPersonalDetails.PanMaskNO,
                        gender = leadPersonalDetails.Gender,
                        mobile_number = leadPersonalDetails.MobileNo ?? "",
                        address = leadPersonalDetails.PermanentAddressLineOne.Length > 40 ? leadPersonalDetails.PermanentAddressLineOne.Substring(0, 39) : leadPersonalDetails.PermanentAddressLineOne,
                        city = leadPersonalDetails.PermanentCityName,
                        pin_code = leadPersonalDetails.PermanentZipCode.ToString(),
                        state_code = stateCode,
                        enquiry_stage = "PRE-SCREEN",
                        enquiry_purpose = "61",
                        enquiry_amount = leadBusinessDetails.InquiryAmount > 0 ? leadBusinessDetails.InquiryAmount.ToString() : "",
                        en_acc_account_number_1 = arthmateUpdateData.LoanAppId,
                        bureau_type = "cibil",
                        tenure = Convert.ToInt32(arthmateUpdateData.Tenure),
                        loan_app_id = arthmateUpdateData.LoanAppId,
                        consent = "Y",
                        product_type = "UBLN",
                        consent_timestamp = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                    ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.AscoreApi(ascore, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.LeadNBFCApiId, leadid);

                    _context.ArthMateCommonAPIRequestResponses.Add(Result);
                    _context.SaveChanges();
                    if (Result.IsSuccess)
                    {
                        var resdata = JsonConvert.DeserializeObject<AScoreAPIResponse>(Result.Response);
                        var arthMateUpdate = _context.ArthMateUpdates.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefault();

                        if (resdata != null && arthMateUpdate != null)
                        {
                            arthMateUpdate.AScoreRequestId = resdata.request_id;
                            arthmateUpdateData.AScoreCreatedDate = currentDateTime;
                            arthMateUpdate.LastModified = currentDateTime;
                            _context.Entry(arthMateUpdate).State = EntityState.Modified;
                            response.Message = "Ascore Generated";
                        }
                        _context.SaveChanges();
                    }
                    response.IsSuccess = Result.IsSuccess;
                }
                string status = response.IsSuccess ? LeadNBFCApiConstants.Completed : LeadNBFCApiConstants.Error;
                _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, status);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, status);
            }
            return response;
        }
        private async Task<ICreateLeadNBFCResponse> Ceplar(long leadid, long NBFCCompanyId, bool isTest = false)
        {
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();

            string subactivityCode = SubActivityConstants.Ceplar;
            var apiList = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
            {
                Code = subactivityCode,
                LeadId = leadid,
                CompanyIdentificationCode = CompanyIdentificationCodeConstants.ArthMate
            });

            if (apiList != null && apiList.Any())
            {
                bool isSuccess = true;
                foreach (var api in apiList)
                {
                    if (isSuccess)
                    {
                        switch (api.Code)
                        {
                            case CompanyApiConstants.ArthmateCeplr:
                                response = await ArthmateCeplr(api, leadid, NBFCCompanyId, isTest);
                                isSuccess = response.IsSuccess;
                                break;
                        }
                        response.IsSuccess = isSuccess;
                    }
                }
            }
            return response;
        }
        private async Task<ICreateLeadNBFCResponse> ArthmatePanVarification(LeadNBFCSubActivityDTO apis, long leadid, long NBFCCompanyId, bool isTest = false)
        {
            if (apis.Status == LeadNBFCApiConstants.Completed || apis.Status == LeadNBFCApiConstants.CompletedWithError)
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "api already completed/completed with error"
                };
            }
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var arthmateUpdateData = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            if (leadPersonalDetails != null && arthmateUpdateData != null)
            {
                PanVerificationRequestJsonV3 request = new PanVerificationRequestJsonV3
                {
                    name = leadPersonalDetails.PanNameOnCard,
                    father_name = leadPersonalDetails.FatherName,
                    dob = leadPersonalDetails.DOB.ToString("yyyy-MM-dd"), //"1990-07-15"
                    loan_app_id = arthmateUpdateData.LoanAppId,
                    pan = leadPersonalDetails.PanMaskNO,
                    consent = "Y",
                    consent_timestamp = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                };
                _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.PanVerificationAsync(request, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.LeadNBFCApiId, leadid);

                _context.ArthMateCommonAPIRequestResponses.Add(Result);
                _context.SaveChanges();
                if (Result.IsSuccess)
                {
                    var PanRes = JsonConvert.DeserializeObject<PanValidationRspnsV3>(Result.Response);
                    if (PanRes != null && PanRes.data != null)
                    {
                        var documentId = _context.ArthMateDocumentMaster.FirstOrDefault(x => x.DocumentName == "pan_card").Id;
                        _context.KYCValidationResponse.Add(new KYCValidationResponse
                        {
                            LeadMasterId = leadid,
                            DocumentMasterId = Convert.ToInt32(documentId),
                            kyc_id = PanRes.kyc_id,
                            Status = PanRes.success == true ? "success" : "failed",
                            ResponseJson = Result.Response,
                            Message = PanRes.data.msg,
                            Created = currentDateTime,
                            LastModified = null,
                            IsActive = true,
                            IsDeleted = false,
                            IsKycVerified = false,
                            Remark = "pan_card"
                        });
                    }
                }
                else
                {
                    response.Message = "PAN verification failed!!!";
                }
                _context.SaveChanges();

                response.IsSuccess = Result.IsSuccess;

                string status = response.IsSuccess ? LeadNBFCApiConstants.Completed : LeadNBFCApiConstants.Error;
                _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, status, Result.Id);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, status);
            }

            return response;
        }
        private async Task<ICreateLeadNBFCResponse> ArthmateCeplr(LeadNBFCSubActivityDTO apis, long leadid, long NBFCCompanyId, bool isTest = false)
        {
            if (apis.Status == LeadNBFCApiConstants.Completed || apis.Status == LeadNBFCApiConstants.CompletedWithError)
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "api already completed/completed with error"
                };
            }
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse { Message = "Something Went Wrong" };
            var arthmateUpdateData = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            if (arthmateUpdateData != null)
            {
                TimeSpan span = currentDateTime.Subtract((DateTime)arthmateUpdateData.AScoreCreatedDate);
                if (!arthmateUpdateData.IsAScoreWebhookHit && span.TotalMinutes <= 5)
                {
                    response.Message = $"Please wait for {Math.Round(5 - span.TotalMinutes, 2)} minutes";
                    response.IsSuccess = false;
                    return response;
                }
            }

            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var leadBankDetails = await _context.LeadBankDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.Type == BankTypeConstant.Borrower && x.IsActive && !x.IsDeleted);
            var tokenDetails = await _context.
                NBFCApiTokens.Where(x => x.IdentificationCode == CompanyIdentificationCodeConstants.ArthMate && x.IsActive).ToListAsync();
            if (leadPersonalDetails != null && leadBankDetails != null && tokenDetails != null && tokenDetails.Any())
            {

                string fip_id = "";
                var Bankdata = _context.CeplrBankList.Where(x => x.fip_name == leadBankDetails.BankName).FirstOrDefault();
                if (Bankdata != null)
                {
                    fip_id = !string.IsNullOrEmpty(Bankdata.pdf_fip_id.ToString()) ? Bankdata.pdf_fip_id.ToString() : null;
                }

                CeplrPdfReportDc pdfReportDc = new CeplrPdfReportDc
                {
                    callback_url = tokenDetails.FirstOrDefault(x => x.TokenType == NBFCApiTokenTypeConstants.CallbackURL).TokenValue,
                    configuration_uuid = tokenDetails.FirstOrDefault(x => x.TokenType == NBFCApiTokenTypeConstants.configuration_uuid).TokenValue,
                    email = leadPersonalDetails.EmailId,
                    file = "",
                    file_password = "",
                    fip_id = fip_id,
                    ifsc_code = leadBankDetails.IFSCCode,
                    mobile = leadPersonalDetails.MobileNo ?? "",
                    name = leadPersonalDetails.FirstName + leadPersonalDetails.LastName
                };

                _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);

                apis.TAPISecretKey = tokenDetails.FirstOrDefault(x => x.TokenType == NBFCApiTokenTypeConstants.CeplarKey).TokenValue;
                var Result = await CeplrPdfReports(pdfReportDc, apis, leadid, apis.LeadNBFCApiId);

                var arthMateUpdate = _context.ArthMateUpdates.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefault();
                if (Result != null && Result.data != null && arthMateUpdate != null)
                {
                    arthMateUpdate.CeplerCustomerId = Result.data.customer_id;
                    arthMateUpdate.CeplerRequestId = Result.data.request_id.ToString();
                    _context.Entry(arthMateUpdate).State = EntityState.Modified;
                    response.IsSuccess = true;
                    response.Message = "Cepler Completed";

                    CeplrPdfReports pdfinsert = new CeplrPdfReports();
                    pdfinsert.LeadMasterId = leadid;
                    pdfinsert.FileName = pdfReportDc.file ?? "";
                    pdfinsert.fip_id = pdfReportDc.fip_id ?? "";
                    pdfinsert.callback_url = pdfReportDc.callback_url ?? "";
                    pdfinsert.file_password = pdfReportDc.file_password ?? "";
                    pdfinsert.customer_id = Result.data.customer_id ?? "";
                    pdfinsert.request_id = Result.data.request_id;
                    pdfinsert.ApiRequest_id = 0;
                    pdfinsert.ApiToken = pdfReportDc.fip_id ?? "";
                    pdfinsert.IsActive = true;
                    pdfinsert.IsDeleted = false;
                    _context.CeplrPdfReports.Add(pdfinsert);
                }

                _context.SaveChanges();

                string status = response.IsSuccess ? LeadNBFCApiConstants.Completed : LeadNBFCApiConstants.Error;

                _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, status);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, status);
                response.Message = status;
            }
            return response;
        }
        public async Task<PdfResDcCeplr> CeplrPdfReports(CeplrPdfReportDc pdfReportDc, LeadNBFCSubActivityDTO apis, long Leadid, long LeadNBFCApiId, bool isTest = false)
        {

            PdfResDcCeplr pdfResDcCeplr = new PdfResDcCeplr();
            List<CeplrPostDc> postlist = new List<CeplrPostDc>();

            try
            {
                int BankFileCount = 0;
                {
                    string baseBath = _hostingEnvironment.ContentRootPath;
                    string FilePath = "";
                    var MultipleBankFile = await _context.LeadDocumentDetails.Where(x => x.LeadId == Leadid && x.DocumentType == BlackSoilBusinessDocTypeConstants.IdProof
                                            && x.DocumentName == BlackSoilBusinessDocNameConstants.Statement && x.IsActive && !x.IsDeleted).ToListAsync();
                    if (MultipleBankFile != null && MultipleBankFile.Any())
                    {
                        BankFileCount = MultipleBankFile.Count();



                        foreach (var file in MultipleBankFile)
                        {
                            _logger.LogWarning("CeplrPdfReports : file");
                            string filename = Path.GetFileName(file.FileUrl);
                            byte[] data = null;
                            data = FileSaverHelper.GetBytecodeFromUrl(file.FileUrl);
                            FilePath = Path.Combine(baseBath, "wwwroot", "OtherDoc");
                            var returnPath = Path.Combine(EnvironmentConstants.LeadServiceBaseURL, "OtherDoc", filename);
                            if (!Directory.Exists(FilePath))
                            {
                                Directory.CreateDirectory(FilePath);
                            }
                            _logger.LogWarning("CeplrPdfReports : file 2");
                            FilePath = Path.Combine(FilePath, filename);
                            File.WriteAllBytes(FilePath, data);

                            _logger.LogWarning("CeplrPdfReports : file" + FilePath);

                            pdfReportDc.file = FilePath;
                            postlist.Add(new CeplrPostDc
                            {
                                callback_url = pdfReportDc.callback_url,
                                configuration_uuid = pdfReportDc.configuration_uuid,
                                email = pdfReportDc.email,
                                file_password = file.PdfPassword ?? "",
                                name = pdfReportDc.name,
                                filepath = pdfReportDc.file,
                                ifsc_code = pdfReportDc.ifsc_code,
                                fip_id = pdfReportDc.fip_id,
                                mobile = pdfReportDc.mobile,
                                Id = file.Id
                            });
                        }
                    }

                    var RequestjsonString = JsonConvert.SerializeObject(postlist);


                    int count = 1;
                    string token = "";
                    int request_id = 0;
                    foreach (var item in postlist.OrderByDescending(x => x.Id))
                    {
                        if (postlist.Count > 1 && count == BankFileCount)
                        {
                            item.last_file = true;
                        }
                        item.allow_multiple = BankFileCount == 1 ? false : true;
                        if (request_id > 0 && item.allow_multiple)
                        {
                            item.token = token;
                            item.request_id = request_id;
                        }

                        string jsonString = "";
                        var response = await _ArthMateNBFCHelper.PostCplr(item, apis.TAPISecretKey, apis.APIUrl, LeadNBFCApiId, Leadid);

                        _context.ArthMateCommonAPIRequestResponses.Add(response);
                        _context.SaveChanges();

                        if (response != null && response.IsSuccess && !string.IsNullOrEmpty(response.Response))
                        {
                            System.IO.File.Delete(FilePath);

                            pdfResDcCeplr = JsonConvert.DeserializeObject<PdfResDcCeplr>(response.Response);

                            if (postlist.Count > 0)
                            {

                                if (pdfResDcCeplr != null && pdfResDcCeplr.data != null)
                                {
                                    if (count == 1)
                                    {
                                        var ceplerdata = _context.CeplrPdfReports.Where(x => x.LeadMasterId == Leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                        if (ceplerdata != null)
                                        {
                                            ceplerdata.ApiRequest_id = pdfResDcCeplr.data.request_id;
                                            ceplerdata.ApiToken = pdfResDcCeplr.data.token ?? "";
                                            _context.Entry(ceplerdata).State = EntityState.Modified;
                                            _context.SaveChanges();
                                            request_id = pdfResDcCeplr.data.request_id;
                                            token = pdfResDcCeplr.data.token;
                                        }
                                    }


                                }
                            }

                        }

                        count++;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("ceplr file catch block:" + ex.ToString());
            }

            return pdfResDcCeplr;
        }

        private async Task<ICreateLeadNBFCResponse> Offer(long leadid, long NBFCCompanyId, bool isTest = false)
        {
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();

            string subactivityCode = SubActivityConstants.Offer;
            var apiList = await _leadNBFCSubActivityManager.GetLeadNBFCSubActivity(new LeadNBFCSubActivityRequest
            {
                Code = subactivityCode,
                LeadId = leadid,
                CompanyIdentificationCode = CompanyIdentificationCodeConstants.ArthMate
            });

            if (apiList != null && apiList.Any())
            {
                bool isSuccess = true;
                foreach (var api in apiList)
                {
                    if (isSuccess)
                    {
                        switch (api.Code)
                        {
                            //Colender
                            case CompanyApiConstants.ArthmateGenerateOffer:
                                response = await ArthmateGenerateOffer(api, leadid, NBFCCompanyId);
                                isSuccess = response.IsSuccess;
                                break;
                        }
                        response.IsSuccess = isSuccess;
                    }
                }
            }
            return response;
        }
        //Colender
        public async Task<ICreateLeadNBFCResponse> ArthmateGenerateOffer(LeadNBFCSubActivityDTO apis, long leadid, long NBFCCompanyId, int tenure = 0, double sactionAmount = 0)
        {
            bool IsRegenerate = (tenure > 0 && sactionAmount > 0) ? true : false;
            if (!IsRegenerate && (apis.Status == LeadNBFCApiConstants.Completed || apis.Status == LeadNBFCApiConstants.CompletedWithError))
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = true,
                    Message = "api already completed/completed with error"
                };
            };


            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse { Message = "Something Went Wrong" };
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var leadBusinessDetails = await _context.BusinessDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var arthmateupdate = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);

            if (!IsRegenerate)
            {
                _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
                _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);
            }
            double MonthlySalary = await GetMonthlySalary(leadid);
            if (MonthlySalary == 0)
            {
                return new CreateLeadNBFCResponse
                {
                    IsSuccess = false,
                    Message = "Monthly Salary is 0 by Third Party API"
                };
            }
            if (leadPersonalDetails != null && leadBusinessDetails != null && arthmateupdate != null && arthmateupdate != null)
            {
                CoLenderRequest colender = new CoLenderRequest
                {
                    first_name = leadPersonalDetails.FirstName,
                    last_name = leadPersonalDetails.LastName ?? "",
                    dob = leadPersonalDetails.DOB.ToString("yyyy-MM-dd"),
                    appl_pan = leadPersonalDetails.PanMaskNO,
                    gender = leadPersonalDetails.Gender == "M" ? "Male" : leadPersonalDetails.Gender == "F" ? "Female" : "Other",
                    appl_phone = leadPersonalDetails.MobileNo ?? "",
                    address = leadPersonalDetails.PermanentAddressLineOne.Length > 40 ? leadPersonalDetails.PermanentAddressLineOne.Substring(0, 39) : leadPersonalDetails.PermanentAddressLineOne,
                    city = leadPersonalDetails.PermanentCityName,
                    state = leadPersonalDetails.PermanentStateName,
                    pincode = leadPersonalDetails.PermanentZipCode.ToString(),
                    enquiry_purpose = "61",
                    bureau_type = "cibil",
                    tenure = IsRegenerate ? tenure : Convert.ToInt32(arthmateupdate.Tenure),
                    request_id_a_score = arthmateupdate.AScoreRequestId ?? "",
                    request_id_b_score = "",
                    ceplr_cust_id = arthmateupdate.CeplerCustomerId ?? "",
                    interest_rate = "",
                    product_type_code = "UBLN",
                    sanction_amount = IsRegenerate ? sactionAmount : leadBusinessDetails.InquiryAmount ?? 0,
                    dscr = 0,
                    monthly_income = MonthlySalary,
                    loan_app_id = arthmateupdate.LoanAppId,
                    consent = "Y",
                    consent_timestamp = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                };

                ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.CoLenderApi(colender, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.LeadNBFCApiId, leadid);
                _context.ArthMateCommonAPIRequestResponses.Add(Result);
                _context.SaveChanges();
                if (Result.IsSuccess)
                {
                    var colenderRes = JsonConvert.DeserializeObject<CoLenderResponseDc>(Result.Response);

                    if (colenderRes != null && colenderRes.status == "success")
                    {
                        if (Convert.ToDouble(colenderRes.loan_amount) == 0 && IsRegenerate)
                        {
                            response.IsSuccess = false;
                            response.Message = "Unfortunately, we do not have any loan offers available for the new tenure at this time. Please proceed with previous loan offer.";
                        }
                        else
                        {
                            if (Convert.ToDouble(colenderRes.loan_amount) > 0)
                            {
                                response.Message = "Offer Generated.";
                                response.IsSuccess = true;
                                var leadoffer = await _context.LeadOffers.Where(x => x.IsActive && x.LeadId == leadid && x.NBFCCompanyId == NBFCCompanyId && !x.IsDeleted).FirstOrDefaultAsync();
                                if (leadoffer != null)
                                {
                                    leadoffer.Status = LeadOfferConstant.OfferGenerated;
                                    leadoffer.CreditLimit = Convert.ToDouble(colenderRes.loan_amount);
                                    _context.Entry(leadoffer).State = EntityState.Modified;
                                }
                            }
                            var colenderResponse = await _context.CoLenderResponse.Where(x => x.LeadMasterId == leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                            if (colenderResponse != null)
                            {
                                colenderResponse.IsActive = false;
                                colenderResponse.IsDeleted = true;
                                _context.Entry(colenderResponse).State = EntityState.Modified;

                            }
                            _context.CoLenderResponse.Add(new CoLenderResponse
                            {
                                LeadMasterId = leadid,
                                request_id = colenderRes.request_id,
                                loan_amount = Convert.ToDouble(colenderRes.loan_amount),
                                pricing = Convert.ToDouble(colenderRes.pricing),
                                co_lender_shortcode = colenderRes.co_lender_shortcode != null ? colenderRes.co_lender_shortcode : "",
                                loan_app_id = colenderRes.loan_app_id,
                                co_lender_assignment_id = colenderRes.co_lender_assignment_id != null ? colenderRes.co_lender_assignment_id : 0,
                                co_lender_full_name = colenderRes.co_lender_full_name != null ? colenderRes.co_lender_full_name : "",
                                status = colenderRes.status != null ? colenderRes.status : "",
                                AScoreRequest_id = arthmateupdate != null ? arthmateupdate.AScoreRequestId : "",
                                ceplr_cust_id = arthmateupdate != null ? arthmateupdate.CeplerCustomerId : "",
                                SanctionAmount = IsRegenerate ? sactionAmount : Convert.ToDouble(colenderRes.loan_amount), //leadBusinessDetails.InquiryAmount ?? 0,
                                Created = currentDateTime,
                                LastModified = null,
                                IsActive = true,
                                IsDeleted = false,

                                ProgramType = (!string.IsNullOrEmpty(colenderRes.program_type) && colenderRes.program_type == "Transaction_POS") ? "Transactions" : colenderRes.program_type ?? "",
                            });

                            if (IsRegenerate)
                            {
                                leadBusinessDetails.InquiryAmount = sactionAmount;
                                _context.Entry(leadBusinessDetails).State = EntityState.Modified;
                                arthmateupdate.Tenure = tenure.ToString();
                            }
                            arthmateupdate.ColenderAssignmentId = colenderRes.co_lender_assignment_id != null ? colenderRes.co_lender_assignment_id : 0;
                            arthmateupdate.ColenderCreatedDate = currentDateTime;
                            arthmateupdate.ColenderLoanAmount = Convert.ToDouble(colenderRes.loan_amount);
                            arthmateupdate.ColenderPricing = Convert.ToDouble(colenderRes.pricing);
                            arthmateupdate.ColenderProgramType = (!string.IsNullOrEmpty(colenderRes.program_type) && colenderRes.program_type == "Transaction_POS") ? "Transactions" : colenderRes.program_type ?? "";
                            arthmateupdate.ColenderRequestId = colenderRes.request_id;
                            arthmateupdate.ColenderStatus = colenderRes.status != null ? colenderRes.status : "";

                            _context.Entry(arthmateupdate).State = EntityState.Modified;

                            _context.SaveChanges();
                        }
                    }
                }
                if (!IsRegenerate)
                {
                    string status = response.IsSuccess ? LeadNBFCApiConstants.Completed : LeadNBFCApiConstants.Error;

                    if (response.IsSuccess)
                    {
                        _leadNBFCSubActivityManager.UpdateLeadMasterStatus(leadid, LeadBusinessLoanStatusConstants.LoanApproved);
                        //_leadNBFCSubActivityManager.UpdateLeadProgressStatus(leadid, ActivityConstants.Inprogress, true);
                    }
                    else
                    {
                        _leadNBFCSubActivityManager.UpdateLeadMasterStatus(leadid, LeadBusinessLoanStatusConstants.LoanRejected);
                    }
                    _leadNBFCSubActivityManager.UpdateStatus(apis.LeadNBFCApiId, status, Result.Id);
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(apis.LeadNBFCSubActivityId, status);
                }
            }
            return response;
        }
        public async Task<ICreateLeadNBFCResponse> LoanGenerate(LeadNBFCSubActivityDTO apis, bool insurance_applied, long leadid, long NBFCCompanyId, double sanctionAmount, ProductCompanyConfigDc loanConfig, bool isTest = false)
        {
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();
            List<LoanApiRequestDc> loanlistobj = new List<LoanApiRequestDc>();

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            var lead = await _context.Leads.Where(x => x.Id == leadid && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            var arthmateupdate = await _context.ArthMateUpdates.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            var leadBankDetails = await _context.LeadBankDetails.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).ToListAsync();
            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var leadBusinessDetails = await _context.BusinessDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var leadCoLenderResponse = await _context.CoLenderResponse.Where(x => x.LeadMasterId == leadid && x.IsActive && !x.IsDeleted).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            var leadUdyogAadhaar = await _context.LeadDocumentDetails.Where(x => x.LeadId == leadid && !string.IsNullOrEmpty(x.FileUrl)
                                    && x.DocumentName == BlackSoilBusinessDocNameConstants.UdyogAadhaar && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            //var loanConfig = await _context.LoanConfiguration.Where(x => x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();



            if (lead != null && arthmateupdate != null && leadBankDetails != null && leadBankDetails.Any() && leadPersonalDetails != null && leadBusinessDetails != null && leadUdyogAadhaar != null && leadCoLenderResponse != null && loanConfig != null)
            {
                var borroBankdetail = leadBankDetails.FirstOrDefault(x => x.Type == BankTypeConstant.Borrower);
                var BeneBankdetail = leadBankDetails.FirstOrDefault(x => x.Type == BankTypeConstant.Beneficiary);

                leadCoLenderResponse.SanctionAmount = sanctionAmount; //leadCoLenderResponse.loan_amount;
                double ProcessingFeePer = loanConfig.PF;// 2; 
                double GstONPfPer = loanConfig.GST;// 18;
                double loan_int_rate = lead.InterestRate ?? 0;//loanConfig.InterestRate;//24;
                int NumberOfApplicants = 1;
                double CalProcessingFeeAmt = 0; double Calc_insurance_amount = 0;
                CalProcessingFeeAmt = Convert.ToDouble(leadCoLenderResponse.SanctionAmount) * (ProcessingFeePer / 100);
                double CalGstONPfAmt = Math.Round(CalProcessingFeeAmt * (GstONPfPer / 100), 2);
                int Loan_Tenure = Convert.ToInt32(arthmateupdate.Tenure);
                decimal Monthly_EMI_Amt = Math.Round(pmt(loan_int_rate, Loan_Tenure, leadCoLenderResponse.SanctionAmount), 2);
                double convenienceFees = 0;
                if (leadCoLenderResponse.loan_amount <= 5000000 && leadCoLenderResponse.loan_amount >= 2000000 && (leadBusinessDetails.BusEntityType.ToLower() == "Partnership".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "PrivateLtd".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "LLP".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "HUF".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "OPC".ToLower()))
                {
                    convenienceFees = 5000;
                }
                else if (leadCoLenderResponse.loan_amount < 500000 && (leadBusinessDetails.BusEntityType.ToLower() == "Proprietorship".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "SelfEmployed".ToLower()))
                {
                    convenienceFees = 500;
                }
                else if (leadCoLenderResponse.loan_amount >= 500000 && leadCoLenderResponse.loan_amount <= 1000000 && (leadBusinessDetails.BusEntityType.ToLower() == "Proprietorship".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "SelfEmployed".ToLower()))
                {
                    convenienceFees = 900;
                }
                else if (leadCoLenderResponse.loan_amount > 100000 && leadCoLenderResponse.loan_amount < 2000000 && (leadBusinessDetails.BusEntityType.ToLower() == "Proprietorship".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "SelfEmployed".ToLower()))
                {
                    convenienceFees = 2000;
                }
                else if (leadCoLenderResponse.loan_amount > 2000000 && (leadBusinessDetails.BusEntityType.ToLower() == "Proprietorship".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "SelfEmployed".ToLower()))
                {
                    convenienceFees = 3000;
                }
                if (insurance_applied)
                {
                    var LaonInsuranceData = _context.LoanInsuranceConfiguration.Where(x => x.IsActive == true && x.IsDeleted == false && x.MonthDuration >= Loan_Tenure).FirstOrDefault();
                    if (LaonInsuranceData == null)
                    {
                        response.Message = "Loan Insurance data Not Found!";
                        return response;
                    }

                    Calc_insurance_amount = Math.Round(((LaonInsuranceData.RateOfInterestInPer / 100) * (leadCoLenderResponse.SanctionAmount * NumberOfApplicants)) * 1.18, 2);
                }
                DateTime FirstEmiDate = currentDateTime;
                if (currentDateTime.Day >= 1 && currentDateTime.Day <= 20)
                { FirstEmiDate = new DateTime(currentDateTime.AddMonths(1).Year, currentDateTime.AddMonths(1).Month, 5); }
                else
                { FirstEmiDate = new DateTime(currentDateTime.AddMonths(2).Year, currentDateTime.AddMonths(2).Month, 5); }

                int BrokenPeriod = (FirstEmiDate.Date - leadCoLenderResponse.Created.Date).Days - 30;
                double Broken_Period_Interest = Math.Round(BrokenPeriod * ((leadCoLenderResponse.SanctionAmount * (loan_int_rate / 100)) / 365), 2);
                Broken_Period_Interest = BrokenPeriod < 0 ? 0 : Broken_Period_Interest;

                double platformfee = 0;
                platformfee = loanConfig.PlatFormFee > 0 ? Math.Round((leadCoLenderResponse.SanctionAmount * loanConfig.PlatFormFee ?? 0) / 100, 2) : 0;

                TimeSpan difference = currentDateTime - leadBusinessDetails.DOI;
                int VintageDays = difference.Days;

                LoanApiRequestDc loanobj = new LoanApiRequestDc();
                loanobj.partner_loan_app_id = arthmateupdate.PartnerloanAppId;
                loanobj.partner_borrower_id = arthmateupdate.PartnerborrowerId;
                loanobj.loan_app_id = arthmateupdate.LoanAppId;
                loanobj.borrower_id = arthmateupdate.borrowerId;
                loanobj.partner_loan_id = arthmateupdate.PartnerloanAppId;
                loanobj.a_score_request_id = arthmateupdate.AScoreRequestId ?? "";
                loanobj.co_lender_assignment_id = arthmateupdate.ColenderAssignmentId > 0 ? arthmateupdate.ColenderAssignmentId.ToString() : "";
                loanobj.loan_app_date = currentDateTime.ToString("yyyy-MM-dd");
                loanobj.loan_amount_requested = (leadBusinessDetails.InquiryAmount < leadCoLenderResponse.SanctionAmount ? leadCoLenderResponse.SanctionAmount : leadBusinessDetails.InquiryAmount).ToString();
                loanobj.sanction_amount = leadCoLenderResponse.SanctionAmount.ToString();
                loanobj.processing_fees_perc = ProcessingFeePer.ToString();
                loanobj.processing_fees_amt = CalProcessingFeeAmt.ToString();
                loanobj.broken_period_int_amt = Broken_Period_Interest.ToString();
                loanobj.gst_on_pf_perc = GstONPfPer.ToString();
                loanobj.gst_on_pf_amt = CalGstONPfAmt.ToString();
                loanobj.conv_fees = Convert.ToString(convenienceFees + platformfee);
                loanobj.insurance_amount = Calc_insurance_amount.ToString();
                loanobj.net_disbur_amt = (Convert.ToDouble(leadCoLenderResponse.SanctionAmount) - (CalProcessingFeeAmt + CalGstONPfAmt + Calc_insurance_amount /*+ Broken_Period_Interest*/ + convenienceFees + platformfee)).ToString();
                loanobj.int_type = "Reducing";
                loanobj.loan_int_rate = loan_int_rate.ToString();
                loanobj.loan_int_amt = Math.Round(((Convert.ToDouble(Monthly_EMI_Amt) * Loan_Tenure) - leadCoLenderResponse.SanctionAmount), 2).ToString();
                loanobj.emi_amount = Monthly_EMI_Amt.ToString();
                loanobj.tenure = arthmateupdate.Tenure ?? "36";
                loanobj.emi_count = arthmateupdate.Tenure ?? "36";
                loanobj.repayment_type = "Monthly";
                loanobj.tenure_type = "Month";
                loanobj.first_inst_date = FirstEmiDate.ToString("yyyy-MM-dd");
                loanobj.final_approve_date = leadCoLenderResponse.Created.ToString("yyyy-MM-dd");
                loanobj.final_remarks = "Done";
                loanobj.borro_bank_name = borroBankdetail.BankName;
                loanobj.borro_bank_acc_num = borroBankdetail.AccountNumber;
                loanobj.borro_bank_ifsc = borroBankdetail.IFSCCode;
                loanobj.borro_bank_account_holder_name = borroBankdetail.AccountHolderName;
                loanobj.borro_bank_account_type = FirstCharSubstring(borroBankdetail.AccountType); //"Current";//FirstCharSubstring(leadBankDetails.AccountType);
                loanobj.business_name = leadBusinessDetails.BusinessName;
                loanobj.business_address_ownership = "Owned";// leadBusinessDetails.OwnershipType;
                loanobj.program_type = string.IsNullOrEmpty(arthmateupdate.ColenderProgramType) ? "Banking" : arthmateupdate.ColenderProgramType;
                loanobj.business_entity_type = FirstCharSubstring(leadBusinessDetails.BusEntityType);
                loanobj.business_pan = leadBusinessDetails.BusMaskPan ?? "";
                loanobj.gst_number = leadBusinessDetails.BusGSTNO ?? "";
                loanobj.udyam_reg_no = leadUdyogAadhaar.DocumentNumber;
                loanobj.purpose_of_loan = "61";
                loanobj.bene_bank_name = BeneBankdetail != null ? BeneBankdetail.BankName : borroBankdetail.BankName;
                loanobj.bene_bank_acc_num = BeneBankdetail != null ? BeneBankdetail.AccountNumber : borroBankdetail.AccountNumber;
                loanobj.bene_bank_ifsc = BeneBankdetail != null ? BeneBankdetail.IFSCCode : borroBankdetail.IFSCCode;
                loanobj.bene_bank_account_holder_name = BeneBankdetail != null ? BeneBankdetail.AccountHolderName : borroBankdetail.AccountHolderName;
                loanobj.bene_bank_account_type = BeneBankdetail != null ? FirstCharSubstring(BeneBankdetail.AccountType) : FirstCharSubstring(borroBankdetail.AccountType);
                loanobj.insurance_company = insurance_applied == true ? "GoDigit" : "NA";
                loanobj.business_vintage_overall = VintageDays.ToString();
                loanobj.marital_status = leadPersonalDetails.Marital;
                loanobj.business_establishment_proof_type = "Udhyog Adhaar";
                loanobj.relation_with_applicant = "";
                loanobj.co_app_or_guar_bureau_type = "";
                loanobj.co_app_or_guar_gender = leadPersonalDetails.Gender == "M" ? "Male" : leadPersonalDetails.Gender == "F" ? "Female" : "Other";
                loanobj.co_app_or_guar_ntc = "No";

                loanlistobj.Add(loanobj);

                ArthMateCommonAPIRequestResponse LoanResult = await _ArthMateNBFCHelper.LoanApi(loanlistobj, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.LeadNBFCApiId, leadid);
                _context.ArthMateCommonAPIRequestResponses.Add(LoanResult);
                _context.SaveChanges();

                if (LoanResult.IsSuccess)
                {
                    if (LoanResult != null && LoanResult.Response != null)
                    {
                        var loanres = JsonConvert.DeserializeObject<GetLoanApiResponseDC>(LoanResult.Response);
                        response.IsSuccess = loanres != null ? loanres.success : false;

                        if (response.IsSuccess)
                        {
                            if (loanres != null && loanres.data != null && loanres.data.Any())
                            {
                                lead.OfferCompanyId = arthmateupdate.NBFCCompanyId;
                                _context.Entry(lead).State = EntityState.Modified;

                                leadCoLenderResponse.SanctionAmount = leadCoLenderResponse.loan_amount;
                                _context.Entry(leadCoLenderResponse).State = EntityState.Modified;
                                _context.SaveChanges();

                                LeadLoan LeadLoanData = new LeadLoan();

                                LeadLoanData.LeadMasterId = leadid;
                                LeadLoanData.ReponseId = LoanResult.Id;
                                LeadLoanData.RequestId = LoanResult.Id;

                                LeadLoanData.IsSuccess = true;
                                LeadLoanData.Message = loanres.message ?? "";
                                LeadLoanData.IsActive = true;
                                LeadLoanData.IsDeleted = false;

                                LeadLoanData.loan_app_id = loanres.data.FirstOrDefault().loan_app_id ?? "";
                                LeadLoanData.loan_id = loanres.data.FirstOrDefault().loan_id ?? "";
                                LeadLoanData.borrower_id = loanres.data.FirstOrDefault().borrower_id ?? "";
                                LeadLoanData.partner_loan_app_id = loanres.data.FirstOrDefault().partner_loan_app_id ?? "";
                                LeadLoanData.partner_loan_id = loanres.data.FirstOrDefault().partner_loan_id ?? "";
                                LeadLoanData.partner_borrower_id = loanres.data.FirstOrDefault().partner_borrower_id ?? "";
                                LeadLoanData.company_id = loanres.data.FirstOrDefault().company_id ?? 0;
                                LeadLoanData.product_id = loanres.data.FirstOrDefault().product_id.ToString() ?? "";
                                LeadLoanData.loan_app_date = loanres.data.FirstOrDefault().loan_app_date ?? "";
                                LeadLoanData.sanction_amount = Convert.ToDouble(loanres.data.FirstOrDefault().sanction_amount);
                                LeadLoanData.gst_on_pf_amt = Convert.ToDouble(loanres.data.FirstOrDefault().gst_on_pf_amt);
                                LeadLoanData.gst_on_pf_perc = loanres.data.FirstOrDefault().gst_on_pf_perc ?? "";
                                LeadLoanData.net_disbur_amt = Convert.ToDouble(loanres.data.FirstOrDefault().net_disbur_amt);
                                LeadLoanData.status = loanres.data.FirstOrDefault().status ?? "";
                                LeadLoanData.stage = loanres.data.FirstOrDefault().stage ?? 0;
                                LeadLoanData.exclude_interest_till_grace_period = loanres.data.FirstOrDefault().exclude_interest_till_grace_period ?? "";
                                LeadLoanData.borro_bank_account_type = loanres.data.FirstOrDefault().borro_bank_account_type ?? "";
                                LeadLoanData.borro_bank_account_holder_name = loanres.data.FirstOrDefault().borro_bank_account_holder_name ?? "";
                                LeadLoanData.loan_int_rate = loan_int_rate.ToString();//loanres.data.FirstOrDefault().loan_int_rate?? "";
                                LeadLoanData.processing_fees_amt = Convert.ToDouble(loanres.data.FirstOrDefault().processing_fees_amt);
                                LeadLoanData.processing_fees_perc = Convert.ToDouble(loanres.data.FirstOrDefault().processing_fees_perc);
                                LeadLoanData.tenure = loanres.data.FirstOrDefault().tenure ?? "";
                                LeadLoanData.tenure_type = loanres.data.FirstOrDefault().tenure_type ?? "";
                                LeadLoanData.int_type = loanres.data.FirstOrDefault().int_type ?? "";
                                LeadLoanData.borro_bank_ifsc = loanres.data.FirstOrDefault().borro_bank_ifsc ?? "";
                                LeadLoanData.borro_bank_acc_num = loanres.data.FirstOrDefault().borro_bank_acc_num ?? "";
                                LeadLoanData.borro_bank_name = loanres.data.FirstOrDefault().borro_bank_name ?? "";
                                LeadLoanData.first_name = loanres.data.FirstOrDefault().first_name ?? "";
                                LeadLoanData.last_name = loanres.data.FirstOrDefault().last_name ?? "";
                                LeadLoanData.current_overdue_value = loanres.data.FirstOrDefault().current_overdue_value;
                                LeadLoanData.bureau_score = loanres.data.FirstOrDefault().bureau_score ?? "";
                                LeadLoanData.loan_amount_requested = Convert.ToDouble(loanres.data.FirstOrDefault().loan_amount_requested);
                                LeadLoanData.bene_bank_name = loanres.data.FirstOrDefault().bene_bank_name ?? "";
                                LeadLoanData.bene_bank_acc_num = loanres.data.FirstOrDefault().bene_bank_acc_num ?? "";
                                LeadLoanData.bene_bank_ifsc = loanres.data.FirstOrDefault().bene_bank_ifsc ?? "";
                                LeadLoanData.bene_bank_account_holder_name = loanres.data.FirstOrDefault().bene_bank_account_holder_name ?? "";
                                LeadLoanData.created_at = loanres.data.FirstOrDefault().created_at;
                                LeadLoanData.updated_at = loanres.data.FirstOrDefault().updated_at;
                                LeadLoanData.v = loanres.data.FirstOrDefault().__v ?? 0;
                                LeadLoanData.co_lender_assignment_id = loanres.data.FirstOrDefault().co_lender_assignment_id ?? 0;
                                LeadLoanData.co_lender_id = loanres.data.FirstOrDefault().co_lender_id ?? 0;
                                LeadLoanData.co_lend_flag = loanres.data.FirstOrDefault().co_lend_flag ?? "";
                                LeadLoanData.itr_ack_no = loanres.data.FirstOrDefault().itr_ack_no ?? "";
                                LeadLoanData.penal_interest = loanres.data.FirstOrDefault().penal_interest ?? 0;
                                LeadLoanData.bounce_charges = loanres.data.FirstOrDefault().bounce_charges ?? 0;
                                LeadLoanData.repayment_type = loanres.data.FirstOrDefault().repayment_type ?? "";
                                LeadLoanData.first_inst_date = loanres.data.FirstOrDefault().first_inst_date;
                                LeadLoanData.final_approve_date = loanres.data.FirstOrDefault().final_approve_date;
                                LeadLoanData.final_remarks = loanres.data.FirstOrDefault().final_remarks ?? "";
                                LeadLoanData.foir = loanres.data.FirstOrDefault().foir ?? "";
                                LeadLoanData.upfront_interest = loanres.data.FirstOrDefault().upfront_interest ?? "";
                                LeadLoanData.business_vintage_overall = loanres.data.FirstOrDefault().business_vintage_overall ?? "";
                                LeadLoanData.loan_int_amt = Convert.ToDouble(loanres.data.FirstOrDefault().loan_int_amt);
                                LeadLoanData.conv_fees = Convert.ToDouble(loanres.data.FirstOrDefault().conv_fees);
                                LeadLoanData.ninety_plus_dpd_in_last_24_months = loanres.data.FirstOrDefault().ninety_plus_dpd_in_last_24_months ?? "";
                                LeadLoanData.dpd_in_last_9_months = loanres.data.FirstOrDefault().dpd_in_last_9_months ?? "";
                                LeadLoanData.dpd_in_last_3_months = loanres.data.FirstOrDefault().dpd_in_last_3_months ?? "";
                                LeadLoanData.dpd_in_last_6_months = loanres.data.FirstOrDefault().dpd_in_last_6_months ?? "";
                                LeadLoanData.insurance_company = loanres.data.FirstOrDefault().insurance_company ?? "";
                                LeadLoanData.credit_card_settlement_amount = Convert.ToDouble(loanres.data.FirstOrDefault().credit_card_settlement_amount);
                                LeadLoanData.emi_amount = Convert.ToDouble(loanres.data.FirstOrDefault().emi_amount);
                                LeadLoanData.emi_allowed = loanres.data.FirstOrDefault().emi_allowed ?? "";
                                LeadLoanData.igst_amount = Convert.ToDouble(loanres.data.FirstOrDefault().igst_amount);
                                LeadLoanData.cgst_amount = Convert.ToDouble(loanres.data.FirstOrDefault().cgst_amount);
                                LeadLoanData.sgst_amount = Convert.ToDouble(loanres.data.FirstOrDefault().sgst_amount);
                                LeadLoanData.emi_count = loanres.data.FirstOrDefault().emi_count ?? 0;
                                LeadLoanData.broken_interest = Convert.ToDouble(loanres.data.FirstOrDefault().broken_interest);
                                LeadLoanData.dpd_in_last_12_months = loanres.data.FirstOrDefault().dpd_in_last_12_months ?? 0;
                                LeadLoanData.dpd_in_last_3_months_credit_card = loanres.data.FirstOrDefault().dpd_in_last_3_months_credit_card ?? 0;
                                LeadLoanData.dpd_in_last_3_months_unsecured = loanres.data.FirstOrDefault().dpd_in_last_3_months_unsecured ?? 0;
                                LeadLoanData.broken_period_int_amt = Convert.ToDouble(loanres.data.FirstOrDefault().broken_period_int_amt);
                                LeadLoanData.dpd_in_last_24_months = loanres.data.FirstOrDefault().dpd_in_last_24_months ?? 0;
                                LeadLoanData.avg_banking_turnover_6_months = 0;// loanres.data.FirstOrDefault().avg_banking_turnover_6_months?? "";
                                LeadLoanData.enquiries_bureau_30_days = loanres.data.FirstOrDefault().enquiries_bureau_30_days ?? 0;
                                LeadLoanData.cnt_active_unsecured_loans = loanres.data.FirstOrDefault().cnt_active_unsecured_loans ?? 0;
                                LeadLoanData.total_overdues_in_cc = loanres.data.FirstOrDefault().total_overdues_in_cc ?? 0;
                                LeadLoanData.insurance_amount = Convert.ToDouble(loanres.data.FirstOrDefault().insurance_amount);
                                LeadLoanData.bureau_outstanding_loan_amt = Convert.ToDouble(loanres.data.FirstOrDefault().bureau_outstanding_loan_amt);
                                LeadLoanData.purpose_of_loan = loanres.data.FirstOrDefault().purpose_of_loan ?? "";
                                LeadLoanData.business_name = loanres.data.FirstOrDefault().business_name ?? "";
                                LeadLoanData.co_app_or_guar_name = loanres.data.FirstOrDefault().co_app_or_guar_name ?? "";
                                LeadLoanData.co_app_or_guar_address = loanres.data.FirstOrDefault().co_app_or_guar_address ?? "";
                                LeadLoanData.co_app_or_guar_mobile_no = loanres.data.FirstOrDefault().co_app_or_guar_mobile_no ?? "";
                                LeadLoanData.co_app_or_guar_pan = loanres.data.FirstOrDefault().co_app_or_guar_pan ?? "";
                                LeadLoanData.business_address_ownership = loanres.data.FirstOrDefault().business_address_ownership ?? "";
                                LeadLoanData.business_pan = loanres.data.FirstOrDefault().business_pan ?? "";
                                LeadLoanData.bureau_fetch_date = currentDateTime;// loanres.data.FirstOrDefault().bureau_fetch_date?? "";
                                LeadLoanData.enquiries_in_last_3_months = loanres.data.FirstOrDefault().enquiries_in_last_3_months ?? 0;
                                LeadLoanData.gst_on_conv_fees = loanres.data.FirstOrDefault().gst_on_conv_fees ?? 0;
                                LeadLoanData.cgst_on_conv_fees = loanres.data.FirstOrDefault().cgst_on_conv_fees ?? 0;
                                LeadLoanData.sgst_on_conv_fees = loanres.data.FirstOrDefault().sgst_on_conv_fees ?? 0;
                                LeadLoanData.igst_on_conv_fees = loanres.data.FirstOrDefault().igst_on_conv_fees ?? 0;
                                LeadLoanData.interest_type = loanres.data.FirstOrDefault().interest_type ?? "";
                                LeadLoanData.conv_fees_excluding_gst = loanres.data.FirstOrDefault().conv_fees_excluding_gst ?? 0;
                                LeadLoanData.a_score_request_id = loanres.data.FirstOrDefault().a_score_request_id ?? "";
                                LeadLoanData.a_score = loanres.data.FirstOrDefault().a_score ?? 0;
                                LeadLoanData.b_score = loanres.data.FirstOrDefault().b_score ?? 0;
                                LeadLoanData.offered_amount = Convert.ToDouble(loanres.data.FirstOrDefault().offered_amount);
                                LeadLoanData.offered_int_rate = loanres.data.FirstOrDefault().offered_int_rate.Value;
                                LeadLoanData.monthly_average_balance = loanres.data.FirstOrDefault().monthly_average_balance.Value;
                                LeadLoanData.monthly_imputed_income = loanres.data.FirstOrDefault().monthly_imputed_income.Value;
                                LeadLoanData.party_type = loanres.data.FirstOrDefault().party_type ?? "";
                                LeadLoanData.co_app_or_guar_dob = currentDateTime; //loanres.data.FirstOrDefault().co_app_or_guar_dob?? "";
                                LeadLoanData.co_app_or_guar_gender = loanres.data.FirstOrDefault().co_app_or_guar_gender ?? "";
                                LeadLoanData.co_app_or_guar_ntc = loanres.data.FirstOrDefault().co_app_or_guar_ntc ?? "";
                                LeadLoanData.udyam_reg_no = loanres.data.FirstOrDefault().udyam_reg_no ?? "";
                                LeadLoanData.program_type = loanres.data.FirstOrDefault().program_type ?? "";
                                LeadLoanData.written_off_settled = loanres.data.FirstOrDefault().written_off_settled ?? 0;
                                LeadLoanData.upi_handle = loanres.data.FirstOrDefault().upi_handle ?? "";
                                LeadLoanData.upi_reference = loanres.data.FirstOrDefault().upi_reference ?? "";
                                LeadLoanData.fc_offer_days = loanres.data.FirstOrDefault().fc_offer_days ?? 0;
                                LeadLoanData.foreclosure_charge = loanres.data.FirstOrDefault().foreclosure_charge ?? "";
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
                                LeadLoanData.cgst_on_application_fees = 0;
                                LeadLoanData.cgst_on_subvention_fees = 0;
                                LeadLoanData.co_app_or_guar_bureau_score = "";
                                LeadLoanData.customer_type_ntc = "";
                                LeadLoanData.emi_obligation = "";
                                LeadLoanData.gst_number = "";
                                LeadLoanData.gst_on_application_fees = 0;
                                LeadLoanData.gst_on_subvention_fees = 0;
                                LeadLoanData.igst_on_application_fees = 0;
                                LeadLoanData.igst_on_subvention_fees = 0;
                                LeadLoanData.monthly_income = "";
                                LeadLoanData.sgst_on_application_fees = 0;
                                LeadLoanData.sgst_on_subvention_fees = 0;
                                LeadLoanData.subvention_fees_amount = 0;
                                LeadLoanData.PlatFormFee = platformfee;
                                _context.LeadLoan.Add(LeadLoanData);
                                _context.SaveChanges();
                            }
                        }
                        else
                        {
                            response.Message = "Internal server error please try after sometime.";
                        }
                    }
                }
                response.IsSuccess = response.IsSuccess; //LoanResult.IsSuccess;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Lead ArthMateUpdates/LeadBankDetails/PersonalDetails/BusinessDetails/CoLenderResponse/LeadDocumentDetails Data Not Found";
            }

            return response;
        }
        public string FirstCharSubstring(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return $"{input[0].ToString().ToUpper()}{input.Substring(1)}";
        }
        public async Task<ICreateLeadNBFCResponse> ArthmateDocValidationJsonXml(LeadNBFCSubActivityDTO apis, long leadid, long NBFCCompanyId, bool isTest = false)
        {
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();

            var DocumentMasters = _context.ArthMateDocumentMaster.Where(x => x.DocumentName == "pan_card" || x.DocumentName == "aadhar_card").ToList();
            var DocumentMastersIds = DocumentMasters.Select(x => x.Id).ToList();

            var kycJson = _context.KYCValidationResponse.Where(x => x.LeadMasterId == leadid && DocumentMastersIds.Contains(x.DocumentMasterId) && x.IsActive && !x.IsDeleted).ToList();
            var arthmateupdate = _context.ArthMateUpdates.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (arthmateupdate != null && DocumentMasters != null && DocumentMasters.Any())
            {
                foreach (var item in kycJson)
                {
                    byte[] mybytearray = item.ResponseJson.ToBytes();
                    var Jsonbase64string = Convert.ToBase64String(mybytearray);

                    JsonXmlRequest jsonXmlRequest = new JsonXmlRequest()
                    {
                        base64pdfencodedfile = Jsonbase64string,
                        borrower_id = arthmateupdate.borrowerId,
                        code = DocumentMasters.FirstOrDefault(x => x.DocumentName == "pan_card").Id == item.DocumentMasterId ? "116" : "114",
                        loan_app_id = arthmateupdate.LoanAppId,
                        partner_borrower_id = arthmateupdate.PartnerborrowerId,
                        partner_loan_app_id = arthmateupdate.PartnerloanAppId
                    };

                    //apis.APIUrl = "https://uat-apiorigin.arthmate.com/api/kyc/loandocument/parser";
                    //apis.TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE";
                    //apis.TAPISecretKey = "";
                    ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.ArthmateDocValidationJsonXml(jsonXmlRequest, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.LeadNBFCApiId, leadid);
                    _context.ArthMateCommonAPIRequestResponses.Add(Result);
                    response.IsSuccess = Result.IsSuccess;
                }
                _context.SaveChanges();
            }
            return response;
        }
        public Task<ICreateLeadNBFCResponse> PanUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PersonAddressUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PersonUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PrpareAgreement(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }
        public async Task<CommonResponseDc> GenerateOtpToAcceptOffer(long LeadMasterId)
        {
            CommonResponseDc response = new CommonResponseDc { Msg = "Something went wrong" };
            var apis = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateAadhaarOtpSend && x.IsActive && !x.IsDeleted);
            if (apis != null && LeadMasterId > 0)
            {
                var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == LeadMasterId && x.IsActive && !x.IsDeleted);
                var arthmateUpdateData = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == LeadMasterId && x.IsActive && !x.IsDeleted);

                if (leadPersonalDetails != null && arthmateUpdateData != null)
                {
                    FirstAadharXMLPost obj = new FirstAadharXMLPost();
                    obj.aadhaar_no = leadPersonalDetails.AadhaarMaskNO;
                    obj.loan_app_id = arthmateUpdateData.LoanAppId;

                    ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.GenerateOtpToAcceptOffer(obj, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.Id, LeadMasterId);
                    _context.ArthMateCommonAPIRequestResponses.Add(Result);
                    _context.SaveChanges();
                    if (Result.IsSuccess)
                    {
                        var resDC = JsonConvert.DeserializeObject<AadharOtpGenerateRes>(Result.Response);
                        if (Result.IsSuccess && resDC != null && resDC.data != null && resDC.data.requestId != null)
                        {
                            response.Data = resDC.data.requestId;
                            response.Msg = resDC.data.result.message;
                            response.Status = resDC.success;
                        }
                        else
                        {
                            response.Msg = resDC != null && resDC.message != null ? resDC.message : "somthing went wrong";
                            response.Status = false;
                        }
                    }
                }
                else
                {
                    response.Msg = "Data Not Found in Lead!";
                    response.Status = false;
                }
            }
            return response;
        }

        public async Task<GRPCReply<string>> AcceptOfferWithXMLAadharOTP(GRPCRequest<SecondAadharXMLDc> request)
        {
            GRPCReply<string> res = new GRPCReply<string>();
            if (request.Request != null)
            {
                var leaddata = await _context.Leads.FirstOrDefaultAsync(x => x.Id == request.Request.LeadMasterId && x.IsActive && !x.IsDeleted);

                var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadMasterId && x.IsActive && !x.IsDeleted);
                var arthmateUpdateData = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadMasterId && x.IsActive && !x.IsDeleted);
                var DocumentMasters = _context.ArthMateDocumentMaster.Where(x => x.DocumentName == "pan_card" || x.DocumentName == "aadhar_card").ToList();
                var DocumentMastersIds = DocumentMasters.Select(x => x.Id).ToList();
                var apis = await _context.LeadNBFCApis.Where(x => (x.Code == CompanyApiConstants.ArthmateAadhaarOtpVerify
                                                        || x.Code == CompanyApiConstants.ArthmateLoanDocument
                                                        || x.Code == CompanyApiConstants.ArthmateLoanGenerate)
                                                        && x.IsActive && !x.IsDeleted).ToListAsync();

                var leadoffer = await _context.LeadOffers.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadMasterId && x.NBFCCompanyId == arthmateUpdateData.NBFCCompanyId && x.IsActive && !x.IsDeleted);

                if (apis != null && apis.Any() && leadPersonalDetails != null && arthmateUpdateData != null && leadoffer != null)
                {
                    SecondAadharXMLPost obj = new SecondAadharXMLPost();
                    obj.aadhaar_no = leadPersonalDetails.AadhaarMaskNO;
                    obj.loan_app_id = arthmateUpdateData.LoanAppId;
                    obj.request_id = request.Request.request_id;
                    obj.otp = request.Request.otp;

                    var AdharApi = apis.FirstOrDefault(x => x.Code == CompanyApiConstants.ArthmateAadhaarOtpVerify);
                    ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.StepTwoAadharToAcceptOffer(obj, AdharApi.APIUrl, AdharApi.TAPIKey, AdharApi.TAPISecretKey, AdharApi.Id, request.Request.LeadMasterId);
                    _context.ArthMateCommonAPIRequestResponses.Add(Result);
                    _context.SaveChanges();
                    if (Result.IsSuccess)
                    {
                        var resDC = JsonConvert.DeserializeObject<SecondAadharXMLResponseDc>(Result.Response);
                        if (resDC != null && resDC.data != null && resDC.data.result.message == "Aadhaar XML file downloaded successfully")
                        {

                            var documentId = DocumentMasters.FirstOrDefault(x => x.DocumentName == "aadhar_card").Id;

                            KYCValidationResponse kyc = new KYCValidationResponse();
                            kyc.LeadMasterId = request.Request.LeadMasterId;
                            kyc.DocumentMasterId = Convert.ToInt32(documentId);
                            kyc.Status = resDC.success == true ? "success" : "failed";
                            kyc.kyc_id = resDC.kyc_id ?? "";
                            kyc.ResponseJson = Result.Response ?? "";
                            kyc.Message = resDC.data.result.message ?? "";
                            kyc.Remark = "aadhar_card";
                            kyc.IsKycVerified = false;
                            kyc.IsDeleted = false;
                            kyc.IsActive = true;
                            _context.KYCValidationResponse.Add(kyc);
                            _context.SaveChanges();

                            List<LoanDocumentPostDc> loanDocumentPostDcs = new List<LoanDocumentPostDc>();
                            var kycJson = _context.KYCValidationResponse.Where(x => x.LeadMasterId == request.Request.LeadMasterId && DocumentMastersIds.Contains(x.DocumentMasterId) && !x.IsKycVerified && x.IsActive && !x.IsDeleted).ToList();
                            foreach (var item in kycJson)
                            {
                                string DocBase64String = "";
                                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(item.ResponseJson);
                                DocBase64String = Convert.ToBase64String(plainTextBytes);

                                loanDocumentPostDcs.Add(new LoanDocumentPostDc
                                {
                                    base64pdfencodedfile = DocBase64String,
                                    file_url = "",
                                    FrontUrl = "",
                                    loan_app_id = arthmateUpdateData.LoanAppId,
                                    partner_borrower_id = arthmateUpdateData.PartnerborrowerId,
                                    partner_loan_app_id = arthmateUpdateData.PartnerloanAppId,
                                    borrower_id = arthmateUpdateData.borrowerId,
                                    PdfPassword = "",
                                    code = DocumentMasters.FirstOrDefault(x => x.DocumentName == "pan_card").Id == item.DocumentMasterId ? "116" : "114",
                                });
                            }
                            string basePath = _hostingEnvironment.ContentRootPath;
                            if (loanDocumentPostDcs.Any())
                            {
                                var DocApis = apis.FirstOrDefault(x => x.Code == CompanyApiConstants.ArthmateLoanDocument);
                                if (DocApis != null)
                                {
                                    foreach (var loanDocument in loanDocumentPostDcs)
                                    {
                                        ArthMateCommonAPIRequestResponse responseData = await _ArthMateNBFCHelper.LoanDocumentApi(loanDocument, DocApis.APIUrl, DocApis.TAPIKey, DocApis.TAPISecretKey, DocApis.Id, request.Request.LeadMasterId, basePath);
                                        _context.ArthMateCommonAPIRequestResponses.Add(responseData);

                                        if (responseData.IsSuccess)
                                        {
                                            var PanJsondata = kycJson.Where(x => x.Remark == "pan_card").ToList();
                                            if (PanJsondata != null && PanJsondata.Any())
                                            {
                                                foreach (var item in PanJsondata)
                                                {
                                                    item.IsKycVerified = true;
                                                    _context.Entry(item).State = EntityState.Modified;
                                                }
                                            }
                                            var AadharJsondata = kycJson.Where(x => x.Remark == "aadhar_card").ToList();
                                            if (AadharJsondata != null && AadharJsondata.Any())
                                            {
                                                foreach (var item in AadharJsondata)
                                                {
                                                    item.IsKycVerified = true;
                                                    _context.Entry(item).State = EntityState.Modified;
                                                }

                                            }
                                        }
                                        _context.SaveChanges();
                                    }
                                }
                            }

                            var colender = await _context.CoLenderResponse.Where(x => x.LeadMasterId == request.Request.LeadMasterId && x.IsActive == true && x.IsDeleted == false && x.status == "success").OrderByDescending(x => x.Id).FirstOrDefaultAsync();

                            // Generate Loan
                            var Loanapi = apis.FirstOrDefault(x => x.Code == CompanyApiConstants.ArthmateLoanGenerate);
                            if (Loanapi != null)
                            {
                                var api = new LeadNBFCSubActivityDTO
                                {
                                    Sequence = 0,
                                    APIUrl = Loanapi.APIUrl,
                                    TAPIKey = Loanapi.TAPIKey,
                                    TAPISecretKey = Loanapi.TAPISecretKey,
                                    LeadNBFCApiId = Loanapi.Id
                                };

                                var LoanresData = await LoanGenerate(api, request.Request.insurance_applied, request.Request.LeadMasterId, 0, request.Request.loan_amt, request.Request.ProductCompanyConfig);

                                if (colender != null && LoanresData.IsSuccess)
                                {
                                    _logger.LogWarning("Accept offer with otp");
                                    colender.SanctionAmount = request.Request.loan_amt;
                                    _context.Entry(colender).State = EntityState.Modified;

                                    _leadNBFCSubActivityManager.UpdateLeadProgressStatus(request.Request.LeadMasterId, ActivityConstants.ArthmateShowOffer);
                                    _leadNBFCSubActivityManager.UpdateLeadMasterStatus(request.Request.LeadMasterId, LeadBusinessLoanStatusConstants.LoanInitiated);

                                    leaddata.CreditLimit = request.Request.loan_amt;
                                    _context.Entry(leaddata).State = EntityState.Modified;

                                    leadoffer.CreditLimit = request.Request.loan_amt;
                                    _context.Entry(leadoffer).State = EntityState.Modified;


                                    res.Response = "Offer Accepted with Aadhar successfully";
                                    res.Status = true;
                                    _logger.LogWarning("Accept offer with otp response:" + res.ToString());
                                }
                                else
                                {
                                    res.Response = LoanresData.Message;
                                    res.Status = false;
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Accept offer Aadhaar xml failed");
                            res.Message = resDC != null && resDC.data != null && resDC.data.result != null && resDC.data.result.message != null ? resDC.data.result.message : "Incorrect OTP";
                            res.Response = resDC != null && resDC.data != null && resDC.data.result != null && resDC.data.result.message != null ? resDC.data.result.message : "Incorrect OTP";
                            res.Status = resDC != null ? resDC.success : false;
                            _logger.LogWarning("Accept offer Aadhaar xml failed : " + res.ToString());
                        }
                    }
                    else
                    {
                        res.Response = "Internal server error please try after sometime.";
                    }
                    _context.SaveChanges();
                }
            }
            else
            {
                res.Response = "Data Not Found in Lead!";
            }
            _logger.LogWarning("Accept offer final return :" + res.Status.ToString());
            return res;
        }

        public async Task<CommonResponseDc> OfferRegenerate(int LeadId, int tenure, double sactionAmount)
        {
            CommonResponseDc res = new CommonResponseDc();
            res.Msg = "Not Verified";
            res.Status = false;
            var ArthMateUpdateData = _context.ArthMateUpdates.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            var leadBusinesDetails = _context.BusinessDetails.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).FirstOrDefault();

            var query = from a in _context.LeadNBFCSubActivitys
                        join b in _context.LeadNBFCApis on a.Id equals b.LeadNBFCSubActivityId
                        where b.IsActive == true && b.IsDeleted == false
                        && a.LeadId == LeadId
                        && a.Code == SubActivityConstants.Offer
                        && b.Code == CompanyApiConstants.ArthmateGenerateOffer
                        && a.IdentificationCode == CompanyIdentificationCodeConstants.ArthMate
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

            var offerApi = query.FirstOrDefault();

            //if (offerApi != null)
            //{
            //    offerApi.Status = LeadNBFCApiConstants.Inprogress;
            //    _leadNBFCSubActivityManager.UpdateStatus(offerApi.LeadNBFCApiId, LeadNBFCApiConstants.Inprogress);
            //    _leadNBFCSubActivityManager.UpdateSubActivityStatus(offerApi.LeadNBFCSubActivityId, LeadNBFCSubActivityConstants.Inprogress);
            //}
            //if (ArthMateUpdateData != null && leadBusinesDetails != null)
            //{
            //    //ArthMateUpdateData.Tenure = tenure.ToString();
            //    //leadBusinesDetails.InquiryAmount = sactionAmount;

            //    //_leadNBFCSubActivityManager.UpdateLeadProgressStatus(LeadId, ActivityConstants.Inprogress, false);

            //    _context.Entry(ArthMateUpdateData).State = EntityState.Modified;
            //    _context.Entry(leadBusinesDetails).State = EntityState.Modified;
            //}
            //if (_context.SaveChanges() > 0)
            {
                if (offerApi != null)
                {
                    long NBFCcompanyId = ArthMateUpdateData.NBFCCompanyId ?? 0;
                    var result = await ArthmateGenerateOffer(offerApi, LeadId, NBFCcompanyId, tenure, sactionAmount);
                    res.Data = result;  //"Your request under process.";
                    res.Msg = result.Message;  //"Your request under process.";
                    res.Status = true;
                }
                else
                {
                    res.Msg = "Generate Offer API Not Found";
                    res.Status = false;
                }
            }
            return res;
        }
        //Generate Agreement
        public async Task<LBABusinessLoanDc> LBABusinessLoan(long leadid, string AgreementURL, bool IsSubmit, ProductCompanyConfigDc loanConfig)
        {
            string htmldata = "";

            LBABusinessLoanDc lbabusinessloan = new LBABusinessLoanDc();
            repay_scheduleDc response = new repay_scheduleDc

            {
                success = false,
                message = "Agreement Not Found"
            };
            var lead = await _context.Leads.Where(x => x.Id == leadid && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            var arthmateupdate = await _context.ArthMateUpdates.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            var leadBankDetails = await _context.LeadBankDetails.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).ToListAsync();
            var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var leadBusinessDetails = await _context.BusinessDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var leadCoLenderResponse = await _context.CoLenderResponse.FirstOrDefaultAsync(x => x.LeadMasterId == leadid && x.IsActive && !x.IsDeleted);
            var leadLoandata = await _context.LeadLoan.Where(x => x.LeadMasterId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            if (lead != null && arthmateupdate != null && leadPersonalDetails != null && leadBusinessDetails != null && leadBankDetails != null && leadBankDetails.Any() && leadCoLenderResponse != null && leadLoandata != null && loanConfig != null)
            {
                var borroBankdetail = leadBankDetails.FirstOrDefault(x => x.Type == BankTypeConstant.Borrower);
                var BeneBankdetail = leadBankDetails.FirstOrDefault(x => x.Type == BankTypeConstant.Beneficiary);

                lbabusinessloan.IseSignEnable = loanConfig.IseSignEnable;

                var Anchorconfig = loanConfig.ProductSlabConfigs.Where(x => x.MinLoanAmount <= leadCoLenderResponse.SanctionAmount && x.MaxLoanAmount >= leadCoLenderResponse.SanctionAmount).ToList();
                if (Anchorconfig != null && Anchorconfig.Any())
                {
                    var profee = Anchorconfig.First(x => x.SlabType == SlabTypeConstants.PF).MaxValue;

                    string replacetext = "";
                    string path = "";

                    Postrepayment_scheduleDc reqdata = new Postrepayment_scheduleDc()
                    {
                        company_id = leadLoandata.company_id.Value,
                        loan_id = leadLoandata.loan_id,
                        product_id = leadLoandata.product_id
                    };
                    var repaymentScheduleApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateRepaymentSchedule && x.IsActive && !x.IsDeleted);
                    ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.repayment_schedule(reqdata, repaymentScheduleApi.APIUrl, repaymentScheduleApi.TAPIKey, repaymentScheduleApi.TAPISecretKey, repaymentScheduleApi.Id, leadid);
                    _context.ArthMateCommonAPIRequestResponses.Add(Result);
                    _context.SaveChanges();
                    if (Result.IsSuccess && !string.IsNullOrEmpty(Result.Response))
                    {
                        string StampUrl = "";
                        var arthmateSlaLbaStamp = _context.ArthmateSlaLbaStampDetail.FirstOrDefault(x => x.LeadmasterId == leadid && x.IsActive && x.IsDeleted == false);
                        if (arthmateSlaLbaStamp != null)
                        {
                            if (arthmateSlaLbaStamp.IsStampUsed == false)
                            {
                                string base64 = await FileSaverHelper.GetStreamFromUrlBase64(arthmateSlaLbaStamp.StampUrl);
                                string imageSrc = "data:image/" + Path.GetExtension(arthmateSlaLbaStamp.StampUrl).Replace(".", "") + ";base64," + base64;
                                int width = 900;
                                int height = 1100;
                                // Modify the StampUrl string to include the width and height attributes
                                StampUrl = $"<img src=\"{imageSrc}\" width=\"{width}\" height=\"{height}\" />";
                            }
                            else
                            {
                                lbabusinessloan.Htmldata = "The Stamp has been used.Please Contact Administrator to reuse this stamp.";
                                return lbabusinessloan;
                            }
                        }
                        else
                        {
                            var StampUrlData = _context.ArthmateSlaLbaStampDetail.FirstOrDefault(x => x.LeadmasterId == 0 && x.IsStampUsed == false && x.IsActive && x.IsDeleted == false);
                            if (StampUrlData != null)
                            {
                                string url = StampUrlData.StampUrl;
                                string base64 = await FileSaverHelper.GetStreamFromUrlBase64(StampUrlData.StampUrl);
                                string imageSrc = "data:image/" + Path.GetExtension(url).Replace(".", "") + ";base64," + base64;
                                // Set the desired width and height
                                int width = 800;
                                int height = 900;
                                // Modify the StampUrl string to include the width and height attributes
                                StampUrl = $"<img src=\"{imageSrc}\" width=\"{width}\" height=\"{height}\" />";
                                StampUrlData.LeadmasterId = leadid;
                                //StampUrlData.DateofUtilisation = DateTime.Now;
                                _context.Entry(StampUrlData).State = EntityState.Modified;
                                _context.SaveChanges();
                            }
                            else
                            {
                                lbabusinessloan.Htmldata = "The Stamp Is Not Available.Please Contact Administrator.";
                                return lbabusinessloan;
                            }
                        }

                        response = JsonConvert.DeserializeObject<repay_scheduleDc>(Result.Response);

                        byte[] file = null;

                        using (var wc = new WebClient())
                        {
                            file = wc.DownloadData(AgreementURL);
                        }

                        if (file.Length > 0)
                        {
                            htmldata = System.Text.Encoding.UTF8.GetString(file);

                            if (!string.IsNullOrEmpty(htmldata))
                            {
                                replacetext = $"{leadLoandata.loan_id}";
                                htmldata = htmldata.Replace("{{loan_id}}", replacetext);

                                replacetext = $"{leadPersonalDetails.FirstName} ";
                                htmldata = htmldata.Replace("{{FirstNameOfBorrower}}", replacetext);

                                replacetext = $"{leadPersonalDetails.MiddleName}&nbsp;";
                                htmldata = htmldata.Replace("{{MiddleNameOfBorrower}}", replacetext);

                                replacetext = $"{leadPersonalDetails.LastName} ";
                                htmldata = htmldata.Replace("{{LastNameOfBorrower}}", replacetext);

                                replacetext = $"{leadid} ";
                                htmldata = htmldata.Replace("{{LeadId}}", replacetext);

                                replacetext = $"{leadPersonalDetails.PanMaskNO} ";
                                htmldata = htmldata.Replace("{{appl_pan}}", replacetext);

                                replacetext = $"{leadBusinessDetails.BusMaskPan} ";
                                htmldata = htmldata.Replace("{{co_app_pan}}", replacetext);

                                //replacetext = $"{loanConfig.InterestRate} ";
                                replacetext = $"{lead.InterestRate} ";
                                htmldata = htmldata.Replace("{{InterestRate}}", replacetext);

                                replacetext = $" {borroBankdetail.AccountHolderName} ";
                                htmldata = htmldata.Replace("{{AccountHolderName}}", replacetext);

                                replacetext = $" {borroBankdetail.BankName} ";
                                htmldata = htmldata.Replace("{{bankName}}", replacetext);

                                replacetext = $" {borroBankdetail.AccountNumber} ";
                                htmldata = htmldata.Replace("{{accountNumber}}", replacetext);

                                replacetext = $" {borroBankdetail.IFSCCode} ";
                                htmldata = htmldata.Replace("{{IFSCCode}}", replacetext);

                                #region   Benefiarcy Bank Details
                                var AccountHolderName = BeneBankdetail != null ? BeneBankdetail.AccountHolderName : borroBankdetail.AccountHolderName;

                                replacetext = $" {AccountHolderName} ";
                                htmldata = htmldata.Replace("{{ben_Accountholdername}}", replacetext);

                                var AccountNumber = BeneBankdetail != null ? BeneBankdetail.AccountNumber : borroBankdetail.AccountNumber;
                                replacetext = $" {AccountNumber} ";
                                htmldata = htmldata.Replace("{{ben_AccountNumber}}", replacetext);

                                var IFSCCode = BeneBankdetail != null ? BeneBankdetail.IFSCCode : borroBankdetail.IFSCCode;

                                replacetext = $" {IFSCCode} ";
                                htmldata = htmldata.Replace("{{ben_IFSCCode}}", replacetext);

                                var AccountType = BeneBankdetail != null ? BeneBankdetail.AccountType : borroBankdetail.AccountType;
                                replacetext = $" {AccountType} ";
                                htmldata = htmldata.Replace("{{ben_Typeofaccount}}", replacetext);

                                var BankName = BeneBankdetail != null ? BeneBankdetail.BankName : borroBankdetail.BankName;
                                replacetext = $" {BankName} ";
                                htmldata = htmldata.Replace("{{ben_Bankname}}", replacetext);


                                #endregion


                                //sanction letter

                                replacetext = $"{arthmateupdate.Created.ToString("dd/MM/yyyy")} ";
                                htmldata = htmldata.Replace("{{DATE}}", replacetext);

                                replacetext = $"{leadBusinessDetails.BusinessName} ";
                                htmldata = htmldata.Replace("{{BorrowerName}}", replacetext);

                                replacetext = $"{leadPersonalDetails.PermanentAddressLineOne} ";
                                htmldata = htmldata.Replace("{{address1}}", replacetext);

                                replacetext = $"{leadPersonalDetails.PermanentAddressLineTwo} ";
                                htmldata = htmldata.Replace("{{address2}}", replacetext);

                                replacetext = $"{leadPersonalDetails.PermanentCityName} ";
                                htmldata = htmldata.Replace("{{city}}", replacetext);

                                replacetext = $"{leadPersonalDetails.PermanentStateName} ";
                                htmldata = htmldata.Replace("{{state}}", replacetext);

                                replacetext = $"{leadPersonalDetails.MobileNo} ";
                                htmldata = htmldata.Replace("{{MobileNo}}", replacetext);

                                replacetext = $"{leadPersonalDetails.EmailId} ";
                                htmldata = htmldata.Replace("{{email_id}}", replacetext);

                                replacetext = $" {arthmateupdate.Created}";
                                htmldata = htmldata.Replace("{{ApplicationDate}}", replacetext);

                                replacetext = $" {leadPersonalDetails.FirstName + leadPersonalDetails.LastName} ";
                                htmldata = htmldata.Replace("{{BorrowerNamess}}", replacetext);

                                replacetext = $" {leadBusinessDetails.AddressLineOne} ";
                                htmldata = htmldata.Replace("{{bus_add_corr_line1}}", replacetext);

                                replacetext = $" {leadBusinessDetails.AddressLineTwo} ";
                                htmldata = htmldata.Replace("{{bus_add_corr_line2}}", replacetext);

                                replacetext = $" {leadBusinessDetails.CityName} ";
                                htmldata = htmldata.Replace("{{bus_add_corr_city}}", replacetext);

                                replacetext = $" {leadBusinessDetails.StateName}";
                                htmldata = htmldata.Replace("{{bus_add_corr_state}}", replacetext);


                                replacetext = $" {profee} ";
                                htmldata = htmldata.Replace("{{ProcessingFees}}", replacetext);

                                replacetext = $"61";
                                htmldata = htmldata.Replace("{{purpose_of_loan}}", replacetext);

                                replacetext = $" {borroBankdetail.AccountType} ";
                                htmldata = htmldata.Replace("{{Acctype}}", replacetext);

                                replacetext = $" {leadBusinessDetails.BusinessName} ";
                                htmldata = htmldata.Replace("{{business_name}}", replacetext);


                                replacetext = $" {borroBankdetail.AccountHolderName} ";
                                htmldata = htmldata.Replace("{{AccountHolderName}}", replacetext);

                                double convenienceFees = 0;
                                if (leadCoLenderResponse.loan_amount <= 5000000 && leadCoLenderResponse.loan_amount >= 2000000 && (leadBusinessDetails.BusEntityType.ToLower() == "Partnership".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "PrivateLtd".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "LLP".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "HUF".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "OPC".ToLower()))
                                {
                                    convenienceFees = 5000;
                                }
                                else if (leadCoLenderResponse.loan_amount < 500000 && (leadBusinessDetails.BusEntityType.ToLower() == "Proprietorship".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "SelfEmployed".ToLower()))
                                {
                                    convenienceFees = 500;
                                }
                                else if (leadCoLenderResponse.loan_amount >= 500000 && leadCoLenderResponse.loan_amount <= 1000000 && (leadBusinessDetails.BusEntityType.ToLower() == "Proprietorship".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "SelfEmployed".ToLower()))
                                {
                                    convenienceFees = 900;
                                }
                                else if (leadCoLenderResponse.loan_amount > 100000 && leadCoLenderResponse.loan_amount < 2000000 && (leadBusinessDetails.BusEntityType.ToLower() == "Proprietorship".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "SelfEmployed".ToLower()))
                                {
                                    convenienceFees = 2000;
                                }
                                else if (leadCoLenderResponse.loan_amount > 2000000 && (leadBusinessDetails.BusEntityType.ToLower() == "Proprietorship".ToLower() || leadBusinessDetails.BusEntityType.ToLower() == "SelfEmployed".ToLower()))
                                {
                                    convenienceFees = 3000;
                                }

                                double platformfee = 0;
                                platformfee = loanConfig.PlatFormFee > 0 ? Math.Round((leadCoLenderResponse.SanctionAmount * loanConfig.PlatFormFee ?? 0) / 100, 2) : 0;


                                replacetext = $" {convenienceFees + platformfee} ";
                                htmldata = htmldata.Replace("{{convenienceFees}}", replacetext);

                                double ProcessingFeesAmt = 0;
                                ProcessingFeesAmt = Math.Round(leadCoLenderResponse.SanctionAmount * (profee / 100), 2);
                                ProcessingFeesAmt = ProcessingFeesAmt * 1.18;


                                replacetext = $" {ProcessingFeesAmt} ";
                                htmldata = htmldata.Replace("{{ProcessingFeeAmount}}", replacetext);

                                var Broken_Period_interest = leadLoandata.broken_period_int_amt;
                                replacetext = $" {Broken_Period_interest} ";
                                htmldata = htmldata.Replace("{{Broken_Period_interest}}", replacetext);

                                if (!string.IsNullOrEmpty(StampUrl))
                                {
                                    replacetext = $" {StampUrl.Trim()} ";
                                    htmldata = htmldata.Replace("{{StampUrl}}", replacetext);
                                }
                                double OutstandingAmount = 0;
                                double PenalAmount = 0;
                                double PenalAmountGST = 0;
                                double TotalPenalAmount = 0;
                                double processing_fees_perc = profee;
                                double prepayment_charges_amt = 0;
                                double processingfeesamount = 0;

                                if (response != null && response.data != null && response.data.rows != null && response.data.rows.Any())
                                {
                                    double borroamt = Convert.ToDouble(leadCoLenderResponse.SanctionAmount) + (Convert.ToDouble(leadLoandata.loan_int_amt)
                                       + Convert.ToDouble(leadLoandata.insurance_amount)) + Convert.ToDouble(convenienceFees) /*+ Convert.ToDouble(leadLoandata.broken_period_int_amt)*/;

                                    replacetext = $"{borroamt + ProcessingFeesAmt}";
                                    htmldata = htmldata.Replace("{{TotalBorroAmt}}", replacetext);//total amt paid by the borrower

                                    OutstandingAmount = response.data.rows.FirstOrDefault().principal_outstanding;
                                    PenalAmount = OutstandingAmount * (loanConfig.PenalPercent / 100);
                                    PenalAmountGST = PenalAmount * (loanConfig.GST / 100);

                                    TotalPenalAmount = PenalAmount + PenalAmountGST;

                                    prepayment_charges_amt = (OutstandingAmount) * (profee / 100);


                                    replacetext = $" {TotalPenalAmount} ";
                                    htmldata = htmldata.Replace("{{TotalPenalAmount}}", replacetext);

                                    replacetext = $" {prepayment_charges_amt} ";
                                    htmldata = htmldata.Replace("{{prepayment_charges_amt}}", replacetext);


                                    //processing fees amount
                                    processingfeesamount = Convert.ToDouble(leadCoLenderResponse.SanctionAmount) * (processing_fees_perc / 100);

                                    replacetext = $" {processingfeesamount} ";
                                    htmldata = htmldata.Replace("{{processingfeesamount}}", replacetext);

                                    replacetext = $" {leadCoLenderResponse.SanctionAmount} ";
                                    htmldata = htmldata.Replace("{{SanctionAmount}}", replacetext);


                                    replacetext = $" {leadLoandata.first_inst_date.Value.ToString("dd/MM/yyyy")} ";
                                    htmldata = htmldata.Replace("{{first_inst_date}}", replacetext);

                                    replacetext = $" {leadLoandata.loan_amount_requested} ";
                                    htmldata = htmldata.Replace("{{loan_amount_requested}}", replacetext);

                                    replacetext = $" {leadLoandata.loan_int_amt} ";
                                    htmldata = htmldata.Replace("{{loan_int_amt}}", replacetext);

                                    replacetext = $" {leadLoandata.insurance_amount} ";
                                    htmldata = htmldata.Replace("{{insurance_amount}}", replacetext);

                                    replacetext = $" {arthmateupdate.Tenure} ";
                                    htmldata = htmldata.Replace("{{tenure}}", replacetext);

                                    replacetext = $" {leadLoandata.net_disbur_amt} ";
                                    htmldata = htmldata.Replace("{{net_disbur_amt}}", replacetext);

                                    replacetext = $" {leadLoandata.loan_int_rate} ";
                                    htmldata = htmldata.Replace("{{loan_int_rate}}", replacetext);

                                    replacetext = $" {leadLoandata.penal_interest} ";
                                    htmldata = htmldata.Replace("{{penal_interest}}", replacetext);

                                    replacetext = $" {leadLoandata.emi_amount} ";
                                    htmldata = htmldata.Replace("{{emi_amount}}", replacetext);


                                    DateTime dt = DateTime.ParseExact(leadLoandata.loan_app_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    replacetext = $" {dt.Date.ToString("dd-MM-yyyy")} ";
                                    htmldata = htmldata.Replace("{{loan_app_date}}", replacetext);


                                    int Tenuare = 0;
                                    double MonthlyEmiAMount = 0;
                                    double SanctionAmount = 0;
                                    double PfAmount = 0;
                                    int InsuranceAmount = 0;
                                    double DocumentCharges = 0;

                                    Tenuare = Convert.ToInt32(arthmateupdate.Tenure);
                                    MonthlyEmiAMount = Convert.ToDouble(leadLoandata.emi_amount);
                                    SanctionAmount = leadCoLenderResponse.SanctionAmount;
                                    PfAmount = processingfeesamount;
                                    InsuranceAmount = Convert.ToInt32(leadLoandata.insurance_amount);
                                    DocumentCharges = 0;

                                    double APR = Financial.Rate(Tenuare, (-MonthlyEmiAMount), SanctionAmount - PfAmount - InsuranceAmount - DocumentCharges) * 12 * 100;

                                    replacetext = $" {Math.Round(APR, 2)} ";
                                    htmldata = htmldata.Replace("{{APR}}", replacetext);
                                }
                                //repayment schedule data
                                if (response != null)
                                {
                                    List<DataTable> dt = new List<DataTable>();
                                    DataTable Repyment_Scdule = new DataTable();
                                    Repyment_Scdule.TableName = "Repyment_Scdule";
                                    dt.Add(Repyment_Scdule);
                                    Repyment_Scdule.Columns.Add("Instalment No");
                                    Repyment_Scdule.Columns.Add("Outstanding Principal Amount(in Rupees)");
                                    Repyment_Scdule.Columns.Add("Instalment(in Rupees)");//emi_amount
                                    Repyment_Scdule.Columns.Add("Interest(in Rupees)");//int_amount
                                    Repyment_Scdule.Columns.Add("Principal(in Rupees)");
                                    foreach (var item in response.data.rows)
                                    {
                                        var dr = Repyment_Scdule.NewRow();
                                        dr["Instalment No"] = item.emi_no;
                                        dr["Outstanding Principal Amount(in Rupees)"] = item.principal_outstanding;
                                        dr["Instalment(in Rupees)"] = item.emi_amount;
                                        dr["Interest(in Rupees)"] = item.int_amount;
                                        dr["Principal(in Rupees)"] = item.prin;
                                        Repyment_Scdule.Rows.Add(dr);

                                    }
                                    var htmltable = ConvertDataTableToHTML(Repyment_Scdule);

                                    replacetext = $" {htmltable} ";
                                    htmldata = htmldata.Replace("{{DYTable}}", replacetext);

                                    lbabusinessloan.Htmldata = htmldata;

                                }
                            }
                        }
                        //if (IsSubmit && !loanConfig.IseSignEnable)
                        //{
                        //    lead.IsAgreementAccept = true;
                        //    lead.AgreementDate = currentDateTime;
                        //    _context.Entry(lead).State = EntityState.Modified;
                        //    _context.SaveChanges();
                        //    _leadNBFCSubActivityManager.UpdateLeadProgressStatus(leadid, ActivityConstants.ArthmateAgreement);
                        //}
                    }
                }
            }
            return lbabusinessloan;
        }

        //new for Esign
        public async Task<string> AgreementEsign(GRPCRequest<EsignAgreementRequest> req) //, long leadid, string AgreementURL, bool IsSubmit
        {
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

            //DateTime currentDateTime = DateTime.Now;
            int day = currentDateTime.Day;
            string monthAbbreviated = currentDateTime.ToString("MMM");
            int year = currentDateTime.Year;

            string htmldata = "";
            string replacetext = "";

            //var lead = await _context.Leads.Where(x => x.Id == req.Request.LeadId && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            //var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            //var leadBusinessDetails = await _context.BusinessDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);

            if (req != null && req.Request.DSAPersonalDetail != null || req.Request.ConnectorPersonalDetail != null)
            {

                byte[] file = null;
                using (var wc = new WebClient())
                {
                    file = wc.DownloadData(req.Request.AgreementURL);
                }

                if (file.Length > 0)
                {
                    htmldata = System.Text.Encoding.UTF8.GetString(file);

                    if (!string.IsNullOrEmpty(htmldata))
                    {
                        if (req.Request.DSAPersonalDetail != null)
                        {
                            string AdressFromAadhar = string.Concat(req.Request.DSAPersonalDetail.PermanentAddress.AddressLineOne, req.Request.DSAPersonalDetail.PermanentAddress.AddressLineTwo,
                       req.Request.DSAPersonalDetail.PermanentAddress.AddressLineThree, " ", req.Request.DSAPersonalDetail.PermanentAddress.CityName, " ",
                       req.Request.DSAPersonalDetail.PermanentAddress.StateName, " ", req.Request.DSAPersonalDetail.PermanentAddress.ZipCode);

                            string businessAdress = string.Concat(req.Request.DSAPersonalDetail.CurrentAddress.AddressLineOne, req.Request.DSAPersonalDetail.CurrentAddress.AddressLineTwo,
                                req.Request.DSAPersonalDetail.CurrentAddress.AddressLineThree, req.Request.DSAPersonalDetail.CurrentAddress.CityName, req.Request.DSAPersonalDetail.CurrentAddress.StateName, " ",
                               req.Request.DSAPersonalDetail.CurrentAddress.CountryName, " ", req.Request.DSAPersonalDetail.CurrentAddress.ZipCode);

                            //if (req.Request.IsSubmit)
                            //{
                            //    string footerHtml = "<footer>\r\n          <div style=\" position: fixed;\r\n        left: 0;\r\n        margin-bottom: 10%;\r\n        width: 100%;\r\n      text-align: center;\"> </div>\r\n        \r\n  <p></p>\r\n  <p style=\"margin-bottom:3cm\" ></p>\r\n</footer> ";

                            //    replacetext = $"{footerHtml} ";
                            //    htmldata = htmldata.Replace("[footerHtml]", replacetext);
                            //}
                            //else
                            //{
                            //    string footerHtml = "";
                            //    replacetext = $"{footerHtml} ";
                            //    htmldata = htmldata.Replace("[footerHtml]", replacetext);
                            //}
                            //string imgUrl = "https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/b9e430c8-7409-4d0a-9615-3f7f174e9bb1.png";

                            //// string imgUrl = "https://www.printforfun.sg/image/catalog/logo-lighting.png";
                            ////string footerHtml = "<footer>\r\n          <div style=\" position: fixed;\r\n        left: 0;\r\n        margin-bottom: 10%;\r\n        width: 100%;\r\n      text-align: center;\"> </div>\r\n        \r\n  <p></p>\r\n  <p style=\"margin-bottom:3cm\" ></p>\r\n</footer> ";
                            ////new code
                            //string footerHtml = "<footer style=\"display: flex; justify-content: flex-end; align-items: center;\"><img src=\"{imgUrl}\" alt=\"Logo\" width=\"160\" height=\"130\"style=\"margin-right: 10px;\"></footer>";
                            //footerHtml = footerHtml.Replace("{imgUrl}", imgUrl);

                            //replacetext = $"{footerHtml} ";
                            //htmldata = htmldata.Replace("[footerHtml]", replacetext);
                            ////new code end


                            // string imgUrl = "https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/b9e430c8-7409-4d0a-9615-3f7f174e9bb1.png";
                            string imgUrl = "https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/2405c419-ea87-45b2-bdff-753591e7821c.png";
                            string SignImageBase64 = await FileSaverHelper.GetStreamFromUrlBase64(imgUrl);


                            string footerHtml = "<footer style=\"display: flex; justify-content: flex-end; align-items: center;\"><img src=\"data:image/png;base64,{imgUrl}\" alt=\"Logo\" width=\"160\" height=\"130\"style=\"margin-right: 10px;\"></footer>";
                            footerHtml = footerHtml.Replace("{imgUrl}", SignImageBase64);

                            replacetext = $"{footerHtml} ";
                            htmldata = htmldata.Replace("[footerHtml]", replacetext);


                            replacetext = $"{day} ";
                            htmldata = htmldata.Replace("[Date]", replacetext);

                            replacetext = $"{monthAbbreviated} ";
                            htmldata = htmldata.Replace("[Month]", replacetext);

                            replacetext = $"{year} ";
                            htmldata = htmldata.Replace("[Year]", replacetext);

                            //name
                            replacetext = $"{req.Request.DSAPersonalDetail.FullName} ";
                            htmldata = htmldata.Replace("[FirstNameOfBorrower]", replacetext);

                            replacetext = $"{""}"; //&nbsp;
                            htmldata = htmldata.Replace("[MiddleNameOfBorrower]", replacetext);

                            replacetext = $"{""}";
                            htmldata = htmldata.Replace("[LastNameOfBorrower]", replacetext);

                            //address
                            replacetext = $"{AdressFromAadhar} ";
                            htmldata = htmldata.Replace("[PermanentAddressLineOne]", replacetext);

                            replacetext = $"{req.Request.DSAPersonalDetail.CompanyName} ";
                            htmldata = htmldata.Replace("[businessname]", replacetext);

                            replacetext = $"{businessAdress} ";
                            htmldata = htmldata.Replace("[BusinessAdress]", replacetext);

                            replacetext = $"{req.Request.DSAPersonalDetail.City} ";
                            htmldata = htmldata.Replace("[city]", replacetext);

                            //dsa Payout
                            //replacetext = $"{req.Request.PayoutPercentage ?? 0} ";
                            //htmldata = htmldata.Replace("[ConnectorPayout]", replacetext);

                            if (req.Request?.SalesAgentCommissions != null && req.Request.SalesAgentCommissions.Any())
                            {
                                List<DataTable> dt = new List<DataTable>();
                                DataTable payOutTable = new DataTable();
                                payOutTable.TableName = "payOutTable";
                                dt.Add(payOutTable);
                                payOutTable.Columns.Add("Type Of Loan");
                                payOutTable.Columns.Add("Monthly Disbursment");
                                payOutTable.Columns.Add("Base Rate(Inclusive TDS)");
                                payOutTable.Columns.Add("Special Incentive");
                                payOutTable.Columns.Add("Total Payout");
                                var typeIndex = 0;
                                foreach (var item in req.Request.SalesAgentCommissions)
                                {
                                    var dr = payOutTable.NewRow();
                                    if (typeIndex == 0)
                                    {
                                        dr["Type Of Loan"] = "Business Loan";
                                        typeIndex = 1;
                                    }
                                    dr["Monthly Disbursment"] = item.MinAmount + "-" + item.MaxAmount;
                                    dr["Base Rate(Inclusive TDS)"] = item.PayoutPercentage;
                                    dr["Special Incentive"] = " ";
                                    dr["Total Payout"] = item.PayoutPercentage;

                                    payOutTable.Rows.Add(dr);

                                }
                                var htmltable = OfferEMIDataTableToHTML1(payOutTable);

                                replacetext = $" {htmltable} ";
                                htmldata = htmldata.Replace("[DYTable]", replacetext);

                                // lbabusinessloan.Htmldata = htmldata;

                            }


                            replacetext = $"{req.Request.DSAPersonalDetail.EmailId} ";
                            htmldata = htmldata.Replace("[Email]", replacetext);

                            //Any extra Fields if Any
                            replacetext = $"{req.Request.DSAPersonalDetail.FirmType} ";
                            htmldata = htmldata.Replace("[FirmType]", replacetext);

                        }
                        else if (req.Request.ConnectorPersonalDetail != null)
                        {

                            string AdressFromAadhars = string.Concat(req.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineOne, req.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineTwo,
                       req.Request.ConnectorPersonalDetail.CurrentAddress.AddressLineThree, req.Request.ConnectorPersonalDetail.CurrentAddress.CityName, " ",
                       req.Request.ConnectorPersonalDetail.CurrentAddress.StateName, "", req.Request.ConnectorPersonalDetail.CurrentAddress.ZipCode);


                            //if (req.Request.IsSubmit)
                            //{
                            //    string footerHtml = "<footer>\r\n          <div style=\" position: fixed;\r\n        left: 0;\r\n        margin-bottom: 3cm;\r\n        width: 100%;\r\n     text-align: center;\"> </div>\r\n        \r\n  <p></p>\r\n  <p style=\"margin-bottom:3cm;\" ></p>\r\n</footer> ";

                            //    replacetext = $"{footerHtml} ";
                            //    htmldata = htmldata.Replace("[footerHtml]", replacetext); ;
                            //}
                            //else
                            //{
                            //    string footerHtml = "";
                            //    replacetext = $"{footerHtml} ";
                            //    htmldata = htmldata.Replace("[footerHtml]", replacetext);
                            //}


                            //string imgUrl = "https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/b9e430c8-7409-4d0a-9615-3f7f174e9bb1.png";
                            //string footerHtml = "<footer style=\"display: flex; justify-content: flex-end; align-items: center;\"><img src=\"{imgUrl}\" alt=\"Logo\" width=\"160\" height=\"130\"style=\"margin-right: 10px;\"></footer>";
                            //footerHtml = footerHtml.Replace("{imgUrl}", imgUrl);

                            //replacetext = $"{footerHtml} ";
                            //htmldata = htmldata.Replace("[footerHtml]", replacetext);

                            //string imgUrl = "https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/b9e430c8-7409-4d0a-9615-3f7f174e9bb1.png";
                            string imgUrl = "https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/2405c419-ea87-45b2-bdff-753591e7821c.png";
                            string SignImageBase64 = await FileSaverHelper.GetStreamFromUrlBase64(imgUrl);


                            string footerHtml = "<footer style=\"display: flex; justify-content: flex-end; align-items: center;\"><img src=\"data:image/png;base64,{imgUrl}\" alt=\"Logo\" width=\"160\" height=\"130\"style=\"margin-right: 10px;\"></footer>";
                            footerHtml = footerHtml.Replace("{imgUrl}", SignImageBase64);

                            replacetext = $"{footerHtml} ";
                            htmldata = htmldata.Replace("[footerHtml]", replacetext);

                            replacetext = $"{day} ";
                            htmldata = htmldata.Replace("[Date]", replacetext);

                            replacetext = $"{monthAbbreviated} ";
                            htmldata = htmldata.Replace("[Month]", replacetext);

                            replacetext = $"{year} ";
                            htmldata = htmldata.Replace("[Year]", replacetext);

                            //name
                            replacetext = $"{req.Request.ConnectorPersonalDetail.FullName} ";
                            htmldata = htmldata.Replace("[FirstNameOfBorrower]", replacetext);

                            replacetext = $"{""}&nbsp;";
                            htmldata = htmldata.Replace("[MiddleNameOfBorrower]", replacetext);

                            replacetext = $"{""} ";
                            htmldata = htmldata.Replace("[LastNameOfBorrower]", replacetext);

                            //address
                            replacetext = $"{AdressFromAadhars} ";
                            htmldata = htmldata.Replace("[PermanentAddressLineOne]", replacetext);

                            replacetext = $"{""} ";
                            htmldata = htmldata.Replace("[PermanentAddressLineTwo]", replacetext);

                            replacetext = $"{""} ";
                            htmldata = htmldata.Replace("[PermanentAddressLineThree]", replacetext);

                            //Connector Payout
                            // replacetext = $"{req.Request.PayoutPercentage ?? 0} ";
                            htmldata = htmldata.Replace("[ConnectorPayout]", replacetext);

                            //Any extra Fields if Any
                            replacetext = $"{req.Request.ConnectorPersonalDetail.EmailId} ";
                            htmldata = htmldata.Replace("[Email]", replacetext);


                            if (req.Request?.SalesAgentCommissions != null && req.Request.SalesAgentCommissions.Any())
                            {
                                List<DataTable> dt = new List<DataTable>();
                                DataTable payOutTable = new DataTable();
                                payOutTable.TableName = "payOutTable";
                                dt.Add(payOutTable);
                                payOutTable.Columns.Add("Type Of Loan");
                                payOutTable.Columns.Add("Monthly Disbursment");
                                payOutTable.Columns.Add("Base Rate(Inclusive TDS)");
                                payOutTable.Columns.Add("Special Incentive");
                                payOutTable.Columns.Add("Total Payout");
                                var typeIndex = 0;
                                foreach (var item in req.Request.SalesAgentCommissions)
                                {
                                    var dr = payOutTable.NewRow();
                                    if (typeIndex == 0)
                                    {
                                        dr["Type Of Loan"] = "Business Loan";
                                        typeIndex = 1;
                                    }
                                    dr["Monthly Disbursment"] = item.MinAmount + "-" + item.MaxAmount;
                                    dr["Base Rate(Inclusive TDS)"] = item.PayoutPercentage;
                                    dr["Special Incentive"] = " ";
                                    dr["Total Payout"] = item.PayoutPercentage;

                                    payOutTable.Rows.Add(dr);

                                }
                                var htmltable = OfferEMIDataTableToHTML1(payOutTable);

                                replacetext = $" {htmltable} ";
                                htmldata = htmldata.Replace("[DYTable]", replacetext);


                                // lbabusinessloan.Htmldata = htmldata;

                            }




                        }
                    }
                }
                //if (req.Request.IsSubmit)
                //{
                //    lead.IsAgreementAccept = true;
                //    lead.AgreementDate = currentDateTime;
                //    _context.Entry(lead).State = EntityState.Modified;
                //    _context.SaveChanges();
                //    _leadNBFCSubActivityManager.UpdateLeadProgressStatus(req.Request.LeadId, ActivityConstants.ArthmateAgreement);
                //}
            }
            return htmldata;
        }

        public async Task<CommonResponseDc> GetLoanByLoanId(long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            var data = _context.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

            if (data != null)
            {
                var Loanapi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateLoanGenerate && x.IsActive && !x.IsDeleted);
                var Response = await _ArthMateNBFCHelper.GetLoanById(data.loan_id, Loanapi.APIUrl, Loanapi.TAPIKey, Loanapi.TAPISecretKey, Loanapi.Id, Leadmasterid);
                _context.ArthMateCommonAPIRequestResponses.Add(Response);
                _context.SaveChanges();
                if (Response.IsSuccess)
                {
                    var getloandata = JsonConvert.DeserializeObject<GetLoanDetailsDc>(Response.Response);
                    res.Data = getloandata;
                    if (getloandata != null)
                    {
                        res.Msg = getloandata.message;

                        if (getloandata.loanDetails != null && data.status != getloandata.loanDetails.status)
                        {
                            data.status = getloandata.loanDetails.status;
                            _context.Entry(data).State = EntityState.Modified;
                        }
                    }
                    _context.SaveChanges();
                }
            }

            return res;
        }

        public async Task<CommonResponseDc> GetDisbursementAPI(long Leadmasterid)
        {
            var result = new CommonResponseDc();
            if (Leadmasterid > 0)
            {
                var Loan = _context.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (Loan != null)
                {
                    DateConvertHelper _DateConvertHelper = new DateConvertHelper();
                    var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

                    var ArthmateDisbursement = _context.ArthmateDisbursements.Where(x => x.loan_id == Loan.loan_id).FirstOrDefault();
                    if (ArthmateDisbursement != null)
                    {
                        result.Data = ArthmateDisbursement;
                        result.Status = true;
                    }
                    else
                    {
                        var GetDisbursementApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateGetDisbursement && x.IsActive && !x.IsDeleted);
                        var Result = await _ArthMateNBFCHelper.GetDisbursementAPI(Loan.loan_id, GetDisbursementApi.APIUrl, GetDisbursementApi.TAPIKey, GetDisbursementApi.TAPISecretKey, GetDisbursementApi.Id, Leadmasterid);
                        _context.ArthMateCommonAPIRequestResponses.Add(Result);
                        _context.SaveChanges();
                        if (Result.IsSuccess)
                        {
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
                                result.Data = arthmateDisbursement;
                                result.Status = true;
                            }
                            else
                            {
                                result.Msg = resdata.message;
                            }
                        }
                    }
                }
                else
                {
                    result.Msg = "Loan Not Generated";
                    result.Status = false;
                }

            }
            return result;
        }
        public async Task<List<LeadLoanDataDc>> GetLoan(long LeadMasterId)
        {
            var Leadid = new SqlParameter("@leadmasterid", LeadMasterId);
            List<LeadLoanDataDc> leadLoanDataDc = _context.Database.SqlQueryRaw<LeadLoanDataDc>("exec GetLoanData @leadmasterid", Leadid).AsEnumerable().ToList();
            return leadLoanDataDc;
        }
        public async Task<CommonResponseDc> LoanRepaymentScheduleDetails(long LeadMasterId)
        {
            CommonResponseDc result = new CommonResponseDc { Msg = "Data Not Found" };
            var Loan = await _context.LeadLoan.Where(x => x.LeadMasterId == LeadMasterId).FirstOrDefaultAsync();
            if (Loan != null)
            {
                var Postrepayment_schedule = new Postrepayment_scheduleDc
                {
                    loan_id = Loan.loan_id,
                    product_id = Loan.product_id,
                    company_id = Loan.company_id ?? 0,
                };
                var repaymentScheduleApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateRepaymentSchedule && x.IsActive && !x.IsDeleted);
                var Response = await _ArthMateNBFCHelper.repayment_schedule(Postrepayment_schedule, repaymentScheduleApi.APIUrl, repaymentScheduleApi.TAPIKey, repaymentScheduleApi.TAPISecretKey, repaymentScheduleApi.Id, LeadMasterId);
                _context.ArthMateCommonAPIRequestResponses.Add(Response);
                _context.SaveChanges();
                result.Status = Response.IsSuccess;
                if (Response.IsSuccess)
                {
                    var RepaymentAllResponse = JsonConvert.DeserializeObject<repay_scheduleDc>(Response.Response);
                    if (Loan.first_inst_date != null && RepaymentAllResponse != null && RepaymentAllResponse.data != null && RepaymentAllResponse.data.rows != null && RepaymentAllResponse.data.rows.Any())
                    {
                        result.Data = RepaymentAllResponse.data.rows.Select(x => new LeadRepaymentScheduleDetailDc
                        {
                            company_id = x.company_id,
                            due_date = Loan.first_inst_date.Value.AddMonths(x.emi_no - 1),
                            emi_amount = x.emi_amount,
                            emi_no = x.emi_no,
                            int_amount = x.int_amount,
                            prin = x.prin,
                            principal_bal = x.principal_bal,
                            principal_outstanding = x.principal_outstanding,
                            product_id = x.product_id,
                            repay_schedule_id = x.repay_schedule_id
                        });
                        result.Msg = RepaymentAllResponse.message;
                    }
                }
            }
            return result;
        }
        public async Task<CommonResponseDc> LoanNach(string UMRN, long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();

            var data = _context.LeadLoan.Where(x => x.LeadMasterId == Leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            if (data != null)
            {
                data.UMRN = UMRN;
                _context.Entry(data).State = EntityState.Modified;
                _context.SaveChanges();

                LoanNachAPIDc obj = new LoanNachAPIDc
                {
                    umrn = UMRN,
                };
                var LoanNachPatchapi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateLoanNachPatch && x.IsActive && !x.IsDeleted);

                var Result = await _ArthMateNBFCHelper.LoanNachPatchAPI(obj, data.loan_id, LoanNachPatchapi.APIUrl, LoanNachPatchapi.TAPIKey, LoanNachPatchapi.TAPISecretKey, LoanNachPatchapi.Id, Leadmasterid);
                _context.ArthMateCommonAPIRequestResponses.Add(Result);
                _context.SaveChanges();
                if (Result.IsSuccess && !string.IsNullOrEmpty(Result.Response))
                {
                    var resdata = JsonConvert.DeserializeObject<LoanNachResponseDC>(Result.Response);
                    if (resdata != null && resdata.success)
                    {
                        res.Msg = resdata.message;
                        res.Status = resdata.success;
                        if (resdata.success)
                        {
                            data.UMRN = UMRN;
                            _context.Entry(data).State = EntityState.Modified;
                            _context.SaveChanges();
                        }
                    }
                }
            }
            else
            {
                res.Msg = "Data Not Found in Loan!";
            }

            return res;
        }
        public async Task<CommonResponseDc> ChangeLoanStatus(long LeadMasterId, string Status)
        {

            //string ReturnStatus = "";
            int userid = 0;
            CommonResponseDc result = new CommonResponseDc();
            var Loan = await _context.LeadLoan.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
            var Loanapi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateLoanGenerate && x.IsActive && !x.IsDeleted);
            var Result = await _ArthMateNBFCHelper.GetLoanById(Loan.loan_id, Loanapi.APIUrl, Loanapi.TAPIKey, Loanapi.TAPISecretKey, Loanapi.Id, LeadMasterId);
            _context.ArthMateCommonAPIRequestResponses.Add(Result);
            _context.SaveChanges();
            if (Result.IsSuccess)
            {
                var LaonStatusApi = JsonConvert.DeserializeObject<GetLoanDetailsDc>(Result.Response);

                string LaonStatus = LaonStatusApi.loanDetails.status;
                if (LaonStatus == "validation_error")
                {
                    LaonStatus = LaonStatusApi.loanDetails.status;
                    result.Msg = "Status is " + LaonStatus + " and " + LaonStatusApi.loanDetails.validations.FirstOrDefault().remarks;
                }
                else
                {

                    if (LaonStatus == "open")
                    {
                        var LoanStatusChangeAPI = new LoanStatusChangeAPIReq
                        {
                            loan_id = Loan.loan_id,
                            partner_borrower_id = Loan.partner_borrower_id,
                            partner_loan_id = Loan.partner_loan_id,
                            status = Status,
                            borrower_id = Loan.borrower_id,
                            loan_app_id = Loan.loan_app_id,
                            partner_loan_app_id = Loan.partner_loan_app_id
                        };
                        var statusapi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateUpdateLoanStatus && x.IsActive && !x.IsDeleted);
                        string APIUrl = statusapi.APIUrl.Replace("{loan_id}", Loan.loan_id);
                        Result = await _ArthMateNBFCHelper.borrowerinfostatusupdate(LoanStatusChangeAPI, APIUrl, statusapi.TAPIKey, statusapi.TAPISecretKey, statusapi.Id, LeadMasterId);
                        _context.ArthMateCommonAPIRequestResponses.Add(Result);
                        _context.SaveChanges();
                        if (Result.IsSuccess)
                        {
                            var LoanStatusChangeAPIresponse = JsonConvert.DeserializeObject<LoanStatusChangeAPIRes>(Result.Response);
                            if (LoanStatusChangeAPIresponse != null)
                            {
                                result.Data = LoanStatusChangeAPIresponse;
                                result.Msg = LoanStatusChangeAPIresponse.message;
                                result.Status = true;
                                if (LoanStatusChangeAPIresponse.message == "Status Changed" || LoanStatusChangeAPIresponse.message == "Loan status updated to kyc_data_approved successfully.")
                                {
                                    Loan.status = Status;
                                    _context.Entry(Loan).State = EntityState.Modified;
                                    _context.SaveChanges();

                                }
                            }
                        }
                    }
                    else
                    {
                        if (LaonStatusApi.loanDetails != null && LaonStatusApi.loanDetails.validations != null && LaonStatusApi.loanDetails.validations.Any())
                        {
                            LaonStatus = LaonStatusApi.loanDetails.status;
                            result.Msg = "Status is " + LaonStatus + " and " + LaonStatusApi.loanDetails.validations.FirstOrDefault().remarks;
                        }
                        else
                        {
                            result.Msg = "Status is " + LaonStatus;
                        }
                    }
                }
            }

            return result;
        }

        public async Task<CommonResponseDc> SaveAgreementESignDocument(long leadmasterid, string eSignDocumentURL)
        {
            CommonResponseDc result = new CommonResponseDc();
            var ArthMateUpdateData = await _context.ArthMateUpdates.Where(x => x.LeadId == leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
            var leadagreement = await _context.leadAgreements.Where(x => x.LeadId == leadmasterid && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();

            if (ArthMateUpdateData != null && leadagreement != null)
            {
                ArthMateUpdateData.AgreementESignDocumentURL = eSignDocumentURL;
                _context.Entry(ArthMateUpdateData).State = EntityState.Modified;

                leadagreement.DocSignedUrl = eSignDocumentURL;
                leadagreement.Status = "Signed";
                leadagreement.LastModified = DateTime.Now;
                _context.Entry(leadagreement).State = EntityState.Modified;

                _context.SaveChanges();

                var LeadLoandata = _context.LeadLoan.Where(x => x.LeadMasterId == leadmasterid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (LeadLoandata != null)
                {
                    if (LeadLoandata.status != LoanStatus)//"kyc_data_approved"
                    {
                        result.Msg = "Documnet Not Uploaded due to Loan stats should be 'kyc_data_approved'";
                        return result;
                    }
                }

                var arthmateSlaLbaStamp = _context.ArthmateSlaLbaStampDetail.FirstOrDefault(x => x.LeadmasterId == leadmasterid && x.IsActive && x.IsDeleted == false);
                if (arthmateSlaLbaStamp != null)
                {
                    arthmateSlaLbaStamp.IsStampUsed = true;
                    arthmateSlaLbaStamp.DateofUtilisation = DateTime.Now;
                    arthmateSlaLbaStamp.LoanId = LeadLoandata.loan_id;
                    _context.Entry(arthmateSlaLbaStamp).State = EntityState.Modified;
                }
                if (LeadLoandata != null)
                {
                    //LeadLoandata.UrlSlaDocument = LogoUrl;//customer sla Document Accept copy pdf
                    LeadLoandata.UrlSlaUploadSignedDocument = eSignDocumentURL;//customer sla Document signed copy pdf
                    _context.Entry(LeadLoandata).State = EntityState.Modified;
                }
                _context.SaveChanges();

                //post Doc To Arthmate 

                var DocStatus = await ArthmatePostDoc(leadmasterid, sLoan_sanction_letter);
                var data = await ArthmatePostDoc(leadmasterid, sSigned_loan_sanction_letter);
                //_context.SaveChanges();
                result.Status = true;
                result.Msg = "Document Saved";

                //------------------S : Make log---------------------
                //NOTE: Testing is Pending.
                #region Make History
                var resultHistory = await _leadHistoryManager.GetLeadHistroy(leadmasterid, "SaveAgreementESignDocument_ManuallyEsignArthmate");
                //LeadUpdateHistoryEvent histroyEvent = new LeadUpdateHistoryEvent
                //{
                //    LeadId = leadmasterid,
                //    UserID = resultHistory.UserId,
                //    UserName = "",
                //    EventName = "Agreement Esign Manually-Arthmate ",//context.Message.KYCMasterCode, //result.EntityIDofKYCMaster.ToString(),
                //    Narretion = resultHistory.Narretion,
                //    NarretionHTML = resultHistory.NarretionHTML,
                //    CreatedTimeStamp = resultHistory.CreatedTimeStamp
                //};
                //await _massTransitService.Publish(histroyEvent);
                #endregion
                //------------------E : Make log---------------------
            }
            return result;
        }
        public async Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            RePaymentScheduleDataDc response = new RePaymentScheduleDataDc();

            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == Leadmasterid && x.IsActive && !x.IsDeleted);

            if (lead != null)
            {
                var leadoffer = _context.LeadOffers.FirstOrDefault(x => x.LeadId == Leadmasterid && x.NBFCCompanyId == lead.OfferCompanyId);
                var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == Leadmasterid && x.IsActive && !x.IsDeleted);

                if (leadoffer != null && leadPersonalDetails != null)
                {
                    if (leadoffer.CompanyIdentificationCode == CompanyIdentificationCodeConstants.ArthMate)
                    {
                        var leadid = new SqlParameter("@LeadMasterId", Leadmasterid);
                        var result = _context.Database.SqlQueryRaw<DisbursedDetailDc>("exec GetDisbursedLoanDetail @LeadMasterId", leadid).AsEnumerable().FirstOrDefault();

                        if (result != null)
                        {
                            Postrepayment_scheduleDc reqdata = new Postrepayment_scheduleDc()
                            {
                                company_id = result.company_id,
                                loan_id = result.loan_id,
                                product_id = result.product_id
                            };

                            var repaymentScheduleApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateRepaymentSchedule && x.IsActive && !x.IsDeleted);
                            var Response = await _ArthMateNBFCHelper.repayment_schedule(reqdata, repaymentScheduleApi.APIUrl, repaymentScheduleApi.TAPIKey, repaymentScheduleApi.TAPISecretKey, repaymentScheduleApi.Id, Leadmasterid);
                            _context.ArthMateCommonAPIRequestResponses.Add(Response);
                            _context.SaveChanges();
                            if (Response.IsSuccess)
                            {
                                var res = JsonConvert.DeserializeObject<repay_scheduleDc>(Response.Response);
                                if (res != null && res.success && res.data.rows.Count > 0)
                                {
                                    List<repay_scheduleDetails> list = new List<repay_scheduleDetails>(res.data.rows);
                                    response.rows = list;
                                    response.MonthlyEMI = result.MonthlyEMI;
                                    response.InsurancePremium = result.InsurancePremium;
                                    response.sanction_amount = result.sanction_amount;
                                    response.borro_bank_acc_num = result.borro_bank_acc_num;
                                    response.loan_int_amt = result.loan_int_amt;
                                }
                            }
                        }
                    }
                    if (leadoffer.CompanyIdentificationCode == CompanyIdentificationCodeConstants.AYEFIN)
                    {
                    }
                    if (leadoffer.CompanyIdentificationCode == CompanyIdentificationCodeConstants.MASFIN)
                    {
                    }
                }
            }



            return response;
        }
        public async Task<double> GetMonthlySalary(long LeadMasterId)
        {
            try
            {
                double MonthlySalary = 0;
                var ceplr_cust = _context.CeplrPdfReports.Where(x => x.LeadMasterId == LeadMasterId && x.IsActive == true && x.IsDeleted == false).OrderByDescending(x => x.Id).FirstOrDefault();
                if (ceplr_cust != null)
                {
                    CeplrBasicReportDc CplrBasic = new CeplrBasicReportDc();
                    CplrBasic.start_date = ceplr_cust.Created.AddMonths(-6).AddDays(-10).ToString("yyyy-MM-dd");
                    CplrBasic.end_date = DateTime.Now.ToString("yyyy-MM-dd");

                    var apis = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateGetBasicReports && x.IsActive && !x.IsDeleted);
                    string APIUrl = apis.APIUrl.Replace("{{customer_uuid}}", ceplr_cust.customer_id);
                    var tokenDetails = await _context.NBFCApiTokens.Where(x => x.IdentificationCode == CompanyIdentificationCodeConstants.ArthMate && x.TokenType == NBFCApiTokenTypeConstants.CeplarKey && x.IsActive).FirstOrDefaultAsync();
                    var Result = await _ArthMateNBFCHelper.CeplrBasicReport(CplrBasic, APIUrl, tokenDetails.TokenValue, apis.TAPISecretKey, apis.Id, LeadMasterId);
                    _context.ArthMateCommonAPIRequestResponses.Add(Result);
                    _context.SaveChanges();
                    if (Result.IsSuccess)
                    {
                        var BasicReport = JsonConvert.DeserializeObject<CeplrBasicReportResponse>(Result.Response);
                        if (BasicReport != null)
                        {
                            if (BasicReport.data != null)
                            {
                                MonthlySalary = Convert.ToDouble(BasicReport.data[0].analytics.salary_summary.total_salary);
                            }
                        }
                    }
                }
                return MonthlySalary;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
        public static string OfferEMIDataTableToHTML(DataTable dt)
        {
            string html = "<table style=\"width:100%;\">";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th>" + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td style=\"text-align: center;\">" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        public static string OfferEMIDataTableToHTML1(DataTable dt)
        {
            string html = "<table style=\"width:100%; border-collapse: collapse; border: 1px solid black;\">";
            // Add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th style=\"border: 1px solid black;\">" + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            // Add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td style=\"text-align: center; border: 1px solid black;\">" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        //public static string OfferEMIDataTableToHTML1(DataTable dt)
        //{
        //    if (dt == null || dt.Rows.Count == 0)
        //        return "<table style=\"width:100%; border-collapse: collapse; border: 1px solid black;\"><tr><td>No data available</td></tr></table>";

        //    // Create a StringBuilder for efficient string manipulation   
        //    var html = new StringBuilder();

        //    html.Append("<table style=\"width:100%; border-collapse: collapse; border: 1px solid black;\">");

        //    // Add header row
        //    html.Append("<tr>");
        //    for (int i = 0; i < dt.Columns.Count; i++)
        //        html.Append("<th style=\"border: 1px solid black;\">" + dt.Columns[i].ColumnName + "</th>");
        //    html.Append("</tr>");

        //    // Track the last loan type and the number of rows with the same type
        //    string lastLoanType = null;
        //    int rowspan = 0;

        //    // Add rows
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        string currentLoanType = dt.Rows[i]["Type Of Loan"].ToString();

        //        if (currentLoanType != lastLoanType)
        //        {
        //            // If the loan type has changed, add the previous row type's rowspan
        //            if (rowspan > 0)
        //            {
        //                html.Append("<td style=\"text-align: center; border: 1px solid black;\" rowspan=\"" + rowspan + "\">" + lastLoanType + "</td>");
        //                rowspan = 0;
        //            }

        //            // Update the last loan type and set a new rowspan
        //            lastLoanType = currentLoanType;
        //        }

        //        // Update the rowspan counter
        //        rowspan++;

        //        // Add the current row's cells
        //        html.Append("<tr>");
        //        html.Append("<td style=\"text-align: center; border: 1px solid black;\">" + currentLoanType + "</td>");
        //        for (int j = 1; j < dt.Columns.Count; j++) // Skip the "LoanType" column
        //        {
        //            html.Append("<td style=\"text-align: center; border: 1px solid black;\">" + dt.Rows[i][j].ToString() + "</td>");
        //        }
        //        html.Append("</tr>");
        //    }

        //    // Add the last row's rowspan
        //    if (rowspan > 0)
        //    {
        //        html.Append("<td style=\"text-align: center; border: 1px solid black;\" rowspan=\"" + rowspan + "\">" + lastLoanType + "</td>");
        //    }

        //    html.Append("</table>");
        //    return html.ToString();
        //}



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

        //Show Offer
        public async Task<CommonResponseDc> GetLeadMasterByLeadId(long leadId)
        {
            CommonResponseDc results = new CommonResponseDc();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == leadId && x.IsActive && !x.IsDeleted);

            if (lead != null)
            {
                var leadoffer = _context.LeadOffers.FirstOrDefault(x => x.LeadId == leadId && x.NBFCCompanyId == lead.OfferCompanyId);
                var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadId && x.IsActive && !x.IsDeleted);

                if (leadoffer != null && leadPersonalDetails != null)
                {
                    if (leadoffer.CompanyIdentificationCode == CompanyIdentificationCodeConstants.ArthMate)
                    {
                        var arthmateUpdateDetails = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadId && x.IsActive && !x.IsDeleted);
                        //var leadPersonalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == leadId && x.IsActive && !x.IsDeleted);
                        if (arthmateUpdateDetails != null)
                        {
                            var offer = _context.CoLenderResponse.Where(x => x.LeadMasterId == leadId && x.IsActive && !x.IsDeleted && x.status == "success").OrderByDescending(x => x.Id).FirstOrDefault();
                            if (offer != null)
                            {
                                var data = new ArthMateOfferDc
                                {
                                    loan_amt = offer.SanctionAmount == 0 ? offer.loan_amount : offer.SanctionAmount,
                                    interest_rt = lead.InterestRate ?? 0,//loanConfig.InterestRate,
                                    loan_tnr = Convert.ToInt32(arthmateUpdateDetails.Tenure) > 0 ? Convert.ToInt32(arthmateUpdateDetails.Tenure) : 36,
                                    loan_tnr_type = "Month",
                                    Orignal_loan_amt = offer.SanctionAmount == 0 ? offer.loan_amount : offer.SanctionAmount, //offer.loan_amount,
                                    Name = leadPersonalDetails.FirstName
                                };
                                results.ArthMateOffer = data;
                                results.NBFCCompanyId = arthmateUpdateDetails.NBFCCompanyId ?? 0;
                            }
                            results.CompanyIdentificationCode = CompanyIdentificationCodeConstants.ArthMate;
                        }
                    }
                    else
                    {
                        var masfin = _context.nbfcOfferUpdate.FirstOrDefault(x => x.LeadId == leadId && x.NBFCCompanyId == lead.OfferCompanyId && x.IsActive && !x.IsDeleted);
                        if (masfin != null)
                        {
                            var data = new ArthMateOfferDc
                            {
                                loan_amt = leadoffer.CreditLimit ?? 0,
                                interest_rt = lead.InterestRate ?? 0,
                                loan_tnr = Convert.ToInt32(masfin.Tenure) > 0 ? Convert.ToInt32(masfin.Tenure) : 36,
                                loan_tnr_type = "Month",
                                Orignal_loan_amt = leadoffer.CreditLimit ?? 0,
                                Name = leadPersonalDetails.FirstName
                            };
                            results.ArthMateOffer = data;
                            results.NBFCCompanyId = masfin.NBFCCompanyId;
                        }

                        results.CompanyIdentificationCode = leadoffer.CompanyIdentificationCode;
                    }
                    results.IsNotEditable = false;
                    results.Status = true;
                }
            }
            return results;
        }


        public async Task<LoanInsuranceConfiguration> GetRateOfInterest(int tenure)
        {
            var InsuranceConfig = await _context.LoanInsuranceConfiguration.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.MonthDuration >= tenure);
            return InsuranceConfig;
        }

        #region Testing Apis
        public async Task<ICreateLeadNBFCResponse> TestArthmateLoanDocument(long leadid, long NBFCCompanyId, bool isTest = false)
        {
            var req = new LeadNBFCSubActivityDTO
            {
                Sequence = 0,
                APIUrl = "https://uat-apiorigin.arthmate.com/api/loandocument",
                TAPIKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb21wYW55X2lkIjo0MTM3Mjg2LCJjb21wYW55X2NvZGUiOiJTSE8wMTYxIiwicHJvZHVjdF9pZCI6NDEzNzYxMSwidXNlcl9pZCI6Mzc1OTU1NSwidXNlcl9uYW1lIjoiU2ltcmFuIFNpbmdoIiwidHlwZSI6ImFwaSIsImxvYW5fc2NoZW1hX2lkIjoiNDEzNzMwMiIsImNyZWRpdF9ydWxlX2dyaWRfaWQiOm51bGwsImF1dG9tYXRpY19jaGVja19jcmVkaXQiOjAsInRva2VuX2lkIjoiNDEzNzI4Ni00MTM3NjExLTE2OTcwOTcyNDY3NTciLCJlbnZpcm9ubWVudCI6InNhbmRib3giLCJpYXQiOjE2OTcwOTcyNDZ9.5I0hEoLGeOtmkQeDKD51mDg6slWkTZOCY30p3F0cQTE"
            };
            var res = await ArthmateLoanDocument(req, leadid, NBFCCompanyId);
            return new CreateLeadNBFCResponse();
        }
        //public async Task<ICreateLeadNBFCResponse> TestLoanGenerate(long leadid, long NBFCCompanyId, bool isTest = false)
        //{
        //    var Loanapi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateLoanGenerate && x.IsActive && !x.IsDeleted);
        //    var api = new LeadNBFCSubActivityDTO
        //    {
        //        Sequence = 0,
        //        APIUrl = Loanapi.APIUrl,
        //        TAPIKey = Loanapi.TAPIKey,
        //        TAPISecretKey = Loanapi.TAPISecretKey,
        //        LeadNBFCApiId = Loanapi.Id
        //    };
        //    var res = await LoanGenerate(api, false, leadid, NBFCCompanyId, 0);
        //    return new CreateLeadNBFCResponse();
        //}
        public async Task<ICreateLeadNBFCResponse> TestMonthlySalary(long leadid)
        {
            var res = await GetMonthlySalary(leadid);
            return new CreateLeadNBFCResponse();
        }

        public Task<ResultViewModel<string>> GetPFCollection(long leadid, string MobileNo)
        {
            throw new NotImplementedException();
        }
        #endregion
        public async Task<CommonResponseDc> ArthmatePostDoc(long leadid, string DocName)
        {
            CommonResponseDc commonResponseDc = new CommonResponseDc();

            string baseBath = _hostingEnvironment.ContentRootPath;
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();
            List<LoanDocumentPostDc> loanDocumentPostDcs = new List<LoanDocumentPostDc>();
            var arthmateUpdateData = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var AgreementURL = await _context.leadAgreements.FirstOrDefaultAsync(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);
            var arthmateDocuments = await _context.ArthMateDocumentMaster.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();

            var query = from a in _context.LeadNBFCSubActivitys
                        join b in _context.LeadNBFCApis on a.Id equals b.LeadNBFCSubActivityId
                        where b.IsActive == true && b.IsDeleted == false
                        && a.LeadId == leadid
                        && a.Code == SubActivityConstants.CreateLead
                        && b.Code == CompanyApiConstants.ArthmateLoanDocument
                        && a.IdentificationCode == CompanyIdentificationCodeConstants.ArthMate
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

            var apis = await query.FirstOrDefaultAsync();
            string fileUrl = "";
            if (DocName == "loan_sanction_letter")
            {
                //fileUrl = arthmateUpdateData.AgreementPdfURL;
                fileUrl = AgreementURL.DocUnSignUrl;
            }
            else
            {
                //fileUrl = arthmateUpdateData.AgreementESignDocumentURL;
                fileUrl = AgreementURL.DocSignedUrl;
            }

            if (arthmateUpdateData != null && arthmateDocuments != null && arthmateDocuments.Any())
            {
                if (!string.IsNullOrEmpty(fileUrl))
                {
                    loanDocumentPostDcs.Add(new LoanDocumentPostDc
                    {
                        base64pdfencodedfile = await ConvertToBase64StringAsync(fileUrl, DocName),
                        file_url = "",
                        FrontUrl = "",
                        LeadMasterId = leadid,
                        loan_app_id = arthmateUpdateData.LoanAppId,
                        partner_borrower_id = arthmateUpdateData.PartnerborrowerId,
                        partner_loan_app_id = arthmateUpdateData.PartnerloanAppId,
                        borrower_id = arthmateUpdateData.borrowerId,
                        PdfPassword = "",
                        code = arthmateDocuments.FirstOrDefault(x => x.DocumentName == DocName).DocumentTypeCode//"099"
                    });
                }
            }
            string basePath = _hostingEnvironment.ContentRootPath;
            if (loanDocumentPostDcs.Any())
            {
                foreach (var loanDocument in loanDocumentPostDcs)
                {
                    ArthMateCommonAPIRequestResponse Result = await _ArthMateNBFCHelper.LoanDocumentApi(loanDocument, apis.APIUrl, apis.TAPIKey, apis.TAPISecretKey, apis.LeadNBFCApiId, leadid, basePath);
                    _context.ArthMateCommonAPIRequestResponses.Add(Result);
                    response.IsSuccess = Result.IsSuccess;
                    if (!response.IsSuccess) break;
                }
                if (response.IsSuccess) _leadNBFCSubActivityManager.UpdateLeadMasterStatus(leadid, LeadBusinessLoanStatusConstants.LoanActivated);
                _context.SaveChanges();

            }
            return commonResponseDc;
        }

        public async Task<CommonResponseDc> LoanDataSave(long leadid, long id)
        {
            CommonResponseDc res = new CommonResponseDc();

            try
            {

                var result = _context.ArthMateCommonAPIRequestResponses.FirstOrDefault(x => x.Id == id).Response;

                //var dd = @"{""success"":true,""message"":""Loan details added successfully"",""data"":[{""selected_events"":[],""co_lender_assignment_id"":9191,""loan_app_id"":""SKUBL-6325696809037"",""loan_id"":""AMLAAYAIR100000005845"",""borrower_id"":""CFUPD9939F"",""partner_loan_app_id"":""LN/2024/51-9486291496"",""partner_loan_id"":""LN/2024/51-9486291496"",""partner_borrower_id"":""LN/2024/51/00001"",""company_id"":4137286,""co_lender_id"":3,""co_lend_flag"":""N"",""product_id"":4137611,""itr_ack_no"":null,""loan_app_date"":""2024-04-25"",""penal_interest"":24,""bounce_charges"":750,""marital_status"":""Married"",""sanction_amount"":""300000"",""gst_on_pf_amt"":""1080"",""gst_on_pf_perc"":""18"",""repayment_type"":""Monthly"",""first_inst_date"":""2024-06-05T00:00:00.000Z"",""net_disbur_amt"":""290052.88"",""final_approve_date"":""2024-04-24T00:00:00.000Z"",""final_remarks"":""Done"",""foir"":""0"",""status"":""batch"",""stage"":905,""upfront_interest"":""0"",""exclude_interest_till_grace_period"":""0"",""borro_bank_account_type"":""Current"",""borro_bank_account_holder_name"":""rajasthan granite an"",""business_vintage_overall"":""2490"",""gst_number"":""06AABFD2364D1ZZ"",""loan_int_amt"":""123714.96"",""loan_int_rate"":""24"",""conv_fees"":""500"",""processing_fees_amt"":""6000"",""processing_fees_perc"":""2"",""tenure"":""36"",""tenure_type"":""Month"",""int_type"":""Reducing"",""borro_bank_ifsc"":""CNRB0018763"",""borro_bank_acc_num"":""120002132414"",""borro_bank_name"":""CANARA BANK"",""first_name"":""SHRIYA"",""last_name"":""SHUKLA"",""ninety_plus_dpd_in_last_24_months"":""0"",""current_overdue_value"":0,""dpd_in_last_9_months"":""0"",""dpd_in_last_3_months"":""0"",""dpd_in_last_6_months"":""0"",""bureau_score"":""-1"",""loan_amount_requested"":""350000"",""insurance_company"":""NA"",""credit_card_settlement_amount"":""0"",""emi_amount"":""11769.86"",""emi_allowed"":""NaN"",""bene_bank_name"":""CANARA BANK"",""bene_bank_acc_num"":""120002132414"",""bene_bank_ifsc"":""CNRB0018763"",""bene_bank_account_holder_name"":""rajasthan granite an"",""bene_bank_account_type"":""Current"",""igst_amount"":""1080"",""cgst_amount"":""0"",""sgst_amount"":""0"",""emi_count"":36,""broken_interest"":""2367.12"",""broken_period_days"":12,""dpd_in_last_12_months"":0,""dpd_in_last_3_months_credit_card"":0,""dpd_in_last_3_months_unsecured"":0,""broken_period_int_amt"":2367.12,""dpd_in_last_24_months"":0,""enquiries_bureau_30_days"":0,""cnt_active_unsecured_loans"":0,""total_overdues_in_cc"":0,""insurance_amount"":0,""bureau_outstanding_loan_amt"":0,""subvention_fees_amount"":null,""gst_on_subvention_fees"":null,""cgst_on_subvention_fees"":null,""sgst_on_subvention_fees"":null,""igst_on_subvention_fees"":null,""purpose_of_loan"":""61"",""business_name"":""DEVENDRA TRADING CO."",""business_establishment_proof_type"":""Udhyog Adhaar"",""co_app_or_guar_name"":null,""co_app_or_guar_address"":null,""co_app_or_guar_mobile_no"":null,""co_app_or_guar_pan"":null,""co_app_or_guar_bureau_score"":null,""business_address_ownership"":""Owned"",""other_business_reg_no"":null,""enquiries_in_last_3_months"":0,""gst_on_conv_fees"":76.27,""cgst_on_conv_fees"":0,""sgst_on_conv_fees"":0,""igst_on_conv_fees"":76.27,""gst_on_application_fees"":null,""cgst_on_application_fees"":null,""sgst_on_application_fees"":null,""igst_on_application_fees"":null,""interest_type"":""rearended"",""conv_fees_excluding_gst"":423.73,""application_fees_excluding_gst"":null,""emi_obligation"":null,""a_score_request_id"":""SHO0161-BUREAU-SCORECARD-V2-1713978220615"",""a_score"":-1,""b_score"":0,""offered_amount"":300000,""offered_int_rate"":25.64,""monthly_average_balance"":184968.17666666667,""monthly_imputed_income"":154140.14722222224,""party_type"":""Individual"",""co_app_or_guar_dob"":null,""co_app_or_guar_gender"":""Female"",""co_app_or_guar_ntc"":""No"",""residence_vintage"":null,""business_entity_type"":""Proprietorship"",""udyam_reg_no"":""UDYAM-UP-38-0014718"",""program_type"":""Banking"",""written_off_settled"":0,""upi_handle"":""mam.100000005845@icici"",""upi_reference"":""100000005845"",""fc_offer_days"":4,""foreclosure_charge"":""5RP"",""is_repoed"":null,""force_esign"":false,""sl_req_sent"":false,""lba_req_sent"":false,""is_force_closed"":false,""is_force_cancelled"":false,""created_at"":""2024-04-24T18:32:58.300Z"",""updated_at"":""2024-04-24T18:32:58.300Z"",""_id"":8883666,""__v"":0}]}";
                var loanres = JsonConvert.DeserializeObject<GetLoanApiResponseDC>(result);
                if (loanres != null && loanres.data != null && loanres.data.Any())
                {
                    LeadLoan LeadLoanData = new LeadLoan();

                    LeadLoanData.LeadMasterId = leadid;
                    LeadLoanData.ReponseId = 1;
                    LeadLoanData.RequestId = 1;

                    LeadLoanData.IsSuccess = true;
                    LeadLoanData.Message = loanres.message ?? "";
                    LeadLoanData.IsActive = true;
                    LeadLoanData.IsDeleted = false;

                    LeadLoanData.loan_app_id = loanres.data.FirstOrDefault().loan_app_id ?? "";
                    LeadLoanData.loan_id = loanres.data.FirstOrDefault().loan_id ?? "";
                    LeadLoanData.borrower_id = loanres.data.FirstOrDefault().borrower_id ?? "";
                    LeadLoanData.partner_loan_app_id = loanres.data.FirstOrDefault().partner_loan_app_id ?? "";
                    LeadLoanData.partner_loan_id = loanres.data.FirstOrDefault().partner_loan_id ?? "";
                    LeadLoanData.partner_borrower_id = loanres.data.FirstOrDefault().partner_borrower_id ?? "";
                    LeadLoanData.company_id = loanres.data.FirstOrDefault().company_id ?? 0;
                    LeadLoanData.product_id = loanres.data.FirstOrDefault().product_id.ToString() ?? "";
                    LeadLoanData.loan_app_date = loanres.data.FirstOrDefault().loan_app_date ?? "";
                    LeadLoanData.sanction_amount = Convert.ToDouble(loanres.data.FirstOrDefault().sanction_amount);
                    LeadLoanData.gst_on_pf_amt = Convert.ToDouble(loanres.data.FirstOrDefault().gst_on_pf_amt);
                    LeadLoanData.gst_on_pf_perc = loanres.data.FirstOrDefault().gst_on_pf_perc ?? "";
                    LeadLoanData.net_disbur_amt = Convert.ToDouble(loanres.data.FirstOrDefault().net_disbur_amt);
                    LeadLoanData.status = loanres.data.FirstOrDefault().status ?? "";
                    LeadLoanData.stage = loanres.data.FirstOrDefault().stage ?? 0;
                    LeadLoanData.exclude_interest_till_grace_period = loanres.data.FirstOrDefault().exclude_interest_till_grace_period ?? "";
                    LeadLoanData.borro_bank_account_type = loanres.data.FirstOrDefault().borro_bank_account_type ?? "";
                    LeadLoanData.borro_bank_account_holder_name = loanres.data.FirstOrDefault().borro_bank_account_holder_name ?? "";
                    LeadLoanData.loan_int_rate = "24";//loanres.data.FirstOrDefault().loan_int_rate?? "";
                    LeadLoanData.processing_fees_amt = Convert.ToDouble(loanres.data.FirstOrDefault().processing_fees_amt);
                    LeadLoanData.processing_fees_perc = Convert.ToDouble(loanres.data.FirstOrDefault().processing_fees_perc);
                    LeadLoanData.tenure = loanres.data.FirstOrDefault().tenure ?? "";
                    LeadLoanData.tenure_type = loanres.data.FirstOrDefault().tenure_type ?? "";
                    LeadLoanData.int_type = loanres.data.FirstOrDefault().int_type ?? "";
                    LeadLoanData.borro_bank_ifsc = loanres.data.FirstOrDefault().borro_bank_ifsc ?? "";
                    LeadLoanData.borro_bank_acc_num = loanres.data.FirstOrDefault().borro_bank_acc_num ?? "";
                    LeadLoanData.borro_bank_name = loanres.data.FirstOrDefault().borro_bank_name ?? "";
                    LeadLoanData.first_name = loanres.data.FirstOrDefault().first_name ?? "";
                    LeadLoanData.last_name = loanres.data.FirstOrDefault().last_name ?? "";
                    LeadLoanData.current_overdue_value = loanres.data.FirstOrDefault().current_overdue_value;
                    LeadLoanData.bureau_score = loanres.data.FirstOrDefault().bureau_score ?? "";
                    LeadLoanData.loan_amount_requested = Convert.ToDouble(loanres.data.FirstOrDefault().loan_amount_requested);
                    LeadLoanData.bene_bank_name = loanres.data.FirstOrDefault().bene_bank_name ?? "";
                    LeadLoanData.bene_bank_acc_num = loanres.data.FirstOrDefault().bene_bank_acc_num ?? "";
                    LeadLoanData.bene_bank_ifsc = loanres.data.FirstOrDefault().bene_bank_ifsc ?? "";
                    LeadLoanData.bene_bank_account_holder_name = loanres.data.FirstOrDefault().bene_bank_account_holder_name ?? "";
                    LeadLoanData.created_at = loanres.data.FirstOrDefault().created_at;
                    LeadLoanData.updated_at = loanres.data.FirstOrDefault().updated_at;
                    LeadLoanData.v = loanres.data.FirstOrDefault().__v ?? 0;
                    LeadLoanData.co_lender_assignment_id = loanres.data.FirstOrDefault().co_lender_assignment_id ?? 0;
                    LeadLoanData.co_lender_id = loanres.data.FirstOrDefault().co_lender_id ?? 0;
                    LeadLoanData.co_lend_flag = loanres.data.FirstOrDefault().co_lend_flag ?? "";
                    LeadLoanData.itr_ack_no = loanres.data.FirstOrDefault().itr_ack_no ?? "";
                    LeadLoanData.penal_interest = loanres.data.FirstOrDefault().penal_interest ?? 0;
                    LeadLoanData.bounce_charges = loanres.data.FirstOrDefault().bounce_charges ?? 0;
                    LeadLoanData.repayment_type = loanres.data.FirstOrDefault().repayment_type ?? "";
                    LeadLoanData.first_inst_date = loanres.data.FirstOrDefault().first_inst_date;
                    LeadLoanData.final_approve_date = loanres.data.FirstOrDefault().final_approve_date;
                    LeadLoanData.final_remarks = loanres.data.FirstOrDefault().final_remarks ?? "";
                    LeadLoanData.foir = loanres.data.FirstOrDefault().foir ?? "";
                    LeadLoanData.upfront_interest = loanres.data.FirstOrDefault().upfront_interest ?? "";
                    LeadLoanData.business_vintage_overall = loanres.data.FirstOrDefault().business_vintage_overall ?? "";
                    LeadLoanData.loan_int_amt = Convert.ToDouble(loanres.data.FirstOrDefault().loan_int_amt);
                    LeadLoanData.conv_fees = Convert.ToDouble(loanres.data.FirstOrDefault().conv_fees);
                    LeadLoanData.ninety_plus_dpd_in_last_24_months = loanres.data.FirstOrDefault().ninety_plus_dpd_in_last_24_months ?? "";
                    LeadLoanData.dpd_in_last_9_months = loanres.data.FirstOrDefault().dpd_in_last_9_months ?? "";
                    LeadLoanData.dpd_in_last_3_months = loanres.data.FirstOrDefault().dpd_in_last_3_months ?? "";
                    LeadLoanData.dpd_in_last_6_months = loanres.data.FirstOrDefault().dpd_in_last_6_months ?? "";
                    LeadLoanData.insurance_company = loanres.data.FirstOrDefault().insurance_company ?? "";
                    LeadLoanData.credit_card_settlement_amount = Convert.ToDouble(loanres.data.FirstOrDefault().credit_card_settlement_amount);
                    LeadLoanData.emi_amount = Convert.ToDouble(loanres.data.FirstOrDefault().emi_amount);
                    LeadLoanData.emi_allowed = loanres.data.FirstOrDefault().emi_allowed ?? "";
                    LeadLoanData.igst_amount = Convert.ToDouble(loanres.data.FirstOrDefault().igst_amount);
                    LeadLoanData.cgst_amount = Convert.ToDouble(loanres.data.FirstOrDefault().cgst_amount);
                    LeadLoanData.sgst_amount = Convert.ToDouble(loanres.data.FirstOrDefault().sgst_amount);
                    LeadLoanData.emi_count = loanres.data.FirstOrDefault().emi_count ?? 0;
                    LeadLoanData.broken_interest = Convert.ToDouble(loanres.data.FirstOrDefault().broken_interest);
                    LeadLoanData.dpd_in_last_12_months = loanres.data.FirstOrDefault().dpd_in_last_12_months ?? 0;
                    LeadLoanData.dpd_in_last_3_months_credit_card = loanres.data.FirstOrDefault().dpd_in_last_3_months_credit_card ?? 0;
                    LeadLoanData.dpd_in_last_3_months_unsecured = loanres.data.FirstOrDefault().dpd_in_last_3_months_unsecured ?? 0;
                    LeadLoanData.broken_period_int_amt = loanres.data.FirstOrDefault().broken_period_int_amt ?? 0;
                    LeadLoanData.dpd_in_last_24_months = loanres.data.FirstOrDefault().dpd_in_last_24_months ?? 0;
                    LeadLoanData.avg_banking_turnover_6_months = 0;// loanres.data.FirstOrDefault().avg_banking_turnover_6_months?? "";
                    LeadLoanData.enquiries_bureau_30_days = loanres.data.FirstOrDefault().enquiries_bureau_30_days ?? 0;
                    LeadLoanData.cnt_active_unsecured_loans = loanres.data.FirstOrDefault().cnt_active_unsecured_loans ?? 0;
                    LeadLoanData.total_overdues_in_cc = loanres.data.FirstOrDefault().total_overdues_in_cc ?? 0;
                    LeadLoanData.insurance_amount = loanres.data.FirstOrDefault().insurance_amount ?? 0;
                    LeadLoanData.bureau_outstanding_loan_amt = loanres.data.FirstOrDefault().bureau_outstanding_loan_amt ?? 0;
                    LeadLoanData.purpose_of_loan = loanres.data.FirstOrDefault().purpose_of_loan ?? "";
                    LeadLoanData.business_name = loanres.data.FirstOrDefault().business_name ?? "";
                    LeadLoanData.co_app_or_guar_name = loanres.data.FirstOrDefault().co_app_or_guar_name ?? "";
                    LeadLoanData.co_app_or_guar_address = loanres.data.FirstOrDefault().co_app_or_guar_address ?? "";
                    LeadLoanData.co_app_or_guar_mobile_no = loanres.data.FirstOrDefault().co_app_or_guar_mobile_no ?? "";
                    LeadLoanData.co_app_or_guar_pan = loanres.data.FirstOrDefault().co_app_or_guar_pan ?? "";
                    LeadLoanData.business_address_ownership = loanres.data.FirstOrDefault().business_address_ownership ?? "";
                    LeadLoanData.business_pan = loanres.data.FirstOrDefault().business_pan ?? "";
                    LeadLoanData.bureau_fetch_date = DateTime.Now;// loanres.data.FirstOrDefault().bureau_fetch_date?? "";
                    LeadLoanData.enquiries_in_last_3_months = loanres.data.FirstOrDefault().enquiries_in_last_3_months ?? 0;
                    LeadLoanData.gst_on_conv_fees = loanres.data.FirstOrDefault().gst_on_conv_fees ?? 0;
                    LeadLoanData.cgst_on_conv_fees = loanres.data.FirstOrDefault().cgst_on_conv_fees ?? 0;
                    LeadLoanData.sgst_on_conv_fees = loanres.data.FirstOrDefault().sgst_on_conv_fees ?? 0;
                    LeadLoanData.igst_on_conv_fees = loanres.data.FirstOrDefault().igst_on_conv_fees ?? 0;
                    LeadLoanData.interest_type = loanres.data.FirstOrDefault().interest_type ?? "";
                    LeadLoanData.conv_fees_excluding_gst = loanres.data.FirstOrDefault().conv_fees_excluding_gst ?? 0;
                    LeadLoanData.a_score_request_id = loanres.data.FirstOrDefault().a_score_request_id ?? "";
                    LeadLoanData.a_score = loanres.data.FirstOrDefault().a_score ?? 0;
                    LeadLoanData.b_score = loanres.data.FirstOrDefault().b_score ?? 0;
                    LeadLoanData.offered_amount = loanres.data.FirstOrDefault().offered_amount ?? 0;
                    LeadLoanData.offered_int_rate = loanres.data.FirstOrDefault().offered_int_rate.Value;
                    LeadLoanData.monthly_average_balance = loanres.data.FirstOrDefault().monthly_average_balance.Value;
                    LeadLoanData.monthly_imputed_income = loanres.data.FirstOrDefault().monthly_imputed_income.Value;
                    LeadLoanData.party_type = loanres.data.FirstOrDefault().party_type ?? "";
                    LeadLoanData.co_app_or_guar_dob = DateTime.Now; //loanres.data.FirstOrDefault().co_app_or_guar_dob?? "";
                    LeadLoanData.co_app_or_guar_gender = loanres.data.FirstOrDefault().co_app_or_guar_gender ?? "";
                    LeadLoanData.co_app_or_guar_ntc = loanres.data.FirstOrDefault().co_app_or_guar_ntc ?? "";
                    LeadLoanData.udyam_reg_no = loanres.data.FirstOrDefault().udyam_reg_no ?? "";
                    LeadLoanData.program_type = loanres.data.FirstOrDefault().program_type ?? "";
                    LeadLoanData.written_off_settled = loanres.data.FirstOrDefault().written_off_settled ?? 0;
                    LeadLoanData.upi_handle = loanres.data.FirstOrDefault().upi_handle ?? "";
                    LeadLoanData.upi_reference = loanres.data.FirstOrDefault().upi_reference ?? "";
                    LeadLoanData.fc_offer_days = loanres.data.FirstOrDefault().fc_offer_days ?? 0;
                    LeadLoanData.foreclosure_charge = loanres.data.FirstOrDefault().foreclosure_charge ?? "";
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
                    LeadLoanData.cgst_on_application_fees = 0;
                    LeadLoanData.cgst_on_subvention_fees = 0;
                    LeadLoanData.co_app_or_guar_bureau_score = "";
                    LeadLoanData.customer_type_ntc = "";
                    LeadLoanData.emi_obligation = "";
                    LeadLoanData.gst_number = "";
                    LeadLoanData.gst_on_application_fees = 0;
                    LeadLoanData.gst_on_subvention_fees = 0;
                    LeadLoanData.igst_on_application_fees = 0;
                    LeadLoanData.igst_on_subvention_fees = 0;
                    LeadLoanData.monthly_income = "";
                    LeadLoanData.sgst_on_application_fees = 0;
                    LeadLoanData.sgst_on_subvention_fees = 0;
                    LeadLoanData.subvention_fees_amount = 0;
                    _context.LeadLoan.Add(LeadLoanData);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {


            }
            return res;
        }
        public async Task<CommonResponseDc> InsertLoanDataByArthmateTest(long leadid, string loanid)
        {
            CommonResponseDc res = new CommonResponseDc();
            //var data = _context.LeadLoan.Where(x => x.LeadMasterId == leadid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

            var Loanapi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateLoanGenerate && x.IsActive && !x.IsDeleted);
            var Response = await _ArthMateNBFCHelper.GetLoanById(loanid, Loanapi.APIUrl, Loanapi.TAPIKey, Loanapi.TAPISecretKey, Loanapi.Id, leadid);
            _context.ArthMateCommonAPIRequestResponses.Add(Response);
            _context.SaveChanges();
            if (Response.IsSuccess)
            {
                var loanres = JsonConvert.DeserializeObject<LoanDetailResponseDc>(Response.Response);

                if (loanres != null && loanres.loanDetails != null)
                {
                    LeadLoan LeadLoanData = new LeadLoan();

                    LeadLoanData.LeadMasterId = leadid;
                    LeadLoanData.ReponseId = 1;
                    LeadLoanData.RequestId = 1;

                    LeadLoanData.IsSuccess = true;
                    LeadLoanData.Message = loanres.message ?? "";
                    LeadLoanData.IsActive = true;
                    LeadLoanData.IsDeleted = false;

                    LeadLoanData.loan_app_id = loanres.loanDetails.loan_app_id ?? "";
                    LeadLoanData.loan_id = loanres.loanDetails.loan_id ?? "";
                    LeadLoanData.borrower_id = loanres.loanDetails.borrower_id ?? "";
                    LeadLoanData.partner_loan_app_id = loanres.loanDetails.partner_loan_app_id ?? "";
                    LeadLoanData.partner_loan_id = loanres.loanDetails.partner_loan_id ?? "";
                    LeadLoanData.partner_borrower_id = loanres.loanDetails.partner_borrower_id ?? "";
                    LeadLoanData.company_id = loanres.loanDetails.company_id;
                    LeadLoanData.product_id = loanres.loanDetails.product_id.ToString() ?? "";
                    LeadLoanData.loan_app_date = loanres.loanDetails.loan_app_date ?? "";
                    LeadLoanData.sanction_amount = Convert.ToDouble(loanres.loanDetails.sanction_amount);
                    LeadLoanData.gst_on_pf_amt = Convert.ToDouble(loanres.loanDetails.gst_on_pf_amt);
                    LeadLoanData.gst_on_pf_perc = loanres.loanDetails.gst_on_pf_perc ?? "";
                    LeadLoanData.net_disbur_amt = Convert.ToDouble(loanres.loanDetails.net_disbur_amt);
                    LeadLoanData.status = loanres.loanDetails.status ?? "";
                    LeadLoanData.stage = loanres.loanDetails.stage;
                    LeadLoanData.exclude_interest_till_grace_period = loanres.loanDetails.exclude_interest_till_grace_period ?? "";
                    LeadLoanData.borro_bank_account_type = loanres.loanDetails.borro_bank_account_type ?? "";
                    LeadLoanData.borro_bank_account_holder_name = loanres.loanDetails.borro_bank_account_holder_name ?? "";
                    LeadLoanData.loan_int_rate = "24";//loanres.loanDetails.loan_int_rate?? "";
                    LeadLoanData.processing_fees_amt = Convert.ToDouble(loanres.loanDetails.processing_fees_amt);
                    LeadLoanData.processing_fees_perc = Convert.ToDouble(loanres.loanDetails.processing_fees_perc);
                    LeadLoanData.tenure = loanres.loanDetails.tenure ?? "";
                    LeadLoanData.tenure_type = loanres.loanDetails.tenure_type ?? "";
                    LeadLoanData.int_type = loanres.loanDetails.int_type ?? "";
                    LeadLoanData.borro_bank_ifsc = loanres.loanDetails.borro_bank_ifsc ?? "";
                    LeadLoanData.borro_bank_acc_num = loanres.loanDetails.borro_bank_acc_num ?? "";
                    LeadLoanData.borro_bank_name = loanres.loanDetails.borro_bank_name ?? "";
                    LeadLoanData.first_name = loanres.loanDetails.first_name ?? "";
                    LeadLoanData.last_name = loanres.loanDetails.last_name ?? "";
                    LeadLoanData.current_overdue_value = loanres.loanDetails.current_overdue_value;
                    LeadLoanData.bureau_score = loanres.loanDetails.bureau_score ?? "";
                    LeadLoanData.loan_amount_requested = Convert.ToDouble(loanres.loanDetails.loan_amount_requested);
                    LeadLoanData.bene_bank_name = loanres.loanDetails.bene_bank_name ?? "";
                    LeadLoanData.bene_bank_acc_num = loanres.loanDetails.bene_bank_acc_num ?? "";
                    LeadLoanData.bene_bank_ifsc = loanres.loanDetails.bene_bank_ifsc ?? "";
                    LeadLoanData.bene_bank_account_holder_name = loanres.loanDetails.bene_bank_account_holder_name ?? "";
                    LeadLoanData.created_at = loanres.loanDetails.created_at;
                    LeadLoanData.updated_at = loanres.loanDetails.updated_at;
                    LeadLoanData.v = loanres.loanDetails.__v;
                    LeadLoanData.co_lender_assignment_id = loanres.loanDetails.co_lender_assignment_id;
                    LeadLoanData.co_lender_id = loanres.loanDetails.co_lender_id;
                    LeadLoanData.co_lend_flag = loanres.loanDetails.co_lend_flag ?? "";
                    LeadLoanData.itr_ack_no = loanres.loanDetails.itr_ack_no ?? "";
                    LeadLoanData.penal_interest = loanres.loanDetails.penal_interest;
                    LeadLoanData.bounce_charges = loanres.loanDetails.bounce_charges;
                    LeadLoanData.repayment_type = loanres.loanDetails.repayment_type ?? "";
                    LeadLoanData.first_inst_date = loanres.loanDetails.first_inst_date;
                    LeadLoanData.final_approve_date = loanres.loanDetails.final_approve_date;
                    LeadLoanData.final_remarks = loanres.loanDetails.final_remarks ?? "";
                    LeadLoanData.foir = loanres.loanDetails.foir ?? "";
                    LeadLoanData.upfront_interest = loanres.loanDetails.upfront_interest ?? "";
                    LeadLoanData.business_vintage_overall = loanres.loanDetails.business_vintage_overall ?? "";
                    LeadLoanData.loan_int_amt = Convert.ToDouble(loanres.loanDetails.loan_int_amt);
                    LeadLoanData.conv_fees = Convert.ToDouble(loanres.loanDetails.conv_fees);
                    LeadLoanData.ninety_plus_dpd_in_last_24_months = loanres.loanDetails.ninety_plus_dpd_in_last_24_months ?? "";
                    LeadLoanData.dpd_in_last_9_months = loanres.loanDetails.dpd_in_last_9_months ?? "";
                    LeadLoanData.dpd_in_last_3_months = loanres.loanDetails.dpd_in_last_3_months ?? "";
                    LeadLoanData.dpd_in_last_6_months = loanres.loanDetails.dpd_in_last_6_months ?? "";
                    LeadLoanData.insurance_company = loanres.loanDetails.insurance_company ?? "";
                    LeadLoanData.credit_card_settlement_amount = Convert.ToDouble(loanres.loanDetails.credit_card_settlement_amount);
                    LeadLoanData.emi_amount = Convert.ToDouble(loanres.loanDetails.emi_amount);
                    LeadLoanData.emi_allowed = loanres.loanDetails.emi_allowed ?? "";
                    LeadLoanData.igst_amount = Convert.ToDouble(loanres.loanDetails.igst_amount);
                    LeadLoanData.cgst_amount = Convert.ToDouble(loanres.loanDetails.cgst_amount);
                    LeadLoanData.sgst_amount = Convert.ToDouble(loanres.loanDetails.sgst_amount);
                    LeadLoanData.emi_count = loanres.loanDetails.emi_count;
                    LeadLoanData.broken_interest = Convert.ToDouble(loanres.loanDetails.broken_interest);
                    LeadLoanData.dpd_in_last_12_months = loanres.loanDetails.dpd_in_last_12_months;
                    LeadLoanData.dpd_in_last_3_months_credit_card = loanres.loanDetails.dpd_in_last_3_months_credit_card;
                    LeadLoanData.dpd_in_last_3_months_unsecured = loanres.loanDetails.dpd_in_last_3_months_unsecured;
                    LeadLoanData.broken_period_int_amt = Convert.ToDouble(loanres.loanDetails.broken_period_int_amt);
                    LeadLoanData.dpd_in_last_24_months = loanres.loanDetails.dpd_in_last_24_months;
                    LeadLoanData.avg_banking_turnover_6_months = 0;// loanres.loanDetails.avg_banking_turnover_6_months?? "";
                    LeadLoanData.enquiries_bureau_30_days = loanres.loanDetails.enquiries_bureau_30_days;
                    LeadLoanData.cnt_active_unsecured_loans = loanres.loanDetails.cnt_active_unsecured_loans;
                    LeadLoanData.total_overdues_in_cc = loanres.loanDetails.total_overdues_in_cc;
                    LeadLoanData.insurance_amount = loanres.loanDetails.insurance_amount;
                    LeadLoanData.bureau_outstanding_loan_amt = loanres.loanDetails.bureau_outstanding_loan_amt;
                    LeadLoanData.purpose_of_loan = loanres.loanDetails.purpose_of_loan ?? "";
                    LeadLoanData.business_name = loanres.loanDetails.business_name ?? "";
                    LeadLoanData.co_app_or_guar_name = loanres.loanDetails.co_app_or_guar_name ?? "";
                    LeadLoanData.co_app_or_guar_address = loanres.loanDetails.co_app_or_guar_address ?? "";
                    LeadLoanData.co_app_or_guar_mobile_no = loanres.loanDetails.co_app_or_guar_mobile_no ?? "";
                    LeadLoanData.co_app_or_guar_pan = loanres.loanDetails.co_app_or_guar_pan ?? "";
                    LeadLoanData.business_address_ownership = loanres.loanDetails.business_address_ownership ?? "";
                    LeadLoanData.business_pan = loanres.loanDetails.business_pan ?? "";
                    LeadLoanData.bureau_fetch_date = DateTime.Now;// loanres.loanDetails.bureau_fetch_date?? "";
                    LeadLoanData.enquiries_in_last_3_months = loanres.loanDetails.enquiries_in_last_3_months;
                    LeadLoanData.gst_on_conv_fees = loanres.loanDetails.gst_on_conv_fees;
                    LeadLoanData.cgst_on_conv_fees = loanres.loanDetails.cgst_on_conv_fees;
                    LeadLoanData.sgst_on_conv_fees = loanres.loanDetails.sgst_on_conv_fees;
                    LeadLoanData.igst_on_conv_fees = loanres.loanDetails.igst_on_conv_fees;
                    LeadLoanData.interest_type = loanres.loanDetails.interest_type ?? "";
                    LeadLoanData.conv_fees_excluding_gst = loanres.loanDetails.conv_fees_excluding_gst;
                    LeadLoanData.a_score_request_id = loanres.loanDetails.a_score_request_id ?? "";
                    LeadLoanData.a_score = loanres.loanDetails.a_score;
                    LeadLoanData.b_score = loanres.loanDetails.b_score;
                    LeadLoanData.offered_amount = loanres.loanDetails.offered_amount;
                    LeadLoanData.offered_int_rate = loanres.loanDetails.offered_int_rate;
                    LeadLoanData.monthly_average_balance = loanres.loanDetails.monthly_average_balance;
                    LeadLoanData.monthly_imputed_income = loanres.loanDetails.monthly_imputed_income;
                    LeadLoanData.party_type = loanres.loanDetails.party_type ?? "";
                    LeadLoanData.co_app_or_guar_dob = DateTime.Now; //loanres.loanDetails.co_app_or_guar_dob?? "";
                    LeadLoanData.co_app_or_guar_gender = loanres.loanDetails.co_app_or_guar_gender ?? "";
                    LeadLoanData.co_app_or_guar_ntc = loanres.loanDetails.co_app_or_guar_ntc ?? "";
                    LeadLoanData.udyam_reg_no = loanres.loanDetails.udyam_reg_no ?? "";
                    LeadLoanData.program_type = loanres.loanDetails.program_type ?? "";
                    LeadLoanData.written_off_settled = loanres.loanDetails.written_off_settled;
                    LeadLoanData.upi_handle = loanres.loanDetails.upi_handle ?? "";
                    LeadLoanData.upi_reference = loanres.loanDetails.upi_reference ?? "";
                    LeadLoanData.fc_offer_days = loanres.loanDetails.fc_offer_days;
                    LeadLoanData.foreclosure_charge = loanres.loanDetails.foreclosure_charge ?? "";
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
                    LeadLoanData.cgst_on_application_fees = 0;
                    LeadLoanData.cgst_on_subvention_fees = 0;
                    LeadLoanData.co_app_or_guar_bureau_score = "";
                    LeadLoanData.customer_type_ntc = "";
                    LeadLoanData.emi_obligation = "";
                    LeadLoanData.gst_number = "";
                    LeadLoanData.gst_on_application_fees = 0;
                    LeadLoanData.gst_on_subvention_fees = 0;
                    LeadLoanData.igst_on_application_fees = 0;
                    LeadLoanData.igst_on_subvention_fees = 0;
                    LeadLoanData.monthly_income = "";
                    LeadLoanData.sgst_on_application_fees = 0;
                    LeadLoanData.sgst_on_subvention_fees = 0;
                    LeadLoanData.subvention_fees_amount = 0;
                    _context.LeadLoan.Add(LeadLoanData);
                    _context.SaveChanges();
                }
            }

            return res;
        }
        public async Task<string> ConvertToBase64StringAsync(string FileUrl, string docName)
        {
            string baseBath = _hostingEnvironment.ContentRootPath;
            string FilePath = "";
            string pdfPath = docName + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";

            FilePath = Path.Combine(baseBath, "wwwroot", "OtherDoc");

            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }

            FilePath = Path.Combine(FilePath, pdfPath);
            string extension = Path.GetExtension(FileUrl);
            string ext = extension.Trim().ToLower();
            string DocBase64String = "";
            try
            {
                if (ext == ".jpeg" || ext == ".png" || ext == ".jpg")
                {
                    byte[] imageData = null;
                    using (var client = new HttpClient())
                    {
                        imageData = await client.GetByteArrayAsync(FileUrl);
                    }

                    // Create a document
                    iTextSharp.text.Document doc = new iTextSharp.text.Document();

                    // Create a PdfWriter instance
                    PdfWriter.GetInstance(doc, new FileStream(FilePath, FileMode.Create));

                    // Open the document
                    doc.Open();

                    // Create an image instance from the downloaded image data
                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageData);

                    // Scale the image to fit within the document
                    image.ScaleToFit(doc.PageSize.Width, doc.PageSize.Height);

                    // Add the image to the document
                    doc.Add(image);


                    // Close the document
                    doc.Close();

                    byte[] bytes = System.IO.File.ReadAllBytes(FilePath);
                    DocBase64String = Convert.ToBase64String(bytes);
                }
                else
                {
                    byte[] imageData = null;
                    using (var client = new HttpClient())
                    {
                        imageData = await client.GetByteArrayAsync(FileUrl);
                    }
                    if (imageData != null)
                    {
                        DocBase64String = Convert.ToBase64String(imageData);
                    }
                }

                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return DocBase64String;
        }

        public async Task<CommonResponseDc> UpdateBeneficiaryBankDetail(BeneficiaryBankDetailDc Obj)
        {
            CommonResponseDc res = new CommonResponseDc();
            if (Obj != null)
            {
                var bankdetail = await _context.LeadBankDetails.Where(x => x.LeadId == Obj.LeadMasterId && x.Id == Obj.BankDetailId && x.Type == BankTypeConstant.Beneficiary && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
                if (bankdetail != null)
                {
                    bankdetail.AccountNumber = Obj.Beneficiary_AccountNumber;
                    bankdetail.AccountHolderName = Obj.Beneficiary_Accountholdername;
                    bankdetail.AccountType = Obj.Beneficiary_Typeofaccount;
                    bankdetail.BankName = Obj.Beneficiary_Bankname;
                    bankdetail.IFSCCode = Obj.Beneficiary_IFSCCode;
                    _context.Entry(bankdetail).State = EntityState.Modified;
                }
                else
                {
                    LeadBankDetail leadBankDetails = new LeadBankDetail
                    {
                        AccountNumber = Obj.Beneficiary_AccountNumber,
                        AccountHolderName = Obj.Beneficiary_Accountholdername,
                        AccountType = Obj.Beneficiary_Typeofaccount,
                        BankName = Obj.Beneficiary_Bankname,
                        IFSCCode = Obj.Beneficiary_IFSCCode,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.LeadBankDetails.Add(leadBankDetails);
                }
                _context.SaveChanges();
                res.Status = true;
                res.Msg = "Bank Detail saved";
            }
            return res;
        }

        public async Task<ResultViewModel<List<OfferEmiDetailDC>>> GetOfferEmiDetails(long leadId, int ReqTenure = 0)
        {
            ResultViewModel<List<OfferEmiDetailDC>> result = new ResultViewModel<List<OfferEmiDetailDC>>();

            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == leadId && x.IsActive && !x.IsDeleted);
            if (lead != null)
            {
                var leadOffers = await _context.LeadOffers.FirstOrDefaultAsync(x => x.LeadId == leadId && x.NBFCCompanyId == lead.OfferCompanyId && x.IsActive && !x.IsDeleted);
                if (leadOffers != null)
                {
                    int tenure = 0;
                    if (leadOffers.CompanyIdentificationCode == CompanyIdentificationCodeConstants.ArthMate)
                    {
                        var arthmateupdate = await _context.ArthMateUpdates.FirstOrDefaultAsync(x => x.LeadId == leadId && x.IsActive && !x.IsDeleted);
                        tenure = arthmateupdate != null ? Convert.ToInt32(arthmateupdate.Tenure) : 0;
                    }
                    else
                    {
                        if (ReqTenure == 0)
                        {
                            var blNBFCUpdate = await _context.nbfcOfferUpdate.FirstOrDefaultAsync(x => x.LeadId == leadId && x.NBFCCompanyId == leadOffers.NBFCCompanyId && x.IsActive && !x.IsDeleted);
                            tenure = blNBFCUpdate != null ? blNBFCUpdate.Tenure ?? 0 : 0;
                        }
                        else
                        {
                            tenure = ReqTenure;
                        }
                    }
                    double FinalAmount = lead.CreditLimit ?? 0;
                    DateConvertHelper _DateConvertHelper = new DateConvertHelper();
                    var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
                    DateTime firstEmiDate = currentDateTime;
                    if (currentDateTime.Day >= 1 && currentDateTime.Day <= 20)
                    { firstEmiDate = new DateTime(currentDateTime.AddMonths(1).Year, currentDateTime.AddMonths(1).Month, 5); }
                    else
                    { firstEmiDate = new DateTime(currentDateTime.AddMonths(2).Year, currentDateTime.AddMonths(2).Month, 5); }
                    var emiAmount = pmt(lead.InterestRate ?? 0, tenure, FinalAmount);

                    var Leadid = new SqlParameter("@leadId", leadId);
                    var SanctionAmount = new SqlParameter("@SanctionAmount", FinalAmount);
                    var EmiAmount = new SqlParameter("@EmiAmount", emiAmount);
                    var RPIValue = new SqlParameter("@RPIValue", lead.InterestRate ?? 0);
                    var Tenure = new SqlParameter("@Tenure", tenure);
                    var FirstEmiDate = new SqlParameter("@FirstEmiDate", firstEmiDate);

                    result.Result = _context.Database.SqlQueryRaw<OfferEmiDetailDC>("exec GetOfferEmiDetails @leadId,@SanctionAmount,@EmiAmount,@RPIValue,@Tenure,@FirstEmiDate", Leadid, SanctionAmount, EmiAmount, RPIValue, Tenure, FirstEmiDate).AsEnumerable().ToList();
                    if (result.Result != null && result.Result.Any())
                    {
                        result.IsSuccess = true;
                        result.Message = "Data Found";
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Data Not Found";
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Leadoffer Not Found";
                }
            }
            return result;
        }
        public async Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> request)
        {
            GRPCReply<string> res = new GRPCReply<string>();

            var result = await GetOfferEmiDetails(request.Request.LeadId, request.Request.ReqTenure);
            if (result != null && result.IsSuccess)
            {
                List<DataTable> dt = new List<DataTable>();
                DataTable Repyment_Scdule = new DataTable();
                Repyment_Scdule.TableName = "Repyment_Scdule";
                dt.Add(Repyment_Scdule);
                Repyment_Scdule.Columns.Add("DueDate");
                Repyment_Scdule.Columns.Add("OutStanding");
                Repyment_Scdule.Columns.Add("Prin");//emi_amount
                Repyment_Scdule.Columns.Add("Interest");//int_amount
                Repyment_Scdule.Columns.Add("EMI");
                Repyment_Scdule.Columns.Add("Principal");

                foreach (var item in result.Result)
                {
                    var dr = Repyment_Scdule.NewRow();
                    dr["DueDate"] = item.DueDate.ToString("dd MMM yyyy");
                    dr["OutStanding"] = item.OutStandingAmount;
                    dr["Prin"] = item.Prin;
                    dr["Interest"] = item.InterestAmount;
                    dr["EMI"] = item.EMIAmount;
                    dr["Principal"] = item.PrincipalAmount;
                    Repyment_Scdule.Rows.Add(dr);
                }
                var htmltable = OfferEMIDataTableToHTML(Repyment_Scdule);

                res.Response = htmltable;
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

        public async Task<CommonResponseDc> InsertCeplrBanklist()
        {
            CommonResponseDc Result = new CommonResponseDc();

            var req = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateCeplerBankList && x.IsActive && !x.IsDeleted);
            if (req != null)
            {
                var res = await _ArthMateNBFCHelper.CeplrBankList(req.APIUrl, req.TAPIKey, req.TAPISecretKey, 0, 0);
                if (res != null && res.IsSuccess)
                {
                    List<CeplrBankList> banklist = new List<CeplrBankList>();
                    var result = JsonConvert.DeserializeObject<CeplrBankListdc>(res.Response);
                    if (result != null)
                    {
                        foreach (var item in result.data)
                        {
                            CeplrBankList obj = new CeplrBankList();
                            obj.aa_fip_id = item.aa_fip_id ?? "";
                            obj.fip_name = item.fip_name ?? "";
                            obj.pdf_fip_id = item.pdf_fip_id ?? 0;
                            obj.enable = item.enable ?? "";
                            obj.fip_logo_uri = item.fip_logo_uri ?? "";
                            obj.IsActive = true;
                            obj.IsDeleted = false;
                            obj.Created = DateTime.Now;
                            obj.LastModified = null;
                            obj.CreatedBy = null;
                            obj.LastModifiedBy = null;
                            banklist.Add(obj);
                        }
                        _context.CeplrBankList.AddRange(banklist);
                        _context.SaveChanges();
                    }
                }
            }


            return Result;
        }
        public async Task<CommonResponseDc> eSignSessionAsync(string AgreementPdfUrl, long leadid)
        {
            CommonResponseDc res = new CommonResponseDc();
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();
            var Persondetail = _context.PersonalDetails.FirstOrDefault(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted);

            if (Persondetail != null)
            {
                var eSignApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.eSignsession && x.IsActive && !x.IsDeleted);
                if (eSignApi != null)
                {
                    string DocumentBase64 = await ConvertToBase64StringAsync(AgreementPdfUrl, "Agreement");
                    eSignSessionRequest eSignReq = new eSignSessionRequest
                    {
                        documentUrl = "",
                        documentB64 = DocumentBase64,
                        referenceNumber = "",
                        documentName = "Agreement",
                        isSequence = true,
                        reviewerMail = "",
                        templateId = "bottom-left",
                        redirectUrl = "",
                        signerInfo = new List<SignerInfo>(),
                    };
                    SignerInfo signer = new SignerInfo
                    {
                        name = Persondetail.FirstName + " " + Persondetail.LastName,
                        email = Persondetail.EmailId,

                        signingInfo = new List<SigningInfo>(),
                    };
                    eSignReq.signerInfo.Add(signer);

                    eSignKarzaCommonAPIRequestResponse Result = await _eSignKarzaHelper.eSignSessionAsync(eSignReq, eSignApi.APIUrl, eSignApi.TAPIKey, eSignApi.TAPISecretKey, leadid);
                    _context.eSignKarzaCommonAPIRequestResponse.Add(Result);
                    await _context.SaveChangesAsync();

                    res.Status = Result.IsSuccess;
                    res.Data = Result.Response;

                }
            }
            return res;
        }
        public async Task<CommonResponseDc> eSignDocumentsAsync(long leadid, string DocumentId)
        {
            CommonResponseDc res = new CommonResponseDc();
            ICreateLeadNBFCResponse response = new CreateLeadNBFCResponse();

            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
            var eSignApi = await _context.LeadNBFCApis.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.eSigndocument && x.IsActive && !x.IsDeleted);
            if (eSignApi != null)
            {
                eSignDocumentRequest eSignDocReq = new eSignDocumentRequest
                {
                    documentId = DocumentId,
                    verificationDetailsRequired = "Y"
                };
                eSignKarzaCommonAPIRequestResponse Result = await _eSignKarzaHelper.eSignDocumentsAsync(eSignDocReq, eSignApi.APIUrl, eSignApi.TAPIKey, eSignApi.TAPISecretKey, leadid);
                _context.eSignKarzaCommonAPIRequestResponse.Add(Result);
                response.IsSuccess = Result.IsSuccess;
                _context.SaveChanges();

                res.Status = Result.IsSuccess;
                res.Data = Result.Response;


                //var eSignResponse = JsonConvert.DeserializeObject<eSignDocumentResponseDc>(Result.Response);
                //if (eSignResponse != null && eSignResponse.statusCode == 101 && eSignResponse.result != null)
                //{
                //    _context.eSignDocumentResponse.Add(new eSignDocumentResponse
                //    {
                //        LeadId = leadid,
                //        documentId = eSignDocReq.documentId,
                //        File = eSignResponse.result.file,
                //        auditTrail = Convert.ToString(eSignResponse.result.auditTrail),
                //        users = JsonConvert.SerializeObject(eSignResponse.result.verificationDetails),
                //        Created = currentDateTime,
                //        IsActive = true,
                //        IsDeleted = false,
                //        clientData = "",
                //        irn = "",
                //        messages = "",
                //    });

                //    if (_context.SaveChanges() > 0)
                //    {
                //        res.Status = true;
                //        res.Msg = "eSign Complete";
                //    };
                //    res.Data = Result;
                //}
                //else
                //{
                //    res.Status = false;
                //    res.Msg = "Internal server error";
                //}
            }
            return res;
        }

        public async Task<GRPCReply<string>> GenerateToken()
        {
            throw new NotImplementedException();


        }
        public async Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
        public async Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }

        public async Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
        public async Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
        public async Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> ayeleadReq)
        {
            throw new NotImplementedException();
        }
       
      
    }
}
