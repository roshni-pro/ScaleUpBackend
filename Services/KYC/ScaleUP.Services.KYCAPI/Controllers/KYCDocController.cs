using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Helper.LeadHistoryHelper;
using ScaleUP.Services.KYCAPI.Helpers;
using ScaleUP.Services.KYCAPI.KYCFactory;
using ScaleUP.Services.KYCAPI.Managers;
using ScaleUP.Services.KYCAPI.Migrations;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCDTO.Constant;
using ScaleUP.Services.KYCDTO.Transacion;
using System.Globalization;
using System.Reflection.Metadata;
using static MassTransit.ValidationResultExtensions;

namespace ScaleUP.Services.KYCAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class KYCDocController : BaseController
    {
        private string basePath;
        private IHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly KycGrpcManager _kycGrpcManager;
        private readonly KYCMasterInfoManager _kYCMasterInfoManager;
        private readonly KarzaPanProfileHelper _karzaPanProfileHelper;
        private readonly KYCHistoryManager _kYCHistoryManager;
        //private readonly KarzaElectricityHelper _karzaElectricityHelper;

        //private IDocType<KarzaPANDTO, KYCActivityPAN> docType;
        //private IDocType<KarzaAadharDTO, KYCActivityAadhar> AadharType;
        //private IDocType<AppyFlowGSTDTO, KYCActivityGST> GstType;
        //private IDocType<BusinessDetailDTO, BusinessActivityDetail> BuisnessType;

        //private readonly IPublisher _publisher;
        public KYCDocController(ApplicationDbContext context , IHostEnvironment hostingEnvironment, KycGrpcManager kycGrpcManager, KYCMasterInfoManager kYCMasterInfoManager, KarzaPanProfileHelper karzaPanProfileHelper, KYCHistoryManager kYCHistoryManager)
        {
            _context = context;
            //KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context);
            //docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            //AadharType = kYCDocFactory.GetDocType<KarzaAadharDTO, KYCActivityAadhar>(Global.Infrastructure.Constants.KYCMasterConstants.Aadhar);
            //GstType = kYCDocFactory.GetDocType<AppyFlowGSTDTO, KYCActivityGST>(Global.Infrastructure.Constants.KYCMasterConstants.GST);
            //_publisher = publisher;
            _hostingEnvironment = hostingEnvironment;
            _kycGrpcManager = kycGrpcManager;
            _kYCMasterInfoManager = kYCMasterInfoManager;
            _karzaPanProfileHelper = karzaPanProfileHelper;
            //_karzaElectricityHelper = karzaElectricityHelper;
            _kYCHistoryManager = kYCHistoryManager;
        }

        [HttpPost]
        [Route("SaveDoc")]
        public async Task<ActionResult<long>> SaveDoc(KarzaPANDTO command)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", basePath);
            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            await docType.SaveDoc(command, "2", UserId, "");
            return 1;


        }
        [HttpPost]
        [Route("GetAndSaveDoc")]
        public async Task<ActionResult<long>> GetAndSaveDoc(SaveKYCDocDTO<KYCActivityPAN> input)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", basePath);
            IDocType<KarzaPANDTO, KYCActivityPAN> docType = kYCDocFactory.GetDocType<KarzaPANDTO, KYCActivityPAN>(Global.Infrastructure.Constants.KYCMasterConstants.PAN);
            var res = await docType.GetAndSaveByUniqueId(input.Input, input.UserId, UserId,"");
            return res.Result;


        }
        [HttpPost]
        [Route("GetLeadAadharGenerateOTP")]
        public async Task<eAadhaarDigilockerResponseDc> GetLeadAadharGenerateOTP(KYCActivityAadhar kycAdhaarInfo)
        {
            string userId = UserId;
            eAadhaarDigilockerResponseDc eAadhaarDigilockerResponseDc = new eAadhaarDigilockerResponseDc();
            try
            {
                basePath = _hostingEnvironment.ContentRootPath;
                KarzaAadharHelper KarzaAadharHelper = new KarzaAadharHelper(_context);
                eAadhaarDigilockerResponseDc = await KarzaAadharHelper.eAdharDigilockerOTPXml(kycAdhaarInfo, basePath, userId);
                return eAadhaarDigilockerResponseDc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return eAadhaarDigilockerResponseDc;
        }
        [HttpPost]
        [Route("GetAndSaveEAadharVerifyDoc")]
        public async Task<ActionResult<bool>> GetAndSaveEAadharVerifyDoc(SaveKYCDocDTO<KYCActivityAadhar> getAdharDc)
        {
            bool res = false;
            basePath = _hostingEnvironment.ContentRootPath;
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, getAdharDc.UserId, basePath);
            IDocType<KarzaAadharDTO, KYCActivityAadhar> AadharType = kYCDocFactory.GetDocType<KarzaAadharDTO, KYCActivityAadhar>(Global.Infrastructure.Constants.KYCMasterConstants.Aadhar);


            //string jsonString = JsonConvert.SerializeObject(getAdharDc.Input);
            ResultViewModel<bool> returnId = await AadharType.ValidateByUniqueId(getAdharDc.Input);
            return returnId.Result;

        }
        [HttpPost]
        [Route("GetAndSaveEAadharOTPVerifyDoc")]
        public async Task<ActionResult<long>> GetAndSaveEAadharOTPVerifyDoc(SaveKYCDocDTO<KYCActivityAadhar> getAdharDc)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, getAdharDc.UserId, basePath);
            IDocType<KarzaAadharDTO, KYCActivityAadhar> AadharType = kYCDocFactory.GetDocType<KarzaAadharDTO, KYCActivityAadhar>(Global.Infrastructure.Constants.KYCMasterConstants.Aadhar);

            //string jsonString = JsonConvert.SerializeObject(getAdharDc.Input);
            var res = await AadharType.GetAndSaveByUniqueId(getAdharDc.Input, getAdharDc.UserId, UserId, "");
            return res.Result;

        }

        [HttpGet]
        [Route("GetGstInfo")]
        public async Task<AppyFlowGSTDTO> GetGstInfo(string input)
        {
            try
            {
                AppyFlowGSTDTO appyFlowGSTDTO = new AppyFlowGSTDTO
                {
                    BusinessName = "TestBusinessName",
                    LandingName = "TestLandingName"
                };
                return appyFlowGSTDTO;
                //KYCActivityGST gst = new KYCActivityGST { UniqueId = input };
                //var res = await GstType.GetByUniqueId(gst);
                //return null;
            }
            catch (Exception)
            {
                throw;
            }

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDoc")]
        public async Task<bool> GetDoc()
        {
            //await BuisnessType.GetDoc("2");
            return true;


        }

        [HttpPost]
        [Route("AllResponse")]
        public async Task<List<KYCAllInfoResponse>> GetAllKycInfo(KYCAllInfoRequest kYCAllInfoRequest)
        {
            //KYCMasterInfoManager kYCMasterInfoManager = new KYCMasterInfoManager(_context);
            var res = await _kYCMasterInfoManager.GetAllKycInfo(kYCAllInfoRequest);
            return res;


        }
        [HttpPost]
        [Route("IsPANDocumentExist")]
        public async Task<ValidAuthenticationPanResDTO> IsPANDocumentExist(string PanNumber, long LeadMasterId)
        {
            ValidAuthenticationPanResDTO ValidAuthenticationPanResDTO = new ValidAuthenticationPanResDTO();
            try
            {
                KarzaHelper KarzaHelper = new KarzaHelper(_context);
                ValidAuthenticationPanResDTO = await KarzaHelper.ValidAuthenticationPan(PanNumber, LeadMasterId,basePath);
                return ValidAuthenticationPanResDTO;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ValidAuthenticationPanResDTO;
        }


        [HttpPost]
        [Route("PostLeadAadharVerifyOTP")]
        public async Task<KarzaAadharDTO> PostLeadAadharVerifyOTP(KYCActivityAadhar updateAadhaarVerificationRequestDC)
        {
            ResultViewModel<KarzaAadharDTO> res = new ResultViewModel<KarzaAadharDTO>();
            KarzaAadharDTO karzaAadharDTO = new KarzaAadharDTO();
            basePath = _hostingEnvironment.ContentRootPath;
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", basePath);

            IDocType<KarzaAadharDTO, KYCActivityAadhar> docTypeAadhar = kYCDocFactory.GetDocType<KarzaAadharDTO, KYCActivityAadhar>(KYCMasterConstants.Aadhar);
            res = await docTypeAadhar.GetByUniqueId(updateAadhaarVerificationRequestDC);
            return res.Result;
        }

        [HttpPost]
        [Route("IsValidateAadhaar")]
        public async Task<bool> ValidateByUniqueId(KYCActivityAadhar kYCActivityAadhar)
        {
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", basePath);
           
            IDocType<KarzaAadharDTO, KYCActivityAadhar> docTypeAadhar = kYCDocFactory.GetDocType<KarzaAadharDTO, KYCActivityAadhar>(KYCMasterConstants.Aadhar);
            ResultViewModel<bool> res = await docTypeAadhar.ValidateByUniqueId(kYCActivityAadhar);
            return res.Result;
        }


        [HttpGet]
        [Route("GetLeadValidPanCard")]
        public async Task<ValidPanResDTO> GetLeadValidPanCard(string PanNumber)
        {
            try
            {
                ValidPanResDTO validPanResDTO = new ValidPanResDTO();
                basePath = _hostingEnvironment.ContentRootPath;
                KarzaHelper KarzaHelper = new KarzaHelper(_context);
                var ValidAuthenticationPanres = await KarzaHelper.ExistValidAuthenticationPan(PanNumber, basePath, UserId);
                if (ValidAuthenticationPanres.StatusCode != 101 || ValidAuthenticationPanres.error != null) //101 mean succesfully 
                {
                    validPanResDTO.Message = ValidAuthenticationPanres.error;
                    return validPanResDTO;
                }
                else
                {
                    validPanResDTO.NameOnPancard = ValidAuthenticationPanres.person.name;
                    validPanResDTO.Message = "Valid Pancard.";
                    return validPanResDTO;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
          
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("SpecificDetail")]
        public async Task<Dictionary<string, List<KYCSpecificDetailResponse>>> GetKYCSpecificDetail()
        {
            GRPCRequest<KYCSpecificDetailRequest> request = new GRPCRequest<KYCSpecificDetailRequest>();
            request.Request = new KYCSpecificDetailRequest();
            request.Request.KYCReqiredFieldList = new Dictionary<string, List<string>>();
            request.Request.KYCReqiredFieldList.Add(KYCMasterConstants.PersonalDetail, new List<string>
            {
                KYCPersonalDetailConstants.FirstName,
                KYCPersonalDetailConstants.MiddleName,
                KYCPersonalDetailConstants.LastName,
                KYCPersonalDetailConstants.EmailId
            });
            //request.Request.UserIdList = new List<string> { "8b63df6d-6690-47a8-809f-c41bd23d6f1b" };

            //var res = await _kycGrpcManager.GetKYCSpecificDetail(request);
            return null;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ProfilePan")]
        public async Task<KarzaPanProfileResponseDTO> KarzaPanProfile(string PanNo)
        {
            string basePath = this.basePath; 
            string userid = UserId;
            var data = await _karzaPanProfileHelper.KarzaPanProfile(PanNo, basePath, userid);
            return data;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetFathersNameByValidPanCard")]
        public async Task<ValidPanResDTO> GetFathersNameByValidPanCard(string PanNumber)
        {
            try
            {
                string basePath = this.basePath;
                string userid = UserId;
                ValidPanResDTO validPanResDTO = new ValidPanResDTO();
                basePath = _hostingEnvironment.ContentRootPath;
                KarzaHelper KarzaHelper = new KarzaHelper(_context);
                var ValidAuthenticationPanres = await _karzaPanProfileHelper.KarzaPanProfile(PanNumber, basePath, userid);
                if (ValidAuthenticationPanres.statusCode != 101) //101 mean succesfully 
                {
                    validPanResDTO.StatusCode = ValidAuthenticationPanres.statusCode;
                    validPanResDTO.Message = ValidAuthenticationPanres.message;
                    return validPanResDTO;
                }
                else
                {
                    validPanResDTO.NameOnPancard = ValidAuthenticationPanres.result.name;
                    validPanResDTO.FathersNameOnPancard = ValidAuthenticationPanres.result.fathersName;
                    validPanResDTO.dob = ValidAuthenticationPanres.result.dob;
                    validPanResDTO.StatusCode = ValidAuthenticationPanres.statusCode;
                    validPanResDTO.Message = "Pan Validate Successfully!!";
                    return validPanResDTO;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("RemoveKYCMasterInfo")]
        public async Task<GRPCReply<bool>> RemoveKYCMasterInfo(string userid)
        {
            var RemoveData = await _kYCMasterInfoManager.RemoveKYCMasterInfo(new GRPCRequest<string> { Request =  userid });
            return RemoveData;
        }

        [HttpGet]
        [Route("EmailExist")]
        [AllowAnonymous]
        public async Task<ResultViewModel<bool>> EmailExist(string UserId, string EmailId)
        {
            var result = await _kYCMasterInfoManager.IsValidTOSave(UserId, EmailId);
            return result;
        }

        [HttpGet]
        [Route("QuickHash")]
        [AllowAnonymous]
        public string QuickHash(string input)
        {
            HashHelper hashHelper = new HashHelper();   
            var result = hashHelper.QuickHash(input);
            return result;
        }

        [HttpPost]
        [Route("UpdateSelfieDoc")]
        public async Task<ResultViewModel<long>> UpdateSelfieDoc(SelfeiActivity input, string userId)
        {
            ResultViewModel<long> result = null;
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", basePath);
            IDocType<SelfieDTO, SelfeiActivity> docType = kYCDocFactory.GetDocType<SelfieDTO, SelfeiActivity>(Global.Infrastructure.Constants.KYCMasterConstants.Selfie);
            result = await docType.GetAndSaveByUniqueId(input, userId, UserId,"");
            return result;

        }

        [HttpPost]
        [Route("UpdateBankStatementCreditLendingDoc")]
        public async Task<ResultViewModel<long>> UpdateBankStatementCreditLendingDoc(BankStatementCreditLendingActivity input, string userId)
        {
            ResultViewModel<long> result = null;
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", basePath);
            IDocType<BankStatementCreditLendingDTO, BankStatementCreditLendingActivity> docType = kYCDocFactory.GetDocType<BankStatementCreditLendingDTO, BankStatementCreditLendingActivity>(Global.Infrastructure.Constants.KYCMasterConstants.BankStatementCreditLending);
            result = await docType.GetAndSaveByUniqueId(input, userId, UserId, "");
            return result;

        }

        [HttpPost]
        [Route("UpdateMSMEDoc")]
        public async Task<ResultViewModel<long>> UpdateMSMEDoc(MSMEActivity input, string userId)
        {
            ResultViewModel<long> result = null;
            KYCDocFactory kYCDocFactory = new KYCDocFactoryConcrete(_context, "1", basePath);
            IDocType<MSMEDTO, MSMEActivity> docType = kYCDocFactory.GetDocType<MSMEDTO, MSMEActivity>(Global.Infrastructure.Constants.KYCMasterConstants.MSME);
            result = await docType.GetAndSaveByUniqueId(input, userId, UserId, "");
            return result;

        }

        [HttpPost]
        [Route("KarzaElectricityAuthentication")]
        [AllowAnonymous]
        public async Task<KarzaElectricityBillAuthenticationDTO> KarzaElectricityAuthentication(KarzaElectricityInputDTO karzaElectricityInputDTO)
        {
            string userId = UserId;
            basePath = _hostingEnvironment.ContentRootPath;
            try
            {
                KarzaElectricityBillAuthenticationDTO karzaElectricityBillAuthenticationDTO = new KarzaElectricityBillAuthenticationDTO();
                KarzaElectricityHelper karzaElectricityHelper = new KarzaElectricityHelper(_context);
                karzaElectricityBillAuthenticationDTO = await karzaElectricityHelper.KarzaElectricity(karzaElectricityInputDTO,basePath,userId);
                return karzaElectricityBillAuthenticationDTO;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet]
        [Route("GetKarzaElectricityServiceProviderList")]
        [AllowAnonymous]
        public async Task<List<KarzaElectricityServiceProviderDTO>> GetKarzaElectricityServiceProviderList()
        {
            List<KarzaElectricityServiceProviderDTO> karzaElectricityServiceProviderDTO = new List<KarzaElectricityServiceProviderDTO>();
            karzaElectricityServiceProviderDTO = _context.ElectricityBillServiceProvider.Where(x => x.IsActive && !x.IsDeleted)
                .Select(x=> new KarzaElectricityServiceProviderDTO
                {
                    Code = x.Code,
                    ServiceProvider = x.ServiceProvider,
                    State = x.State,
                }).Distinct().OrderBy(x => x.ServiceProvider).ToList();
            //if(serverProviderList.Count > 0)
            //{
            //    foreach(var serverData in serverProviderList)
            //    {

            //        KarzaElectricityServiceProviderDTO electricityDTO = new KarzaElectricityServiceProviderDTO
            //        {
            //            Code = serverData.Code,
            //            ServiceProvider = serverData.ServiceProvider,
            //            State = serverData.State,
            //        };
            //        karzaElectricityServiceProviderDTO.Add(electricityDTO);
            //    }

            //}

            return karzaElectricityServiceProviderDTO;


        }
        [HttpGet]
        [Route("GetKarzaElectricityState")]
        [AllowAnonymous]
        public async Task<List<KarzaElectricityStateDTO>> GetKarzaElectricityState(string state)
        {
            List<KarzaElectricityStateDTO> karzaElectricityStateDTOs= new List<KarzaElectricityStateDTO>();
            karzaElectricityStateDTOs = _context.karzaElectricityDistricts.Where(x => x.State == state && x.IsActive && !x.IsDeleted).Select(x=> new KarzaElectricityStateDTO
            {
                DistrictCode = x.DistrictCode,
                DistrictName = x.DistrictName,
                State  = x.State
            }).Distinct().OrderBy(x=>x.DistrictName).ToList();
            //if (stateList.Count > 0)
            //{
            //    foreach (var st in stateList)
            //    {

            //        KarzaElectricityStateDTO stateData = new KarzaElectricityStateDTO
            //        {
            //            DistrictCode = st.DistrictCode,
            //            DistrictName = st.DistrictName,
            //            State = st.State,
            //        };
            //        karzaElectricityStateDTOs.Add(stateData);
            //    }
            //}

            return karzaElectricityStateDTOs;


        }

        [HttpGet]
        [Route("IVRSNumberExist")]
        [AllowAnonymous]
        public async Task<ResultViewModel<bool>> IVRSNumberExist(string UserId, string IVRSNumber)
        {
            var result = await _kYCMasterInfoManager.IsIVRSNumberValidTOSave(UserId, IVRSNumber);
            return result;
        }

        [HttpGet]
        [Route("GetKarzaElectricityStateById")]
        [AllowAnonymous]
        public async Task<List<KarzaElectricityStateDTO>> GetKarzaElectricityStateById(int stateId)
        {
            List<KarzaElectricityStateDTO> karzaElectricityStateDTOs = new List<KarzaElectricityStateDTO>();
            karzaElectricityStateDTOs = _context.karzaElectricityDistricts.Where(x => x.DistrictCode == stateId && x.IsActive && !x.IsDeleted).Select(x => new KarzaElectricityStateDTO
            {
                DistrictCode = x.DistrictCode,
                DistrictName = x.DistrictName,
                State = x.State
            }).Distinct().OrderBy(x => x.DistrictName).ToList();
            return karzaElectricityStateDTOs;


        }
        [HttpGet]
        [Route("DSAEmailExist")]
        [AllowAnonymous]
        public async Task<ResultViewModel<bool>> DSAEmailExist(string UserId, string EmailId, string productCode)
        {
            var result = await _kYCMasterInfoManager.IsDSAEmailExist(UserId, EmailId, productCode);
            return result;
        }
        //[HttpGet]
        //[Route("DSAGSTExist")]
        //public async Task<ResultViewModel<bool>> DSAGSTExist(string UserId, string gst, string productCode)
        //{
        //    var result = await _kYCMasterInfoManager.DSAGSTExist(UserId, gst, productCode);
        //    return result;
        //}


        
        [HttpGet]
        [Route("GetKycHistroy")]
        [AllowAnonymous]
        public async Task<KYCHistory> GetKycHistroy(long entityID, string doctype)
        {
            var result = await _kYCHistoryManager.GetKycHistroy(entityID, doctype);
            return result;
        }

    }
}
