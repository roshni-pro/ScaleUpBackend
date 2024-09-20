using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Helper.LeadHistoryHelper;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadModels;
using System.Runtime.Serialization;
using static iTextSharp.text.pdf.AcroFields;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class LeadHistoryManager
    {
        private readonly LeadApplicationDbContext _context;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private string docType_LeadReject = "LeadReject";
        //private string docType_SendToLosInprogress = "SendToLosInprogress";
        //private string docType_SendToLosComplete = "SendToLosCompleted";
        private string docType_SendToLos = "SendToLos";

        private List<string> bankDetailFields { get; set; }

        public LeadHistoryManager(LeadApplicationDbContext context)
        {
            _context = context;

            bankDetailFields = new List<string>()
            { SelfieConstant.FrontDocumentId };
        }

        public async Task<KYCHistory> GetLeadHistroy(long leadid, string doctype)
        {
            string res = "";
            string docType_KYCAcceptRejectByBackendTeam = "VerifyLeadDocument";
            string docType_OfferAcceptRejectByBackendTeam = "UpdateLeadOffers";
            string docType_OfferAcceptRejectByClient = "GenerateKarzaAadhaarOtpForNBFC";
            string docType_AgrementUpdateStatus = "UpdateDocStatus_PrepareBlackSoil";
            string docType_AgrementESignStatus = "AgreementEsign_BlackSoil";
            string docType_AgrementUpdateStatusArthmate = "SaveArthMateNBFCAgreement_Prepare";
            string docType_AgrementManuallyESignStatusArthmate = "SaveAgreementESignDocument_ManuallyEsignArthmate";
            string docType_AgrementAutoESignStatusArthmate = "eSignDocumentsAsync_EsignArthmate";
            string docType_AddLeadConsentLog_GetLeadForMobile = "LeadConsentLog_MobileOTP";
            string docType_AddLeadConsentLog_PostLeadPAN = "LeadConsentLog_PAN";
            string docType_AddLeadConsentLog_PostLeadAadharVerifyOTP = "LeadConsentLog_Aadhar";
            string docType_AddLeadConsentLog_CheckeSignDocumentStatus = "LeadConsentLog_Agrement";
            string docType_Lead_LGLC = "Lead_LG LC";


            KycHistroyResponse kycHistroyResponse = new KycHistroyResponse();

            if (doctype == "SaveLeadBankDetail")
            {
                var query = from a in _context.AuditLogs
                            join b in _context.LeadBankDetails on a.EntityId equals b.Id
                            where (a.EntityName == "LeadDocumentDetail" || a.EntityName == "LeadBankDetail")
                            && b.LeadId == leadid && b.IsActive == true && b.IsDeleted == false
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

                kycHistroyResponse = query.OrderByDescending(x => x.Id).FirstOrDefault();
            }
            else if (doctype == docType_KYCAcceptRejectByBackendTeam || doctype == docType_OfferAcceptRejectByBackendTeam || doctype == docType_OfferAcceptRejectByClient
                || doctype == docType_AgrementUpdateStatus || doctype == docType_AgrementESignStatus
                || doctype == docType_AgrementAutoESignStatusArthmate || doctype == docType_AgrementUpdateStatusArthmate
                || doctype == docType_LeadReject)
            {
                var query = from a in _context.AuditLogs
                            join p in _context.LeadActivityMasterProgresses on a.EntityId equals p.Id
                            where (a.EntityName == "LeadActivityMasterProgresses")
                            && p.LeadMasterId == leadid && p.IsActive == true && p.IsDeleted == false
                            select new KycHistroyResponse
                            {
                                Id = a.Id,
                                UserId = a.UserId,
                                EntityId = a.EntityId,
                                DatabaseName = string.IsNullOrEmpty(p.SubActivityMasterName) ? p.ActivityMasterName : p.ActivityMasterName + " " + p.SubActivityMasterName,  // a.DatabaseName,
                                EntityName = a.EntityName,
                                Action = a.Action,
                                Timestamp = a.Timestamp,
                                Changes = a.Changes
                            };

                kycHistroyResponse = query.OrderByDescending(x => x.Id).FirstOrDefault();
            }
            else if (doctype == docType_AddLeadConsentLog_GetLeadForMobile || doctype == docType_AddLeadConsentLog_PostLeadPAN
                || doctype == docType_AddLeadConsentLog_PostLeadAadharVerifyOTP || doctype == docType_AddLeadConsentLog_CheckeSignDocumentStatus)
            {
                var query = from a in _context.AuditLogs
                            join p in _context.LeadConsentLogs on a.EntityId equals p.Id
                            where (a.EntityName == "LeadConsentLog")
                            && p.LeadId == leadid && p.IsActive == true && p.IsDeleted == false
                            select new KycHistroyResponse
                            {
                                Id = a.Id,
                                UserId = a.UserId,
                                EntityId = a.EntityId,
                                DatabaseName = p.Type,  // a.DatabaseName,
                                EntityName = a.EntityName,
                                Action = a.Action,
                                Timestamp = a.Timestamp,
                                Changes = a.Changes
                            };

                kycHistroyResponse = query.OrderByDescending(x => x.Id).FirstOrDefault();
            }
            else if (doctype == docType_Lead_LGLC)
            {
                var query = from a in _context.AuditLogs
                            where (a.EntityName == "Leads")
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

                kycHistroyResponse = query.OrderByDescending(x => x.Id).FirstOrDefault();
            }
            else if (doctype == docType_SendToLos)// docType_SendToLosInprogress || doctype == docType_SendToLosComplete)
            {
                var query = from a in _context.AuditLogs
                            where (a.EntityName == "LeadOffers")
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

                kycHistroyResponse = query.OrderByDescending(x => x.Id).FirstOrDefault();
            }
            KYCHistory histroy = new KYCHistory();

            if (kycHistroyResponse != null)
            {
                histroy.Action = kycHistroyResponse.Action;
                histroy.UserId = kycHistroyResponse.UserId;
                histroy.EntityIDofKYCMaster = kycHistroyResponse.EntityId;
                histroy.CreatedTimeStamp = kycHistroyResponse.Timestamp;

                histroy.MasterFieldChanges = new List<LeadAuditContent>();
                LeadHistoryHelper helper = new LeadHistoryHelper();

                if (doctype == "SaveLeadBankDetail" || doctype == docType_OfferAcceptRejectByBackendTeam || doctype == docType_OfferAcceptRejectByClient
                    || doctype == docType_AgrementUpdateStatus || doctype == docType_AgrementESignStatus
                    || doctype == docType_AgrementAutoESignStatusArthmate || doctype == docType_AgrementUpdateStatusArthmate
                    || doctype == docType_LeadReject
                    //|| doctype == docType_SendToLosInprogress || doctype == docType_SendToLosComplete
                    )
                {
                    List<leadInfoDC> LeadInfolist = new List<leadInfoDC>();
                    LeadInfolist = await GeLeadDetailInfoByTimeStamp(kycHistroyResponse.Timestamp, leadid, doctype);

                    foreach (var item in LeadInfolist)
                    {
                        if (!string.IsNullOrEmpty(item.Changes))
                        {
                            string aContent = item.Changes;//  @"Id: 29\nAPIConfigId: 1\nCompanyId: 0\nCreated: 03/20/2024 14:10:45\nCreatedBy: \nDeleted: \nDeletedBy: \nIsActive: True\nIsDeleted: False\nIsError: False\nLastModified: \nLastModifiedBy: \nProcessedResponse: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Response_5e5192d6-9f2b-4c03-8db1-3e4a2ebd1bbe.json\nRequest: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Request_6ee7e9e8-0f9f-4822-af5a-674b06dee8f1.json\nResponse: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Response_5e5192d6-9f2b-4c03-8db1-3e4a2ebd1bbe.json\nUserId: 79b978fd-8b78-48eb-8472-63608166c46c\n";
                            string action = item.Action; // "Added";

                            List<LeadAuditContent> list = new List<LeadAuditContent>();
                            list = helper.GetHistroy(aContent, action);
                            histroy.MasterFieldChanges.AddRange(list);
                        }
                    }

                    //List<string> LeadBankDetailUsedKeys = new List<string>() { "AccountHolderName","AccountNumber","AccountType","BankName","IFSCCode","PdfPassword","SurrogateType","Type"
                    //                                                            ,"DocumentName", "DocumentNumber",  "FileUrl", "DocumentType" };

                    Dictionary<int, string> myDict = new Dictionary<int, string>();
                    myDict = helper.GetLeadBankDetailUsedKeys();
                    List<string> LeadBankDetailUsedKeys = new List<string>(myDict.Values);

                    if (doctype == "SaveLeadBankDetail")
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => LeadBankDetailUsedKeys.Contains(x.FieldName)).ToList();
                    if (doctype == docType_AgrementESignStatus)
                    {
                        List<string> AgrementESignStatusRemoveKeys = new List<string>() { "LastModified", "Id", "Created", "CreatedBy", "Deleted", "DeletedBy", "IsActive", "IsDeleted", "IsSuccess"
                        ,"LastModifiedBy", "LeadId", "LeadNBFCApiId", ""};
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => !AgrementESignStatusRemoveKeys.Contains(x.FieldName)).ToList();
                    }
                    if (doctype == docType_LeadReject)
                    {
                        List<string> LeadRejectRequiredKeys = new List<string>() { "Status", "RejectMessage" };
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => LeadRejectRequiredKeys.Contains(x.FieldName)).ToList();
                    }
                }
                else if (doctype == docType_KYCAcceptRejectByBackendTeam
                    || doctype == docType_AddLeadConsentLog_GetLeadForMobile || doctype == docType_AddLeadConsentLog_PostLeadPAN
                    || doctype == docType_AddLeadConsentLog_PostLeadAadharVerifyOTP || doctype == docType_AddLeadConsentLog_CheckeSignDocumentStatus
                    || doctype == docType_Lead_LGLC
                    || doctype == docType_SendToLos)
                {
                    string aContent = (doctype == docType_Lead_LGLC ? "" : kycHistroyResponse.DatabaseName) + " " + kycHistroyResponse.Changes;//  @"Id: 29\nAPIConfigId: 1\nCompanyId: 0\nCreated: 03/20/2024 14:10:45\nCreatedBy: \nDeleted: \nDeletedBy: \nIsActive: True\nIsDeleted: False\nIsError: False\nLastModified: \nLastModifiedBy: \nProcessedResponse: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Response_5e5192d6-9f2b-4c03-8db1-3e4a2ebd1bbe.json\nRequest: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Request_6ee7e9e8-0f9f-4822-af5a-674b06dee8f1.json\nResponse: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Response_5e5192d6-9f2b-4c03-8db1-3e4a2ebd1bbe.json\nUserId: 79b978fd-8b78-48eb-8472-63608166c46c\n";
                    string action = kycHistroyResponse.Action; // "Added";

                    histroy.MasterFieldChanges = helper.GetHistroy(aContent, action);

                    if (doctype == docType_AddLeadConsentLog_GetLeadForMobile || doctype == docType_AddLeadConsentLog_PostLeadPAN
                    || doctype == docType_AddLeadConsentLog_PostLeadAadharVerifyOTP || doctype == docType_AddLeadConsentLog_CheckeSignDocumentStatus)
                    {
                        List<string> LeadConsentLogUsedKeys = new List<string>() { "Created", "CreatedBy" };
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => LeadConsentLogUsedKeys.Contains(x.FieldName)).ToList();
                    }
                    if (doctype == docType_Lead_LGLC)
                    {
                        List<string> Lead_LglcUsedKeys = new List<string>() { "LeadConverter", "LeadGenerator" };
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => Lead_LglcUsedKeys.Contains(x.FieldName)).ToList();
                    }
                    //if (doctype == docType_SendToLosInprogress || doctype == docType_SendToLosComplete)
                    if (doctype == docType_SendToLos)
                    {
                        //List<string> SendToLosRequiredKeys = new List<string>() { "Status" };
                        List<string> SendToLosRequiredKeys = new List<string>() { "Comment", "CompanyIdentificationCode", "Status" };
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => SendToLosRequiredKeys.Contains(x.FieldName)).ToList();
                    }
                }

                histroy.Narretion = helper.GetNarretion(kycHistroyResponse.Action, histroy);
                histroy.NarretionHTML = helper.GetNarretionHtml(kycHistroyResponse.Action, histroy);


                //    //foreach (var item in kycHistroyResponse)
                //    {
                //        string aContent = kycHistroyResponse.Changes;//  @"Id: 29\nAPIConfigId: 1\nCompanyId: 0\nCreated: 03/20/2024 14:10:45\nCreatedBy: \nDeleted: \nDeletedBy: \nIsActive: True\nIsDeleted: False\nIsError: False\nLastModified: \nLastModifiedBy: \nProcessedResponse: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Response_5e5192d6-9f2b-4c03-8db1-3e4a2ebd1bbe.json\nRequest: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Request_6ee7e9e8-0f9f-4822-af5a-674b06dee8f1.json\nResponse: https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/Response_5e5192d6-9f2b-4c03-8db1-3e4a2ebd1bbe.json\nUserId: 79b978fd-8b78-48eb-8472-63608166c46c\n";
                //        string action = kycHistroyResponse.Action; // "Added";

                //        LeadHistoryHelper helper = new LeadHistoryHelper();
                //        histroy.MasterFieldChanges = helper.GetHistroy(aContent, action);

                //        List<KYCDetailInfoDC> KYCDetailInfolist = new List<KYCDetailInfoDC>();
                //        KYCDetailInfolist = await GeLeadDetailInfoByTimeStamp(kycHistroyResponse.Timestamp, entityID);
                //        if (KYCDetailInfolist != null && KYCDetailInfolist.Any())
                //        {
                //            histroy.DetailFieldChanges = new List<LeadAuditContent>();
                //            foreach (var itemVal in KYCDetailInfolist)
                //            {
                //                var detailHistory = helper.GetHistroy(itemVal.Changes, itemVal.Action);
                //                if (detailHistory != null && detailHistory.Any(x => x.FieldName == "FieldValue"))
                //                {
                //                    histroy.DetailFieldChanges.Add(new LeadAuditContent
                //                    {
                //                        FieldName = itemVal.KYCDetails_Field,
                //                        FieldOldValue = detailHistory.First(x => x.FieldName == "FieldValue").FieldOldValue,
                //                        FieldNewValue = detailHistory.First(x => x.FieldName == "FieldValue").FieldNewValue,
                //                        FieldInfoType = itemVal.FieldInfoType,
                //                    });
                //                }
                //            }

                //            //DateTime myDate = DateTime.ParseExact(kycHistroyResponse.Timestamp, "yyyy-MM-dd HH:mm:ss,fff",
                //            //               System.Globalization.CultureInfo.InvariantCulture);

                //            histroy.Action = kycHistroyResponse.Action;
                //            histroy.UserId = kycHistroyResponse.UserId;
                //            histroy.EntityIDofKYCMaster = kycHistroyResponse.EntityId;
                //            histroy.CreatedTimeStamp = kycHistroyResponse.Timestamp;
                //        }


                //        List<string> PANAAdharUsedKeys = new List<string>() { "UniqueId", "UserId" };

                //        List<string> OtherDocUsedKeys = new List<string>() { "UserId" };


                //        if (histroy.MasterFieldChanges != null && (doctype == Global.Infrastructure.Constants.KYCMasterConstants.PAN || doctype == Global.Infrastructure.Constants.KYCMasterConstants.Aadhar))
                //            histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => PANAAdharUsedKeys.Contains(x.FieldName)).ToList();
                //        else if (histroy.MasterFieldChanges != null)
                //            histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => OtherDocUsedKeys.Contains(x.FieldName)).ToList();

                //        if (histroy.DetailFieldChanges != null)
                //            histroy.DetailFieldChanges = RemoveDetailFields(histroy.DetailFieldChanges, doctype);

                //        histroy.Narretion = helper.GetNarretion(kycHistroyResponse.Action, histroy);
                //        histroy.NarretionHTML = helper.GetNarretionHtml(kycHistroyResponse.Action, histroy);
                //    }
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
                    //case Global.Infrastructure.Constants.KYCMasterConstants.PAN:
                    //    newList = detailList.Where(x => panFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.Aadhar:
                    //    newList = detailList.Where(x => aadharFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.GST:
                    //    newList = detailList.Where(x => gstFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.PersonalDetail:
                    //    newList = detailList.Where(x => personalDetailFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.BankStatement:
                    //    newList = detailList.Where(x => bankStatementFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.BuisnessDetail:
                    //    newList = detailList.Where(x => buisnessDetailFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.MSME:
                    //    newList = detailList.Where(x => msmeFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.Selfie:
                    //    newList = detailList.Where(x => selfieFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.BankStatementCreditLending:
                    //    newList = detailList.Where(x => bankStatementCreditLendingFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.DSAProfileType:
                    //    newList = detailList.Where(x => dsaProfileTypeFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.DSAPersonalDetail:
                    //    newList = detailList.Where(x => dsaPersonalDetailFields.Contains(x.FieldName)).ToList();
                    //    break;
                    //case KYCMasterConstants.ConnectorPersonalDetail:
                    //    newList = detailList.Where(x => connectorPersonalDetailFields.Contains(x.FieldName)).ToList();
                    //    break;

                    default:
                        break;
                }
            }
            return newList;

        }


        public async Task<List<leadInfoDC>> GeLeadDetailInfoByTimeStamp(DateTime timeStamp, long leadID, string doctype)
        {
            //CommonResponse result = new CommonResponse();

            List<leadInfoDC> list = new List<leadInfoDC>();
            var timeStampValue = new SqlParameter("Timestamp", timeStamp);
            if (doctype == "SaveLeadBankDetail")
            {
                var leadMasterid = new SqlParameter("leadId", leadID);
                list = _context.Database.SqlQueryRaw<leadInfoDC>("exec GetLeadDetailInfoForBank @Timestamp, @leadId", timeStampValue, leadMasterid).AsEnumerable().ToList();
            }
            else if (doctype == docType_LeadReject)
            {
                var leadMasterid = new SqlParameter("leadId", leadID);
                list = _context.Database.SqlQueryRaw<leadInfoDC>("exec GetLeadDetailInfoForLeadReject @Timestamp, @leadId", timeStampValue, leadMasterid).AsEnumerable().ToList();
            }
            //else if (doctype == docType_SendToLosInprogress || doctype == docType_SendToLosComplete)
            //{
            //    var leadMasterid = new SqlParameter("leadId", leadID);
            //    list = _context.Database.SqlQueryRaw<leadInfoDC>("exec GetLeadDetailInfoForSendToLos @Timestamp, @leadId", timeStampValue, leadMasterid).AsEnumerable().ToList();
            //}
            else
            {
                list = _context.Database.SqlQueryRaw<leadInfoDC>("exec GetLeadDetailInfo @Timestamp", timeStampValue).AsEnumerable().ToList();
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



        public class leadInfoDC
        {
            public long EntityId { get; set; }

            public string EntityName { get; set; }

            public string Changes { get; set; }

            public string Action { get; set; }
        }

    }
}
