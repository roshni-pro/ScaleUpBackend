using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.Services.ProductAPI.Persistence;
using ScaleUP.Services.ProductDTO.Master;
using ScaleUP.Services.ProductModels.Master;

namespace ScaleUP.Services.ProductAPI.Manager
{
    public class NBFCCompanyApiManager
    {
        private readonly ProductApplicationDbContext _context;

        public NBFCCompanyApiManager(ProductApplicationDbContext context)
        {
            _context = context;
        }

        public long SaveNBFCCompanyApiData(NBFCCompanyApiRequest nBFCCompanyApiRequest)
        {
            CompanyApi saveNBFCCompanyApi = _context.CompanyApis.Where(x => x.Code == nBFCCompanyApiRequest.Code && x.CompanyId == nBFCCompanyApiRequest.NBFCCompanyId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (saveNBFCCompanyApi == null)
            {
                saveNBFCCompanyApi = new CompanyApi
                {
                    Code = nBFCCompanyApiRequest.Code,
                    CompanyId = nBFCCompanyApiRequest.NBFCCompanyId,
                    APIUrl = nBFCCompanyApiRequest.APIUrl,
                    IsActive = true,
                    IsDeleted = false
                };
                _context.CompanyApis.Add(saveNBFCCompanyApi);
                _context.SaveChanges();
            }
            else
            {
                saveNBFCCompanyApi.CompanyId = nBFCCompanyApiRequest.NBFCCompanyId;
                saveNBFCCompanyApi.APIUrl = nBFCCompanyApiRequest.APIUrl;
            }
            _context.SaveChanges();
            return saveNBFCCompanyApi.Id;
        }

        public async Task<List<NBFCCompanyGetData>> GetNBFCComapanyApiData(long NBFCComapanyId)
        {
            List<NBFCCompanyGetData> nBFCCompanyGetData = new List<NBFCCompanyGetData>();
            var ReturnnBFCCompanyGetData = _context.CompanyApis.Where(x => x.CompanyId == NBFCComapanyId && x.IsActive && !x.IsDeleted).ToList();
            if (ReturnnBFCCompanyGetData != null)
            {
                nBFCCompanyGetData = ReturnnBFCCompanyGetData.Select(x => new NBFCCompanyGetData
                {
                    APIUrl = x.APIUrl,
                    Code = x.Code,
                    NBFCCompanyApiId = x.Id,
                    NBFCCompanyId = x.CompanyId
                }).ToList();
            }
            return nBFCCompanyGetData;
        }

        public async Task<NBFCCompanyResponseMsg> UpdateNBFCCompanyApi(NBFCCompanyGetData nBFCCompanyGetData)
        {
            NBFCCompanyResponseMsg nBFCCompanyResponseMsg = new NBFCCompanyResponseMsg();

            var UpdateData = _context.CompanyApis.Where(x => x.Id == nBFCCompanyGetData.NBFCCompanyApiId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (UpdateData != null)
            {
                UpdateData.APIUrl = nBFCCompanyGetData.APIUrl;
                UpdateData.Code = nBFCCompanyGetData.Code;
                UpdateData.CompanyId = nBFCCompanyGetData.NBFCCompanyId;
                UpdateData.Id = nBFCCompanyGetData.NBFCCompanyApiId;

                nBFCCompanyResponseMsg = new NBFCCompanyResponseMsg
                {
                    Msg = "Updated SuccesfUlly!",
                    Status = true
                };
                _context.SaveChanges();
            }
            else
            {
                nBFCCompanyResponseMsg = new NBFCCompanyResponseMsg
                {
                    Msg = "Failed!",
                    Status = false
                };
            }
            return nBFCCompanyResponseMsg;
        }


        public bool SaveBlackSoilCompanyApiData(long CompanyId)
        {
            CompanyApi? IsCompanyApiExist = _context.CompanyApis.Where(x => x.CompanyId == CompanyId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (IsCompanyApiExist == null)
            {
                List<CompanyApi> addCompanAPI = new List<CompanyApi>();
                _context.CompanyApis.AddRange(
                new CompanyApi { Code = "BlackSoilCreateLead", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilSendToLos", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/send_to_los/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilGenerateDocs", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/los/applications/{{APPLICATION_ID}}/documents/generate_in_bulk/?document_names=sanction_letter,loan_agreement", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilAttachStamps", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/los/applications/{{APPLICATION_ID}}/documents/attach_stamp_paper/?document_ids={{SANCTION_DOC_ID}},{{AGREEMENT_DOC_ID}}", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilEsignStamps", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/service/esigns/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilUpdateDocStatus", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/service/esigns/{{ESIGN _ID}}/?expand=signers&update_status=1", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilCreateBank", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/banks/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilWithdrawalRequest", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/los/applications/{{APPLICATION_ID}}/invoices/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilWithdrawalRequestUpdateDoc", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/los/applications/{{APPLICATION_ID}}/invoices/{{ID}}/files/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilGetCreditLine", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/credit_line/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilGetAvailableCreditLimit", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/available_limit/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilLoanRepayment", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/lms/loan_accounts/{{accountid}}/repayments/{{repaymentid}}/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilLoanAccountDetail", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/lms/loan_accounts/{{accountid}}/?expand=topups.loan_data,business,extra,topups.invoice.files,topups.extra", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilBulkInvoicesApprove", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/los/invoices_bulk/bulk_invoice_files_update/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilRecalculateAccounting", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/lms/loan_accounts/{{loan_account_id}}/recalculate_accounting/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "CreateBusinessPurchaseInvoice", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/invoices/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilListRepayments", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/lms/loan_accounts/{{LOAN_ACCOUNT_ID}}/repayments/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "CreateBusinessDocument", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/{{BUSINESS_ID}}/documents/", IsActive = true, IsDeleted = false },
                
                new CompanyApi { Code = "BlackSoilEmailPost", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/persons/{{PERSON_ID}}/contact_details/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilSelfiePost", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/persons/{{PERSON_ID}}/photos/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilGetEmbedSession", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.saraloan.in/api/v1/core/businesses/get_embed_session/", IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "BlackSoilPFCollection", CompanyId = CompanyId, IsWebhook = false, APIUrl = "https://stag.app.saraloan.in/retailer/login?session_id={SESSION_ID}&business_id={BUSINESS_ID}&person_id={PERSON_ID}", IsActive = true, IsDeleted = false }


                );

                return _context.SaveChanges() > 0;

            }
            else
            {
                return false;
            }
        }

        public bool SaveArthmateCompanyApiData(long CompanyId)
        {
            CompanyApi? IsCompanyApiExist = _context.CompanyApis.Where(x => x.CompanyId == CompanyId && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (IsCompanyApiExist == null)
            {
                List<CompanyApi> addCompanAPI = new List<CompanyApi>();
                _context.CompanyApis.AddRange(
                new CompanyApi { Code = "ArthmateCreateLead", APIUrl = "https://uat-apiorigin.arthmate.com/api/lead", CompanyId = CompanyId, IsWebhook = false, IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "ArthmateLoanDocument", APIUrl = "https://uat-apiorigin.arthmate.com/api/loandocument", CompanyId = CompanyId, IsWebhook = false, IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "ArthmateRequestAScore", APIUrl = "https://uat-apiorigin.arthmate.com/api/bureau-scorecard-v2", CompanyId = CompanyId, IsWebhook = false, IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "ArthmateCeplr", APIUrl = "https://api-demo.ceplr.com/fiu/pdf", CompanyId = CompanyId, IsWebhook = false, IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "ArthmatePanValidate", APIUrl = "https://uat-apiorigin.arthmate.com/api/pan_kyc_v2", CompanyId = CompanyId, IsWebhook = false, IsActive = true, IsDeleted = false },
                new CompanyApi { Code = "ArthmateGenerateOffer", APIUrl = "https://uat-apiorigin.arthmate.com/api/co-lender-selector", CompanyId = CompanyId, IsWebhook = false, IsActive = true, IsDeleted = false }

                );

                return _context.SaveChanges() > 0;

            }
            else
            {
                return false;
            }
        }
    }
}
