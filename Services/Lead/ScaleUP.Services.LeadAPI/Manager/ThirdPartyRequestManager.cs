using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.ThirdApiConfig;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.FinBox;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class ThirdPartyRequestManager
    {
        private readonly LeadApplicationDbContext _context;
        public ThirdPartyRequestManager(LeadApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<bool> SaveThirdPartyRequest(ThirdPartyRequestDTO thirdPartyRequestDTO)
        {
            bool res = false;
            ThirdPartyRequest thirdPartyRequest = new ThirdPartyRequest
            {
                ActivityId = thirdPartyRequestDTO.ActivityId,
                Code = thirdPartyRequestDTO.Code,
                CompanyId = thirdPartyRequestDTO.CompanyId,
                LeadId = thirdPartyRequestDTO.LeadId,
                ProcessedResponse = thirdPartyRequestDTO.ProcessedResponse,
                Request = thirdPartyRequestDTO.Request,
                Response = thirdPartyRequestDTO.Response,
                SubActivityId = thirdPartyRequestDTO.SubActivityId,
                IsActive = true,
                IsDeleted = false,
                IsError = thirdPartyRequestDTO.IsError,
                ThirdPartyApiConfigId = thirdPartyRequestDTO.ThirdPartyApiConfigId,
            };
            _context.ThirdPartyRequests.Add(thirdPartyRequest);
            if (_context.SaveChanges() > 0)
            {
                res = true;
                return res;
            }
            return res;
        }

        public async Task<bool> SaveFinBoxRequestResponse(FinBoxRequestResponseDTO finboxRequestDTO)
        {
            bool res = false;
            FinBoxCommonAPIRequestResponse thirdPartyRequest = new FinBoxCommonAPIRequestResponse
            {
                LeadId = finboxRequestDTO.LeadId,
                Request = finboxRequestDTO.Request ?? "",
                Response = finboxRequestDTO.Response ?? "",
                StatusCode = finboxRequestDTO.StatusCode,
                URL = finboxRequestDTO.URL,
                IsActive = true,
                IsDeleted = false,
            };
            await _context.FinBoxCommonAPIRequestResponse.AddAsync(thirdPartyRequest);
            if (_context.SaveChanges() > 0)
            {
                res = true;
                return res;
            }
            return res;
        }
    }
}
