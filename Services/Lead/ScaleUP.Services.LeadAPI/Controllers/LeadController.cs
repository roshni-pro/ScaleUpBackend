using Microsoft.AspNetCore.Mvc;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Global.Infrastructure.Constants;
using MassTransit;
using Serilog;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.LeadAPI.Manager;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadModels;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.LeadDTO.Cashfree;
using System;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.DSA;
using ScaleUP.Services.LeadDTO.Lead.DSA;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Services.LeadAPI.Helper;
using ScaleUP.Services.LeadAPI.Persistence;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Enum;
using eAadhaarDigilockerResponseDc = ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.eAadhaarDigilockerResponseDc;
using ScaleUP.Services.LeadModels.LeadNBFC;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using static System.Net.WebRequestMethods;
using System.Reflection;
using ScaleUP.Services.LeadAPI.Migrations;
using eAadhaarDigilockerRequesTVerifyOTPDCXml = ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.eAadhaarDigilockerRequesTVerifyOTPDCXml;


namespace ScaleUP.Services.LeadAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class LeadController : BaseController
    {
        private readonly LeadManager _leadManager;
        private readonly IMassTransitService _massTransitService;
        private readonly IRequestClient<ICheckStatus> _checkStatusEvent;
        private readonly IRequestClient<ICreateLeadActivityMessage> _createLeadactivityEvent;
        //Serilog.ILogger logger = Log.ForContext<LeadController>();
        private readonly ILogger<LeadController> _logger;

        private readonly NBFCSchedular _nBFCSchedular;


        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly LeadApplicationDbContext _context;
        private IHostEnvironment _hostingEnvironment;

        public LeadController(LeadManager leadManager, NBFCSchedular nBFCSchedular,

        LeadNBFCSubActivityManager leadNBFCSubActivityManager,
            IMassTransitService massTransitService,
            IRequestClient<ICheckStatus> checkStatusEvent, IRequestClient<ICreateLeadActivityMessage> createLeadactivityEvent, ILogger<LeadController> logger,
            LeadApplicationDbContext context, IHostEnvironment hostingEnvironment)
        {
            _leadManager = leadManager;
            _massTransitService = massTransitService;
            _checkStatusEvent = checkStatusEvent;
            _createLeadactivityEvent = createLeadactivityEvent;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _nBFCSchedular = nBFCSchedular;
            _logger = logger;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [Route("GetLeadResponse")]
        public async Task<bool> GetLeadResponse(string guild)
        {

            try
            {
                var (status, notFound) = await _checkStatusEvent.GetResponse<ILeadStatus, ILeadNotFound>(new { LeadGuild = guild });

                if (status.IsCompletedSuccessfully)
                {
                    var response = await status;
                    // return Ok(response.Message);
                }
                else
                {
                    var response = await notFound;
                    //return NotFound(response.Message);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetLeadCurrentActivity")]
        public async Task<LeadCurrentActivity> GetLeadCurrentActivity(string MobileNo, long ProductId, long CompanyId, long? LeadId)
        {
            return await _leadManager.GetLeadCurrentActivity(MobileNo, ProductId, CompanyId, LeadId);
        }

        [HttpPost]
        [Route("PostLeadPAN")]
        public async Task<IActionResult> PostLeadPAN(LeadPanDTO command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            KYCActivityPAN kycActivityPAN = new KYCActivityPAN
            {
                DocumentId = command.DocumentId,
                UniqueId = command.UniqueId,
                FathersName = !string.IsNullOrEmpty(command.FathersName) ? command.FathersName : "",
                DOB = command.DOB,
                Name = command.Name
            };


            var lead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(lead).State = EntityState.Detached;
            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadRequestId = NewId.NextGuid(),
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.PAN,
                NextSequence = 2,
                UserId = command.UserId,
                CompanyId = command.CompanyId,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(kycActivityPAN),
                ProductCode = lead.ProductCode

            };
            //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);

            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
                if (leadResponseDTO.IsSuccess)
                {
                    await _leadManager.AddLeadConsentLog(command.LeadId, LeadTypeConsentConstants.PAN, command.UserName);
                    await _leadManager.UpdateLeadGenerator(command.LeadId, command.UserName);
                }
                return Ok(leadResponseDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($" {DateTime.Now} : Error while waiting for response : {ex.ToString()}");

                throw;
            }



            //if (status.IsCompletedSuccessfully)
            //{
            //    var response = await status;
            //    // return Ok(response.Message);
            //}
            //else
            //{
            //    var response = await notFound;
            //    //return NotFound(response.Message);
            //}


            return Ok(leadResponseDTO);
        }

        //[HttpPost]
        //[Route("PostLeadAadharGenerateOTP")]
        //public async Task<string> PostLeadAadharGenerateOTP(LeadAadharDTO command)
        //{
        //    KYCActivityAadhar kYCActivityAadhar = new KYCActivityAadhar
        //    {
        //        DocumentNumber = command.DocumentNumber,
        //        BackFileUrl = command.BackFileUrl,
        //        FrontFileUrl = command.FrontFileUrl,
        //        requestId = command.requestId,
        //        otp = command.otp
        //    };

        //    ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
        //    {
        //        LeadGuild = Guid.NewGuid().ToString(),
        //        ActivityId = command.ActivityId,
        //        LeadId = command.LeadId,
        //        SubActivityId = command.SubActivityId,
        //        CurrentSequence = 1,
        //        KYCMasterCode = KYCMasterConstants.Aadhar,
        //        NextSequence = 2,
        //        JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(kYCActivityAadhar)
        //    };
        //    await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);

        //    return createOrderMessage.LeadGuild;
        //}
        [HttpPost]
        [Route("PostLeadAadharVerifyOTP")]
        public async Task<ResultViewModel<long>> PostLeadAadharVerifyOTP(LeadAadharDTO command)
        {
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            KYCActivityAadhar kYCActivityAadhar = new KYCActivityAadhar
            {
                DocumentNumber = command.DocumentNumber,
                requestId = command.requestId,
                otp = command.otp,
                BackDocumentId = command.BackDocumentId,
                FrontDocumentId = command.FrontDocumentId
            };


            var lead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(lead).State = EntityState.Detached;
            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadRequestId = NewId.NextGuid(),
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.Aadhar,
                NextSequence = 2,
                UserId = command.UserId,
                CompanyId = command.CompanyId,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(kYCActivityAadhar),
                ProductCode = lead.ProductCode
            };
            //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);

            //return createOrderMessage.LeadGuild;
            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
                if (leadResponseDTO.IsSuccess)
                {
                    await _leadManager.AddLeadConsentLog(command.LeadId, LeadTypeConsentConstants.Aadhar, command.UserId);
                }
                else if(leadResponseDTO.Result==0 && !leadResponseDTO.IsSuccess)
                {
                    await _leadManager.UpdateKYCStatusForMismatchAadhar(command.LeadId, command.UserId);
                }
                return leadResponseDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError($" {DateTime.Now} : Error while waiting for response : {ex.ToString()}");

                throw;
            }
        }

        [HttpPost]
        [Route("PostLeadBankStatement")]
        public async Task<ResultViewModel<long>> PostLeadBankStatement(LeadCBankStatementDTO command)
        {
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            KYCBankStatementActivity kYCBankStatementActivity = new KYCBankStatementActivity
            {
                //FrontFileUrl = command.FrontFileUrl,
                DocumentNumber = command.DocumentNumber,
                //CreatedBy = command.UserId,
                //CreatedDate = DateTime.Now,
                DocumentId = command.documentId,
                BankStatement = command.bankStatement,
                BorroBankAccNum = command.borroBankAccNum,
                BorroBankIFSC = command.borroBankIFSC,
                AccType = command.accType,
                BorroBankName = command.borroBankName,
                EnquiryAmount = command.enquiryAmount,
                //GSTStatement = command.GSTStatement,
                //DOB = command.DOB,
                //DocumentMasterId = command.DocumentMasterId,
                //IsActive = true,
                //IsDeleted = false,
                //IssuedDate = command.IssuedDate,
                //IsVerified = true,
                LeadMasterId = command.LeadId,
                //NameOnCard = command.NameOnCard,
                //OtherInfo = command.OtherInfo,
                PdfPassword = command.pdfPassword
            };

            var lead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(lead).State = EntityState.Detached;

            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.BankStatement,
                NextSequence = 2,
                UserId = command.UserId,
                CompanyId = command.CompanyId,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(kYCBankStatementActivity),
                ProductCode = lead.ProductCode
            };
            //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);

            //return createOrderMessage.LeadGuild;
            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return leadResponseDTO;

        }

        //[HttpPost]
        //[Route("PostLeadMSME")]
        //public async Task<ResultViewModel<long>> PostLeadMSME(LeadMsmeDTO command)
        //{
        //    ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
        //    MSMEActivity MSMEActivity = new MSMEActivity
        //    {
        //        //FrontFileUrl = command.FrontFileUrl,
        //        LeadMasterId = command.LeadId,
        //        FrontDocumentId = command.frontDocumentId,
        //        Vintage = command.vintage,
        //        BusinessType = command.businessType,
        //        BusinessName = command.businessName,
        //        MSMECertificate = command.msmeCertificateUrl,
        //        MSMERegNum = command.msmeRegNum
        //    };

        //    var lead = await _leadManager.GetLeadById(command.LeadId);

        //    ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
        //    {
        //        LeadGuild = Guid.NewGuid().ToString(),
        //        ActivityId = command.ActivityId,
        //        LeadId = command.LeadId,
        //        SubActivityId = command.SubActivityId,
        //        CurrentSequence = 1,
        //        KYCMasterCode = KYCMasterConstants.MSME,
        //        NextSequence = 2,
        //        UserId = command.UserId,
        //        CompanyId = command.CompanyId,
        //        JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(MSMEActivity),
        //        ProductCode = lead.ProductCode
        //    };
        //    //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);
        //    try
        //    {
        //        var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
        //        leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
        //        leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
        //        leadResponseDTO.Message = res1.Message.State;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return leadResponseDTO;
        //    //return createOrderMessage.LeadGuild;
        //}

        [HttpPost]
        [Route("PostLeadMSME")]
        public async Task<IActionResult> PostLeadMSME(LeadMsmeDTO command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            MSMEActivity MSMEActivity = new MSMEActivity
            {
                //FrontFileUrl = command.FrontFileUrl,
                LeadMasterId = command.LeadId,
                FrontDocumentId = command.frontDocumentId,
                Vintage = command.vintage,
                BusinessType = command.businessType,
                BusinessName = command.businessName,
                MSMECertificate = command.msmeCertificateUrl,
                MSMERegNum = command.msmeRegNum
            };

            var lead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(lead).State = EntityState.Detached;

            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.MSME,
                NextSequence = 2,
                UserId = command.UserId,
                CompanyId = command.CompanyId,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(MSMEActivity),
                ProductCode = lead.ProductCode
            };
            //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);
            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Ok(leadResponseDTO);
            //return createOrderMessage.LeadGuild;
        }

        private IActionResult PostLeadData(LeadMsmeDTO leadMsmeDTO)
        {
            if (ModelState.IsValid)
            {
                return Ok();
            }
            else
            {

                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [Route("PostLeadPersonalDetail")]
        public async Task<ResultViewModel<long>> PostLeadPersonalDetail(LeadPersonalDetailDTO command)
        {
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            KYCPersonalDetailActivity kYCPersonalDetailActivity = new KYCPersonalDetailActivity
            {
                TypeOfAddress = command.TypeOfAddress,
                State = command.State,
                AlternatePhoneNo = command.AlternatePhoneNo,
                City = command.City,
                EmailId = command.EmailId,
                FatherLastName = command.FatherLastName,
                FatherName = command.FatherName,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Gender = command.Gender,
                LeadMasterId = command.LeadId,
                PermanentAddressLine1 = command.PermanentAddressLine1,
                PermanentAddressLine2 = command.PermanentAddressLine2,
                PermanentCity = command.PermanentCity,
                PermanentPincode = command.PermanentPincode,
                PermanentState = command.PermanentState,
                Pincode = command.Pincode,
                ResAddress1 = command.ResAddress1,
                ResAddress2 = command.ResAddress2,
                ResidenceStatus = command.ResidenceStatus,
                MiddleName = command.MiddleName,
                MobileNo = command.MobileNo,
                Marital = command.Marital,
                OwnershipType = command.OwnershipType,
                OwnershipTypeResponseId = command.OwnershipTypeResponseId,
                OwnershipTypeProof = command.OwnershipTypeProof,
                OwnershipTypeName = command.OwnershipTypeName,
                IVRSNumber = command.IVRSNumber,
                OwnershipTypeAddress = command.OwnershipTypeAddress,
                ElectricityBillDocumentId = command.ElectricityBillDocumentId,
                ElectricityState = command.ElectricityState,
                ElectricityServiceProvider = command.ElectricityServiceProvider,
            };

            var Lead = _leadManager.PostPersonalDetailByLeadId(command);

            var thislead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(thislead).State = EntityState.Detached;

            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.PersonalDetail,
                NextSequence = 2,
                CompanyId = command.CompanyId,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(kYCPersonalDetailActivity),
                UserId = command.UserId,
                ProductCode = thislead.ProductCode
            };
            //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);
            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return leadResponseDTO;

            //return res1.Message.LeadGuild;
        }
        [HttpPost]
        [Route("PostLeadBuisnessDetail")]
        public async Task<ResultViewModel<long>> PostLeadBuisnessDetail(LeadBuisnessDetail command)
        {
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            BusinessActivityDetail businessActivityDetail = new BusinessActivityDetail
            {
                BusAddCorrCity = command.BusAddCorrCity,
                BusAddCorrLine1 = command.BusAddCorrLine1,
                BusAddCorrLine2 = command.BusAddCorrLine2,
                BusAddCorrPincode = command.BusAddCorrPincode,
                BusAddCorrState = command.BusAddCorrState,
                BuisnessMonthlySalary = command.BuisnessMonthlySalary,
                //BusAddPerCity = command.BusAddPerCity,
                //BusAddPerLine1 = command.BusAddPerLine1,
                //BusAddPerLine2 = command.BusAddPerLine2,
                //BusAddPerPincode = command.BusAddPerPincode,
                //BusAddPerState = command.BusAddPerState,
                BusEntityType = command.BusEntityType,
                BusGSTNO = command.BusGSTNO,
                BusName = command.BusName,
                //BusPan = command.BusPan,
                DOI = command.DOI,
                LeadMasterId = command.LeadId,

                IncomeSlab = command.IncomeSlab,

                BuisnessDocumentNo = command.BuisnessDocumentNo,
                BuisnessProof = command.BuisnessProof,
                BuisnessProofDocId = command.BuisnessProofDocId,
                InquiryAmount = command.InquiryAmount,
                SurrogateType = command.SurrogateType

            };

            var lead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(lead).State = EntityState.Detached;

            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.BuisnessDetail,
                NextSequence = 2,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(businessActivityDetail),
                UserId = command.UserId,
                CompanyId = command.CompanyId,
                ProductCode = lead.ProductCode
            };
            //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);
            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
                if (leadResponseDTO.IsSuccess)
                {
                    DateTime currentDate = DateTime.Now;
                    DateTime givenDate = new DateTime();
                    string format = "yyyy-MM-dd";
                    // Convert the string to DateTime
                    givenDate = DateTime.ParseExact(command.DOI, format, System.Globalization.CultureInfo.InvariantCulture);
                    TimeSpan difference = currentDate - givenDate;
                    await _leadManager.CheckVintage(command.LeadId, command.CompanyId, difference.Days);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return leadResponseDTO;
            //return createOrderMessage.LeadGuild;
        }

        [HttpPost]
        [Route("PostLeadSelfie")]
        public async Task<ResultViewModel<long>> PostLeadSelfie(SelfieDTO command)
        {
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            SelfeiActivity selfeiActivity = new SelfeiActivity
            {
                //FrontImageUrl = command.FrontImageUrl,
                FrontDocumentId = command.FrontDocumentId,
                LeadId = command.LeadId
            };

            var lead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(lead).State = EntityState.Detached;


            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.Selfie,
                NextSequence = 2,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(selfeiActivity),
                UserId = command.UserId,
                CompanyId = command.ComapnyId,
                ProductCode = lead.ProductCode
            };
            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return leadResponseDTO;
        }

        [HttpPost]
        [Route("PostLeadBankStatementCreditLending")]
        public async Task<ResultViewModel<long>> PostLeadBankStatementCreditLending(LeadBankStatementCreditLending command)
        {
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            BankStatementCreditLendingActivity bankStatementCreditLendingActivity = new BankStatementCreditLendingActivity
            {
                BankDocumentId = command.BankDocumentId,
                SarrogateDocId = command.SarrogateDocId,
                SurrogateType = command.SurrogateType
            };

            var lead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(lead).State = EntityState.Detached;

            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.BankStatementCreditLending,
                NextSequence = 2,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(bankStatementCreditLendingActivity),
                UserId = command.UserId,
                CompanyId = command.CompanyId,
                ProductCode = lead.ProductCode
            };

            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return leadResponseDTO;
        }

        [HttpPost]
        [Route("VerifyLeadDocument")]
        public async Task<VerifyLeadDocumentReply> VerifyLeadDocument(VerifyLeadDocumentRequest verifyLeadDocumentRequest)
        {
            return await _leadManager.VerifyLeadDocument(verifyLeadDocumentRequest);
        }

        [HttpPost]
        [Route("GetVerifiedLeadDocumentStatus")]
        public async Task<VerifyLeadDocumentReply> GetVerifiedLeadDocumentStatus(VerifyLeadDocumentRequest verifyLeadDocumentRequest)
        {
            return await _leadManager.GetVerifiedLeadDocumentStatus(verifyLeadDocumentRequest);
        }


        [HttpGet]
        [Route("GetLeadOffer")]
        public async Task<LeadOfferDTO> GetOffer(long LeadId)
        {
            return await _leadManager.GetLeadOffer(LeadId);
        }

        [HttpGet]
        [Route("GetOfferAccepted")]
        public async Task<LeadOfferAcceptedDTO> GetOfferAccepted(long LeadId)
        {
            return await _leadManager.GetOfferAccepted(LeadId);
        }

        [HttpGet]
        [Route("LeadExistsForCustomer")]
        //[AllowAnonymous]
        public async Task<long> LeadExistsForCustomer(string companyCode, string productCode, string Mobile, string customerReferenceNo)
        {
            return await _leadManager.LeadExistsForCustomer(companyCode, productCode, Mobile, customerReferenceNo);
        }


        [HttpGet]
        [Route("UpdateLeadOffer")]
        public async Task<bool> UpdateLeadOffer(long LeadOfferId)
        {
            return await _leadManager.UpdateLeadOffer(LeadOfferId);
        }


        [HttpGet]
        [Route("GetGenerateOfferStatus")]
        public async Task<List<LeadCompanyGenerateOfferDTO>> GetGenerateOfferStatus(long LeadId)
        {
            return await _leadNBFCSubActivityManager.GetLeadActivityOfferStatus(LeadId, ActivityConstants.GenerateOffer);
        }

        [HttpGet]
        [Route("GetGenerateAgreementStatus")]
        public async Task<List<LeadCompanyGenerateOfferDTO>> GetGenerateAgreementStatus(long LeadId)
        {
            return await _leadNBFCSubActivityManager.GetLeadActivityOfferStatus(LeadId, ActivityConstants.Agreement);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SaveModifyTemplateMaster")]
        public async Task<TemplateResponseDc> SaveModifyTemplateMaster(TemplateDc templatedc)
        {
            return await _leadManager.SaveModifyTemplateMaster(templatedc);
        }

        [HttpPost]
        [Route("DisbursementReject")]
        public async Task<bool> DisbursementReject(DisbursementRejectDTO disbursementRejectDTO)
        {
            return await _leadManager.DisbursementReject(disbursementRejectDTO);
        }


        [HttpGet]
        [Route("IsOfferGenerated")]
        public async Task<ResultViewModel<bool>> IsOfferGenerated(long LeadId)
        {
            return await _leadManager.IsOfferGenerated(LeadId);
        }


        [HttpGet]
        [Route("ThirdPartyCreateLeadRetry")]
        public async Task<GRPCReply<string>> LeadThirdPartyRetry(long LeadId, long NbfcId)
        {
            return await _nBFCSchedular.GenerateOfferForLeadRetry(LeadId, NbfcId);
        }

        [HttpGet]
        [Route("ThirdPartyAgreementLeadRetry")]
        [AllowAnonymous]
        public async Task ThirdPartyAgreementLeadRetry(long LeadId, long NbfcId)
        {
            await _nBFCSchedular.GenerateAgreementRetry(LeadId, NbfcId);
        }


        [HttpGet]
        [Route("GetAllLeadOfferStatus")]
        public async Task<ResultViewModel<List<LeadOfferStatusDTO>>> GetAllLeadOfferStatus(long LeadId)
        {
            return await _leadManager.GetAllLeadOfferStatus(LeadId);
        }

        [HttpGet]
        [Route("LeadReject")]
        public async Task<ResultViewModel<bool>> LeadReject(long LeadId, string Message)
        {
            return await _leadManager.LeadReject(LeadId, Message);
        }

        [HttpGet]
        [Route("GetLeadCommonDetail")]
        public async Task<LeadListDetail> GetLeadCommonDetail(long LeadId)
        {
            return await _leadManager.GetLeadCommonDetail(LeadId);
        }

        [HttpGet]
        [Route("ResetLeadActivityMasterProgresse")]
        public async Task<bool> ResetLeadActivityMasterProgresse(long LeadId)
        {
            return await _leadManager.ResetLeadActivityMasterProgresse(LeadId);
        }

        [HttpGet]
        [Route("DisbursementNext")]
        public async Task<ResultViewModel<bool>> DisbursementNext(long LeadId)
        {
            return await _leadManager.DisbursementNext(LeadId);
        }

        [HttpGet]
        [Route("GetCompanyBuyingHistory")]
        //[AllowAnonymous]
        public async Task<ResultViewModel<List<CompanyBuyingHistoryDTO>>> GetCompanyBuyingHistory(long CompanyId, long LeadId)
        {
            return await _leadManager.GetCompanyBuyingHistory(CompanyId, LeadId);
        }

        [HttpPost]
        [Route("AddLeadGeneratorConvertor")]
        //[AllowAnonymous]
        public async Task<bool> AddLeadGeneratorConvertor(AddLeadGeneratorConvertorDTO addLeadGeneratorConvertorDTO)
        {
            return await _leadManager.AddLeadGeneratorConvertor(addLeadGeneratorConvertorDTO);
        }

        [HttpGet]
        [Route("LeadDataOnInProgressScreen")]
        public async Task<ResultViewModel<Leads>> LeadDataOnInProgressScreen(long LeadId)
        {
            return await _leadManager.LeadDataOnInProgressScreen(LeadId);
        }

        [Route("getSlaLbaStampDetailsData")]
        [HttpGet]
        //[AllowAnonymous]
        public async Task<ResultViewModel<List<SlaLbaStampDetailsData>>> getSlaLbaStampDetailsData(int isStampUsed, int skip, int take)
        {
            return await _leadManager.getSlaLbaStampDetailsData(isStampUsed, skip, take);
        }

        [HttpGet]
        [Route("GetPFCollectionActivityStatus")]
        public async Task<ResultViewModel<bool>> GetPFCollectionActivityStatus(long LeadId)
        {
            return await _leadManager.GetPFCollectionActivityStatus(LeadId);
        }

        [HttpGet]
        [Route("StampPagerDelete")]
        public async Task<ResultViewModel<bool>> StampPagerDelete(long ArthmateSlaId)
        {
            return await _leadManager.StampPagerDelete(ArthmateSlaId);
        }

        [HttpGet]
        [Route("StampPaperNumberCheck")]
        public async Task<ResultViewModel<bool>> StampPaperNumberCheck(int stampPaperNo)
        {
            return await _leadManager.StampPaperNumberCheck(stampPaperNo);
        }

        [HttpPost]
        [Route("UploadLeadDocuments")]
        [AllowAnonymous]
        public async Task<ResultViewModel<bool>> UploadLeadDocuments(LeadDocumentDetailDTO leadDocumentDetailDTO)
        {
            return await _leadManager.UploadLeadDocuments(leadDocumentDetailDTO);
        }

        [HttpPost]
        [Route("GetLeadDocumentsByLeadId")]
        [AllowAnonymous]
        public async Task<ResultViewModel<List<LeadDocumentDetailDTO>>> GetLeadDocumentsByLeadId(long LeadId)
        {
            return await _leadManager.GetLeadDocumentsByLeadId(LeadId);
        }

        [HttpPost]
        [Route("UpdateCibilDetails")]
        [AllowAnonymous]
        public async Task<ResultViewModel<bool>> UpdateCibilDetails(AddCibilDetailsRequestDTO addCibilDetailsRequestDTO)
        {
            return await _leadManager.UpdateCibilDetails(addCibilDetailsRequestDTO);
        }

        #region Cashfree
        [HttpPost]
        [Route("GenerateSubscriptionwithPlanInfo")]
        [AllowAnonymous]
        public async Task<ResultViewModel<CashfreeCreateSubscriptionDetailResponse>> GenerateSubscriptionwithPlanInfo(long LeadId)
        {
            return await _leadManager.GenerateSubscriptionwithPlanInfo(LeadId);
        }
        #endregion

        #region TestCashfree
        [HttpGet]
        [Route("testDate")]
        [AllowAnonymous]
        public async Task<ResultViewModel<bool>> testDate()
        {
            ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();

            //var todaydate = DateTime.Now;
            DateTime todayDateTime = DateTime.Now;
            DateTime tomorrowDateTime = DateTime.Now.AddDays(1).Date;
            var diffime = 24 - todayDateTime.Hour;
            var diffMin = 00 - todayDateTime.Minute;
            var remaining = TimeSpan.FromHours(24) - DateTime.Now.TimeOfDay;
            var remainingMin = TimeSpan.FromMinutes(60) - DateTime.Now.TimeOfDay;
            //var nowdate = DateTime.Now.Date;
            //var timeZone = nowdate.ToString("yyyy/MM/dd 00:00:00");
            resultViewModel.IsSuccess = true;
            return resultViewModel;
        }
        #endregion


        [HttpGet]
        [Route("CheckIsOfferRejected")]
        public async Task<ResultViewModel<bool>> CheckIsOfferRejected(long leadId)
        {
            return await _leadManager.CheckIsOfferRejected(leadId);
        }


        #region DSA
        [HttpPost]
        [Route("PostLeadDSAProfileType")]
        public async Task<ResultViewModel<long>> PostLeadDSAProfileType(DSAProfileTypeDTO command)
        {
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            KYCDSAProfileTypeActivity kYCDSAProfileTypeActivity = new KYCDSAProfileTypeActivity
            {
                DSAType = command.DSAType
            };

            var thislead = await _leadManager.GetLeadById(command.LeadId);
            _context.Entry(thislead).State = EntityState.Detached;

            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.DSAProfileType,
                NextSequence = 2,
                CompanyId = command.CompanyId,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(kYCDSAProfileTypeActivity),
                UserId = command.UserId,
                ProductCode = thislead.ProductCode
            };
            //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);
            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return leadResponseDTO;

            //return res1.Message.LeadGuild;
        }
            [HttpPost]
            [Route("PostLeadDSAPersonalDetail")]
            public async Task<ResultViewModel<long>> PostLeadDSAPersonalDetail(DSAPersonalDetailDTO command)
            {
                ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
                KYCDSAPersonalDetailActivity kYCDSAPersonalDetailActivity = new KYCDSAPersonalDetailActivity
                {
                    CompanyName = command.CompanyName,
                    AlternatePhoneNo = command.AlternatePhoneNo,
                    BuisnessDocument = command.BuisnessDocument,
                    DocumentId = command.DocumentId,
                    EmailId = command.EmailId,
                    FatherOrHusbandName = command.FatherOrHusbandName,
                    FirmType = command.FirmType,
                    FullName = command.FullName,
                    GSTNumber = command.GSTNumber,
                    GSTRegistrationStatus = command.GSTRegistrationStatus,
                    LanguagesKnown = command.LanguagesKnown,
                    LeadMasterId = command.LeadMasterId,
                    NoOfYearsInCurrentEmployment = command.NoOfYearsInCurrentEmployment,
                    PresentOccupation = command.PresentOccupation,
                    Qualification = command.Qualification,
                    ReferneceContact = command.ReferneceContact,
                    WorkingLocation = command.WorkingLocation,
                    ReferneceName = command.ReferneceName,
                    WorkingWithOther = command.WorkingWithOther,
                    //CurrentAddressId = command.CurrentAddressId,
                    MobileNo = command.MobileNo,
                    Address = command.Address,
                    City = command.City,
                    Pincode = command.Pincode,
                    State = command.State,
                    CompanyState = command.CompanyState,
                    CompanyPincode = command.CompanyPincode,
                    CompanyCity = command.CompanyCity,
                    CompanyAddress = command.CompanyAddress,

                };

                var thislead = await _leadManager.GetLeadById(command.LeadId);
                _context.Entry(thislead).State = EntityState.Detached;

                ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
                {
                    LeadGuild = Guid.NewGuid().ToString(),
                    ActivityId = command.ActivityId,
                    LeadId = command.LeadId,
                    SubActivityId = command.SubActivityId,
                    CurrentSequence = 1,
                    KYCMasterCode = KYCMasterConstants.DSAPersonalDetail,
                    NextSequence = 2,
                    CompanyId = command.CompanyId,
                    JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(kYCDSAPersonalDetailActivity),
                    UserId = command.UserId,
                    ProductCode = thislead.ProductCode
                };
                //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);
                try
                {
                    var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                    leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                    leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                    leadResponseDTO.Message = res1.Message.State;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return leadResponseDTO;

                //return res1.Message.LeadGuild;
            }

        [HttpPost]
        [Route("PostLeadConnectorPersonalDetail")]
        public async Task<ResultViewModel<long>> PostLeadConnectorPersonalDetail(ConnectorPersonalDetailDTO command)
        {
            ResultViewModel<long> leadResponseDTO = new ResultViewModel<long>();
            KYCConnectorPersonalDetailActivity kYCConnectorPersonalDetailActivity = new KYCConnectorPersonalDetailActivity
            {
                AlternatePhoneNo = command.AlternatePhoneNo,
                //CurrentAddressId = command.CurrentAddressId,
                EmailId = command.EmailId,
                FatherName = command.FatherName,
                FullName = command.FullName,
                LanguagesKnown = command.LanguagesKnown,
                LeadMasterId = command.LeadMasterId,
                PresentEmployment = command.PresentEmployment,
                ReferenceName = command.ReferenceName,
                ReferneceContact = command.ReferneceContact,
                WorkingLocation = command.WorkingLocation,
                WorkingWithOther = command.WorkingWithOther,
                MobileNo = command.MobileNo,
                Address = command.Address,
                City = command.City,
                Pincode = command.Pincode,
                State = command.State
            };

            var thislead = await _leadManager.GetLeadById(command.LeadMasterId);
            _context.Entry(thislead).State = EntityState.Detached;

            ICreateLeadActivityMessage createOrderMessage = new CreateLeadActivityMessage
            {
                LeadGuild = Guid.NewGuid().ToString(),
                ActivityId = command.ActivityId,
                LeadId = command.LeadId,
                SubActivityId = command.SubActivityId,
                CurrentSequence = 1,
                KYCMasterCode = KYCMasterConstants.ConnectorPersonalDetail,
                NextSequence = 2,
                CompanyId = command.CompanyId,
                JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(kYCConnectorPersonalDetailActivity),
                UserId = command.UserId,
                ProductCode = thislead.ProductCode
            };
            //await _massTransitService.Send<ICreateLeadActivityMessage>(createOrderMessage, QueuesConsts.CreateLeadActivityMessageQueueName);
            try
            {
                var res1 = await _createLeadactivityEvent.GetResponse<Leadresponse>(createOrderMessage);
                leadResponseDTO.Result = long.Parse(res1.Message.LeadGuild);
                leadResponseDTO.IsSuccess = res1.Message.IsSuccess;
                leadResponseDTO.Message = res1.Message.State;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return leadResponseDTO;

            //return res1.Message.LeadGuild;
        }
        #endregion

        #region MAS finance Agreement

        [HttpGet]
        [Route("GetMASFinanceAgreement")]
        public async Task<ResultViewModel<LeadAgreement>> GetMASFinanceAgreement(long LeadId)
        {
            return await _leadManager.GetMASFinanceAgreement(LeadId);
        }

        [Route("GenerateKarzaAadhaarOtpForNBFC")]
        [HttpGet]
        public async Task<CommonResponseDc> GenerateKarzaAadhaarOtpForNBFC(long leadId)
        {
            CommonResponseDc res = new CommonResponseDc();
            eAadhaarDigilockerResponseDc eAadhaarDigilockerResponseDc = new eAadhaarDigilockerResponseDc();
            var leadinfo = _context.Leads.FirstOrDefault(x => x.Id == leadId && x.IsActive && !x.IsDeleted);
            var requiredAadharData = _context.PersonalDetails.FirstOrDefault(x => x.LeadId == leadId && x.IsActive && !x.IsDeleted);
            string basePath = _hostingEnvironment.ContentRootPath;
            KarzaAadharHelper karzaAadharHelper = new KarzaAadharHelper(_context);
            eAadhaarDigilockerResponseDc = await karzaAadharHelper.eAdharDigilockerOTPXml(requiredAadharData.AadhaarMaskNO, leadinfo.UserName, basePath);

            if (eAadhaarDigilockerResponseDc != null && eAadhaarDigilockerResponseDc.statusCode == 101 && eAadhaarDigilockerResponseDc.result.message == "OTP sent to registered mobile number")
            {
                //OTP sent to registered mobile number
                res.Status = true;
                res.Data = eAadhaarDigilockerResponseDc;
                res.Msg = eAadhaarDigilockerResponseDc.result.message;
            }
            else
            {
                res.Status = true;
                res.Msg = eAadhaarDigilockerResponseDc.error.error.message;
            }
            return res;
        }

        [Route("KarzaAadhaarOtpVerifyForNBFC")]
        [HttpPost]
        public async Task<bool> KarzaAadhaarOtpVerifyForNBFC(eAadhaarDigilockerRequesTVerifyOTPDCXml obj)
        {
            bool res = false;
            var personalInfo = await _context.PersonalDetails.FirstOrDefaultAsync(x => x.LeadId == obj.LeadMasterId && x.IsActive && !x.IsDeleted);
            obj.aadhaarNo = personalInfo.AadhaarMaskNO;


            KarzaAadharHelper karzaAadharHelper = new KarzaAadharHelper(_context);
            var verifyData = await karzaAadharHelper.eAadharDigilockerVerifyOTPXml(obj, UserId, "");
            if (verifyData.result.dataFromAadhaar != null)//(verifyData.Result.statusCode == 200)
            {
                res = true;
            }
            return res;
        }

        #endregion

        #region MAS/AYE finance Update OfferAmount
        [HttpPost]
        [Route("UpdateLeadOfferByFinance")]
        public async Task<ResultViewModel<bool>> UpdateLeadOfferByFinance(UpdateLeadOfferFinanceRequestDTO updateLeadOfferFinanceRequestDTO)
        {
            return await _leadManager.UpdateLeadOfferByFinance(updateLeadOfferFinanceRequestDTO);
        }

        [HttpGet]
        [Route("GetAcceptedLoanDetail")]
        public async Task<ResultViewModel<GetAcceptedLoanDetailDC>> GetAcceptedLoanDetail(long LeadId)
        {
            return await _leadManager.GetAcceptedLoanDetail(LeadId);
        }

        [HttpPost]
        [Route("RejectNBFCOffer")]
        public async Task<ResultViewModel<string>> RejectNBFCOffer(RejectNBFCOfferDC req)
        {
            return await _leadManager.RejectNBFCOffer(req);
        }

        [HttpGet]
        [Route("GetLeadOfferInitiatedStatus")]
        public async Task<ResultViewModel<List<LeadOfferInitiatedStatusResponseDTO>>> GetLeadOfferInitiatedStatus(long LeadId)
        {
            return await _leadManager.GetLeadOfferInitiatedStatus(LeadId);
        }

        [HttpPost]
        [Route("NbfcOfferAccepted")]
        public async Task<ResultViewModel<bool>> NbfcOfferAccepted(NbfcOfferRequestDc obj)
        {
            return await _leadManager.NbfcOfferAccepted(obj);
        }
        #endregion

        [HttpPost]
        [Route("OfferReject")]
        public async Task<ResultViewModel<string>> OfferReject(OfferRejectDc obj)
        {
            return await _leadManager.OfferReject(obj);
        }

        [HttpPost]
        [Route("GetLeadUpdateHistorie")]
        public async Task<List<LeadUpdateHistorieDTO>> GetLeadUpdateHistorie(LeadUpdateHistorieRequestDTO obj)
        {
            return await _leadManager.GetLeadUpdateHistorie(obj);
        }


        [HttpPost]
        [Route("ProceedCreditApplication")]
        public async Task<GRPCReply<string>> ProceedCreditApplication(long leadId)
        {
            return await _leadManager.ProceedCreditApplication(leadId);
        }


        #region Change KYCReject to KYCSuccess
        //[HttpPost]
        //[Route("UpdateKYCStatus")]
        //public async Task<bool> UpdateKYCStatus(ManageKYCStatusDc request)
        //{
        //    return await _leadManager.UpdateKYCStatus(request,UserId);
        //}

        #endregion
    }
}
