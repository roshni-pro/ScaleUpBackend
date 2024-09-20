using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadModels.LeadNBFC;
using static MassTransit.ValidationResultExtensions;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class LeadCommonRequestResponseManager
    {
        private readonly LeadApplicationDbContext _context;

        public LeadCommonRequestResponseManager(LeadApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultViewModel<long>> SaveLeadCommonRequestResponse(LeadCommonRequestInput leadCommonRequestInput)
        {
             ResultViewModel<long> res = new ResultViewModel<long>();
            LeadCommonRequestResponse leadCommonRequestResponse = new LeadCommonRequestResponse 
            {
                CommonRequestResponseId = leadCommonRequestInput.CommonRequestResponseId,
                LeadId = leadCommonRequestInput.LeadId,
                IsActive = true,
                IsDeleted = false
            };
            _context.LeadCommonRequestResponses.Add(leadCommonRequestResponse);
            _context.SaveChanges();
            res = new ResultViewModel<long>
            {
                IsSuccess = true,
                Message = "SuccessFully!",
                Result = leadCommonRequestResponse.Id
            };
            return res;
        }

        public async Task<LeadCommonRequestResponseDTO> GetLeadCommonRequestResponseById(long Id)
        {
            LeadCommonRequestResponseDTO leadCommonRequestResponseDTO = new LeadCommonRequestResponseDTO();
            var GetData = _context.LeadCommonRequestResponses.Where(x => x.Id == Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (GetData != null)
            {
                leadCommonRequestResponseDTO.LeadId = GetData.LeadId;
                leadCommonRequestResponseDTO.CommonRequestResponseId = GetData.CommonRequestResponseId;  
            }
            return leadCommonRequestResponseDTO;
        }

        public  List<LeadCommonRequestResponse> GetLeadCommonRequestResponseList(long LeadId)
        {
           var query = from x in _context.LeadCommonRequestResponses
                       where x.LeadId == LeadId && x.IsActive == true && x.IsDeleted == true
                       select x;

            var result = query.ToList();
            return result;
        }
    }
}
