using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadModels;

namespace ScaleUP.Services.LeadAPI.Manager
{
    public class ExperianManager
    {
        private readonly LeadApplicationDbContext _context;
        public ExperianManager(LeadApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<bool> SaveExperianState(BuildingBlocks.GRPC.Contracts.Lead.DataContracts.ExperianStateRequest experianStateRequest)
        {
            bool res = false;
            var ExperianStateData = _context.ExperianStates.Where(x => x.LocationStateId == experianStateRequest.LocationStateId).FirstOrDefault();
            if (ExperianStateData == null)
            {
                ExperianState experianState = new ExperianState
                {
                    LocationStateId = experianStateRequest.LocationStateId,
                    ExperianStateId = experianStateRequest.ExperianStateId,
                    IsActive = true,
                    IsDeleted = false
                };
                _context.ExperianStates.Add(experianState);
            }
            else
            {
                ExperianStateData.ExperianStateId = experianStateRequest.ExperianStateId;
                _context.ExperianStates.Update(ExperianStateData);
            }
            if (_context.SaveChanges() > 0)
            {
                res = true;
                return res;
            }
            return res;
        }

        public async Task<GRPCReply<ExperianStateReply>> GetExperianStateId(long LocationStateId)
        {
            GRPCReply<ExperianStateReply> experianStateRequest = new GRPCReply<ExperianStateReply>();
            var stateId = _context.ExperianStates.Where(x => x.LocationStateId == LocationStateId).FirstOrDefault();
            if (stateId != null) 
            {
                experianStateRequest.Response = new ExperianStateReply();
                experianStateRequest.Response.ExperianStateId = stateId.ExperianStateId;
                experianStateRequest.Response.LocationStateId = stateId.LocationStateId;
                experianStateRequest.Message = "Success";
                experianStateRequest.Status = true;
            }
            else 
            {
                experianStateRequest.Message = "Not found";
                experianStateRequest.Status = false;
            }
            return experianStateRequest;
        }
    }
}
