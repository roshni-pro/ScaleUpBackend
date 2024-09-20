using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Helper.LeadHistoryHelper;
using ScaleUP.Services.LoanAccountAPI.Persistence;

namespace ScaleUP.Services.LoanAccountAPI.Managers
{
    public class LoanAccountHistoryManager
    {
        private readonly LoanAccountApplicationDbContext _context;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public LoanAccountHistoryManager(LoanAccountApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<KYCHistory> GetLeadHistroy(long leadid, string doctype, long loanAccountid)
        {
            string res = "";
            string docType_Payments = "Payments";


            KycHistroyResponse kycHistroyResponse = new KycHistroyResponse();

            if (doctype == docType_Payments)
            {
                var query = from a in _context.AuditLogs
                            join d in _context.AccountTransactionDetails on a.EntityId equals d.Id
                            join t in _context.AccountTransactions on d.AccountTransactionId equals t.Id
                            join i in _context.Invoices on t.InvoiceId equals i.Id
                            where (a.EntityName == "AccountTransactionDetail")
                            && d.IsActive == true && d.IsDeleted == false && t.IsActive == true && t.IsDeleted == false && i.IsActive == true && i.IsDeleted == false
                            && t.LoanAccountId == loanAccountid
                            select new KycHistroyResponse
                            {
                                Id = a.Id,
                                UserId = a.UserId,
                                EntityId = a.EntityId,
                                DatabaseName = i.OrderNo + " " + i.InvoiceNo,  // a.DatabaseName,
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

                if (doctype == docType_Payments)
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


                    if (doctype == docType_Payments)
                    {
                        List<string> LoanAccountPaymentUsedKeys = new List<string>() { "Amount", "PaymentReqNo" , "PaymentDate" };
                        histroy.MasterFieldChanges = histroy.MasterFieldChanges.Where(x => LoanAccountPaymentUsedKeys.Contains(x.FieldName)).ToList();
                    }
                }

                histroy.Narretion = helper.GetNarretion(kycHistroyResponse.Action, histroy);
                histroy.NarretionHTML = helper.GetNarretionHtml(kycHistroyResponse.Action, histroy);

            }

            return histroy;
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
