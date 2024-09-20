using Google.Protobuf.WellKnownTypes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using ScaleUP.Global.Infrastructure.Helper.LeadHistoryHelper;
using ScaleUP.Services.KYCAPI.Persistence;
using ScaleUP.Services.KYCModels.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScaleUP.Services.KYCAPI.Managers
{
    public class KYCHistoryManager : BaseManager
    {
        //private readonly ApplicationDbContext _context;
        //public KYCHistoryManager(ApplicationDbContext context)
        //{
        //    _context = context;
        //}
        private List<string> panFields { get; set; }
        private List<string> aadharFields { get; set; }
        private List<string> gstFields { get; set; }
        private List<string> personalDetailFields { get; set; }
        private List<string> bankStatementFields { get; set; }
        private List<string> buisnessDetailFields { get; set; }
        private List<string> msmeFields { get; set; }
        private List<string> selfieFields { get; set; }
        private List<string> bankStatementCreditLendingFields { get; set; }
        private List<string> dsaProfileTypeFields { get; set; }
        private List<string> dsaPersonalDetailFields { get; set; }
        private List<string> connectorPersonalDetailFields { get; set; }

        public KYCHistoryManager(ApplicationDbContext context) : base(context)
        {
            panFields = new List<string>() { KYCDetailConstants.Age, KYCDetailConstants.DOB 
                    , KYCDetailConstants.FatherName, KYCDetailConstants.NameOnCard
                    , KYCDetailConstants.PanDocId, KYCDetailConstants.DocumentId};

            aadharFields = new List<string>() { KYCAadharConstants.Name, KYCAadharConstants.Gender
            , KYCAadharConstants.FrontDocumentId, KYCAadharConstants.BackDocumentId, KYCAadharConstants.DOB, KYCAadharConstants.FatherName
            , KYCAadharConstants.DocumentNumber
            //, KYCAadharConstants.HouseNumber, KYCAadharConstants.Street, KYCAadharConstants.Landmark
            //, KYCAadharConstants.Subdistrict , KYCAadharConstants.District , KYCAadharConstants.State, KYCAadharConstants.Country , KYCAadharConstants.Pincode, KYCAadharConstants.CombinedAddress, KYCAadharConstants.VtcName
            };


            gstFields = new List<string>() { GstConstant.Gst, GstConstant.Gst18, GstConstant.Gst5, GstConstant.Tds, GstConstant.Tds5, GstConstant.Tds10 };


            personalDetailFields = new List<string>()
            { KYCPersonalDetailConstants.FirstName , KYCPersonalDetailConstants.MiddleName, KYCPersonalDetailConstants.LastName
            , KYCPersonalDetailConstants.FatherName, KYCPersonalDetailConstants.FatherLastName,KYCPersonalDetailConstants.AlternatePhoneNo
            , KYCPersonalDetailConstants.EmailId, KYCPersonalDetailConstants.PermanentAddressId, KYCPersonalDetailConstants.CurrentAddressId
            , KYCPersonalDetailConstants.IncomeSlab, KYCPersonalDetailConstants.OwnershipType, KYCPersonalDetailConstants.MobileNo
            , KYCPersonalDetailConstants.Marital, KYCPersonalDetailConstants.OwnershipTypeProof, KYCPersonalDetailConstants.ElectricityBillDocumentId
            , KYCPersonalDetailConstants.IVRSNumber, KYCPersonalDetailConstants.OwnershipTypeName, KYCPersonalDetailConstants.OwnershipTypeAddress
            , KYCPersonalDetailConstants.OwnershipTypeResponseId, KYCPersonalDetailConstants.ElectricityServiceProvider
            , KYCPersonalDetailConstants.ElectricityState, KYCPersonalDetailConstants.SurrogateType};

            bankStatementFields = new List<string>()
            { KYCBankStatementConstants.PdfPassword, KYCBankStatementConstants.DocumentId, KYCBankStatementConstants.BorroBankName, KYCBankStatementConstants.BorroBankIFSC
            , KYCBankStatementConstants.BorroBankAccNum , KYCBankStatementConstants.EnquiryAmount, KYCBankStatementConstants.AccType};

            buisnessDetailFields = new List<string>()
            { KYCBuisnessDetailConstants.BusinessName, KYCBuisnessDetailConstants.BusGSTNO, KYCBuisnessDetailConstants.DOI, KYCBuisnessDetailConstants.BusPan
            , KYCBuisnessDetailConstants.BusEntityType, KYCBuisnessDetailConstants.CurrentAddressId, KYCBuisnessDetailConstants.BuisnessMonthlySalary
            , KYCBuisnessDetailConstants.IncomeSlab, KYCBuisnessDetailConstants.OwnershipType, KYCBuisnessDetailConstants.CustomerElectricityNumber
            , KYCBuisnessDetailConstants.BuisnessProof, KYCBuisnessDetailConstants.BuisnessProofDocId, KYCBuisnessDetailConstants.BuisnessDocumentNo
            ,KYCBuisnessDetailConstants.InquiryAmount, KYCBuisnessDetailConstants.SurrogateType, KYCBuisnessDetailConstants.BuisnessPhotos
            , KYCBuisnessDetailConstants.BuisnessPhotoDocId};

            msmeFields = new List<string>()
            { KYCMSMEConstants.FrontDocumentId, KYCMSMEConstants.BusinessName, KYCMSMEConstants.BusinessType, KYCMSMEConstants.Vintage, KYCMSMEConstants.MSMERegNum };

            selfieFields = new List<string>()
            { SelfieConstant.FrontDocumentId };

            bankStatementCreditLendingFields = new List<string>()
            { KYCBankStatementCreditLendingConstant.DocumentId, KYCBankStatementCreditLendingConstant.SarrogateDocId, KYCBankStatementCreditLendingConstant.SurrogateType };

            dsaProfileTypeFields = new List<string>()
            { DSAProfileTypeConstants.DSA, DSAProfileTypeConstants.Connector, DSAProfileTypeConstants.DSAUser };


            dsaPersonalDetailFields = new List<string>()
            {DSAPersonalDetailConstants.GSTRegistrationStatus, DSAPersonalDetailConstants.GSTNumber, DSAPersonalDetailConstants.FirmType, DSAPersonalDetailConstants.BuisnessDocument
            , DSAPersonalDetailConstants.DocumentId, DSAPersonalDetailConstants.CompanyName,DSAPersonalDetailConstants.FullName, DSAPersonalDetailConstants.FatherOrHusbandName
            ,DSAPersonalDetailConstants.CurrentAddressId,DSAPersonalDetailConstants.PermanentAddressId, DSAPersonalDetailConstants.AlternatePhoneNo
            , DSAPersonalDetailConstants.EmailId, DSAPersonalDetailConstants.PresentOccupation , DSAPersonalDetailConstants.NoOfYearsInCurrentEmployment
            , DSAPersonalDetailConstants.Qualification, DSAPersonalDetailConstants.LanguagesKnown, DSAPersonalDetailConstants.WorkingWithOther
            , DSAPersonalDetailConstants.ReferneceName , DSAPersonalDetailConstants.ReferneceContact , DSAPersonalDetailConstants.MobileNo };

            connectorPersonalDetailFields = new List<string>()
            { ConnectorPersonalDetailConstants.FullName,ConnectorPersonalDetailConstants.FatherName, ConnectorPersonalDetailConstants.CurrentAddressId
            ,ConnectorPersonalDetailConstants.AlternateContactNumber, ConnectorPersonalDetailConstants.EmailId, ConnectorPersonalDetailConstants.PresentEmployment
            , ConnectorPersonalDetailConstants.LanguagesKnown,ConnectorPersonalDetailConstants.WorkingWithOther , ConnectorPersonalDetailConstants.ReferneceName
            , ConnectorPersonalDetailConstants.ReferneceContact , ConnectorPersonalDetailConstants.WorkingLocation , ConnectorPersonalDetailConstants.MobileNo };
        }

        public async Task<KYCHistory> GetKycHistroy(long entityID, string doctype)
        {
            string res = "";
            var query = from a in _context.AuditLogs
                        where a.EntityName == "KYCMasterInfo" //&& a.Id == 13784
                        && a.EntityId == entityID
                        select new KycHistroyResponse
                        {
                            Id = a.Id,
                            UserId = a.UserId,
                            EntityId = a.EntityId,
                            DatabaseName = a.DatabaseName,
                            EntityName = a.EntityName,
                            Action = a.Action,
                            Timestamp = a.Timestamp,
                            Changes = a.Changes
                        };
            KycHistroyResponse kycHistroyResponse = query.OrderByDescending(x => x.Id).FirstOrDefault();

            KYCHistory histroy = new KYCHistory();

            if (kycHistroyResponse != null)
            {
                //foreach (var item in kycHistroyResponse)
                {
                    string aContent = kycHistroyResponse.Changes;//  @"Id: 29\nAPIConfigId: 1\nCompanyId: 0\nCreated: 03/20/2024 14:10:45\nCreatedBy: \nDeleted: \nDeletedBy: \nIsActive: True\nIsDeleted: False\nIsError: False\nLastModified: \nLastModifiedBy: \nProcessedResponse: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Response_5e5192d6-9f2b-4c03-8db1-3e4a2ebd1bbe.json\nRequest: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Request_6ee7e9e8-0f9f-4822-af5a-674b06dee8f1.json\nResponse: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Response_5e5192d6-9f2b-4c03-8db1-3e4a2ebd1bbe.json\nUserId: 79b978fd-8b78-48eb-8472-63608166c46c\n";
                    string action = kycHistroyResponse.Action; // "Added";
                    LeadHistoryHelper helper = new LeadHistoryHelper();
                    histroy.MasterFieldChanges = helper.GetHistroy(aContent, action);

                    List<KYCDetailInfoDC> KYCDetailInfolist = new List<KYCDetailInfoDC>();
                    KYCDetailInfolist = await GetKYCDetailInfoByTimeStamp(kycHistroyResponse.Timestamp, entityID);
                    if (KYCDetailInfolist != null && KYCDetailInfolist.Any())
                    {
                        histroy.DetailFieldChanges = new List<LeadAuditContent>();
                        foreach (var itemVal in KYCDetailInfolist)
                        {
                            var detailHistory = helper.GetHistroy(itemVal.Changes, itemVal.Action);
                            if (detailHistory != null && detailHistory.Any(x => x.FieldName == "FieldValue"))
                            {
                                histroy.DetailFieldChanges.Add(new LeadAuditContent
                                {
                                    FieldName = itemVal.KYCDetails_Field,
                                    FieldOldValue = detailHistory.First(x => x.FieldName == "FieldValue").FieldOldValue,
                                    FieldNewValue = detailHistory.First(x => x.FieldName == "FieldValue").FieldNewValue,
                                    FieldInfoType = itemVal.FieldInfoType,
                                });
                            }
                        }

                        //DateTime myDate = DateTime.ParseExact(kycHistroyResponse.Timestamp, "yyyy-MM-dd HH:mm:ss,fff",
                        //               System.Globalization.CultureInfo.InvariantCulture);

                        histroy.Action = kycHistroyResponse.Action;
                        histroy.UserId = kycHistroyResponse.UserId;
                        histroy.EntityIDofKYCMaster = kycHistroyResponse.EntityId;
                        histroy.CreatedTimeStamp = kycHistroyResponse.Timestamp; 
                    }


                    List<string> PANAAdharUsedKeys = new List<string>() { "UniqueId", "UserId" };

                    List<string> OtherDocUsedKeys = new List<string>() { "UserId" };


                    if (histroy.MasterFieldChanges != null && (doctype == Global.Infrastructure.Constants.KYCMasterConstants.PAN || doctype == Global.Infrastructure.Constants.KYCMasterConstants.Aadhar))
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => PANAAdharUsedKeys.Contains(x.FieldName)).ToList();
                    else if(histroy.MasterFieldChanges != null)
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => OtherDocUsedKeys.Contains(x.FieldName)).ToList();

                    if (histroy.DetailFieldChanges != null)
                        histroy.DetailFieldChanges = RemoveDetailFields(histroy.DetailFieldChanges, doctype);

                    histroy.Narretion = helper.GetNarretion(kycHistroyResponse.Action, histroy);
                    histroy.NarretionHTML = helper.GetNarretionHtml(kycHistroyResponse.Action, histroy);
                }
            }


            return histroy;
        }

        private List<LeadAuditContent> RemoveDetailFields(List<LeadAuditContent> detailList, string docType)
        {
            List<LeadAuditContent> newList = null;
            if (detailList != null)
            {
                switch (docType)
                {
                    case Global.Infrastructure.Constants.KYCMasterConstants.PAN:
                        newList = detailList.Where(x => panFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.Aadhar:
                        newList = detailList.Where(x => aadharFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.GST:
                        newList = detailList.Where(x => gstFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.PersonalDetail:
                        newList = detailList.Where(x => personalDetailFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.BankStatement:
                        newList = detailList.Where(x => bankStatementFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.BuisnessDetail:
                        newList = detailList.Where(x => buisnessDetailFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.MSME:
                        newList = detailList.Where(x => msmeFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.Selfie:
                        newList = detailList.Where(x => selfieFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.BankStatementCreditLending:
                        newList = detailList.Where(x => bankStatementCreditLendingFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.DSAProfileType:
                        newList = detailList.Where(x => dsaProfileTypeFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.DSAPersonalDetail:
                        newList = detailList.Where(x => dsaPersonalDetailFields.Contains(x.FieldName)).ToList();
                        break;
                    case KYCMasterConstants.ConnectorPersonalDetail:
                        newList = detailList.Where(x => connectorPersonalDetailFields.Contains(x.FieldName)).ToList();
                        break;

                    default:
                        break;
                }
            }
            return newList;

        }


        public async Task<List<KYCDetailInfoDC>> GetKYCDetailInfoByTimeStamp(DateTime timeStamp, long kycMasterInfoId)
        {
            //CommonResponse result = new CommonResponse();

            List<KYCDetailInfoDC> list = new List<KYCDetailInfoDC>();
            var timeStampValue = new SqlParameter("Timestamp", timeStamp);

            var kycMasterInfoValue = new SqlParameter("KYCMasterInfoId", kycMasterInfoId);

            try
            {
                list = _context.Database.SqlQueryRaw<KYCDetailInfoDC>("exec GetKYCDetailInfo @Timestamp, @KYCMasterInfoId", timeStampValue, kycMasterInfoValue).AsEnumerable().ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            //if (list!=null && list.Any())
            //{
            //    result.Result = list;
            //    result.status = true;
            //    result.Message = "Success";
            //}
            //else
            //{
            //    result.Result = new List<KYCDetailInfoDC>();
            //    result.status = false;
            //    result.Message = "Not Found";
            //}

            return list;
        }





        public class CommonResponse
        {
            public bool status { get; set; }
            public string Message { get; set; }
            public object Result { get; set; }
        }



    }
}
