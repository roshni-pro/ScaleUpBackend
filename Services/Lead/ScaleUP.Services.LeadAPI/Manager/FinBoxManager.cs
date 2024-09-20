using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LeadAPI.Constants;
using ScaleUP.Services.LeadAPI.Helper;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.FinBox;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class FinBoxManager
    {
        private readonly LeadApplicationDbContext _context;
        private readonly FinBoxHelper _finboxhelper;
        private readonly IHostEnvironment _hostingEnvironment;
        public FinBoxManager(LeadApplicationDbContext context, FinBoxHelper finBoxHelper, IHostEnvironment environment)
        {
            _context = context;
            _finboxhelper = finBoxHelper;
            _hostingEnvironment = environment;
        }

        public async Task<finboxConfig> GetFinBoxApi(string code)
        {
            finboxConfig finboxConfig = new finboxConfig();

            var finboxconfig = await _context.finBoxApiConfigs.FirstOrDefaultAsync(x => x.Code == code && x.IsActive && !x.IsDeleted);
            if (finboxconfig != null)
            {
                finboxConfig = new finboxConfig
                {
                    APIKey = finboxconfig.APIKey,
                    ApiURL = finboxconfig.ApiURL,
                    ServerHash = finboxconfig.ServerHash
                };
            }
            return finboxConfig;
        }
        public async Task<string> CreateSessionAsync(string linkid, long leadid)
        {
            var finConfig = await GetFinBoxApi("CreateSession");
            string sessionid = "";
            if (finConfig != null)
            {
                string basePath = _hostingEnvironment.ContentRootPath;
                var req = new CreateSessionPost { api_key = finConfig.APIKey, link_id = linkid };
                var res = await _finboxhelper.CreateSessionAsync(req, finConfig.ApiURL, leadid, basePath);
                sessionid = res.session_id;
            }
            return sessionid;
        }
        public async Task<string> UploadSessionAsync(string linkid, long leadid)
        {
            string result = "";
            var sessionid = "4d51dd0a-4918-487e-8032-6787a7b44e53"; //await CreateSessionAsync(linkid, leadid);

            var finConfig = await GetFinBoxApi("UploadSession");
            if (finConfig != null)
            {
                //var bankstatement = await _context.LeadDocumentDetails.FirstOrDefaultAsync(x => x.LeadId == leadid && x.DocumentType == "statement" && x.IsActive! && x.IsDeleted);
                // if (bankstatement != null)
                {
                    string fileurl = "https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/166c1728-6f5f-4e93-91f2-24b92a94ed4c.pdf";

                    string basePath = _hostingEnvironment.ContentRootPath;
                    var req = new UploadSessionPost
                    {
                        file = fileurl,
                        bank_name = "HDFC",
                        pdf_password = "",
                        session_id = sessionid,
                        upload_type = "PDF"
                    };
                    var res = await _finboxhelper.UploadSessionAsync(req, finConfig.ApiURL, leadid, finConfig.APIKey, finConfig.ServerHash, basePath);
                    result = res.message;
                }
            }
            return result;
        }
        public async Task<SessionUploadStatusResponse> SessionUploadStatus(string session_id, long LeadId)
        {
            var finConfig = await GetFinBoxApi("SessionUploadStatus");
            SessionUploadStatusResponse res = new SessionUploadStatusResponse();
            if (finConfig != null)
            {
                string basePath = _hostingEnvironment.ContentRootPath;
                var req = new finboxConfig { APIKey = finConfig.APIKey, ServerHash = finConfig.ServerHash, ApiURL = finConfig.ApiURL };
                res = await _finboxhelper.SessionUploadStatus(req, basePath, session_id, LeadId);
            }
            return res;
        }

        public async Task<InitiateProcessingResponse> InitiateProcessing(string session_id, long LeadId)
        {
            InitiateProcessingResponse responseDc = new InitiateProcessingResponse();
            var finConfig = await GetFinBoxApi("InitiateProcessing");
            if (finConfig != null)
            {
                string basePath = _hostingEnvironment.ContentRootPath;
                var req = new finboxConfig { APIKey = finConfig.APIKey, ServerHash = finConfig.ServerHash, ApiURL = finConfig.ApiURL };
                responseDc = await _finboxhelper.InitiateProcessing(req, basePath, session_id, LeadId);
            }
            return responseDc;
        }

        public async Task<ProcessingStatusResponse> ProcessingStatus(string session_id, long LeadId)
        {
            ProcessingStatusResponse responseDc = new ProcessingStatusResponse();
            var finConfig = await GetFinBoxApi("ProcessingStatus");
            //string result = "";
            if (finConfig != null)
            {
                string basePath = _hostingEnvironment.ContentRootPath;
                var req = new finboxConfig { APIKey = finConfig.APIKey, ServerHash = finConfig.ServerHash, ApiURL = finConfig.ApiURL };
                responseDc = await _finboxhelper.ProcessingStatus(req, basePath, session_id, LeadId);
            }
            return responseDc;
        }
        public async Task<SessionStatusResponse> SessionStatus(string session_id, long LeadId)
        {
            SessionStatusResponse responseDc = new SessionStatusResponse();
            var finConfig = await GetFinBoxApi("ProcessingStatus");
            // string result = "";
            if (finConfig != null)
            {
                string basePath = _hostingEnvironment.ContentRootPath;
                var req = new finboxConfig { APIKey = finConfig.APIKey, ServerHash = finConfig.ServerHash, ApiURL = finConfig.ApiURL };
                responseDc = await _finboxhelper.SessionStatus(req, basePath, session_id, LeadId);
            }
            return responseDc;
        }

        public async Task<InsightsResponse> Insights(string session_id, long LeadId)
        {
            InsightsResponse responseDc = new InsightsResponse();
            var finConfig = await GetFinBoxApi("Insights");
            //string result = "";
            if (finConfig != null)
            {
                string basePath = _hostingEnvironment.ContentRootPath;
                var req = new finboxConfig { APIKey = finConfig.APIKey, ServerHash = finConfig.ServerHash, ApiURL = finConfig.ApiURL };
                responseDc = await _finboxhelper.Insights(req, basePath, session_id, LeadId);
            }
            return responseDc;
        }
    }
}
