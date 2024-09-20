using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.LeadAPI.Helper.NBFC;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.NBFCFactory;
using ScaleUP.Services.LeadAPI.NBFCFactory.Implementation;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using System.Runtime.ConstrainedExecution;

namespace ScaleUP.Services.LeadAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NBFCController : ControllerBase
    {
        private readonly LeadNBFCFactory _leadNBFCFactory;

        private IHostEnvironment _hostingEnvironment;
        private readonly LeadApplicationDbContext _context;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly LeadCommonRequestResponseManager _leadCommonRequestResponseManager;
        private readonly ArthMateNBFCHelper _ArthMateNBFCHelper;
        private readonly ArthMateNBFCService ser;
        //private readonly DateConvertHelper _DateConvertHelper;
        public NBFCController(LeadApplicationDbContext context, LeadNBFCSubActivityManager leadNBFCSubActivityManager, LeadCommonRequestResponseManager leadCommonRequestResponseManager
            , IHostEnvironment hostingEnvironment, LeadNBFCFactory leadNBFCFactory, ArthMateNBFCService _ser)
        {
            _context = context;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _leadCommonRequestResponseManager = leadCommonRequestResponseManager;
            _ArthMateNBFCHelper = new ArthMateNBFCHelper();
            //_DateConvertHelper = dateConvertHelper;
            _hostingEnvironment = hostingEnvironment;
            _leadNBFCFactory = leadNBFCFactory;
            ser = _ser;
        }


        //public NBFCController(LeadNBFCFactory leadNBFCFactory)
        //{
        //    _leadNBFCFactory = leadNBFCFactory;
        //}

        [Route("CreateLead")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ICreateLeadNBFCResponse> CreateLead()
        {
            string json = @"{
    ""aadharDetail"": {
        ""name"": ""Mayur Jain"",
        ""dob"": ""2001-05-29T00:00:00"",
        ""gender"": ""M"",
        ""houseNumber"": null,
        ""street"": null,
        ""state"": ""Madhya Pradesh"",
        ""country"": ""India"",
        ""pincode"": 465106,
        ""combinedAddress"": ""Ward No. 09 Bhawsar Mohalla Maksi, Badod, Shajapur, Shajapur, Maksi, Maksi, Madhya Pradesh, India, 465106"",
        ""frontImageUrl"": ""https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/0423364a-c05d-4f43-b81a-92d9f46d2a83.png"",
        ""backImageUrl"": ""https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/967d06df-afa4-44d4-9a7b-b7359a02ab59.png"",
        ""maskedAadhaarNumber"": ""XXXX XXXX 7523"",
        ""generatedDateTime"": ""2024-01-26T18:18:37.804"",
        ""subdistrict"": ""Badod"",
        ""emailHash"": null,
        ""mobileHash"": ""3cb9a50af87b0fa3b128d43fa4affe569551845d9adf72b6cc639081afc538ea"",
        ""frontDocumentId"": 774,
        ""backDocumentId"": 774,
        ""uniqueId"": ""340670017523"",
        ""locationID"": 157,
        ""locationAddress"": {
            ""addressLineOne"": """",
            ""addressLineTwo"": "", Maksi"",
            ""addressLineThree"": ""Maksi"",
            ""zipCode"": 465106,
            ""cityName"": ""Shajapur"",
            ""cityId"": 51,
            ""stateName"": ""Madhya Pradesh"",
            ""stateId"": 18,
            ""countryName"": ""India"",
            ""countryId"": 1,
            ""id"": 157,
            ""addressTypeId"": 3,
            ""addressTypeName"": ""Current""
        },
        ""fatherName"": ""Anil Kumar Jain""
    },
    ""panDetail"": {
        ""age"": 0,
        ""dob"": ""2001-05-29T00:00:00"",
        ""dateOfIssue"": ""0001-01-01T00:00:00"",
        ""fatherName"": ""Anil Kumar Jain"",
        ""frontImageUrl"": ""https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/4b5d487d-b9c3-467b-b771-7736908f7fd5.png"",
        ""minor"": false,
        ""nameOnCard"": ""MAYUR JAIN"",
        ""panType"": null,
        ""uniqueId"": ""BYWPJ2375J"",
        ""idScanned"": false,
        ""documentId"": 773
    },
    ""personalDetail"": {
        ""firstName"": ""Mayur"",
        ""lastName"": ""Jain"",
        ""gender"": ""M"",
        ""alternatePhoneNo"": ""1234567813"",
        ""emailId"": ""mayur@gmail.com"",
        ""permanentAddressId"": 158,
        ""currentAddressId"": 159,
        ""permanentAddress"": {
            ""addressLineOne"": ""mayur villa"",
            ""addressLineTwo"": "", Maksi"",
            ""addressLineThree"": """",
            ""zipCode"": 465106,
            ""cityName"": ""Shajapur"",
            ""cityId"": 51,
            ""stateName"": ""Madhya Pradesh"",
            ""stateId"": 18,
            ""countryName"": ""India"",
            ""countryId"": 1,
            ""id"": 158,
            ""addressTypeId"": 4,
            ""addressTypeName"": ""Permanent""
        },
        ""currentAddress"": {
            ""addressLineOne"": ""mayur villa"",
            ""addressLineTwo"": "", Maksi"",
            ""addressLineThree"": """",
            ""zipCode"": 465106,
            ""cityName"": ""Shajapur"",
            ""cityId"": 51,
            ""stateName"": ""Madhya Pradesh"",
            ""stateId"": 18,
            ""countryName"": ""India"",
            ""countryId"": 1,
            ""id"": 159,
            ""addressTypeId"": 3,
            ""addressTypeName"": ""Current""
        },
        ""mobileNo"": ""8435761559"",
        ""ownershipTypeProof"": ""Electricity Manual Bill Upload"",
        ""ownershipType"": ""Owned by parents"",
        ""marital"": ""UM"",
        ""electricityBillDocumentId"": 777,
        ""ivrsNumber"": null,
        ""ownershipTypeName"": null,
        ""ownershipTypeAddress"": null,
        ""ownershipTypeResponseId"": null,
        ""manualElectricityBillImage"": ""https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/970e21d2-dd08-4421-88e6-a2f9babb375c.pdf""
    },
    ""buisnessDetail"": {
        ""businessName"": ""RAVI TRADERS"",
        ""doi"": ""2017-07-01T00:00:00"",
        ""busGSTNO"": ""29AALFR4681B1ZZ"",
        ""busEntityType"": ""Proprietorship"",
        ""busPan"": ""BYWPJ2375J"",
        ""currentAddressId"": 160,
        ""currentAddress"": {
            ""addressLineOne"": ""1157/1, , A P M C YARD C BLOCK"",
            ""addressLineTwo"": ""A P M C YARD C BLOCK"",
            ""addressLineThree"": """",
            ""zipCode"": 577003,
            ""cityName"": ""Indore"",
            ""cityId"": 1,
            ""stateName"": ""Madhya Pradesh"",
            ""stateId"": 18,
            ""countryName"": ""India"",
            ""countryId"": 1,
            ""id"": 160,
            ""addressTypeId"": 3,
            ""addressTypeName"": ""Current""
        },
        ""incomeSlab"": ""5 lac- 25 lac"",
        ""buisnessMonthlySalary"": 876867
    },
    ""bankStatementDetail"": {
        ""pdfPassword"": null,
        ""documentId"": null,
        ""borroBankName"": null,
        ""borroBankIFSC"": null,
        ""borroBankAccNum"": null,
        ""enquiryAmount"": null,
        ""accType"": null,
        ""bankOrGSTImageUrl"": null,
        ""umrn"": null
    },
    ""msmeDetail"": null,
    ""selfieDetail"": {
        ""frontDocumentId"": 776,
        ""frontImageUrl"": ""https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/b81279f6-0343-4bd8-88da-2282281a8974.png""
    },
    ""bankStatementCreditLendingDeail"": null,
    ""creditBureauDetails"": null,
    ""agreementDetail"": null,
    ""userId"": null
}";

            var userDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDetailsReply>(json);

            var service = _leadNBFCFactory.GetService(LeadNBFCConstants.BlackSoil.ToString());
            //return await service.CreateLead(0, true);
            return null;
        }

        [Route("SendToLos")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ICreateLeadNBFCResponse> SendToLos(string BusinessId)
        {
            var service = _leadNBFCFactory.GetService(LeadNBFCConstants.BlackSoil.ToString());
            return null;
            //return await service.SendToLos(BusinessId, 0, true);
        }

        #region ArthMate Testing apis


        [Route("TestLead")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> LeadGenerate(long leadid, long nbfcCompanyId)
        {
            var service = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());
            var res = await service.GenerateOffer(leadid, nbfcCompanyId);
            return true;

        }
        
        [Route("TestLoanGenerate")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> LoanGenerate(long leadid, long nbfcCompanyId)
        {
            var service = _leadNBFCFactory.GetService(LeadNBFCConstants.ArthMate.ToString());

            var res = await service.GenerateOffer(leadid, nbfcCompanyId);
            return true;

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("TestArthmateLoanDocument")]
        public async Task<bool> TestArthmateLoanDocument(long Leadmasterid, long NBFCCompanyId)
        {
            //ArthMateNBFCService ser = new ArthMateNBFCService(_context, _leadNBFCSubActivityManager, _leadCommonRequestResponseManager, _hostingEnvironment);
            var res = await ser.TestArthmateLoanDocument(Leadmasterid, NBFCCompanyId);
            return true;
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("TestLoanGenerate")]
        //public async Task<bool> TestLoanGenerate(long Leadmasterid, long NBFCCompanyId)
        //{
        //    var res = await ser.TestLoanGenerate(Leadmasterid, NBFCCompanyId);
        //    return true;
        //}

        [AllowAnonymous]
        [HttpGet]
        [Route("TestMonthlySalary")]
        public async Task<bool> TestMonthlySalary(long Leadmasterid)
        {
            //ArthMateNBFCService ser = new ArthMateNBFCService(_context, _leadNBFCSubActivityManager, _leadCommonRequestResponseManager, _hostingEnvironment);
            var res = await ser.TestMonthlySalary(Leadmasterid);
            return true;
        }

        [Route("TestArthmateDocValidationJsonXml")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> ArthmateDocValidationJsonXml(long leadid, long NBFCCompanyId)
        {
            LeadNBFCSubActivityDTO api = new LeadNBFCSubActivityDTO { Sequence = 0 };
            var res = await ser.ArthmateDocValidationJsonXml(api, leadid, NBFCCompanyId, false);
            return true;
        }


        [Route("TestAadhaarOtpGenerate")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> AadhaarOtpGenerate(long leadid)
        {

            var res = await ser.GenerateOtpToAcceptOffer(leadid);
            return true;
        }

        //[Route("TestAadhaarOtpVerify")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<bool> AadhaarOtpVerify(SecondAadharXMLDc AadharObj)
        //{
            
        //    var res = await ser.AcceptOfferWithXMLAadharOTP(AadharObj);
        //    return true;
        //}

        [Route("TestGetLeadMasterByLeadId")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CommonResponseDc> GetLeadMasterByLeadId(long leadid)
        {

            var res = await ser.GetLeadMasterByLeadId(leadid);
            return res;
        }

        [Route("Testdeserialise")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> deserialise(long leadid)
        {
            var r1 = "{\"success\":true,\"message\":\"Lead generated successfully\",\"data\":{\"preparedbiTmpl\":[{\"partner_loan_app_id\":\"LN/2024/6-9926303404\",\"partner_borrower_id\":\"LN/2024/6/00001\",\"loan_app_id\":\"SKUBL-9632634789079\",\"borrower_id\":\"BRIPS8543F\"}]}}";
            var res = JsonConvert.DeserializeObject<LeadResponseDc>(r1);
            return true;

        }
        #endregion
    }
}
