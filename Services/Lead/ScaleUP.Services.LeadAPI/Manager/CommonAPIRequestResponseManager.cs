using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Common.Models;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadModels.LeadNBFC;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class CommonAPIRequestResponseManager
    {
        private readonly LeadApplicationDbContext _context;

        public CommonAPIRequestResponseManager(LeadApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CommonAPIRequestResponseDTO> GetCommonAPIRequestResponseById(long Id)
        {
            CommonAPIRequestResponseDTO commonAPIRequestResponseDTO = new CommonAPIRequestResponseDTO();
            var GetData = _context.BlackSoilCommonAPIRequestResponses.Where(x=> x.Id == Id).FirstOrDefault();
            if (GetData != null) 
            {
                commonAPIRequestResponseDTO.StatusCode = GetData.StatusCode;
                commonAPIRequestResponseDTO.URL = GetData.URL;
                commonAPIRequestResponseDTO.Response = GetData.Response;
                commonAPIRequestResponseDTO.Request = GetData.Request;
                commonAPIRequestResponseDTO.IsSuccess = GetData.IsSuccess;
                commonAPIRequestResponseDTO.Id = Id;
                commonAPIRequestResponseDTO.LeadId = GetData.LeadId;
                commonAPIRequestResponseDTO.LeadNBFCApiId = GetData.LeadNBFCApiId;

            }
            return commonAPIRequestResponseDTO;
        }

        public async Task<ResultViewModel<bool>> SaveCommonAPIRequestResponse(CommonAPIRequestResponseDTO commonAPIRequestResponseDTO)
        {
            ResultViewModel<bool> res = new ResultViewModel<bool>();
            BlackSoilCommonAPIRequestResponse commonAPIRequestResponse = new BlackSoilCommonAPIRequestResponse
            {
                Request = commonAPIRequestResponseDTO.Request,
                Response = commonAPIRequestResponseDTO.Response,
                URL = commonAPIRequestResponseDTO.URL,
                StatusCode = commonAPIRequestResponseDTO.StatusCode,
                IsSuccess = commonAPIRequestResponseDTO.IsSuccess,
                IsActive = true,
                IsDeleted = false,
                LeadNBFCApiId = commonAPIRequestResponseDTO.LeadNBFCApiId,
                LeadId = commonAPIRequestResponseDTO.LeadId
            };
            _context.BlackSoilCommonAPIRequestResponses.Add(commonAPIRequestResponse);
            _context.SaveChanges();
            res = new ResultViewModel<bool>
            {
                Result = true,
                IsSuccess = true,
                Message = "SuccesFully!"
            };
            return res;
        }
    }
}
